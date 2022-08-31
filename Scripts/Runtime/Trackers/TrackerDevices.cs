using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HEVS.Trackers
{
    /// <summary>
    /// Base class for custom trackers.
    /// </summary>
    public abstract class TrackerDevice
    {
        /// <summary>
        /// The owner of this device.
        /// </summary>
        public Tracker owner { get; private set; }

        internal TrackerDevice(Tracker owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Method to initialise the device.
        /// </summary>
        /// <returns>Returns true if the device was successfully initialised.</returns>
        public abstract bool Initialise();
        /// <summary>
        /// Method to cleanup the device on shutdown.
        /// </summary>
        public abstract void Release();
        /// <summary>
        /// Method to update the state of the device.
        /// </summary>
        public abstract void Update();
    }

    /// <summary>
    /// Mouse tracker device.
    /// </summary>
    [CustomTracker("mouse")]
    public class MouseTrackerDevice : TrackerDevice
    {
        /// <summary>
        /// Constructs a mouse tracker device.
        /// </summary>
        /// <param name="owner">Owner of this device.</param>
        public MouseTrackerDevice(Tracker owner) : base(owner) { }

        /// <summary>
        /// Method to initialise the device.
        /// </summary>
        /// <returns>Returns true if the device was successfully initialised.</returns>
        public override bool Initialise()
        {
            return true;
        }

        /// <summary>
        /// Method to cleanup the device on shutdown.
        /// </summary>
        public override void Release()
        {

        }

        /// <summary>
        /// Updates the device based on the mouse cursor position and screen ray.
        /// </summary>
        public override void Update()
        {
            Ray screenRay = Core.ScreenPointToRay(Input.mousePosition);

            owner.transform.position = screenRay.origin;
            owner.transform.forward = screenRay.direction.normalized;
        }
    }

    /// <summary>
    /// XR tracker device, such as a VR controller.
    /// </summary>
    [CustomTracker("xr")]
    public class XRTrackerDevice : TrackerDevice
    {
        Filter.OneEuroFilterVector3 tFilter;
        Filter.OneEuroFilterQuaternion rFilter;

        /// <summary>
        /// Create the tracker device.
        /// </summary>
        /// <param name="owner">Owner of this device.</param>
        public XRTrackerDevice(Tracker owner) : base(owner)
        {
        }

        /// <summary>
        /// Initialise the device.
        /// </summary>
        /// <returns>Returns true if the device is successfully initialised.</returns>
        public override bool Initialise()
        {
            if (owner.smoothing && IsNodeValid())
            {
                (var position, var rotation) = GetTrackerState();
                tFilter = new Filter.OneEuroFilterVector3(position, 1, 0.007f, 1);
                rFilter = new Filter.OneEuroFilterQuaternion(rotation, 1, 0.007f, 1);
            }
            return true;
        }

        /// <summary>
        /// Method to cleanup the device on shutdown.
        /// </summary>
        public override void Release()
        {

        }

        bool IsNodeValid()
        {
            List<XRNodeState> nodes = new List<XRNodeState>();
            InputTracking.GetNodeStates(nodes);

            return nodes.Exists(s => s.nodeType == owner.xrNode);
        }

        (Vector3 position, Quaternion rotation) GetTrackerState()
        {
            List<XRNodeState> nodes = new List<XRNodeState>();
            InputTracking.GetNodeStates(nodes);

            if (nodes.Exists(s => s.nodeType == owner.xrNode))
            {
                var state = nodes.Find(s => s.nodeType == owner.xrNode);

                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;

                state.TryGetPosition(out position);
                state.TryGetRotation(out rotation);

                return (position, rotation);
            }

            return (Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Update this device.
        /// </summary>
        public override void Update()
        {
            List<XRNodeState> nodes = new List<XRNodeState>();
            InputTracking.GetNodeStates(nodes);

            if (nodes.Exists(s => s.nodeType == owner.xrNode))
            {
                var state = nodes.Find(s => s.nodeType == owner.xrNode);

                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;

                state.TryGetPosition(out position);
                state.TryGetRotation(out rotation);

                if (XRSettings.loadedDeviceName == "Oculus")
                {
                    var headState = nodes.Find(s => s.nodeType == XRNode.Head);
                    Vector3 headPosition = Vector3.zero;
                    headState.TryGetPosition(out headPosition);

                    Vector3 nodeDirection = position - headPosition;

                    Transform cameraTransform = HEVS.Camera.main.transform;
                    position = cameraTransform.position + SceneOrigin.gameObject.transform.TransformDirection(nodeDirection);
                    rotation = SceneOrigin.gameObject.transform.rotation * rotation;
                }
                else
                {
                    position = SceneOrigin.gameObject.transform.TransformPoint(position);
                    rotation = SceneOrigin.gameObject.transform.rotation * rotation;
                }

                Config.Transform td = new Config.Transform(position, rotation, Vector3.one);
                td = owner.offsetTransform.PostConcatenate(td);

                if ((owner.transformFlags & (int)TransformFlags.Position) != 0)
                {
                    if (owner.smoothing)
                        owner.transform.localPosition = tFilter.Filter(td.Translation, Time.deltaTime);
                    else
                        owner.transform.localPosition = td.Translation;
                }

                if ((owner.transformFlags & (int)TransformFlags.Rotation) != 0)
                {
                    if (owner.smoothing)
                        owner.transform.localRotation = rFilter.Filter(td.Rotation, Time.deltaTime);
                    else
                        owner.transform.localRotation = td.Rotation;
                }
            }
        }
    }

    /// <summary>
    /// A tracker device whos data comes from Virtual Reality Peripheral Network (VRPN) packets.
    /// </summary>
    [CustomTracker("vrpn")]
    public class VRPNTrackerDevice : TrackerDevice
    {
        VRPN.IDevice tracker;

        Filter.OneEuroFilterVector3 tFilter;
        Filter.OneEuroFilterQuaternion rFilter;

        /// <summary>
        /// Create the tracker device.
        /// </summary>
        /// <param name="owner">Owner of this device.</param>
        public VRPNTrackerDevice(Tracker owner) : base(owner)
        {
        }

        /// <summary>
        /// Initialise the device.
        /// </summary>
        /// <returns>Returns true if the device is successfully initialised.</returns>
        public override bool Initialise()
        {
            if (!string.IsNullOrWhiteSpace(owner.address))
            {
                tracker = VRPN.AddOrGetDevice(owner.address);

                if (tracker != null &&
                    owner.smoothing)
                {
                    tFilter = new Filter.OneEuroFilterVector3(tracker.position, 1, 0.007f, 1);
                    rFilter = new Filter.OneEuroFilterQuaternion(tracker.rotation, 1, 0.007f, 1);
                }
            }

            return true;
        }

        /// <summary>
        /// Method to cleanup the device on shutdown.
        /// </summary>
        public override void Release()
        {

        }

        /// <summary>
        /// Update this device.
        /// </summary>
        public override void Update()
        {
            if (tracker != null)
            {
                Vector3 p = Vector3.zero;
                Quaternion r = Quaternion.identity;

                if ((owner.transformFlags & (int)TransformFlags.Position) != 0)
                {
                    switch (owner.right)
                    {
                        case TrackerAxis.X: p.x = tracker.position.x; break;
                        case TrackerAxis.Y: p.x = tracker.position.y; break;
                        case TrackerAxis.Z: p.x = tracker.position.z; break;
                        case TrackerAxis.NEG_X: p.x = -tracker.position.x; break;
                        case TrackerAxis.NEG_Y: p.x = -tracker.position.y; break;
                        case TrackerAxis.NEG_Z: p.x = -tracker.position.z; break;
                    }
                    switch (owner.up)
                    {
                        case TrackerAxis.X: p.y = tracker.position.x; break;
                        case TrackerAxis.Y: p.y = tracker.position.y; break;
                        case TrackerAxis.Z: p.y = tracker.position.z; break;
                        case TrackerAxis.NEG_X: p.y = -tracker.position.x; break;
                        case TrackerAxis.NEG_Y: p.y = -tracker.position.y; break;
                        case TrackerAxis.NEG_Z: p.y = -tracker.position.z; break;
                    }
                    switch (owner.forward)
                    {
                        case TrackerAxis.X: p.z = tracker.position.x; break;
                        case TrackerAxis.Y: p.z = tracker.position.y; break;
                        case TrackerAxis.Z: p.z = tracker.position.z; break;
                        case TrackerAxis.NEG_X: p.z = -tracker.position.x; break;
                        case TrackerAxis.NEG_Y: p.z = -tracker.position.y; break;
                        case TrackerAxis.NEG_Z: p.z = -tracker.position.z; break;
                    }
                }

                if ((owner.transformFlags & (int)TransformFlags.Rotation) != 0)
                {
                    switch (owner.right)
                    {
                        case TrackerAxis.X: r.x = tracker.rotation.x; break;
                        case TrackerAxis.Y: r.x = tracker.rotation.y; break;
                        case TrackerAxis.Z: r.x = tracker.rotation.z; break;
                        case TrackerAxis.NEG_X: r.x = -tracker.rotation.x; break;
                        case TrackerAxis.NEG_Y: r.x = -tracker.rotation.y; break;
                        case TrackerAxis.NEG_Z: r.x = -tracker.rotation.z; break;
                    }
                    switch (owner.up)
                    {
                        case TrackerAxis.X: r.y = tracker.rotation.x; break;
                        case TrackerAxis.Y: r.y = tracker.rotation.y; break;
                        case TrackerAxis.Z: r.y = tracker.rotation.z; break;
                        case TrackerAxis.NEG_X: r.y = -tracker.rotation.x; break;
                        case TrackerAxis.NEG_Y: r.y = -tracker.rotation.y; break;
                        case TrackerAxis.NEG_Z: r.y = -tracker.rotation.z; break;
                    }
                    switch (owner.forward)
                    {
                        case TrackerAxis.X: r.z = tracker.rotation.x; break;
                        case TrackerAxis.Y: r.z = tracker.rotation.y; break;
                        case TrackerAxis.Z: r.z = tracker.rotation.z; break;
                        case TrackerAxis.NEG_X: r.z = -tracker.rotation.x; break;
                        case TrackerAxis.NEG_Y: r.z = -tracker.rotation.y; break;
                        case TrackerAxis.NEG_Z: r.z = -tracker.rotation.z; break;
                    }
                    if (owner.handedness == TrackerHandedness.Right)
                        r.w = -tracker.rotation.w;
                    else
                        r.w = tracker.rotation.w;
                }

                Config.Transform td = new Config.Transform(p,r,Vector3.one);
                td = owner.offsetTransform.Concatenate(td);

                if ((owner.transformFlags & (int)TransformFlags.Position) != 0)
                {
                    if (owner.smoothing)
                        owner.transform.localPosition = tFilter.Filter(td.Translation, Time.deltaTime);
                    else
                        owner.transform.localPosition = td.Translation;
                }

                if ((owner.transformFlags & (int)TransformFlags.Rotation) != 0)
                {
                    if (owner.smoothing)
                        owner.transform.localRotation = rFilter.Filter(td.Rotation, Time.deltaTime);
                    else
                        owner.transform.localRotation = td.Rotation;
                }
            }
        }
    }

    /// <summary>
    /// An tracker device whos data comes from OpenSoundControl (OSC) packets.
    /// </summary>
    [CustomTracker("osc")]
    public class OSCTrackerDevice : TrackerDevice
    {
        /// <summary>
        /// Create the tracker device.
        /// </summary>
        /// <param name="owner">Owner of this device.</param>
        public OSCTrackerDevice(Tracker owner) : base(owner) { }

        Filter.OneEuroFilterVector3 tFilter;
        Filter.OneEuroFilterQuaternion rFilter;

        static Dictionary<int, OSCReceiver> receivers = new Dictionary<int, OSCReceiver>();

        /// <summary>
        /// Initialise the device.
        /// </summary>
        /// <returns>Returns true if the device is successfully initialised.</returns>
        public override bool Initialise()
        {
            // is there an open receiver for the port?
            if (receivers.ContainsKey(owner.port))
            {
                // add this receiver for the id
                receivers[owner.port].receivers.Add(owner.configId, this);
            }
            else
            {
                // add a new receiver for the port
                OSCReceiver receiver = new OSCReceiver()
                {
                    osc = new OSCsharp.Net.UDPReceiver(owner.port, false),
                    receivers = new Dictionary<string, OSCTrackerDevice>()
                };
                receiver.osc.MessageReceived += receiver.OnMessageReceived;
                receivers.Add(owner.port, receiver);

                // add this receiver for the id
                receiver.receivers.Add(owner.configId, this);

                // start receiving
                receiver.osc.Start();
            }

            if (owner.smoothing)
            {
                tFilter = new Filter.OneEuroFilterVector3(owner.transform.localPosition, 1, 0.007f, 1);
                rFilter = new Filter.OneEuroFilterQuaternion(owner.transform.localRotation, 1, 0.007f, 1);
            }
            return true;
        }

        /// <summary>
        /// Method to cleanup the device on shutdown.
        /// </summary>
        public override void Release()
        {
            // remove from our receiver
            if (receivers.ContainsKey(owner.port))
            {
                receivers[owner.port].receivers.Remove(owner.configId);

                // if no more receivers on port then kill the OSC receiver
                if (receivers[owner.port].receivers.Count == 0)
                {
                    receivers[owner.port].osc.Stop();
                    receivers.Remove(owner.port);
                }
            }
        }

        /// <summary>
        /// Update this device.
        /// </summary>
        public override void Update()
        {

        }

        struct OSCReceiver
        {
            public OSCsharp.Net.UDPReceiver osc;
            public Dictionary<string, OSCTrackerDevice> receivers;

            public void OnMessageReceived(object sender, OSCsharp.Net.OscMessageReceivedEventArgs args)
            {
                if (args.Message == null)
                    return;

                foreach (var pair in receivers)
                {
                    if (args.Message.Address.StartsWith("/" + pair.Key))
                    {
                        pair.Value.OnMessageReceived(sender, args);
                    }
                }
            }
        }

        void OnMessageReceived(object sender, OSCsharp.Net.OscMessageReceivedEventArgs args)
        {
            if (args.Message == null)
                return;

            // strip our ID from the address
            string address = args.Message.Address.Replace("/" + owner.configId, "");

            switch (address)
            {
                case "/transform":
                    {
                        SetPosition(new Vector3((float)args.Message.Data[0], (float)args.Message.Data[1], (float)args.Message.Data[2]));
                        SetRotation(Quaternion.Euler((float)args.Message.Data[3], (float)args.Message.Data[4], (float)args.Message.Data[5]));
                        SetScale(new Vector3((float)args.Message.Data[6], (float)args.Message.Data[7], (float)args.Message.Data[8]));
                    }
                    break;
                case "/position":
                    {
                        SetPosition(new Vector3((float)args.Message.Data[0], (float)args.Message.Data[1], (float)args.Message.Data[2]));
                    }
                    break;
                case "/rotation":
                    {
                        SetRotation(Quaternion.Euler((float)args.Message.Data[0], (float)args.Message.Data[1], (float)args.Message.Data[2]));
                    }
                    break;
                case "/scale":
                    {
                        SetScale(new Vector3((float)args.Message.Data[0], (float)args.Message.Data[1], (float)args.Message.Data[2]));
                    }
                    break;
            }
        }

        void SetPosition(Vector3 position)
        {
            if ((owner.transformFlags & (int)TransformFlags.Position) != 0)
            {
                Vector3 p = Vector3.zero;

                switch (owner.right)
                {
                    case TrackerAxis.X: p.x = position.x; break;
                    case TrackerAxis.Y: p.x = position.y; break;
                    case TrackerAxis.Z: p.x = position.z; break;
                    case TrackerAxis.NEG_X: p.x = -position.x; break;
                    case TrackerAxis.NEG_Y: p.x = -position.y; break;
                    case TrackerAxis.NEG_Z: p.x = -position.z; break;
                }
                switch (owner.up)
                {
                    case TrackerAxis.X: p.y = position.x; break;
                    case TrackerAxis.Y: p.y = position.y; break;
                    case TrackerAxis.Z: p.y = position.z; break;
                    case TrackerAxis.NEG_X: p.y = -position.x; break;
                    case TrackerAxis.NEG_Y: p.y = -position.y; break;
                    case TrackerAxis.NEG_Z: p.y = -position.z; break;
                }
                switch (owner.forward)
                {
                    case TrackerAxis.X: p.z = position.x; break;
                    case TrackerAxis.Y: p.z = position.y; break;
                    case TrackerAxis.Z: p.z = position.z; break;
                    case TrackerAxis.NEG_X: p.z = -position.x; break;
                    case TrackerAxis.NEG_Y: p.z = -position.y; break;
                    case TrackerAxis.NEG_Z: p.z = -position.z; break;
                }

                //    owner.transform.localPosition = owner.offsetTransform.TransformPoint(p);
                if (owner.smoothing)
                {
                    //   owner.transform.localPosition = tFilter.Filter(td.translate, Time.deltaTime);
                    owner.transform.localPosition = tFilter.Filter(owner.offsetTransform.TransformPoint(p), Time.deltaTime);
                }
                else
                    owner.transform.localPosition = owner.offsetTransform.TransformPoint(p);
            }
        }

        void SetRotation(Quaternion rotation)
        {
            if ((owner.transformFlags & (int)TransformFlags.Rotation) != 0)
            {
                Quaternion r = Quaternion.identity;

                switch (owner.right)
                {
                    case TrackerAxis.X: r.x = rotation.x; break;
                    case TrackerAxis.Y: r.x = rotation.y; break;
                    case TrackerAxis.Z: r.x = rotation.z; break;
                    case TrackerAxis.NEG_X: r.x = -rotation.x; break;
                    case TrackerAxis.NEG_Y: r.x = -rotation.y; break;
                    case TrackerAxis.NEG_Z: r.x = -rotation.z; break;
                }
                switch (owner.up)
                {
                    case TrackerAxis.X: r.y = rotation.x; break;
                    case TrackerAxis.Y: r.y = rotation.y; break;
                    case TrackerAxis.Z: r.y = rotation.z; break;
                    case TrackerAxis.NEG_X: r.y = -rotation.x; break;
                    case TrackerAxis.NEG_Y: r.y = -rotation.y; break;
                    case TrackerAxis.NEG_Z: r.y = -rotation.z; break;
                }
                switch (owner.forward)
                {
                    case TrackerAxis.X: r.z = rotation.x; break;
                    case TrackerAxis.Y: r.z = rotation.y; break;
                    case TrackerAxis.Z: r.z = rotation.z; break;
                    case TrackerAxis.NEG_X: r.z = -rotation.x; break;
                    case TrackerAxis.NEG_Y: r.z = -rotation.y; break;
                    case TrackerAxis.NEG_Z: r.z = -rotation.z; break;
                }
                if (owner.handedness == TrackerHandedness.Right)
                    r.w = -rotation.w;
                else
                    r.w = rotation.w;

                //owner.transform.localRotation = owner.offsetTransform.TransformRotation(r);
                if (owner.smoothing)
                {
                    //   owner.transform.localPosition = tFilter.Filter(td.translate, Time.deltaTime);
                    owner.transform.localRotation = rFilter.Filter(owner.offsetTransform.TransformRotation(r), Time.deltaTime);
                }
                else
                    owner.transform.localRotation = owner.offsetTransform.TransformRotation(r);
            }
        }

        void SetScale(Vector3 scale)
        {
            if ((owner.transformFlags & (int)TransformFlags.Scale) != 0)
            {
                Vector3 s = Vector3.one;

                switch (owner.right)
                {
                    case TrackerAxis.X: s.x = scale.x; break;
                    case TrackerAxis.Y: s.x = scale.y; break;
                    case TrackerAxis.Z: s.x = scale.z; break;
                    case TrackerAxis.NEG_X: s.x = -scale.x; break;
                    case TrackerAxis.NEG_Y: s.x = -scale.y; break;
                    case TrackerAxis.NEG_Z: s.x = -scale.z; break;
                }
                switch (owner.up)
                {
                    case TrackerAxis.X: s.y = scale.x; break;
                    case TrackerAxis.Y: s.y = scale.y; break;
                    case TrackerAxis.Z: s.y = scale.z; break;
                    case TrackerAxis.NEG_X: s.y = -scale.x; break;
                    case TrackerAxis.NEG_Y: s.y = -scale.y; break;
                    case TrackerAxis.NEG_Z: s.y = -scale.z; break;
                }
                switch (owner.forward)
                {
                    case TrackerAxis.X: s.z = scale.x; break;
                    case TrackerAxis.Y: s.z = scale.y; break;
                    case TrackerAxis.Z: s.z = scale.z; break;
                    case TrackerAxis.NEG_X: s.z = -scale.x; break;
                    case TrackerAxis.NEG_Y: s.z = -scale.y; break;
                    case TrackerAxis.NEG_Z: s.z = -scale.z; break;
                }

                if (owner.offsetTransform.HasScale)
                {
                    s.x *= owner.offsetTransform.Scale.x;
                    s.y *= owner.offsetTransform.Scale.y;
                    s.z *= owner.offsetTransform.Scale.z;
                }

                owner.transform.localScale = s;
            }
        }
    }    
}