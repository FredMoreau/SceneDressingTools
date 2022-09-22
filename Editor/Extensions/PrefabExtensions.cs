using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Unity.SceneDressingTools.Editor
{
    internal static class PrefabExtensions
    {
        internal static string GetInnermostPrefabAssetPath(this Component component)
        {
            var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(component);
            return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originalSource);
        }
    }
}