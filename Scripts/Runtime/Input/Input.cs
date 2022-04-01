using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using HEVS.Collections;

namespace HEVS
{
    /// <summary>
    /// Attribute used for creating custom input sources that can be loaded from HEVS JSON configuration files.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomInputSourceAttribute : Attribute
    {
        /// <summary>
        /// Name of the type read from the JSON configuration file.
        /// </summary>
        public string typeName;

        /// <summary>
        /// Constructs a custom input source with a set type name.
        /// </summary>
        /// <param name="typeName">The type name to use.</param>
        public CustomInputSourceAttribute(string typeName) { this.typeName = typeName; }
    }

    /// <summary>
    /// A HEVS Touch object.
    /// </summary>
    public class Touch
    {
        /// <summary>
        /// Construct a Touch object, specifying its source.
        /// </summary>
        /// <param name="source">The source of the touch. Default is "Unity".</param>
        public Touch(string source = "Unity") { this.source = source; }

        /// <summary>
        /// The source of the touch.
        /// </summary>
        public string source;

        /// <summary>
        /// The finger ID of this touch.
        /// </summary>
        public int fingerId { get; set; }

        /// <summary>
        /// The tap count of this touch.
        /// </summary>
        public int tapCount { get; set; }

        /// <summary>
        /// The delta-time of this touch.
        /// </summary>
        public float deltaTime { get; set; }

        /// <summary>
        /// The normalized position of this touch.
        /// </summary>
        public Vector2 position { get; set; }

        /// <summary>
        /// The normalized delta-position of this touch since its last position.
        /// </summary>
        public Vector2 deltaPosition { get; set; }

        /// <summary>
        /// The phase of the touch.
        /// </summary>
        public TouchPhase phase { get; set; }

        /// <summary>
        /// The touch type.
        /// </summary>
        public TouchType type { get; set; }
    }

    /// <summary>
    /// Base class for a HEVS Input Source. Input Sources provide an interface for creating user-defined objects that can query button and axis states
    /// </summary>
    public abstract class InputSource
    {
        /// <summary>
        /// The Config object that created this Input Source.
        /// </summary>
        public Config.InputSource config { get; private set; }

        /// <summary>
        /// Base constructor for an Input Source.
        /// </summary>
        /// <param name="config">The Config object used to create this Input Suorce.</param>
        public InputSource(Config.InputSource config)
        {
            this.config = config;
        }

        /// <summary>
        /// Abstract method for gathering all buttons that are currently "down".
        /// </summary>
        /// <param name="buttons">A list to populate with all currently "down" buttons.</param>
        public abstract void GatherDownButtons(List<string> buttons);

        /// <summary>
        /// Abstract method for gather all non-zero axes and their values.
        /// </summary>
        /// <param name="axes">Collection of axis-value pairs for all non-zero axes.</param>
        public abstract void GatherNonZeroAxes(Dictionary<string, float> axes);
    }

    /// <summary>
    /// An Input Source whose values come from VRPN packets.
    /// </summary>
    [HEVS.CustomInputSource("vrpn")]
    internal class VRPNInputSource : InputSource
    {
        VRPN.IDevice vrpnDevice;

        /// <summary>
        /// Construct the Input Source from a specified Config object.
        /// </summary>
        /// <param name="config">The Config object to use for creation.</param>
        public VRPNInputSource(Config.InputSource config) : base(config)
        {
            vrpnDevice = VRPN.AddOrGetDevice(config.source);
        }

        /// <summary>
        /// Method for gathering all buttons that are currently "down".
        /// </summary>
        /// <param name="buttons">A list to populate with all currently "down" buttons.</param>
        public override void GatherDownButtons(List<string> buttons)
        {
            foreach (var button in config.buttonMappings)
            {
                if (vrpnDevice.GetButton(button.index))
                {
                    foreach (string mapping in button.mappings)
                        if (!buttons.Contains(mapping)) buttons.Add(mapping);
                }
            }
        }

        /// <summary>
        /// Method for gather all non-zero axes and their values.
        /// </summary>
        /// <param name="axes">Collection of axis-value pairs for all non-zero axes.</param>
        public override void GatherNonZeroAxes(Dictionary<string, float> axes)
        {
            foreach (var axis in config.axisMappings)
            {
                double d = vrpnDevice.GetAxis(axis.index);
                if (d != 0)
                {
                    if (axis.inverted)
                        d *= -1;

                    foreach (string mapping in axis.mappings)
                    {
                        if (axes.ContainsKey(mapping))
                            // combine - HACKY
                            axes[mapping] = (axes[mapping] + (float)d) * 0.5f;
                        else
                            axes.Add(mapping, (float)d);
                    }
                }
            }
        }
    }

    /// <summary>
    /// An Input Source whose values come from OSC messages.
    /// </summary>
    [HEVS.CustomInputSource("osc")]
    internal class OSCInputSource : InputSource
    {
        /// <summary>
        /// Construct the Input Source from a specified Config object.
        /// </summary>
        /// <param name="config">The Config object to use for creation.</param>
        public OSCInputSource(Config.InputSource config) : base(config)
        {

        }

        /// <summary>
        /// Method for gathering all buttons that are currently "down".
        /// </summary>
        /// <param name="buttons">A list to populate with all currently "down" buttons.</param>
        public override void GatherDownButtons(List<string> buttons)
        {

        }

        /// <summary>
        /// Method for gather all non-zero axes and their values.
        /// </summary>
        /// <param name="axes">Collection of axis-value pairs for all non-zero axes.</param>
        public override void GatherNonZeroAxes(Dictionary<string, float> axes)
        {

        }
    }

    /// <summary>
    /// HEVS Input wrapper class, responsible for querying input and synchronising it within a cluster.
    /// This class should be used instead of UnityEngine.Input.
    /// </summary>
    public class Input
    {
        static System.Array keyCodes = Enum.GetValues(typeof(KeyCode));
        static List<string> buttons = new List<string>();
        static List<string> axes = new List<string>();

        List<InputSource> alternateSources = new List<InputSource>();

        internal static Dictionary<string, Type> registeredSourceTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        internal void GatherCustomSourceTypes()
        {
            // find and register all custom display data
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                                    .Where(x => typeof(InputSource).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            foreach (Type type in types)
            {
                CustomInputSourceAttribute attrib = (CustomInputSourceAttribute)Attribute.GetCustomAttribute(type, typeof(CustomInputSourceAttribute));
                if (attrib != null)
                {
                    if (registeredSourceTypes.ContainsKey(attrib.typeName))
                        Debug.LogError($"HEVS: Input source type already registered for type [{attrib.typeName}]! New definition will be ignored. This can cause unknown behaviour!");
                    else
                        registeredSourceTypes.Add(attrib.typeName, type);
                }
            }
        }

        /// <summary>
        /// Delegate definition for injecting user-defined events into the input packet being synchronised within a cluster.
        /// </summary>
        public delegate void InputAccumulateFunc();

        /// <summary>
        /// Delegate for injecting your own user-defined input events into the input packet before it is syncrhonised within a cluster.
        /// This method is called on the master node after standard input has been queried, but before the packet is broadcast.
        /// </summary>
        public static InputAccumulateFunc AccumulateCustomInput;

        /// <summary>
        /// Force an input button to be "down" this frame only.
        /// Typically called within a user-defined InputAccumulateFunc().
        /// </summary>
        /// <param name="buttonID">The button to flag as "down".</param>
        public static void ForceButtonThisFrame(string buttonID)
        {
            int index = buttons.IndexOf(buttonID);
            if (index >= 0)
            {
                if (!instance.downButtons.Contains(index))
                    instance.downButtons.Add(index);
            }
        }

        /// <summary>
        /// Force an input key to be "down" this frame only.
        /// Typically called within a user-defined InputAccumulateFunc().
        /// </summary>
        /// <param name="keyCode">The key to flag as "down".</param>
        public static void ForceKeyThisFrame(KeyCode keyCode)
        {
            int index = Array.IndexOf(keyCodes, keyCode);
            if (index >= 0)
            {
                if (!instance.downKeys.Contains(index))
                    instance.downKeys.Add(index);
            }
        }

        /// <summary>
        /// Accumulate a value on an input axis this frame only.
        /// Typically called within a user-defined InputAccumulateFunc().
        /// </summary>
        /// <param name="axis">The axis to accumulate.</param>
        /// <param name="value">The value to accumulate on the axis.</param>
        public static void AccumulateAxisThisFrame(string axis, float value)
        {
            if (value == 0)
                return;

            int index = axes.IndexOf(axis);
            if (index >= 0)
            {
                if (instance.nonZeroAxes.ContainsKey(index))
                {
                    // combine
                    instance.nonZeroAxes[index] = (instance.nonZeroAxes[index] + value) * 0.5f;
                }
                else
                    instance.nonZeroAxes.Add(index, value);
            }
        }

        internal Input()
        {
            if (instance != null)
                throw new UnityException("HEVS Input already initialised!");

            instance = this;

            buttons.Clear();
            axes.Clear();
            touches.Clear();

            var inputMappings = Resources.Load<TextAsset>("inputmapping");
            if (inputMappings != null &&
                inputMappings.text != null &&
                !string.IsNullOrWhiteSpace(inputMappings.text))
            {
                SimpleJSON.JSONNode cfg = SimpleJSON.JSON.Parse(inputMappings.text);

                if (cfg != null)
                {
                    // load button names
                    var buttonList = cfg["buttons"].AsArray;
                    foreach (SimpleJSON.JSONNode entry in buttonList)
                        buttons.Add(entry);
                    // load axis names
                    var axisList = cfg["axes"].AsArray;
                    foreach (SimpleJSON.JSONNode entry in axisList)
                        axes.Add(entry);
                }
            }
            else
                Debug.LogWarning("HEVS: Failed to read input mappings! Does inputmapping.txt exist in the Resources folder?");

            GatherCustomSourceTypes();

            // open devices
            if (Core.activePlatform.tuioDevices.Count > 0)
            {
                foreach (var config in Core.activePlatform.tuioDevices)
                {
                    TUIODevice device = new TUIODevice();
                    device.Connect(config.Value);
                    tuioDevices.Add(device);
                }
            }

            foreach (var source in Core.activePlatform.inputSources)
            {
                var newSource = (InputSource)Activator.CreateInstance(registeredSourceTypes[source.Value.type], source.Value);
                if (newSource != null)
                    alternateSources.Add(newSource);
            }
        }

        internal static void Update()
        {
            instance.InternalUpdate();
        }

        internal static void Deserialize(ByteBufferReader reader)
        {
            // read mouse state
            instance.mouseState.position = reader.ReadVector3();
            instance.mouseState.delta = reader.ReadVector3();
            instance.mouseState.scrollDelta = reader.ReadVector2();
            instance.mouseState.buttonState[0] = reader.ReadBoolean();
            instance.mouseState.buttonState[1] = reader.ReadBoolean();
            instance.mouseState.buttonState[2] = reader.ReadBoolean();

            // lock keys
            instance.capslock = reader.ReadBoolean();
            instance.scrollock = reader.ReadBoolean();
            instance.numlock = reader.ReadBoolean();

            // keys, buttons and axes
            instance.downKeys.Clear();
            instance.downButtons.Clear();
            instance.nonZeroAxes.Clear();

            // down keys
            int count = reader.ReadInt();
            for (int i = 0; i < count; ++i)
                instance.downKeys.Add(reader.ReadInt());

            // down buttons
            count = reader.ReadInt();
            for (int i = 0; i < count; ++i)
                instance.downButtons.Add(reader.ReadInt());

            // non-zero axis
            count = reader.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                int key = reader.ReadInt();
                float value = reader.ReadFloat();
                instance.nonZeroAxes.Add(key, value);
            }

            // touches
            touches.Clear();
            count = reader.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                Touch touch = new Touch();
                touch.source = reader.ReadString();
                touch.fingerId = reader.ReadInt();
                touch.tapCount = reader.ReadInt();
                touch.deltaTime = reader.ReadFloat();
                touch.position = reader.ReadVector2();
                touch.deltaPosition = reader.ReadVector2();
                touch.phase = (TouchPhase)reader.ReadInt();
                touch.type = (TouchType)reader.ReadInt();
                touches.Add(touch);
            }
        }

        internal static void Serialize(ByteBufferWriter writer)
        {
            // write mouse state
            writer.Write(instance.mouseState.position);
            writer.Write(instance.mouseState.delta);
            writer.Write(instance.mouseState.scrollDelta);
            writer.Write(instance.mouseState.buttonState[0]);
            writer.Write(instance.mouseState.buttonState[1]);
            writer.Write(instance.mouseState.buttonState[2]);

            // write lock keys
            writer.Write(instance.capslock);
            writer.Write(instance.scrollock);
            writer.Write(instance.numlock);

            // write down keys
            writer.Write(instance.downKeys.Count);
            foreach (int value in instance.downKeys)
                writer.Write(value);

            // write down buttons
            writer.Write(instance.downButtons.Count);
            foreach (int value in instance.downButtons)
                writer.Write(value);

            // write non-zero axis
            writer.Write(instance.nonZeroAxes.Count);
            foreach (var value in instance.nonZeroAxes)
            {
                writer.Write((int)value.Key);
                writer.Write((float)value.Value);
            }

            // write touches
            writer.Write(touches.Count);
            foreach (var touch in touches)
            {
                writer.Write(touch.source);
                writer.Write(touch.fingerId);
                writer.Write(touch.tapCount);
                writer.Write(touch.deltaTime);
                writer.Write(touch.position);
                writer.Write(touch.deltaPosition);
                writer.Write((int)touch.phase);
                writer.Write((int)touch.type);
            }
        }

        #region AXIS
        /// <summary>
        /// Query the state of an axis.
        /// </summary>
        /// <param name="axis">The axis to query.</param>
        /// <returns>Returns the current value of the axis.</returns>
        public static float GetAxis(string axis)
        {
            int index = axes.IndexOf(axis);
            if (instance == null || instance.nonZeroAxes.ContainsKey(index) == false)
                return 0;
            else
                return instance.nonZeroAxes[index];
        }
        #endregion

        #region BUTTONS
        /// <summary>
        /// Get the state of a button.
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <returns>Returns if the button is "down" or not.</returns>
        public static bool GetButton(string button)
        {
            int index = buttons.IndexOf(button);
            return instance != null && instance.downButtons.Contains(index);
        }
        /// <summary>
        /// Query if a button is "up".
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <returns>Returns true if the button is not "down".</returns>
        public static bool GetButtonUp(string button)
        {
            int index = buttons.IndexOf(button);
            return instance != null && !instance.downButtons.Contains(index) && instance.previousDownButtons.Contains(index);
        }
        /// <summary>
        /// Query if a button is "down".
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <returns>Return true if the button is "down".</returns>
        public static bool GetButtonDown(string button)
        {
            int index = buttons.IndexOf(button);
            return instance != null && instance.downButtons.Contains(index) && !instance.previousDownButtons.Contains(index);
        }
        #endregion

        #region KEYBOARD
        /// <summary>
        /// Get the state of a key.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>Returns if the key is "down" or not.</returns>
        public static bool GetKey(KeyCode key)
        {
            int index = System.Array.IndexOf(keyCodes, key);
            return instance != null && instance.downKeys.Contains(index);
        }
        /// <summary>
        /// Query if a key is "up".
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>Returns true if the key is not "down".</returns>
        public static bool GetKeyUp(KeyCode key)
        {
            int index = System.Array.IndexOf(keyCodes, key);
            return instance != null && !instance.downKeys.Contains(index) && instance.previousDownKeys.Contains(index);
        }
        /// <summary>
        /// Query if a key is "down".
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>Return true if the key is "down".</returns>
        public static bool GetKeyDown(KeyCode key)
        {
            int index = System.Array.IndexOf(keyCodes, key);
            return instance != null && instance.downKeys.Contains(index) && !instance.previousDownKeys.Contains(index);
        }

        /// <summary>
        /// True if any shift key is "down".
        /// </summary>
        public static bool IsShift { get { return GetKey(KeyCode.LeftShift) || GetKey(KeyCode.RightShift); } }
        /// <summary>
        /// True if any control key is "down".
        /// </summary>
        public static bool IsControl { get { return GetKey(KeyCode.LeftControl) || GetKey(KeyCode.RightControl); } }

        /// <summary>
        /// Query if a lock key is "locked".
        /// </summary>
        /// <param name="key">The KeyCode to query, either Numlock, ScrollLock or CapsLock.</param>
        /// <returns>Returns true if the specified key is "locked".</returns>
        public static bool IsKeyLocked(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Numlock: return instance == null ? false : instance.numlock;
                case KeyCode.ScrollLock: return instance == null ? false : instance.scrollock;
                case KeyCode.CapsLock: return instance == null ? false : instance.capslock;
                default: Debug.LogWarning("HEVS: Requesting lock-status of a non-lock key: " + key.ToString()); return false;
            }
        }
        /// <summary>
        /// True if capslock is "locked".
        /// </summary>
        public static bool IsCaps { get { return IsKeyLocked(KeyCode.CapsLock); } }
        /// <summary>
        /// True if capslock is "locked" or either shift key is "down".
        /// </summary>
        public static bool IsShiftOrCaps { get { return IsShift || IsCaps; } }
        #endregion

        #region MOUSE
        /// <summary>
        /// The position of the mouse cursor.
        /// </summary>
        public static Vector3 mousePosition { get { return instance == null ? Vector3.zero : instance.mouseState.position; } }
        /// <summary>
        /// The delta of the mouse cursor movement.
        /// </summary>
        public static Vector3 mouseDelta { get { return instance == null ? Vector3.zero : instance.mouseState.delta; } }
        /// <summary>
        /// The delta of the mouse scrollwheel.
        /// </summary>
        public static Vector2 mouseScrollDelta { get { return instance == null ? Vector2.zero : instance.mouseState.scrollDelta; } }

        /// <summary>
        /// Query the state of a mouse button.
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <returns>Returns true if the button is "down", and false otherwise.</returns>
        public static bool GetMouseButton(int button)
        {
            return instance != null && instance.mouseState.buttonState[button];
        }
        /// <summary>
        /// Query if a mouse button is "up".
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <returns>Returns true if the button is "up", and false otherwise.</returns>
        public static bool GetMouseButtonUp(int button)
        {
            return instance != null && !instance.mouseState.buttonState[button] && instance.mouseState.previousButtonState[button];
        }
        /// <summary>
        /// Query if a mouse button is "down".
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <returns>Returns true if the button is "down", and false otherwise.</returns>
        public static bool GetMouseButtonDown(int button)
        {
            return instance != null && instance.mouseState.buttonState[button] && !instance.mouseState.previousButtonState[button];
        }
        #endregion

        #region TOUCH       
        /// <summary>
        /// A list of all current touches.
        /// </summary>
        public static List<Touch> touches { get; private set; } = new List<Touch>();
        /// <summary>
        /// The current number of active touches.
        /// </summary>
        public static int touchCount { get { return touches.Count; } }

        /// <summary>
        /// Get a specific touch.
        /// </summary>
        /// <param name="index">The index of the touch to query.</param>
        /// <returns>Returns a touch at the specified index, or null.</returns>
        public static Touch GetTouch(int index)
        {
            if (index < touches.Count)
                return touches[index];
            return null;
        }
        #endregion

        #region INTERNAL
        static Input instance;

        class MouseState
        {
            public Vector3 position = Vector3.zero;
            public Vector3 delta = Vector3.zero;
            public Vector2 scrollDelta = Vector2.zero;
            public bool[] buttonState = new bool[3] { false, false, false };
            public bool[] previousButtonState = new bool[3] { false, false, false };
        }

        MouseState mouseState = new MouseState();

        List<int> downKeys = new List<int>();
        List<int> downButtons = new List<int>();
        Dictionary<int, float> nonZeroAxes = new Dictionary<int, float>();
        List<TUIODevice> tuioDevices = new List<TUIODevice>();

        List<int> previousDownKeys = new List<int>();
        List<int> previousDownButtons = new List<int>();

        bool capslock = false;
        bool scrollock = false;
        bool numlock = false;

        void InternalUpdate()
        {
            // track previous down state
            previousDownKeys = downKeys.ToList();
            previousDownButtons = downButtons.ToList();

            downKeys.Clear();
            downButtons.Clear();
            nonZeroAxes.Clear();

            int i = 0;
            foreach (KeyCode key in Input.keyCodes)
            {
                if (UnityEngine.Input.GetKey(key))
                    downKeys.Add(i);
                ++i;
            }

            mouseState.delta = UnityEngine.Input.mousePosition - mouseState.position;
            mouseState.position = UnityEngine.Input.mousePosition;
            mouseState.scrollDelta = UnityEngine.Input.mouseScrollDelta;
            mouseState.previousButtonState[0] = mouseState.buttonState[0];
            mouseState.previousButtonState[1] = mouseState.buttonState[1];
            mouseState.previousButtonState[2] = mouseState.buttonState[2];
            mouseState.buttonState[0] = UnityEngine.Input.GetMouseButton(0);
            mouseState.buttonState[1] = UnityEngine.Input.GetMouseButton(1);
            mouseState.buttonState[2] = UnityEngine.Input.GetMouseButton(2);

            capslock = IsKeyLocked(KeyCode.CapsLock);
            scrollock = IsKeyLocked(KeyCode.ScrollLock);
            numlock = IsKeyLocked(KeyCode.Numlock);

            // buttons
            i = 0;
            foreach (string button in Input.buttons)
            {
                if (UnityEngine.Input.GetButton(button))
                    downButtons.Add(i);
                ++i;
            }

            // axis
            i = 0;
            foreach (string axis in Input.axes)
            {
                float value = UnityEngine.Input.GetAxis(axis);
                if (value != 0)
                    nonZeroAxes.Add(i, value);
                ++i;
            }

            // alternative input sources
            if (Core.isActive)
            {
                List<string> altButtons = new List<string>();
                Dictionary<string, float> altAxes = new Dictionary<string, float>();
                foreach (var inputSource in alternateSources)
                {
                    inputSource.GatherDownButtons(altButtons);
                    inputSource.GatherNonZeroAxes(altAxes);
                }

                // find index of the mapped axis
                foreach (var pair in altAxes)
                {
                    i = axes.IndexOf(pair.Key);
                    if (i >= 0)
                    {
                        if (nonZeroAxes.ContainsKey(i))
                        {
                            // combine - HACKY
                            nonZeroAxes[i] = (nonZeroAxes[i] + pair.Value) * 0.5f;
                        }
                        else
                            nonZeroAxes.Add(i, pair.Value);
                    }
                }

                // find index of the mapped button
                foreach (string mapping in altButtons)
                {
                    i = buttons.IndexOf(mapping);
                    if (i >= 0)
                    {
                        if (!downButtons.Contains(i))
                            downButtons.Add(i);
                    }
                }
            }

            // TOUCHES
            touches.Clear();

            // screen size used to normalize Unity touches
            var ss = new Vector2(Screen.width, Screen.height);

            // gather unity touches
            foreach (var touch in UnityEngine.Input.touches)
            {
                Touch newTouch = new Touch();
                newTouch.deltaPosition = touch.deltaPosition / ss;
                newTouch.deltaTime = touch.deltaTime;
                newTouch.fingerId = touch.fingerId;
                newTouch.phase = touch.phase;
                newTouch.position = touch.position / ss;
                newTouch.tapCount = touch.tapCount;
                newTouch.type = touch.type;

                touches.Add(newTouch);
            }

            // gather TUIO touches
            foreach (var device in tuioDevices)
            {
                foreach (var cursor in device.cursors)
                {
                    Touch newTouch = new Touch(device.id);
                    newTouch.fingerId = cursor.Id;
                    newTouch.position = new Vector2(cursor.X, cursor.Y);
                    newTouch.deltaPosition = new Vector2(cursor.VelocityX, cursor.VelocityY);

                    newTouch.deltaTime = 0; // !<<<<<
                    newTouch.tapCount = 0; // !<<<<<<<<<<<<<<<<<
                    newTouch.phase = TouchPhase.Moved; // !<<<<<<<<<<<<<<<<<<<<
                    newTouch.type = TouchType.Direct; // !<<<<<<<<<<<<<<

                    touches.Add(newTouch);
                }
            }

            AccumulateCustomInput?.Invoke();
        }
        #endregion
    }
}