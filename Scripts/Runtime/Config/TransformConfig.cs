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
        public class Transform : IConfigObject
        {
            /// <summary>
            /// The ID of the transform.
            /// </summary>
            public string id;

            /// <summary>
            /// The translation component of the transform.
            /// </summary>
            Vector3? _translate;

            public bool HasTranslation => _translate.HasValue;

            public Vector3 Translation => _translate.HasValue ? _translate.Value : Vector3.zero;

            /// <summary>
            /// The rotation component of the transform.
            /// </summary>
            Quaternion? _rotate;

            public bool HasRotation => _rotate.HasValue;

            public Quaternion Rotation => _rotate.HasValue ? _rotate.Value : Quaternion.identity;

            /// <summary>
            /// The scale component of the transform.
            /// </summary>
            Vector3? _scale;

            public bool HasScale => _scale.HasValue;

            public Vector3 Scale => _scale.HasValue ? _scale.Value : Vector3.one;

            /// <summary>
            /// Convert this transform to a 4x4 Matrix.
            /// </summary>
            public Matrix4x4 matrix => Matrix4x4.TRS(_translate.HasValue ? _translate.Value : Vector3.zero, 
                _rotate.HasValue ? _rotate.Value : Quaternion.identity, 
                _scale.HasValue ? _scale.Value : Vector3.one); 

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
                json = null;
            }

            /// <summary>
            /// Creates a TransformConfig from values with an optional ID.
            /// </summary>
            /// <param name="translation">Translation</param>
            /// <param name="rotation">Rotation</param>
            /// <param name="scale">Scale</param>
            /// <param name="id">Optional ID to assign to this transform.</param>
            public Transform(Vector3 translation, Quaternion rotation, Vector3 scale, string id = "unknown")
            {
                this.id = id;
                _translate = translation;
                _rotate = rotation;
                _scale = scale;
                json = null;
            }

            /// <summary>
            /// Populates the given UnityEngine.Transform's local data with data from this transform, then returns it.
            /// </summary>
            /// <param name="t">The UnityEngine.Transform to populate.</param>
            /// <returns>Returns the populated UnityEngine.Transform.</returns>
            public UnityEngine.Transform PopulateLocal(UnityEngine.Transform t)
            {
                t.localPosition = _translate.HasValue ? _translate.Value : Vector3.zero;
                t.localRotation = _rotate.HasValue ? _rotate.Value : Quaternion.identity;
                t.localScale = _scale.HasValue ? _scale.Value : Vector3.one;
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
                t.position = _translate.HasValue ? _translate.Value : Vector3.zero;
                t.rotation = _rotate.HasValue ? _rotate.Value : Quaternion.identity;
                t.localScale = _scale.HasValue ? _scale.Value : Vector3.one;
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
                    _translate = new Vector3(json["translate"][0].AsFloat, json["translate"][1].AsFloat, json["translate"][2].AsFloat);

                if (json.Keys.Contains("rotate"))
                    _rotate = Quaternion.Euler(json["rotate"][0].AsFloat, json["rotate"][1].AsFloat, json["rotate"][2].AsFloat);

                if (json.Keys.Contains("scale"))
                    _scale = new Vector3(json["scale"][0].AsFloat, json["scale"][1].AsFloat, json["scale"][2].AsFloat);

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
                get { return Translation == Vector3.zero && Rotation == Quaternion.identity && Scale == Vector3.one; }
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
                if (HasRotation)
                {
                    td._translate = _rotate.Value * position;
                    td._rotate = _rotate.Value * orientation;
                }
                if (HasTranslation)
                    td._translate += _translate.Value;

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
                if (HasRotation)
                {
                    td._translate = _rotate.Value * transform.Translation;
                    td._rotate = _rotate.Value * transform.Rotation;
                }
                if (HasTranslation)
                    td._translate += _translate.Value;

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
                if (HasRotation)
                    td._rotate = transform.Rotation * _rotate.Value;
                if (HasTranslation)
                    td._translate = transform.Rotation * _translate.Value + transform.Translation;

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
                if (HasRotation)
                {
                    td._translate = _rotate.Value * transform.position;
                    td._rotate = _rotate.Value * transform.rotation;
                }
                if (HasTranslation)
                    td._translate += _translate.Value;
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
                if (HasRotation)
                    td._rotate = transform.rotation * _rotate.Value;
                if (HasTranslation)
                    td._translate += transform.rotation * _translate.Value + transform.position;
                return td;
            }

            /// <summary>
            /// Transforms a point by this transform's orientation and translation. Scale is ignored.
            /// </summary>
            /// <param name="point">The point to transform.</param>
            /// <returns>Returns the transformed point.</returns>
            public Vector3 TransformPoint(Vector3 point)
            {
                return Rotation * point + Translation;
            }

            /// <summary>
            /// Transforms an orientation by this transform's orientation. Translation and scale is ignored.
            /// </summary>
            /// <param name="rotation">The rotation to transform.</param>
            /// <returns>Returns the transformed rotation.</returns>
            public Quaternion TransformRotation(Quaternion rotation)
            {
                return Rotation * rotation;
            }
        }
    }
}