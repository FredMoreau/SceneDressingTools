using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEditor.PlayerSettings;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Unity.SceneDressingTools.Editor
{
    public static class PivotPlacementTool
    {
        static Vector3 firstPosition, secondPosition, thirdPosition;
        static Bounds? firstBounds, secondBounds, thirdBounds;

        [MenuItem("Tools/Scene Dressing/Create Pivot Object")]
        public static void CreatePivotObject()
        {
            if (Selection.transforms.Length == 2)
                CreateWithTwoObjects();
            else
                CreateWithThreeObjects();
        }

        [MenuItem("Tools/Scene Dressing/Create Pivot Object", validate = true)]
        public static bool CreatePivotObject_Validate()
        {
            return Selection.transforms.Length > 1 && Selection.transforms.Length < 4;
        }

        static void CreateWithTwoObjects()
        {
            var firstTx = Selection.transforms[0];
            var secondTx = Selection.transforms[1];

            firstPosition = firstTx.position;
            secondPosition = secondTx.position;

            firstBounds = firstTx.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer) ? meshRenderer.bounds : null;
            secondBounds = secondTx.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer2) ? meshRenderer2.bounds : null;

            var first = firstBounds.HasValue ? firstBounds.Value.center : firstPosition;
            var second = secondBounds.HasValue ? secondBounds.Value.center : secondPosition;

            var pivotTx = new GameObject("PIVOT").transform;
            pivotTx.position = (first + second) * 0.5f;
            pivotTx.LookAt(second);

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                pivotTx.SetParent(prefabStage.prefabContentsRoot.transform, true);
            }

            //SceneView.duringSceneGui += SceneView_PickPosition;
        }

        private static void SceneView_PickPosition(SceneView sceneView)
        {
            Handles.color = Color.yellow;
            GUI.color = Color.cyan;

            DrawBounds(firstBounds);
            DrawBounds(secondBounds);
        }

        private static void SceneView_PickLookat(SceneView sceneView)
        {
            Handles.color = Color.yellow;
            GUI.color = Color.cyan;

            DrawBounds(firstBounds);
            DrawBounds(secondBounds);
        }

        static void CreateWithThreeObjects()
        {
            var firstTx = Selection.transforms[0];
            var secondTx = Selection.transforms[1];
            var thirdTx = Selection.transforms[2];

            firstPosition = firstTx.position;
            secondPosition = secondTx.position;
            thirdPosition = thirdTx.position;

            firstBounds = firstTx.GetComponent<MeshRenderer>()?.bounds;
            secondBounds = secondTx.GetComponent<MeshRenderer>()?.bounds;
            thirdBounds = thirdTx.GetComponent<MeshRenderer>()?.bounds;

            var first = firstBounds.HasValue ? firstBounds.Value.center : firstPosition;
            var second = secondBounds.HasValue ? secondBounds.Value.center : secondPosition;
            var third = thirdBounds.HasValue ? thirdBounds.Value.center : thirdPosition;

            var pivotTx = new GameObject("PIVOT").transform;
            pivotTx.position = (second + first) * 0.5f;
            pivotTx.LookAt(second, third);
            //pivotTx.rotation = Quaternion.LookRotation((second - first).normalized, third);
        }

        static void DrawBounds(Bounds? bounds, string label = "")
        {
            Handles.color =
                bounds.Value.Contains(SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(Event.current.mousePosition)) ?
                Color.yellow :
                Color.red;

            if (!firstBounds.HasValue)
                return;

            Handles.DrawWireCube(bounds.Value.center, bounds.Value.size);

            if (label != string.Empty)
                Handles.Label(bounds.Value.center, label);
        }
    }
}