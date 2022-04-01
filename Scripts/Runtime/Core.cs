using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Collections;
using HEVS.Extensions;
using System.Runtime.InteropServices;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HEVS
{
    /// <summary>
    /// The HEVS Core components. This component acts as the "brain" for HEVS, tying together all systems.
    /// </summary>
    [AddComponentMenu("HEVS/HEVS Core")]
    public class Core : MonoBehaviour, IRPCInterface
    {
        /// <summary>
        /// Static access to the GameObject the component has been added to.
        /// </summary>
        public static GameObject coreGameObject { get { return instance.gameObject; } }

        /// <summary>
        /// Flag for checking if HEVS is active and its systems are running.
        /// </summary>
        public static bool isActive { get; private set; }

        /// <summary>
        /// Access to the active Platform's config data.
        /// </summary>
        public static Config.Platform activePlatform { get; private set; }

        /// <summary>
        /// Access to the active Node's config data.
        /// </summary>
        public static Config.Node activeNode { get; private set; }

        /// <summary>
        /// Utility access to the active Node's index.
        /// </summary>
        public static Vector3Int nodeIndex { get { return activeNode.index; } }

        /// <summary>
        /// Access to this node's primary display, i.e. the first one defined within the config.
        /// </summary>
        public static Display primaryActiveDisplay { get { return platformDisplays.Count > 0 ? platformDisplays[0] : null; } }

        /// <summary>
        /// Access to this node's displays.
        /// </summary>
        public static List<Display> activeDisplays { get; private set; } = new List<Display>();

        /// <summary>
        /// Access to all displays defined within the active platform.
        /// Note: The list of displays will include all platform displays, regardless of if a node within the platform uses them or not.
        /// </summary>
        public static List<Display> platformDisplays { get; private set; } = new List<Display>();

        /// <summary>
        /// Access to all registered and active OSC receiver MonoBehaviours.
        /// </summary>
        public Dictionary<string, List<KeyValuePair<MonoBehaviour, MethodInfo>>> oscReceivers { get; private set; } = new Dictionary<string, List<KeyValuePair<MonoBehaviour, MethodInfo>>>();

        [SerializeField]
        private bool includeOSC = false;

        [SerializeField]
        private bool includeConsole = false;

        /// <summary>
        /// Will generate a Unity Mesh for the active node's displays.
        /// </summary>
        /// <returns>Unity Mesh</returns>
        public static Mesh GenerateMeshForActiveNode()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            List<Vector3> subvertices = new List<Vector3>();
            List<int> sybindices = new List<int>();
            List<Vector2> subuvs = new List<Vector2>();

            foreach (var d in activeDisplays)
            {
                d.GatherDisplayGeometry(subvertices, sybindices, subuvs);

                int offset = vertices.Count;
                vertices.AddRange(subvertices);
                uvs.AddRange(subuvs);
                foreach (int i in sybindices)
                    indices.Add(i + offset);
            }

            if (vertices.Count > 0)
            {
                Mesh mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0, true);
                mesh.RecalculateNormals();
                return mesh;
            }

            return null;
        }

        /// <summary>
        /// Will generate a Unity Mesh for all displays within the active platform, even if no node uses them.
        /// </summary>
        /// <returns>Unity Mesh</returns>
        public static Mesh GenerateMeshForActivePlatform()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            List<Vector3> subvertices = new List<Vector3>();
            List<int> sybindices = new List<int>();
            List<Vector2> subuvs = new List<Vector2>();

            foreach (var d in platformDisplays)
            {
                d.GatherDisplayGeometry(subvertices, sybindices, subuvs);

                int offset = vertices.Count;
                vertices.AddRange(subvertices);
                uvs.AddRange(subuvs);
                foreach (int i in sybindices)
                    indices.Add(i + offset);
            }

            if (vertices.Count > 0)
            {
                Mesh mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0, true);
                mesh.RecalculateNormals();
                return mesh;
            }

            return null;
        }

        /// <summary>
        /// Convert a screen point to a Ray.
        /// Will check all active displays and return the first successful ray found from a matching display.
        /// </summary>
        /// <param name="point">The screen point to create the ray from.</param>
        /// <returns>The Ray found, or a Ray from the Camera.main if no matching displays were found.</returns>
        public static Ray ScreenPointToRay(Vector3 point)
        {
            // convert point to viewspace [0,1]
            Vector2 uv = point.xy();
            uv.x /= Screen.width;
            uv.y /= Screen.height;

            // check if point is within any displays (their viewport)
            foreach (Display display in activeDisplays)
            {
                Ray ray = new Ray();
                if (display.ViewportPointToRay(uv, ref ray))
                    return ray;
            }

            return Camera.main.ScreenPointToRay(point);
        }

        /// <summary>
        /// The current HEVS version number.
        /// </summary>
        public const string VERSION = "2.0.0";

        /// <summary>
        /// The path to the JSON file used for initialising HEVS. This can be overridden via command lines arguments or environment variables.
        /// </summary>
        public string configFile;

        /// <summary>
        /// The current Config object.
        /// </summary>
        public static HEVS.Config config { get; private set; } = new Config();
        
        /// <summary>
        /// The random seed used to initialise all nodes within the cluster. 
        /// This aids in enabling the use of random, however it is still reliant on calls 
        /// being made on the master and clients in the same order to maintain synchronisation. 
        /// It is recommended that you only use random on a master and then synchronise the result.
        /// </summary>
        public int randomSeed = 42;

        /// <summary>
        /// The delegate definition for a PreUpdate handler that can be used by scripts to have a 
        /// method called before all other scripts begin their update event.
        /// </summary>
        public delegate void PreUpdate();

        /// <summary>
        /// The PreUpdate delegate.
        /// </summary>
        public static PreUpdate OnPreUpdate;

        /// <summary>
        /// Flag for if Unity should quit the application when the Escape key is pressed.
        /// </summary>
        public bool quitOnEscape = false;

        internal static UnityEngine.Camera mainCamera = null;

        [SerializeField]
        private string selectedPlatform = null;
        [SerializeField]
        private string selectedNode = null;
        [SerializeField]
        private string loadedConfig;

        [SerializeField]
        internal bool clusterInEditor = false;

        [SerializeField]
        private bool enableDataBroadcast = false;

        /// <summary>
        /// An editor flag for displaying only the current node's Gizmos.
        /// </summary>
        public bool debugDrawCurrentOnly = false;

        /// <summary>
        /// A flag specifying if HEVS should ignore the hostname of the running instance and instead 
        /// use the name of a selected node to impersonate.
        /// </summary>
        public bool debugImpersonateNode = false;

        /// <summary>
        /// A registered list of prefabs that can be instantiated by the Cluster.
        /// </summary>
        public List<ClusterObject> spawnablePrefabList;

        static Core instance;

        /// <summary>
        /// The port that OSC will use for receiving messages.
        /// </summary>
        public int oscPort { get { return _oscPort; } }

        [SerializeField]
        int _oscPort = 7800;

        OSCsharp.Net.UDPReceiver oscReceiver;

        void Awake()
        {
            if (instance != null)
                throw new UnityException("Only one HEVS Core Component is allowed to be active at a time!");

            instance = this;

            if (!isActive)
            {
				if (!UnityEngine.Application.isEditor)
                    debugImpersonateNode = false;

                Graphics.Initialise();

                // gather custom display types before parsing config
                Display.GatherCustomDisplayTypes();

                // load hevs config from streaming assets
                string env = Environment.GetEnvironmentVariable("HEVS_CONFIG");
                if (!string.IsNullOrEmpty(env))
                    configFile = env;

                string nodeName = (debugImpersonateNode ? selectedNode : SystemInfo.deviceName).ToLower();
                string platformName = selectedPlatform;

                env = Environment.GetEnvironmentVariable("HEVS_PLATFORM");
                if (!string.IsNullOrEmpty(env))
                    platformName = env;

                // handle command line arguments
                string[] args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("-nocursor", StringComparison.OrdinalIgnoreCase))
                        Cursor.visible = false;
                    else if (args[i].Equals("-config", StringComparison.OrdinalIgnoreCase))
                        configFile = args[++i];
                    else if (args[i].Equals("-platform", StringComparison.OrdinalIgnoreCase))
                        platformName = args[++i];
                    else if (args[i].Equals("-node", StringComparison.OrdinalIgnoreCase))
                        nodeName = args[++i];
                }

                if (nodeName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    nodeName = SystemInfo.deviceName;

                Cluster.enableDataBroadcast = !args.Contains("-disable_broadcast") && enableDataBroadcast;

                Debug.Log("HEVS " + VERSION + " starting up... Config: [" + configFile + "] Platform: [" + platformName + "] Node: [" + nodeName + "]");
                config.ParseConfig(configFile);

                bool ignoreCluster = (clusterInEditor && UnityEngine.Application.isEditor) || Environment.GetCommandLineArgs().Contains("-nocluster");

                Config.Platform platform;
                Config.Node node;
                if (!config.FindPlatformAndNode(platformName, nodeName, out platform, out node))
                    Debug.Log("HEVS: No matching Platform and Node combination found, using Default platform.");

                activePlatform = platform;
                activeNode = node;

                // create platform displays
                foreach (var display in activePlatform.displays)
                {
                    platformDisplays.Add(Display.CreateDisplay(display.Value));

                    if (activeNode.displays.Contains(display.Value))
                        activeDisplays.Add(platformDisplays.Last());
                }

                // osc from config
                if (activePlatform.oscPort.HasValue)
                    _oscPort = activePlatform.oscPort.Value;
                if (activePlatform.oscEnabled.HasValue)
                    includeOSC = activePlatform.oscEnabled.Value;

                RPC.RegisterStaticRPCalls();
				Cluster.Initialise(platform.cluster, spawnablePrefabList, randomSeed, clusterInEditor);
                new Input();
                VRPN.CheckVRPNAvailable();

                if (includeConsole)
                    gameObject.GetOrAddComponent<Console>();

                isActive = true;
			}

			Cluster.InitialisePhysics();

            Tracker.GatherCustomTrackerTypes();

            ConfigureCameraAndDisplayForScene();

            // setup cluster IDs
            var cos = GameObject.FindObjectsOfType<ClusterObject>();
            foreach (var co in cos)
            {
                if (co.clusterID == -1)
                    co.clusterID = Cluster.nextClusterID;
            }

            if (Cluster.isMaster)
            {
                string[] args = Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-logsession");
#if UNITY_EDITOR
                if (index > 0 ||
                    EditorPrefs.GetInt("replay_onplay_state") == 2)
#else
                if (index > 0)
#endif
                {
                    Replay.active = new Replay(args[index + 1]);
                    Replay.active.StartRecording();
                }
                else
                {
                    index = Array.IndexOf(args, "-playsession");
#if UNITY_EDITOR
                    if (index > 0 ||
                        EditorPrefs.GetInt("replay_onplay_state") == 1)
#else
                    if (index > 0)
#endif
                    {
                        Replay.active = new Replay(args[index + 1]);
                        Replay.active.StartPlayback();
                    }
                }
            }

            // only works where user32.dll is available (i.e. Windows)
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                // rename the window
                var windowPtr = FindWindow(null, Application.productName);
                if (windowPtr != null)
                    SetWindowText(windowPtr, Application.productName + " - " + activeNode.id + (Cluster.isMaster ? " (master)" : " (client)"));
            }

            // start end of frame coroutine?
            StartCoroutine(OnEndOfFrame());
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        static extern System.IntPtr FindWindow(System.String className, System.String windowName);

        internal void OnOSCReceived(object sender, OSCsharp.Net.OscMessageReceivedEventArgs args)
        {
            if (oscReceivers.ContainsKey(args.Message.Address))
            {
                foreach (var receiver in oscReceivers[args.Message.Address])
                    receiver.Value.Invoke(receiver.Key, new object[] { args.Message.Data });
            }
        }

        void Start()
        {
            if (Cluster.isMaster &&
                includeOSC)
            {
                // setup osc receiver
                oscReceiver = new OSCsharp.Net.UDPReceiver(oscPort, true);
                oscReceiver.MessageReceived += OnOSCReceived;
                oscReceiver.Start();
            }

            Application.targetFrameRate = Cluster.isMaster ? 120 : 60;
        }

        IEnumerator OnEndOfFrame()
        {
            do
            {
                yield return new WaitForEndOfFrame();

                Cluster.FrameSync();

                if (Graphics.outputRenderTexture)
                    Graphics.Sync(); 

            } while (instance != null);
        }

        void OnDestroy()
        {
            if (Cluster.isMaster)
            {
                if (oscReceiver != null)
                {
                    oscReceiver.Stop();
                    oscReceiver = null;
                }
            }

            Cluster.ResetState();
            instance = null;
        }

        void Update()
        {
            VRPN.UpdateDevices();
			Input.Update();
            Cluster.PreUpdate();
            OnPreUpdate?.Invoke();

            if (quitOnEscape &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                Graphics.Shutdown();
                UnityEngine.Application.Quit();
            }
		}

        void LateUpdate()
        {
			Cluster.PostUpdate();
		}

        void OnApplicationQuit()
		{ 
            Cluster.Shutdown();
        }

#region Camera and Display Configuration
        void ConfigureCameraAndDisplayForScene()
        {
            Camera.displayCameras.Clear();

            // find the main camera and setup displays!
            mainCamera = null;

            // find main camera
            UnityEngine.Camera[] cameras = FindObjectsOfType<UnityEngine.Camera>();
            foreach (UnityEngine.Camera c in cameras)
            {
                if (c.gameObject.tag == "MainCamera")
                {
                    if (mainCamera != null)
						throw new UnityException("Too many cameras tagged as MainCamera found in the scene!");

                    mainCamera = c;
                }
            }

            if (mainCamera == null)
                throw new UnityException("Unable to find a camera tagged as MainCamera! There must be one and only one!");

            // should we attach a tracker to the mainCamera?
            if (Config.Stereo.current != null &&
                Config.Stereo.current.tracker != null)
            {
                bool found = false;
                var trackers = mainCamera.GetComponents<Tracker>();
                foreach (var tracker in trackers)
                {
                    if (tracker.configId == Config.Stereo.current.tracker.id)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var tracker = mainCamera.gameObject.AddComponent<Tracker>();
                    tracker.forceMouseInEditor = false;
                    tracker.configId = Config.Stereo.current.tracker.id;
                    Debug.Log("HEVS: Added tracker [" + tracker.configId + "] to main camera");
                }
            }

            if (activeNode != null)
            {
                if (Screen.currentResolution.width != activeNode.resolution.x ||
                    Screen.currentResolution.height != activeNode.resolution.y ||
                    Screen.fullScreenMode != activeNode.screenMode)
                {
                    Screen.SetResolution(activeNode.resolution.x,
                                        activeNode.resolution.y,
                                        activeNode.screenMode);
                }
                    
                mainCamera.enabled = false;

                if (activeDisplays.Count == 1)
                    activeDisplays[0].InitialiseDisplay(mainCamera.gameObject);
                else if (activeDisplays.Count > 0)
                    activeDisplays.ForEach(display => display.InitialiseDisplay());
                else
                    mainCamera.enabled = true;
            }
        }

#endregion

#region Gizmo
        void OnDrawGizmos()
        {
            Config.Platform platform = null;
            Config.Node node = null;

            if (config != null)
            {
                // ensure correct data is loaded
                if (!string.IsNullOrEmpty(configFile) &&
                    !config.jsonPath.Equals(configFile, StringComparison.OrdinalIgnoreCase))
                    config.ParseConfig(configFile);

                // find the selected platform to draw display for
                if (!string.IsNullOrEmpty(selectedPlatform) &&
                    config.platforms.TryGetValue(selectedPlatform, out platform))
                {
                    platform.nodes.TryGetValue(selectedNode, out node);

                    if (Display.registeredDisplayTypes.Count == 0)
                        Display.GatherCustomDisplayTypes();

                    foreach (var pair in platform.displays)
                    {
                        // select highlight colour, or skip display
                        Gizmos.color = Color.white;

                        if (node != null && node.displays.Contains(pair.Value))
                            Gizmos.color = Color.green;
                        else if (debugDrawCurrentOnly)
                            continue;

                        Display.DrawGizmoForDisplay(pair.Value);
                    }
                }
            }
        }
        #endregion
    }
}