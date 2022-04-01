using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HEVS.Extensions;

namespace HEVS
{
    /// <summary>
    /// The type of frame syncing being used.
    /// </summary>
    public enum FrameLockMode
    {
        /// <summary>
        /// Software frame-locking attempts to block all nodes from continuing past their update event until all nodes are ready.
        /// Low frame-rates can cause sync issues.
        /// </summary>
        Software,
        /// <summary>
        /// Hardware frame-locking uses specific hardware features to prevent nodes from swapping their back-buffers until they are all ready.
        /// This requires specific hardware running under OpenGL, such as NVidia Quadro GPUs.
        /// </summary>
        Hardware
    }

    public partial class Config
    {
        /// <summary>
        /// JSON config object for HEVS Cluster options.
        /// </summary>
        public class Cluster : IConfigObject
        {
            /// <summary>
            /// Creates a cluster config with a set master node.
            /// </summary>
            /// <param name="platform">The platform this config belongs to.</param>
            public Cluster(Platform platform)//, Node master)
            {
                this.platform = platform;
           //     this.master = master;
            }

            /// <summary>
            /// Access to the cluster's owning platform config.
            /// </summary>
            public Platform platform { get; private set; }

            /// <summary>
            /// Access the cluster's master node config.
            /// </summary>
            public Node master { get; internal set; }

            /// <summary>
            /// Port used for broadcasting/receiving frame data.
            /// </summary>
            public int dataPort { get; private set; } = 7777;

            /// <summary>
            /// Port used for synchronising frames.
            /// </summary>
            public int syncPort { get; private set; } = 7778;

            /// <summary>
            /// Port used for data broadcasting of large packets (project must enable data broadcasting to make use of this).
            /// </summary>
            public int broadcastPort { get; private set; } = 7776;

            /// <summary>
            /// Temporary port used for ensuring nodes join a hardware framelock group (only applies to GPU hardware with swapgroup support).
            /// </summary>
            public int lockPort { get; private set; } = 7779;

            /// <summary>
            /// The byte limit for packets sent between the master and client nodes.
            /// </summary>
            public int packetLimit { get; private set; } = 65536;

            /// <summary>
            /// The framelock mode to use for the cluster.
            /// </summary>
            public FrameLockMode framelockMode { get; private set; } = FrameLockMode.Software;

            /// <summary>
            /// Access to the cluster's framelock master node.
            /// </summary>
            public Node framelockMaster { get; private set; }

            /// <summary>
            /// Flag to disable Unity physics on client nodes.
            /// </summary>
            public bool disableClientPhysics { get; private set; } = true;

            /// <summary>
            /// Flag to automatically attach CLusterObject components to GameObjects that contain RigidBody components.
            /// </summary>
            public bool autoSyncRigidBodies { get; private set; } = true;

            /// <summary>
            /// The time limit before an unresponsive client is dropped, in milliseconds.
            /// </summary>
            public int clientTimeoutLimit { get; private set; } = 5000;

            /// <summary>
            /// Access to the SimpleJSON JSON data that was used to create this ClusterConfig.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// The method used to parse JSON data and initialise this instance.
            /// </summary>
            /// <param name="json">The SimpleJSON JSON data.</param>
            /// <returns>Returns true if JSON successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                this.json = json;

                if (json.Keys.Contains("framelock"))
                    framelockMode = (FrameLockMode)Enum.Parse(typeof(FrameLockMode), json["framelock"], true);

                if (json.Keys.Contains("data_port"))
                    dataPort = json["data_port"].AsInt;

                if (json.Keys.Contains("sync_port"))
                    syncPort = json["sync_port"].AsInt;

                if (json.Keys.Contains("broadcast_port"))
                    broadcastPort = json["broadcast_port"].AsInt;

                if (json.Keys.Contains("lock_port"))
                    lockPort = json["lock_port"].AsInt;

                if (json.Keys.Contains("packet_limit"))
                    packetLimit = json["packet_limit"].AsInt;

                if (json.Keys.Contains("auto_sync_rigid"))
                    autoSyncRigidBodies = json["auto_sync_rigid"].AsBool;

                if (json.Keys.Contains("disable_client_physics"))
                    disableClientPhysics = json["disable_client_physics"].AsBool;

                if (json.Keys.Contains("client_timeout"))
                    clientTimeoutLimit = json["client_timeout"].AsInt;

                return true;
            }

            /// <summary>
            /// The method to parse JSON data and initialise this instance, using the current platform's nodes.
            /// </summary>
            /// <param name="json">The SimpleJSON JSON data to parse.</param>
            /// <param name="platformNodes">The current platform's nodes.</param>
            /// <returns>Returns true if JSON successfully parsed, false otherwise.</returns>
            internal bool Parse(SimpleJSON.JSONNode json, Dictionary<string, Node> platformNodes)
            {
                if (!Parse(json))
                    return false;

                if (json.Keys.Contains("master"))
                {
                    Node masterNode;
                    if (!platformNodes.TryGetValue(json["master"].Value, out masterNode))
                    {
                        Debug.LogError("HEVS: Invalid cluster options - can not find master node!");
                        return false;
                    }
                    master = masterNode;
                }

                if (json.Keys.Contains("framelock_master"))
                {
                    Node masterNode;
                    if (!platformNodes.TryGetValue(json["framelock_master"].Value, out masterNode))
                    {
                        Debug.LogError("HEVS: Invalid cluster options - can not find framelock master node!");
                        return false;
                    }
                    framelockMaster = masterNode;
                }

                return true;
            }
        }
    }
}