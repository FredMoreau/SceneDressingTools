using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.SceneDressingTools.Editor
{
    public static class ExtractMeshes
    {
        [MenuItem("GameObject/Scene Dressing/Extract Meshes From Selection")]
        [MenuItem("Assets/Scene Dressing/Extract Meshes From Selection")]
        public static void ExtractMeshesFromSelectionRoot()
        {
            Dictionary<Mesh, Mesh> meshes = new Dictionary<Mesh, Mesh>();

            var meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();

            foreach (var mFilter in meshFilters)
            {
                if (!meshes.ContainsKey(mFilter.sharedMesh))
                {
                    meshes.Add(mFilter.sharedMesh, mFilter.sharedMesh.ExtractMesh());
                }
            }

            foreach (var kvp in meshes)
            {
                MeshFilterUtilities.ReplaceMeshInSelection(kvp.Key, kvp.Value);
            }
        }

        [MenuItem("GameObject/Scene Dressing/Extract Meshes From Selection", validate = true)]
        [MenuItem("Assets/Scene Dressing/Extract Meshes From Selection", validate = true)]
        public static bool ExtractMeshesFromSelectionRoot_Validate()
        {
            return Selection.activeGameObject != null;
        }
    }
}