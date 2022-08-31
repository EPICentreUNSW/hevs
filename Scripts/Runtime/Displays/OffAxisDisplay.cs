using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using HEVS.Extensions;

namespace HEVS
{
    /// <summary>
    /// Display class for off-axis flat-panel displays, typical of CAVE installations.
    /// Options are read from the JSON configuration and then used to setup a display rig.
    /// </summary>
    [HEVS.CustomDisplay("offaxis")]
    public class OffAxisDisplay : Display
    {
        /// <summary>
        /// The physical upper-left corner of the display.
        /// </summary>
        public Vector3 ul { get; private set; } = Vector3.zero;
        /// <summary>
        /// The physical lower-left corner of the display.
        /// </summary>
        public Vector3 ll { get; private set; } = Vector3.zero;
        /// <summary>
        /// The physical lower-right corner of the display.
        /// </summary>
        public Vector3 lr { get; private set; } = Vector3.zero;
        /// <summary>
        /// The physical upper-right corner of the display.
        /// </summary>
        public Vector3 ur { get { return lr + (ul - ll); } }
        /// <summary>
        /// The orientation of the display.
        /// </summary>
        public Quaternion orientation { get { return Quaternion.LookRotation(-facing, up); } }
        /// <summary>
        /// The physical center of the display.
        /// </summary>
        public Vector3 center { get { return (ll + lr) * 0.5f + (ul - ll) * 0.5f; } }
        /// <summary>
        /// The physical width and height of the display.
        /// </summary>
        public Vector2 size { get { return new Vector2((ll - lr).magnitude, (ll - ul).magnitude); } }
        /// <summary>
        /// The facing direction of the display.
        /// </summary>
        public Vector3 facing { get { return Vector3.Cross(up, right).normalized; } }
        /// <summary>
        /// The right direction of the display.
        /// </summary>
        public Vector3 right { get { return (lr - ll).normalized; } }
        /// <summary>
        /// The up direction of the display.
        /// </summary>
        public Vector3 up { get { return (ul - ll).normalized; } }

        /// <summary>
        /// Create a off-axis display from a specified display config.
        /// </summary>
        /// <param name="config">The config to create the display from.</param>
        public OffAxisDisplay(Config.Display config) : base(config)
        {
            if (config.json.Keys.Contains("ul"))
                ul = new Vector3(config.json["ul"][0].AsFloat, config.json["ul"][1].AsFloat, config.json["ul"][2].AsFloat);

            if (config.json.Keys.Contains("ll"))
                ll = new Vector3(config.json["ll"][0].AsFloat, config.json["ll"][1].AsFloat, config.json["ll"][2].AsFloat);

            if (config.json.Keys.Contains("lr"))
                lr = new Vector3(config.json["lr"][0].AsFloat, config.json["lr"][1].AsFloat, config.json["lr"][2].AsFloat);
        }

        /// <summary>
        /// Creates a cloned copy of this display.
        /// </summary>
        /// <returns>Returns the cloned copy of this display.</returns>
        public override Display Clone()
        {
            return new OffAxisDisplay(config);
        }

        /// <summary>
        /// Sets up the Unity scene to use this display.
        /// </summary>
        public override void ConfigureDisplayForScene()
        {
            captureCameras.Clear();
            outputCameras.Clear();

            UnityEngine.Camera leftCamera = null, rightCamera = null, singleCamera = null;

            // setup capture camera
            if (config.requiresDualCameras)
            {
                leftCamera = Camera.ConfigureCaptureCamera(gameObject, this, 2, StereoTargetEyeMask.Left);
                captureCameras.Add(leftCamera);

                rightCamera = Camera.ConfigureCaptureCamera(gameObject, this, 1, StereoTargetEyeMask.Right);
                captureCameras.Add(rightCamera);

                // add offaxis components
                leftCamera.gameObject.GetOrAddComponent<OffAxisCameraExtension>().eye = StereoTargetEyeMask.Left;
                rightCamera.gameObject.GetOrAddComponent<OffAxisCameraExtension>().eye = StereoTargetEyeMask.Right;
            }
            else
            {
                singleCamera = gameObject.GetOrAddComponent<UnityEngine.Camera>();
                captureCameras.Add(singleCamera);

                // copy over camera settings from original camera
                if (singleCamera != Camera.main)
                    singleCamera.CopyFrom(Camera.main);

                if (config.stereoMode == StereoMode.LeftOnly)
                    Camera.ConfigureCaptureCamera(singleCamera, this, StereoTargetEyeMask.Left);
                else if (config.stereoMode == StereoMode.RightOnly)
                    Camera.ConfigureCaptureCamera(singleCamera, this, StereoTargetEyeMask.Right);
                else
                    Camera.ConfigureCaptureCamera(singleCamera, this, StereoTargetEyeMask.Both);
            }

            // attach off-axis component
            foreach (var c in captureCameras) c.gameObject.GetOrAddComponent<OffAxisCameraExtension>().display = this;

            outputCameras.AddRange(Camera.ConfigureOutputCameras(this, singleCamera ? singleCamera : leftCamera, rightCamera));
        }

        /// <summary>
        /// Cast a world-space ray into this display and return if there is an 
        /// intersection, along with distance to the intersection and the 
        /// relative 2D intersection point on the display. Only applies to displays 
        /// that exist within world-space.
        /// </summary>
        /// <param name="ray">The world-space ray.</param>
        /// <param name="distance">The returned distance to the intersection, or 0 if no intersection.</param>
        /// <param name="hitPoint2D">The returned 2D relative display-space intersection point.</param>
        /// <returns>Returns true if the world-space ray intersects the display, otherwise it returns false.</returns>
        public override bool Raycast(Ray ray, out float distance, out Vector2 hitPoint2D)
        {
            var sp = SceneOrigin.position;
            var sr = SceneOrigin.rotation;

            if (config.transform != null)
            {
                if (config.transform.HasTranslation)
                    sp += SceneOrigin.rotation * config.transform.Translation;
                if (config.transform.HasRotation)
                    sr *= config.transform.Rotation;
            }

            Plane plane = new Plane(sr * facing, sr * ll + sp);

            float d = float.MaxValue;

            if (plane.Raycast(ray, out d))
            {
                Vector3 p = ray.origin + ray.direction * d;

                // is point actually in the screen
                float t = Vector3.Dot(p - (sr * ll + sp), sr * right);
                if (t >= 0 && t <= size.x)
                {
                    float t2 = Vector3.Dot(p - (sr * ll + sp), sr * up);
                    if (t2 >= 0 && t2 <= size.y)
                    {
                        distance = d;
                        hitPoint2D = new Vector2(t, t2);
                        return true;
                    }
                }
            }

            distance = 0;
            hitPoint2D = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Get a world-space ray from the display via a 2D display-space point.
        /// </summary>
        /// <param name="displayspacePoint">The 2D display-space point to get the ray from, which is in [0,1] range for each axis.</param>
        /// <returns>Returns the world-space ray that passes through the display-space point.</returns>
        public override Ray ViewportPointToRay(Vector2 displayspacePoint)
        {
            return primaryCaptureCamera.ViewportPointToRay(displayspacePoint);
        }

        /// <summary>
        /// Set the clear flag for all cameras.
        /// </summary>
        /// <param name="flags">CameraClearFlags to set.</param>
        public override void SetClearFlags(CameraClearFlags flags)
        {
            foreach (var camera in captureCameras)
                camera.clearFlags = flags;
        }

        /// <summary>
        /// Set the clear colour for all cameras.
        /// </summary>
        /// <param name="colour">The colour to set.</param>
        public override void SetBackgroundColour(Color colour)
        {
            foreach (var camera in captureCameras)
                camera.backgroundColor = colour;
        }

        /// <summary>
        /// A method to draw the display's Gizmo within the editor.
        /// </summary>
        /// <param name="config">The config data to use for drawing a gizmo of this type.</param>
        public static void DrawGizmo(Config.Display config)
        {
            var sp = SceneOrigin.position;
            var sr = SceneOrigin.rotation;

            if (config.transform != null)
            {
                if (config.transform.HasTranslation)
                    sp += SceneOrigin.rotation * config.transform.Translation;
                if (config.transform.HasRotation)
                    sr *= config.transform.Rotation;
            }

            Vector3 ul = Vector3.zero;
            Vector3 ll = Vector3.zero;
            Vector3 lr = Vector3.zero;

            if (config.json.Keys.Contains("ul"))
                ul = new Vector3(config.json["ul"][0].AsFloat, config.json["ul"][1].AsFloat, config.json["ul"][2].AsFloat);

            if (config.json.Keys.Contains("ll"))
                ll = new Vector3(config.json["ll"][0].AsFloat, config.json["ll"][1].AsFloat, config.json["ll"][2].AsFloat);

            if (config.json.Keys.Contains("lr"))
                lr = new Vector3(config.json["lr"][0].AsFloat, config.json["lr"][1].AsFloat, config.json["lr"][2].AsFloat);

            Gizmos.DrawLine(sr * ul + sp, sr * ll + sp);
            Gizmos.DrawLine(sr * lr + sp, sr * ll + sp);
            Gizmos.DrawLine(sr * lr + sp, sr * (lr + (ul - ll)) + sp);
            Gizmos.DrawLine(sr * ul + sp, sr * (lr + (ul - ll)) + sp);
        }

        /// <summary>
        /// Gather the geometry that represents this display.
        /// </summary>
        /// <param name="vertices">The vertices that represent the display's mesh.</param>
        /// <param name="indices">The indices used to define the display's mesh topography.</param>
        /// <param name="uvs">The texture coordinates to apply across the display's mesh.</param>
        public override void GatherDisplayGeometry(List<Vector3> vertices, List<int> indices, List<Vector2> uvs)
        {
            vertices.Clear();
            vertices.Add(ul);
            vertices.Add(ll);
            vertices.Add(lr);
            vertices.Add(ur);
            indices.Clear();
            indices.Add(0);
            indices.Add(2);
            indices.Add(1);
            indices.Add(0);
            indices.Add(3);
            indices.Add(2);
            uvs.Clear();
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
        }

        #region Helper Methods
        /// <summary>
        /// Get the display's "right" direction.
        /// </summary>
        /// <param name="offset">Transform offset to apply to the display.</param>
        /// <param name="orientation">Transform orientation to apply to the display.</param>
        /// <returns>Returns the display's "right" direction.</returns>
        public Vector3 GetRight(Vector3 offset, Quaternion orientation)
        {
            // transform the screen layout
            var LR = orientation * lr + offset;
            var LL = orientation * ll + offset;

            return (LR - LL).normalized;
        }

        /// <summary>
        /// Get the display's center position.
        /// </summary>
        /// <param name="offset">Transform offset to apply to the display.</param>
        /// <param name="orientation">Transform orientation to apply to the display.</param>
        /// <returns>Returns the display's center position.</returns>
        public Vector3 GetCenter(Vector3 offset, Quaternion orientation)
        {
            // transform the screen layout
            var LR = orientation * lr + offset;
            var LL = orientation * ll + offset;
            var UL = orientation * ul + offset;

            // calculate physical screen orientation
            var vr = (LR - LL).normalized;
            var vu = (UL - LL).normalized;

            return LL + vr * 0.5f + vu * 0.5f;
        }

        /// <summary>
        /// Get the display's "forward" direction.
        /// </summary>
        /// <param name="offset">Transform offset to apply to the display.</param>
        /// <param name="orientation">Transform orientation to apply to the display.</param>
        /// <returns>Returns the display's "forward" direction.</returns>
        public Vector3 GetFacing(Vector3 offset, Quaternion orientation)
        {
            // transform the screen layout
            var LR = orientation * lr + offset;
            var LL = orientation * ll + offset;
            var UL = orientation * ul + offset;

            // calculate physical screen orientation
            var vr = (LR - LL).normalized;
            var vu = (UL - LL).normalized;
            var vn = -Vector3.Cross(vu, vr).normalized;

            return vn;
        }
        #endregion

        #region View Matrix Methods
        /// <summary>
        /// Get the display's view matrix for a specific viewer.
        /// </summary>
        /// <param name="eyePos">The position of the viewer.</param>
        /// <returns>Returns the display's view matrix.</returns>
        public Matrix4x4 GetViewFrom(Vector3 eyePos)
        {
            return GetViewFrom(eyePos, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Get the display's view matrix for a specific viewer.
        /// </summary>
        /// <param name="viewer">The position of the viewer.</param>
        /// <returns>Returns the display's view matrix.</returns>
        public Matrix4x4 GetViewFrom(Transform viewer)
        {
            return GetViewFrom(viewer.position, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Get the display's view matrix for a specific viewer.
        /// </summary>
        /// <param name="viewer">The position of the viewer.</param>
        /// <param name="offset">Transform offset of the display.</param>
        /// <param name="facing">Transform orientation of the display.</param>
        /// <returns>Returns the display's view matrix.</returns>
        public Matrix4x4 GetViewFrom(Transform viewer, Vector3 offset, Quaternion facing)
        {
            return GetViewFrom(viewer.position, offset, facing);
        }

        /// <summary>
        /// Get the display's view matrix for a specific viewer.
        /// </summary>
        /// <param name="eyePos">The position of the viewer.</param>
        /// <param name="offset">Transform offset of the display.</param>
        /// <param name="facing">Transform orientation of the display.</param>
        /// <returns>Returns the display's view matrix.</returns>
        public Matrix4x4 GetViewFrom(Vector3 eyePos, Vector3 offset, Quaternion facing)
        {
            // transform the screen layout
            var LR = facing * lr + offset;
            var LL = facing * ll + offset;
            var UL = facing * ul + offset;

            // calculate physical screen orientation
            var vr = (LR - LL).normalized;
            var vu = (UL - LL).normalized;
            var vn = Vector3.Cross(vu, vr).normalized;

            // calculate transform
            Matrix4x4 basis = new Matrix4x4();
            basis[0, 0] = vr[0];
            basis[0, 1] = vr[1];
            basis[0, 2] = vr[2];
            basis[0, 3] = 0;
            basis[1, 0] = vu[0];
            basis[1, 1] = vu[1];
            basis[1, 2] = vu[2];
            basis[1, 3] = 0;
            basis[2, 0] = vn[0];
            basis[2, 1] = vn[1];
            basis[2, 2] = vn[2];
            basis[2, 3] = 0;
            basis[3, 0] = 0; // -eyePos.x
            basis[3, 1] = 0; // -eyePos.y
            basis[3, 2] = 0; // -eyePos.z
            basis[3, 3] = 1;
            return basis * Matrix4x4.Translate(-eyePos);
        }
        #endregion

        #region Projection Methods
        /// <summary>
        /// Get the projection matrix for this display from a specific viewer, with a specific near and far plane distance.
        /// </summary>
        /// <param name="viewer">The position of the viewer.</param>
        /// <param name="near">Distance to the near plane.</param>
        /// <param name="far">Distance to the far plane.</param>
        /// <returns>Returns the projection matrix of this display.</returns>
        public Matrix4x4 GetProjectionFrom(Transform viewer, float near, float far)
        {
            return GetProjectionFrom(viewer.position, near, far, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Get the projection matrix for this display from a specific viewer, with a specific near and far plane distance.
        /// </summary>
        /// <param name="eyePos">The position of the viewer.</param>
        /// <param name="near">Distance to the near plane.</param>
        /// <param name="far">Distance to the far plane.</param>
        /// <returns>Returns the projection matrix of this display.</returns>
        public Matrix4x4 GetProjectionFrom(Vector3 eyePos, float near, float far)
        {
            return GetProjectionFrom(eyePos, near, far, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Get the projection matrix for this display from a specific viewer, with a specific near and far plane distance.
        /// </summary>
        /// <param name="viewer">The position of the viewer.</param>
        /// <param name="near">Distance to the near plane.</param>
        /// <param name="far">Distance to the far plane.</param>
        /// <param name="offset">Transform offset of the display.</param>
        /// <param name="facing">Transform orientation of the display.</param>
        /// <returns>Returns the projection matrix of this display.</returns>
        public Matrix4x4 GetProjectionFrom(Transform viewer, float near, float far, Vector3 offset, Quaternion facing)
        {
            return GetProjectionFrom(viewer.position, near, far, offset, facing);
        }

        /// <summary>
        /// Get the projection matrix for this display from a specific viewer, with a specific near and far plane distance.
        /// </summary>
        /// <param name="eyePos">The position of the viewer.</param>
        /// <param name="near">Distance to the near plane.</param>
        /// <param name="far">Distance to the far plane.</param>
        /// <param name="offset">Transform offset of the display.</param>
        /// <param name="facing">Transform orientation of the display.</param>
        /// <returns>Returns the projection matrix of this display.</returns>
        public Matrix4x4 GetProjectionFrom(Vector3 eyePos, float near, float far, Vector3 offset, Quaternion facing)
        {
            // transform the screen layout
            var LR = facing * lr + offset;
            var LL = facing * ll + offset;
            var UL = facing * ul + offset;

            // calculate physical screen orientation
            var vr = (LR - LL).normalized;
            var vu = (UL - LL).normalized;
            var vn = Vector3.Cross(vu, vr).normalized;

            // compute vectors to screen corners
            var va = LL - eyePos;
            var vb = LR - eyePos;
            var vc = UL - eyePos;

            // distance of eye to projection plane
            float dist = -Vector3.Dot(va, vn);

            // extent of perpendicular projection
            float left = Vector3.Dot(vr, va) * near / dist;
            float right = Vector3.Dot(vr, vb) * near / dist;
            float bottom = Vector3.Dot(vu, va) * near / dist;
            float top = Vector3.Dot(vu, vc) * near / dist;

            // build frustum transform
            return Matrix4x4.Frustum(left, right, bottom, top, near, far);
        }
        #endregion
    }
}