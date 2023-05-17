using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.SceneDressingTools.Editor
{
    public static class PrefabOverideBugInvestigation
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            var evt = Event.current;

            if (evt.type == EventType.DragPerform && evt.control)
            {
                var go = HandleUtility.PickGameObject(evt.mousePosition, out int submeshIndex);
                if (go && go.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                {
                    Material droppedMaterial = DragAndDrop.objectReferences.FirstOrDefault(x => x is Material) as Material;

                    if (droppedMaterial == null)
                        return;

                    var undoLvl = Undo.GetCurrentGroup();
                    Undo.RecordObject(meshRenderer, "Assign Material");

                    if (meshRenderer.sharedMaterials.Length == 1)
                    {
                        meshRenderer.sharedMaterial = droppedMaterial;
                    }
                    else
                    {
                        var materials = meshRenderer.sharedMaterials;
                        materials[submeshIndex] = droppedMaterial;
                        meshRenderer.sharedMaterials = materials;
                    }

                    if (PrefabUtility.IsPartOfPrefabInstance(meshRenderer))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(meshRenderer);
                    }

                    Undo.SetCurrentGroupName("Assign Material");
                    Undo.CollapseUndoOperations(undoLvl);
                }

                evt.Use();
            }
        }
    }
}