using UnityEngine;
using System.Linq;
using System.IO;
using System;

using HEVS.Extensions;
using System.Collections.Generic;

namespace HEVS
{
    /// <summary>
    /// Display class for Hemispherical Domes.
    /// Options are read from the JSON configuration and then used to setup a display rig.
    /// Note: the warp/blend settings are under development and will change in a future build.
    /// </summary>
    [HEVS.CustomDisplay("dome")]
    public class DomeDisplay : Display
    {
        /// <summary>
        /// The physical radius of the dome.
        /// </summary>
        public float radius { get; private set; } = 1;

        /// <summary>
        /// The number of projectors used to display the dome.
        /// </summary>
        public int projectorCount { get; private set; } = 1;

        /// <summary>
        /// The resolution of the RenderTexture used for capturing the fisheye image for the dome.
        /// </summary>
        public Vector2Int fisheyeResolution { get; private set; } = new Vector2Int(1024, 1024);

        /// <summary>
        /// When using VIOSO Anyblend, you can specify the path to the VIOSO vwf which HEVS can use 
        /// to configure the warp/blend for dome projectors.
        /// See https://vioso.com/ for more details.
        /// </summary>
        public string viosoPath { get; private set; }

        /// <summary>
        /// Should the warp/blend use border smoothing.
        /// </summary>
        public bool smoothBorder { get; private set; } = false;

        /// <summary>
        /// The size of the warp/blend border smoothing.
        /// </summary>
        public float borderPercent { get; private set; } = 0;

        /// <summary>
        /// When using custom warp you can specify the location of warp mesh csv data that the projectors will use.
        /// </summary>
        public string warpMeshPath { get; private set; }

        /// <summary>
        /// The projector IDs that this display uses. For example, for a dome that uses 8 total 
        /// projectors to display the dome, display A might use projectors 1, 2, 3 and 4, while 
        /// display B uses projectors 5, 6, 7 and 8.
        /// </summary>
        public int[] projectorIDs { get; private set; }

        /// <summary>
        /// A list of monitors that each projector will output to.
        /// </summary>
        public int[] projectorMonitors { get; private set; }

        /// <summary>
        /// The 2D layout of the projectors when using a single display to output to multiple pojectors.
        /// </summary>
        public Vector2Int layout { get; private set; } = Vector2Int.one;

        /// <summary>
        /// Create a dome display from a specified display config.
        /// </summary>
        /// <param name="config">The config to create the display from.</param>
        public DomeDisplay(Config.Display config) : base(config) 
        {
            // required data
            var radiusConfig = config.GetProperty("radius");
            if (radiusConfig != null)
                radius = radiusConfig.AsFloat;

            var fisheyeResolutionConfig = config.GetProperty("fisheye_resolution");
            if (fisheyeResolutionConfig != null)
                fisheyeResolution = new Vector2Int(fisheyeResolutionConfig[0].AsInt, fisheyeResolutionConfig[1].AsInt);

            // optional data
            var projectorIDsConfig = config.GetProperty("projectors");
            if (projectorIDsConfig != null)
            {
                projectorCount = projectorIDsConfig.Count;
                projectorIDs = new int[projectorCount];
                for (int i = 0; i < projectorCount; ++i)
                    projectorIDs[i] = projectorIDsConfig[i].AsInt;
            }
            else
            {
                var projectorCountConfig = config.GetProperty("projector_count");
                if (projectorCountConfig)
                    projectorCount = projectorCountConfig.AsInt;
            }

            var projectorMonitorsConfig = config.GetProperty("projector_monitors");
            if (projectorMonitorsConfig != null)
            {
                projectorMonitors = new int[projectorMonitorsConfig.Count];
                for (int i = 0; i < projectorMonitors.Length; ++i)
                    projectorMonitors[i] = projectorMonitorsConfig[i].AsInt;
            }

            var warpMeshPathConfig = config.GetProperty("warp_mesh_path");
            if (warpMeshPathConfig != null)
                warpMeshPath = warpMeshPathConfig;


            var layoutConfig = config.GetProperty("layout");
            if (layoutConfig != null)
                layout = new Vector2Int(layoutConfig[0].AsInt, layoutConfig[1].AsInt);

            var viosoConfig = config.GetProperty("vioso_path");
            if (viosoConfig != null)
                viosoPath = viosoConfig;

            var smoothConfig = config.GetProperty("smooth_border");
            if (smoothConfig != null)
                smoothBorder = smoothConfig.AsBool;

            var borderConfig = config.GetProperty("border_percent");
            if (borderConfig != null)
                borderPercent = borderConfig.AsFloat;

            // if multiple projectors then a warp directory must be set
            if (projectorCount > 1 &&
                (string.IsNullOrEmpty(warpMeshPath)))
            {
                Debug.LogError("HEVS: Config file specifies a dome display with multiple projectors but no warp mesh data directory is set! Multi-display dome's currently require a directory warp_mesh_path containing warpmesh data, rather than warp_path for a single warp image.");
            }
        }

        /// <summary>
        /// Creates a cloned copy of this display.
        /// </summary>
        /// <returns>Returns the cloned copy of this display.</returns>
        public override Display Clone()
        {
            return new DomeDisplay(config);
        }

        /// <summary>
        /// Sets up the Unity scene to use this display.
        /// </summary>
        public override void ConfigureDisplayForScene()
        {
            int fisheyeLayerMask = LayerMask.GetMask("HEVSCameras");
            int hevsCaptureCameraMask = LayerMask.GetMask(new string[]{
                "HEVSFullscreenOverlay",
                "HEVSMonitor0", 
                "HEVSMonitor1",
                "HEVSMonitor2",
                "HEVSMonitor3",
                "HEVSMonitor4",
                "HEVSMonitor5",
                "HEVSMonitor6",
                "HEVSMonitor7"
            });

            // setup capture cameras - for now no stereo
            captureCameras.Clear();
            outputCameras.Clear();

            // create fisheye capture camera
            Func<string, Transform, UnityEngine.Camera, Quaternion, UnityEngine.Camera> setupDomeCamera = (name, parent, original, orientation) =>
            {
                // create object and attach it
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.transform.localRotation = orientation;

                // create camera and set it up
                UnityEngine.Camera camera = go.AddComponent<UnityEngine.Camera>();
                camera.fieldOfView = 90;
                camera.allowHDR = false;
                camera.backgroundColor = original.backgroundColor;
                camera.clearFlags = original.clearFlags;
                camera.cullingMask = config.layerMask & ~fisheyeLayerMask & ~hevsCaptureCameraMask;

                // create render target using half of the fisheye texture resolution
                camera.targetTexture = new RenderTexture(fisheyeResolution.x / 2, fisheyeResolution.y / 2, 24, RenderTextureFormat.Default);
                camera.targetTexture.wrapMode = TextureWrapMode.Clamp;

                // copy camera extensions
                original.gameObject.CopyComponentsOfTypeTo<CameraExtension>(go);

                captureCameras.Add(camera);

                return camera;
            };

            // create and set up cameras
            UnityEngine.Camera leftCamera = setupDomeCamera(config.id + "_left", gameObject.transform, Camera.main, Quaternion.Euler(0, -45, 0));
            UnityEngine.Camera rightCamera = setupDomeCamera(config.id + "_right", gameObject.transform, Camera.main, Quaternion.Euler(0, 45, 0));
            UnityEngine.Camera topCamera = setupDomeCamera(config.id + "_top", gameObject.transform, Camera.main, Quaternion.Euler(-90, -45, 0));
            UnityEngine.Camera bottomCamera = setupDomeCamera(config.id + "_bottom", gameObject.transform, Camera.main, Quaternion.Euler(90, 45, 0));

            // need to set viewport!

            // setup output camera - for now no stereo

            var fisheyeCamera = gameObject.GetOrAddComponent<UnityEngine.Camera>();
            outputCameras.Add(fisheyeCamera);

            fisheyeCamera.orthographic = true;
            fisheyeCamera.orthographicSize = 1;
            fisheyeCamera.nearClipPlane = 0.1f;
            fisheyeCamera.farClipPlane = 1;
            fisheyeCamera.useOcclusionCulling = false;
            fisheyeCamera.allowHDR = false;
            fisheyeCamera.cullingMask = fisheyeLayerMask;
            fisheyeCamera.backgroundColor = Color.black;
            fisheyeCamera.clearFlags = CameraClearFlags.SolidColor;

            var domeExtension = gameObject.GetOrAddComponent<HEVSCameraExtension>();
            domeExtension.display = this;
            domeExtension.aspectScale = config.aspectScale;
            domeExtension.SetFisheye(            
                leftCamera.targetTexture,
                rightCamera.targetTexture,
                topCamera.targetTexture,
                bottomCamera.targetTexture
            );

            // are we using custom warp/blend for multiple projectors?
            if (projectorCount > 1 ||
                (projectorIDs != null && projectorIDs.Length > 0))
            {
                fisheyeCamera.targetTexture = new RenderTexture(fisheyeResolution.x,
                                                                fisheyeResolution.y,
                                                                24, RenderTextureFormat.Default);
                fisheyeCamera.targetTexture.wrapMode = TextureWrapMode.Clamp;

                int projectorOutputMask = LayerMask.GetMask("HEVSMonitor" + config.monitor);
                int projectorMonitorLayer = LayerMask.NameToLayer("HEVSMonitor" + config.monitor);

                GameObject domeSlices = null;

                if (projectorMonitors == null)
                {
                    domeSlices = new GameObject(config.id + "_slices");
                    domeSlices.transform.SetParent(gameObject.transform, false);
                    domeSlices.transform.localPosition = new Vector3(5, 0, 0);

                    UnityEngine.Camera domeCam = domeSlices.AddComponent<UnityEngine.Camera>();
                    domeCam.nearClipPlane = 0.1f;
                    domeCam.farClipPlane = 1;
                    domeCam.orthographic = true;
                    domeCam.orthographicSize = 1;
                    domeCam.cullingMask = projectorOutputMask;//fisheyeLayerMask;
                    domeCam.allowHDR = false;
                    domeCam.useOcclusionCulling = false;

                    domeCam.SetAndActivateTargetDisplay(config.monitor);
                }

                float screenAspect = Screen.width / (float)Screen.height;// * 0.5f;

                // this should move into the camera extension
                for (int i = 0; i < projectorCount; ++i)
                {
                    int projector = projectorIDs == null ? i : projectorIDs[i];

                    if (projectorMonitors != null)
                    {
                        projectorOutputMask = LayerMask.GetMask("HEVSMonitor" + projectorMonitors[i]);
                        projectorMonitorLayer = LayerMask.NameToLayer("HEVSMonitor" + projectorMonitors[i]);

                        domeSlices = new GameObject(config.id + "_slices_monitor_" + projectorMonitors[i]);
                        domeSlices.transform.SetParent(gameObject.transform, false);
                        domeSlices.transform.localPosition = new Vector3(5, 0, 0);

                        UnityEngine.Camera domeCam = domeSlices.AddComponent<UnityEngine.Camera>();
                        domeCam.nearClipPlane = 0.1f;
                        domeCam.farClipPlane = 1;
                        domeCam.orthographic = true;
                        domeCam.orthographicSize = 0.5f;
                        domeCam.cullingMask = projectorOutputMask;//fisheyeLayerMask;
                        domeCam.allowHDR = false;
                        domeCam.useOcclusionCulling = false;

                        domeCam.SetAndActivateTargetDisplay(projectorMonitors[i]);
                    }

                    GameObject slice = new GameObject(config.id + "_slice_" + projector);
                    slice.transform.SetParent(domeSlices.transform, false);
                    slice.layer = projectorMonitorLayer;//fisheyeLayer;

                    // attach mesh
                    MeshFilter filter = slice.AddComponent<MeshFilter>();
                    MeshRenderer renderer = slice.AddComponent<MeshRenderer>();

                    // create warped mesh
                    filter.mesh = WarpMeshUtilities.ParseDomeProjectionWarpMesh(Path.Combine(warpMeshPath, "warpmap_" + projector + ".csv"),
                                                               Path.Combine(warpMeshPath, "cutting_" + projector + ".csv"),
                                                               config.aspectScale * screenAspect);

                    // load and attach blend textures, and assign shader/material
                    renderer.material = new Material(Shader.Find("HEVS/ProjectorBlend"));
                    renderer.material.SetInt("_BlendMode", 1); // Note: temporary until dome blend uses same format as cylinder.
                    renderer.material.SetTexture("_MainTex", fisheyeCamera.targetTexture);
                    renderer.material.SetTexture("_BlendTex", Utils.LoadTexture(Path.Combine(warpMeshPath, "blending_" + projector + ".png")));
                    renderer.material.SetTextureScale("_BlendTex", new Vector2(1.0f / (config.aspectScale * screenAspect), -1f));
                    renderer.material.SetTextureOffset("_BlendTex", new Vector2(0.5f, 0.5f));

                    // offset panel if it uses a single output for multiple panels (i.e. mosaic)
                    if (projectorMonitors == null)
                    {
                        // calculate panel's layout position
                        // which panel are we?
                        int x = i % layout.x;
                        int y = i / layout.x;
                        float xOffset = -config.aspectScale * screenAspect * layout.x * 0.5f + config.aspectScale * screenAspect * 0.5f;
                        float yOffset = layout.y * 0.5f - 0.5f;
                        slice.transform.localPosition = new Vector3(xOffset + x * config.aspectScale * screenAspect, yOffset - y, 0.5f);
                    }
                    else
                        slice.transform.localPosition = new Vector3(0, 0, 0.5f);
                    slice.transform.localRotation = Quaternion.Euler(180, 0, 0);
                }
            }
            else
            {
                Camera.ConfigureViewportForCamera(fisheyeCamera, this);

                // does it use vioso warp/blend?
                if (!string.IsNullOrWhiteSpace(viosoPath))
                {
                    // create fisheye camera
                    ViosoVwfReader vwfReader = new ViosoVwfReader();
                    if (File.Exists(viosoPath))
                    {
                        vwfReader.ReadVwf(viosoPath);

                        if (vwfReader.blendTextures.Count > 0)
                            domeExtension.blendTexture = vwfReader.blendTextures[0];
                        if (vwfReader.warpTextures.Count > 0)
                            domeExtension.warpTexture = vwfReader.warpTextures[0];
                        domeExtension.border = smoothBorder ? 0.01f * borderPercent : 0f;
                    }
                    else
                        Debug.Log("HEVS: no VWF file found, continuing without any warp/blend");
                }
                else
                    Camera.ConfigureWarpAndBlend(this, gameObject);

                fisheyeCamera.SetAndActivateTargetDisplay(config.monitor);
            }
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
            var sp = SceneOrigin.position;
            var sr = SceneOrigin.rotation;

            if (config.transform != null)
            {
                if (config.transform.HasTranslation)
                    sp +=SceneOrigin.rotation * config.transform.Translation;
                if (config.transform.HasRotation)
                    sr *= config.transform.Rotation;
            }

            Vector3[] p = Intersection.RaySphereIntersection(ray, sp, radius);
            if (p != null)
            {
                var toHit = p[1] - ray.origin;
                distance = Vector3.Magnitude(toHit);
                toHit /= distance;
                hitPoint2D = new Vector3(Vector3.Dot(toHit, sr * Vector3.right), Vector3.Dot(toHit, sr * Vector3.up));
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
            // convert point to -1 to 1 range
            var p = displayspacePoint * 2.0f - Vector2.one;
            p.x = Mathf.Clamp(p.x, -1.0f, 1.0f);
            p.y = Mathf.Clamp(p.y, -1.0f, 1.0f) * -1;
            p = Vector2.ClampMagnitude(p, 1.0f);

            Quaternion r = Quaternion.Euler(p.y * 90, p.x * 90, 0);

            return new Ray(gameObject.transform.position, gameObject.transform.rotation * (r * Vector3.forward));
        }

        /// <summary>
        /// A method to draw the display's Gizmo within the editor.
        /// </summary>
        /// <param name="config">The config data to use for drawing a gizmo of this type.</param>
        public static void DrawGizmo(Config.Display config)
        {
            float radius = config.json["radius"].AsFloat;

            int segments = 16;
            for (int i = 0; i < segments; ++i)
            {
                float angle = i / (float)segments * Mathf.PI * 2;
                float angle2 = (i + 1) / (float)segments * Mathf.PI * 2;

                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;
                float x2 = Mathf.Sin(angle2) * radius;
                float z2 = Mathf.Cos(angle2) * radius;

                Vector3 v1 = new Vector3(x, z, 0);
                Vector3 v2 = new Vector3(x2, z2, 0);

                // NOPE NOPE NOPE
                var td = config.transform.PostConcatenate(UnityEngine.Camera.main ? UnityEngine.Camera.main.transform : Camera.main.transform);

                v1 = td.Rotation * v1 + td.Translation;
                v2 = td.Rotation * v2 + td.Translation;

                Gizmos.DrawLine(v1, v2);

                for (int j = 0; j < (segments / 4); ++j)
                {
                    float angle3 = j / (float)segments * Mathf.PI * 2;
                    float angle4 = (j + 1) / (float)segments * Mathf.PI * 2;

                    float c1 = Mathf.Cos(j / (float)(segments / 4) * (Mathf.PI * 0.5f));
                    float c2 = Mathf.Cos((j + 1) / (float)(segments / 4) * (Mathf.PI * 0.5f));

                    float y = Mathf.Sin(angle3) * radius;
                    float y2 = Mathf.Sin(angle4) * radius;

                    v1 = new Vector3(x * c1, z * c1, y);
                    v2 = new Vector3(x * c2, z * c2, y2);

                    v1 = td.Rotation * v1 + td.Translation;
                    v2 = td.Rotation * v2 + td.Translation;

                    Gizmos.DrawLine(v1, v2);
                }
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
        }
    }
}