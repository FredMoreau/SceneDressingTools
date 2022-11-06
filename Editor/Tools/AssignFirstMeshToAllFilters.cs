using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Unity.SceneDressingTools.Editor
{
    public static class AssignFirstMeshToAllFilters
    {
        [MenuItem("Assets/Scene Dressing/AssignFirstMeshToAllFiltersFromSelection")]
        public static void AssignFirstMeshToAllFiltersFromSelection()
        {
            var meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);

            var filters = (from item in Object.FindObjectsOfType<MeshFilter>(true)
                           where meshes.Contains(item.sharedMesh)
                           select item).ToArray();

            foreach (var filter in filters)
                filter.sharedMesh = meshes[0];

            var paths = new List<string>();
            for (int i = 1; i < meshes.Length; i++)
            {
                paths.Add(AssetDatabase.GetAssetPath(meshes[i]));
            }
            var failed = new List<string>();
            AssetDatabase.DeleteAssets(paths.ToArray(), failed);
        }

        [MenuItem("Assets/Scene Dressing/AssignFirstMeshToAllFiltersFromSelection", validate = true)]
        public static bool AssignFirstMeshToAllFiltersFromSelection_Validate()
        {
            return Selection.GetFiltered<Mesh>(SelectionMode.Assets).Length > 1;
        }
    }
}