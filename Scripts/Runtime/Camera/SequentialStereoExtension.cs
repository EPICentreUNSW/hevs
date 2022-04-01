using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// A component that controls toggling between two cameras for sequential stereoscopic 3D on certain hardware.
    /// </summary>
    [AddComponentMenu("")]
    public class SequentialStereoExtension : MonoBehaviour
    {
        /// <summary>
        /// The camera used for the left eye capture.
        /// </summary>
        public UnityEngine.Camera left;

        /// <summary>
        /// The camera used for the right eye capture.
        /// </summary>
        public UnityEngine.Camera right;

        bool leftOn = true;

        /// <summary>
        /// Manually swap which eye is active.
        /// </summary>
        public void SwapEyes()
        {
            leftOn = !leftOn;
            left.enabled = leftOn;
            right.enabled = !leftOn;
        }

        void Start()
        {
            QualitySettings.vSyncCount = 1;
            UnityEngine.Application.targetFrameRate = 120;
        }

        void Update()
        {
            SwapEyes();
        }
    }
}