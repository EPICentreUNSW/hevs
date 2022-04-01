using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace HEVS
{
    public class EditorPreferencesChecker : EditorWindow
    {
        static EditorPreferencesChecker singleton;

        [InitializeOnLoadMethod]
        public static void Initiailise()
        {
       /*     if (!EditorPrefs.HasKey("HEVS_Suppress_Pref_Check"))
                EditorPrefs.SetBool("HEVS_Suppress_Pref_Check", false);

            if (!EditorPrefs.GetBool("HEVS_Suppress_Pref_Check") && 
                PreferencesNotSet() &&
                singleton == null)
                singleton = GetWindow<EditorPreferencesChecker>("Check Editor Preferences");*/
        }

        public static bool PreferencesNotSet()
        {
            bool allGood = true;

            // has GL been enabled?
            UnityEngine.Rendering.GraphicsDeviceType[] apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            if (apis.Length == 0 ||
                !apis.Contains(UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore))
            {
                allGood = false;
            }

            // enable XR if using hardware stereo
        /*    if (!PlayerSettings.virtualRealitySupported)
            {
                allGood = false;
            }*/

            // set Stereoscopic (no HMD)
            //   if (PlayerSettings.virtualRealitySupported)
         /*   {
                string[] sdks = PlayerSettings.GetVirtualRealitySDKs(BuildTargetGroup.Standalone);
                // make stereo first
                if (sdks.Length > 0 && sdks[0] != "stereo")
                {
                    allGood = false;
                }
            }*/

            return !allGood;
        }

    //    [MenuItem("HEVS/Check Editor Preferences", priority = 0)]
        public static void MenuCheckEditorPrefences()
        {
            if (singleton == null)
                singleton = GetWindow<EditorPreferencesChecker>("Check Editor Preferences");
        }

        // inspector panel layout
        void OnGUI()
        {
            GUILayout.Label("Editor Preferences", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying)
            {
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
                style.wordWrap = true;
                GUILayout.Label("Please wait, Editor is either compiling, refreshing assets, or is currently playing.", style);
            }
            else
            {
                bool allGood = true;

                // has GL been enabled?
                UnityEngine.Rendering.GraphicsDeviceType[] apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
                if (apis.Length == 0 ||
                    !apis.Contains(UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore))
                {
                    allGood = false;

                    // not GL, change to GL?
                    if (EditorGUILayout.Toggle("Include OpenGLCore API", false))
                    {
                        List<UnityEngine.Rendering.GraphicsDeviceType> list = new List<UnityEngine.Rendering.GraphicsDeviceType>(apis);
                        list.Add(UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore);

                        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, list.ToArray());
                    }
                    EditorGUILayout.Space();
                }

                // enable XR if using hardware stereo
             /*   if (!PlayerSettings.virtualRealitySupported)
                {
                    allGood = false;

                    if (EditorGUILayout.Toggle("Enable XR for QuadBuffers and VR", false))
                    {
                        PlayerSettings.virtualRealitySupported = true;
                    }
                    EditorGUILayout.Space();
                }

                // set Stereoscopic (no HMD)
                if (PlayerSettings.virtualRealitySupported)
                {
                    string[] sdks = PlayerSettings.GetVirtualRealitySDKs(BuildTargetGroup.Standalone);
                    if (sdks.Length == 0 || !sdks.Contains("stereo"))
                    {
                        allGood = false;
                        if (EditorGUILayout.Toggle("Enable Hardware Stereoscopic", false))
                        {
                            List<string> list = new List<string>(sdks);
                            list.Add("stereo");

                            PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Standalone, list.ToArray());
                        }
                        EditorGUILayout.Space();
                    }
                }*/

                if (allGood)
                {
                    GUIStyle style = new GUIStyle();

                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Italic;
                    style.wordWrap = true;

                    GUILayout.Label("All preferences have been set.", style);
                }

                EditorGUILayout.Space();

                //if (GUILayout.Button("Create Joystick Axes and Buttons"))
                //{
                    //InputEditor.AddAxesSet(InputEditor.AxesSet.Joystick);
                //}

                EditorGUILayout.Space();
                EditorPrefs.SetBool("HEVS_Suppress_Pref_Check",
                    !EditorGUILayout.Toggle("Auto-show this dialog",
                    !EditorPrefs.GetBool("HEVS_Suppress_Pref_Check"))
                    );
            }
        }
    }
}