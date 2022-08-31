using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Attribute used for creating custom display types that can be loaded from HEVS JSON configuration files.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomDisplayAttribute : Attribute
    {
        /// <summary>
        /// Name of the type read from the JSON configuration file.
        /// </summary>
        public string typeName;

        /// <summary>
        /// Constructs a custom display with a set type name.
        /// </summary>
        /// <param name="typeName">The type name to use.</param>
        public CustomDisplayAttribute(string typeName) { this.typeName = typeName; }
    }

    /// <summary>
    /// Base class for HEVS displays. Can be used for creating derived custom display types.
    /// </summary>
    public abstract class Display
    {
        internal static Dictionary<string, Type> registeredDisplayTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        internal static void GatherCustomDisplayTypes()
        {
            // find and register all custom display data
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                                 .Where(x => typeof(Display).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            foreach (Type type in types)
            {
                CustomDisplayAttribute attrib = (CustomDisplayAttribute)Attribute.GetCustomAttribute(type, typeof(CustomDisplayAttribute));
                if (attrib != null)
                {
                    if (registeredDisplayTypes.ContainsKey(attrib.typeName))
                        Debug.LogError("HEVS: Display type already registered for type [" + attrib.typeName + "]! New definition will be ignored. This can cause unknown behaviour!");
                    else
                        registeredDisplayTypes.Add(attrib.typeName, type);
                }
            }
        }

        internal static Display CreateDisplay(Config.Display display)
        {
            if (registeredDisplayTypes.ContainsKey(display.type))
                return (Display)Activator.CreateInstance(registeredDisplayTypes[display.type], display);

            return null;
        }

        /// <summary>
        /// This display's GameObject within the scene heirarchy.
        /// </summary>
        public GameObject gameObject { get; private set; }

        /// <summary>
        /// The config used to define this display.
        /// </summary>
        public Config.Display config { get; private set; }

        /// <summary>
        /// Access to the camera components created for this display to capture the scene.
        /// </summary>
        public List<UnityEngine.Camera> captureCameras { get; protected set; } = new List<UnityEngine.Camera>();

        /// <summary>
        /// Access to the primary camera used to capture the scene for this display.
        /// </summary>
        public UnityEngine.Camera primaryCaptureCamera => captureCameras[0];

        /// <summary>
        /// Access to the camera components created for this display to output the scene.
        /// </summary>
        public List<UnityEngine.Camera> outputCameras { get; protected set; } = new List<UnityEngine.Camera>();

        /// <summary>
        /// Access to the primary camera used to output the captured display.
        /// </summary>
        public UnityEngine.Camera primaryOutputCamera => outputCameras[0];

        #region Config Helpers
        /// <summary>
        /// The type of display, as a string. Can be user defined using the CustomDisplayAttribute.
        /// </summary>
        public string type => config.type;

        /// <summary>
        /// The ID of the display.
        /// </summary>
        public string id => config.id;

        /// <summary>
        /// Access to the display's viewport config, or null if no custom viewport is set.
        /// </summary>
        public Config.Viewport viewport => config.viewport;

        /// <summary>
        /// A 3D integer-based "index" for the display. This is entirely user-defined and can be used to identifying displays within a grid layout.
        /// </summary>
        public Vector3Int index => config.index;

        /// <summary>
        /// The layermask for the display's cameras.
        /// </summary>
        public LayerMask layerMask => config.layerMask;

        /// <summary>
        /// An optional transform that is applied to the display's cameras and GameObjects when it is initialised.
        /// </summary>
        public Config.Transform transform => config.transform;

        /// <summary>
        /// The Field-of-View for the display.
        /// </summary>
        public float fov => config.fov;

        /// <summary>
        /// The aspect scale for the display. This is useful for displays that use a single projector/screen for multiple displays (or stereoscopic) and need to adjust the aspect to combat squashed aspects.
        /// </summary>
        public float aspectScale => config.aspectScale;

        #region Stereo Config Wrappers
        /// <summary>
        /// Flag specifying if the display should initialise separate Unity Cameras for each eye, or use a single camera with its stereo eye set to "Both".
        /// Will use the data from the StereoConfig, or an overridden value.
        /// </summary>
    //    public bool separateEyes { get { return config.separateEyes; } }

        /// <summary>
        /// The stereo mode that this display uses.
        /// Will use the data from the StereoConfig, or an overridden value.
        /// </summary>
        public StereoMode stereoMode { get { return config.stereoMode; } }

        /// <summary>
        /// THe stereo alignment for this screen. Either Screen-aligned or Camera-aligned.
        /// Will use the data from the StereoConfig, or an overridden value.
        /// </summary>
        public StereoAlignment stereoAlignment { get { return config.stereoAlignment; } }

        /// <summary>
        /// The eye separation for this display's cameras.
        /// Will use the data from the StereoConfig, or an overridden value.
        /// </summary>
        public float eyeSeparation { get { return config.eyeSeparation; } }

        /// <summary>
        /// Does this display require two cameras to capture the scene.
        /// </summary>
        public bool requiresDualCameras { get { return config.requiresDualCameras; } }

        /// <summary>
        /// Should this display swap the stereo eyes, i.e. left becomes right, right becomes left.
        /// </summary>
        public bool swapEyes { get { return config.swapEyes; } }
        #endregion

        /// <summary>
        /// The current platform's StereoConfig object.
        /// </summary>
        public Config.Stereo stereoConfig => config.stereoConfig;

        /// <summary>
        /// The display adapter (connected output monitor/projector) that this display outputs to.
        /// </summary>
        public int monitor => config.monitor;

        /// <summary>
        /// The near plane distance that will be used for the cameras this display requires. Default = -1 (use the values from the Unity scene instead)
        /// </summary>
        public float nearClip => config.nearClip;

        /// <summary>
        /// The far plane distance that will be used for the cameras this display requires. Default = -1 (use the values from the Unity scene instead)
        /// </summary>
        public float farClip => config.farClip;
        /// <summary>
        /// When using custom warp you can specify the location of the warp data that the projectors will use.
        /// </summary>
        public string warpPath => config.warpPath;

        /// <summary>
        /// When using custom projector blending you can specify the location of the blend information that the projectors will use.
        /// </summary>
        public string blendPath => config.blendPath;
        #endregion

        /// <summary>
        /// Access to the viewport rectangle that this display uses.
        /// </summary>
        public Rect screenRect
        {
            get
            {
                if (viewport != null)
                    return viewport.screenRect;
                else
                    return new Rect(0, 0, 1, 1);
            }
        }

        /// <summary>
        /// Constructs a display based off of a config entry.
        /// </summary>
        /// <param name="config">Config settings for this display.</param>
        public Display(Config.Display config)
        {
            this.config = config;
        }

        /// <summary>
        /// Display desctructor.
        /// </summary>
        ~Display()
        {
            if (gameObject)
                GameObject.DestroyImmediate(gameObject);
        }

        /// <summary>
        /// Clones this display.
        /// </summary>
        /// <returns>A cloned copy of this display.</returns>
        public abstract Display Clone();

        /// <summary>
        /// Initialises a GameObject for the display then configures required componenents and behaviours.
        /// </summary>
        internal void InitialiseDisplay(GameObject gameObject = null)
        {
            if (this.gameObject)
                GameObject.DestroyImmediate(this.gameObject);

            if (gameObject)
                this.gameObject = gameObject;
            else
            {
                this.gameObject = new GameObject(config.id);
                this.gameObject.transform.SetParent(Camera.main.transform, false);
            }
            
            if (config.transform != null)
            {
                if (config.transform.HasTranslation)
                    this.gameObject.transform.localPosition = config.transform.Translation;
                if (config.transform.HasRotation)
                    this.gameObject.transform.localRotation = config.transform.Rotation;
            }

            ConfigureDisplayForScene();

            // make sure all cameras are enabled
            foreach (var c in captureCameras) c.enabled = true;
            foreach (var c in outputCameras) c.enabled = true;
        }

        /// <summary>
        /// Method for setting up the custom display.
        /// </summary>
        public abstract void ConfigureDisplayForScene();

        /// <summary>
        /// A method to cast a world-space ray into the screen to find a world-space hit, along with a 2D display-space point in the display.
        /// </summary>
        /// <param name="ray">The world-space ray.</param>
        /// <param name="distance">Sets distance to the distance from the ray to the intersection point, if there is one.</param>
        /// <param name="hitPoint2D">The 2D display-space position that the ray intersects the display, if it intersects.</param>
        /// <returns>Returns true if the world-space ray intersects the display, and false if it does not.</returns>
        public abstract bool Raycast(Ray ray, out float distance, out Vector2 hitPoint2D);

        /// <summary>
        /// Get a world-space ray from the display via a 2D display-space point.
        /// </summary>
        /// <param name="displaySpacePoint">The 2D display-space point to get the ray from, which is in [0,1] range for each axis.</param>
        /// <returns>Returns the world-space ray that passes through the display-space point.</returns>
        public abstract Ray ViewportPointToRay(Vector2 displaySpacePoint);

        /// <summary>
        /// Draw a Gizmo for the specified display config.
        /// </summary>
        /// <param name="config">The display to display.</param>
        public static void DrawGizmoForDisplay(Config.Display config)
        {
            if (registeredDisplayTypes.ContainsKey(config.type))
                registeredDisplayTypes[config.type].GetMethod("DrawGizmo").Invoke(null, new object[] { config });
        }

        /// <summary>
        /// A method to gather a triangle mesh to represent the surface of the display.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="uvs"></param>
        public abstract void GatherDisplayGeometry(List<Vector3> vertices, List<int> indices, List<Vector2> uvs);

        /// <summary>
        /// A method for setting the camera clear flags for this display's cameras.
        /// </summary>
        /// <param name="flags">Clear flag to set.</param>
        public abstract void SetClearFlags(CameraClearFlags flags);

        /// <summary>
        /// A method for setting the background clear colour for this display's cameras.
        /// </summary>
        /// <param name="colour">Colour to use.</param>
        public abstract void SetBackgroundColour(Color colour);

        /// <summary>
        /// Get a world-space ray from the display via a 2D view-space point.
        /// </summary>
        /// <param name="viewspacePoint">The 2D view-space point to get the ray from.</param>
        /// <param name="ray">The world-space ray that passes through the display-space point.</param>
        /// <returns>Returns true if the view-space point is over top of this display, false otherwise.</returns>
        public bool ViewportPointToRay(Vector2 viewspacePoint, ref Ray ray)
        {
            // ensure within total viewport
            if (config.viewport != null)
            {
                if (config.viewport.absolute)
                {
                    viewspacePoint.x *= Screen.width;
                    viewspacePoint.y *= Screen.height;
                }

                viewspacePoint = (viewspacePoint - config.viewport.xy) / config.viewport.dimensions;
            }

            if (viewspacePoint.x < 0 ||
                viewspacePoint.y < 0 ||
                viewspacePoint.x > 1.0f ||
                viewspacePoint.y > 1.0f)
            {
                return false;
            }

            // stereo?
            if (config.stereoMode == StereoMode.TopBottom)
                viewspacePoint.y = (viewspacePoint.y % 0.5f) / 0.5f;
            else if (config.stereoMode == StereoMode.SideBySide)
                viewspacePoint.x = (viewspacePoint.x % 0.5f) / 0.5f;  

            // it's within the viewport, so we can output a ray
            ray = ViewportPointToRay(viewspacePoint);
            return true;
        }

        /// <summary>
        /// Creates a UnityEngine.Mesh from the display's geometry.
        /// </summary>
        /// <returns>Returns the generated Mesh object.</returns>
        public Mesh GenerateMeshForDisplay()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();

            GatherDisplayGeometry(vertices, indices, uvs);

            if (vertices.Count > 0)
            {
                for (int i = 0; i < vertices.Count; ++i)
                {
                    uvs[i] = uvs[i] + new Vector2(config.index.x, config.index.y);
                    vertices[i] = config.transform.TransformPoint(vertices[i]);
                }

                Mesh mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.RecalculateNormals();
                return mesh;
            }

            return null;
        }
    }
}
