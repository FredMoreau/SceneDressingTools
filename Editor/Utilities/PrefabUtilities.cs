using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

namespace Unity.SceneDressingTools.Editor
{
    internal static class PrefabUtilities
    {
        internal static Dictionary<string, string> GetPrefabsHierarchy(UnityEngine.Object obj)
        {
            Dictionary<string, string> namesAndPaths = new Dictionary<string, string>();

            if (!PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(obj))
                return namesAndPaths;

            var topLevel = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            var leafLevel = PrefabUtility.GetNearestPrefabInstanceRoot(obj);

            var source = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj);

            var leafPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originalSource);

            namesAndPaths.Add(originalSource.name, leafPath);

            bool isNested = source != originalSource;
            if (isNested)
            {
                GameObject interLevel = leafLevel;
                do
                {
                    interLevel = PrefabUtility.GetNearestPrefabInstanceRoot(interLevel.transform.parent.gameObject);
                    var name = PrefabUtility.GetCorrespondingObjectFromOriginalSource(interLevel).name;
                    var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(interLevel);
                    namesAndPaths.Add(name, path);
                }
                while (interLevel != topLevel);
            }

            namesAndPaths = namesAndPaths.Reverse().ToDictionary(x => x.Key, x => x.Value); ;
            return namesAndPaths;
        }

        internal static void ApplyMaterialsOverrides(MeshRenderer meshRenderer, string path)
        {
            SerializedObject serializedObject = new SerializedObject(meshRenderer);
            SerializedProperty serializedProperty = serializedObject.FindProperty("m_Materials");

            if (serializedProperty != null)
            {
                PrefabUtility.ApplyPropertyOverride(serializedProperty, path, InteractionMode.AutomatedAction);
                if (serializedProperty.arraySize > 0)
                {
                    for (var i = 0; i < serializedProperty.arraySize; i++)
                    {
                        var element = serializedProperty.GetArrayElementAtIndex(i);
                        //Debug.LogFormat("<color=cyan>{0} --> {1}</color>", element.propertyPath, element.objectReferenceValue);
                        PrefabUtility.ApplyPropertyOverride(element, path, InteractionMode.AutomatedAction);
                    }
                }
            }
        }

        internal static void ApplyMeshOverrides(MeshFilter meshFilter, string path)
        {
            SerializedObject serializedObject = new SerializedObject(meshFilter);
            SerializedProperty serializedProperty = serializedObject.FindProperty("m_Mesh");

            if (serializedProperty != null)
            {
                PrefabUtility.ApplyPropertyOverride(serializedProperty, path, InteractionMode.AutomatedAction);
            }
        }
    }
}