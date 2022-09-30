using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Unity.SceneDressingTools.Editor
{
    internal static class MaterialUtilities
    {
        internal static void CopyMaterialOver(MeshRenderer meshRenderer, int index, Material material)
        {
            Undo.RecordObject(meshRenderer.sharedMaterials[index], "Copy Material Properties");

            meshRenderer.sharedMaterials[index].shader = material.shader;
            meshRenderer.sharedMaterials[index].CopyPropertiesFromMaterial(material);
            // HACK : updating the thumbnail
            var tmpCol = meshRenderer.sharedMaterials[index].color;
            meshRenderer.sharedMaterials[index].color = Color.black;
            meshRenderer.sharedMaterials[index].color = tmpCol;
        }

        internal static void AssignMaterial(MeshRenderer meshRenderer, int index, Material material, bool applyOverridesToMostInnerPrefab = false)
        {
            var undoLvl = Undo.GetCurrentGroup();
            Undo.RecordObject(meshRenderer, "Replace Material");

            if (meshRenderer.sharedMaterials.Length == 1)
            {
                meshRenderer.sharedMaterial = material;
            }
            else
            {
                var materials = meshRenderer.sharedMaterials;
                materials[index] = material;
                meshRenderer.sharedMaterials = materials;
            }

            RecordIfPrefabInstanceAndApplyOverridesIfCanBeAppliedTo(meshRenderer, applyOverridesToMostInnerPrefab);

            Undo.SetCurrentGroupName("Replace Material");
            Undo.CollapseUndoOperations(undoLvl);
        }

        internal static void AssignMaterialToAllRenderersAllIDs(MeshRenderer[] meshRenderers, Material material, bool applyOverridesToMostInnerPrefab = false)
        {
            var undoLvl = Undo.GetCurrentGroup();
            Undo.RecordObjects(meshRenderers, "Assign Material to Renderers");
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (meshRenderer.sharedMaterials.Length == 0)
                {
                    meshRenderer.sharedMaterial = material;
                }
                else
                {
                    var materials = meshRenderer.sharedMaterials;
                    for (int m = 0; m < materials.Length; m++)
                        materials[m] = material;
                    meshRenderer.sharedMaterials = materials;
                }

                RecordIfPrefabInstanceAndApplyOverridesIfCanBeAppliedTo(meshRenderer, applyOverridesToMostInnerPrefab);
            }
            Undo.SetCurrentGroupName("Replace Material");
            Undo.CollapseUndoOperations(undoLvl);
        }

        // TODO : optimize so that it doesn't take a crazy amount of time to loop over all instances
        static void RecordIfPrefabInstanceAndApplyOverridesIfCanBeAppliedTo (MeshRenderer meshRenderer, bool applyOverridesToMostInnerPrefab = false)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(meshRenderer))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(meshRenderer);
            }

            if (PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(meshRenderer) && (applyOverridesToMostInnerPrefab || Preferences.AutoApplyOverrides))
            {
                var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshRenderer);
                var leafPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originalSource);
                PrefabUtilities.ApplyMaterialsOverrides(meshRenderer, leafPath);
            }
        }

        internal static void ReplaceMaterial(MeshRenderer[] meshRenderers, Material target, Material replacement)
        {
            var mRenderers = (from item in meshRenderers
                              where item.sharedMaterials.Contains(target)
                              select item).ToArray();

            foreach (MeshRenderer meshRenderer in mRenderers)
            {
                int slot = Array.IndexOf(meshRenderer.sharedMaterials, target);
                AssignMaterial(meshRenderer, slot, replacement);
            }
        }

        internal static void ReplaceMaterialInSelection(Material targetMaterial, Material sourceMaterial)
        {
            if (targetMaterial.GetSameMaterialRenderersWithinSelection(out MeshRenderer[] meshRenderers))
            {
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    int slot = Array.IndexOf(meshRenderer.sharedMaterials, targetMaterial);
                    AssignMaterial(meshRenderer, slot, sourceMaterial);
                }
            }
        }
    }
}