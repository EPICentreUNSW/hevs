using UnityEditor;
using UnityEngine;

namespace HEVS
{
    public class SceneChecker
    {
        public static bool warningOnHierarchyChange = true;
        static bool overrideWarnings = false;
        static bool justEnteredEditor = false;

        [InitializeOnLoadMethod]
        public static void Initiailise()
        {
            if (warningOnHierarchyChange)
                EditorApplication.hierarchyChanged += OnCheckClusterObjectParents;

            EditorApplication.playModeStateChanged += OnPlayStateChange;
        }

        static void OnPlayStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                overrideWarnings = false;
                justEnteredEditor = true;
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
                overrideWarnings = true;
        }

        static bool CheckClusterObjectParents()
        {
            bool allClear = true;
       /*     ClusterObject[] cos = SceneView.FindObjectsOfType<ClusterObject>();
            foreach (ClusterObject co in cos)
            {
                if (co.transform.parent &&
                    co.transform.parent.GetComponent<ClusterObject>() == null &&
                    !co.transform.parent.gameObject.isStatic)
                {
                    //Debug.LogWarning("ClusterObject [" + co.name + "] is a child of object [" + co.transform.parent.name + "] that does not contain a ClusterObject component and is not flagged as static! Please either add a ClusterObject to the parent, or ensure that the parent transform never changes, so that it can synchronize correctly within a cluster.");
                    allClear = false;
                }
            }*/
            return allClear;
        }

        static void OnCheckClusterObjectParents()
        {
            if (!overrideWarnings)
            {
                if (justEnteredEditor)
                    justEnteredEditor = false;
                else
                    CheckClusterObjectParents();
            }
        }

        public static void ToggleWarnings(bool enable)
        {
            if (enable != SceneChecker.warningOnHierarchyChange)
            {
                if (SceneChecker.warningOnHierarchyChange)
                    EditorApplication.hierarchyChanged -= OnCheckClusterObjectParents;
                else
                    EditorApplication.hierarchyChanged += OnCheckClusterObjectParents;
                warningOnHierarchyChange = enable;
            }
        }

     //   [MenuItem("HEVS/Check Scene", priority = 1)]
        static void CheckScene()
        {
            Debug.Log("HEVS: Start Scene Check...");
            bool allClear = true;

            // does scene contain HEVS Settings object?
            var settings = SceneView.FindObjectsOfType<HEVS.Core>();
            if (settings == null || settings.Length == 0)
            {
                Debug.LogError("HEVS: Could not find a HEVS Application component on any object within the scene!");
                allClear = false;
            }
            else if (settings.Length > 1)
            {
                Debug.LogError("HEVS: Too many HEVS Application components were found within the scene! There can be only one!");
                allClear = false;
            }

            // does scene contain a single main camera?
            int mainCameraCounter = 0;
            var objects = SceneView.FindObjectsOfType<UnityEngine.Camera>();
            foreach (var go in objects)
            {
                if (go.tag == "MainCamera")
                {
                    mainCameraCounter++;
                }
            }
            if (mainCameraCounter > 1)
            {
                Debug.LogError("HEVS: Too main Cameras tagged as MainCamera within the scene!");
                allClear = false;
            }
            else if (mainCameraCounter == 0)
            {
                Debug.LogError("HEVS: Could not find a Camera tagged as MainCamera within the scene!");
                allClear = false;
            }

            SceneOrigin[] origins = SceneView.FindObjectsOfType<SceneOrigin>();
            if (origins.Length > 1)
            {
                Debug.LogError("HEVS: Too many SceneOrigins within the scene! Ensure only one SceneOrigin exists within the scene.");
                allClear = false;
            }

            bool parentsOk = CheckClusterObjectParents();

            if (allClear)
            {
                if (parentsOk)
                    Debug.Log("HEVS: Scene Check Complete.\nThe scene contains the minimum requirements to run, as long as a valid configuration file is used!");
                else
                    Debug.Log("HEVS: Scene Check Complete (With Warnings!).\nThe scene contains the minimum requirements to run, as long as a valid configuration file is used, but the hierarchy does contain inconsistent parenting that may effect cluster synchronization. See above warnings for more details!");
            }
            else
                Debug.LogError("HEVS: Scene Check Complete (With Errors!).\nThe scene is incorrectly set up for running HEVS! Please address the issues listed above.");
        }
    }
}