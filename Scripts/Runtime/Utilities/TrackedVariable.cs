using UnityEngine;
using System.Collections.Generic;
using System;

namespace HEVS
{
    /// <summary>
    /// A templated type that can be used to track changes in variables.
    /// </summary>
    /// <typeparam name="T">The type of variable to track.</typeparam>
    public class TrackedVariable<T>
    {
        private Func<T> readValue = null;
        private T _lastValue;

        /// <summary>
        /// Access to the previous value of the variable.
        /// </summary>
        public T lastValue { get { return _lastValue; } }

        /// <summary>
        /// Consruct a TrackedVariable with a function for accessing the value to track.
        /// </summary>
        /// <param name="readValue">The function for accessing the source variable.</param>
        public TrackedVariable(Func<T> readValue)
        {
            this.readValue = readValue;
            _lastValue = readValue();
        }

        /// <summary>
        /// Check if the value has changed since the last call to this method.
        /// </summary>
        public bool hasChanged
        {
            get
            {
                T newValue = this.readValue();
                if (!EqualityComparer<T>.Default.Equals(_lastValue, newValue))
                {
                    _lastValue = newValue;
                    return true;
                }
                return false;
            }
        }
    }

    /// <summary>
    /// A TrackedVariable for tracking a boolean.
    /// </summary>
    public class TrackedBool : TrackedVariable<bool>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a bool.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedBool(Func<bool> readValue) : base(readValue) { }
    }

    /// <summary>
    /// A TrackedVariable for tracking a integer.
    /// </summary>
    public class TrackedInt : TrackedVariable<int>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a int.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedInt(Func<int> readValue) : base(readValue) { }
    }

    /// <summary>
    /// A TrackedVariable for tracking a float.
    /// </summary>
    public class TrackedFloat : TrackedVariable<float>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a float.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedFloat(Func<float> readValue) : base(readValue) { }
    }

    /// <summary>
    /// A TrackedVariable for tracking a double.
    /// </summary>
    public class TrackedDouble : TrackedVariable<double>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a double.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedDouble(Func<double> readValue) : base(readValue) { }
    }

    /// <summary>
    /// A TrackedVariable for tracking a Vector2.
    /// </summary>
    public class TrackedVector2 : TrackedVariable<Vector2>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a Vector2.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedVector2(Func<Vector2> readValue) : base(readValue) { }
    }

    /// <summary>
    /// A TrackedVariable for tracking a Vector3.
    /// </summary>
    public class TrackedVector3 : TrackedVariable<Vector3>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a Vector3.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedVector3(Func<Vector3> readValue) : base(readValue) { }
    }

    /// <summary>
    /// A TrackedVariable for tracking a Vector4.
    /// </summary>
    public class TrackedVector4 : TrackedVariable<Vector4>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a Vector4.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedVector4(Func<Vector4> readValue) : base(readValue) { }
    }

    /// <summary>
    /// A TrackedVariable for tracking a Color.
    /// </summary>
    public class TrackedColor : TrackedVariable<Color>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a Color.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedColor(Func<Color> readValue) : base(readValue) { }
    }

    /// <summary>
    /// A TrackedVariable for tracking a Quaternion.
    /// </summary>
    public class TrackedQuaternion : TrackedVariable<Quaternion>
    {
        /// <summary>
        /// Create a TrackedVariable tracking a Quaternion.
        /// </summary>
        /// <param name="readValue">The method for accessing the source variable.</param>
        public TrackedQuaternion(Func<Quaternion> readValue) : base(readValue) { }
    }
}