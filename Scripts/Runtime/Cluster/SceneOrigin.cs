using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Component that signals that the owning GameObject's transform represents the scene origin 
    /// for HEVS, which all trackers and displays are relative to.
    /// Also used as a static class to access the scene origin position and orientation.
    /// </summary>
    [AddComponentMenu("HEVS/Scene Origin")]
    [RequireComponent(typeof(ClusterObject))]
    public sealed class SceneOrigin : MonoBehaviour
    {
        static SceneOrigin instance;

        static Vector3 defaultPosition = Vector3.zero;
        static Quaternion defaultRotation = Quaternion.identity;

        /// <summary>
        /// Access to the Scene Object's GameObject, if valid.
        /// </summary>
        public new static GameObject gameObject
        {
            get
            {
                if (instance)
                    return instance.transform.gameObject;
                return null;
            }
        }

        /// <summary>
        /// Access to the scene origin's position.
        /// If the origin has been set by a SceneOrigin component then this accesses the GameObject's Transform.
        /// If no component was set then this represents a global Vector3 that can be modified.
        /// </summary>
        public static Vector3 position
        {
            set
            {
                if (instance)
                    instance.transform.position = value;
                else if (UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying)
                {
                    var so = FindObjectOfType<SceneOrigin>();
                    if (so)
                        so.transform.position = value;
                }
                else
                    defaultPosition = value;
            }
            get
            {
                if (instance)
                    return instance.transform.position;
                else if (UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying)
                {
                    var so = FindObjectOfType<SceneOrigin>();
                    if (so)
                        return so.transform.position;
                }

                return defaultPosition;
            }
        }

        /// <summary>
        /// Access to the scene origin's orientation.
        /// If the origin has been set by a SceneOrigin component then this accesses the GameObject's Transform.
        /// If no component was set then this represents a global Quaternion that can be modified.
        /// </summary>
        public static Quaternion rotation
        {
            set
            {
                if (instance)
                    instance.transform.rotation = value;
                else if (UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying)
                {
                    var so = FindObjectOfType<SceneOrigin>();
                    if (so)
                        so.transform.rotation = value;
                }
                else
                    defaultRotation = value;
            }
            get
            {
                if (instance)
                    return instance.transform.rotation;
                else if (UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying)
                {
                    var so = FindObjectOfType<SceneOrigin>();
                    if (so)
                        return so.transform.rotation;
                }
                return defaultRotation;
            }
        }

        void Awake()
        {
            if (instance != null)
                throw new UnityException("Multiple SceneOrigin's detected! There can only be one active within the scene!");
            else
                instance = this;
        }

        void OnDestroy()
        {
            instance = null;
        }
    }
}