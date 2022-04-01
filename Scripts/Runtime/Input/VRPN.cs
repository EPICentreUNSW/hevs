using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Virtual-Reality Peripheral Netwrok (VRPN) access.
    /// </summary>
    public static class VRPN
    {
        /// <summary>
        /// An interface for a VRPN device.
        /// </summary>
        public interface IDevice
        {
            /// <summary>
            /// The name of the device.
            /// (i.e. for Tracker1@192.0.0.1 the name would be Tracker1).
            /// </summary>
            string name { get; }

            /// <summary>
            /// The host address of the device.
            /// (i.e. for Tracker1@192.0.0.1 the host would be 192.0.0.1).
            /// </summary>
            string host { get; }

            /// <summary>
            /// The full combined address of the device.
            /// (i.e. name@host).
            /// </summary>
            string address { get; }
            
            /// <summary>
            /// Access the position of the device.
            /// </summary>
            Vector3 position { get; }

            /// <summary>
            /// Access the orientation of the device.
            /// </summary>
            Quaternion rotation { get; }

            /// <summary>
            /// The stored position and orientation of the device.
            /// </summary>
            Config.Transform transform { get; }

            /// <summary>
            /// Check a button state on the device.
            /// </summary>
            /// <param name="button">The button to check.</param>
            /// <returns>Rturns true if the button is currently pressed, false otherwise.</returns>
            bool GetButton(int button);
            
            /// <summary>
            /// Access an axis value on the device.
            /// </summary>
            /// <param name="axis">The axis to check.</param>
            /// <returns>Returns the current value of the axis.</returns>
            double GetAxis(int axis);

            void UpdateState(int frame);
        }

        static bool vrpnAvailable = true;

        internal static void CheckVRPNAvailable()
        {
            try
            {
                double state = 0;
                vrpnAnalogState("test@test", 0, ref state, Time.frameCount);
            }
            catch (DllNotFoundException)
            {
                Debug.LogWarning("hevs.native: VRPN not available. Is the native plugin missing?");
                vrpnAvailable = false;
            }
        }

        static Dictionary<string, IDevice> devices = new Dictionary<string, IDevice>();

        /// <summary>
        /// 
        /// </summary>
        public static void UpdateDevices()
        {
            foreach (var device in devices)
                device.Value.UpdateState(Time.frameCount);
        }

        /// <summary>
        /// Retrieves an existing device based on its full address.
        /// </summary>
        /// <param name="address">The device to find.</param>
        /// <returns>Returns the found device, or null if not found.</returns>
        public static IDevice GetDevice(string address)
        {
            if (devices.ContainsKey(address))
                return devices[address];
            return null;
        }

        /// <summary>
        /// Adds or retrieves a VRPN device based on its full address.
        /// </summary>
        /// <param name="address">The device to add/get.</param>
        /// <returns>Returns the newly created VRPN device, or the previously added one.</returns>
        public static IDevice AddOrGetDevice(string address)
        {
            if (devices.ContainsKey(address))
                return devices[address];

            Device device = new Device(address);
            devices.Add(address, device);
            return device;
        }

        /// <summary>
        /// Adds or retrieves a VRPN device based on its name and host.
        /// </summary>
        /// <param name="name">The device name to add/get.</param>
        /// <param name="host">The device host to add/get.</param>
        /// <returns>Returns the newly created VRPN device, or the previously added one.</returns>
        public static IDevice AddOrGetDevice(string name, string host)
        {
            string address = name + "@" + host;
            if (devices.ContainsKey(address))
                return devices[address];

            Device device = new Device(address);
            devices.Add(address, device);
            return device;
        }

        // TRACKERS
        [DllImport("hevs.native")]
        private static extern bool vrpnTrackerConnected(string address);

        public static bool IsTrackerConnected(string device, string hostAddress)
        {
            return IsTrackerConnected(device + "@" + hostAddress);
        }

        public static bool IsTrackerConnected(string address)
        {
            if (System.Environment.Is64BitProcess)
                return vrpnTrackerConnected(address);
            else
                return false;
        }

        [DllImport("hevs.native")]
        private static extern bool vrpnTrackerState(string address, ref float x, ref float y, ref float z, ref float rx, ref float ry, ref float rz, ref float rw, int frameCount);

        /// <summary>
        /// Retrieve the state of the tracker based on its name and host.
        /// </summary>
        /// <param name="name">Name of the device.</param>
        /// <param name="host">Host address of the device.</param>
        /// <param name="position">The position variable that will be filled with the position of the device, if it exists.</param>
        /// <param name="rotation">The rotation variable that will be filled with the position of the device, if it exists.</param>
        /// <param name="frame">The current frame index, used to prevent spamming VRPN for updates.</param>
        public static void GetTrackerState(string name, string host, ref Vector3 position, ref Quaternion rotation, int frame)
        {
            if (vrpnAvailable && System.Environment.Is64BitProcess)
                vrpnTrackerState(name + "@" + host, ref position.x, ref position.y, ref position.z,
                            ref rotation.x, ref rotation.y, ref rotation.z, ref rotation.w, frame);
        }

        /// <summary>
        /// Retrieve the state of the tracker based on its address.
        /// </summary>
        /// <param name="address">Full address of the device.</param>
        /// <param name="position">The position variable that will be filled with the position of the device, if it exists.</param>
        /// <param name="rotation">The rotation variable that will be filled with the position of the device, if it exists.</param>
        /// <param name="frame">The current frame index, used to prevent spamming VRPN for updates.</param>
        public static void GetTrackerState(string address, ref Vector3 position, ref Quaternion rotation, int frame)
        {
            if (vrpnAvailable && System.Environment.Is64BitProcess)
                vrpnTrackerState(address, ref position.x, ref position.y, ref position.z,
                            ref rotation.x, ref rotation.y, ref rotation.z, ref rotation.w, frame);
        }

        // BUTTONS
        [DllImport("hevs.native")]
        private static extern bool vrpnButtonConnected(string address);

        public static bool IsButtonConnected(string device, string hostAddress)
        {
            return IsButtonConnected(device + "@" + hostAddress);
        }

        public static bool IsButtonConnected(string address)
        {
            if (System.Environment.Is64BitProcess)
                return vrpnButtonConnected(address);
            else
                return false;
        }

        [DllImport("hevs.native")]
        private static extern bool vrpnButtonState(string address, int button, ref bool state, int frameCount);

        /// <summary>
        /// Get the button state of a VRPN button based on a device name and host address.
        /// </summary>
        /// <param name="name">Name of the device.</param>
        /// <param name="host">Host address of the device.</param>
        /// <param name="button">The index of the button to query.</param>
        /// <returns>Returns true if the button is pressed, false otherwise.</returns>
        public static bool GetButtonState(string name, string host, int button)
        {
            return GetButtonState(name + "@" + host, button);
        }

        /// <summary>
        /// Get the button state of a VRPN button based on a combined device name and host address.
        /// </summary>
        /// <param name="address">Full address of the device.</param>
        /// <param name="button">The index of the button to query.</param>
        /// <returns>Returns true if the button is pressed, false otherwise.</returns>
        public static bool GetButtonState(string address, int button)
        {
            bool state = false;
            if (vrpnAvailable && System.Environment.Is64BitProcess)
                vrpnButtonState(address, button, ref state, Time.frameCount);
            return state;
        }

        // ANALOG
        [DllImport("hevs.native")]
        private static extern bool vrpnAnalogConnected(string address);

        public static bool IsAxisConnected(string device, string hostAddress)
        {
            return IsAnalogConnected(device + "@" + hostAddress);
        }

        public static bool IsAnalogConnected(string address)
        {
            if (System.Environment.Is64BitProcess)
                return vrpnAnalogConnected(address);
            else
                return false;
        }

        [DllImport("hevs.native")]
        private static extern bool vrpnAnalogState(string address, int axis, ref double state, int frameCount);

        /// <summary>
        /// Get the current value of a VRPN axis using a device name and host address.
        /// </summary>
        /// <param name="name">Name of the device.</param>
        /// <param name="host">Host address of the device.</param>
        /// <param name="axis">The index of the axis to query.</param>
        /// <returns>Returns the current value of the axis.</returns>
        public static double GetAxisState(string name, string host, int axis)
        {
            return GetAxisState(name + "@" + host, axis);
        }

        /// <summary>
        /// Get the current value of a VRPN axis using a combined device name and host address.
        /// </summary>
        /// <param name="address">Full address of the device.</param>
        /// <param name="axis">The index of the axis to query.</param>
        /// <returns>Returns the current value of the axis.</returns>
        public static double GetAxisState(string address, int axis)
        {
            double state = 0;

             if (vrpnAvailable && System.Environment.Is64BitProcess)
                vrpnAnalogState(address, axis, ref state, Time.frameCount);

            return state;
        }
        
        internal class Device : IDevice
        {
            public Device(string address)
            {
                this.address = address;
                var split = this.address.Split('@');
                name = split[0];
                host = split[1];
            }
            public Device(string name, string host)
            {
                this.name = name;
                this.host = host;
                address = name + "@" + host;
            }

            Config.Transform _transform = new Config.Transform();

            int _trackerFrame = -1;

            public string name { get; private set; }
            public string host { get; private set; }
            public string address { get; private set; }
            
            public virtual Vector3 position             { get { return _transform.translate; } }
            public virtual Quaternion rotation          { get { return _transform.rotate; } }
            public virtual Config.Transform transform   { get { return _transform; } }

            public void UpdateState(int frame)
            {
                if (frame > _trackerFrame)
                {
                    GetTrackerState(address, ref _transform.translate, ref _transform.rotate, frame);
                    _trackerFrame = frame;
                }
            }

            public bool GetButton(int button) { return GetButtonState(address, button); }
            public double GetAxis(int axis) { return GetAxisState(address, axis); }
        }
    }        
}
