using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace HEVS
{
  /*  [CustomPropertyDrawer(typeof(Core.OSCReceivers))]
    public class ReceiverDrawer : PropertyDrawer
    {
        //ReorderableList chapterList;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                var address = property.FindPropertyRelative("address");
                var receivers = property.FindPropertyRelative("receivers");

                Rect titleRect = new Rect(position.x + 20, position.y, position.width - 20, EditorGUIUtility.singleLineHeight);

                property.isExpanded = EditorGUI.Foldout(titleRect, property.isExpanded, new GUIContent("/" + address.stringValue));

                if (property.isExpanded)
                {
                    var contentRect = new Rect(titleRect.x + 20, titleRect.y + EditorGUIUtility.singleLineHeight, titleRect.width - 20, EditorGUIUtility.singleLineHeight);
                    address.stringValue = EditorGUI.TextField(contentRect, "Address", address.stringValue);
                    contentRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(contentRect, receivers, new GUIContent("Handlers"), true);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                // address
                height += EditorGUIUtility.singleLineHeight;

                // receivers
                height += EditorGUIUtility.singleLineHeight;

                var receivers = property.FindPropertyRelative("receivers");
                if (receivers.isExpanded)
                {
                    // receiver count
                    height += EditorGUIUtility.singleLineHeight;

                    height += receivers.arraySize * EditorGUIUtility.singleLineHeight + 10;
                }
            }

            return height;
        }
  
        //    private ReorderableList BuildChaptersReorderableList(SerializedProperty property)
        //    {
        //        ReorderableList list = new ReorderableList(property.serializedObject, property, true, true, true, true);
        //
        //        list.drawHeaderCallback = (Rect rect) =>
        //        {
        //            EditorGUI.LabelField(rect, "Chapters");
        //        };
        //        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
        //            EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), true);
        //        };
        //        return list;
        //    }
    }*/

    [CustomEditor(typeof(Core))]
    public class CoreInspector : Editor
    {
        SerializedProperty configProperty;

        SerializedProperty versionNumber;

        SerializedProperty debugImpersonateNodeProperty;

        SerializedProperty randomSeedProperty;

        List<string> platformList;
        int selectedPlatformIndex;
        SerializedProperty selectedPlatform;

        List<string> nodeList;
        int selectedNodeIndex;
        SerializedProperty selectedNode;

        SerializedProperty clusterInEditor;

        SerializedProperty quitOnEscape;

        SerializedProperty debugDrawCurrentOnly;

        SerializedProperty spawnablePrefabList;

        SerializedProperty enableDataBroadcast;

        SerializedProperty includeConsole;

        bool oscFoldout = false;
        SerializedProperty oscPort;
        SerializedProperty includeOSC;
    //    SerializedProperty oscReceivers;

        GUIStyle style = new GUIStyle();
        GUIStyle boldStyle = new GUIStyle();

        Config config = new Config();

        void UpdateSelectedPlatformAndNode()
        {
            platformList = config.platforms.Keys.ToList();

            selectedPlatformIndex = platformList.IndexOf(selectedPlatform.stringValue);
            if (selectedPlatformIndex < 0)
            {
                selectedPlatformIndex = 0;
                selectedPlatform.stringValue = "Default";
            }

            nodeList = config.platforms[selectedPlatform.stringValue].nodes.Keys.ToList();

            selectedNodeIndex = nodeList.IndexOf(selectedNode.stringValue);
            if (selectedNodeIndex < 0)
            {
                selectedNodeIndex = 0;
                selectedNode.stringValue = nodeList[selectedNodeIndex];
            }
        }

        void OnEnable()
        {
            // Setup the SerializedProperties.
            configProperty = serializedObject.FindProperty("configFile");

            selectedPlatform = serializedObject.FindProperty("selectedPlatform");
            selectedNode = serializedObject.FindProperty("selectedNode");
            enableDataBroadcast = serializedObject.FindProperty("enableDataBroadcast");

            includeConsole = serializedObject.FindProperty("includeConsole");

            config.ParseConfig(configProperty.stringValue);

            UpdateSelectedPlatformAndNode();

            debugImpersonateNodeProperty = serializedObject.FindProperty("debugImpersonateNode");
            randomSeedProperty = serializedObject.FindProperty("randomSeed");

            debugDrawCurrentOnly = serializedObject.FindProperty("debugDrawCurrentOnly");

            spawnablePrefabList = serializedObject.FindProperty("spawnablePrefabList");
            quitOnEscape = serializedObject.FindProperty("quitOnEscape");

            clusterInEditor = serializedObject.FindProperty("clusterInEditor");

            versionNumber = serializedObject.FindProperty("VERSION");

            style.alignment = TextAnchor.UpperCenter;
            style.fontStyle = FontStyle.Italic;
            style.wordWrap = true;
            style.normal.textColor = Color.grey;

            boldStyle.alignment = TextAnchor.MiddleCenter;
            boldStyle.fontStyle = FontStyle.Bold;
            boldStyle.wordWrap = true;
            boldStyle.normal.textColor = Color.white;

        //    oscReceivers = serializedObject.FindProperty("oscReceivers");
            oscPort = serializedObject.FindProperty("_oscPort");
            includeOSC = serializedObject.FindProperty("includeOSC");
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(UnityEngine.Application.isPlaying);

            Color defaultColour = GUI.backgroundColor;

            EditorGUILayout.LabelField("HEVS Master Control options.\nThe config file can be overwritten at run-time using the \"config=<filepath>\" or \"-config <filepath>\" command line options, or by setting a HEVS_CONFIG environment variable to the config file path.", 
                style);

            EditorGUILayout.Space();
            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Select Configuration File"))
            {
                string filename = EditorUtility.OpenFilePanel("Config File", UnityEngine.Application.dataPath, "json");

                if (filename.StartsWith(UnityEngine.Application.streamingAssetsPath))
                    filename = filename.Replace(UnityEngine.Application.streamingAssetsPath, "StreamingAssets");

                if (!string.IsNullOrEmpty(filename))
                {
                    config = new Config();
                    config.ParseConfig(filename);

                    configProperty.stringValue = filename;

                    UpdateSelectedPlatformAndNode();
                }
            }

            GUI.backgroundColor = defaultColour;

            EditorGUILayout.LabelField("Config File", configProperty.stringValue);

            EditorGUILayout.Space();

            if (config != null &&
                config.platforms.Count > 0)
            {
                // system drop-down
                selectedPlatformIndex = EditorGUILayout.Popup(new GUIContent("Target Platform",
                    "Specify target platform that HEVS will run on. Can be overritten via command line \"platform=<platformname>\" or via the environment variable HEVS_PLATFORM."), 
                    selectedPlatformIndex, platformList.ToArray());

                if (selectedPlatform.stringValue != platformList[selectedPlatformIndex])
                {
                    selectedPlatform.stringValue = platformList[selectedPlatformIndex];

                    // repopulate node list
                    nodeList = config.platforms[selectedPlatform.stringValue].nodes.Keys.ToList();

                    selectedNodeIndex = nodeList.IndexOf(selectedNode.stringValue);
                    if (selectedNodeIndex < 0)
                    {
                        selectedNodeIndex = 0;
                        selectedNode.stringValue = nodeList[selectedNodeIndex];
                    }
                }

                if (nodeList.Count > 0 &&
                    selectedPlatform.stringValue != "Default")
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(clusterInEditor, new GUIContent("Allow Cluster in Editor", "If selected platform uses a cluster, should we allow the editor to connect to the cluster?"));

                    // impersonate node dropdown
                    debugImpersonateNodeProperty.boolValue = EditorGUILayout.Toggle(new GUIContent("Impersonate Node?",
                        "Impersonate a node within th specified platform while running within the editor."), debugImpersonateNodeProperty.boolValue);

                    if (debugImpersonateNodeProperty.boolValue)
                    {
                        EditorGUI.indentLevel++;

                        selectedNodeIndex = EditorGUILayout.Popup(new GUIContent("Node",
                            "Impersonate a node within the specified platform while running within the editor."),
                            selectedNodeIndex,
                            nodeList.ToArray());

                        selectedNode.stringValue = nodeList[selectedNodeIndex];

                        EditorGUILayout.PropertyField(debugDrawCurrentOnly, new GUIContent("Node's Gizmo Only", "Display only the current impersonated node's display gizmos."));

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.Space();
            }


            randomSeedProperty.intValue = EditorGUILayout.IntField(new GUIContent("Random Seed",
                    "The seed value that will be used for Random when both clustered and non-clustered. Only applies to first active HEVSApplication object!"), randomSeedProperty.intValue);

            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(spawnablePrefabList, new GUIContent("Spawnable Prefab List", "A list of prefabs that can be instantiated within a cluster"), true);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(includeOSC, new GUIContent("Include OSC", "Specifies if the OSC port should be opened for receiving OSC packets"));

            if (includeOSC.boolValue)//oscFoldout = EditorGUILayout.Foldout(oscFoldout, new GUIContent("OSC")))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(oscPort, new GUIContent("Default OSC Port", "The default port that HEVS will use to receive OSC messages. Can be overridden via the config file, or via command-line argument '-osc_port ####'."));
           //     EditorGUILayout.PropertyField(oscReceivers, new GUIContent("Handlers", "A list of GameObjects that can receive OSC messages sent to specific addresses"), true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(includeConsole, new GUIContent("Include Console", "Should the application include a console terminal?"));

            EditorGUILayout.PropertyField(enableDataBroadcast, new GUIContent("Enable Data Broadasting", "Enable the ability o broadcast arbitrary data within a cluster. The broadcast port can be set within the config file."));

            EditorGUILayout.PropertyField(quitOnEscape, new GUIContent("Quit On Escape Key", "Should the application close if the escape key is pressed?"));

            EditorGUI.EndDisabledGroup();

         /*   System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;*/
        //    EditorGUILayout.LabelField("HEVS Version", Core.VERSION);

            serializedObject.ApplyModifiedProperties();
        }
    }
}