using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HEVS
{
    public partial class Config {
        /// <summary>
        /// The HEVS universal display config object which contains the display-specific data along with common settings.
        /// </summary>
        public class Display : IConfigObject
        {
            /// <summary>
            /// The ID of the display.
            /// </summary>
            public string id { get; private set; }

            /// <summary>
            /// The type of this display.
            /// </summary>
            public string type { get; private set; } = "standard";

            /// <summary>
            /// Access to the display's viewport config, or null if no custom viewport is set.
            /// </summary>
            public Viewport viewport { get { return _viewport; } }
            Viewport _viewport;

            /// <summary>
            /// A 3D integer-based "index" for the display. This is entirely user-defined and can be used to identifying displays within a grid layout.
            /// </summary>
            public Vector3Int index { get; private set; } = Vector3Int.zero;

            /// <summary>
            /// The layermask for the display's cameras.
            /// </summary>
            public LayerMask layerMask { get; private set; } = ~0;

            /// <summary>
            /// An optional transform that is applied to the display's cameras and GameObjects when it is initialised.
            /// </summary>
            public Transform transform { get { return _transform; } }
            Transform _transform = Transform.identity;

            /// <summary>
            /// The Field-of-View for the display.
            /// </summary>
            public float fov { get; private set; } = 60;

            /// <summary>
            /// The aspect scale for the display. This is useful for displays that use a single projector/screen for multiple displays (or stereoscopic) and need to adjust the aspect to combat squashed aspects.
            /// </summary>
            public float aspectScale { get; private set; } = 1;

            #region Stereo Config Wrappers
            /// <summary>
            /// The stereo mode that this display uses.
            /// Will use the data from the StereoConfig, or an overridden value.
            /// </summary>
            public StereoMode stereoMode { get { return customStereoMode.HasValue ? customStereoMode.Value : stereoConfig != null ? stereoConfig.mode : StereoMode.Mono; } }

            /// <summary>
            /// THe stereo alignment for this screen. Either Screen-aligned or Camera-aligned.
            /// Will use the data from the StereoConfig, or an overridden value.
            /// </summary>
            public StereoAlignment stereoAlignment { get { return customStereoAlignment.HasValue ? customStereoAlignment.Value : stereoConfig != null ? stereoConfig.alignment : StereoAlignment.Screen; } }

            /// <summary>
            /// The eye separation for this display's cameras.
            /// Will use the data from the StereoConfig, or an overridden value.
            /// </summary>
            public float eyeSeparation { get { return customEyeSeparation.HasValue ? customEyeSeparation.Value : stereoConfig != null ? stereoConfig.eyeSeparation : 0.065f; } }

            /// <summary>
            /// Does this display require dual cameras for stereo?
            /// </summary>
            public bool requiresDualCameras { get { return Stereo.RequiresDualCameras(stereoMode); } }

            /// <summary>
            /// Are the stereo eyes swapped?
            /// </summary>
            public bool swapEyes { get { return customSwapEyes.HasValue ? customSwapEyes.Value : stereoConfig == null ? false : stereoConfig.swapEyes; } }
            #endregion

            /// <summary>
            /// The current platform's StereoConfig object.
            /// </summary>
            public Stereo stereoConfig { get; private set; }

            /// <summary>
            /// The display adapter (connected output monitor/projector) that this display outputs to.
            /// </summary>
            public int monitor { get; private set; } = 0;

            /// <summary>
            /// The near plane distance that will be used for the cameras this display requires. Default = -1 (use the values from the Unity scene instead)
            /// </summary>
            public float nearClip { get; private set; } = -1;

            /// <summary>
            /// The far plane distance that will be used for the cameras this display requires. Default = -1 (use the values from the Unity scene instead)
            /// </summary>
            public float farClip { get; private set; } = -1;
            /// <summary>
            /// When using custom warp you can specify the location of the warp data that the projectors will use.
            /// </summary>
            public string warpPath { get; private set; }

            /// <summary>
            /// When using custom projector blending you can specify the location of the blend information that the projectors will use.
            /// </summary>
            public string blendPath { get; private set; }

            // INTERNAL
            bool? customSwapEyes;
            StereoMode? customStereoMode;
            StereoAlignment? customStereoAlignment;
            float? customEyeSeparation;

            /// <summary>
            /// Access to this display's SimpleJSON JSON data.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// Creates a standard display with optional StereoConfig data.
            /// </summary>
            /// <param name="id">The ID for the display.</param>
            /// <param name="stereoConfig">Optional StereoConfig data.</param>
            public Display(string id, Stereo stereoConfig = null)
            {
                this.json = SimpleJSON.JSON.Parse("{}");
                this.id = id;
                this.stereoConfig = stereoConfig;
            }

            /// <summary>
            /// Parses the JSON config for a display and initialises it with optional StereoConfig. Has access to the current platform's viewports.
            /// </summary>
            /// <param name="json">The SimpleJSON JSON data from the config.</param>
            /// <param name="displays">A list of the Platform's previously defined displays.</param>
            /// <param name="stereoConfig">Optional StereoConfig data.</param>
            /// <param name="viewports">A list of the current platform's viewports.</param>
            /// <param name="transforms">A list of the Platform's named transforms.</param>
            /// <returns>Returns true if the display was successfully set up, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json, Dictionary<string, Display> displays, Stereo stereoConfig, Dictionary<string, Viewport> viewports, Dictionary<string, Transform> transforms)
            {
                if (json.Keys.Contains("inherit"))
                {
                    string base_id = json["inherit"];
                    Display baseConfig;
                    if (displays.TryGetValue(base_id, out baseConfig))
                        Parse(baseConfig.json);
                    else
                    {
                        Debug.LogError("HEVS: No inherited display [" + base_id + "] previously defined for display [" + id + "]!");
                        return false;
                    }
                }

                if (!Parse(json))
                    return false;

                this.stereoConfig = stereoConfig;

                if (json.Keys.Contains("viewport"))
                {
                    if (!viewports.TryGetValue(json["viewport"], out _viewport))
                    {
                        Debug.LogError("HEVS: Failed to find viewport [" + json["viewport"].Value + "] for display [" + id + "]");
                        return false;
                    }
                }

                if (json.Keys.Contains("transform"))
                {
                    // is it a string? if so, find transform in Platform's list
                    if (json["transform"].Tag == SimpleJSON.JSONNodeType.String)
                    {
                        if (!transforms.TryGetValue(json["transform"], out _transform))
                        {
                            Debug.LogError("HEVS: Failed to find transform [" + json["transform"].Value + "] for display [" + id + "]");
                            return false;
                        }
                    }
                    else
                    {
                        // else parse the transform
                        if (!transform.Parse(json["transform"]))
                        {
                            Debug.LogError("HEVS: Failed to parse transform for display [" + id + "]");
                            return false;
                        }
                    }
                }

                return true;
            }

            /// <summary>
            /// Parses the JSON config for a display.
            /// </summary>
            /// <param name="json">The SimpleJSON JSON data from the config.</param>
            /// <returns>Returns true if the display was successfully set up, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                this.json = json;

                if (json.Keys.Contains("stereo_mode"))
                    customStereoMode = (StereoMode)Enum.Parse(typeof(StereoMode), json["stereo_mode"], true);

                if (json.Keys.Contains("eye_separation"))
                    customEyeSeparation = json["eye_separation"].AsFloat;

                if (json.Keys.Contains("transform"))
                    transform.Parse(json["transform"]);

                if (json.Keys.Contains("fov"))
                    fov = json["fov"].AsFloat;

                if (json.Keys.Contains("near"))
                    nearClip = json["near"].AsFloat;

                if (json.Keys.Contains("far"))
                    farClip = json["far"].AsFloat;

                if (json.Keys.Contains("warp_path"))
                {
                    warpPath = json["warp_path"];
                    if (warpPath.StartsWith("StreamingAssets"))
                        warpPath = warpPath.Replace("StreamingAssets", UnityEngine.Application.streamingAssetsPath);
                }

                if (json.Keys.Contains("blend_path"))
                {
                    blendPath = json["blend_path"];
                    if (blendPath.StartsWith("StreamingAssets"))
                        blendPath = blendPath.Replace("StreamingAssets", UnityEngine.Application.streamingAssetsPath);
                }

                if (json.Keys.Contains("aspect_scale"))
                    aspectScale = json["aspect_scale"].AsFloat;

                if (json.Keys.Contains("swap_eyes"))
                    customSwapEyes = json["swap_eyes"].AsBool;

                if (json.Keys.Contains("stereo_alignment"))
                    customStereoAlignment = (StereoAlignment)Enum.Parse(typeof(StereoAlignment), json["stereo_alignment"], true);

                if (json.Keys.Contains("index"))
                {
                    SimpleJSON.JSONArray data = json["index"].AsArray;
                    if (data.Count == 3)
                        index = new Vector3Int(data[0].AsInt, data[1].AsInt, data[2].AsInt);
                    else if (data.Count == 2)
                        index = new Vector3Int(data[0].AsInt, data[1].AsInt, 0);
                    else if (data.Count == 1)
                        index = new Vector3Int(data[0].AsInt, 0, 0);
                    else
                    {
                        Debug.LogError("HEVS: Requested display [" + id + "] has an invalid index option!");
                    }
                }

                if (json.Keys.Contains("layers"))
                {
                    SimpleJSON.JSONArray layers = json["layers"].AsArray;
                    if (layers != null)
                    {
                        List<string> layerNames = new List<string>();
                        foreach (var layer in layers.Children)
                            layerNames.Add(layer.Value);
                        layerMask = LayerMask.GetMask(layerNames.ToArray());
                    }
                }

                if (json.Keys.Contains("cull_layers"))
                {
                    SimpleJSON.JSONArray layers = json["cull_layers"].AsArray;
                    if (layers != null)
                    {
                        List<string> layerNames = new List<string>();
                        foreach (var layer in layers.Children)
                            layerNames.Add(layer.Value);

                        // remove these from the layer mask
                        layerMask = ~LayerMask.GetMask(layerNames.ToArray());
                    }
                }

                if (json.Keys.Contains("monitor"))
                    monitor = json["monitor"].AsInt;

                // get display type
                if (json.Keys.Contains("type"))
                    type = json["type"].Value.ToLower();

                return true;
            }
        }
    }
}