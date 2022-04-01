using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace HEVS
{
    public class ReplayEditor : EditorWindow
    {
        Vector2 scrollPosition = Vector2.zero;

        bool showRPCState = false;
    //    bool showInputState = false;
    //    bool showVariableState = false;
    //    bool showObjectState = false;

        private UnityEditorInternal.ReorderableList dimensionList;

    //    Replay replay = null;

        List<int> intList = new List<int>();

        static ReplayEditor editor;

        GUIContent saveButtonContent;
        GUIContent saveAsButtonContent;
        GUIContent undoButtonContent;
        GUIContent recordButtonContent; 
        GUIContent playButtonContent;
        GUIContent stopButtonContent;
        GUIContent pauseButtonContent;
        GUIContent beginingButtonContent;
        GUIContent backButtonContent;
        GUIContent forwardButtonContent;
        GUIContent endButtonContent;

   //     [MenuItem("HEVS/Replay Editor")]
        static void ShowEditor()
        {
            if (editor == null)
            {
                editor = GetWindow<ReplayEditor>("HEVS Replay Editor");
                editor.Show();
           //     EditorApplication.playModeStateChanged += OnPlayModeStateChange;
            }
        }

        void OnDestroy()
        {
        //    Replay.active = null;
        //    EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
        }

        // use this callback when the editor state changes, i.e. when the user presses Play/Pause/Stop
    /*    static void OnPlayModeStateChange(PlayModeStateChange state)
        {
            int replayState = EditorPrefs.GetInt("replay_state");

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (replayState == (int)ReplayEditorState.Playing)
                {
                    editor.OnPlay();
                }
                else if (replayState == (int)ReplayEditorState.Recording)
                {
                    editor.OnRecord();
                }
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                editor.OnStop();
            }
        }*/

        void OnEnable()
        {
            saveButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/save"), "Save Replay");
            saveAsButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/saveAs"), "Save Replay As");
            undoButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/return"), "Undo Changes");

            recordButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/buttonR"), "Record");
            playButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/forward"), "Play");
            stopButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/stop"), "Stop");
            pauseButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/pause"), "Pause");
            beginingButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/previous"), "Jump To Start");
            backButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/rewind"), "Previous Frame");
            forwardButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/fastforward"), "Next Frame");
            endButtonContent = new GUIContent(Resources.Load<Texture2D>("UI/next"), "Jump To End");

            autoRepaintOnSceneChange = true;

            if (dimensionList == null)
                dimensionList = new UnityEditorInternal.ReorderableList(intList, typeof(int), false, true, true, true);

            if (EditorPrefs.HasKey("replay_file"))
                Replay.active = new Replay(EditorPrefs.GetString("replay_file"));
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        enum ReplayEditorOnPlayAction
        {
            None = 0,
            AutoPlay = 1,
            AutoRecord = 2
        }

        #region Timeline Buttons
        void OnRecord()
        {
            if (UnityEngine.Application.isPlaying &&
                Replay.active.isReady)
                Replay.active.StartRecording();

            EditorPrefs.SetInt("replay_onplay_state", (int)ReplayEditorOnPlayAction.AutoRecord);
        }

        void OnPlay()
        {
            if (UnityEngine.Application.isPlaying &&
                Replay.active.isReady)
                Replay.active.StartPlayback();

            EditorPrefs.SetInt("replay_onplay_state", (int)ReplayEditorOnPlayAction.AutoPlay);
        }

        void OnStop()
        {
            if (UnityEngine.Application.isPlaying)
            {
                switch (Replay.active.state)
                {
                    case ReplayState.Recording:
                    case ReplayState.RecordingPaused:
                        {
                            Replay.active.StopRecording();
                        }
                        break;
                    case ReplayState.Playing:
                    case ReplayState.PlayingPaused:
                        {
                            Replay.active.StopPlayback();
                        }
                        break;
                    default: break;
                }
            }

            EditorPrefs.SetInt("replay_onplay_state", (int)ReplayEditorOnPlayAction.None);
        }

        void OnPause()
        {
            switch (Replay.active.state)
            {
                case ReplayState.Recording:
                    {
                        Replay.active.PauseRecording(true);
                    }
                    break;
                case ReplayState.RecordingPaused:
                    {
                        Replay.active.PauseRecording(false);
                    }
                    break;
                case ReplayState.Playing:
                    {
                        Replay.active.PausePlayback(true);
                    }
                    break;
                case ReplayState.PlayingPaused:
                    {
                        Replay.active.PausePlayback(false);
                    }
                    break;
                default: break;
            }
        }

        void OnJumpToStart()
        {
            switch (Replay.active.state)
            {
                case ReplayState.Ready:
                    {

                    }
                    break;
                case ReplayState.Playing:
                    {

                    }
                    break;
                case ReplayState.PlayingPaused:
                    {

                    }
                    break;
                default: break;
            }
        }

        void OnJumpToEnd()
        {
            switch (Replay.active.state)
            {
                case ReplayState.Ready:
                    {

                    }
                    break;
                case ReplayState.Playing:
                    {

                    }
                    break;
                case ReplayState.PlayingPaused:
                    {

                    }
                    break;
                default: break;
            }
        }

        void OnNext()
        {
            if (Replay.active.isPaused &&
                Replay.active.isPlaying)
            {

            }
        }

        void OnPrevious()
        {
            if (Replay.active.isPaused &&
                Replay.active.isPlaying)
            {

            }
        }
        #endregion

        public void OnGUI()
        {
            string path = EditorPrefs.GetString("replay_file");
            if (string.IsNullOrWhiteSpace(path))
            {
                if (GUILayout.Button("Select Replay File"))
                {
                    path = EditorUtility.OpenFilePanel("Open Replay File", UnityEngine.Application.streamingAssetsPath, "replay");
                    if (path.Length > 0)
                    {
                        EditorPrefs.SetString("replay_file", path);
                    }
                }
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUIUtility.labelWidth = 100;

            /// FILE INFORMATION / FILE LOADING
            
            GUILayout.Label("File Info", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(saveButtonContent, GUILayout.Width(38), GUILayout.Height(38)))
            {
                if (EditorUtility.DisplayDialog("Save", "Are you sure you want to save changes? The replay file will be overwritten!", "Save", "Cancel"))
                    Replay.active.Flush();
            }
            if (GUILayout.Button(saveAsButtonContent, GUILayout.Width(38), GUILayout.Height(38)))
            {
                path = EditorUtility.SaveFilePanel("Save Replay File", UnityEngine.Application.streamingAssetsPath, "", "replay");
                if (path.Length > 0)
                {
                    EditorPrefs.SetString("replay_file", path);
                    if (Replay.active != null)
                    {
                        Replay.active.SetReplayPath(path);
                        Replay.active.Flush();
                    }
                }
            }
            if (GUILayout.Button(undoButtonContent, GUILayout.Width(38), GUILayout.Height(38)))
            {
                EditorUtility.DisplayDialog("Discard Changes", "Are you sure you want to discard changes? All changes will be lost!", "Discard", "Cancel");
            }

            EditorGUILayout.BeginVertical();

            if (Replay.active != null)
            {
                EditorGUILayout.LabelField("Replay File: ", Replay.active.filePath);
                float time = Replay.active.duration;
                int min = Mathf.FloorToInt(time / 60);
                int sec = Mathf.FloorToInt(time - min * 60);
                int mill = Mathf.FloorToInt((time - (float)Math.Truncate(time)) * 100);
                EditorGUILayout.LabelField("Duration: ", "00:00:00");

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(UnityEngine.Application.isPlaying);
                int state = EditorPrefs.GetInt("replay_onplay_state");
                state = (int)(ReplayEditorOnPlayAction)EditorGUILayout.EnumPopup("OnPlay Action", (ReplayEditorOnPlayAction)state);
                EditorPrefs.SetInt("replay_onplay_state", state);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();


                /// FRAME INFORMATION
                {
                    /// TIMELINE SCRUBBER
                    DrawTimeline();

                    EditorGUILayout.Space();

                    GUILayout.Label("Frame Info", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Current Frame: ", Replay.active.currentFrame.ToString());
                    time = Replay.active.currentTime;
                    min = Mathf.FloorToInt(time / 60);
                    sec = Mathf.FloorToInt(time - min * 60);
                    mill = Mathf.FloorToInt((time - (float)Math.Truncate(time)) * 100);
                    EditorGUILayout.LabelField("Current Time: ", min.ToString("00") + ":" + sec.ToString("00") + ":" + mill.ToString("00"));
                }

                /// INPUT STATE FOR SELECTED FRAME
                /*    showInputState = EditorGUILayout.Foldout(showInputState, "Input State");
                    if (showInputState)
                    {
                        EditorGUI.indentLevel++;
                        DrawInputState();
                        EditorGUI.indentLevel--;
                    }*/

                showRPCState = EditorGUILayout.Foldout(showRPCState, "Remote Procedure Calls");
                if (showRPCState)
                {
                    EditorGUI.indentLevel++;
                    DrawRPCState();
                    EditorGUI.indentLevel--;
                }

                /*   showVariableState = EditorGUILayout.Foldout(showVariableState, "Cluster Variables");
                   if (showVariableState)
                   {
                       EditorGUI.indentLevel++;
                       DrawVariableState();
                       EditorGUI.indentLevel--;
                   }

                   showObjectState = EditorGUILayout.Foldout(showObjectState, "Cluster Object States");
                   if (showObjectState)
                   {
                       EditorGUI.indentLevel++;
                       DrawObjectState();
                       EditorGUI.indentLevel--;
                   }*/
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close"))
            {
                EditorPrefs.DeleteKey("replay_file");
            }
        }

        #region GUI Drawing
        void DrawTimeline()
        {
            Rect r = EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            Color guiColor = GUI.color;

            EditorGUI.BeginDisabledGroup(!UnityEngine.Application.isPlaying);

            // record
            if (Replay.active.isRecording)
                GUI.color = Color.red;
            EditorGUI.BeginDisabledGroup(Replay.active.state != ReplayState.Ready);
            if (GUILayout.Button(recordButtonContent, GUILayout.Width(32), GUILayout.Height(32)))
                OnRecord();
            EditorGUI.EndDisabledGroup();

            GUI.color = guiColor;

            // play
            if (Replay.active.isPlaying)
                GUI.color = Color.green;
            EditorGUI.BeginDisabledGroup(Replay.active.state != ReplayState.Ready);
            if (GUILayout.Button(playButtonContent, GUILayout.Width(32), GUILayout.Height(32)))
                OnPlay();
            EditorGUI.EndDisabledGroup();

            GUI.color = guiColor;

            // stop
            if (GUILayout.Button(stopButtonContent, GUILayout.Width(32), GUILayout.Height(32)))
                OnStop();
            EditorGUI.EndDisabledGroup();

            // pause
            if (Replay.active.isPaused)
                GUI.color = Color.grey;
            EditorGUI.BeginDisabledGroup(Replay.active.state != ReplayState.Recording &&
                Replay.active.state != ReplayState.Playing &&
                Replay.active.state != ReplayState.PlayingPaused &&
                Replay.active.state != ReplayState.RecordingPaused);
            if (GUILayout.Button(pauseButtonContent, GUILayout.Width(32), GUILayout.Height(32)))
                OnPause();

            GUI.color = guiColor;

            // reset to start
            EditorGUI.BeginDisabledGroup(!Replay.active.isPaused);
            if (GUILayout.Button(beginingButtonContent, GUILayout.Width(32), GUILayout.Height(32)))
                OnJumpToStart();

            // back 1 frame
            if (GUILayout.Button(backButtonContent, GUILayout.Width(32), GUILayout.Height(32)))
                OnPrevious();

            // forward 1 frame
            if (GUILayout.Button(forwardButtonContent, GUILayout.Width(32), GUILayout.Height(32)))
                OnNext();

            // reset to end
            if (GUILayout.Button(endButtonContent, GUILayout.Width(32), GUILayout.Height(32)))
                OnJumpToEnd();
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            r = EditorGUILayout.BeginVertical();

            Vector2 size = GUIStyle.none.CalcSize(new GUIContent("00:00:00"));

            EditorGUI.DrawRect(new Rect(r.x + size.x * 0.5f + 5, r.y, r.width - size.x - 10, 50), Color.grey);

            EditorGUI.DrawRect(new Rect(r.x + 10 + 50, r.y, 1, 50), Color.white);

            EditorGUI.LabelField(new Rect(r.x + 5, r.y + 50, size.x + 5, size.y), "00:00:00");
            EditorGUI.LabelField(new Rect(r.width - size.x - 5, r.y + 50, size.x + 5, size.y), "00:00:00");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(50 + size.y);
        }

        void DrawInputState()
        {
            EditorGUILayout.Vector3Field("Mouse Position", Input.mousePosition);

            // key states
            dimensionList.DoLayoutList();

            // button states

            // axis states

        }

        void DrawRPCState()
        {
            // list of RPC calls and their data
        }

        void DrawObjectState()
        {
            // list of objects and their transforms
        }

        void DrawVariableState()
        {
            // list of shared variables and their values
        }
        #endregion
    }
}
