using System.Linq;
using UnityEngine;

namespace HEVS
{
    public partial class Config {
        /// <summary>
        /// Viewport JSON object that can represent a relative or absolute viewport size.
        /// </summary>
        public class Viewport : IConfigObject
        {
            /// <summary>
            /// Constructs a default viewport with a specified ID.
            /// </summary>
            /// <param name="id">The ID for this viewport.</param>
            public Viewport(string id) { this.id = id; }

            /// <summary>
            /// The string ID of the viewport.
            /// </summary>
            public string id;

            /// <summary>
            /// Absolute viewports specify dimensions in pixel coordinates. Non-absolute use normalized 0-to-1 screen space.
            /// </summary>
            public bool absolute = false;

            /// <summary>
            /// The dimensions of the viewport in absolute pixel coordinates or non-absolute normalized coordinates.
            /// Note: Starting left coordinate.
            /// </summary>
            public float x = 0;
            /// <summary>
            /// The dimensions of the viewport in absolute pixel coordinates or non-absolute normalized coordinates.
            /// Note: Starting lower coordinate.
            /// </summary>
            public float y = 0;
            /// <summary>
            /// The dimensions of the viewport in absolute pixel coordinates or non-absolute normalized coordinates.
            /// Note: Width of the viewport.
            /// </summary>
            public float width = 1;
            /// <summary>
            /// The dimensions of the viewport in absolute pixel coordinates or non-absolute normalized coordinates.
            /// Note: Height of the viewport.
            /// </summary>
            public float height = 1;

            /// <summary>
            /// Returns the dimensions of this viewport as a Rect.
            /// </summary>
            /// <returns>Returns the dimensions of this viewport as a Rect.</returns>
            public Rect AsRect() { return new Rect(x, y, width, height); }

            /// <summary>
            /// Returns the dimensions of this viewport as a Vector4.
            /// </summary>
            /// <returns>Returns the dimensions of this viewport as a Vector4.</returns>
            public Vector4 AsVector4() { return new Vector4(x, y, width, height); }

            /// <summary>
            /// Access the starting XY position of the viewport.
            /// </summary>
            public Vector2 xy { get { return new Vector2(x, y); } }

            /// <summary>
            /// Access the width and height of the viewport as a Vector2.
            /// </summary>
            public Vector2 dimensions { get { return new Vector2(width, height); } }

            /// <summary>
            /// The JSON dictionary used to parse the viewport.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// Access to the screen rectangle for the viewport.
            /// </summary>
            public Rect screenRect
            {
                get
                {
                    Rect viewport = new Rect(0, 0, 1, 1);
                    if (viewport != null)
                    {
                        // resize viewport
                        if (absolute)
                        {
                            viewport.x = x / Screen.width;
                            viewport.y = y / Screen.height;
                            viewport.width = width / Screen.width;
                            viewport.height = height / Screen.height;
                        }
                        else
                        {
                            viewport.x = x;
                            viewport.y = y;
                            viewport.width = width;
                            viewport.height = height;
                        }
                    }
                    return viewport;
                }
            }

            /// <summary>
            /// Parse JSON data to initialise this config.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <returns>Returns true if the data is successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                this.json = json;
                if (json.Keys.Contains("absolute"))
                    absolute = json["absolute"].AsBool;
                if (json.Keys.Contains("x"))
                    x = json["x"].AsFloat;
                if (json.Keys.Contains("y"))
                    y = json["y"].AsFloat;
                if (json.Keys.Contains("width"))
                    width = json["width"].AsFloat;
                if (json.Keys.Contains("height"))
                    height = json["height"].AsFloat;
                return true;
            }
        }
    }
}