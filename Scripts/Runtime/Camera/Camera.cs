using UnityEngine;
using System.Collections.Generic;

using HEVS.Extensions;

namespace HEVS
{
    /// <summary>
    /// A helper for accessing the stored main camera.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Access to the original MainCamera, which will be disabled for most platforms but will have all display cameras 
        /// attached to it as children in the scene hierarchy.
        /// </summary>
        public static UnityEngine.Camera main { get { return Core.mainCamera == null ? UnityEngine.Camera.main : Core.mainCamera; } }

        /// <summary>
        /// Cameras created by HEVS to represents the current node's displays.
        /// </summary>
        public static List<UnityEngine.Camera> displayCameras = new List<UnityEngine.Camera>();

        #region Capture Cameras
        internal static UnityEngine.Camera ConfigureCaptureCamera(GameObject parent, Display display, int depth, StereoTargetEyeMask eye)
        {
            GameObject left = new GameObject(display.id + (eye == StereoTargetEyeMask.Left ? "_left" : eye == StereoTargetEyeMask.Right ? "_right" : ""));
            UnityEngine.Camera camera = left.AddComponent<UnityEngine.Camera>();

            // attach new objects to old
            left.transform.SetParent(parent.transform, false);

            // copy over camera settings from original camera
            camera.CopyFrom(Camera.main);

            camera.depth = Camera.main.depth - depth;

            // remove settings that can't be used
            camera.allowMSAA = false;
            camera.allowHDR = false;

            Camera.ConfigureCaptureCamera(camera, display, eye);

            return camera;
        }

        /// <summary>
        /// Configures a camera to capture the scene based on settings from a specified HEVS DisplayConfig read from a JSON configuration file.
        /// </summary>
        /// <param name="camera">The camera to configure.</param>
        /// <param name="display">The DisplayConfig to base the camera on.</param>
        /// <param name="eye">Which eye does the camera represent. Will reposition the camera based on the display's eye separation if required.</param>
        public static void ConfigureCaptureCamera(UnityEngine.Camera camera, Display display, StereoTargetEyeMask eye)
        {
            // copy over any camera extensions from the main camera
            if (main != camera)
                main.gameObject.CopyComponentsOfTypeTo<CameraExtension>(camera.gameObject);

            // offset for eye
            if (eye == StereoTargetEyeMask.Left)
                camera.transform.localPosition = Vector3.left * display.eyeSeparation * .5f;
            else if (eye == StereoTargetEyeMask.Right)
                camera.transform.localPosition = Vector3.right * display.eyeSeparation * .5f;            

            camera.fieldOfView = display.fov;

            if (eye == StereoTargetEyeMask.Both)
                camera.stereoSeparation = display.eyeSeparation;
            else
                camera.stereoSeparation = 0;

            camera.stereoTargetEye = eye;

            // layer mask
            camera.cullingMask = display.layerMask;

            // remove eye-specific cull mask
            if (eye == StereoTargetEyeMask.Left)
                camera.cullingMask &= ~(1 << LayerMask.NameToLayer("HEVSRightEyeOnly"));
            else if (eye == StereoTargetEyeMask.Right)
                camera.cullingMask &= ~(1 << LayerMask.NameToLayer("HEVSLeftEyeOnly"));

            // THIS MAY HAVE AN ISSUE WITH OFF-AXIS
            if (display.aspectScale != 1)
                camera.gameObject.AddComponent<AspectScaleExtension>().scale = display.aspectScale;
        }
        #endregion

        #region Output Cameras
        internal static UnityEngine.Camera[] ConfigureOutputCameras(Display display, UnityEngine.Camera left, UnityEngine.Camera right)
        {
            if (left && right)
            {
                Camera.ConfigureViewportForCamera(left, display, true);
                Camera.ConfigureViewportForCamera(right, display, false);
            
                // can the cameras render directly to the backbuffer, or require a camera?
                if (display.stereoMode == StereoMode.RedGreen ||
                    display.stereoMode == StereoMode.RedBlue ||
                    display.stereoMode == StereoMode.RedCyan)
                {
                    var singleCamera = left.transform.parent.gameObject.GetOrAddComponent<UnityEngine.Camera>();

                    if (singleCamera != Camera.main)
                        singleCamera.CopyFrom(Camera.main);

                    // disable rendering of the scene
                    singleCamera.cullingMask = 0;

                    // setup render targets
                    left.targetTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, 4);
                    left.targetTexture.Create();
                    right.targetTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, 4);
                    right.targetTexture.Create();

                    var ce = singleCamera.gameObject.GetOrAddComponent<HEVSCameraExtension>();
                    ce.display = display;
                    ce.SetAnaglyph(display.stereoMode, left.targetTexture, right.targetTexture);                    

                    Camera.ConfigureWarpAndBlend(display, singleCamera.gameObject);

                    if (Core.activeNode.hardwareSynced)
                        Graphics.SetupPluginRenderTarget(singleCamera);

                    singleCamera.SetAndActivateTargetDisplay(display.monitor);

                    return new UnityEngine.Camera[]{ singleCamera };
                }

                Camera.ConfigureWarpAndBlend(display, left.gameObject, right.gameObject);

                if (!Application.isEditor &&
                    (display.stereoMode == StereoMode.QuadBuffered ||
                    Core.activeNode.hardwareSynced))
                    Graphics.SetupPluginRenderTarget(left, right);

                left.SetAndActivateTargetDisplay(display.monitor);
                right.SetAndActivateTargetDisplay(display.monitor);

                return new UnityEngine.Camera[] { left, right };
            }
            else
            {
                Camera.ConfigureViewportForCamera(left, display);
                left.SetAndActivateTargetDisplay(display.monitor);

                Camera.ConfigureWarpAndBlend(display, left.gameObject);

                if (!Application.isEditor &&
                    Core.activeNode.hardwareSynced)
                    Graphics.SetupPluginRenderTarget(left);

                return new UnityEngine.Camera[] { left };
            }
        }

        /// <summary>
        /// Configures camera to output to a specified display and viewport.
        /// </summary>
        /// <param name="camera">The camera to configure.</param>
        /// <param name="display">The DisplayConfig to base the camera on.</param>
        /// <param name="leftEye">Is this camera meant for the left eye (default: true).</param>
        public static void ConfigureViewportForCamera(UnityEngine.Camera camera, Display display, bool leftEye = true)
        {
            // setup viewport
            Rect viewport = display.screenRect;

            leftEye = display.swapEyes ? !leftEye : leftEye;

            if (string.IsNullOrWhiteSpace(display.warpPath) &&
                string.IsNullOrWhiteSpace(display.blendPath))
            {
                // should the viewport be split?
                switch (display.stereoMode)
                {
                    case StereoMode.QuadBuffered:
                        {
                            // quad buffered splits all left-eyes onto the left half of the 'screen' (not viewport), and all right-eyes on the right half
                            if (leftEye)
                                camera.rect = new Rect(viewport.x * 0.5f, viewport.y, viewport.width / 2, viewport.height);
                            else
                                camera.rect = new Rect(viewport.x * 0.5f + 0.5f, viewport.y, viewport.width / 2, viewport.height);
                            break;
                        }
                    case StereoMode.SideBySide:
                    case StereoMode.SideBySideMono:
                    case StereoMode.SideBySideLeftOnly:
                    case StereoMode.SideBySideRightOnly:
                        {
                            if (leftEye)
                                camera.rect = new Rect(viewport.x, viewport.y, viewport.width / 2, viewport.height);
                            else
                                camera.rect = new Rect(viewport.x + viewport.width / 2, viewport.y, viewport.width / 2, viewport.height);
                            break;
                        }
                    case StereoMode.TopBottom:
                    case StereoMode.TopBottomMono:
                    case StereoMode.TopBottomLeftOnly:
                    case StereoMode.TopBottomRightOnly:
                        {
                            if (leftEye)
                                camera.rect = new Rect(viewport.x, viewport.y + viewport.height / 2, viewport.width, viewport.height / 2);
                            else
                                camera.rect = new Rect(viewport.x, viewport.y, viewport.width, viewport.height / 2);
                            break;
                        }
                    default:
                        {
                            camera.rect = new Rect(viewport.x, viewport.y, viewport.width, viewport.height);
                            break;
                        }
                }
            }
            else
                camera.rect = new Rect(viewport.x, viewport.y, viewport.width, viewport.height);
        }

        internal static void ConfigureWarpAndBlend(Display display, GameObject left, GameObject right = null)
        {
            if (!string.IsNullOrWhiteSpace(display.warpPath))
            {
                if (right == null)
                {
                    HEVSCameraExtension hevsCamera = left.GetOrAddComponent<HEVSCameraExtension>();
                    hevsCamera.display = display;
                    hevsCamera.warpTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
                    hevsCamera.warpTexture.wrapMode = TextureWrapMode.Clamp;
                    (hevsCamera.warpTexture as Texture2D).LoadFloatingpointTiff(display.warpPath);
                }
                else
                {
                    var ce = display.gameObject.GetOrAddComponent<HEVSCameraExtension>();
                    ce.display = display;

                    ce.warpTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
                    ce.warpTexture.wrapMode = TextureWrapMode.Clamp;
                    (ce.warpTexture as Texture2D).LoadFloatingpointTiff(display.warpPath);

                    // now need to have left and right render, either into separate textures, or into the same texture
                    UnityEngine.Camera cam1 = left.GetComponent<UnityEngine.Camera>();
                    UnityEngine.Camera cam2 = right.GetComponent<UnityEngine.Camera>();
                    ce.leftEye = cam1.targetTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, 4);
                    ce.rightEye = cam2.targetTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, 4);
                    cam1.targetTexture.Create();
                    cam2.targetTexture.Create();
                }
            }

            if (!string.IsNullOrWhiteSpace(display.blendPath))
            {
                if (right == null)
                {
                    HEVSCameraExtension hevsCamera = left.GetOrAddComponent<HEVSCameraExtension>();
                    hevsCamera.display = display;
                    hevsCamera.blendTexture = Utils.LoadTexture(display.blendPath);
                    hevsCamera.blendTexture.wrapMode = TextureWrapMode.Clamp;
                }
                else
                {
                    var ce = display.gameObject.GetOrAddComponent<HEVSCameraExtension>();
                    ce.display = display;
                    ce.blendTexture = Utils.LoadTexture(display.blendPath);
                    ce.blendTexture.wrapMode = TextureWrapMode.Clamp;

                    UnityEngine.Camera cam1 = left.GetComponent<UnityEngine.Camera>();
                    UnityEngine.Camera cam2 = right.GetComponent<UnityEngine.Camera>();

                    if (ce.leftEye == null)
                    {
                        ce.leftEye = cam1.targetTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, 4);
                        ce.rightEye = cam2.targetTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, 4);
                        cam1.targetTexture.Create();
                        cam2.targetTexture.Create();
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Set the camera clear flags for all of the active node's display cameras.
        /// </summary>
        public static CameraClearFlags clearFlags
        {
            set
            {
                // go through all HEVS cameras and set their clear flags
                foreach (var display in Core.activeDisplays)
                    display.SetClearFlags(value);
            }
        }

        /// <summary>
        /// Set the background clear colour for all of the active node's display cameras.
        /// </summary>
        public static Color backgroundColor
        {
            set
            {
                // go through all HEVS cameras and set their clear flags
                foreach (var display in Core.activeDisplays)
                    display.SetBackgroundColour(value);
            }
        }
    }

    /// <summary>
    /// The base MonoBehaviour used for HEVS Camera extensions.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(UnityEngine.Camera))]
    abstract public class CameraBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Access to the HEVS DisplayConfig used by this camera.
        /// </summary>
        public Display display;
    }
}
