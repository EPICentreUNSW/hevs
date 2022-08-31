using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HEVS
{
    /// <summary>
    /// Flags used to mask which parts of a transform should be synched or used.
    /// </summary>
    public enum TransformFlags
    {
        /// <summary>
        /// None of the transform.
        /// </summary>
        None = 0,
        /// <summary>
        /// Translation only.
        /// </summary>
        Position = 1 << 0,
        /// <summary>
        /// Rotation only.
        /// </summary>
        Rotation = 1 << 1,
        /// <summary>
        /// Scale only.
        /// </summary>
        Scale = 1 << 2,
        /// <summary>
        /// Translation and Rotation only.
        /// </summary>
        PositionRotation = Position | Rotation,
        /// <summary>
        /// All.
        /// </summary>
        All = ~0
    }

    /// <summary>
    /// An enumeration of potential axes, including negative axes.
    /// </summary>
    public enum TrackerAxis
    {
        /// <summary>
        /// Position X axis (1,0,0).
        /// </summary>
        X,
        /// <summary>
        /// Position Y axis (0,1,0).
        /// </summary>
        Y,
        /// <summary>
        /// Position Z axis (0,0,1).
        /// </summary>
        Z,
        /// <summary>
        /// negative X axis (-1,0,0).
        /// </summary>
        NEG_X,
        /// <summary>
        /// Negative Y axis (0,-1,0).
        /// </summary>
        NEG_Y,
        /// <summary>
        /// Negative Z axis (0,0,-1).
        /// </summary>
        NEG_Z
    }

    /// <summary>
    /// An enumeration for coordinate space handedness, which can be either Left-handed or Right-handed.
    /// See https://en.wikipedia.org/wiki/Right-hand_rule for reference.
    /// </summary>
    public enum TrackerHandedness
    {
        /// <summary>
        /// Left-handed coordinate space (if (1,0,0) is Right, and (0,1,0) is Up, then (0,0,1) is Forward).
        /// </summary>
        Left,

        /// <summary>
        /// Right-handed coordinate space (if (1,0,0) is Right, and (0,1,0) is Up, then (0,0,-1) is Forward).
        /// </summary>
        Right
    }

    public partial class Config
    {
        /// <summary>
        /// A HEVS tracker configuration.
        /// </summary>
        public class Tracker : IConfigObject
        {
            /// <summary>
            /// Construct a new TrackerConfig with a given ID.
            /// </summary>
            /// <param name="id">The ID to assign to this tracker.</param>
            public Tracker(string id) { this.id = id; }

            /// <summary>
            /// The ID of the tracker.
            /// </summary>
            public string id;

            /// <summary>
            /// The type of the HEVS tracker.
            /// </summary>
            public string type;

            /// <summary>
            /// The axis that represents the "forward" direction for this tracker.
            /// </summary>
            public TrackerAxis forward = TrackerAxis.Z;

            /// <summary>
            /// The axis that represents the "right" direction for this tracker.
            /// </summary>
            public TrackerAxis right = TrackerAxis.X;

            /// <summary>
            /// The axis that represents the "up" direction for this tracker.
            /// </summary>
            public TrackerAxis up = TrackerAxis.Y;

            /// <summary>
            /// The coordinate space handedness for this tracker.
            /// </summary>
            public TrackerHandedness handedness = TrackerHandedness.Left;

            /// <summary>
            /// Transform flags for this tracker.
            /// </summary>
            public int transformFlags = (int)(TransformFlags.PositionRotation);

            /// <summary>
            /// Should this tracker apply rudimentary 
            /// ing of the tracked transform?
            /// This can help smooth noisy tracking systems, but does add some latency.
            /// </summary>
            public bool smoothing = false;

            /// <summary>
            /// A multiplier to apply to smoothed trackers. A higher multiplier will reduce latency, but can add noise back in.
            /// </summary>
            public float smoothMultiplier = 1;

            /// <summary>
            /// A transform to apply on top of a tracker. This can help reorientate tracker spaces between platforms.
            /// </summary>
            public Transform transform = Transform.identity;

            /// <summary>
            /// The SimpleJSON JSON data used to create this tracker.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// The Unity XR node to track.
            /// </summary>
            public XRNode xrNode = XRNode.LeftEye;

            /// <summary>
            /// The port that this OSC connection should listen to.
            /// </summary>
            public int port = 7890;

            /// <summary>
            /// The VRPN IP address of the tracked device.
            /// </summary>
            public string address;

            /// <summary>
            /// Parse JSON data to initialise this config.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <param name="transforms">A list of the Platform's named transforms.</param>
            /// <returns>Returns true if the data is successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json, Dictionary<string, Transform> transforms)
            {
                if (!Parse(json))
                    return false;

                if (json.Keys.Contains("transform"))
                {
                    // is it a string? if so, find transform in Platform's list
                    if (json["transform"].Tag == SimpleJSON.JSONNodeType.String)
                    {
                        //     offsetTransform = transforms.Find(t => t.id.Equals(json["transform"], StringComparison.OrdinalIgnoreCase));
                        if (!transforms.TryGetValue(json["transform"], out transform))
                        {
                            Debug.LogError("HEVS: Failed to find transform [" + json["transform"].Value + "] for tracker [" + id + "]");
                            return false;
                        }
                    }
                    else
                    {
                        // else parse the transform
                        if (!transform.Parse(json["transform"]))
                        {
                            Debug.LogError("HEVS: Failed to parse transform for tracker [" + id + "]");
                            return false;
                        }
                    }
                }

                return true;
            }

            /// <summary>
            /// Parse JSON data to initialise this config.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <returns>Returns true if the data is successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                this.json = json;

                if (json.Keys.Contains("smooth"))
                    smoothing = json["smooth"].AsBool;

                if (json.Keys.Contains("smooth_multiplier"))
                    smoothMultiplier = json["smooth_multiplier"].AsFloat;

                if (json.Keys.Contains("handedness"))
                {
                    string hand = json["handedness"];
                    if (hand.ToLower() == "right") handedness = TrackerHandedness.Right;
                    else if (hand.ToLower() == "left") handedness = TrackerHandedness.Left;
                    else
                    {
                        Debug.LogError("HEVS: Invalid handedness for vrpn tracker [" + json["id"] + "]!");
                        return false;
                    }
                }
                if (json.Keys.Contains("forward"))
                {
                    string axis = json["forward"];
                    if (axis.ToLower() == "x") forward = TrackerAxis.X;
                    else if (axis.ToLower() == "y") forward = TrackerAxis.Y;
                    else if (axis.ToLower() == "z") forward = TrackerAxis.Z;
                    else if (axis.ToLower() == "-x") forward = TrackerAxis.NEG_X;
                    else if (axis.ToLower() == "-y") forward = TrackerAxis.NEG_Y;
                    else if (axis.ToLower() == "-z") forward = TrackerAxis.NEG_Z;
                    else
                    {
                        Debug.LogError("HEVS: Invalid forward option for vrpn tracker [" + json["id"] + "]!");
                        return false;
                    }
                }
                if (json.Keys.Contains("right"))
                {
                    string axis = json["right"];
                    if (axis.ToLower() == "x") right = TrackerAxis.X;
                    else if (axis.ToLower() == "y") right = TrackerAxis.Y;
                    else if (axis.ToLower() == "z") right = TrackerAxis.Z;
                    else if (axis.ToLower() == "-x") right = TrackerAxis.NEG_X;
                    else if (axis.ToLower() == "-y") right = TrackerAxis.NEG_Y;
                    else if (axis.ToLower() == "-z") right = TrackerAxis.NEG_Z;
                    else
                    {
                        Debug.LogError("HEVS: Invalid right option for vrpn tracker [" + json["id"] + "]!");
                        return false;
                    }
                }
                if (json.Keys.Contains("up"))
                {
                    string axis = json["up"];
                    if (axis.ToLower() == "x") up = TrackerAxis.X;
                    else if (axis.ToLower() == "y") up = TrackerAxis.Y;
                    else if (axis.ToLower() == "z") up = TrackerAxis.Z;
                    else if (axis.ToLower() == "-x") up = TrackerAxis.NEG_X;
                    else if (axis.ToLower() == "-y") up = TrackerAxis.NEG_Y;
                    else if (axis.ToLower() == "-z") up = TrackerAxis.NEG_Z;
                    else
                    {
                        Debug.LogError("HEVS: Invalid up option for vrpn tracker [" + json["id"] + "]!");
                        return false;
                    }
                }

                if (json.Keys.Contains("type"))                
                    type = json["type"];

                // read in the node type as an enum for XRNode
                if (json.Keys.Contains("node"))
                    xrNode = (XRNode)Enum.Parse(typeof(XRNode), json["node"], true);

                if (json.Keys.Contains("address"))
                    address = json["address"];

                if (json.Keys.Contains("port"))
                    port = json["port"].AsInt;

                // check which properties to apply
                if (json.Keys.Contains("sync"))
                {
                    // ignore whitespace, split by comma
                    string temp = json["sync"];
                    string[] options = temp.Split(',', '|', ' ', ';');
                    transformFlags = 0;
                    foreach (string option in options)
                        transformFlags |= (int)(TransformFlags)Enum.Parse(typeof(TransformFlags), option, true);
                }

                return true;
            }
        }
    }
}