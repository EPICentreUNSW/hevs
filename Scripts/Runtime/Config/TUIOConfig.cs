using System.Linq;

namespace HEVS
{
    /// <summary>
    /// The type of the TUIO input message.
    /// </summary>
    public enum TUIOInputType
    {
        /// <summary>
        /// Pointer.
        /// </summary>
        Cursors = 1 << 0,
        /// <summary>
        /// Shape.
        /// </summary>
        Blobs = 1 << 1,
        /// <summary>
        /// Tagged object.
        /// </summary>
        Objects = 1 << 2
    }

    public partial class Config
    {
        /// <summary>
        /// A TUIO receiver object used to setup communication with a TUIO broadcaster.
        /// </summary>
        public class TUIODevice : IConfigObject
        {
            /// <summary>
            /// Construct a new TUIO receiver with a given ID.
            /// </summary>
            /// <param name="id">The ID to assign to this tracker.</param>
            public TUIODevice(string id) { this.id = id; }

            /// <summary>
            /// The string ID of this TUIO receiver.
            /// </summary>
            public string id;

            /// <summary>
            /// The port used to receive TUIO messages.
            /// </summary>
            public int port = 3333;

            /// <summary>
            /// The supported TUIO input types that this receiver can receiver.
            /// (Note: for now we only support cursors).
            /// </summary>
            public TUIOInputType supportedInput { get { return TUIOInputType.Cursors; } private set { } }

            /// <summary>
            /// The JSON dictionary used to parse the TUIO object.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// Parse JSON data to initialise this config.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <returns>Returns true if the data is successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                this.json = json;

                if (json.Keys.Contains("port"))
                    port = json["port"].AsInt;

                return true;
            }
        }
    }
}