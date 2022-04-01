using UnityEngine;
using UnityEditor;

namespace HEVS
{
    [CustomEditor(typeof(ClusterObject))]
    public class ClusterObjectInspector : Editor
    {
        SerializedProperty id;
        SerializedProperty flags;
		SerializedProperty updateStateOnClients; 

		void OnEnable()
        {
            id = serializedObject.FindProperty("clusterID");
            flags = serializedObject.FindProperty("transformFlags");
			updateStateOnClients = serializedObject.FindProperty("updateStateOnClients");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Cluster ID", id.intValue.ToString());

            flags.intValue = (int)(TransformFlags)EditorGUILayout.EnumFlagsField(new GUIContent("Transform Sync", "Which transform properties to sync."), (TransformFlags)flags.intValue);

			updateStateOnClients.boolValue = EditorGUILayout.Toggle("Update State on Clients", updateStateOnClients.boolValue); 

			serializedObject.ApplyModifiedProperties();
        }
    }
}