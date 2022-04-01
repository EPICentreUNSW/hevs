using System.Linq;
using System.Collections.Generic;
using System;

namespace HEVS
{
    public partial class Config
    {
        /// A HEVS input source config object that defines an alternative source of
        /// input (i.e. buttons/axis come from a VRPN device, rather than an attached gamepad).
        public class InputSource : IConfigObject
        {
            /// <summary>
            /// A struct defining an input mapping between VRPN/OSC and Unity Input Manager for HEVS.
            /// </summary>
            public class Mapping
            {
                /// <summary>
                /// VRPN/OSC button/axis index.
                /// </summary>
                public int index;

                /// <summary>
                /// The current value of the source.
                /// Buttons: 0 = up/released, 1 = down/pressed.
                /// Axis: value from the axis.
                /// </summary>
                public float value;

                /// <summary>
                /// Unity Input Manager axis strings.
                /// A single VRPN/OSC button/axis can map to multiple Unity axes at once.
                /// </summary>
                public string[] mappings;

                /// <summary>
                /// For VRPN/OSC mappings to axes, should the value be inverted? 
                /// </summary>
                public bool inverted;
            }

            /// <summary>
            /// Constructs a new InputSOurceConfig with a given ID.
            /// </summary>
            /// <param name="id">The ID to assign to this input source.</param>
            public InputSource(string id) { this.id = id; }

            /// <summary>
            /// The ID of this input source (optional)
            /// </summary>
            public string id;

            /// <summary>
            /// The type of this input source.
            /// </summary>
            public string type;

            /// <summary>
            /// The source of the input.
            /// For VRPN this is a VRPN device's address.
            /// For OSC this is the listening port number.
            /// </summary>
            public string source;

            /// <summary>
            /// A list of button mappings for this input source.
            /// </summary>
            public List<Mapping> buttonMappings = new List<Mapping>();

            /// <summary>
            /// Query if a valid button is currently being pressed.
            /// </summary>
            /// <param name="index">The index of the button to query on this input source.</param>
            /// <returns>Returns true if the index is valid for this device and it is being pressed, otherwise returns false.</returns>
        /*    public bool GetButton(int index)
            {
                var button = buttonMappings.Find(b => b.index == index);
                return GetButton(button);
            }

            internal bool GetButton(Mapping button)
            {
                if (button != null)
                {
                    //    if (type == InputSourceType.VRPN)
                    //        button.value = vrpnDevice.GetButton(button.index) ? 1 : 0;
                    return button.value != 0;
                }
                return false;
            }*/

            /// <summary>
            /// A list of axis mappings for an OSC device.
            /// </summary>
            public List<Mapping> axisMappings = new List<Mapping>();

            /// <summary>
            /// Query the value of a valid axis on this input source.
            /// </summary>
            /// <param name="index">The index of the axis on this input source.</param>
            /// <returns>Returns the value of this axis, or 0 if the index is not valid for this input source.</returns>
       /*     public float GetAxis(int index)
            {
                var axis = axisMappings.Find(a => a.index == index);
                return GetAxis(axis);
            }

            internal float GetAxis(Mapping axis)
            {
                if (axis != null)
                {
                    //    if (type == InputSourceType.VRPN)
                    //        axis.value = (float)vrpnDevice.GetAxis(axis.index);
                    return axis.value;
                }
                return 0;
            }*/

            /// <summary>
            /// [Internal] Called when the owning platform is the one that is currently running.
            /// </summary>
            /*    public void OnPlatformActivated()
                {
                    // connect to source?
                    switch (type)
                    {
                        case InputSourceType.VRPN:
                            {
                                vrpnDevice = VRPN.GetDevice(source);
                            }
                            break;
                        case InputSourceType.OSC:
                            {
                                int port = int.Parse(source);
                                if (openOSCPorts.ContainsKey(port))
                                {
                                    openOSCPorts[port].MessageReceived += OnMessageReceived;
                                }
                                else
                                {
                                    var osc = new OSCsharp.Net.UDPReceiver(int.Parse(source), false);
                                    osc.MessageReceived += OnMessageReceived;

                                    // add to dictionary
                                    openOSCPorts.Add(port, osc);

                                    // start receiving
                                    osc.Start();
                                }
                            }
                            break;
                    }
                }*/

            /// <summary>
            /// Parse a SimpleJSON JSON config to setup alternative input sources for this platform.
            /// </summary>
            /// <param name="json">The JSON source to parse.</param>
            /// <returns>Returns true if JSON successfully parsed, and false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                if (json.Keys.Contains("type"))
                    type = json["type"];
             /*   {
                    var temp = (InputSourceType)Enum.Parse(typeof(InputSourceType), json["type"], true);
                    // if changing types then reset the source!
                    if (temp != type)
                    {
                        type = temp;
                        source = string.Empty;
                    }
                }*/

                if (json.Keys.Contains("source"))
                    source = json["source"];

                if (json.Keys.Contains("buttons"))
                {
                    SimpleJSON.JSONArray buttons = json["buttons"].AsArray;
                    foreach (SimpleJSON.JSONNode button in buttons)
                    {
                        Mapping b = new Mapping();
                        b.inverted = false;
                        b.index = button["index"].AsInt;
                        var mappings = button["mapping"].AsArray;
                        if (mappings != null)
                        {
                            b.mappings = new string[mappings.Count];
                            int index = 0;
                            foreach (SimpleJSON.JSONNode mapping in mappings)
                            {
                                b.mappings[index++] = mapping;
                            }
                        }
                        else
                        {
                            b.mappings = new string[1];
                            b.mappings[0] = button["mapping"];
                        }
                        buttonMappings.Add(b);
                    }
                }
                if (json.Keys.Contains("axes"))
                {
                    SimpleJSON.JSONArray axes = json["axes"].AsArray;
                    foreach (SimpleJSON.JSONNode axis in axes)
                    {
                        Mapping a = new Mapping();
                        a.index = axis["index"].AsInt;
                        var mappings = axis["mapping"].AsArray;
                        if (mappings != null)
                        {
                            a.mappings = new string[mappings.Count];
                            int index = 0;
                            foreach (SimpleJSON.JSONNode mapping in mappings)
                            {
                                a.mappings[index++] = mapping;
                            }
                        }
                        else
                        {
                            a.mappings = new string[1];
                            a.mappings[0] = axis["mapping"];
                        }

                        if (axis.Keys.Contains("invert"))
                            a.inverted = axis["invert"].AsBool;
                        else
                            a.inverted = false;

                        axisMappings.Add(a);
                    }
                }

                return !string.IsNullOrWhiteSpace(source);
            }

            #region VRPN Input Handling
            //    VRPN.IDevice vrpnDevice;
            #endregion

            #region OSC Input Handling
            /// <summary>
            ///  OSC receivers open on specific ports (can have multiple sources use the one receiver port).
            /// </summary>
            /*        static Dictionary<int, OSCsharp.Net.UDPReceiver> openOSCPorts = new Dictionary<int, OSCsharp.Net.UDPReceiver>();

                    void OnMessageReceived(object sender, OSCsharp.Net.OscMessageReceivedEventArgs args)
                    {
                        if (args.Message == null)
                            return;

                        // is it to be handled by us?
                        if (!args.Message.Address.StartsWith("/input/" + id))
                            return;

                        // strip our ID from the address
                        string address = args.Message.Address.Replace("/input/" + id, "");

                        // handle button or axis
                        switch (address)
                        {
                            case "/button":
                                {
                                    // buttonIndex
                                    int index = (int)args.Message.Data[0];
                                    // buttonState (0 = released, 1 = press)
                                    int state = (int)args.Message.Data[1];

                                    var button = buttonMappings.Find(b => b.index == index);
                                    if (button != null)
                                        button.value = (float)state;
                                }
                                break;
                            case "/axis":
                                {
                                    // axisIndex
                                    int index = (int)args.Message.Data[0];
                                    // axisValue
                                    float value = (float)args.Message.Data[1];

                                    var axis = axisMappings.Find(a => a.index == index);
                                    if (axis != null)
                                        axis.value = value;
                                }
                                break;
                        }
                    }*/
            #endregion
        }
    }
}