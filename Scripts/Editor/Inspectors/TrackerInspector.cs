using UnityEngine;
using UnityEditor;
using System;

namespace HEVS
{
    [CustomEditor(typeof(Tracker))]
    public class TrackerInspector : Editor
    {
        SerializedProperty id;
        SerializedProperty disableIfNotFound;
		SerializedProperty mouseInEditor;
        SerializedProperty defaultType;
        SerializedProperty masterOnly;

        SerializedProperty flags;
        SerializedProperty node;
        SerializedProperty smooth;
        SerializedProperty smoothMultiplier;
        SerializedProperty transform;
        SerializedProperty address;
        SerializedProperty right;
        SerializedProperty up;
        SerializedProperty forward;
        SerializedProperty port;
        SerializedProperty handedness;

        bool foldout = false;

        void OnEnable()
        {
            id = serializedObject.FindProperty("configId");
            disableIfNotFound = serializedObject.FindProperty("disableIfNotFound");
            mouseInEditor = serializedObject.FindProperty("forceMouseInEditor");
            defaultType = serializedObject.FindProperty("_defaultType");
            masterOnly = serializedObject.FindProperty("masterOnly");

            flags = serializedObject.FindProperty("_transformFlags");
            node = serializedObject.FindProperty("_xrNode");
            smooth = serializedObject.FindProperty("_smoothing");
            smoothMultiplier = serializedObject.FindProperty("_smoothMultiplier");
            transform = serializedObject.FindProperty("_offsetTransform");
            address = serializedObject.FindProperty("_address");
            right = serializedObject.FindProperty("_right");
            up = serializedObject.FindProperty("_up");
            forward = serializedObject.FindProperty("_forward");
            port = serializedObject.FindProperty("_port");
            handedness = serializedObject.FindProperty("_handedness");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(masterOnly, new GUIContent("Master Only", "Should the tracker only update on the master?"));

            EditorGUILayout.PropertyField(id, new GUIContent("Tracker Config ID", "The tracker ID from the HEVS configuration file."));

            if (!string.IsNullOrEmpty(id.stringValue))
                EditorGUILayout.PropertyField(disableIfNotFound, new GUIContent("Disable if not found", "Specify if the GameObject should be disabled if the Tracker ID is not found within the current platform's config."));

            bool isMouse = false;
            if (!disableIfNotFound.boolValue)
            {
                EditorGUILayout.PropertyField(defaultType, new GUIContent("Default Tracker Type", "The default tracker type for this component if not using a config definition."));

                var e = (Tracker.DefaultType)Enum.GetValues(typeof(Tracker.DefaultType)).GetValue(defaultType.enumValueIndex);

                EditorGUI.indentLevel++;
                switch (e)
                {
                    case Tracker.DefaultType.Mouse:
                        {
                            isMouse = true;
                            break;
                        }
                    case Tracker.DefaultType.VRPN:
                        {
                            EditorGUILayout.PropertyField(address, new GUIContent("Address", "The tracker VRPN address."));
                            TrackerAxisGUI();
                            TransformOffsetGUI();
                            TransformFlagsGUI();
                            SmoothingGUI();
                            break;
                        }
                    case Tracker.DefaultType.OSC:
                        {
                            EditorGUILayout.PropertyField(port, new GUIContent("Port", "The port to listen for OSC transform information."));

                            TrackerAxisGUI();
                            TransformOffsetGUI();
                            TransformFlagsGUI();
                            SmoothingGUI();
                            break;
                        }
                    case Tracker.DefaultType.XR:
                        {
                            EditorGUILayout.PropertyField(node, new GUIContent("Tracked Node", "The XR node that will be tracked, if available."));

                            TransformOffsetGUI();
                            TransformFlagsGUI();
                            SmoothingGUI();
                            break;
                        }
                }
                EditorGUI.indentLevel--;
            }

            if (!isMouse ||
                !string.IsNullOrEmpty(id.stringValue))
                EditorGUILayout.PropertyField(mouseInEditor, new GUIContent("Force Mouse Tracker in Editor", "If running in Editor Mode, should we create a mouse tracker instead of any other?"));

            serializedObject.ApplyModifiedProperties();
        }

        void TrackerAxisGUI()
        {
            if (foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Tracker Axis Mapping", "Specify how the axis from the tracker are mapped to Unity's Left-Handed Z-Forward Y-Up X-Right coordinate space.")))
            {
                EditorGUILayout.PropertyField(right, new GUIContent("Right Axis"));
                EditorGUILayout.PropertyField(up, new GUIContent("Up Axis"));
                EditorGUILayout.PropertyField(forward, new GUIContent("Forward Axis"));
                EditorGUILayout.PropertyField(handedness, new GUIContent("Handedness"));
            }
        }

        void TransformFlagsGUI()
        {
            flags.intValue = (int)(TransformFlags)EditorGUILayout.EnumFlagsField(new GUIContent("Track Transform", "Which transform properties to update each frame."), (TransformFlags)flags.intValue);
        }

        void TransformOffsetGUI()
        {
            EditorGUILayout.PropertyField(transform, new GUIContent("Transform Offset", "Specify a transform to apply on top of the tracker once it has been converted to Unity's coordinate space."));
        }

        void SmoothingGUI()
        {
            EditorGUILayout.PropertyField(smooth, new GUIContent("Apply Smoothing", "Should the tracker apply smoothing to the received data."));

          /*  if (smooth.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(smoothMultiplier, new GUIContent("Smoothing Multiplier", "Smothing multiplier; lower multipliers reduce the tracking speed. Default is 1."));
                EditorGUI.indentLevel--;
            }*/
        }
    }
}