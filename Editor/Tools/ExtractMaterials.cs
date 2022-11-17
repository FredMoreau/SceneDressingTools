using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.SceneDressingTools.Editor
{
    public static class ExtractMaterials
    {
        [MenuItem("GameObject/Scene Dressing/Extract Materials From Selection")]
        [MenuItem("Assets/Scene Dressing/Extract Materials From Selection")]
        public static void ExtractMaterialsFromSelectionRoot()
        {
            // TODO : check if any object is within a non editable asset

            Dictionary<Material, Material> materials = new Dictionary<Material, Material>();

            var renderers = Selection.activeGameObject.GetComponentsInChildren<MeshRenderer>(true);

            var n = Selection.activeGameObject.transform.root.name;
            var path = Preferences.AssetExtractionPath.Replace("[modelName]", n);
            path = path.Replace("[assetType]", "Materials");
            path = "Assets/" + path;

            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (!materials.ContainsKey(material))
                    {
                        materials.Add(material, material.ExtractMaterial(path));
                    }
                }
            }

            foreach (var kvp in materials)
            {
                MaterialUtilities.ReplaceMaterialInSelection(kvp.Key, kvp.Value);
            }
        }

        [MenuItem("GameObject/Scene Dressing/Extract Materials From Selection", validate = true)]
        [MenuItem("Assets/Scene Dressing/Extract Materials From Selection", validate = true)]
        public static bool ExtractMaterialsFromSelectionRoot_Validate()
        {
            return Selection.activeGameObject != null;
        }
    }
}