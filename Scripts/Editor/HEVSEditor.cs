using UnityEngine;
using UnityEditor;

namespace HEVS
{
    public class HEVSEditor : EditorWindow
    {
        [InitializeOnLoadMethod]
        public static void Initiailise()
        {
            // add layers
            EditorUtilityExtenion.CreateLayer("HEVSCameras");
            EditorUtilityExtenion.CreateLayer("HEVSLeftEyeOnly");
            EditorUtilityExtenion.CreateLayer("HEVSRightEyeOnly");
            EditorUtilityExtenion.CreateLayer("HEVSFullscreenOverlay");

            // add the required shaders
            EditorCoroutine.Start(EditorUtilityExtenion.AddAlwaysIncludedShader("Unlit/Texture"));
            EditorCoroutine.Start(EditorUtilityExtenion.AddAlwaysIncludedShader("HEVS/CameraOverlay"));
            EditorCoroutine.Start(EditorUtilityExtenion.AddAlwaysIncludedShader("HEVS/ProjectorBlend"));
            EditorCoroutine.Start(EditorUtilityExtenion.AddAlwaysIncludedShader("HEVS/Blacklevel"));

            // ensure HEVSConfiguration runs first every frame
            EnsureConfigFirst();
        }

        static void EnsureConfigFirst()
        {
            MonoScript configScript = null;
            string configName = typeof(Core).Name;

            MonoScript firstScript = null;
            int firstScriptOrder = 0;

            MonoScript[] scripts = MonoImporter.GetAllRuntimeMonoScripts();
            foreach (MonoScript monoScript in scripts)
            {
                if (firstScript == null)
                {
                    firstScript = monoScript;
                    firstScriptOrder = MonoImporter.GetExecutionOrder(monoScript);
                }
                else
                {
                    int o = MonoImporter.GetExecutionOrder(monoScript);
                    if (o < firstScriptOrder)
                    {
                        firstScript = monoScript;
                        firstScriptOrder = o;
                    }
                }

                if (configName == monoScript.name)
                    configScript = monoScript;
            }

            if (configScript != null && firstScript != configScript)
                MonoImporter.SetExecutionOrder(configScript, firstScriptOrder - 100);
        }
        
     //   [MenuItem("HEVS/Show Options",priority = 15)]
        static void ShowEditor()
        {
            // Get existing open window or if none, make a new one:
            GetWindow<HEVSEditor>("HEVS Options");
        }

        // inspector panel layout
        void OnGUI()
        {
            GUILayout.Label("HEVS Options", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUIUtility.labelWidth = 200;

            SceneChecker.ToggleWarnings(
                EditorGUILayout.Toggle(new GUIContent("ClusterObject Parent Warnings",
                "Prints warnings when the hierarchy changes and it finds ClusterObjects with parents that aren't also ClusterObjects or aren't flagged as static."),
                SceneChecker.warningOnHierarchyChange));

            EditorGUILayout.Space();

            // extra stuff that can only run when NOT playing / compiling / paused
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling);

            EditorGUI.EndDisabledGroup();

            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperCenter;
            style.fontStyle = FontStyle.Italic;
            style.wordWrap = true;
            EditorGUI.indentLevel++;
            if (EditorApplication.isCompiling)
                GUILayout.Label("Compiling, please wait...", style);
            if (EditorApplication.isPlaying || EditorApplication.isPaused)
                GUILayout.Label("Currently playing, please wait...", style);
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel--;
        }

        void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}