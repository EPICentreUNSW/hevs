using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor.Build.Content;

namespace HEVS
{
    [InitializeOnLoad]
    class BuildPipeline : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {        
        public int callbackOrder { get { return 0; } }

        static BuildPipeline()
        {
            LogInputAxes();
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        [InitializeOnLoadMethod]
        public static void Initiailise()
        {
            LogInputAxes();
        }

        // use this callback when the editor state changes, i.e. when the user presses Play/Pause/Stop
        private static void OnPlayModeStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || 
                state == PlayModeStateChange.ExitingEditMode)
                LogInputAxes();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            //EditorPreferencesChecker.CheckPreferences();
            LogInputAxes();

            if (report.summary.platform == BuildTarget.StandaloneWindows)
                EditorUtility.DisplayDialog("Warning - x86 Build", "You are building an x86 build which is not supported by HEVS. Your application may not work correctly.", "OK");
        }

        public void OnPostprocessBuild(BuildReport report)
        {

        }

        #region InputLogging
        public enum InputType
        {
            KeyOrMouseButton,
            MouseMovement,
            JoystickAxis,
        };

        [MenuItem("HEVS/Generate Missing Input Mapping")]
        static void LogInputAxes()
        {
            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

            SerializedObject obj = new SerializedObject(inputManager);
            SerializedProperty axisArray = obj.FindProperty("m_Axes");

            if (axisArray.arraySize == 0)
                return;

            HashSet<string> buttons = new HashSet<string>();
            HashSet<string> axes = new HashSet<string>();

            for (int i = 0; i < axisArray.arraySize; ++i)
            {
                var axis = axisArray.GetArrayElementAtIndex(i);

                var name = axis.FindPropertyRelative("m_Name").stringValue;
                var axisVal = axis.FindPropertyRelative("axis").intValue;
                var inputType = (InputType)axis.FindPropertyRelative("type").intValue;

                if (inputType == InputType.KeyOrMouseButton)
                    buttons.Add(name);
                
                axes.Add(name);
            }

            // write list to StreamingAssets folder
            StringBuilder output = new StringBuilder("{ \"buttons\": [ ");
            int index = 0;
            foreach (string button in buttons)
            {
                output.Append("\"" + button + "\"");
                if (++index != buttons.Count)
                    output.Append(", ");
            }
            output.Append(" ], \"axes\": [ ");
            index = 0;
            foreach (string axis in axes)
            {
                output.Append("\"" + axis + "\"");
                if (++index != axes.Count)
                    output.Append(", ");
            }
            output.Append(" ] }");

            // write the input mappings to the resources folder so that it gets packed into the build
            Utils.CreateFolder(UnityEngine.Application.dataPath + "/Resources");
            StreamWriter fs = new StreamWriter(UnityEngine.Application.dataPath + "/Resources/inputmapping.txt");
            fs.Write(output);
            fs.Close();
        }
        #endregion
    }
}