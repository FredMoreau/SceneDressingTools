using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Unity.SceneDressingTools.Editor
{
    internal static class MeshFilterUtilities
    {
        internal static void AssignMesh(MeshFilter meshFilter, Mesh mesh, bool applyOverridesToMostInnerPrefab = false)
        {
            var undoLvl = Undo.GetCurrentGroup();
            Undo.RecordObject(meshFilter, "Replace Mesh");

            meshFilter.sharedMesh = mesh;

            if (PrefabUtility.IsPartOfPrefabInstance(meshFilter))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);
            }

            if (PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(meshFilter) && applyOverridesToMostInnerPrefab)
            {
                var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshFilter);
                var leafPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originalSource);
                PrefabUtilities.ApplyMeshOverrides(meshFilter, leafPath);
            }

            Undo.SetCurrentGroupName("Replace Mesh");
            Undo.CollapseUndoOperations(undoLvl);
        }

        internal static void ReplaceMesh(MeshFilter[] meshFilters, Mesh target, Mesh replacement)
        {
            var mFilters = (from item in meshFilters
                            where item.sharedMesh == target
                              select item).ToArray();

            foreach (MeshFilter meshFilter in mFilters)
            {
                AssignMesh(meshFilter, replacement);
            }
        }

        internal static void ReplaceMeshInSelection(Mesh targetMesh, Mesh sourceMesh)
        {
            if (targetMesh.GetSameMeshFiltersWithinSelection(out MeshFilter[] meshFilters))
            {
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    AssignMesh(meshFilter, sourceMesh);
                }
            }
        }
    }
}