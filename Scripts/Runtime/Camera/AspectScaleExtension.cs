using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Camera component that applies a scale to the camera's projection matrix aspect ratio. This is useful for dealing with stereoscopic or CAVE displays where the display's aspect ratio isn't suitable due to viewport sizes.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(Camera))]
    public class AspectScaleExtension : MonoBehaviour
    {
        /// <summary>
        /// The scale to apply to the projection transform.
        /// </summary>
        public float scale = 1;

        new UnityEngine.Camera camera;

        void Start()
        {
            camera = GetComponent<UnityEngine.Camera>();
        }

        void OnPreRender()
        {
            camera.projectionMatrix = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect * scale, camera.nearClipPlane, camera.farClipPlane);
        }
    }
}