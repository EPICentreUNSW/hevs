using UnityEngine;

namespace HEVS.Extensions
{
    /// <summary>
    /// A utility class for extension methods.
    /// </summary>
    public static class CameraExtensions
    {
        /// <summary>
        /// Activates a target display, if available, and assigns the camera to it.
        /// </summary>
        /// <param name="camera">The camera to assign to a display adapter.</param>
        /// <param name="display">The display adapter to activate and assign to the camera.</param>
        /// <returns>Returns true if display adapter was activated and set, false otherwise.</returns>
        public static bool SetAndActivateTargetDisplay(this UnityEngine.Camera camera, int display)
        {
            if (!UnityEngine.Application.isEditor)
            {
                if (display > 0 &&
                    display < UnityEngine.Display.displays.Length)
                {
                    if (!UnityEngine.Display.displays[display].active)
                        UnityEngine.Display.displays[display].Activate();
                }
                else if (display != 0)
                {
                    Debug.LogWarning("HEVS: Invalid target display adapter [" + display + "]");
                    return false;
                }
            }

            camera.targetDisplay = display;

            return true;
        }
    }
}
