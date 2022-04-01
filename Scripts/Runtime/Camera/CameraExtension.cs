using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// A MonoBehaviour that can be used as a base class for Camera behaviours that need to 
    /// be copied from the Main Camera onto HEVS scene-capture camera rigs.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(UnityEngine.Camera))]
    abstract public class CameraExtension : MonoBehaviour
    {

    }
}