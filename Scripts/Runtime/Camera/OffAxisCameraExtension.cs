using UnityEngine;
using UnityEngine.Rendering;

namespace HEVS
{
    /// <summary>
    /// The component responsible for overriding a Camera's projection transform to enable
    /// off-axis rendering (i.e. for CAVE-like immersive rendering).
    /// </summary>
    [AddComponentMenu("")]
    public class OffAxisCameraExtension : CameraBehaviour
    {
        internal StereoTargetEyeMask eye = StereoTargetEyeMask.None;

        void LateUpdate()
        {
            UnityEngine.Camera camera = GetComponent<UnityEngine.Camera>();

            OffAxisDisplay offAxisDisplay = display as OffAxisDisplay;

            Vector3 offset = SceneOrigin.position;
            Quaternion orientation = SceneOrigin.rotation;

            if (display.transform != null)
            {
                if (display.transform.HasTranslation)
                    offset += SceneOrigin.rotation * display.transform.Translation;
                if (display.transform.HasRotation)
                    orientation *= display.transform.Rotation;
            }

            // adjust view
            transform.rotation = orientation * offAxisDisplay.orientation;

            // if screen alignment, rotate parent to face screen
            if (display.stereoAlignment == StereoAlignment.Screen)
            {
                // need to offset the eye correctly
                if (eye == StereoTargetEyeMask.Left)
                    transform.localPosition = offAxisDisplay.orientation * Vector3.right * display.eyeSeparation * -.5f;
                else if (eye == StereoTargetEyeMask.Right)
                    transform.localPosition = offAxisDisplay.orientation * Vector3.right * display.eyeSeparation * .5f;
            }

            // adjust projection
            camera.projectionMatrix = offAxisDisplay.GetProjectionFrom(transform.position, 
                                                                    camera.nearClipPlane, camera.farClipPlane, 
                                                                    offset, orientation);

            // adjust eye convergence
            camera.stereoConvergence = Vector3.Distance(transform.position, orientation * offAxisDisplay.center + offset);
        }
    }
}
 