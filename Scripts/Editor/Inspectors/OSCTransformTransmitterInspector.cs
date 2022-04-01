using UnityEngine;
using UnityEditor;

namespace HEVS
{
    [CustomEditor(typeof(OSCTransformTransmitter))]
    public class OSCTransformTransmitterInspector : Editor
    {
        SerializedProperty id;
        SerializedProperty address;
        SerializedProperty port;
        SerializedProperty flags;
        SerializedProperty broadcastOnStart;

        void OnEnable()
        {
            id = serializedObject.FindProperty("id");
            port = serializedObject.FindProperty("port");
            flags = serializedObject.FindProperty("transformFlags");
            address = serializedObject.FindProperty("address");
            broadcastOnStart = serializedObject.FindProperty("broadcastOnStart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(id, new GUIContent("ID", "The ID of the object being tracked."));
            EditorGUILayout.PropertyField(address, new GUIContent("Address", "The address to send OSC transforms to."));
            EditorGUILayout.PropertyField(port, new GUIContent("Port", "The port to send OSC transform information."));
            flags.intValue = (int)(TransformFlags)EditorGUILayout.EnumFlagsField(new GUIContent("Sync", "Which transform properties to sync."), (TransformFlags)flags.intValue);
            EditorGUILayout.PropertyField(broadcastOnStart, new GUIContent("Broadcast On Start", "Should the transmitter begin broadcasting on startup."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}