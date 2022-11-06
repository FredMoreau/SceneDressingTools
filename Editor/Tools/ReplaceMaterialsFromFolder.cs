using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Unity.SceneDressingTools.Editor
{
    public static class ReplaceMaterialsFromFolder
    {
        [MenuItem("Tools/Scene Dressing/Replace Materials from Folder")]
        static void ReplaceSelectionMaterialFromFolder()
        {
            var selection = Selection.GetFiltered<MeshRenderer>(SelectionMode.Deep);

            var path = EditorUtility.OpenFolderPanel("Select Material Source Folder", Application.dataPath, "");

            if (path == string.Empty)
                return;

            var assetDbPath = new string[1] { path.Replace(Application.dataPath, "/Assets") };

            Undo.RecordObjects(selection, "Replace Materials from Folder");

            foreach (var meshRenderer in selection)
            {
                if (meshRenderer.sharedMaterials.Length == 0)
                    continue;

                Material[] materials = meshRenderer.sharedMaterials;

                for (int i = 0; i < materials.Length; i++)
                {
                    var assetPath = AssetDatabase.FindAssets(meshRenderer.sharedMaterials[i].name, assetDbPath);
                    if (assetPath.Length == 0)
                        continue;

                    var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath[0]);
                    if (mat == null)
                        continue;

                    materials[i] = mat;
                }

                meshRenderer.sharedMaterials = materials;
            }
        }
    }

    public class ReplaceMaterialWindow : EditorWindow
    {
        [System.Serializable]
        public struct MaterialReplacement
        {
            public Material original;
            public Material replacement;
        }

        [SerializeField] MaterialReplacement[] materialReplacements = new MaterialReplacement[0];
        SerializedObject serializedObject;
        SerializedProperty serializedProperty;

        [MenuItem("Tools/Scene Dressing/Replace Materials")]
        static void Init()
        {
            GetWindow<ReplaceMaterialWindow>().Show();
        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            serializedProperty = serializedObject.FindProperty("materialReplacements");
        }

        private void OnGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedProperty);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("GO"))
            {
                var selection = Selection.GetFiltered<MeshRenderer>(SelectionMode.Deep);

                Undo.RecordObjects(selection, "Replace Materials");

                var replacementDictionary = materialReplacements.ToDictionary(x => x.original, x => x.replacement);

                foreach (var meshRenderer in selection)
                {
                    if (meshRenderer.sharedMaterials.Length == 0)
                        continue;

                    Material[] materials = meshRenderer.sharedMaterials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = replacementDictionary[materials[i]];
                    }

                    meshRenderer.sharedMaterials = materials;
                }
            }
        }
    }
}