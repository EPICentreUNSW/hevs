using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HEVS.Extensions
{
    /// <summary>
    /// A utility class for extension methods.
    /// </summary>
    public static class GameObjectExtensions
    {
        #region Game Object Extensions
        /// <summary>
        /// Copy MonoBehavour components of a certain type from a source GameObject to a target GameObject.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour type to copy.</typeparam>
        /// <param name="source">The source GameObject to copy from.</param>
        /// <param name="target">The target gameObject to copy to.</param>
        public static void CopyComponentsOfTypeTo<T>(this GameObject source, GameObject target) where T : Component
        {
            // copy over any camera extensions
            T[] extensions = source.GetComponents<T>();

            // copy each over?
            foreach (T ext in extensions)
            {
                FieldInfo[] sourceFields = ext.GetType().GetFields(BindingFlags.Public |
                                                         BindingFlags.NonPublic |
                                                         BindingFlags.Instance);

                // create a new component for each new camera object
                var newExt = target.AddComponent(ext.GetType());

                int i = 0;
                for (i = 0; i < sourceFields.Length; i++)
                {
                    var value = sourceFields[i].GetValue(ext);
                    sourceFields[i].SetValue(newExt, value);
                }

                (newExt as MonoBehaviour).enabled = (ext as MonoBehaviour).enabled;
            }
        }
        /// <summary>
        /// Copy MonoBehavour components of a certain type to a target GameObject from a source GameObject.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour type to copy.</typeparam>
        /// <param name="target">The target GameObject to copy to.</param>
        /// <param name="source">The source gameObject to copy from.</param>
        public static void CopyComponentsOfTypeFrom<T>(this GameObject target, GameObject source) where T : Component
        {
            // copy over any camera extensions
            T[] extensions = source.GetComponents<T>();

            // copy each over?
            foreach (T ext in extensions)
            {
                FieldInfo[] sourceFields = ext.GetType().GetFields(BindingFlags.Public |
                                                         BindingFlags.NonPublic |
                                                         BindingFlags.Instance);

                // create a new component for each new camera object
                var newExt = target.AddComponent(ext.GetType());

                int i = 0;
                for (i = 0; i < sourceFields.Length; i++)
                {
                    var value = sourceFields[i].GetValue(ext);
                    sourceFields[i].SetValue(newExt, value);
                }

                (newExt as MonoBehaviour).enabled = (ext as MonoBehaviour).enabled;
            }
        }

        /// <summary>
        /// Attempts to get a component from a GameObject. If the component doesn't exist then one is added.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour to get/add.</typeparam>
        /// <param name="go">The GameObject to get/add from/to.</param>
        /// <returns>Returns the MonoBehaviour.</returns>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T result = go.GetComponent<T>();
            if (result == null)
            {
                result = go.AddComponent<T>();
            }
            return result;
        }
        
        /// <summary>
        /// Searches for the first instance of a MonoBehaviour in a GameObject's children, including searching through deactivated children.
        /// </summary>
        /// <typeparam name="T">The MonoBejaviour type to search for.</typeparam>
        /// <param name="go">The parent GameObject to search.</param>
        /// <returns>Returns the first instance of the specified MonoBehavour, or null if not found.</returns>
        public static T GetComponentFromChildrenInactive<T>(this GameObject go) where T : MonoBehaviour
        {
            foreach (Transform t2 in go.transform)
            {
                T result = t2.GetComponent<T>();
                if (result != null)
                {
                    return result;
                }

            }
            return null;
        }

        /// <summary>
        /// Gathers all instances of a component from a list of Transforms.
        /// </summary>
        /// <typeparam name="T">The component type to search for.</typeparam>
        /// <param name="items">The Transforms to search.</param>
        /// <returns>Returns the List of the specified components if found.</returns>
        public static List<T> GetComponentFromList<T>(this List<Transform> items) where T : Component
        {
            List<T> result = new List<T>();

            foreach (Transform trans in items)
            {
                T component = trans.gameObject.GetComponent<T>();
                if (component != null)
                {
                    result.Add(component);
                }
            }
            return result;
        }

        /// <summary>
        /// Gathers all instances of a component from the children of a list of Transforms.
        /// </summary>
        /// <typeparam name="T">The component type to search for.</typeparam>
        /// <param name="items">The parents that will have their children searched.</param>
        /// <returns>Returns the List of the specified components if found.</returns>
        public static List<T> GetComponentsInChildrenFromList<T>(this List<Transform> items) where T : Component
        {
            List<T> result = new List<T>();

            foreach (Transform trans in items)
            {
                T[] components = trans.gameObject.GetComponentsInChildren<T>();
                foreach (T component in components)
                {
                    result.Add(component);
                }
            }
            return result;
        }

        /// <summary>
        /// Searches for a child GameObject on a specified Transform, including searching through deactivated children.
        /// </summary>
        /// <param name="parent">The parent to search.</param>
        /// <param name="name">The name of the child GameObject to search for.</param>
        /// <returns>Returns the child GameObject's Transform, or null if not found.</returns>
        public static Transform FindChildIncludingDeactivated(this Transform parent, string name)
        {
            return parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(c => c.name == name);
        }

        /// <summary>
        /// Plays a non-looping AudioClip on a specified GameObject. If the GameObject doesn't have an AudioSource then one is added.
        /// </summary>
        /// <param name="gameObject">The GameObject to play the AudioClip on.</param>
        /// <param name="clip">The AudioClip to play.</param>
        public static void PlayOneShot(this GameObject gameObject, AudioClip clip)
        {
            if (clip == null)
                return;
            if (!gameObject.GetComponent<AudioSource>())
            {
                gameObject.AddComponent<AudioSource>();
                gameObject.GetComponent<AudioSource>().playOnAwake = false;
            }
            gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
        }

        /// <summary>
        /// Plays a looping AudioClip on a specified GameObject. If the GameObject doesn't have an AudioSource then one is added.
        /// </summary>
        /// <param name="gameObject">The GameObject to play the AudioClip on.</param>
        /// <param name="clip">The AudioClip to play.</param>
        public static void PlayAudio(this GameObject gameObject, AudioClip clip)
        {
            if (clip == null)
                return;
            if (!gameObject.GetComponent<AudioSource>())
            {
                gameObject.AddComponent<AudioSource>();
                gameObject.GetComponent<AudioSource>().playOnAwake = false;
            }
            gameObject.GetComponent<AudioSource>().clip = clip;
            gameObject.GetComponent<AudioSource>().loop = true;
            gameObject.GetComponent<AudioSource>().Play();
        }
        #endregion
    }
}
