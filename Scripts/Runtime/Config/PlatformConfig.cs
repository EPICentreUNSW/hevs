using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace HEVS
{
    /// <summary>
    /// A helper enumeration of potential platform types that HEVS may use. 
    /// This is not a strict list, but can be used to help identify particular platform designs.
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// Undefined platform type.
        /// </summary>
        Undefined,
        /// <summary>
        /// Standard desktop platform.
        /// </summary>
        Desktop,
        /// <summary>
        /// A VR/AR platform.
        /// </summary>
        XR,
        /// <summary>
        /// A cylindrical platform. The platform might use Curved or Off-Axis displays to define a cylinder shape.
        /// </summary>
        Cylinder,
        /// <summary>
        /// A dome platform, typically using one or more projectors.
        /// </summary>
        Dome,
        /// <summary>
        /// A CAVE platform, of unknown configuration, that typically uses Off-Axis displays.
        /// </summary>
        CAVE,
        /// <summary>
        /// A handheld platform, such as phone or tablet.
        /// </summary>
        Handheld
    }

    public partial class Config
    {
        /// <summary>
        /// A HEVS platform configuration that contains all nodes, displays, trackers and other features 
        /// that are used to implement HEVS for a particular platform.
        /// </summary>
        public class Platform
        {
            /// <summary>
            /// Construct a platform with a specific ID and optional type.
            /// </summary>
            /// <param name="id">The ID of the platform.</param>
            /// <param name="type">Optional platform type. Default is Undefined.</param>
            public Platform(string id, PlatformType type = PlatformType.Undefined)
            {
                this.id = id;
                this.type = type;
            }

            /// <summary>
            /// The ID of the platform.
            /// </summary>
            public string id { get; private set; }

            /// <summary>
            /// List of all platform IDs that this platform inherited from.
            /// </summary>
            public List<string> inherited { get; private set; } = new List<string>();

            /// <summary>
            /// Optional type for this platform. This is not used by any internal HEVS system, but it 
            /// is provided for user access. Dependant on being defined within the JSON config.
            /// </summary>
            public PlatformType type { get; private set; } = PlatformType.Undefined;

            /// <summary>
            /// Access to the master node config if within a cluster.
            /// </summary>
            public Node masterNode { get { return cluster == null ? null : cluster.master; } }

            /// <summary>
            /// A list of all HEVS Nodes that are conencted to this platform. May or may not be in a clustered configuration.
            /// </summary>
            public Dictionary<string, Node> nodes { get; private set; } = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// A list of all HEVS Displays that this platform uses. Not all displays need to be referenced by nodes.
            /// </summary>
            public Dictionary<string, Display> displays { get; private set; } = new Dictionary<string, Display>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// A list of all HEVS Trackers that this platform uses.
            /// </summary>
            public Dictionary<string, Tracker> trackers { get; private set; } = new Dictionary<string, Tracker>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// A list of all TUIO connections that this platform uses.
            /// </summary>
            public Dictionary<string, TUIODevice> tuioDevices { get; private set; } = new Dictionary<string, TUIODevice>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// A list of all alternative input sources that this platform uses.
            /// </summary>
            public Dictionary<string, InputSource> inputSources { get; private set; } = new Dictionary<string, InputSource>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// A list of all HEVS Viewports that this platform uses. Not all viewports need to be referenced by displays.
            /// </summary>
            public Dictionary<string, Viewport> viewports { get; private set; } = new Dictionary<string, Viewport>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// The stereoscopic configuration for this platform. This can be overridden on a per-display basis.
            /// </summary>
            public Stereo stereo { get; private set; }

            /// <summary>
            /// The cluster configuration for this platform. Multi-node platforms don't have to behave within 
            /// a cluster, although they typically will.
            /// </summary>
            public Cluster cluster { get; private set; }

            /// <summary>
            /// Flag to define if OSC should be enabled for this platform
            /// </summary>
            public bool? oscEnabled { get; private set; }

            /// <summary>
            /// Port for the OSC receive rto use.
            /// </summary>
            public int? oscPort { get; private set; }

            /// <summary>
            /// A list of named transforms specified within the config file.
            /// </summary>
            public Dictionary<string, Transform> transforms { get; private set; } = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// The preferred UI mode that this platform uses. Default is RenderMode.WorldSpace as this "should" work for 
            /// majority of platform configurations, but the UI is very dependant on the display layout.
            /// </summary>
            public RenderMode preferredUIMode { get; set; } = RenderMode.WorldSpace;

            /// <summary>
            /// Utility flag for checking if the platform is a dome.
            /// </summary>
            public bool isDome { get { return type == PlatformType.Dome; } }
            /// <summary>
            /// Utility flag for checking if the platform is a cylinder.
            /// </summary>
            public bool isCylinder { get { return type == PlatformType.Cylinder; } }
            /// <summary>
            /// Utility flag for checking if the platform is a CAVE.
            /// </summary>
            public bool isCAVE { get { return type == PlatformType.CAVE; } }
            /// <summary>
            /// Utility flag for checking if the platform is a standard desktop.
            /// </summary>
            public bool isDesktop { get { return type == PlatformType.Desktop; } }

            /// <summary>
            /// A collection of user-defined global variables for the platform, stored as a dictionary. This
            /// is setup directly from the JSON definition.
            /// </summary>
            public Dictionary<string, object> globals { get; private set; } = new Dictionary<string, object>();

            /// <summary>
            /// Access to the SimpleJSON JSON that was used to configure this platform.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// Parse JSON data to initialise this platform config, using previously parsed platforms for any inheritance.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <param name="existingPlatforms">Previously loaded platforms used for inheritance purposes.</param>
            /// <returns>Returns true if the data is successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json, Dictionary<string, Platform> existingPlatforms)
            {
                // grab our json (not an inherited copy)
                if (this.json == null)
                    this.json = json;

                // load inherit
                if (json.Keys.Contains("inherit"))
                {
                    var jsonArray = json["inherit"].AsArray;
                    if (jsonArray != null)
                    {
                        foreach (SimpleJSON.JSONNode jsonInherit in jsonArray)
                        {
                            string inheritFrom = jsonInherit;
                            if (!string.IsNullOrEmpty(inheritFrom))
                            {
                                Platform inheritedPlatform;
                                if (existingPlatforms.TryGetValue(inheritFrom, out inheritedPlatform))
                                {
                                    if (!Parse(inheritedPlatform.json, existingPlatforms))
                                    {
                                        Debug.LogError("HEVS: Failed to parse inherited platform [" + inheritFrom + "] for platform [" + id + "].");
                                        return false;
                                    }

                                    if (!inherited.Contains(inheritFrom))
                                        inherited.Add(inheritFrom);
                                }
                                else
                                {
                                    Debug.LogError("HEVS: Failed to find platform [" + inheritFrom + "] to inherit from for platform [" + id + "].");
                                }
                            }
                        }
                    }
                    else
                    {
                        string inheritFrom = json["inherit"];
                        if (!string.IsNullOrEmpty(inheritFrom))
                        {
                            Platform inheritedPlatform;
                            if (existingPlatforms.TryGetValue(inheritFrom, out inheritedPlatform))
                            {
                                if (!Parse(inheritedPlatform.json, existingPlatforms))
                                {
                                    Debug.LogError("HEVS: Failed to parse inherited platform [" + inheritFrom + "] for platform [" + id + "].");
                                    return false;
                                }

                                if (!inherited.Contains(inheritFrom))
                                    inherited.Add(inheritFrom);
                            }
                            else
                            {
                                Debug.LogError("HEVS: Failed to find platform [" + inheritFrom + "] to inherit from for platform [" + id + "].");
                            }
                        }
                    }
                }

                // set the type
                if (json.Keys.Contains("type"))
                    type = (PlatformType)Enum.Parse(typeof(PlatformType), json["type"], true);

                if (json.Keys.Contains("osc_enabled"))
                    oscEnabled = json["osc_enabled"].AsBool;

                if (json.Keys.Contains("osc_port"))
                    oscPort = json["osc_port"].AsInt;

                // set the preferred ui mode
                if (json.Keys.Contains("preferred_ui"))
                    preferredUIMode = (RenderMode)Enum.Parse(typeof(RenderMode), json["preferred_ui"], true);

                // load globals
                if (json.Keys.Contains("globals"))
                {
                    CombineDictionary(Utils.JSONToDictionary(json["globals"]), globals);
                }

                // load transforms
                if (json.Keys.Contains("transforms"))
                {
                    foreach (string transform_id in json["transforms"].Keys)
                    {
                        Transform transform;
                        if (!transforms.TryGetValue(transform_id, out transform))
                        {
                            transform = new Transform(transform_id);
                            transforms.Add(transform_id, transform);
                        }

                        if (!transform.Parse(json["transforms"][transform_id]))
                            Debug.LogError("HEVS: Failed to parse transform [" + transform_id + "] for platform [" + id + "]");
                    }
                }

                // load trackers
                if (json.Keys.Contains("trackers"))
                {
                    foreach (string tracker_id in json["trackers"].Keys)
                    {
                        // already got the tracker defined?
                        Tracker tracker;
                        if (!trackers.TryGetValue(tracker_id, out tracker))
                        {
                            tracker = new Tracker(tracker_id);
                            trackers.Add(tracker_id, tracker);
                        }

                        if (!tracker.Parse(json["trackers"][tracker_id], transforms))
                            Debug.LogError("HEVS: Failed to parse tracker [" + tracker_id + "] for platform [" + id + "]");
                    }
                }

                // load input sources
                if (json.Keys.Contains("input"))
                {
                    foreach (string input_id in json["input"].Keys)
                    {
                        // already got the input source defined?
                        InputSource inputSource;
                        if (!inputSources.TryGetValue(input_id, out inputSource))
                        {
                            inputSource = new InputSource(input_id);
                            inputSources.Add(input_id, inputSource);
                        }

                        if (!inputSource.Parse(json["input"][input_id]))
                            Debug.LogError("HEVS: Failed to parse input source [" + input_id + "] for platform [" + id + "]");
                    }
                }

                // load tuio devices
                if (json.Keys.Contains("tuio"))
                {
                    foreach (string tuio_id in json["tuio"].Keys)
                    {
                        // already got the device defined?  
                        TUIODevice device;
                        if (!tuioDevices.TryGetValue(tuio_id, out device))
                        {
                            device = new TUIODevice(tuio_id);
                            tuioDevices.Add(tuio_id, device);
                        }

                        if (!device.Parse(json["tuio"][tuio_id]))
                            Debug.LogError("HEVS: Failed to parse tuio [" + tuio_id + "] for platform [" + id + "]");
                    }
                }

                // load stereo
                if (stereo == null)
                    stereo = new Stereo();
                if (json.Keys.Contains("stereo"))
                    if (!stereo.Parse(json["stereo"], trackers))
                        Debug.LogError("HEVS: Failed to parse stereo config for platform [" + id + "]");

                // load viewports
                if (json.Keys.Contains("viewports"))
                {
                    foreach (string viewport_id in json["viewports"].Keys)
                    {
                        // does it already exist?
                        Viewport viewport;
                        if (!viewports.TryGetValue(viewport_id, out viewport))
                        {
                            viewport = new Viewport(viewport_id);
                            viewports.Add(viewport_id, viewport);
                        }
                       if (!viewport.Parse(json["viewports"][viewport_id]))
                            Debug.LogError("HEVS: Failed to parse viewport [" + viewport_id + "] for platform [" + id + "]");
                    }
                }

                // load displays
                if (json.Keys.Contains("displays"))
                {
                    foreach (string display_id in json["displays"].Keys)
                    {
                        // does it already exist?
                        Display newDisplay;
                        if (!displays.TryGetValue(display_id, out newDisplay))
                        {
                            newDisplay = new Display(display_id);
                            displays.Add(display_id, newDisplay);
                        }
                        if (!newDisplay.Parse(json["displays"][display_id], displays, stereo, viewports, transforms))
                            Debug.LogError("HEVS: Failed to parse display [" + display_id + "] for platform [" + id + "]");
                    }
                }

                // get a list of potentially ignore nodes
                List<string> ignoreNodes = new List<string>();
                var commandArgs = System.Environment.GetCommandLineArgs().ToList();
                int index = commandArgs.FindIndex(s => s.Equals("-ignorenodes", StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                    ignoreNodes.AddRange(commandArgs[index + 1].Split(','));

                // load nodes
                if (json.Keys.Contains("nodes"))
                {
                    foreach (string node_id in json["nodes"].Keys)
                    {
                        // ignore it?
                        if (ignoreNodes.Exists(s => s.Equals(node_id, StringComparison.OrdinalIgnoreCase))) continue;

                        string actual_id = node_id;
                        if (node_id.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                            actual_id = SystemInfo.deviceName;

                        // does it already exist?
                        Node newNode;
                        if (!nodes.TryGetValue(actual_id, out newNode))
                        {
                            newNode = new Node(this, actual_id);
                            nodes.Add(actual_id, newNode);
                        }

                        if (!newNode.Parse(json["nodes"][node_id], displays, stereo))
                            Debug.LogError("HEVS: Failed to parse node [" + node_id + "] for platform [" + id + "]");
                    }
                }

                // add a default node if there are no nodes
                if (nodes.Count == 0)
                {
                    // one default localhost node
                    Node newNode = new Node(this, SystemInfo.deviceName);
                    nodes.Add(SystemInfo.deviceName, newNode);
                    newNode.Parse("", displays, stereo);
                }

                // load cluster
                if (json.Keys.Contains("cluster"))
                {
                    if (cluster == null)
                        cluster = new Cluster(this);
                    if (!cluster.Parse(json["cluster"], nodes))
                        Debug.LogError("HEVS: Failed to parse cluster config for platform [" + id + "]");
                }

                return true;
            }

            void CombineDictionary(Dictionary<string, object> src, Dictionary<string, object> dst)
            {
                // for each original entry....
                src.ToList().ForEach(p =>
                {
                // if not in the destination, add it
                if (!dst.ContainsKey(p.Key))
                        dst.Add(p.Key, p.Value);
                    else
                    {
                    // if in the destrination, are they both dictionaries that we can combine?
                    if (dst[p.Key].GetType() == typeof(Dictionary<string, object>) &&
                            p.Value.GetType() == typeof(Dictionary<string, object>))
                        {
                        // combine
                        CombineDictionary(p.Value as Dictionary<string, object>,
                                              dst[p.Key] as Dictionary<string, object>);
                        }
                        else
                        // replacement value
                        dst[p.Key] = p.Value;
                    }
                });
            }

            /// <summary>
            /// Activates this platform, which may include connecting any trackers required.
            /// </summary>
     /*       internal void Activate()
            {
                foreach (var tracker in trackers)
                {
                    if (tracker.type == TrackerType.VRPN)
                    {
                        // register VRPN tracker with VRPN system
                        VRPNTrackerData data = tracker.data as VRPNTrackerData;
                        var device = VRPN.GetDevice(data.address);
                        if (device == null)
                            device = VRPN.AddOrGetDevice(data.address);
                    }
                }

                OnPlatformActivated();
            }*/

            /// <summary>
            /// Called when the Platform has been activated.
            /// </summary>
       /*     public void OnPlatformActivated()
            {
                foreach (Display display in displays) display.OnPlatformActivated();
                foreach (Node node in nodes) node.OnPlatformActivated();
                foreach (TrackerConfig tracker in trackers) tracker.OnPlatformActivated();
                if (stereoConfig != null) stereoConfig.OnPlatformActivated();
                if (clusterConfig != null) clusterConfig.OnPlatformActivated();
            }*/

            /// <summary>
            /// Query if there is a current platform and its type matches, if its type was set.
            /// </summary>
            /// <param name="type">The platform type to query for.</param>
            /// <returns>Returns true if there is a current active platform and its type matches, false otherwise.</returns>
       /*     public static bool IsActiveType(PlatformType type)
            {
                return current != null && current.type == type;
            }*/

       /*     public Mesh GenerateMeshForDisplays()
            {
                List<Vector3> vertices = new List<Vector3>();
                List<int> indices = new List<int>();
                List<Vector2> uvs = new List<Vector2>();

                foreach (var d in displays)
                {
                    Mesh subMesh = d.GenerateMeshForDisplay();

                    if (subMesh == null) continue;

                    int offset = vertices.Count;
                    vertices.AddRange(subMesh.vertices);
                    uvs.AddRange(subMesh.uv);
                    int[] ind = subMesh.GetIndices(0);
                    foreach (int i in ind)
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
            }*/
        }
    }
}