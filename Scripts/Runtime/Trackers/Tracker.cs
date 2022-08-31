using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace HEVS
{
    /// <summary>
    /// Attribute used for creating custom tracker types that can be loaded from HEVS JSON configuration files.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomTrackerAttribute : Attribute
    {
        /// <summary>
        /// Name of the type read from the JSON configuration file.
        /// </summary>
        public string typeName;

        /// <summary>
        /// Constructs a custom tracker with a set type name.
        /// </summary>
        /// <param name="typeName">The type name to use.</param>
        public CustomTrackerAttribute(string typeName) { this.typeName = typeName; }
    }

    /// <summary>
    /// A HEVS component used to apply tracking to a GameObject.
    /// It will first use its ID to check the HEVS platform config for a matching tracker and then use 
    /// the settings from the config file. If it is unable to find a matching tracker then it will 
    /// fall back to the component values.
    /// </summary>
	[AddComponentMenu("HEVS/Tracker")]
	[RequireComponent(typeof(ClusterObject))]
	public class Tracker : MonoBehaviour
    {
        internal static Dictionary<string, Type> registeredTrackerDevices = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        static Trackers.TrackerDevice CreateDevice(string type, params object[] args)//Tracker owner)
        {
            if (registeredTrackerDevices.ContainsKey(type))
                return (Trackers.TrackerDevice)Activator.CreateInstance(registeredTrackerDevices[type], args);
            return null;
        }

        internal static void GatherCustomTrackerTypes()
        {
            // find and register all custom tracker types
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                                 .Where(x => typeof(Trackers.TrackerDevice).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            foreach (Type type in types)
            {
                CustomTrackerAttribute attrib = (CustomTrackerAttribute)Attribute.GetCustomAttribute(type, typeof(CustomTrackerAttribute));
                if (attrib != null)
                {
                    if (registeredTrackerDevices.ContainsKey(attrib.typeName))
                        Debug.LogError("HEVS: Tracker type already registered for type [" + attrib.typeName + "]! New definition will be ignored. This can cause unknown behaviour!");
                    else
                        registeredTrackerDevices.Add(attrib.typeName, type);
                }
            }
        }

        Trackers.TrackerDevice device;

        /// <summary>
        /// Access to the config used to create this tracker.
        /// </summary>
		public Config.Tracker config { get; private set; }

        /// <summary>
        /// The ID of the tracker to search for within the HEVS platform config.
        /// </summary>
		public string configId;

        /// <summary>
        /// Should this GameOBject be deactivated if it cannot find a matching tracker ID within the config file?
        /// </summary>
		public bool disableIfNotFound = false;

        /// <summary>
        /// Default tracker types supported by HEVS.
        /// </summary>
        public enum DefaultType
        {
            /// <summary>
            /// Mouse tracker type.
            /// </summary>
            Mouse,
            /// <summary>
            /// XR/VR tracker type.
            /// </summary>
            XR,
            /// <summary>
            /// OSC tracker type.
            /// </summary>
            OSC,
            /// <summary>
            /// VRPN tracker type.
            /// </summary>
            VRPN
        }

        [SerializeField]
        DefaultType _defaultType = DefaultType.Mouse;

        /// <summary>
        /// The default tracker type to use if the matching config entry cannot be found.
        /// </summary>
        public DefaultType defaultType { get { return _defaultType; } }

        [SerializeField]
        string _address = string.Empty;

        /// <summary>
        /// The address used by the tracker (i.e. name@IP for VRPN, etc)
        /// </summary>
        public string address { get { return config == null ? _address : config.address; } }

        [SerializeField]
        int _port = 7890;

        /// <summary>
        /// The port that OSC will listen on.
        /// </summary>
        public int port { get { return config == null ? _port : config.port; } }

        [SerializeField]
        int _transformFlags = (int)TransformFlags.All;

        /// <summary>
        /// Flags to control which parts of the transform will synchronise.
        /// </summary>
        public int transformFlags { get { return config == null ? _transformFlags : config.transformFlags; } }

        /// <summary>
        /// Specifies which tracker axis maps to Unity's "right" axis.
        /// </summary>
        public TrackerAxis right { get { return config == null ? _right : config.right; } }
        [SerializeField]
        TrackerAxis _right = TrackerAxis.X;

        /// <summary>
        /// Specifies which tracker axis maps to Unity's "up" axis.
        /// </summary>
        public TrackerAxis up { get { return config == null ? _up : config.up; } }
        [SerializeField]
        TrackerAxis _up = TrackerAxis.Y;

        /// <summary>
        /// Specifies which tracker axis maps to Unity's "forward" axis.
        /// </summary>
        public TrackerAxis forward { get { return config == null ? _forward : config.forward; } }
        [SerializeField]
        TrackerAxis _forward = TrackerAxis.Z;

        /// <summary>
        /// Specifies the coordinate space handedness that the tracker uses. Either left-handed or right-handed.
        /// </summary>
        public TrackerHandedness handedness { get { return config == null ? _handedness : config.handedness; } }
        [SerializeField]
        TrackerHandedness _handedness = TrackerHandedness.Left;

        /// <summary>
        /// Specifies the XR device to track.
        /// </summary>
        public XRNode xrNode { get { return config == null ? _xrNode : config.xrNode; } }
        [SerializeField]
        XRNode _xrNode = XRNode.TrackingReference;

        /// <summary>
        /// Specifies an optional transform to apply to the received tracker transform before applying it to the Unity GameObject's Transform.
        /// </summary>
        public Config.Transform offsetTransform => _offsetTransform;// { get { return _offsetTransform; } set { _offsetTransform = value; } }
        [SerializeField]
        Config.Transform _offsetTransform = Config.Transform.identity;

        /// <summary>
        /// Specifies if redumentary tracker smoothing should be applied to the tracker. This may help noisy tracker systems, but does add latency.
        /// </summary>
        public bool smoothing { get { return config == null ? _smoothing : config.smoothing; } }
        [SerializeField]
        bool _smoothing = false;

        /// <summary>
        /// A multiplier that can be used to speed up a smoothed tracker.
        /// </summary>
        public float smoothMultiplier { get { return config == null ? _smoothMultiplier : config.smoothMultiplier; } }
        [SerializeField]
        float _smoothMultiplier = 1;

        /// <summary>
        /// Specifies if the tracker should always fallback to the mouse within the editor.
        /// </summary>
		public bool forceMouseInEditor = false;

        /// <summary>
        /// Is the tracker updating on the master only.
        /// </summary>
        public bool masterOnly = true;

        void OnDestroy()
        {         
            if (device != null)
                device.Release();
            device = null;
        }

        void Start()
		{
            if (masterOnly &&
                !Cluster.isMaster)
                return;

            // does it exist in the config?
            Config.Tracker config = null;
            Core.activePlatform.trackers.TryGetValue(configId, out config);

            this.config = config;

            if (config != null)
                this._offsetTransform = config.transform;

            device = CreateDevice((Application.isEditor && forceMouseInEditor) ? "Mouse" : (config != null ? config.type : defaultType.ToString()), this);

            if (device != null)
                device.Initialise();
        }

		void Update()
        {
            if (device != null)
                device.Update();
        }
    }
}