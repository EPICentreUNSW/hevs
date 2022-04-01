using System;
using System.Collections.Generic;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Attribute used for registering methods as handles for OSC packets.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OSCReceiverAttribute : Attribute
    {
        /// <summary>
        /// The OSC packet address that the method listens to.
        /// </summary>
        public string address;

        /// <summary>
        /// Marks a method as a callback for a specified OSC packet address.
        /// </summary>
        /// <param name="address">The address to listen to.</param>
        public OSCReceiverAttribute(string address) { this.address = address; }
    }

}
