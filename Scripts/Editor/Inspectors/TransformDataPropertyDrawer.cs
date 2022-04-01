using UnityEngine;
using UnityEditor;

namespace HEVS
{
    [CustomPropertyDrawer(typeof(Config.Transform))]
    public class TransformDataPropertyDrawer : PropertyDrawer
    {
        bool foldout = false;

        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            if (foldout = EditorGUI.Foldout(position, foldout, label))
            {
                EditorGUI.indentLevel++;
                property.FindPropertyRelative("translate").vector3Value = EditorGUILayout.Vector3Field("Translate", property.FindPropertyRelative("translate").vector3Value);

                Quaternion q = property.FindPropertyRelative("rotate").quaternionValue;
                q.eulerAngles = EditorGUILayout.Vector3Field("Rotate", q.eulerAngles);
                property.FindPropertyRelative("rotate").quaternionValue = q;

                EditorGUI.indentLevel--;
            }
        }
    }
}