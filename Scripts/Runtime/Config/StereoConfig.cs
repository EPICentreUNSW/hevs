using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// An enumeration of different stereoscopic implementations available to HEVS.
    /// </summary>
    public enum StereoMode
    {
        /// <summary>
        /// A standard monoscopic mode outputting into the entire viewport.
        /// </summary>
        Mono,
        /// <summary>
        /// Only the left eye's content should be displayed to the entire viewport.
        /// </summary>
        LeftOnly,
        /// <summary>
        /// Only the right eye's content should be displayed to the entire viewport.
        /// </summary>
        RightOnly,
        /// <summary>
        /// 3D stereoscopic where the left eye's content will be displayed on the left half of the viewport, 
        /// and the right eye's content will be displayed on the right half of the viewport.
        /// </summary>
        SideBySide,
        /// <summary>
        /// 3D stereoscopic where the left eye's content will be displayed on the top half of the viewport, 
        /// and the right eye's content will be displayed on the bottom half of the viewport.
        /// </summary>
        TopBottom,
        /// <summary>
        /// 3D stereoscopic where the left eye's content will be displayed within the entire viewport for one
        /// frame, and then the right eye's content will be displayed within the entire viewport in the subsequent frame, 
        /// and then the process will repeat. This mode is not recommended.
        /// </summary>
        Sequential,
        /// <summary>
        /// 3D stereoscopic using hardware Quadbuffering for specific compatible hardware, such as NVidia Quadro.
        /// </summary>
        QuadBuffered,
        /// <summary>
        /// A monoscopic mode that will render the same view into the top half of the viewport and into the bottom half 
        /// of the viewport. This mode is useful for platforms that output TopBottom/BottomTop stereoscopic but want to 
        /// output monoscopic without having to change hardware configurations.
        /// </summary>
        TopBottomMono,
        /// <summary>
        /// A monoscopic mode that will render the same view into the left half of the viewport and into the right half 
        /// of the viewport. This mode is useful for platforms that output LeftRight/RightLeft stereoscopic but want to 
        /// output monoscopic without having to change hardware configurations.
        /// </summary>
        SideBySideMono,
        /// <summary>
        /// A mode that will render the left eye's content into the top half of the viewport and into the bottom half 
        /// of the viewport. This mode is useful for platforms that output TopBottom/BottomTop stereoscopic but want to 
        /// output just the left eye without having to change hardware configurations.
        /// </summary>
        TopBottomLeftOnly,
        /// <summary>
        /// A mode that will render the left eye's content into the left half of the viewport and into the right half 
        /// of the viewport. This mode is useful for platforms that output LeftRight/RightLeft stereoscopic but want to 
        /// output just the left eye without having to change hardware configurations.
        /// </summary>
        SideBySideLeftOnly,
        /// <summary>
        /// A mode that will render the right eye's content into the top half of the viewport and into the bottom half 
        /// of the viewport. This mode is useful for platforms that output TopBottom/BottomTop stereoscopic but want to 
        /// output just the right eye without having to change hardware configurations.
        /// </summary>
        TopBottomRightOnly,
        /// <summary>
        /// A mode that will render the right eye's content into the left half of the viewport and into the right half 
        /// of the viewport. This mode is useful for platforms that output LeftRight/RightLeft stereoscopic but want to 
        /// output just the right eye without having to change hardware configurations.
        /// </summary>
        SideBySideRightOnly,
        /// <summary>
        /// Anaglyph for red-green glasses.
        /// </summary>
        RedGreen,
        /// <summary>
        /// Anaglyph for red-blue glasses.
        /// </summary>
        RedBlue,
        /// <summary>
        /// Anaglyph for red-cyan glasses.
        /// </summary>
        RedCyan,
    }

    /// <summary>
    /// An enumeration specifying the alignment of stereoscopic cameras.
    /// </summary>
    public enum StereoAlignment
    {
        /// <summary>
        /// Screen-aligned means that the left and right eye cameras are positioned 
        /// based on the display's right and up axes. This primarily applies of Off-Axis 
        /// displays where you want the 3D stereoscopic effect to work for multiple viewers.
        /// </summary>
        Screen,
        /// <summary>
        /// Camera-aligned means hat the left and right eye cameras are positioned 
        /// relative to the MainCamera's orientation.
        /// </summary>
        Camera
    }

    public partial class Config
    {
        /// <summary>
        /// A HEVS Stereoscopic config object that defines the stereoscopic options for a platform.
        /// Most options can be overridden on a per-display basis.
        /// </summary>
        public class Stereo : IConfigObject
        {
            /// <summary>
            /// Access to the current stereo config, or null if not being used.
            /// </summary>
            public static Stereo current { get { return HEVS.Core.activePlatform != null ? HEVS.Core.activePlatform.stereo : null; } }

            /// <summary>
            /// The stereoscopic mode to use for this platform. 
            /// Default is Mono.
            /// </summary>
            public StereoMode mode { get; private set; } = StereoMode.Mono;

            /// <summary>
            /// The stereo camera alignment to use for this platform. 
            /// The default is Camera alignment.
            /// </summary>
            public StereoAlignment alignment { get; private set; } = StereoAlignment.Camera;

            /// <summary>
            /// The total eye separation to use for stereoscopic effects. Default is 0.065 meters (65mm).
            /// </summary>
            public float eyeSeparation { get; private set; } = 0.065f;

            /// <summary>
            /// A flag used to instruct Unity to either use a single Camera that can output to both eyes, 
            /// or to set up individual cameras for each eye. Each method has limitations; shadows may not work 
            /// correctly when using a single camera, and StereoMode.QuadBuffered may not work correctly when 
            /// using separate cameras for each eye if also using additional cameras with RenderTextures. 
            /// The default is to use a single camera.
            /// </summary>
        //    public bool separateEyes { get; private set; } = false;

            /// <summary>
            /// Specifies if the left and right eyes be swapped.
            /// </summary>
            public bool swapEyes { get; private set; } = false;

            /// <summary>
            /// Optional tracker to use. If there is a valid tracker then the camera will have a Tracker attached to it.
            /// </summary>
            public Tracker tracker { get { return _tracker; } }
            Tracker _tracker;

            /// <summary>
            /// The SimpleJSON JSON data used to configure this object.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// Parse JSON data to initialise this config.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <returns>Returns true if the data is successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                this.json = json;

                if (json.Keys.Contains("mode"))
                    mode = (StereoMode)Enum.Parse(typeof(StereoMode), json["mode"], true);

                if (json.Keys.Contains("separation"))
                    eyeSeparation = json["separation"].AsFloat;

                if (json.Keys.Contains("alignment"))
                    alignment = (StereoAlignment)Enum.Parse(typeof(StereoAlignment), json["alignment"], true);

          //      if (json.Keys.Contains("separate_eyes"))
         //           separateEyes = json["separate_eyes"].AsBool;

                if (json.Keys.Contains("swap_eyes"))
                    swapEyes = json["swap_eyes"].AsBool;

                return true;
            }

            /// <summary>
            /// Parse JSON data to initialise this config, with access to the current trackers in case of using trackign.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <param name="platformTrackers">A list of the current platform's trackers.</param>
            /// <returns>Returns true if the data is successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json, Dictionary<string, Tracker> platformTrackers)
            {
                if (!Parse(json))
                    return false;

                if (json.Keys.Contains("tracker"))
                {
                    string trackerName = json["tracker"];
                    if (!platformTrackers.TryGetValue(trackerName, out _tracker))
                    {
                        Debug.LogError("HEVS: Unable to find tracker [" + trackerName + "] for automatic stereoscopic tracking!");
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Utility flag for checking if the stereo mode is a "side-by-side" setup.
            /// </summary>
            public bool isSideBySideMode
            {
                get
                {
                    return mode == StereoMode.SideBySide ||
                     mode == StereoMode.SideBySideLeftOnly ||
                     mode == StereoMode.SideBySideRightOnly ||
                     mode == StereoMode.SideBySideMono;
                }
            }

            /// <summary>
            /// Utility flag for checking if the stereo mode is a "top-bottom" setup.
            /// </summary>
            public bool isTopBottomMode
            {
                get
                {
                    return mode == StereoMode.TopBottom ||
                     mode == StereoMode.TopBottomLeftOnly ||
                     mode == StereoMode.TopBottomRightOnly ||
                     mode == StereoMode.TopBottomMono;
                }
            }

            /// <summary>
            /// Utility flag for checking if the stereo mode using anaglyph rendering.
            /// </summary>
            public bool isAnaglyphMode
            {
                get
                {
                    return mode == StereoMode.RedGreen ||
                     mode == StereoMode.RedBlue ||
                     mode == StereoMode.RedCyan;
                }
            }

            /// <summary>
            /// Utility flag for checking if the stereo mode requires two cameras to capture the scene.
            /// </summary>
            public bool requiresDualCameras
            {
                get
                {
                    return mode == StereoMode.SideBySide ||
                     mode == StereoMode.SideBySideLeftOnly ||
                     mode == StereoMode.SideBySideRightOnly ||
                     mode == StereoMode.SideBySideMono ||
                     mode == StereoMode.TopBottom ||
                     mode == StereoMode.TopBottomLeftOnly ||
                     mode == StereoMode.TopBottomRightOnly ||
                     mode == StereoMode.TopBottomMono ||
                     mode == StereoMode.RedGreen ||
                     mode == StereoMode.RedBlue ||
                     mode == StereoMode.RedCyan ||
                     mode == StereoMode.QuadBuffered;
                }
            }

            /// <summary>
            /// Utility method for checking if a particular stereo mode requires two cameras for capturing the scene.
            /// </summary>
            /// <param name="mode">The StereoMode to check.</param>
            /// <returns>Returns true if the specified StereoMode requires two cameras to capture the scene.</returns>
            public static bool RequiresDualCameras(StereoMode mode)
            {
                return mode == StereoMode.SideBySide ||
                    mode == StereoMode.SideBySideLeftOnly ||
                    mode == StereoMode.SideBySideRightOnly ||
                    mode == StereoMode.SideBySideMono ||
                    mode == StereoMode.TopBottom ||
                    mode == StereoMode.TopBottomLeftOnly ||
                    mode == StereoMode.TopBottomRightOnly ||
                    mode == StereoMode.TopBottomMono ||
                    mode == StereoMode.RedGreen ||
                    mode == StereoMode.RedBlue ||
                    mode == StereoMode.RedCyan ||
                    mode == StereoMode.QuadBuffered;
            }
        }
    }
}