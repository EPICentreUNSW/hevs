using System;
using UnityEngine;

namespace HEVS.Collections
{
    /// <summary>
    /// Utility class for writing data into an array of bytes.
    /// </summary>
    public class ByteBufferWriter : IDisposable
    {
        byte[] buffer;
        int position;
        int capacity;

        /// <summary>
        /// The current position we have written up to in the buffer.
        /// </summary>
        public int Position { get { return position; } set { if (value >= 0 && value < buffer.Length) position = value; else throw new UnityException("Invalid position for ByteBufferReader"); } }

        /// <summary>
        /// Get this ByteBufferWriter as an array of bytes.
        /// </summary>
        /// <returns>Returns this buffer as an array of bytes.</returns>
        public byte[] AsArray() { return buffer; }
        
        /// <summary>
        /// Gets or sets the total capacity of the buffer.
        /// Setting will overwrite any existing data.
        /// </summary>
        public int Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                if (capacity == value)
                    return;
                buffer = new byte[value];
                capacity = value;
                position = 0;
            }
        }

        /// <summary>
        /// Resets the current position of the buffer so that all data can be overwritten.
        /// </summary>
        public void Clear()
        {
            position = 0;
        }

        /// <summary>
        /// Query the current "written" content length.
        /// </summary>
        public int Length { get { return position; } }

        /// <summary>
        /// Create a buffer with a starting capacity.
        /// </summary>
        /// <param name="capacity"></param>
        public ByteBufferWriter(int capacity)
        {
            this.capacity = capacity;
            this.position = 0;
            buffer = new byte[capacity];
        }

        /// <summary>
        /// Deletes the contents of the buffer.
        /// </summary>
        public void Dispose()
        {
            buffer = null;
            capacity = 0;
            position = 0;
        }

        /// <summary>
        /// Write a string into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(string input)
        {
            // write length of string
            Write(input.Length);
            // write each entry
            foreach (char c in input)
                Write(c);
        }

        /// <summary>
        /// Write a byte into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(byte input)
        {
            buffer[position++] = input;
        }

        /// <summary>
        /// Write a sbyte into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(sbyte input)
        {
            Write(Convert.ToByte(input));
        }

        /// <summary>
        /// Write a byte array into the buffer, writing its length first (int.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(byte[] input)
        {
            foreach (byte b in BitConverter.GetBytes(input.Length))
                buffer[position++] = b;
            foreach (byte b in input)
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a char into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(char input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a float into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(float input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a double into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(double input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a bool into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(bool input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a short into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(short input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a int into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(int input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a long into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(long input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a ushort into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(ushort input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a uint into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(uint input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a ulong into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(ulong input)
        {
            foreach (byte b in BitConverter.GetBytes(input))
                buffer[position++] = b;
        }

        /// <summary>
        /// Write a Vector2 into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(Vector2 input)
        {
            Write(input.x);
            Write(input.y);
        }

        /// <summary>
        /// Write a Vector3 into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(Vector3 input)
        {
            Write(input.x);
            Write(input.y);
            Write(input.z);
        }

        /// <summary>
        /// Write a Vector4 into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(Vector4 input)
        { 
            Write(input.x);
            Write(input.y);
            Write(input.z);
            Write(input.w);
        }

        /// <summary>
        /// Write a Quaternion into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(Quaternion input)
        {
            Write(input.x);
            Write(input.y);
            Write(input.z);
            Write(input.w);
        }

        /// <summary>
        /// Write a Color into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(Color input)
        {
            Write(input.r);
            Write(input.g);
            Write(input.b);
            Write(input.a);
        }

        /// <summary>
        /// Write a Color32 into the buffer.
        /// </summary>
        /// <param name="input">The content to write.</param>
        public void Write(Color32 input)
        {
            Write(input.r);
            Write(input.g);
            Write(input.b);
            Write(input.a);
        }
    }

    /// <summary>
    /// Utility class for reading data from a byte array.
    /// </summary>
    public class ByteBufferReader : IDisposable
    {
        int position = 0;
        byte[] buffer;

        public bool HasData { get { return Position < Capacity; } }

        /// <summary>
        /// The current position we have read up to in the buffer.
        /// </summary>
        public int Position { get { return position; } set { if (value >= 0 && value < buffer.Length) position = value; else throw new UnityException("Invalid position for ByteBufferReader"); } }

        /// <summary>
        /// The total capacity of the buffer that we can read.
        /// </summary>
        public int Capacity { get { return buffer.Length; } }

        /// <summary>
        /// Constructs a ByteBufferReader, using an optional source array and flags to clone the data or use the original source directly.
        /// </summary>
        /// <param name="buffer">The source buffer.</param>
        /// <param name="clone">Should this ByteBufferReader clone the source, or use it directly.</param>
        public ByteBufferReader(byte[] buffer = null, bool clone = false) { if (clone) this.buffer = buffer.Clone() as byte[]; else this.buffer = buffer; }

        /// <summary>
        /// Sets the source buffer for this ByteBufferReader, and optionally clones the buffer or uses it directly.
        /// </summary>
        /// <param name="buffer">The source buffer.</param>
        /// <param name="clone">Should this ByteBufferReader clone the source, or use it directly.</param>
        public void SetSource(byte[] buffer, bool clone = false) { if (clone) this.buffer = buffer.Clone() as byte[]; else this.buffer = buffer; position = 0; }

        /// <summary>
        /// Deletes the buffer.
        /// </summary>
        public void Dispose()
        {
            buffer = null;
            position = 0;
        }

        /// <summary>
        /// Reads a single byte from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the byte read.</returns>
        public byte ReadByte() { return buffer[position++]; }

        /// <summary>
        /// Reads a single sbyte from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the sbyte read.</returns>
        public sbyte ReadSByte() { return Convert.ToSByte(buffer[position++]); }

        /// <summary>
        /// Reads an array of bytes from the buffer, reading the amount of bytes to read first (an int), then advancing the current position.
        /// </summary>
        /// <returns>Returns the byte array read.</returns>
        public byte[] ReadByteArray()
        {
            int length = ReadInt();
            byte[] v = new byte[length];
            for (int i = 0; i < length; i++)
                v[i] = buffer[position++];
            return v;
        }

        /// <summary>
        /// Reads a string from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the string read.</returns>
        public string ReadString()
        {
            string str = null;
            // how many chars?
            int count = ReadInt();
            if (count > 0)
                str = "";
            for (int i = 0; i < count; i++)
                str += ReadChar();
            return str;
        }

        /// <summary>
        /// Reads a single char from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the char read.</returns>
        public char ReadChar()
        {
            char v = BitConverter.ToChar(buffer,position);
            position += 2;
            return v;
        }

        /// <summary>
        /// Reads a single float from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the float read.</returns>
        public float ReadFloat()
        {
            float v = BitConverter.ToSingle(buffer, position);
            position += 4;
            return v;
        }

        /// <summary>
        /// Reads a single double from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the double read.</returns>
        public double ReadDouble()
        {
            double v = BitConverter.ToDouble(buffer, position);
            position += 8;
            return v;
        }

        /// <summary>
        /// Reads a single bool from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the bool read.</returns>
        public bool ReadBoolean()
        {
            bool v = BitConverter.ToBoolean(buffer, position);
            position += 1;
            return v;
        }

        /// <summary>
        /// Reads a single short from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the short read.</returns>
        public short ReadShort()
        {
            short v = BitConverter.ToInt16(buffer, position);
            position += 2;
            return v;
        }

        /// <summary>
        /// Reads a single int from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the int read.</returns>
        public int ReadInt()
        {
            int v = BitConverter.ToInt32(buffer, position);
            position += 4;
            return v;
        }

        /// <summary>
        /// Reads a single long from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the long read.</returns>
        public long ReadLong()
        {
            long v = BitConverter.ToInt64(buffer, position);
            position += 8;
            return v;
        }

        /// <summary>
        /// Reads a single ushort from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the ushort read.</returns>
        public ushort ReadUShort()
        {
            ushort v = BitConverter.ToUInt16(buffer, position);
            position += 2;
            return v;
        }

        /// <summary>
        /// Reads a single uint from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the uint read.</returns>
        public uint ReadUInt()
        {
            uint v = BitConverter.ToUInt32(buffer, position);
            position += 4;
            return v;
        }

        /// <summary>
        /// Reads a single ulong from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the ulong read.</returns>
        public ulong ReadULong()
        {
            ulong v = BitConverter.ToUInt64(buffer, position);
            position += 8;
            return v;
        }

        /// <summary>
        /// Reads a single Vector2 from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the Vector2 read.</returns>
        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        /// <summary>
        /// Reads a single Vector3 from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the Vector3 read.</returns>
        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        /// <summary>
        /// Reads a single Vector4 from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the Vector4 read.</returns>
        public Vector4 ReadVector4()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        /// <summary>
        /// Reads a single Quaternion from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the Quaternion read.</returns>
        public Quaternion ReadQuaternion()
        {
            Quaternion q = new Quaternion();
            q.x = ReadFloat();
            q.y = ReadFloat();
            q.z = ReadFloat();
            q.w = ReadFloat();
            return q;
        }

        /// <summary>
        /// Reads a single Color from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the Color read.</returns>
        public Color ReadColor()
        {
            Color q = new Color();
            q.r = ReadFloat();
            q.g = ReadFloat();
            q.b = ReadFloat();
            q.a = ReadFloat();
            return q;
        }

        /// <summary>
        /// Reads a single Color32 from the buffer, advancing the current position.
        /// </summary>
        /// <returns>Returns the Color32 read.</returns>
        public Color32 ReadColor32()
        {
            Color32 q = new Color32();
            q.r = ReadByte();
            q.g = ReadByte();
            q.b = ReadByte();
            q.a = ReadByte();
            return q;
        }
    }
}