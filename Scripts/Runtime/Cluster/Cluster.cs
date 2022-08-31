using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;

using HEVS.Collections;

using System.Threading;
using NetMQ.Monitoring;
using System.Threading.Tasks;

namespace HEVS
{
    /// <summary>
    /// The Cluster controller. This is a static class that controls the sockets and the broadcasting of cluster data. It also controls frame synchronisation.
    /// </summary>
    public class Cluster
    {
        static Config.Cluster config;

        /// <summary>
        /// Are the cluster sockets active.
        /// </summary>
        public static bool active { get; private set; }
        /// <summary>
        /// Is the current platform running as a cluster. Will always report false when running within the Unity editor.
        /// </summary>
        public static bool isCluster { get { return Application.isEditor ? clusterInEditorAllowed && config != null : config != null; } }
        /// <summary>
        /// The IP address of the cluster master node.
        /// </summary>
        public static string masterAddress { get { return config.master.address; } }
        /// <summary>
        /// Is this node the cluster's master node.
        /// </summary>
        public static bool isMaster { get { return !isCluster || config.platform.masterNode == Core.activeNode; } }
        /// <summary>
        /// Is this instance running as a client.
        /// </summary>
        public static bool isClient { get { return !isMaster; } }
        /// <summary>
        /// The number of client nodes within the current platform. This number is equivalent to the total number of nodes, minus 1 for the master.
        /// </summary>
        public static int clientCount { get { return isCluster && Core.activePlatform != null? Core.activePlatform.nodes.Count - 1 : 0; } }
        /// <summary>
        /// The current Unity time of the cluster's master.
        /// </summary>
        public static float time { get; private set; }
        /// <summary>
        /// The current Unity delta-time of the cluster's master.
        /// </summary>
        public static float deltaTime { get; private set; }
        /// <summary>
        /// The current Unity frame count of the cluster's master.
        /// </summary>
        public static int frameCount { get; private set; }

        /// <summary>
        /// The number of clients which have dropped out of the cluster since startup. 
        /// </summary>
        public static int clientDropoutCount { get; private set; } = 0;

        /// <summary>
        /// The time it takes a master to complete the pre-sync broadcast, or a client to receive the pre-sync broadcast, in milliseconds.
        /// </summary>
        public static double preSyncTimeMS { get; private set; } = 0;

        /// <summary>
        /// The time it takes a master to complete the post-sync broadcast, or a client to receive the post-sync broadcast, in milliseconds.
        /// </summary>
        public static double postSyncTimeMS { get; private set; } = 0;

        /// <summary>
        /// The time it takes a master to sync the cluster frame swap, or a client to receive and complete a frame swap, in milliseconds.
        /// </summary>
        public static double frameSyncTimeMS { get; private set; } = 0;

        /// <summary>
        /// The size of the most recent pre-sync packet (time & input) in bytes.
        /// </summary>
        public static int preSyncPacketSize { get; private set; } = 0;

        /// <summary>
        /// The size of the most recent post-sync packet (transforms, variables, RPC) in bytes.
        /// </summary>
        public static int postSyncPacketSize { get; private set; } = 0;

        /// <summary>
        /// Collection of instantiated ClusterObject's, stored by their cluster ID.
        /// </summary>
        public static Dictionary<int, ClusterObject> clusterObjects = new Dictionary<int, ClusterObject>();

        /// <summary>
        /// The current node's frames-per-second (FPS).
        /// </summary>
        public static double fps { get; private set; } = 0.0;

        /// <summary>
        /// The rate at which the FPS is updated per-second.
        /// </summary>
        public static double fpsUpdateRate = 1.0;

        /// <summary>
        /// A list of all gameobject's spawned last update. 
        /// </summary>
        public static List<GameObject> gameObjectsSpawnedLastUpdateList { get; private set; } = new List<GameObject>();

		private static List<ClusterObject> spawnablePrefabList;

		private static List<int> destroyedClusterObjectsList = new List<int>();

		/// <summary>
		/// List of ClusterObject IDs which have had their state changed on the Master, and what the state change is.
		/// </summary>
		private static List<KeyValuePair<int, int>> clusterObjectsChangedState = new List<KeyValuePair<int, int>>();

        /// <summary>
        /// Current number of active client nodes.
        /// </summary>
		public static int numberOfActiveClients { get { return (!clusterInEditorAllowed && UnityEngine.Application.isEditor) ? 0 : clientCount - clientDropoutCount; } }

        /// <summary>
        /// Delegate definition for serializing pre-update data.
        /// </summary>
        /// <param name="writer">The byte buffer to write into.</param>
        public delegate void PreUpdateSerializeDataDelegate(ByteBufferWriter writer);

        /// <summary>
        /// Delegate definition for deserializing pre-update data.
        /// </summary>
        /// <param name="reader">The byte buffer to read from.</param>
        public delegate void PreUpdateDeserializeDataDelegate(ByteBufferReader reader);

        /// <summary>
        /// Delegate definition for serializing post-update data.
        /// </summary>
        /// <param name="writer">The byte buffer to write into.</param>
        public delegate void PostUpdateSerializeDataDelegate(ByteBufferWriter writer);

        /// <summary>
        /// Delegate definition for deserializing post-update data.
        /// </summary>
        /// <param name="reader">The byte buffer to read from.</param>
        public delegate void PostUpdateDeserializeDataDelegate(ByteBufferReader reader);

        /// <summary>
        /// Delegate for serializing pre-update data.
        /// </summary>
        static public PreUpdateSerializeDataDelegate PreUpdateSerializationDelegate;

        /// <summary>
        /// Delegate for deserializing pre-update data.
        /// </summary>
        static public PreUpdateDeserializeDataDelegate PreUpdateDeserializationDelegate;

        /// <summary>
        /// Delegate for serializing post-update data.
        /// </summary>
        static public PostUpdateSerializeDataDelegate PostUpdateSerializationDelegate;

        /// <summary>
        /// Delegate for deserializing post-update data.
        /// </summary>
        static public PostUpdateDeserializeDataDelegate PostUpdateDeserializationDelegate;

        #region Private Members
        // used to control internal Unity time adjustments for animation
        static float timeAdjustmentTimeout = 0.1f; //<! 1/10 of a frame seems to be good

        // internal socket usage
        static NetMQSocket dataSocket;
        static NetMQSocket syncSocket;
    //    static NetMQMonitor syncSocketMonitor;
    //    static bool networkConnected = false;

        static ByteBufferWriter payloadWriter;
        static ByteBufferReader payloadReader;

        static bool clusterInEditorAllowed = false;

        static long fpsFrameCount = 0;
        static double fpsDeltaTime = 0.0;
        internal static int _nextClusterID = 0;

        internal static int nextClusterID { get { return _nextClusterID++; } }

        static System.Diagnostics.Stopwatch profiler = new System.Diagnostics.Stopwatch();
        #endregion

        #region Initialisation
        /// <summary>
        /// Initialise the cluster.
        /// Called from HEVSApplication Awake().
        /// </summary>
        /// <param name="config">The config options for this cluster.</param>
        /// <param name="configurationSpawnablePrefabList">A list of all registered spawnable prefabs.</param>
        /// <param name="randomSeed">The random seed to broadcast around the cluster (default: 42).</param>
        /// <param name="clusterInEditor">Should run as a cluster in editor (default: false)</param>
        internal static void Initialise(Config.Cluster config, List<ClusterObject> configurationSpawnablePrefabList, int randomSeed = 42, bool clusterInEditor = false)
        {
            if (active)
                return;

            Cluster.config = config;

            clusterInEditorAllowed = clusterInEditor;
            spawnablePrefabList = configurationSpawnablePrefabList;
        
            time = UnityEngine.Time.time;
            deltaTime = UnityEngine.Time.deltaTime;
            frameCount = 0;

            UnityEngine.Random.InitState(randomSeed);

            if (isCluster)
            {
                if (Core.VerboseLogging) Debug.Log("HEVS: Initialising sockets...");

                InitialiseSockets(randomSeed);

                if (Core.VerboseLogging) Debug.Log("HEVS: Enabling data broadcasting if required.");
                if (enableDataBroadcast)
                    InitialiseDataBroadcasting();

                // disable vsync unless requested
                if (Environment.GetCommandLineArgs().Contains("-vsynced"))
                {
                    if (Core.VerboseLogging) Debug.Log("HEVS: Enabling vsync.");
                    QualitySettings.vSyncCount = 1;
                }
                else
                {
                    if (Core.VerboseLogging) Debug.Log("HEVS: Disabling vsync.");
                    QualitySettings.vSyncCount = 0;
                }

                // hide cursor
                if (!isMaster)
                    Cursor.visible = false;
            }

            active = true;
        }

        /// <summary>
        /// Initialises physics for the cluster by potentially disabling RigidBodies on clients,
        /// and adding ClusterObjects so that physics objects sync from the master to clients.
        /// </summary>
        internal static void InitialisePhysics()
        {
            // force all rigid bodies to have ClusterObject unless we're a slave that shouldn't sync rigid bodies
            var rigidBodies = GameObject.FindObjectsOfType<Rigidbody>();
            foreach (Rigidbody rb in rigidBodies)
            {
                if ((!isCluster ||
                    config.autoSyncRigidBodies) &&
                     rb.GetComponent<ClusterObject>() == null)
                    rb.gameObject.AddComponent<ClusterObject>().transformFlags = (int)TransformFlags.All;
            }

            if (isCluster)
            {
                // modify physics
                if (config.autoSyncRigidBodies ||
                    config.disableClientPhysics)
                {
                    foreach (Rigidbody rb in rigidBodies)
                    {
                        if (config.disableClientPhysics &&
                            !Cluster.isMaster)
                            GameObject.Destroy(rb);
                    }
                }
            }
        }
        #endregion

        #region Network Startup
        static void InitialiseSockets(int randomSeed)
        {
            // open sockets
            AsyncIO.ForceDotNet.Force();

            if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::InitialiseSockets(): Creating read/write buffers.");
            payloadWriter = new ByteBufferWriter(config.packetLimit);
            payloadReader = new ByteBufferReader();

            if (isMaster)
            {
                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::InitialiseSockets(): Binding publisher socket tcp://*:{config.dataPort}");
                // create PUB socket
                dataSocket = new PublisherSocket();
                dataSocket.Options.SendBuffer = config.packetLimit;
                dataSocket.Bind("tcp://*:" + config.dataPort);

                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::InitialiseSockets(): Binding response socket tcp://*:{config.syncPort}");
                // create REP socket
                syncSocket = new ResponseSocket();
                syncSocket.Options.SendBuffer = config.packetLimit;
                syncSocket.Bind("tcp://*:" + config.syncPort);
            }
            else
            {
                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::InitialiseSockets(): Subscribing to master tcp://{config.master.address}:{config.dataPort}");
                // create SUB socket
                dataSocket = new SubscriberSocket();
                dataSocket.Options.ReceiveBuffer = config.packetLimit;

                dataSocket.Connect("tcp://" + config.master.address + ":" + config.dataPort);
                (dataSocket as SubscriberSocket).SubscribeToAnyTopic();

                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::InitialiseSockets(): Connecting request socket to master tcp://{config.master.address}:{config.syncPort}");

                // create REQ socket
                syncSocket = new RequestSocket();
                syncSocket.Options.ReceiveBuffer = config.packetLimit;

         /*       syncSocketMonitor = new NetMQMonitor(syncSocket, "inproc://monitor.sync", SocketEvents.Disconnected);
                syncSocketMonitor.Disconnected += OnSocketDisconnected;

                Task.Factory.StartNew(syncSocketMonitor.Start);*/

                syncSocket.Connect("tcp://" + config.master.address + ":" + config.syncPort);
            }

            //    networkConnected = true;
            if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::InitialiseSockets(): Startup barrier...");

            StartupBarrier(randomSeed);

            if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::InitialiseSockets(): Complete.");
        }

        static void StartupBarrier(int randomSeed)
        {
            if (isMaster)
            {
                int remainingConnections = (!clusterInEditorAllowed && UnityEngine.Application.isEditor) ? 0 : clientCount;

                TimeSpan ts = new TimeSpan(0, 0, 0, config.clientTimeoutLimit);

                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::StartupBarrier(): Waiting for {remainingConnections} clients...");

                // wait for all connected
                while ((remainingConnections - clientDropoutCount) > 0)
                {
                    // wait for connection signal
                    if (syncSocket.TrySkipFrame(ts))
                    {
                        if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::StartupBarrier(): Client connected, {remainingConnections} remaining.");

                        // reply with random seed
                        syncSocket.SendFrame(randomSeed.ToString());
                        remainingConnections--;
                    }
                    else
                    {
                        clientDropoutCount++;
                        Debug.LogWarning("HEVS: Client connection timeout - client has been dropped.");
                    }
                }

                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::StartupBarrier(): All connected, sending OK.");

                // if all connected, send ready
                dataSocket.SignalOK();
            }
            else
            {
                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::StartupBarrier(): Signalling ready.");

                // send connection signal
                syncSocket.SignalOK();

                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::StartupBarrier(): Waiting for acknowledgement.");
                // receive random seed
                string randomSeedString = syncSocket.ReceiveFrameString();
                UnityEngine.Random.InitState(Convert.ToInt32(randomSeedString));

                if (Core.VerboseLogging) Debug.Log($"HEVS Cluster::StartupBarrier(): Acknowledgement received, waiting for OK signal that whole cluster is connected.");

                // wait until "server ready"
                dataSocket.SkipFrame();
            }
        }
        #endregion

        #region Shutdown
        /// <summary>
        /// Remove cluster objects, spawned objects, and reset cluster ID.
        /// </summary>
        internal static void ResetState()
        {
            spawnCommands.Clear();
            clusterObjects.Clear();
			RPC.ResetState();
        }
        
        /// <summary>
        /// Close cluster sockets.
        /// </summary>
        internal static void Shutdown()
        {
            ResetState();
            if (isCluster)
            {
                /* if (!isMaster)
                     Task.Factory.StartNew(() => syncSocketMonitor.Dispose()).Wait(1000);*/

                ShutdownDataBroadcasting();

                if (syncSocket != null)
                {
                    syncSocket.Close();
                    syncSocket = null;
                }
                if (dataSocket != null)
                {
                    dataSocket.Close();
                    dataSocket = null;
                }
                NetMQConfig.Cleanup();
            }
        }

     /*   static void OnSocketDisconnected(object sender, NetMQMonitorSocketEventArgs e)
        {
            networkConnected = false;
        }*/
        #endregion

        #region Pre and Post Update
        /// <summary>
        /// Broadcast time and input state for the following frame.
        /// </summary>
        internal static void PreUpdate()
        {
            // always update time info (will be overridden if a client)
            time = UnityEngine.Time.time - (Replay.active == null ? 0 : Replay.active.startTime);
            deltaTime = UnityEngine.Time.deltaTime;
            frameCount++;

            // manual FPS tracking
            fpsFrameCount++;
            fpsDeltaTime += UnityEngine.Time.deltaTime;
            if (fpsDeltaTime > (1.0 / fpsUpdateRate))
            {
                fps = fpsFrameCount / fpsDeltaTime;
                fpsFrameCount = 0;
                fpsDeltaTime -= 1.0 / fpsUpdateRate;
            }

            // should we log or override the frame data?
            //   if (isMaster &&
            //       Replay.active != null)
            //       ReplayPreUpdateStep();

            if (isCluster)// && networkConnected)
                PreUpdateDataSync();
        }

        /// <summary>
        /// Broadcast ClusterTransforms, ClusterVariables and RPC, spawn objects, sync the frame.
        /// </summary>
        internal static void PostUpdate()
        {
            // should we log or override the frame data?
        //    if (isMaster &&
        //        Replay.active != null)
        //        ReplayPostUpdateStep();

            if (isCluster)// && networkConnected)
                PostUpdateDataSync();
            // we may have received spawn commands from the replay system!
            else /*if (Replay.active == null ||
                !Replay.active.isPlaying)*/
                SpawnObjects();

            // clear frame's spawned list
            spawnCommands.Clear();

            ClusterVariable.CleanDirtyVariables();

            RPC.InvokeCallsOnAll();

            // has a client lost connection to the master?
        /*    if (isCluster && 
                !isMaster &&
                !networkConnected)
            {
                // TODO: deal with this somehow!
           //     if (Application.isEditor)
           //         EditorApplication.isPlaying = false;
                UnityEngine.Application.Quit();
            }*/
        }
        #endregion

        #region Per-Frame Sync Points
        static void PreUpdateDataSync()
        {
            if (isMaster)
            {
                // send per-frame data (time, input)
                payloadWriter.Clear();

                // time
                payloadWriter.Write(frameCount);
                payloadWriter.Write(time);
                payloadWriter.Write(deltaTime);

                Input.Serialize(payloadWriter);

                // append app specific payload
                PreUpdateSerializationDelegate?.Invoke(payloadWriter);

                preSyncPacketSize = payloadWriter.Length;

                profiler.Restart();
                dataSocket.SendFrame(payloadWriter.AsArray(), payloadWriter.Length);
                profiler.Stop();

                preSyncTimeMS = profiler.Elapsed.TotalMilliseconds;
            }
            else
            {
                // receive per-frame data (time, input)
                profiler.Restart();

                // is this causing issues?
                //    while (networkConnected && !dataSocket.TryReceiveFrameBytes(out data)) if (!networkConnected) break;

                byte[] data = dataSocket.ReceiveFrameBytes();

                profiler.Stop();

                preSyncTimeMS = profiler.Elapsed.TotalMilliseconds;

            //    if (networkConnected)
                {
                    payloadReader.SetSource(data);

                    preSyncPacketSize = data.Length;

                    frameCount = payloadReader.ReadInt();
                    time = payloadReader.ReadFloat();
                    deltaTime = payloadReader.ReadFloat();

                    // calculate a time scale so that the client adjusts their time to match the master
                    // Note: should never go negative, so limit to >=0
                    float desiredTimeScale = ((time - UnityEngine.Time.time) + timeAdjustmentTimeout) / timeAdjustmentTimeout;
                    UnityEngine.Time.timeScale = Mathf.Max(0, desiredTimeScale);

                    Input.Deserialize(payloadReader);

                    // read app specific payload
                    PreUpdateDeserializationDelegate?.Invoke(payloadReader);
                }

                if (dataBroadcastRunning)
                    HandleReceivedDataBroadcasts();
            }
        }

        static void SendReceivePostUpdatePacket()
        {
            if (isMaster)
            {
                // send post-frame payload transforms / variables / RPC
                payloadWriter.Clear();

                SerializeSpawnedObjects(payloadWriter);

                //    if (Replay.active == null ||
                //        Replay.active.isPlaying == false)
                SpawnObjects();

                SerializeClusterObjectStates(payloadWriter);

                SerializeClusterTransforms(payloadWriter);

                ClusterVariable.SerializeDirtyVariables(payloadWriter);

                RPC.SerializeCallsOnAll(payloadWriter);

                PostUpdateSerializationDelegate?.Invoke(payloadWriter);

                postSyncPacketSize = payloadWriter.Length;

                dataSocket.SendFrame(payloadWriter.AsArray(), payloadWriter.Length);
            }
            else
            {
                byte[] data = dataSocket.ReceiveFrameBytes();

                postSyncPacketSize = data.Length;

                // update transforms / variables / receive RPC
                payloadReader.SetSource(data);

                DeserializeSpawnedObjects(payloadReader);

                SpawnObjects();

                DeserializeClusterObjectStates(payloadReader);

                ChangeClusterObjectStates();

                DeserializeClusterTransforms(payloadReader);

                ClusterVariable.DeserializeDirtyVariables(payloadReader);

                RPC.DeserializeCallsOnAll(payloadReader);

                // read app specific payload
                PostUpdateDeserializationDelegate?.Invoke(payloadReader);
            }
        }

        static void SyncBarrier()
        {
            if (isMaster)
            {
                // wait for all client's ready
                int remainingConnections = (!clusterInEditorAllowed && UnityEngine.Application.isEditor) ? 0 : clientCount;
                while (remainingConnections > 0)
                {
                    // wait for ready signal
                    syncSocket.SkipFrame();
                    // send ack
                    syncSocket.SignalOK();
                    remainingConnections--;
                }

                // tell all clients to go ahead
                dataSocket.SignalOK();
            }
            else
            {
                // tell master I'm ready
                syncSocket.SignalOK();
                // wait for ack
                syncSocket.SkipFrame();
                // wait for all ready
                dataSocket.SkipFrame();
            }
        }

        static void PostUpdateDataSync()
        {
            profiler.Restart();
   
            SendReceivePostUpdatePacket();

            // might not be needed
        //    SyncBarrier();

            profiler.Stop();
            postSyncTimeMS = profiler.Elapsed.TotalMilliseconds;
        }

        internal static void FrameSync()
        {
            if (!isCluster) return;

            profiler.Restart();

            if (isMaster)
            {
                TimeSpan ts = new TimeSpan(0, 0, 0, config.clientTimeoutLimit);

                // wait for all clients ready, and gather any client-to-master RPC they send
                int remainingConnections = (!clusterInEditorAllowed && UnityEngine.Application.isEditor) ? 0 : clientCount;
                while ( (remainingConnections - clientDropoutCount) > 0)
                {
					bool hasRPC = false;
                    // receive sync request, and check if data was attached for RPC
                    // also check timeout to drop a client (this seems really really bad, since we don't know who dropped!)
                    if (syncSocket.TrySkipFrame(ts, out hasRPC))
                    {
						if (hasRPC)
						{
                            // gather the RPC data
							byte[] data = syncSocket.ReceiveFrameBytes();
							payloadReader.SetSource(data);
							if (payloadReader.Capacity > 0)
								RPC.DeserializeCallsOnMaster(payloadReader);
						}

                        // send ack
                        syncSocket.SignalOK();

                        remainingConnections--;
                    }
					else
					{
						Debug.Log("HEVS: Client is unresponsive, dropping from sync.");
						clientDropoutCount++; 
					}
                }

                // invoke received client-to-master RPC
				RPC.InvokeCallsOnMaster();

				// signal the go-ahead
				dataSocket.SignalOK();
            }
            else
            {
                // send any client-to-master RPC, or send empty sync request
                if (RPC.numberOfMasterCalls > 0)
                {
                    payloadWriter.Clear();
                    RPC.SerializeCallsOnMaster(payloadWriter);
                    syncSocket.SendMoreFrameEmpty().SendFrame(payloadWriter.AsArray());
                }
                else
                    syncSocket.SendFrameEmpty();

                // wait for ack from master
                syncSocket.SkipFrame();

                // wait for go-ahead
                dataSocket.SkipFrame();
            }

            profiler.Stop();
            frameSyncTimeMS = profiler.Elapsed.TotalMilliseconds;
        }
        #endregion

        #region Cluster Objects
        internal static void RegisterObject(int clusterID, ClusterObject obj)
        {
            if (clusterObjects.ContainsKey(clusterID))
            {
                Debug.LogError("HEVS: ID " + clusterID + " already registered!");
            }
            else
                clusterObjects.Add(clusterID, obj);
        }

        internal static void DeregisterObject(int clusterID)
        {
            clusterObjects.Remove(clusterID);
        }

        internal static void DeregisterObject(ClusterObject obj)
        {
            clusterObjects.Remove(obj.clusterID);
        }

        static void UpdateObjectID(int oldID, int newID)
        {
            ClusterObject obj = clusterObjects[oldID];
            clusterObjects.Remove(oldID);
            clusterObjects.Add(newID, obj);
        }

        internal static void ChangeClusterObjectState(int clusterID, ClusterObject.State state)
		{
            clusterObjectsChangedState.Add(new KeyValuePair<int, int>(clusterID, (int)state));
        }

        internal static void SerializeClusterObjectStates(ByteBufferWriter writer)
		{
            writer.Write(clusterObjectsChangedState.Count);
            foreach (var stateChange in clusterObjectsChangedState)
            {
                writer.Write(stateChange.Key);
                writer.Write(stateChange.Value);
            }
            clusterObjectsChangedState.Clear();
		}

        internal static void DeserializeClusterObjectStates(ByteBufferReader reader)
		{
			int count = reader.ReadInt();
            clusterObjectsChangedState.Clear();
			for (int index = 0; index < count; index++)
            {
                int clusterID = reader.ReadInt();
                int state = reader.ReadInt();
                clusterObjectsChangedState.Add(new KeyValuePair<int, int>(clusterID, state));
            }
        }

        internal static void ChangeClusterObjectStates()
        {
            foreach (var stateChange in clusterObjectsChangedState)
            {
                if (clusterObjects.ContainsKey(stateChange.Key))
                {
                    ClusterObject co = clusterObjects[stateChange.Key];
                    co.SetClientState(stateChange.Value);
                }
            }
        }
		#endregion

		#region Object Spawning
		class SpawnCommand
        {
            public bool isPrefab;
            public int typeOrIndex;
            public int clusterID;
            public object[] args;
        }

        /// <summary>
        /// Delegate definition for a spawn handler method.
        /// </summary>
        /// <param name="obj">The GameObject that has just been spawned by the master node.</param>
        /// <param name="args">Spawn arguments.</param>
        public delegate void SpawnHandler(GameObject obj, params object[] args);

        static Dictionary<int, SpawnHandler> spawnHandlers = new Dictionary<int, SpawnHandler>();

        static List<SpawnCommand> spawnCommands = new List<SpawnCommand>();

        static List<GameObject> spawnedObjects = new List<GameObject>();

        /// <summary>
        /// Registers are spawn handler with the cluster, for an associated ID.
        /// </summary>
        /// <param name="typeID">The ID to associate the spawn handler with.</param>
        /// <param name="func">The spawn handler delegate.</param>
        public static void RegisterSpawnFactoryMethod(int typeID, SpawnHandler func)
        {
            if (spawnHandlers.ContainsKey(typeID))
                Debug.LogError("HEVS: Spawn handler for type ID [" + typeID + "] already registered! Deregister the old one first if you want to replace it.");
            else
                spawnHandlers.Add(typeID, func);
        }

        /// <summary>
        /// Deregister a registered spawn handler for a specified type ID.
        /// </summary>
        /// <param name="typeID">The type ID to deregister the handler for.</param>
        public static void DeregisterSpawnFactoryMethod(int typeID)
        {
            spawnHandlers.Remove(typeID);
        }

        /// <summary>
        /// Called on the master node only. Creates a GameObject and passes it into a register SpawnHandler factory method.
        /// </summary>
        /// <param name="typeID">The type ID for the matching SpawnHandler.</param>
        /// <param name="arguments">Optional arguments to pass to the spawn handler.</param>
        public static int Spawn(int typeID, params object[] arguments)
        {
            if (!isMaster)
            {
                Debug.LogWarning("HEVS: Calling Cluster.Spawn() on client! No action will be performed.");
                return -1;
            }

            // register spawn call and the args
            int clusterID = nextClusterID;
            spawnCommands.Add(new SpawnCommand() { isPrefab = false, typeOrIndex = typeID, clusterID = clusterID, args = arguments });

            return clusterID;
        }

        /// <summary>
        /// Called on the master node only. Instantiates an instance of a prefab within the cluster.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <returns>Returns the cluster ID of the instantiated GameObject.</returns>
        public static int Spawn(GameObject prefab)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Called on the master node only. Instantiates an instance of a prefab within the cluster.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="position">The position to instantiate the GameObject at.</param>
        /// <returns>Returns the cluster ID of the instantiated GameObject.</returns>
        public static int Spawn(GameObject prefab, Vector3 position)
        {
            return Spawn(prefab, position, Quaternion.identity);
        }

        /// <summary>
        /// Called on the master node only. Instantiates an instance of a prefab within the cluster.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="position">The position to instantiate the GameObject at.</param>
        /// <param name="rotation">The rotation of the instantiated GameObject.</param>
        /// <returns>Returns the cluster ID of the instantiated GameObject.</returns>
        public static int Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!isMaster)
            {
                Debug.LogWarning("HEVS: Calling Cluster.Spawn() on client! No action will be performed.");
                return -1;
            }

            int index = -1;
            for (int count = 0; count < spawnablePrefabList.Count; count++)
            {
                ClusterObject spawnableObject = spawnablePrefabList[count];
                if (spawnableObject.gameObject == prefab)
                    index = count;
            }

            if (index >= 0)
            {
                object[] arguments = { position, rotation };

                // register spawn call and the args
                int clusterID = nextClusterID;
                spawnCommands.Add(new SpawnCommand() { isPrefab = true, typeOrIndex = index, clusterID = clusterID, args = arguments });
                return clusterID; 
            }
            else
            {
                Debug.LogWarning("HEVS: Cluster.Spawn() could not find prefab [" + prefab.name + "] on HEVS Core Component's spawnablePrefabList");
                return -1;
            }            
        }

        static void SpawnObjects()
        {
            gameObjectsSpawnedLastUpdateList = new List<GameObject>();

            foreach (SpawnCommand command in spawnCommands)
            {
                if (command.isPrefab)
                {
                    GameObject go = GameObject.Instantiate(spawnablePrefabList[command.typeOrIndex].gameObject);
                    ClusterObject co = go.GetComponent<ClusterObject>();
                    co.clusterID = command.clusterID;

					// update next cluster ID so that it matches the master. 
					if (_nextClusterID <= command.clusterID)
                        _nextClusterID = command.clusterID + 1; 

                    go.transform.position = (Vector3)command.args[0];
                    go.transform.rotation = (Quaternion)command.args[1];

                    gameObjectsSpawnedLastUpdateList.Add(go);

                    spawnedObjects.Add(go);
                }
                else
                {
                    GameObject go = new GameObject();
                    go.AddComponent<ClusterObject>().clusterID = command.clusterID;

                    if (spawnHandlers.ContainsKey(command.typeOrIndex))
                        spawnHandlers[command.typeOrIndex](go, command.args);
                    else
                        Debug.LogWarning("HEVS: No spawn handler registered for spawned type [" + command.typeOrIndex + "].");

                    gameObjectsSpawnedLastUpdateList.Add(go);

                    spawnedObjects.Add(go);
                }
            }
        }

        internal static void RemoveSpawnedObjects()
        {
            foreach (var go in spawnedObjects)
            {
                if (go != null)
                {
                    DeregisterObject(go.GetComponent<ClusterObject>());
                    GameObject.Destroy(go);
                }
            }
            spawnedObjects.Clear();
        }

        static void SerializeSpawnedObjects(ByteBufferWriter writer)
        {
            // how many objects are we spawning?
            writer.Write(spawnCommands.Count);

            // spawn objects
            foreach (SpawnCommand command in spawnCommands)
            {
                // write type of command (prefab or not)
                writer.Write(command.isPrefab);
                // object type ID for callback
                writer.Write(command.typeOrIndex);
                // cluster ID
                writer.Write(command.clusterID);

                // write arguments
                writer.Write(command.args != null ? command.args.Length : 0);
                foreach (object arg in command.args)
                {
                    // what is the type of each argument?
                    Type argType = arg.GetType();

                    // write the data for the single element
                    switch (Type.GetTypeCode(argType))
                    {
                        case TypeCode.Empty: writer.Write((byte)0); break; // null
                        case TypeCode.Boolean: writer.Write((byte)1); writer.Write((bool)arg); break;
                        case TypeCode.Char: writer.Write((byte)2); writer.Write((char)arg); break;
                        case TypeCode.SByte: writer.Write((byte)3); writer.Write((sbyte)arg); break;
                        case TypeCode.Byte: writer.Write((byte)4); writer.Write((byte)arg); break;
                        case TypeCode.Int16: writer.Write((byte)5); writer.Write((short)arg); break;
                        case TypeCode.UInt16: writer.Write((byte)6); writer.Write((ushort)arg); break;
                        case TypeCode.Int32: writer.Write((byte)7); writer.Write((int)arg); break;
                        case TypeCode.UInt32: writer.Write((byte)8); writer.Write((uint)arg); break;
                        case TypeCode.Int64: writer.Write((byte)9); writer.Write((long)arg); break;
                        case TypeCode.UInt64: writer.Write((byte)10); writer.Write((ulong)arg); break;
                        case TypeCode.Single: writer.Write((byte)11); writer.Write((float)arg); break;
                        case TypeCode.Double: writer.Write((byte)12); writer.Write((double)arg); break;
                        case TypeCode.String: writer.Write((byte)14); writer.Write((string)arg); break;
                        default:
                            {
                                // vector2
                                if (argType == typeof(Vector2))
                                {
                                    writer.Write((byte)15);
                                    writer.Write((Vector2)arg);
                                }
                                // vector3
                                else if (argType == typeof(Vector3))
                                {
                                    writer.Write((byte)16);
                                    writer.Write((Vector3)arg);
                                }
                                // vector4
                                else if (argType == typeof(Vector4))
                                {
                                    writer.Write((byte)17);
                                    writer.Write((Vector4)arg);
                                }
                                // color
                                else if (argType == typeof(Color))
                                {
                                    writer.Write((byte)18);
                                    writer.Write((Color)arg);
                                }
                                // color32
                                else if (argType == typeof(Color32))
                                {
                                    writer.Write((byte)19);
                                    writer.Write((Color32)arg);
                                }
                                // quaternion
                                else if (argType == typeof(Quaternion))
                                {
                                    writer.Write((byte)20);
                                    writer.Write((Quaternion)arg);
                                }
                                else
                                {
                                    // invalid type!!!!!
                                    Debug.LogError("HEVS: Invalid Spawn parameter, unable to sync it with clients!");
                                }
                            }
                            break;
                    }
                }
            }
        }

        static void DeserializeSpawnedObjects(ByteBufferReader reader, bool discard = false)
        {
            // how many objects are we spawning?
            int count = reader.ReadInt();

            for (int i = 0; i < count; ++i)
            {
                SpawnCommand command = new SpawnCommand();

                // write type of command (prefab or not)
                command.isPrefab = reader.ReadBoolean();
                // what is the object type for callback?
                command.typeOrIndex = reader.ReadInt();
                // cluster ID
                command.clusterID = reader.ReadInt();

                // how many arguments for callback?
                int argCount = reader.ReadInt();
                if (argCount == 0)
                {
                    command.args = null;
                }
                else
                {
                    command.args = new object[argCount];

                    for (int arg = 0; arg < argCount; ++arg)
                    {
                        byte argType = reader.ReadByte();

                        switch (argType)
                        {
                            case 0: command.args[arg] = null; break;
                            case 1: command.args[arg] = reader.ReadBoolean(); break;
                            case 2: command.args[arg] = reader.ReadChar(); break;
                            case 3: command.args[arg] = reader.ReadSByte(); break;
                            case 4: command.args[arg] = reader.ReadByte(); break;
                            case 5: command.args[arg] = reader.ReadShort(); break;
                            case 6: command.args[arg] = reader.ReadUShort(); break;
                            case 7: command.args[arg] = reader.ReadInt(); break;
                            case 8: command.args[arg] = reader.ReadUInt(); break;
                            case 9: command.args[arg] = reader.ReadLong(); break;
                            case 10: command.args[arg] = reader.ReadULong(); break;
                            case 11: command.args[arg] = reader.ReadFloat(); break;
                            case 12: command.args[arg] = reader.ReadDouble(); break;
                            case 14: command.args[arg] = reader.ReadString(); break;
                            case 15: command.args[arg] = reader.ReadVector2(); break;
                            case 16: command.args[arg] = reader.ReadVector3(); break;
                            case 17: command.args[arg] = reader.ReadVector4(); break;
                            case 18: command.args[arg] = reader.ReadColor(); break;
                            case 19: command.args[arg] = reader.ReadColor32(); break;
                            case 20: command.args[arg] = reader.ReadQuaternion(); break;
                            default: Debug.LogError("HEVS: Invalid spawn argument type received for type [" + command.typeOrIndex + "]."); break;
                        }
                    }
                }

                if (!discard)
                    spawnCommands.Add(command);
            }
        }
        #endregion

        #region Replay
        static void ReplayPreUpdateStep()
        {
            if (Replay.active.isPlaying)
            {
                if (Replay.active.isPlaybackDone == false)
                {
                    // read size
                    int packetSize = Replay.active.playbackStream.ReadInt();

                    // override time
                    float originalTime = time;
                    frameCount = Replay.active.playbackStream.ReadInt();
                    time = Replay.active.playbackStream.ReadFloat();
                    deltaTime = Replay.active.playbackStream.ReadFloat();

                    Input.Deserialize(Replay.active.playbackStream);

                    PreUpdateDeserializationDelegate?.Invoke(Replay.active.playbackStream);

                    // slow down framerate to match logged rate
                    if (originalTime < time)
                        Thread.Sleep(Mathf.CeilToInt((time - originalTime) * 1000));
                }
            }
            else if (Replay.active.isRecording)
            {
                // write placeholder packet size
                int loc = Replay.active.recordStream.Position;
                Replay.active.recordStream.Write((int)0);

                // time
                Replay.active.recordStream.Write(frameCount);
                Replay.active.recordStream.Write(time);
                Replay.active.recordStream.Write(deltaTime);

                Input.Serialize(Replay.active.recordStream);

                // append app specific payload
                PreUpdateSerializationDelegate?.Invoke(Replay.active.recordStream);

                // write packet size
                int size = Replay.active.recordStream.Position - loc;
                Replay.active.recordStream.Position = loc;
                Replay.active.recordStream.Write(size);
                Replay.active.recordStream.Position += size - sizeof(int);
            }
        }

        static void ReplayPostUpdateStep()
        {
            switch (Replay.active.state)
            {
                case ReplayState.Playing:
                    {
                        // read packet size
                        int size = Replay.active.playbackStream.ReadInt();

                        DeserializeSpawnedObjects(Replay.active.playbackStream, true);

                        SpawnObjects();

                        DeserializeClusterObjectStates(Replay.active.playbackStream);

                        ChangeClusterObjectStates();

                        DeserializeClusterTransforms(Replay.active.playbackStream);
                        ClusterVariable.DeserializeDirtyVariables(Replay.active.playbackStream);
                        RPC.DeserializeCallsOnAll(Replay.active.playbackStream);
                        PostUpdateDeserializationDelegate?.Invoke(Replay.active.playbackStream);

                        // done
                        if (Replay.active.isPlaybackDone)
                            Replay.active.StopPlayback();

                        // pause
                        // TODO: Change to something else to control pausing
                        if (UnityEngine.Input.GetKeyUp(KeyCode.Pause))
                            Replay.active.PausePlayback();
                    }
                    break;
                case ReplayState.PlayingPaused:
                    {
                        // unpause
                        if (UnityEngine.Input.GetKeyUp(KeyCode.Pause))
                            Replay.active.PausePlayback(false);
                    }
                    break;
                case ReplayState.Recording:
                    {
                        // write packet size placeholder
                        int loc = Replay.active.recordStream.Position;
                        Replay.active.recordStream.Write((int)0);

                        // TODO: wrap this up in the replay system somehow
                        SerializeSpawnedObjects(Replay.active.recordStream);
                        SerializeClusterObjectStates(Replay.active.recordStream);
                        SerializeClusterTransforms(Replay.active.recordStream);
                        ClusterVariable.SerializeDirtyVariables(Replay.active.recordStream);
                        RPC.SerializeCallsOnAll(Replay.active.recordStream);
                        PostUpdateSerializationDelegate?.Invoke(Replay.active.recordStream);

                        // write actual packet size
                        int size = Replay.active.recordStream.Position - loc;
                        Replay.active.recordStream.Position = loc;
                        Replay.active.recordStream.Write(size);
                        Replay.active.recordStream.Position += size - sizeof(int);

                        // save the data
                        Replay.active.Flush();

                        // pause
                        // TODO: Change to something else to control pausing
                        if (UnityEngine.Input.GetKeyUp(KeyCode.Pause))
                            Replay.active.PauseRecording();
                    }
                    break;
                case ReplayState.RecordingPaused:
                    {
                        // unpause
                        if (UnityEngine.Input.GetKeyUp(KeyCode.Pause))
                            Replay.active.PauseRecording(false);
                    }
                    break;
            }
        }
        #endregion

        #region Cluster Transforms
        static void SerializeClusterTransforms(ByteBufferWriter writer)
        {
            writer.Write(clusterObjects.Count); //Write clusterObjects Count
            foreach (var co in clusterObjects)
            {           
                writer.Write(co.Key); //Write id
                writer.Write((byte)co.Value.transformFlags); //Write flags

                if ((co.Value.transformFlags & (int)TransformFlags.Position) != 0)
                    writer.Write(co.Value.transform.position);	//Write position
                if ((co.Value.transformFlags & (int)TransformFlags.Rotation) != 0)
                    writer.Write(co.Value.transform.rotation);	//Write rotation
                if ((co.Value.transformFlags & (int)TransformFlags.Scale) != 0)
                    writer.Write(co.Value.transform.localScale);	//Write scale

                bool changed = co.Value.parentChanged;
                writer.Write(changed);	//Write parent change
                if (changed)
                    writer.Write(co.Value.parentID);	//Write parent ID
            }
        }

        static void DeserializeClusterTransforms(ByteBufferReader reader)
        {
            ClusterObject[] cos = GameObject.FindObjectsOfType<ClusterObject>();
            int transformCount = reader.ReadInt(); //Read clusterObjects Count
            for (int i = 0; i < transformCount; ++i)
            {
                int id = reader.ReadInt(); //Read id
                byte flags = reader.ReadByte(); //Read flags

				Vector3 p = Vector3.zero, s = Vector3.one;
				Quaternion r = Quaternion.identity;

				bool parentChanged = false;
				int parentID = -1;

				if ((flags & (int)TransformFlags.Position) != 0)
					p = reader.ReadVector3();	//Read position
				if ((flags & (int)TransformFlags.Rotation) != 0)
					r = reader.ReadQuaternion();	//Read rotation
				if ((flags & (int)TransformFlags.Scale) != 0)
					s = reader.ReadVector3();	//Read scale

				parentChanged = reader.ReadBoolean(); //Read parent changed
				if (parentChanged)
					parentID = reader.ReadInt();  //Read parent ID

				if (clusterObjects.ContainsKey(id))
				{
					ClusterObject co = clusterObjects[id];

					// find parent and assign them!
					if (parentChanged)
                    {
                        if (parentID == -1)
                            co.transform.SetParent(null);
                        else
                        {
                            if (clusterObjects.ContainsKey(parentID))
                                co.transform.SetParent(clusterObjects[parentID].transform);
                            else
                                Debug.Log("HEVS: Unable to set parent!");
                        }
                    }

					if ((flags & (int)TransformFlags.Position) != 0)
						co.transform.position = p;
					if ((flags & (int)TransformFlags.Rotation) != 0)
						co.transform.rotation = r;
					if ((flags & (int)TransformFlags.Scale) != 0)
						co.transform.localScale = s;
				}
            }
        }
        #endregion

        #region Data Broadcasting
        internal static bool enableDataBroadcast = false;

        static NetMQSocket dataBroadcastSocket;
        static Thread dataBroadcastThread;
        static volatile bool dataBroadcastRunning = false;
        static volatile object dataBroadcastLock = new object();
        static volatile bool hasBroadcastDataToSend = false;

        class DataBroadcastPacket
        {
            public int header;
            public byte[] data;
        }

        static Queue<DataBroadcastPacket> dataBroadcastPackets = new Queue<DataBroadcastPacket>();

        static List<IDataBroadcastReceiver> dataBroadcastReceivers = new List<IDataBroadcastReceiver>();

        /// <summary>
        /// Method to register a receiver for data broadcasting.
        /// </summary>
        /// <param name="receiver">The receiver for data broadcasting that you wish to register.</param>
        public static void RegisterDataBroadcastReceiver(IDataBroadcastReceiver receiver)
        {
            if (!dataBroadcastReceivers.Contains(receiver))
                dataBroadcastReceivers.Add(receiver);
        }

        /// <summary>
        /// Deregister a receiver for data broadcasting.
        /// </summary>
        /// <param name="receiver">The receiver to deregister.</param>
        public static void DeregisterDataBroadcastReceiver(IDataBroadcastReceiver receiver)
        {
            dataBroadcastReceivers.Remove(receiver);
        }

        static void InitialiseDataBroadcasting()
        {
            if (isMaster)
            {
                dataBroadcastSocket = new PublisherSocket();
                dataBroadcastSocket.Bind("tcp://*:" + config.broadcastPort);
                dataBroadcastThread = new Thread(MasterBroadcastThreadFunc);
            }
            else
            {
                dataBroadcastSocket = new SubscriberSocket();
                dataBroadcastSocket.Connect("tcp://" + masterAddress + ":" + config.broadcastPort);
                (dataBroadcastSocket as SubscriberSocket).SubscribeToAnyTopic();
                dataBroadcastThread = new Thread(ClientBroadcastThreadFunc);
            }

            dataBroadcastThread.Start();
            dataBroadcastRunning = true;
        }

        static void ShutdownDataBroadcasting()
        {
            // kill thread
            Monitor.Enter(dataBroadcastLock);
            dataBroadcastRunning = false;
            Monitor.Exit(dataBroadcastLock);

            if (dataBroadcastThread != null)
                dataBroadcastThread.Join();
            dataBroadcastThread = null;

            // kill socket
            if (dataBroadcastSocket != null)
                dataBroadcastSocket.Close();
            dataBroadcastSocket = null;
        }

        /// <summary>
        /// Broadcast data within a clustered platform.
        /// Data broadcast is threaded to have minimal impact on the performance of the cluster while broadcasting.
        /// Large buffers and files can be broadcast this way.
        /// </summary>
        /// <param name="packetHeader">An identifier for the packet being sent. This serves no purpose internally, but can be used to identify data being received by clients within the cluster.</param>
        /// <param name="data">The data to be broadcast.</param>
        public static void BroadcastData(int packetHeader, byte[] data)
        {
            if (isMaster)
            {
                if (dataBroadcastRunning)
                {
                    Monitor.Enter(dataBroadcastLock);
                    dataBroadcastPackets.Enqueue(new DataBroadcastPacket() { header = packetHeader, data = data });
                    hasBroadcastDataToSend = true;
                    Monitor.Exit(dataBroadcastLock);
                }
                else
                    Debug.LogWarning("HEVS: Data Broadcast is not active!");
            }
            else
                Debug.LogError("HEVS: Client tried to broadcast data! Only the master node can broadcast.");
        }

        static void MasterBroadcastThreadFunc()
        {
            while (dataBroadcastRunning)
            {
                while (hasBroadcastDataToSend)
                {
                    Monitor.Enter(dataBroadcastLock);
                    var packet = dataBroadcastPackets.Dequeue();
                    dataBroadcastSocket.SendMoreFrame(System.BitConverter.GetBytes(packet.header)).SendFrame(packet.data);
                    hasBroadcastDataToSend = dataBroadcastPackets.Count > 0;
                    Monitor.Exit(dataBroadcastLock);
                }
            }
        }

        static void ClientBroadcastThreadFunc()
        {
            while (dataBroadcastRunning)
            {
                bool more;
                byte[] header;
                if (dataBroadcastSocket.TryReceiveFrameBytes(out header, out more))
                {
                    Monitor.Enter(dataBroadcastLock);
                    dataBroadcastPackets.Enqueue(new DataBroadcastPacket() { header = System.BitConverter.ToInt32(header, 0), data = dataBroadcastSocket.ReceiveFrameBytes() });
                    Monitor.Exit(dataBroadcastLock);
                }
            }
        }

        static void HandleReceivedDataBroadcasts()
        {
            Monitor.Enter(dataBroadcastLock);

            while (dataBroadcastPackets.Count > 0)
            {
                var packet = dataBroadcastPackets.Dequeue();
                foreach (var receiver in dataBroadcastReceivers)
                    receiver.OnDataReceived(packet.header, packet.data);
            }

            Monitor.Exit(dataBroadcastLock);
        }
        #endregion
    }
}
