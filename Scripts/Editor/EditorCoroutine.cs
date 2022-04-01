using System.Collections;
using UnityEditor;

namespace HEVS
{
    public class EditorCoroutine
    {
        public static EditorCoroutine Start(IEnumerator _routine)
        {
            EditorCoroutine coroutine = new EditorCoroutine(_routine);
            coroutine.Start();
            return coroutine;
        }

        readonly IEnumerator routine;

        EditorCoroutine(IEnumerator _routine)
        {
            routine = _routine;
        }

        void Start()
        {
            EditorApplication.update += Update;
        }

        public void Stop()
        {
            EditorApplication.update -= Update;
        }

        void Update()
        {
            /* NOTE: no need to try/catch MoveNext,
			 * if an IEnumerator throws, its next iteration returns false.
			 * Also, Unity probably catches when calling EditorApplication.Update.
			 */
            if (!routine.MoveNext())
            {
                Stop();
            }
        }
    }
}