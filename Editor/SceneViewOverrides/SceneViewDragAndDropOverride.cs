using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Reflection;

namespace Unity.SceneDressingTools.Editor
{
    public static class SceneViewDragAndDropOverride
    {
        internal static bool isOn;
        internal static bool IsOn
        {
            get => isOn;
            set
            {
                if (value == isOn)
                    return;
                isOn = value;
                if (isOn)
#if UNITY_2019_1_OR_NEWER
                    SceneView.duringSceneGui += OnSceneGUI;
#else
                    SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
                else
#if UNITY_2019_1_OR_NEWER
                    SceneView.duringSceneGui -= OnSceneGUI;
#else
                    SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
            }
        }

        [InitializeOnLoadMethod]
        static void Init()
        {
            isOn = Preferences.EnableDragAndDropOverride;
            if (isOn)
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui += OnSceneGUI;
#else
                SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif

            Preferences.OnSettingsChanged += Preferences_OnSettingsChanged;
        }

        private static void Preferences_OnSettingsChanged()
        {
            IsOn = Preferences.EnableDragAndDropOverride;
        }

        internal static event Action<Material> OnMaterialClipboardChanged;

        internal enum AssignmentMode
        {
            AssignOriginal,
            AssignDuplicate,
#if UNITY_2022_1_OR_NEWER
            AssignVariant,
#endif
            CopyProperties
        }

        internal enum GameObjectMode
        {
            Assign,
            PopupMenu,
            AssignToAllMeshInstances,
            ReplaceInAllOpenScenes,
            ReplaceOnAllMeshInstances,
            ReplaceWithinSceneRoot
        }

        internal enum PrefabInstanceMode
        {
            Assign,
            PopupMenu,
            AssignToMostInnerPrefab
        }

        struct Mode
        {
            public string name;
            public AssignmentMode assignmentMode;
            public GameObjectMode gameObjectMode;
            public PrefabInstanceMode prefabInstanceMode;

            public Mode(string name, AssignmentMode assignmentMode, GameObjectMode gameObjectMode, PrefabInstanceMode prefabInstanceMode)
            {
                this.name = name;
                this.assignmentMode = assignmentMode;
                this.gameObjectMode = gameObjectMode;
                this.prefabInstanceMode = prefabInstanceMode;
            }
        }

        static Mode ctrlMode = new Mode("Assign Original to instances", AssignmentMode.AssignOriginal, GameObjectMode.AssignToAllMeshInstances, PrefabInstanceMode.Assign);
        static Mode altMode = new Mode("Replace all with Original", AssignmentMode.AssignOriginal, GameObjectMode.ReplaceInAllOpenScenes, PrefabInstanceMode.Assign);
        static Mode shiftMode = new Mode("Copy Properties", AssignmentMode.CopyProperties, GameObjectMode.Assign, PrefabInstanceMode.Assign);
        
        static Mode ctrlAltMode = new Mode("Replace with Orignal on all Instances", AssignmentMode.AssignOriginal, GameObjectMode.ReplaceOnAllMeshInstances, PrefabInstanceMode.PopupMenu);
        static Mode ctrlShiftMode = new Mode("Assign Duplicate To Instances", AssignmentMode.AssignDuplicate, GameObjectMode.AssignToAllMeshInstances, PrefabInstanceMode.PopupMenu);
        static Mode altShiftMode = new Mode("Replace all with Duplicate", AssignmentMode.AssignDuplicate, GameObjectMode.ReplaceInAllOpenScenes, PrefabInstanceMode.PopupMenu);
        
        static Mode ctrlAltShiftMode = new Mode("Replace with Duplicate on all Instances", AssignmentMode.AssignDuplicate, GameObjectMode.ReplaceOnAllMeshInstances, PrefabInstanceMode.PopupMenu);

        static Material materialClipboard;

        public static Material MaterialClipboard { get => materialClipboard; }

        static Type projectBrowserType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");

        static void OnSceneGUI(SceneView sceneView)
        {
#if !UNITY_2021_2_OR_NEWER
            Handles.BeginGUI();
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            style.stretchWidth = false;
            style.alignment = TextAnchor.MiddleLeft;
            style.padding = new RectOffset(24, 0, 0, 0);
            var h = sceneView.position.height;

            string prompt = "<b>Scene Dressing Override is On</b>.\n" +
                        "Hold <b>Control</b> to Assign Original to instances.\n" +
                        "Hold <b>Alt</b> to Replace all with Original.\n" +
                        "Hold <b>Shift</b> to Copy Properties\n" +
                        "Hold <b>Alt + Control</b> to Replace with Orignal on all Instances.\n" +
                        "Read documentation for more shortcuts.";

            var rect = GUILayoutUtility.GetRect(450, 84, style);
            rect.position = new Vector2(12, h - 120);
            GUI.Label(rect, prompt, style);

            Handles.EndGUI();
#endif

            var evt = Event.current;

            if (evt.type == EventType.MouseDown)
            {
                if (evt.button == 1 && evt.control && !evt.shift)
                {
                    var go = HandleUtility.PickGameObject(evt.mousePosition, out int submeshIndex);
                    if (go && go.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                    {
                        //EditorGUIUtility.PingObject(meshRenderer.sharedMaterials[submeshIndex]);
                        PingObjectInProjectWindow(meshRenderer.sharedMaterials[submeshIndex]);
                    }
                    evt.Use();
                }
                else if (evt.button == 1 && evt.shift && !evt.control)
                {
                    var go = HandleUtility.PickGameObject(evt.mousePosition, out int submeshIndex);
                    if (go && go.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                    {
                        EditorGUIUtility.PingObject(meshRenderer.GetComponent<MeshFilter>()?.sharedMesh);
                    }
                    evt.Use();
                }
                else if (evt.button == 1 && evt.control && evt.shift)
                {
                    var go = HandleUtility.PickGameObject(evt.mousePosition, out int submeshIndex);
                    if (go && go.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                    {
                        materialClipboard = meshRenderer.sharedMaterials[submeshIndex];
                        OnMaterialClipboardChanged?.Invoke(materialClipboard);
                    }
                    evt.Use();
                }
                else if (evt.button == 2 && evt.control && evt.shift)
                {
                    var go = HandleUtility.PickGameObject(evt.mousePosition, out int submeshIndex);
                    if (materialClipboard && go && go.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                    {
                        MaterialUtilities.AssignMaterial(meshRenderer, submeshIndex, materialClipboard, Preferences.AutoApplyOverrides);
                    }
                    evt.Use();
                }
            }
            else if (evt.type == EventType.DragUpdated)
            {
                // TODO : change pointer
                // TODO : implement preview
            }
            else if (evt.type == EventType.DragPerform)
            {
                Mode currentMode;
                if (evt.control && evt.alt && evt.shift)
                    currentMode = ctrlAltShiftMode;
                else if (evt.control && evt.alt)
                    currentMode = ctrlAltMode;
                else if (evt.control && evt.shift)
                    currentMode = ctrlShiftMode;
                else if (evt.alt && evt.shift)
                    currentMode = altShiftMode;
                else if (evt.control)
                    currentMode = ctrlMode;
                else if (evt.alt)
                    currentMode = altMode;
                else if (evt.shift)
                    currentMode = shiftMode;
                else
                    currentMode = new Mode("Custom", Preferences.AssignmentMode, Preferences.GameObjectMode, Preferences.PrefabInstanceMode);

                var go = HandleUtility.PickGameObject(evt.mousePosition, out int submeshIndex);
                if (go && go.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                {
                    Material targetMaterial = meshRenderer.sharedMaterials[submeshIndex];
                    Material sourceMaterial = DragAndDrop.objectReferences.FirstOrDefault(x => x is Material) as Material;

                    if (sourceMaterial == null)
                        return;

                    if (currentMode.assignmentMode == AssignmentMode.CopyProperties) // TODO Extract if Immutable
                    {
                        MaterialUtilities.CopyMaterialOver(meshRenderer, submeshIndex, sourceMaterial);
                    }
                    else
                    {
                        switch (currentMode.assignmentMode)
                        {
                            case AssignmentMode.AssignOriginal:
                                break;
                            case AssignmentMode.AssignDuplicate:
                                sourceMaterial = sourceMaterial.DuplicateMaterial(); // TODO Extract if Immutable
                                break;
#if UNITY_2022_1_OR_NEWER
                            case AssignmentMode.AssignVariant:
                                sourceMaterial = sourceMaterial.CreateMaterialVariant();
                                break;
#endif
                            default:
                                break;
                        }

                        if (PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(meshRenderer))
                        {
                            switch (currentMode.prefabInstanceMode)
                            {
                                case PrefabInstanceMode.Assign:
                                    MaterialUtilities.AssignMaterial(meshRenderer, submeshIndex, sourceMaterial);
                                    break;
                                case PrefabInstanceMode.PopupMenu:
                                    GenericMenu menu = new GenericMenu();
                                    AddGameObjectMenuItems(ref menu);
                                    menu.AddSeparator("");
                                    AddPrefabsMenuItems(ref menu);
                                    menu.ShowAsContext();
                                    break;
                                case PrefabInstanceMode.AssignToMostInnerPrefab:
                                    MaterialUtilities.AssignMaterial(meshRenderer, submeshIndex, sourceMaterial, true);
                                    break;
                            }
                        }
                        else
                        {
                            switch (currentMode.gameObjectMode)
                            {
                                case GameObjectMode.Assign:
                                    MaterialUtilities.AssignMaterial(meshRenderer, submeshIndex, sourceMaterial);
                                    break;
                                case GameObjectMode.PopupMenu:
                                    GenericMenu menu = new GenericMenu();
                                    AddGameObjectMenuItems(ref menu);
                                    menu.ShowAsContext();
                                    break;
                                case GameObjectMode.AssignToAllMeshInstances:
                                    AssignToAllMeshInstances();
                                    break;
                                case GameObjectMode.ReplaceInAllOpenScenes:
                                    ReplaceInAllOpenScenes();
                                    break;
                                case GameObjectMode.ReplaceOnAllMeshInstances:
                                    ReplaceOnAllMeshInstances();
                                    break;
                                case GameObjectMode.ReplaceWithinSceneRoot:
                                    ReplaceWithinSceneRoot();
                                    break;
                            }
                        }
                    }

                    evt.Use();

                    void AddPrefabsMenuItems(ref GenericMenu menu)
                    {
                        var prefabs = PrefabUtilities.GetPrefabsHierarchy(meshRenderer);
                        foreach (KeyValuePair<string, string> kvp in prefabs)
                        {
                            menu.AddItem(new GUIContent(string.Format("{0}{1}", "Assign to ", kvp.Key), "Apply override to Prefab"), false, () =>
                            {
                                MaterialUtilities.AssignMaterial(meshRenderer, submeshIndex, sourceMaterial);
                                PrefabUtilities.ApplyMaterialsOverrides(meshRenderer, kvp.Value);
                                return;
                            });
                        }
                    }

                    void AddGameObjectMenuItems(ref GenericMenu menu)
                    {
                        var meshName = meshRenderer.GetComponent<MeshFilter>().sharedMesh.name;

                        menu.AddItem(new GUIContent("Assign Material (Default)", ""), false, () =>
                        {
                            MaterialUtilities.AssignMaterial(meshRenderer, submeshIndex, sourceMaterial);
                        });

                        menu.AddItem(new GUIContent(string.Format("Assign {0} to all instances of {1} (ID:{2})", sourceMaterial.name, meshName, submeshIndex), ""), false, () =>
                        {
                            AssignToAllMeshInstances();
                        });

                        menu.AddItem(new GUIContent(string.Format("Replace {0} with {1} in all open scenes", targetMaterial.name, sourceMaterial.name), ""), false, () =>
                        {
                            ReplaceInAllOpenScenes();
                        });

                        menu.AddItem(new GUIContent(string.Format("Replace {0} with {1} on all instances of {2}", targetMaterial.name, sourceMaterial.name, meshName), ""), false, () =>
                        {
                            ReplaceOnAllMeshInstances();
                        });

                        menu.AddItem(new GUIContent(string.Format("Replace {0} with {1} within {2}", targetMaterial.name, sourceMaterial.name, meshRenderer.transform.root.name), ""), false, () =>
                        {
                            ReplaceWithinSceneRoot();
                        });
                    }

                    void AssignToAllMeshInstances()
                    {
                        var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                        //Debug.LogFormat("<color=cyan>Applying {1} on all instances of {0}</color>", meshFilter.sharedMesh.name, sourceMaterial.name);
                        if (meshFilter.sharedMesh.GetInstancedRenderers(out MeshRenderer[] meshRenderers))
                        {
                            int slot = Array.IndexOf(meshRenderer.sharedMaterials, targetMaterial);
                            foreach (MeshRenderer meshRenderer in meshRenderers)
                                MaterialUtilities.AssignMaterial(meshRenderer, slot, sourceMaterial);
                        }
                    }

                    void ReplaceInAllOpenScenes()
                    {
                        //Debug.LogFormat("<color=cyan>Replacing {1} with {0}</color>", sourceMaterial.name, targetMaterial.name);
                        if (targetMaterial.GetSameMaterialRenderers(out MeshRenderer[] meshRenderers))
                        {
                            foreach (MeshRenderer meshRenderer in meshRenderers)
                            {
                                int slot = Array.IndexOf(meshRenderer.sharedMaterials, targetMaterial);
                                MaterialUtilities.AssignMaterial(meshRenderer, slot, sourceMaterial);
                            }
                        }
                    }

                    // FIXME : does not work when called from within a Prefab (Stage Mode)
                    void ReplaceOnAllMeshInstances()
                    {
                        var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                        //Debug.LogFormat("<color=cyan>Replacing {1} with {0} on all instances of {2}</color>", sourceMaterial.name, targetMaterial.name, meshFilter.sharedMesh.name);
                        if (meshFilter.sharedMesh.GetInstancedRenderers(out MeshRenderer[] r1))
                        {
                            MaterialUtilities.ReplaceMaterial(r1, targetMaterial, sourceMaterial);
                        }
                    }

                    void ReplaceWithinSceneRoot()
                    {
                        var renderers = meshRenderer.transform.root.GetComponentsInChildren<MeshRenderer>();
                        MaterialUtilities.ReplaceMaterial(renderers, targetMaterial, sourceMaterial);
                    }
                }
            }
        }

        static async void PingObjectInProjectWindow(UnityEngine.Object obj)
        {
            MethodInfo hasOpenInstancesMethod = typeof(EditorWindow).GetMethod("HasOpenInstances",
                                BindingFlags.Public | BindingFlags.Static);
            hasOpenInstancesMethod = hasOpenInstancesMethod.MakeGenericMethod(projectBrowserType);

            //MethodInfo createInstanceMethod = typeof(EditorWindow).GetMethod("CreateWindow",
            //                    BindingFlags.Public | BindingFlags.Static);
            //createInstanceMethod = createInstanceMethod.MakeGenericMethod(projectBrowserType);

            if (!(bool)hasOpenInstancesMethod.Invoke(null, null))
            {
                //var projectWindow = (EditorWindow)createInstanceMethod.Invoke(null, null);
                var projectWindow = EditorWindow.GetWindow(projectBrowserType);
                projectWindow.Show();
                await Task.Delay(1000);
            }
            else
            {
                var projectWindow = EditorWindow.GetWindow(projectBrowserType);
                projectWindow.Show();
            }

            EditorGUIUtility.PingObject(obj);
        }
    }
}