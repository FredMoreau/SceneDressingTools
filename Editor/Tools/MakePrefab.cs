using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace Unity.SceneDressingTools.Editor
{
    public static class MakePrefab
    {
        static readonly bool makePrefabOfSingleInstanceMeshes = false, makePrefabOfFirstLevelChildren = true;

        [MenuItem("GameObject/Scene Dressing/MakePrefabFromSelection")]
        public static void MakePrefabFromSelection()
        {
            var root = Selection.activeGameObject;

            var path = EditorUtility.SaveFilePanelInProject("Save Prefab to", root.name, "prefab", "Save the new Prefab as", Application.dataPath);
            if (path == string.Empty)
                return;
            
            var dir = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var meshesAndFilters = new Dictionary<Mesh, List<MeshFilter>>();

            foreach (var mFilter in root.GetComponentsInChildren<MeshFilter>())
            {
                if (mFilter.transform.childCount > 0) // only leaf objects for now
                    continue;

                if (!meshesAndFilters.ContainsKey(mFilter.sharedMesh))
                    meshesAndFilters.Add(mFilter.sharedMesh, new List<MeshFilter>() { mFilter });
                else
                    meshesAndFilters[mFilter.sharedMesh].Add(mFilter);
            }

            foreach (var kvp in meshesAndFilters)
            {
                if (kvp.Value.Count == 1 && !makePrefabOfSingleInstanceMeshes)
                    continue;

                var firstInstance = kvp.Value[0].gameObject;
                var instance = PrefabUtility.SaveAsPrefabAssetAndConnect(firstInstance, Path.Combine(dir, firstInstance.name + ".prefab"), InteractionMode.AutomatedAction);

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var go = PrefabUtility.InstantiatePrefab(instance, kvp.Value[i].transform.parent) as GameObject;
                    go.transform.localRotation = kvp.Value[i].transform.localRotation;
                    go.transform.localPosition = kvp.Value[i].transform.localPosition;
                    go.transform.localScale = kvp.Value[i].transform.localScale;
                    Object.DestroyImmediate(kvp.Value[i].gameObject);
                }
            }

            if (makePrefabOfFirstLevelChildren)
            {
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    var child = root.transform.GetChild(i).gameObject;
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(child))
                        continue;

                    PrefabUtility.SaveAsPrefabAssetAndConnect(child, Path.Combine(dir, child.name + ".prefab"), InteractionMode.AutomatedAction);
                }
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(root, path, InteractionMode.AutomatedAction);
        }

        [MenuItem("GameObject/Scene Dressing/MakePrefabFromSelection", validate = true)]
        public static bool MakePrefabFromSelection_Validate()
        {
            return Selection.activeGameObject != null && !PrefabUtility.IsPartOfPrefabInstance(Selection.activeGameObject);
        }
    }
}