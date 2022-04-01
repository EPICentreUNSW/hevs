using HEVS.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Display class for a standard on-axis flat-panel display, typical of a desktop computer.
    /// Options are read from the JSON configuration and then used to setup a display rig.
    /// </summary>
    [HEVS.CustomDisplay("standard")]
    public class StandardDisplay : Display
    {
        /// <summary>
        /// Create a standard display from a specified display config.
        /// </summary>
        /// <param name="config">The config to create the display from.</param>
        public StandardDisplay(Config.Display config) : base(config)
        {

        }

        /// <summary>
        /// Creates a cloned copy of this display.
        /// </summary>
        /// <returns>Returns the cloned copy of this display.</returns>
        public override Display Clone()
        {
            return new StandardDisplay(config);
        }

        /// <summary>
        /// Setup this display from JSON data.
        /// Note: StandardDisplays have no options so this method does nothing.
        /// </summary>
        /// <param name="json">The SimpleJSON JSON data used to confgiure this display.</param>
        /// <returns>Returns true if the JSON data was successfully parsed, and false otherwise.</returns>
        public bool Parse(SimpleJSON.JSONNode json) { return true; }

        /// <summary>
        /// Sets up the Unity scene to use this display.
        /// </summary>
        public override void ConfigureDisplayForScene()
        {
            captureCameras.Clear();
            outputCameras.Clear();

            UnityEngine.Camera leftCamera = null, rightCamera = null, singleCamera = null;

            if (requiresDualCameras)
            {
                // capture cameras
                leftCamera = Camera.ConfigureCaptureCamera(gameObject, this, 2, StereoTargetEyeMask.Left);
                captureCameras.Add(leftCamera);

                rightCamera = Camera.ConfigureCaptureCamera(gameObject, this, 1, StereoTargetEyeMask.Right);
                captureCameras.Add(rightCamera);
            }
            else
            {
                // capture camera
                singleCamera = gameObject.GetOrAddComponent<UnityEngine.Camera>();
                captureCameras.Add(singleCamera);

                // copy over camera settings from original camera
                if (singleCamera != Camera.main)
                    singleCamera.CopyFrom(Camera.main);

                if (config.stereoMode == StereoMode.LeftOnly)
                    Camera.ConfigureCaptureCamera(singleCamera, this, StereoTargetEyeMask.Left);
                else if (config.stereoMode == StereoMode.RightOnly)
                    Camera.ConfigureCaptureCamera(singleCamera, this, StereoTargetEyeMask.Right);
                else
                    Camera.ConfigureCaptureCamera(singleCamera, this, StereoTargetEyeMask.Both);
            }

            outputCameras.AddRange(Camera.ConfigureOutputCameras(this, singleCamera ? singleCamera : leftCamera, rightCamera));
        }

        /// <summary>
        /// Set the clear flag for all cameras.
        /// </summary>
        /// <param name="flags">CameraClearFlags to set.</param>
        public override void SetClearFlags(CameraClearFlags flags)
        {
            foreach (var camera in captureCameras)
                camera.clearFlags = flags;
        }

        /// <summary>
        /// Set the clear colour for all cameras.
        /// </summary>
        /// <param name="colour">The colour to set.</param>
        public override void SetBackgroundColour(Color colour)
        {
            foreach (var camera in captureCameras)
                camera.backgroundColor = colour;
        }

        /// <summary>
        /// Cast a world-space ray into this display and return if there is an 
        /// intersection, along with distance to the intersection and the 
        /// relative 2D intersection point on the display. Only applies to displays 
        /// that exist within world-space.
        /// </summary>
        /// <param name="ray">The world-space ray.</param>
        /// <param name="distance">The returned distance to the intersection, or 0 if no intersection.</param>
        /// <param name="hitPoint2D">The returned 2D relative display-space intersection point.</param>
        /// <returns>Returns true if the world-space ray intersects the display, otherwise it returns false.</returns>
        public override bool Raycast(Ray ray, out float distance, out Vector2 hitPoint2D)
        {
            distance = 0;
            hitPoint2D = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Get a world-space ray from the display via a 2D display-space point.
        /// </summary>
        /// <param name="displaySpacePoint">The 2D display-space point to get the ray from, which is in [0,1] range for each axis.</param>
        /// <returns>Returns the world-space ray that passes through the display-space point.</returns>
        public override Ray ViewportPointToRay(Vector2 displaySpacePoint)
        {
            return primaryCaptureCamera.ViewportPointToRay(displaySpacePoint);
        }

        /// <summary>
        /// A method to draw the display's Gizmo within the editor.
        /// </summary>
        /// <param name="config">The config data to use for drawing a gizmo of this type.</param>
        public static void DrawGizmo(Config.Display config)
        {

        }

        /// <summary>
        /// Gather the geometry that represents this display.
        /// Note: No geometry is created for standard displays!
        /// </summary>
        /// <param name="vertices">The vertices that represent the display's mesh.</param>
        /// <param name="indices">The indices used to define the display's mesh topography.</param>
        /// <param name="uvs">The texture coordinates to apply across the display's mesh.</param>
        public override void GatherDisplayGeometry(List<Vector3> vertices, List<int> indices, List<Vector2> uvs)
        {
            vertices.Clear();
            indices.Clear();
            uvs.Clear();
        }
    }
}