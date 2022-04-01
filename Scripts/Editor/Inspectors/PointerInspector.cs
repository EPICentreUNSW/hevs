using UnityEngine;
using UnityEditor;

namespace HEVS
{
    [CustomEditor(typeof(Pointer))]
    public class PointerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}