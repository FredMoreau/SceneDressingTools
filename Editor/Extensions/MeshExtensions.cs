using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace Unity.SceneDressingTools.Editor
{
    internal static class MeshExtensions
    {
        internal static bool GetInstancedRenderers(this Mesh mesh, out MeshRenderer[] meshRenderers)
        {
            meshRenderers = (from item in Object.FindObjectsOfType<MeshFilter>(true)
                             where item.sharedMesh == mesh
                             select item.GetComponent<MeshRenderer>()).ToArray();

            return meshRenderers.Length > 0;
        }

        internal static bool GetSameMeshFilters(this Mesh mesh, out MeshFilter[] meshFilters)
        {
            meshFilters = (from item in Object.FindObjectsOfType<MeshFilter>()
                             where item.sharedMesh == mesh
                             select item.GetComponent<MeshFilter>()).ToArray();

            return meshFilters.Length > 0;
        }

        internal static bool GetSameMeshFiltersWithinSelection(this Mesh mesh, out MeshFilter[] meshFilters)
        {
            meshFilters = (from item in Selection.GetFiltered<MeshFilter>(SelectionMode.Deep)
                             where item.sharedMesh == mesh
                             select item).ToArray();

            return meshFilters.Length > 0;
        }

        internal static Mesh ExtractMesh(this Mesh original, string path = "", string name = "")
        {
            if (path == string.Empty)
                path = "Assets/SceneDressing/Meshes";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (name == string.Empty)
                name = original.name;

            Mesh newMesh = Object.Instantiate(original);
            string destinationPath = Path.Combine(path, name);
            destinationPath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + ".asset");
            AssetDatabase.CreateAsset(newMesh, destinationPath);

            return AssetDatabase.LoadAssetAtPath<Mesh>(destinationPath);
        }
    }
}