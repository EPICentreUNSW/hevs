using UnityEngine;
using System.Linq;
using System.Collections.Generic;

using HEVS.Extensions;

namespace HEVS
{
    /// <summary>
    /// Display class for projectors projecting onto a curved surface with a large field-of-view.
    /// Options are read from the JSON configuration and then used to setup a display rig.
    /// Note: The warp/blend configuration for this type of display is currently under development and will change in a future build.
    /// </summary>
    [HEVS.CustomDisplay("curved")]
    public class CurvedDisplay : Display
    {
        /// <summary>
        /// The starting/left angle of the display, in degrees.
        /// </summary>
        public float angleStart { get; private set; } = 0;
        /// <summary>
        /// The ending/right angle of the display, in degrees.
        /// </summary>
        public float angleEnd { get; private set; } = 0;
        /// <summary>
        /// The number of slices used to construct the display.
        /// This is used to combat artefacts from rendering large fields-of-view.
        /// </summary>
        public int slices { get; private set; } = 1;
        /// <summary>
        /// The height of the display, in meters.
        /// </summary>
        public float height { get; private set; } = 1;
        /// <summary>
        /// The radius of a cylinder that this curved display would belong to.
        /// </summary>
        public float radius { get; private set; } = 1;
        /// <summary>
        /// The ground offset of the display, in meters. The entire display is offset upwards by this amount.
        /// </summary>
        public float groundOffset { get; private set; } = 0;
        /// <summary>
        /// The optional file path location of the warp information used for this display.
        /// If using a third-party warp/blend solution then this is not needed.
        /// </summary>
        public string warpMeshPath { get; private set; }
        /// <summary>
        /// The optional file path location of the black-level texture mask used to help combat overbrightness from overlapping projectors.
        /// </summary>
        public string blackLevelPath { get; private set; }
        /// <summary>
        /// The black-level threshold.
        /// </summary>
        public float blackLevel { get; private set; } = 0;
        /// <summary>
        /// Scale used to stretch the RenderTexture that this display used.
        /// </summary>
        public float renderTargetStretchFactor { get; private set; } = 1;
        /// <summary>
        /// The total angle that this display represents, in degrees.
        /// </summary>
        public float projectorAngle { get { return Mathf.Abs(angleEnd - angleStart); } }
        /// <summary>
        /// The total angle that each individual slice takes up, in degrees.
        /// </summary>
        public float sliceAngle { get { return Mathf.Abs(angleEnd - angleStart) / slices; } }
        /// <summary>
        /// The heading of this display, which is the angle half way between the startAngle and endAngle.
        /// </summary>
        public float heading { get { return angleStart + projectorAngle / 2; } }
        /// <summary>
        /// A list of all camera components used for this display for the left eye.
        /// </summary>
        public List<UnityEngine.Camera> leftSliceCameras { get; private set; } = new List<UnityEngine.Camera>();
        /// <summary>
        /// A list of all camera components used for this display for the right eye.
        /// </summary>
        public List<UnityEngine.Camera> rightSliceCameras { get; private set; } = new List<UnityEngine.Camera>();

        /// <summary>
        /// Create a Curved display from display config data.
        /// </summary>
        /// <param name="config"></param>
        public CurvedDisplay(Config.Display config) : base(config)
        {
            // parse the extra data
            if (config.json.Keys.Contains("angle_start"))
                angleStart = config.json["angle_start"].AsFloat;

            if (config.json.Keys.Contains("angle_end"))
                angleEnd = config.json["angle_end"].AsFloat;

            if (config.json.Keys.Contains("radius"))
                radius = config.json["radius"].AsFloat;

            if (config.json.Keys.Contains("height"))
                height = config.json["height"].AsFloat;

            if (config.json.Keys.Contains("ground_offset"))
                groundOffset = config.json["ground_offset"].AsFloat;

            if (config.json.Keys.Contains("slices"))
                slices = config.json["slices"].AsInt;

            if (config.json.Keys.Contains("warp_mesh_path"))
                warpMeshPath = config.json["warp_mesh_path"];

            if (config.json.Keys.Contains("black_level_path"))
                blackLevelPath = config.json["black_level_path"];

            if (config.json.Keys.Contains("black_level"))
                blackLevel = config.json["black_level"].AsFloat;

            if (config.json.Keys.Contains("rendertarget_stretch_factor"))
                renderTargetStretchFactor = config.json["rendertarget_stretch_factor"].AsFloat;
        }

        /// <summary>
        /// Creates a cloned copy of this display.
        /// </summary>
        /// <returns>Returns the cloned copy of this display.</returns>
        public override Display Clone()
        {
            return new CurvedDisplay(config);
        }

        /// <summary>
        /// Sets up the Unity scene to use this display.
        /// </summary>
        public override void ConfigureDisplayForScene()
        {
            rightSliceCameras.Clear();
            leftSliceCameras.Clear();
                        
            // setup capture cameras
            if (config.requiresDualCameras)
            {
                GameObject leftSlices = new GameObject(config.id + "_leftslices");
                GameObject rightSlices = new GameObject(config.id + "_rightslices");
                leftSlices.transform.SetParent(gameObject.transform, false);
                rightSlices.transform.SetParent(gameObject.transform, false);

                Vector3 leftPos = Vector3.left * config.eyeSeparation / 2.0f;
                Vector3 rightPos = Vector3.right * config.eyeSeparation / 2.0f;

                if (config.stereoMode == StereoMode.SideBySideLeftOnly ||
                    config.stereoMode == StereoMode.TopBottomLeftOnly)
                    rightPos = leftPos;
                else if (config.stereoMode == StereoMode.SideBySideRightOnly ||
                    config.stereoMode == StereoMode.TopBottomRightOnly)
                    leftPos = rightPos;
                else if (config.stereoMode == StereoMode.SideBySideMono ||
                    config.stereoMode == StereoMode.TopBottomMono)
                    leftPos = rightPos = Vector3.zero;

                leftSlices.transform.localPosition = leftPos;
                rightSlices.transform.localPosition = rightPos;

                for (int i = 0; i < slices; ++i)
                {
                    // create a slice camera that renders into a set viewport
                    GameObject goL = new GameObject(config.id + "_slice[" + i + "]");
                    GameObject goR = new GameObject(config.id + "_slice[" + i + "]");

                    UnityEngine.Camera cameraL = goL.AddComponent<UnityEngine.Camera>();
                    UnityEngine.Camera cameraR = goR.AddComponent<UnityEngine.Camera>();

                    leftSliceCameras.Add(cameraL);
                    rightSliceCameras.Add(cameraR);

                    var extL = goL.AddComponent<CurvedSliceCameraExtension>();
                    var extR = goR.AddComponent<CurvedSliceCameraExtension>();
                    extL.display = this;
                    extL.sliceIndex = i;
                    extR.display = this;
                    extR.sliceIndex = i;

                    cameraL.CopyFrom(Camera.main);
                    cameraR.CopyFrom(Camera.main);

                    cameraL.depth = Camera.main.depth - slices * 2 + i;
                    cameraR.depth = Camera.main.depth - slices + i;

                    cameraL.transform.SetParent(leftSlices.transform, false);
                    cameraR.transform.SetParent(rightSlices.transform, false);

                    cameraL.transform.localPosition = Vector3.zero;
                    cameraR.transform.localPosition = Vector3.zero;

                    // Rotate camera to its correct position, looking at the angle halfway between its two seams
                    cameraL.transform.Rotate(Vector3.up, angleStart + sliceAngle * i + sliceAngle / 2);
                    cameraL.allowHDR = false;
                    cameraR.transform.Rotate(Vector3.up, angleStart + sliceAngle * i + sliceAngle / 2);
                    cameraR.allowHDR = false;

                    cameraL.cullingMask = config.layerMask & ~LayerMask.GetMask("HEVSRightEyeOnly");
                    cameraR.cullingMask = config.layerMask & ~LayerMask.GetMask("HEVSLeftEyeOnly");

                    goL.CopyComponentsOfTypeFrom<CameraExtension>(Camera.main.gameObject);
                    goR.CopyComponentsOfTypeFrom<CameraExtension>(Camera.main.gameObject);
                }
            }
            else
            {
                for (int i = 0; i < slices; ++i)
                {
                    // create a slice camera that renders into a set viewport
                    GameObject go = new GameObject(config.id + "_slice[" + i + "]");

                    UnityEngine.Camera camera = go.AddComponent<UnityEngine.Camera>();

                    leftSliceCameras.Add(camera);

                    var extL = go.AddComponent<CurvedSliceCameraExtension>();
                    extL.display = this;
                    extL.sliceIndex = i;

                    camera.CopyFrom(Camera.main);

                    camera.depth = Camera.main.depth - slices * 2 + i;

                    camera.transform.SetParent(gameObject.transform, false);

                    if (config.stereoMode == StereoMode.LeftOnly)
                        camera.transform.localPosition = Vector3.left * config.eyeSeparation * 0.5f;
                    else if (config.stereoMode == StereoMode.RightOnly)
                        camera.transform.localPosition = Vector3.right * config.eyeSeparation * 0.5f;
                    else
                        camera.transform.localPosition = Vector3.zero;

                    // Rotate camera to its correct position, looking at the angle halfway between its two seams
                    camera.transform.Rotate(Vector3.up, angleStart + sliceAngle * i + sliceAngle / 2);
                    camera.allowHDR = false;

                    camera.cullingMask = config.layerMask;

                    go.CopyComponentsOfTypeFrom<CameraExtension>(Camera.main.gameObject);
                }
            }

            // camera needs to be scaled to render inside of the viewport
            float vpX = 0;
            float vpY = 0;
            float vpWidth = 1;
            float vpHeight = 1;
            
            if (config.viewport != null)
            {
                if (config.viewport.absolute)
                {
                    vpX = config.viewport.x / Core.activeNode.resolution.x;
                    vpY = config.viewport.y / Core.activeNode.resolution.y;
                    vpWidth = config.viewport.width / Core.activeNode.resolution.x;
                    vpHeight = config.viewport.height / Core.activeNode.resolution.y;
                }
                else
                {
                    vpX = config.viewport.x;
                    vpY = config.viewport.y;
                    vpWidth = config.viewport.width;
                    vpHeight = config.viewport.height;
                }
            }

            GameObject blendProjector = null;

            AVIECameraExtension avieCamera = null;
            HEVSCameraExtension hevsCamera = null;

            // are we using AVIE warp mesh?
            if (!string.IsNullOrWhiteSpace(warpMeshPath))
            {
                blendProjector = new GameObject(config.id + "_output");
                blendProjector.transform.position = new Vector3(9999 + config.index.x * 5, 9999, 9999);
                blendProjector.transform.SetParent(gameObject.transform, true);

                avieCamera = blendProjector.AddComponent<AVIECameraExtension>();
                avieCamera.display = this;
                avieCamera.SetupAVIEProjector(config.index.x * 2);

                avieCamera.GetComponent<UnityEngine.Camera>().SetAndActivateTargetDisplay(config.monitor);
            }
            // standard warp?
            else if (!string.IsNullOrWhiteSpace(config.warpPath))
            {
                blendProjector = gameObject;

                hevsCamera = blendProjector.GetOrAddComponent<HEVSCameraExtension>();
                hevsCamera.display = this;
                hevsCamera.warpTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
                hevsCamera.warpTexture.wrapMode = TextureWrapMode.Clamp;
                (hevsCamera.warpTexture as Texture2D).LoadFloatingpointTiff(config.warpPath);

                hevsCamera.GetComponent<UnityEngine.Camera>().cullingMask = 0;
                hevsCamera.GetComponent<UnityEngine.Camera>().SetAndActivateTargetDisplay(config.monitor);

                // should we apply some sort of stereo?
                switch (config.stereoMode)
                {
                    case StereoMode.SideBySide:
                    case StereoMode.SideBySideLeftOnly:
                    case StereoMode.SideBySideRightOnly:
                    case StereoMode.SideBySideMono:
                    case StereoMode.TopBottom:
                    case StereoMode.TopBottomLeftOnly:
                    case StereoMode.TopBottomRightOnly:
                    case StereoMode.TopBottomMono:
                        {
                            hevsCamera.leftEye = new RenderTexture((int)(Core.activeNode.resolution.x * vpWidth), (int)(Core.activeNode.resolution.y * vpHeight), 16, RenderTextureFormat.ARGB32);
                            hevsCamera.rightEye = new RenderTexture((int)(Core.activeNode.resolution.x * vpWidth), (int)(Core.activeNode.resolution.y * vpHeight), 16, RenderTextureFormat.ARGB32);
                            break;
                        }
                    case StereoMode.RedGreen:
                    case StereoMode.RedBlue:
                    case StereoMode.RedCyan:
                        {
                            break;
                        }
                    default:
                        // is this needed?
                        hevsCamera.sourceImage = new RenderTexture((int)(Core.activeNode.resolution.x * vpWidth), (int)(Core.activeNode.resolution.y * vpHeight), 24);
                        break;
                }
            }

            // anaglyph?
            if (config.stereoMode == StereoMode.RedGreen ||
                config.stereoMode == StereoMode.RedBlue ||
                config.stereoMode == StereoMode.RedCyan)
            {
                if (!blendProjector)
                    blendProjector = gameObject;

                hevsCamera = blendProjector.GetOrAddComponent<HEVSCameraExtension>();
                hevsCamera.display = this;
                hevsCamera.GetComponent<UnityEngine.Camera>().cullingMask = 0;

                switch (config.stereoMode)
                {
                    case StereoMode.RedGreen: hevsCamera.anaglyphType = AnaglyphType.RedGreen; break;
                    case StereoMode.RedBlue: hevsCamera.anaglyphType = AnaglyphType.RedBlue; break;
                    case StereoMode.RedCyan: hevsCamera.anaglyphType = AnaglyphType.RedCyan; break;
                }

                hevsCamera.leftEye = new RenderTexture((int)(Core.activeNode.resolution.x * vpWidth), (int)(Core.activeNode.resolution.y * vpHeight), 16, RenderTextureFormat.ARGB32);
                hevsCamera.rightEye = new RenderTexture((int)(Core.activeNode.resolution.x * vpWidth), (int)(Core.activeNode.resolution.y * vpHeight), 16, RenderTextureFormat.ARGB32);

                hevsCamera.GetComponent<UnityEngine.Camera>().SetAndActivateTargetDisplay(config.monitor);
            }

            // how much of the viewport does a slice take up?
            float vpSlice = vpWidth / slices;

            for (int i = 0; i < slices; ++i)
            {
                // if using warp/blend/anaglyph then need to render into a render target
                if (avieCamera)
                {
                    leftSliceCameras[i].targetTexture = avieCamera.renderTexture;
                    if (rightSliceCameras.Count > 0)
                        rightSliceCameras[i].targetTexture = avieCamera.renderTexture;
                }
                else if (hevsCamera)
                {
                    // setup target textures
                    leftSliceCameras[i].targetTexture = hevsCamera.leftEye ? hevsCamera.leftEye as RenderTexture : hevsCamera.sourceImage;
                    if (rightSliceCameras.Count > 0)
                        rightSliceCameras[i].targetTexture = hevsCamera.rightEye ? hevsCamera.rightEye as RenderTexture : hevsCamera.sourceImage;
                }
                else
                {
                    leftSliceCameras[i].SetAndActivateTargetDisplay(config.monitor);
                    if (rightSliceCameras.Count > 0)
                        rightSliceCameras[i].SetAndActivateTargetDisplay(config.monitor);
                }

                switch (config.stereoMode)
                {
                    case StereoMode.Mono:
                    case StereoMode.LeftOnly:
                    case StereoMode.RightOnly:
                        {
                            leftSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight);
                            break;
                        }

                    case StereoMode.Sequential:
                    case StereoMode.QuadBuffered:
                    case StereoMode.RedGreen:
                    case StereoMode.RedBlue:
                    case StereoMode.RedCyan:
                        {
                            leftSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight);
                            rightSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight);
                            break;
                        }

                    case StereoMode.SideBySide:
                    case StereoMode.SideBySideLeftOnly:
                    case StereoMode.SideBySideRightOnly:
                    case StereoMode.SideBySideMono:
                        {
                            if (hevsCamera && hevsCamera.leftEye && hevsCamera.rightEye)
                            {
                                leftSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight);
                                rightSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight);
                            }
                            else
                            {
                                if (config.swapEyes)
                                {
                                    leftSliceCameras[i].rect = new Rect(vpX + vpWidth / 2 + (vpSlice / 2) * i, vpY, vpSlice / 2, vpHeight);
                                    rightSliceCameras[i].rect = new Rect(vpX + (vpSlice / 2) * i, vpY, vpSlice / 2, vpHeight);
                                }
                                else
                                {
                                    leftSliceCameras[i].rect = new Rect(vpX + (vpSlice / 2) * i, vpY, vpSlice / 2, vpHeight);
                                    rightSliceCameras[i].rect = new Rect(vpX + vpWidth / 2 + (vpSlice / 2) * i, vpY, vpSlice / 2, vpHeight);
                                }
                            }
                            break;
                        }

                    case StereoMode.TopBottom:
                    case StereoMode.TopBottomLeftOnly:
                    case StereoMode.TopBottomRightOnly:
                    case StereoMode.TopBottomMono:
                        {
                            if (hevsCamera && hevsCamera.leftEye && hevsCamera.rightEye)
                            {
                                leftSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight);
                                rightSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight);
                            }
                            else
                            {
                                if (config.swapEyes)
                                {
                                    leftSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight / 2);
                                    rightSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY + vpHeight / 2, vpSlice, vpHeight / 2);
                                }
                                else
                                {
                                    leftSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY + vpHeight / 2, vpSlice, vpHeight / 2);
                                    rightSliceCameras[i].rect = new Rect(vpX + vpSlice * i, vpY, vpSlice, vpHeight / 2);
                                }
                            }
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Set the clear flag for all cameras.
        /// </summary>
        /// <param name="flags">CameraClearFlags to set.</param>
        public override void SetClearFlags(CameraClearFlags flags)
        {
            foreach (var camera in leftSliceCameras)
                camera.clearFlags = flags;
            foreach (var camera in rightSliceCameras)
                camera.clearFlags = flags;
        }

        /// <summary>
        /// Set the clear colour for all cameras.
        /// </summary>
        /// <param name="colour">The colour to set.</param>
        public override void SetBackgroundColour(Color colour)
        {
            foreach (var camera in leftSliceCameras)
                camera.backgroundColor = colour;
            foreach (var camera in rightSliceCameras)
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
            var sp = SceneOrigin.position;
            var sr = SceneOrigin.rotation;

            if (config.transform != null)
            {
                sp += SceneOrigin.rotation * config.transform.Translation;
                sr *= config.transform.Rotation;
            }

            // intersect a ray with a cylinder
            Vector3[] p = Intersection.RayCylinderIntersection(ray, sp, sp + Vector3.up * height, radius);
            if (p != null)
            {
                var toHit = p[1] - ray.origin;
                distance = Vector3.Magnitude(toHit);
                toHit /= distance;
                hitPoint2D = new Vector2(Mathf.Atan2(toHit.z, toHit.x), Vector3.Dot(toHit, Vector3.up));
                return true;
            }

            distance = 0;
            hitPoint2D = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Get a world-space ray from the display via a 2D display-space point.
        /// </summary>
        /// <param name="displayspacePoint">The 2D display-space point to get the ray from, which is in [0,1] range for each axis.</param>
        /// <returns>Returns the world-space ray that passes through the display-space point.</returns>
        public override Ray ViewportPointToRay(Vector2 displayspacePoint)
        {
            float s = 1.0f / slices;
            int currentSlice = Mathf.FloorToInt(displayspacePoint.x / s);
            displayspacePoint.x = (displayspacePoint.x % s) / s;
            return leftSliceCameras[currentSlice].ViewportPointToRay(displayspacePoint);
        }

        /// <summary>
        /// A method to draw the display's Gizmo within the editor.
        /// </summary>
        /// <param name="config">The config data to use for drawing a gizmo of this type.</param>
        public static void DrawGizmo(Config.Display config)
        {
            var highlight = Gizmos.color;

            var sp = SceneOrigin.position;
            var sr = SceneOrigin.rotation;
            
            if (config.transform != null)
            {
                if (config.transform.HasTranslation)
                    sp += SceneOrigin.rotation * config.transform.Translation;
                if (config.transform.HasRotation)
                    sr *= config.transform.Rotation;
            }

            float radius = config.json["radius"].AsFloat;
            float height = config.json["height"].AsFloat;
            float angleStart = config.json["angle_start"].AsFloat;
            float angleEnd = config.json["angle_end"].AsFloat;
            int slices = config.json["slices"].AsInt;

            float sliceAngle = Mathf.Abs(angleEnd - angleStart) / slices;

            for (int i = 0; i < slices; ++i)
            {
                float s = Mathf.Sin(Mathf.Deg2Rad * (angleStart + i * sliceAngle)) * radius;
                float c = Mathf.Cos(Mathf.Deg2Rad * (angleStart + i * sliceAngle)) * radius;
                float s2 = Mathf.Sin(Mathf.Deg2Rad * (angleStart + (i + 1) * sliceAngle)) * radius;
                float c2 = Mathf.Cos(Mathf.Deg2Rad * (angleStart + (i + 1) * sliceAngle)) * radius;

                Vector3 ll = sr * new Vector3(s, 0, c);
                Vector3 lr = sr * new Vector3(s2, 0, c2);

                Vector3 up = sr * Vector3.up;

                Gizmos.color = highlight;

                // draw a closing vertical line
                if (i == slices - 1)
                    Gizmos.DrawLine(lr + up * height + sp, lr + sp);

                // draw two horizontal lines (but not on last vertical)
                Gizmos.DrawLine(ll + up * height + sp, lr + up * height + sp);
                Gizmos.DrawLine(ll + sp, lr + sp);

                // draw a vertical line
                if (i != 0)
                    Gizmos.color = Color.grey;
                Gizmos.DrawLine(ll + up * height + sp, ll + sp);
            }
        }

        /// <summary>
        /// Gather the geometry that represents this display.
        /// </summary>
        /// <param name="vertices">The vertices that represent the display's mesh.</param>
        /// <param name="indices">The indices used to define the display's mesh topography.</param>
        /// <param name="uvs">The texture coordinates to apply across the display's mesh.</param>
        public override void GatherDisplayGeometry(List<Vector3> vertices, List<int> indices, List<Vector2> uvs)
        {
            vertices.Clear();
            indices.Clear();
            uvs.Clear();

            for (int i = 0; i < slices; ++i)
            {
                float s = Mathf.Sin(Mathf.Deg2Rad * (angleStart + i * sliceAngle)) * radius;
                float c = Mathf.Cos(Mathf.Deg2Rad * (angleStart + i * sliceAngle)) * radius;
                float s2 = Mathf.Sin(Mathf.Deg2Rad * (angleStart + (i + 1) * sliceAngle)) * radius;
                float c2 = Mathf.Cos(Mathf.Deg2Rad * (angleStart + (i + 1) * sliceAngle)) * radius;

                Vector3 ll = new Vector3(s, 0, c);
                Vector3 lr = new Vector3(s2, 0, c2);

                vertices.Add(ll + Vector3.up * height);
                vertices.Add(ll);
                vertices.Add(lr);
                vertices.Add(lr + Vector3.up * height);

                indices.Add(i * 4 + 0);
                indices.Add(i * 4 + 2);
                indices.Add(i * 4 + 1);
                indices.Add(i * 4 + 0);
                indices.Add(i * 4 + 3);
                indices.Add(i * 4 + 2);

                float u0 = i / (float)slices;
                float u1 = (i+1) / (float)slices;

                uvs.Add(new Vector2(u0,1));
                uvs.Add(new Vector2(u0,0));
                uvs.Add(new Vector2(u1,0));
                uvs.Add(new Vector2(u1,1));
            }
        }
    }
}