using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// An interface used to create a data receiver that can be registered with HEVS.Cluster to receive broadcast packets.
    /// </summary>
    public interface IDataBroadcastReceiver
    {
        /// <summary>
        /// A method for receiving broadcast data.
        /// </summary>
        /// <param name="packetHeader">A user-defined packet identifier assigned to this broadcast data.</param>
        /// <param name="data">The broadcasted data.</param>
        void OnDataReceived(int packetHeader, byte[] data);
    }
}