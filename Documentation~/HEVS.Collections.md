## `ByteBufferReader`

Utility class for reading data from a byte array.
```csharp
public class HEVS.Collections.ByteBufferReader
    : IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | Capacity | The total capacity of the buffer that we can read. | 
| `Boolean` | HasData |  | 
| `Int32` | Position | The current position we have read up to in the buffer. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | Dispose() | Deletes the buffer. | 
| `Boolean` | ReadBoolean() | Reads a single bool from the buffer, advancing the current position. | 
| `Byte` | ReadByte() | Reads a single byte from the buffer, advancing the current position. | 
| `Byte[]` | ReadByteArray() | Reads an array of bytes from the buffer, reading the amount of bytes to read first (an int), then advancing the current position. | 
| `Char` | ReadChar() | Reads a single char from the buffer, advancing the current position. | 
| `Color` | ReadColor() | Reads a single Color from the buffer, advancing the current position. | 
| `Color32` | ReadColor32() | Reads a single Color32 from the buffer, advancing the current position. | 
| `Double` | ReadDouble() | Reads a single double from the buffer, advancing the current position. | 
| `Single` | ReadFloat() | Reads a single float from the buffer, advancing the current position. | 
| `Int32` | ReadInt() | Reads a single int from the buffer, advancing the current position. | 
| `Int64` | ReadLong() | Reads a single long from the buffer, advancing the current position. | 
| `Quaternion` | ReadQuaternion() | Reads a single Quaternion from the buffer, advancing the current position. | 
| `SByte` | ReadSByte() | Reads a single sbyte from the buffer, advancing the current position. | 
| `Int16` | ReadShort() | Reads a single short from the buffer, advancing the current position. | 
| `String` | ReadString() | Reads a string from the buffer, advancing the current position. | 
| `UInt32` | ReadUInt() | Reads a single uint from the buffer, advancing the current position. | 
| `UInt64` | ReadULong() | Reads a single ulong from the buffer, advancing the current position. | 
| `UInt16` | ReadUShort() | Reads a single ushort from the buffer, advancing the current position. | 
| `Vector2` | ReadVector2() | Reads a single Vector2 from the buffer, advancing the current position. | 
| `Vector3` | ReadVector3() | Reads a single Vector3 from the buffer, advancing the current position. | 
| `Vector4` | ReadVector4() | Reads a single Vector4 from the buffer, advancing the current position. | 
| `void` | SetSource(`Byte[]` buffer, `Boolean` clone = False) | Sets the source buffer for this ByteBufferReader, and optionally clones the buffer or uses it directly. | 


## `ByteBufferWriter`

Utility class for writing data into an array of bytes.
```csharp
public class HEVS.Collections.ByteBufferWriter
    : IDisposable

```

Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Int32` | Capacity | Gets or sets the total capacity of the buffer.  Setting will overwrite any existing data. | 
| `Int32` | Length | Query the current "written" content length. | 
| `Int32` | Position | The current position we have written up to in the buffer. | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Byte[]` | AsArray() | Get this ByteBufferWriter as an array of bytes. | 
| `void` | Clear() | Resets the current position of the buffer so that all data can be overwritten. | 
| `void` | Dispose() | Deletes the contents of the buffer. | 
| `void` | Write(`String` input) | Write a string into the buffer. | 
| `void` | Write(`Byte` input) | Write a string into the buffer. | 
| `void` | Write(`SByte` input) | Write a string into the buffer. | 
| `void` | Write(`Byte[]` input) | Write a string into the buffer. | 
| `void` | Write(`Char` input) | Write a string into the buffer. | 
| `void` | Write(`Single` input) | Write a string into the buffer. | 
| `void` | Write(`Double` input) | Write a string into the buffer. | 
| `void` | Write(`Boolean` input) | Write a string into the buffer. | 
| `void` | Write(`Int16` input) | Write a string into the buffer. | 
| `void` | Write(`Int32` input) | Write a string into the buffer. | 
| `void` | Write(`Int64` input) | Write a string into the buffer. | 
| `void` | Write(`UInt16` input) | Write a string into the buffer. | 
| `void` | Write(`UInt32` input) | Write a string into the buffer. | 
| `void` | Write(`UInt64` input) | Write a string into the buffer. | 
| `void` | Write(`Vector2` input) | Write a string into the buffer. | 
| `void` | Write(`Vector3` input) | Write a string into the buffer. | 
| `void` | Write(`Vector4` input) | Write a string into the buffer. | 
| `void` | Write(`Quaternion` input) | Write a string into the buffer. | 
| `void` | Write(`Color` input) | Write a string into the buffer. | 
| `void` | Write(`Color32` input) | Write a string into the buffer. | 


