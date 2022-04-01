using System;
using UnityEngine;
using UnityEngine.UI;

namespace HEVS
{
    /// <summary>
    /// Anaglyph formats supported by HEVS. 
    /// </summary>
    public enum AnaglyphType
    {
        /// <summary>
        /// Red-Green Anaglyph.
        /// </summary>
        RedGreen = 0,
        /// <summary>
        /// Red-Blue Anaglyph.
        /// </summary>
        RedBlue,
        /// <summary>
        /// Red-Cyan Anaglyph.
        /// </summary>
        RedCyan,
        /// <summary>
        /// No Anaglyph is being used.
        /// </summary>
        None
    }

    /// <summary>
    /// Object for storing textures captured by a fisheye camera rig to create a fisheye image.
    /// </summary>
    [Serializable]
    public class FisheyeTextures
    {
        /// <summary>
        /// Access to the left quadrant captured image.
        /// </summary>
        public Texture left;
        /// <summary>
        /// Access to the right quadrant captured image.
        /// </summary>
        public Texture right;
        /// <summary>
        /// Access to the above quadrant captured image.
        /// </summary>
        public Texture up;
        /// <summary>
        /// Access to the lower quadrant captured image.
        /// </summary>
        public Texture down;
    }

    /// <summary>
    /// A UnityEngine.Camera Mono Behaviour that implements support for anaglyph, domes, warping and blending, and blacklevel correction.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class HEVSCameraExtension : MonoBehaviour
    {
        Material overlayMaterial;

        /// <summary>
        /// Flag for checking if this extension is setup for stereoscopic rendering.
        /// </summary>
        public bool isStereo { get { return leftEye && rightEye; } }

        /// <summary>
        /// The stereoscopic left eye source image captured by other camera.
        /// </summary>
        public Texture leftEye;

        /// <summary>
        /// The stereoscopic right eye source image captured by other camera.
        /// </summary>
        public Texture rightEye;

        /// <summary>
        /// A warp texture to apply to the output image. Used for software keystoning.
        /// </summary>
        public Texture warpTexture;

        /// <summary>
        /// A blend texture used to blend this output image with other output images, typically when using overlapping projectors in a multi-output setup.
        /// </summary>
        public Texture blendTexture;

        /// <summary>
        /// A mask to apply to the output for defining regions to apply black-level correction to.
        /// </summary>
        public Texture blackLevelMask;

        /// <summary>
        /// The black-level correction to apply to this output.
        /// </summary>
        public float blackLevel = 0;

        /// <summary>
        /// The stereoscopic anaglyph mode to use for this output, if any.
        /// </summary>
        public AnaglyphType anaglyphType = AnaglyphType.None;

        FisheyeTextures fisheyeTextures = null;

        internal void SetFisheye(Texture left, Texture right, Texture up, Texture down)
        {
            fisheyeTextures = new FisheyeTextures();
            fisheyeTextures.left = left;
            fisheyeTextures.right = right;
            fisheyeTextures.up = up;
            fisheyeTextures.down = down;
        }

        const int STEREO_MODE_MONO = 0;
        const int STEREO_MODE_SIDEBYSIDE = 1;
        const int STEREO_MODE_TOPBOTTOM = 2;
        const int STEREO_MODE_ANAGLYPH = 3;
        const int STEREO_MODE_DOME = 4;

        /// <summary>
        /// The display that this extension belongs to.
        /// </summary>
        public Display display;

        /// <summary>
        /// A percentage value specifying texture border thickness.
        /// </summary>
        public float border = 0;

        /// <summary>
        /// Aspect ratio scale applied to width of the display.
        /// </summary>
        public float aspectScale = 1;

        /// <summary>
        /// The original source of the captured image to be output. In the case of stereoscopic or dome output, this value will be null.
        /// </summary>
        public RenderTexture sourceImage = null;

        internal void SetAnaglyph(StereoMode mode, Texture left, Texture right)
        {
            leftEye = left;
            rightEye = right;

            switch (mode)
            {
                case StereoMode.RedGreen: anaglyphType = AnaglyphType.RedGreen; break;
                case StereoMode.RedBlue: anaglyphType = AnaglyphType.RedBlue; break;
                case StereoMode.RedCyan: anaglyphType = AnaglyphType.RedCyan; break;
            }
        }

        void OnDestroy()
        {
            if (sourceImage)
                sourceImage.Release();
            if (leftEye && leftEye.GetType() == typeof(RenderTexture))
                (leftEye as RenderTexture).Release();
            if (rightEye && rightEye.GetType() == typeof(RenderTexture))
                (rightEye as RenderTexture).Release();
        }

        void Start()
        {
            SetupFullscreen();
        }

        void SetupFullscreen()
        {
            UnityEngine.Camera camera = null;
            GameObject cameraGO = null;

            // create a render target for this camera
            if (sourceImage ||
                fisheyeTextures != null || 
                isStereo)
            {
                // can hook up this camera to see the overlay
                cameraGO = gameObject;
                camera = GetComponent<UnityEngine.Camera>();
            }
            else
            {
                // this camera needs to render to a target that a new camera will display on the overlay
                GetComponent<UnityEngine.Camera>().targetTexture = sourceImage = new RenderTexture(Screen.width, Screen.height, 24);

                // create new output camera
                cameraGO = new GameObject(display.id + "_output");
                cameraGO.transform.SetParent(transform, false);
                camera = cameraGO.AddComponent<UnityEngine.Camera>();
            }

            // create new overlay object
            var canvasGO = new GameObject(display.id + "_output_canvas");
            canvasGO.transform.SetParent(cameraGO.transform, false);

            // create the camera-space canvas and setup layers
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.5f;
            canvas.gameObject.layer = LayerMask.NameToLayer("HEVSFullscreenOverlay");

            camera.orthographic = true;
            camera.orthographicSize = 1;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1;
            camera.useOcclusionCulling = false;
            camera.allowHDR = false;
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.cullingMask = LayerMask.GetMask("HEVSFullscreenOverlay");
            camera.enabled = true;

            // create the fullscreen quad and attach the correct material
            var img = canvasGO.AddComponent<RawImage>();
            img.material = overlayMaterial = new Material(Shader.Find("HEVS/CameraOverlay"));
        }

        void LateUpdate()
        {
            // using either alternate source or the camera's source
            overlayMaterial.SetTexture("_MainTex", sourceImage);

            if (fisheyeTextures != null)
            {
                overlayMaterial.SetTexture("_Left", fisheyeTextures.left);
                overlayMaterial.SetTexture("_Right", fisheyeTextures.right);
                overlayMaterial.SetTexture("_Top", fisheyeTextures.up);
                overlayMaterial.SetTexture("_Bottom", fisheyeTextures.down);
                overlayMaterial.SetFloat("_Border", border);
                overlayMaterial.SetFloat("_AspectScale", aspectScale);
                overlayMaterial.SetInt("_StereoMode", STEREO_MODE_DOME);
            }
            else if (anaglyphType != AnaglyphType.None)
            {
                overlayMaterial.SetTexture("_Left", leftEye);
                overlayMaterial.SetTexture("_Right", rightEye);

                // basic implementation for now, could be improved by tweaking modifiers
                switch (anaglyphType)
                {
                    case AnaglyphType.RedGreen:
                        {
                            overlayMaterial.SetVector("_LeftModifier", new Vector4(1, 0, 0, 0));
                            overlayMaterial.SetVector("_RightModifier", new Vector4(0, 1, 0, 0));
                            break;
                        }
                    case AnaglyphType.RedBlue:
                        {
                            overlayMaterial.SetVector("_LeftModifier", new Vector4(1, 0, 0, 0));
                            overlayMaterial.SetVector("_RightModifier", new Vector4(0, 0, 1, 0));
                            break;
                        }
                    case AnaglyphType.RedCyan:
                        {
                            overlayMaterial.SetVector("_LeftModifier", new Vector4(1, 0, 0, 0));
                            overlayMaterial.SetVector("_RightModifier", new Vector4(0, 1, 1, 0));
                            break;
                        }
                }
                overlayMaterial.SetInt("_StereoMode", STEREO_MODE_ANAGLYPH);
            }
            else
            {
                if (leftEye && rightEye)
                {
                    if (display == null ||
                        !display.swapEyes)
                    {
                        overlayMaterial.SetTexture("_Left", leftEye);
                        overlayMaterial.SetTexture("_Right", rightEye);
                    }
                    else
                    {
                        overlayMaterial.SetTexture("_Left", rightEye);
                        overlayMaterial.SetTexture("_Right", leftEye);
                    }

                    if (display.stereoMode == StereoMode.SideBySide || 
                        display.stereoMode == StereoMode.SideBySideMono ||
                        display.stereoMode == StereoMode.SideBySideLeftOnly ||
                        display.stereoMode == StereoMode.SideBySideRightOnly)
                        overlayMaterial.SetInt("_StereoMode", STEREO_MODE_SIDEBYSIDE);
                    else
                        overlayMaterial.SetInt("_StereoMode", STEREO_MODE_TOPBOTTOM);
                }
                else
                    overlayMaterial.SetInt("_StereoMode", STEREO_MODE_MONO);
            }

            if (warpTexture)
                overlayMaterial.SetTexture("_WarpTex", warpTexture);
            if (blendTexture)
                overlayMaterial.SetTexture("_BlendTex", blendTexture);
            if (blackLevelMask)
            {
                overlayMaterial.SetTexture("_BlackLevelMask", blackLevelMask);
                overlayMaterial.SetFloat("_BlackLevel", blackLevel);
            }

            overlayMaterial.SetInt("_UseWarp", warpTexture ? 1 : 0);
            overlayMaterial.SetInt("_UseBlend", blendTexture ? 1 : 0);
            overlayMaterial.SetInt("_UseBlackLevel", blackLevelMask ? 1 : 0);
        }
    }
}