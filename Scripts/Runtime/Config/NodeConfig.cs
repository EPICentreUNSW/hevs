using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using HEVS.Extensions;

namespace HEVS
{
    public partial class Config
    {
        /// <summary>
        /// Th HEVS Node configuration object, which defines all options for a node runnings HEVS within a platform.
        /// </summary>
        public class Node : IConfigObject
        {
            /// <summary>
            /// Creates a default NodeConfig using the current system resolution.
            /// </summary>
            /// <param name="platform">The platform this node belongs to.</param>
            /// <param name="id">The ID to assign to the node.</param>
            public Node(Platform platform, string id) { this.platform = platform; this.id = id; resolution = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height); }

            /// <summary>
            /// The platform this node belongs to.
            /// </summary>
            public Platform platform { get; private set; }
            /// <summary>
            /// The ID of the node.
            /// </summary>
            public string id { get; private set; }
            /// <summary>
            /// The IP address of the node, or its hostname.
            /// </summary>
            public string address { get; private set; } = "127.0.0.1";
            /// <summary>
            /// Flag to specify if the node is frame-locked in hardware.
            /// Requires hardware with hardware-frame-lock support, such as NVidia Quadro GPUs with genlock support.
            /// </summary>
            public bool hardwareSynced { get { return customHardwareSync.HasValue ? customHardwareSync.Value : platform.cluster != null && platform.cluster.framelockMode == FrameLockMode.Hardware; } }
            bool? customHardwareSync;
            /// <summary>
            /// An optional 3D index of the node. This is not used internally by HEVS and is 
            /// provided simply as a way for a user to arrange their nodes within a grid.
            /// </summary>
            public Vector3Int index { get; private set; } = Vector3Int.zero;
            /// <summary>
            /// The screen resolution of this node.
            /// </summary>
            public Vector2Int resolution { get; private set; }
            /// <summary>
            /// The fullscreen setting for this node.
            /// </summary>
            public FullScreenMode screenMode { get; private set; } = FullScreenMode.FullScreenWindow;
            /// <summary>
            /// A list of all displays that this node uses, which are also stored within the node's platform.
            /// </summary>
            public List<Display> displays { get; private set; } = new List<Display>();
            /// <summary>
            /// Whether Screen Space UI should be scaled across the cluster's displays. 
            /// </summary>
            public bool applyScreenSpaceScaling { get; private set; } = true;
            /// <summary>
            /// Is this node the master node in its platform?
            /// </summary>
            public bool isMaster { get { return platform.masterNode == null ? true : this == platform.masterNode; } }
            /// <summary>
            /// Is this node a client within its platform?
            /// </summary>
            public bool isClient { get { return !isMaster; } }

            /// <summary>
            /// The SimpleJSON JSON data used to initialise this node.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// Parse a JSON config to initialise this node, with no displays.
            /// </summary>
            /// <param name="json">The SimpleJSON JSON data used to initialise this node.</param>
            /// <returns>Returns true if the JSON was successfully parsed, and false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                this.json = json;

                if (json.Keys.Contains("address"))
                    address = json["address"];

                if (json.Keys.Contains("hardware_synced"))
                    customHardwareSync = json["hardware_synced"].AsBool;

                if (json.Keys.Contains("resolution"))
                    resolution = new Vector2Int(json["resolution"][0].AsInt, json["resolution"][1].AsInt);

                if (json.Keys.Contains("index"))
                {
                    // dimensions?
                    SimpleJSON.JSONArray data = json["index"].AsArray;
                    if (data.Count == 3)
                        index = new Vector3Int(data[0].AsInt, data[1].AsInt, data[2].AsInt);
                    else if (data.Count == 2)
                        index = new Vector3Int(data[0].AsInt, data[1].AsInt, 0);
                    else if (data.Count == 1)
                        index = new Vector3Int(data[0].AsInt, 0, 0);
                    else
                    {
                        Debug.LogError("HEVS: Requested node [" + id + "] has an invalid index option!");
                        return false;
                    }
                }

                bool exclusiveFullscreen = screenMode == FullScreenMode.ExclusiveFullScreen;

                bool hasNewExclusiveFlag = false;
                if (json.Keys.Contains("fullscreen_exclusive"))
                {
                    hasNewExclusiveFlag = true;
                    exclusiveFullscreen = json["fullscreen_exclusive"].AsBool;
                }

                if (json.Keys.Contains("screenspace_scaling"))
                {
                    applyScreenSpaceScaling = json["screenspace_scaling"].AsBool;
                }

                if (json.Keys.Contains("fullscreen"))
                {
                    screenMode = json["fullscreen"].AsBool ?
                                (exclusiveFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.FullScreenWindow) : FullScreenMode.Windowed;
                }
                else if (hasNewExclusiveFlag)
                {
                    if (exclusiveFullscreen && screenMode == FullScreenMode.FullScreenWindow)
                        screenMode = FullScreenMode.ExclusiveFullScreen;
                    else if (!exclusiveFullscreen && screenMode == FullScreenMode.ExclusiveFullScreen)
                        screenMode = FullScreenMode.FullScreenWindow;
                }
                return true;
            }

            /// <summary>
            /// Parse a JSON config to initialise this node, with access to the platform's displays and stereoscopic data.
            /// If the node does not contain any displays then a default display is created.
            /// </summary>
            /// <param name="json">The SimpleJSON JSON data used to initialise this node.</param>
            /// <param name="platformDisplays">A list of the platform's loaded displays.</param>
            /// <param name="platformStereo">The platforms stereoscopic settings.</param>
            /// <returns>Returns true if the JSON was successfully parsed, and false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json, Dictionary<string, Display> platformDisplays, Stereo platformStereo)
            {
                if (!Parse(json))
                    return false;

                string displayKey = string.Empty;
                if (json.Keys.Contains("display"))
                    displayKey = "display";
                else if (json.Keys.Contains("displays"))
                    displayKey = "displays";

                if (!string.IsNullOrWhiteSpace(displayKey))
                {
                    displays.Clear();

                    SimpleJSON.JSONArray displayIds = json[displayKey].AsArray;
                    if (displayIds != null)
                    {
                        foreach (SimpleJSON.JSONNode displayId in displayIds)
                        {
                            string did = displayId;

                            // ensure it exists
                            Display display;
                            if (platformDisplays.TryGetValue(did, out display))
                            {
                                // already added to node?
                                if (!displays.Contains(display))
                                    displays.Add(display);
                            }
                            else
                            {
                                Debug.LogError("HEVS: Requested display [" + did + "] does not exist!");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        string did = json[displayKey];

                        Display display;
                        if (platformDisplays.TryGetValue(did, out display))
                        {
                            // already added to node?
                            if (!displays.Contains(display))
                                displays.Add(display);
                        }
                        else
                        {
                            Debug.LogError("HEVS: Requested display [" + did + "] does not exist!");
                            return false;
                        }
                    }
                }
                else
                {
                    // no specified display, create a standard display
                    Display newDisplay = new Display("disp_" + id, platformStereo);
                    displays.Add(newDisplay);
                    platformDisplays.Add(newDisplay.id, newDisplay);
                }

                return true;
            }
        }
    }
}