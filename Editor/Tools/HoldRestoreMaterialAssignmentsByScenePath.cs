using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.SceneDressingTools.Editor
{
    public static class HoldRestoreMaterialAssignmentsByScenePath
    {
        static Dictionary<string, Material[]> materialAssignments = new Dictionary<string, Material[]>();

        [MenuItem("Tools/Scene Dressing/Hold Selection Materials")]
        static void HoldSelectionMaterial()
        {
            var selection = Selection.GetFiltered<MeshRenderer>(SelectionMode.Deep);
            foreach (var meshRenderer in selection)
            {
                materialAssignments.Add(meshRenderer.transform.GetGameObjectPath(), meshRenderer.sharedMaterials);
            }
        }

        [MenuItem("Tools/Scene Dressing/Restore Selection Materials")]
        static void RestoreSelectionMaterial()
        {
            var selection = Selection.GetFiltered<MeshRenderer>(SelectionMode.Deep);

            Undo.RecordObjects(selection, "Restore Material Assignments");

            foreach (var meshRenderer in selection)
            {
                meshRenderer.sharedMaterials = materialAssignments[meshRenderer.transform.GetGameObjectPath()];
            }
        }

        private static string GetGameObjectPath(this Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}