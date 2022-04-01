using System;
using System.Linq;
using UnityEngine;

namespace HEVS
{
    public partial class Config {
        /// <summary>
        /// A HEVS Transform config object.
        /// </summary>
        [Serializable]
        public struct Transform : IConfigObject
        {
            /// <summary>
            /// The ID of the transform.
            /// </summary>
            public string id;

            /// <summary>
            /// The translation component of the transform.
            /// </summary>
            public Vector3 translate;

            /// <summary>
            /// The rotation component of the transform.
            /// </summary>
            public Quaternion rotate;

            /// <summary>
            /// The scale component of the transform.
            /// </summary>
            public Vector3 scale;

            /// <summary>
            /// Convert this transform to a 4x4 Matrix.
            /// </summary>
            public Matrix4x4 matrix { get { return Matrix4x4.TRS(translate, rotate, scale); } }

            /// <summary>
            /// Access to the SimpleJSON JSON used to create this transform.
            /// </summary>
            public SimpleJSON.JSONNode json { get; private set; }

            /// <summary>
            /// Creates a default TransformConfig with an optional ID.
            /// </summary>
            /// <param name="id">Optional ID to assign to this transform.</param>
            public Transform(string id = "unknown")
            {
                this.id = id;
                translate = Vector3.zero;
                rotate = Quaternion.identity;
                scale = Vector3.one;
                json = null;
            }

            /// <summary>
            /// Populates the given UnityEngine.Transform's local data with data from this transform, then returns it.
            /// </summary>
            /// <param name="t">The UnityEngine.Transform to populate.</param>
            /// <returns>Returns the populated UnityEngine.Transform.</returns>
            public UnityEngine.Transform PopulateLocal(UnityEngine.Transform t)
            {
                t.localPosition = translate;
                t.localRotation = rotate;
                t.localScale = scale;
                return t;
            }

            /// <summary>
            /// Populates the given UnityEngine.Transform's global data with data from this transform, then returns it.
            /// Note: Scale will be applied as a local scale.
            /// </summary>
            /// <param name="t">The UnityEngine.Transform to populate.</param>
            /// <returns>Returns the populated UnityEngine.Transform.</returns>
            public UnityEngine.Transform PopulateGlobal(UnityEngine.Transform t)
            {
                t.position = translate;
                t.rotation = rotate;
                t.localScale = scale;
                return t;
            }

            /// <summary>
            /// Parse JSON data to initialise this config.
            /// </summary>
            /// <param name="json">The JSON data to parse.</param>
            /// <returns>Returns true if the data is successfully parsed, false otherwise.</returns>
            public bool Parse(SimpleJSON.JSONNode json)
            {
                this.json = json;

                if (json.Keys.Contains("id"))
                    id = json["id"];

                if (json.Keys.Contains("translate"))
                    translate = new Vector3(json["translate"][0].AsFloat, json["translate"][1].AsFloat, json["translate"][2].AsFloat);

                if (json.Keys.Contains("rotate"))
                    rotate = Quaternion.Euler(json["rotate"][0].AsFloat, json["rotate"][1].AsFloat, json["rotate"][2].AsFloat);

                if (json.Keys.Contains("scale"))
                    scale = new Vector3(json["scale"][0].AsFloat, json["scale"][1].AsFloat, json["scale"][2].AsFloat);

                return true;
            }

            /// <summary>
            /// A global "identity" TransformConfig.
            /// </summary>
            public static Transform identity = new Transform("identity");

            /// <summary>
            /// Check if this transform is equal to identity, or is "null".
            /// </summary>
            public bool isIdentity
            {
                get { return translate == Vector3.zero && rotate == Quaternion.identity && scale == Vector3.one; }
            }

            /// <summary>
            /// Combines this transform's translation and rotation with the given transform options.
            /// </summary>
            /// <param name="position">Position/translation to concatenate.</param>
            /// <param name="orientation">Rotation/orientation to concatenate.</param>
            /// <returns>Returns a combined TransformConfig.</returns>
            public Transform Concatenate(Vector3 position, Quaternion orientation)
            {
                Transform td = new Transform();
                td.translate = rotate * position + translate;
                td.rotate = rotate * orientation;
                return td;
            }

            /// <summary>
            /// Combines this transform's translation and rotation with another transform.
            /// </summary>
            /// <param name="transform">The transform to combine with this transform.</param>
            /// <returns>Returns a combined TransformConfig.</returns>
            public Transform Concatenate(Transform transform)
            {
                Transform td = new Transform();
                td.translate = rotate * transform.translate + translate;
                td.rotate = rotate * transform.rotate;
                return td;
            }

            /// <summary>
            /// Combines this transform's translation and rotation with another transform.
            /// </summary>
            /// <param name="transform">The transform to combine with this transform.</param>
            /// <returns>Returns a combined TransformConfig.</returns>
            public Transform PostConcatenate(Transform transform)
            {
                Transform td = new Transform();
                td.translate = transform.rotate * translate + transform.translate;
                td.rotate = transform.rotate * rotate;
                return td;
            }

            /// <summary>
            /// Combines this transform's translation and rotation with another transform.
            /// </summary>
            /// <param name="transform">The transform to combine with this transform.</param>
            /// <returns>Returns a combined TransformConfig.</returns>
            public Transform Concatenate(UnityEngine.Transform transform)
            {
                Transform td = new Transform();
                td.translate = rotate * transform.position + translate;
                td.rotate = rotate * transform.rotation;
                return td;
            }

            /// <summary>
            /// Combines this transform's translation and rotation with another transform.
            /// </summary>
            /// <param name="transform">The transform to combine with this transform.</param>
            /// <returns>Returns a combined TransformConfig.</returns>
            public Transform PostConcatenate(UnityEngine.Transform transform)
            {
                Transform td = new Transform();
                td.translate = transform.rotation * translate + transform.position;
                td.rotate = transform.rotation * rotate;
                return td;
            }

            /// <summary>
            /// Transforms a point by this transform's orientation and translation. Scale is ignored.
            /// </summary>
            /// <param name="point">The point to transform.</param>
            /// <returns>Returns the transformed point.</returns>
            public Vector3 TransformPoint(Vector3 point)
            {
                return rotate * point + translate;
            }

            /// <summary>
            /// Transforms an orientation by this transform's orientation. Translation and scale is ignored.
            /// </summary>
            /// <param name="rotation">The rotation to transform.</param>
            /// <returns>Returns the transformed rotation.</returns>
            public Quaternion TransformRotation(Quaternion rotation)
            {
                return rotate * rotation;
            }
        }
    }
}