using System;
using System.Collections.Generic;
using UnityEngine;

using HEVS.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace HEVS
{
    /// <summary>
    /// Base class for variables that can be synchronised within a cluster from the master node to all client nodes.
    /// </summary>
    public abstract class ClusterVariable
    {
        /// <summary>
        /// The name of the clustered variable. The name is used to identify the variable within the cluster.
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// Return true if this variable is valid and registered with HEVS internal systems.
        /// </summary>
        public bool isValid { get; private set; } = false;
        
        /// <summary>
        /// Set the value of the object. The new value wont take effect until the end of the frame.
        /// </summary>
        /// <param name="value">The new value for the variable.</param>
        public void SetValue(object value)
        {
            if (isValid && Cluster.isMaster)
                registeredVariables[name].Set(value);
        }

        /// <summary>
        /// Return the current value of the variable. If the value has recently been changed with a call to SetValue() then 
        /// the returned variable will still return the old value and wont return the new value until after this frame's Update().
        /// </summary>
        /// <param name="value">Reference to the value that will be set to the current variable's value.</param>
        /// <returns>Returns true if the value is valid or not.</returns>
        public bool GetValue(ref object value)
        {
            if (isValid)
                value = registeredVariables[name].value;
            return isValid;
        }

        // will this actually work?
        private static int nextID = 0;

        /// <summary>
        /// Create an unnamed variable. Unnamed variables will receive the name "_unnamed_N" where N is an incremented integer.
        /// </summary>
        /// <param name="value">Starting value of the variable.</param>
        protected ClusterVariable(object value)
        {
            this.name = "_unnamed_" + nextID++;
            RegisterVariable(name, value);
        }

        /// <summary>
        /// Create an named variable.
        /// </summary>
        /// <param name="name">The name to assign to the variable.</param>
        /// <param name="value">Starting value of the variable.</param>
        protected ClusterVariable(string name, object value)
        {
            this.name = name;
            RegisterVariable(name, value);
        }

        /// <summary>
        /// Cleanup the variable.
        /// </summary>
        ~ClusterVariable()
        {
            DeregisterVariable();
        }

        private bool RegisterVariable(string name, object value)
        {
            // if not registered, add new variable
            if (!registeredVariables.ContainsKey(name))
            {
                registeredVariables.Add(name, new RegisteredData() { refCount = 1, dirty = true, type = value.GetType(), value = value, newValue = value });
                isValid = true;
            }
            // if registered, is it the same type?
            else
            {
                // if not, return null
                if (registeredVariables[name].type != value.GetType())
                {
                    Debug.LogError("HEVS: ClusterVariable [" + name + "] already registered with different type!");
                }
                // if so, increase ref count
                else
                {
                    registeredVariables[name].refCount++;
                    isValid = true;
                }
            }
            return isValid;
        }

        private void DeregisterVariable()
        {
            // if registered, and same type, decrement reference count
            if (isValid && registeredVariables.ContainsKey(name))
            {
                // if count is 0, remove        
                if (--registeredVariables[name].refCount <= 0)
                    registeredVariables.Remove(name);

                isValid = false;
            }
        }

        /// <summary>
        /// Registered variable data.
        /// </summary>
        protected class RegisteredData
        {
            /// <summary>
            /// Reference count of how many variables reference this.
            /// </summary>
            public int refCount;
            /// <summary>
            /// The current value of the data.
            /// </summary>
            public object value;
            /// <summary>
            /// The newly assigned value of the data.
            /// </summary>
            public object newValue;
            /// <summary>
            /// The data type.
            /// </summary>
            public Type type;
            /// <summary>
            /// Flag is the value has changed to not.
            /// </summary>
            public bool dirty;

            /// <summary>
            /// Assign a new value to the data and flag as dirty.
            /// </summary>
            /// <param name="value">The value to assign.</param>
            public void Set(object value)
            {
                if (this.value != value)
                {
                    dirty = true;
                    newValue = value;
                }
            }
        }

        /// <summary>
        /// Updates all registered ClusterVariables to their most recently assigned value.
        /// </summary>
        internal static void CleanDirtyVariables()
        {
            foreach (var entry in registeredVariables)
            {
                if (entry.Value.dirty)
                {
                    // update its value
                    entry.Value.value = entry.Value.newValue;
                    entry.Value.dirty = false;
                }
            }
        }

        /// <summary>
        /// Serializes all registered ClusterVariables that have had their values updated, writing their ID, value type, and new value.
        /// </summary>
        /// <param name="writer">The ByteBufferWriter to fill with the updated data.</param>
        internal static void SerializeDirtyVariables(ByteBufferWriter writer)
        {
            // count dirty
            int count = 0;
            foreach (var entry in registeredVariables)
                if (entry.Value.dirty)
                    ++count;

            writer.Write(count);

            // write each dirty var
            foreach (var entry in registeredVariables)
            {
                if (entry.Value.dirty)
                {
                    // write the name
                    writer.Write(entry.Key);

                    // write its type and data
                    Type argType = entry.Value.type;

                    // write the data for the single element
                    switch (Type.GetTypeCode(argType))
                    {
                        case TypeCode.Boolean: writer.Write((byte)1); writer.Write((bool)entry.Value.newValue); break;
                        case TypeCode.Char: writer.Write((byte)2); writer.Write((char)entry.Value.newValue); break;
                        case TypeCode.SByte: writer.Write((byte)3); writer.Write((sbyte)entry.Value.newValue); break;
                        case TypeCode.Byte: writer.Write((byte)4); writer.Write((byte)entry.Value.newValue); break;
                        case TypeCode.Int16: writer.Write((byte)5); writer.Write((short)entry.Value.newValue); break;
                        case TypeCode.UInt16: writer.Write((byte)6); writer.Write((ushort)entry.Value.newValue); break;
                        case TypeCode.Int32: writer.Write((byte)7); writer.Write((int)entry.Value.newValue); break;
                        case TypeCode.UInt32: writer.Write((byte)8); writer.Write((uint)entry.Value.newValue); break;
                        case TypeCode.Int64: writer.Write((byte)9); writer.Write((long)entry.Value.newValue); break;
                        case TypeCode.UInt64: writer.Write((byte)10); writer.Write((ulong)entry.Value.newValue); break;
                        case TypeCode.Single: writer.Write((byte)11); writer.Write((float)entry.Value.newValue); break;
                        case TypeCode.Double: writer.Write((byte)12); writer.Write((double)entry.Value.newValue); break;
                        case TypeCode.String: writer.Write((byte)14); writer.Write((string)entry.Value.newValue); break;
                        default:
                            {
                                // vector2
                                if (argType == typeof(Vector2))
                                {
                                    writer.Write((byte)15);
                                    writer.Write((Vector2)entry.Value.newValue);
                                }
                                // vector3
                                else if (argType == typeof(Vector3))
                                {
                                    writer.Write((byte)16);
                                    writer.Write((Vector3)entry.Value.newValue);
                                }
                                // vector4
                                else if (argType == typeof(Vector4))
                                {
                                    writer.Write((byte)17);
                                    writer.Write((Vector4)entry.Value.newValue);
                                }
                                // color
                                else if (argType == typeof(Color))
                                {
                                    writer.Write((byte)18);
                                    writer.Write((Color)entry.Value.newValue);
                                }
                                // color32
                                else if (argType == typeof(Color32))
                                {
                                    writer.Write((byte)19);
                                    writer.Write((Color32)entry.Value.newValue);
                                }
                                // quaternion
                                else if (argType == typeof(Quaternion))
                                {
                                    writer.Write((byte)20);
                                    writer.Write((Quaternion)entry.Value.newValue);
                                }
                                else
                                {
                                    // invalid type!!!!!
                                                                    }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize updated ClusterVariable data from a buffer and apply the changes to the variables.
        /// </summary>
        /// <param name="reader">The ByteBufferReader to read the updated data from.</param>
        internal static void DeserializeDirtyVariables(ByteBufferReader reader)
        {
            // how many dirty variables?
            int count = reader.ReadInt();

            for (int i = 0; i < count; ++i)
            {
                // read variable name
                string name = reader.ReadString();

                // what is the variable type?
                byte argType = reader.ReadByte();

                object value = null;

                switch (argType)
                {
                    case 1: value = reader.ReadBoolean(); break;
                    case 2: value = reader.ReadChar(); break;
                    case 3: value = reader.ReadSByte(); break;
                    case 4: value = reader.ReadByte(); break;
                    case 5: value = reader.ReadShort(); break;
                    case 6: value = reader.ReadUShort(); break;
                    case 7: value = reader.ReadInt(); break;
                    case 8: value = reader.ReadUInt(); break;
                    case 9: value = reader.ReadLong(); break;
                    case 10: value = reader.ReadULong(); break;
                    case 11: value = reader.ReadFloat(); break;
                    case 12: value = reader.ReadDouble(); break;
                    case 14: value = reader.ReadString(); break;
                    case 15: value = reader.ReadVector2(); break;
                    case 16: value = reader.ReadVector3(); break;
                    case 17: value = reader.ReadVector4(); break;
                    case 18: value = reader.ReadColor(); break;
                    case 19: value = reader.ReadColor32(); break;
                    case 20: value = reader.ReadQuaternion(); break;
                    default: break;
                }

                if (value != null)
                {
                    // do we have a registered var with the correct type?
                    if (registeredVariables.ContainsKey(name))
                    {
                        if (registeredVariables[name].type == value.GetType())
                        {
                            registeredVariables[name].newValue = value;
                            registeredVariables[name].dirty = true;
                        }
                    }
                    else
                    {
                        // add it!
                        registeredVariables.Add(name, new RegisteredData() { refCount = 1, type = value.GetType(), value = value });
                    }
                }
            }
        }

        byte[] ObjectToByteArray(object obj)
        {
            if (obj != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }
            }
            return null;
        }

        /// <summary>
        /// Deregisters all ClusterVariables.
        /// </summary>
        internal static void ClearRegisteredVariables()
        {
            registeredVariables.Clear();
        }

        /// <summary>
        /// A collection of registered variables.
        /// </summary>
        protected static Dictionary<string, RegisteredData> registeredVariables = new Dictionary<string, RegisteredData>();
    }

    /// <summary>
    /// Generic Cluster Variable container.
    /// </summary>
    /// <typeparam name="T">The type that this Cluster Variable contains.</typeparam>
    public class ClusterVar<T> : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterVar(T value = default(T)) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterVar(string name, T value = default(T)) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public T GetValue()
        {
            return (T)registeredVariables[name].value;
        }
    }

    #region Cluster variable Types
    /// <summary>
    /// A clustered float value.
    /// </summary>
 /*   public class ClusterFloat : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterFloat(float value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterFloat(string name, float value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public float GetValue()
        {
            return (float)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered double value.
    /// </summary>
    public class ClusterDouble : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterDouble(double value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterDouble(string name, double value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public double GetValue()
        {
            return (double)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered char variable.
    /// </summary>
    public class ClusterChar : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterChar(char value = '\0') : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterChar(string name, char value = '\0') : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public char GetValue()
        {
            return (char)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered sbyte variable.
    /// </summary>
    public class ClusterSByte : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterSByte(sbyte value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterSByte(string name, sbyte value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public sbyte GetValue()
        {
            return (sbyte)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered byte variable.
    /// </summary>
    public class ClusterByte : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterByte(byte value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterByte(string name, byte value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public byte GetValue()
        {
            return (byte)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered short variable.
    /// </summary>
    public class ClusterShort : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterShort(short value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterShort(string name, short value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public short GetValue()
        {
            return (short)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered ushort variable.
    /// </summary>
    public class ClusterUShort : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterUShort(ushort value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterUShort(string name, ushort value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public ushort GetValue()
        {
            return (ushort)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered int variable.
    /// </summary>
    public class ClusterInt : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterInt(int value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterInt(string name, int value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public int GetValue()
        {
            return (int)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered uint variable.
    /// </summary>
    public class ClusterUInt : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterUInt(uint value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterUInt(string name, uint value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public uint GetValue()
        {
            return (uint)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered long variable.
    /// </summary>
    public class ClusterLong : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterLong(long value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterLong(string name, long value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public long GetValue()
        {
            return (long)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered ulong variable.
    /// </summary>
    public class ClusterULong : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterULong(ulong value = 0) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterULong(string name, ulong value = 0) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public ulong GetValue()
        {
            return (ulong)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered bool variable.
    /// </summary>
    public class ClusterBool : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterBool(bool value = false) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterBool(string name, bool value = false) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public bool GetValue()
        {
            return (bool)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered UnityEngine.Vector2 variable.
    /// </summary>
    public class ClusterVector2 : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        public ClusterVector2() : base(Vector2.zero)
        {
            SetValue(Vector2.zero);
        }

        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterVector2(Vector2 value) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        public ClusterVector2(string name) : base(name, Vector2.zero)
        {
            SetValue(Vector2.zero);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterVector2(string name, Vector2 value) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public Vector2 GetValue()
        {
            if (isValid)
                return (Vector2)registeredVariables[name].value;
            return Vector2.zero;
        }
    }

    /// <summary>
    /// A clustered UnityEngine.Vector3 variable.
    /// </summary>
    public class ClusterVector3 : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        public ClusterVector3() : base(Vector3.zero)
        {
            SetValue(Vector3.zero);
        }

        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterVector3(Vector3 value) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        public ClusterVector3(string name) : base(name, Vector3.zero)
        {
            SetValue(Vector3.zero);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterVector3(string name, Vector3 value) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public Vector3 GetValue()
        {
            if (isValid)
                return (Vector3)registeredVariables[name].value;
            return Vector3.zero;
        }
    }

    /// <summary>
    /// A clustered UnityEngine.Vector4 variable.
    /// </summary>
    public class ClusterVector4 : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        public ClusterVector4() : base(Vector4.zero)
        {
            SetValue(Vector4.zero);
        }

        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterVector4(Vector4 value) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        public ClusterVector4(string name) : base(name, Vector4.zero)
        {
            SetValue(Vector4.zero);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterVector4(string name, Vector4 value) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public Vector4 GetValue()
        {
            if (isValid)
                return (Vector4)registeredVariables[name].value;
            return Vector4.zero;
        }
    }

    /// <summary>
    /// A clustered UnityEngine.Color variable.
    /// </summary>
    public class ClusterColor : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        public ClusterColor() : base(Color.magenta)
        {
            SetValue(Color.magenta);
        }

        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterColor(Color value) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        public ClusterColor(string name) : base(name, Color.magenta)
        {
            SetValue(Color.magenta);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterColor(string name, Color value) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public Color GetValue()
        {
            return (Color)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered UnityEngine.Color32 variable.
    /// </summary>
    public class ClusterColor32 : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        public ClusterColor32() : base(new Color32(255, 0, 255, 255))
        {
            SetValue(new Color32(255, 0, 255, 255));
        }

        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterColor32(Color32 value) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        public ClusterColor32(string name) : base(name, new Color32(255,0,255,255))
        {
            SetValue(Color.magenta);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterColor32(string name, Color32 value) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public Color32 GetValue()
        {
            return (Color32)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered UnityEngine.Quaternion variable.
    /// </summary>
    public class ClusterQuaternion : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        public ClusterQuaternion() : base(Quaternion.identity)
        {
            SetValue(Quaternion.identity);
        }

        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterQuaternion(Quaternion value) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        public ClusterQuaternion(string name) : base(name, Quaternion.identity)
        {
            SetValue(Quaternion.identity);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterQuaternion(string name, Quaternion value) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public Quaternion GetValue()
        {
            return (Quaternion)registeredVariables[name].value;
        }
    }

    /// <summary>
    /// A clustered string variable.
    /// </summary>
    public class ClusterString : ClusterVariable
    {
        /// <summary>
        /// Create a cluster variable and assign it a generic name (i.e. "_unnamed_#").
        /// If created on a client node within a cluster then the variable will receive 
        /// updated values from the master node from a matching named variable.
        /// </summary>
        /// <param name="value">Starting value for the variable.</param>
        public ClusterString(string value) : base(value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Create a cluster variable and assign it a name.
        /// The name is used to identify the variable within the cluster. A master node will 
        /// broadcast changes to the variable, and client nodes will update the variables with 
        /// matching names so that their value matches the master.
        /// </summary>
        /// <param name="name">The name of the variable within the cluster.</param>
        /// <param name="value">The starting value of the variable.</param>
        public ClusterString(string name, string value) : base(name, value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Get the value of the variable this frame.
        /// </summary>
        /// <returns>The current value of the variable this frame.</returns>
        public string GetValue()
        {
            return (string)registeredVariables[name].value;
        }
    }*/
    #endregion
}