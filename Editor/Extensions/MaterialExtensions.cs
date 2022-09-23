using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
//using System;

namespace Unity.SceneDressingTools.Editor
{
    public static class MaterialExtensions
    {
        internal static bool GetSameMaterialRenderers(this Material material, out MeshRenderer[] meshRenderers)
        {
            meshRenderers = (from item in Object.FindObjectsOfType<MeshRenderer>()
                             where item.sharedMaterials.Contains(material)
                             select item.GetComponent<MeshRenderer>()).ToArray();

            return meshRenderers.Length > 0;
        }

        internal static bool GetSameMaterialRenderersWithinSelection(this Material material, out MeshRenderer[] meshRenderers)
        {
            meshRenderers = (from item in Selection.GetFiltered<MeshRenderer>(SelectionMode.Deep)
                             where item.sharedMaterials.Contains(material)
                             select item.GetComponent<MeshRenderer>()).ToArray();

            return meshRenderers.Length > 0;
        }

        internal static Material DuplicateMaterial(this Material original, string path = "", string name = "")
        {
            string sourcePath = AssetDatabase.GetAssetPath(original);
            if (Path.GetExtension(sourcePath) != ".mat")
            {
                return ExtractMaterial(original, path, name);
            }

            if (path == string.Empty)
                path = "Assets/SceneDressing/Materials";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (name == string.Empty)
                name = original.name;

            string destinationPath = Path.Combine(path, name);
            destinationPath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + ".mat");
            AssetDatabase.CopyAsset(sourcePath, destinationPath);

            return AssetDatabase.LoadAssetAtPath<Material>(destinationPath);
        }

        internal static Material ExtractMaterial(this Material original, string path = "", string name = "")
        {
            if (path == string.Empty)
                path = "Assets/SceneDressing/Materials";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (name == string.Empty)
                name = original.name;

            Material material = new Material(original);
            string destinationPath = Path.Combine(path, name);
            destinationPath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + ".mat");
            AssetDatabase.CreateAsset(material, destinationPath);

            return AssetDatabase.LoadAssetAtPath<Material>(destinationPath);
        }

#if UNITY_2022_1_OR_NEWER
        internal static Material CreateMaterialVariant(this Material original, string path = "", string name = "")
        {
            var mat = DuplicateMaterial(original, path, name);
            mat.parent = original;
            AssetDatabase.SaveAssetIfDirty(mat);

            return mat;
        }
#endif
    }
}