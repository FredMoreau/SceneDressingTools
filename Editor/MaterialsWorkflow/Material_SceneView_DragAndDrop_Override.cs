using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

namespace Unity.SceneDressingTools.Editor
{
    [InitializeOnLoad]
    public static class Material_SceneView_DragAndDrop_Override
    {
        static bool useShortcuts = false;
        static bool autoApplyOverrides = false; // FIXME --> causes out of bound indexes

        static Material_SceneView_DragAndDrop_Override()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (Selection.activeObject?.GetType() == typeof(Material))
            {
                Handles.BeginGUI();
                GUIStyle style = new GUIStyle(EditorStyles.helpBox);
                style.richText = true;
                style.stretchWidth = false;
                style.alignment = TextAnchor.MiddleLeft;
                style.padding = new RectOffset(24, 0, 0, 0);
                var h = sceneView.position.height;
                
                if (useShortcuts)
                {
                    var rect = GUILayoutUtility.GetRect(450, 84, style);
                    rect.position = new Vector2(12, h - 120);
                    GUI.Label(rect, "Use popup menu to Automatically Apply overrides to Prefabs.\n" +
                        "Hold <b>Control</b> to replace material in the whole scene.\n" +
                        "Hold <b>Alt</b> to propagate material assignments to all objects using the same Mesh.\n" +
                        "Hold <b>Alt + Control</b> to propagate to objects using the same Mesh and same Material.\n" +
                        //"Hold <b>Shift</b> to Automatically Apply overrides to Leaf Prefabs.\n" +
                        "Hold <b>Shift only</b> to copy/paste material properties.\n", style);
                }
                else
                {
                    var rect = GUILayoutUtility.GetRect(450, 32, style);
                    rect.position = new Vector2(12, h - 80);
                    GUI.Label(rect, "Hold <b>Ctrl (Cmd)</b> for more options when assigning a <i>Material</i> in <i>Scene View</i>.", style);
                }
                
                Handles.EndGUI();
            }

            var evt = Event.current;

            if (evt.type == EventType.DragUpdated)
            {
                // TODO : change pointer
            }
            else if (evt.type == EventType.DragPerform)
            {
                int submeshIndex;
                if (HandleUtility.PickGameObject(evt.mousePosition, out submeshIndex).TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
                {
                    Material targetMaterial = renderer.sharedMaterials[submeshIndex];
                    Material sourceMaterial = DragAndDrop.objectReferences.FirstOrDefault(x => x is Material) as Material;
                    
                    void PropagateToMeshInstances()
                    {
                        //evt.Use();
                        var meshFilter = renderer.GetComponent<MeshFilter>();
                        //Debug.LogFormat("<color=cyan>Applying {1} on all instances of {0}</color>", meshFilter.sharedMesh.name, sourceMaterial.name);
                        if (GetInstancedRenderers(meshFilter, out List<MeshRenderer> meshRenderers))
                        {
                            int slot = Array.IndexOf(renderer.sharedMaterials, targetMaterial);
                            foreach (MeshRenderer meshRenderer in meshRenderers)
                            {
                                ReplaceMaterial(meshRenderer, slot, sourceMaterial);
                            }
                        }
                    }

                    void PropagateToMaterials()
                    {
                        //Debug.LogFormat("<color=cyan>Replacing {1} with {0}</color>", sourceMaterial.name, targetMaterial.name);
                        //evt.Use();
                        if (GetSameMaterialRenderers(targetMaterial, out List<MeshRenderer> meshRenderers))
                        {
                            foreach (MeshRenderer meshRenderer in meshRenderers)
                            {
                                int slot = Array.IndexOf(meshRenderer.sharedMaterials, targetMaterial);
                                ReplaceMaterial(meshRenderer, slot, sourceMaterial);
                            }
                        }
                    }

                    void PropagateToMeshInstancesUsingMaterial()
                    {
                        //evt.Use();
                        var meshFilter = renderer.GetComponent<MeshFilter>();
                        //Debug.LogFormat("<color=cyan>Replacing {1} with {0} on all instances of {2}</color>", sourceMaterial.name, targetMaterial.name, meshFilter.sharedMesh.name);
                        if (GetInstancedRenderers(meshFilter, out List<MeshRenderer> r1) && GetSameMaterialRenderers(targetMaterial, out List<MeshRenderer> r2))
                        {
                            var meshRenderers = (from item in r1
                                                 where r2.Contains(item)
                                                 select item).ToList();

                            foreach (MeshRenderer meshRenderer in meshRenderers)
                            {
                                int slot = Array.IndexOf(meshRenderer.sharedMaterials, targetMaterial);
                                ReplaceMaterial(meshRenderer, slot, sourceMaterial);
                            }
                        }
                    }

                    void CopyMaterialProperties()
                    {
                        //evt.Use();
                        CopyMaterialOver(renderer, submeshIndex, sourceMaterial);
                    }

                    void AssignMaterialDuplicate()
                    {
                        var duplicate = DuplicateMaterial(sourceMaterial);
                        ReplaceMaterial(renderer, submeshIndex, duplicate);
                    }

#if UNITY_2022_1_OR_NEWER
                    void AssignMaterialVariant()
                    {
                        var duplicate = CreateMaterialVariant(sourceMaterial);
                        ReplaceMaterial(renderer, submeshIndex, duplicate);
                    }
#endif

                    void AddPrefabsMenuItems(GenericMenu menu, string path = "")
                    {
                        var prefabs = GetPrefabsHierarchy(renderer);
                        foreach (KeyValuePair<string, string> kvp in prefabs)
                        {
                            menu.AddItem(new GUIContent(string.Format("{0}{1}", path, kvp.Key), "Apply override to Prefab"), false, () => {
                                ReplaceMaterial(renderer, submeshIndex, sourceMaterial);
                                ApplyOverrides(renderer, kvp.Value);
                                return;
                            });
                        }
                    }

                    int unduLvl = Undo.GetCurrentGroup();

                    if (!useShortcuts && evt.control)
                    {
                        evt.Use();
                        var meshName = renderer.GetComponent<MeshFilter>().sharedMesh.name;

                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Assign Material (Default)", ""), false, () => {
                            ReplaceMaterial(renderer, submeshIndex, sourceMaterial);
                        });

                        menu.AddItem(new GUIContent(string.Format("Propagate to {0} instances", meshName), ""), false, () => {
                            PropagateToMeshInstances();
                        });

                        menu.AddItem(new GUIContent(string.Format("Propagate to meshes using {0}", targetMaterial.name), ""), false, () => {
                            PropagateToMaterials();
                        });

                        menu.AddItem(new GUIContent(string.Format("Propagate to {0} instances using {1}", meshName, targetMaterial.name), ""), false, () => {
                            PropagateToMeshInstancesUsingMaterial();
                        });

                        menu.AddItem(new GUIContent(string.Format("Copy {0} properties over {1}", sourceMaterial.name, targetMaterial.name), ""), false, () => {
                            CopyMaterialProperties();
                        });

                        menu.AddItem(new GUIContent(string.Format("Assign a copy of {0}", sourceMaterial.name), ""), false, () => {
                            AssignMaterialDuplicate();
                        });

#if UNITY_2022_1_OR_NEWER
                        menu.AddItem(new GUIContent(string.Format("Assign a Variant of {0}", sourceMaterial.name), ""), false, () => {
                            AssignMaterialVariant();
                        });
#endif

                        if (PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(renderer))
                        {
                            menu.AddSeparator("");
                            //menu.AddDisabledItem(new GUIContent("Assign and Apply to Prefab", ""));
                            AddPrefabsMenuItems(menu, "Assign and Apply to Prefab/");
                        }

                        menu.ShowAsContext();
                    }
                    else if (useShortcuts)
                    {
                        evt.Use();
                        if (!evt.alt && !evt.control && !evt.shift && PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(renderer))
                        {
                            GenericMenu menu = new GenericMenu();
                            
                            menu.AddItem(new GUIContent("Scene (Default)", "Store as override in the Scene"), false, () => {
                                return;
                            });
                            menu.AddSeparator("");

                            AddPrefabsMenuItems(menu);
                            menu.ShowAsContext();
                        }
                        else if (evt.alt && !evt.control)
                        {
                            PropagateToMeshInstances();
                        }
                        else if (evt.control && !evt.alt)
                        {
                            PropagateToMaterials();
                        }
                        else if (evt.control && evt.alt)
                        {
                            PropagateToMeshInstancesUsingMaterial();
                        }
                        else if (evt.shift)
                        {
                            CopyMaterialProperties();
                        }
                    }
                }
            }
        }

        static void CopyMaterialOver(MeshRenderer meshRenderer, int index, Material material)
        {
            Undo.RecordObject(meshRenderer.sharedMaterials[index], "Copy Material Properties");

            meshRenderer.sharedMaterials[index].CopyPropertiesFromMaterial(material);
            // HACK : updating the thumbnail
            var tmpCol = meshRenderer.sharedMaterials[index].color;
            meshRenderer.sharedMaterials[index].color = Color.black;
            meshRenderer.sharedMaterials[index].color = tmpCol;
        }

        // TODO : add material duplication option
        static Material DuplicateMaterial(Material original, string path = "", string name = "")
        {
            string sourcePath = AssetDatabase.GetAssetPath(original);
            
            if (path == string.Empty)
                path = "Assets/_Materials";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (name == string.Empty)
                name = original.name;

            string destinationPath = Path.Combine(path, name);
            destinationPath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + ".mat");
            AssetDatabase.CopyAsset(sourcePath, destinationPath);
            
            return AssetDatabase.LoadAssetAtPath<Material>(destinationPath);
        }

#if UNITY_2022_1_OR_NEWER
        static Material CreateMaterialVariant(Material original, string path = "", string name = "")
        {
            var mat = DuplicateMaterial(original, path, name);
            mat.parent = original;
            AssetDatabase.SaveAssetIfDirty(mat);

            return mat;
        }
#endif

        static Dictionary<string, string> GetPrefabsHierarchy(UnityEngine.Object obj)
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

        static void ApplyOverrides(MeshRenderer meshRenderer, string path)
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

        static void ReplaceMaterial(MeshRenderer meshRenderer, int index, Material material)
        {
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

            // TODO : add Undo recording of Prefab asset
            if (PrefabUtility.IsPartOfPrefabInstance(meshRenderer))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(meshRenderer);
            }

            if (PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(meshRenderer) && autoApplyOverrides)
            {
                var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshRenderer);
                var leafPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originalSource);
                ApplyOverrides(meshRenderer, leafPath);
            }
        }

        static bool GetInstancedRenderers(MeshFilter meshFilter, out List<MeshRenderer> meshRenderers)
        {
            meshRenderers = (from item in Component.FindObjectsOfType<MeshFilter>()
                            where item.sharedMesh == meshFilter.sharedMesh
                            select item.GetComponent<MeshRenderer>()).ToList();

            return meshRenderers.Count > 0;
        }

        static bool GetSameMaterialRenderers(Material material, out List<MeshRenderer> meshRenderers)
        {
            meshRenderers = (from item in Component.FindObjectsOfType<MeshRenderer>()
                             where item.sharedMaterials.Contains(material)
                             select item.GetComponent<MeshRenderer>()).ToList();

            return meshRenderers.Count > 0;
        }
    }
}