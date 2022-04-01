using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// An OSC transmitter that transmits this GameObject's local transform, either 
    /// combined as /transform using 9 floats for position, rotation (euler), scale, 
    /// or separately as /position, /rotation and /scale respectively.
    /// </summary>
    [AddComponentMenu("HEVS/Trackers/OSC Transform Transmitter")]
    public class OSCTransformTransmitter : MonoBehaviour
    {
        OSCsharp.Net.UDPTransmitter oscTransmitter = null;

        /// <summary>
        /// Should the transmitter start broadcasting as soon as it has loaded into the scene.
        /// </summary>
        public bool broadcastOnStart = true;

        /// <summary>
        /// The ID of the broadcaster.
        /// </summary>
        public string id;

        /// <summary>
        /// The IP address / hostname that the transmitter will send to.
        /// </summary>
        public string address = "127.0.0.1";

        /// <summary>
        /// The port that the transmitter will send to.
        /// </summary>
        public int port = 7890;

        /// <summary>
        /// Flags to control which parts of the transform will be transmitted.
        /// </summary>
        public int transformFlags = (int)TransformFlags.All;

        void Start()
        {
            if (broadcastOnStart)
                StartBroadcasting();
        }

        void LateUpdate()
        {
            if (oscTransmitter != null)
            {
                if (transformFlags == (int)TransformFlags.All)
                {
                    OSCsharp.Data.OscMessage msg = new OSCsharp.Data.OscMessage("/" + id + "/transform");
                    msg.Append(transform.localPosition.x);
                    msg.Append(transform.localPosition.y);
                    msg.Append(transform.localPosition.z);
                    msg.Append(transform.localRotation.eulerAngles.x);
                    msg.Append(transform.localRotation.eulerAngles.y);
                    msg.Append(transform.localRotation.eulerAngles.z);
                    msg.Append(transform.localScale.x);
                    msg.Append(transform.localScale.y);
                    msg.Append(transform.localScale.z);
                    oscTransmitter.Send(msg);
                }
                else
                {
                    if ((transformFlags & (int)TransformFlags.Position) != 0)
                    {
                        OSCsharp.Data.OscMessage msg = new OSCsharp.Data.OscMessage("/" + id + "/position");
                        msg.Append(transform.localPosition.x);
                        msg.Append(transform.localPosition.y);
                        msg.Append(transform.localPosition.z);
                        oscTransmitter.Send(msg);
                    }
                    if ((transformFlags & (int)TransformFlags.Rotation) != 0)
                    {
                        OSCsharp.Data.OscMessage msg = new OSCsharp.Data.OscMessage("/" + id + "/rotation");
                        msg.Append(transform.localRotation.eulerAngles.x);
                        msg.Append(transform.localRotation.eulerAngles.y);
                        msg.Append(transform.localRotation.eulerAngles.z);
                        oscTransmitter.Send(msg);
                    }
                    if ((transformFlags & (int)TransformFlags.Scale) != 0)
                    {
                        OSCsharp.Data.OscMessage msg = new OSCsharp.Data.OscMessage("/" + id + "/scale");
                        msg.Append(transform.localScale.x);
                        msg.Append(transform.localScale.y);
                        msg.Append(transform.localScale.z);
                        oscTransmitter.Send(msg);
                    }
                }
            }
        }

        /// <summary>
        /// Start broadcasting to the default address and port.
        /// </summary>
        public void StartBroadcasting()
        {
            StartBroadcasting(address, port);
        }

        /// <summary>
        /// Start broadcasting to a specified address and port.
        /// </summary>
        /// <param name="address">The IP address or hostname to transmit to.</param>
        /// <param name="port">The port to transmit to.</param>
        public void StartBroadcasting(string address, int port = 7890)
        {
            if (string.IsNullOrWhiteSpace(id))
                Debug.LogError("HEVS: Invalid id for OSC Transmitter, can't start broadcasting.");
            else if (string.IsNullOrWhiteSpace(address))
                Debug.LogError("HEVS: Invalid address for OSC Transmitter, can't start broadcasting.");
            else if (oscTransmitter != null)
                Debug.LogError("HEVS: OSC Trasmitter already transmitting! Stop transmitting before attempting to start a new transmission.");
            else
            {
                oscTransmitter = new OSCsharp.Net.UDPTransmitter(address, port);
                oscTransmitter.Connect();
            }
        }

        /// <summary>
        /// Stop broadcasting from this transmitter.
        /// </summary>
        public void StopBroadcasting()
        {
            if (oscTransmitter == null)
            {
                Debug.LogWarning("HEVS: Attempting to stop an OSC Transmitter that is already stopped.");
                return;
            }

            oscTransmitter.Close();
            oscTransmitter = null;
        }
    }
}
