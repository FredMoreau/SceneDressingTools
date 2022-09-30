using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Unity.SceneDressingTools.Editor
{
    public static class HierarchyDragAndDropOverrride
    {
        internal static bool isOn;
        internal static bool IsOn
        {
            get => Preferences.EnableDragAndDropOverride;
            set
            {
                if (value == isOn)
                    return;
                isOn = value;
                if (isOn)
                    DragAndDrop.AddDropHandler(HierarchyDropHandler);
                else
                    DragAndDrop.RemoveDropHandler(HierarchyDropHandler);
            }
        }

        [InitializeOnLoadMethod]
        static void Init()
        {
            isOn = Preferences.EnableDragAndDropOverride;
            if (isOn)
                DragAndDrop.AddDropHandler(HierarchyDropHandler);

            Preferences.OnSettingsChanged += Preferences_OnSettingsChanged;
        }

        private static void Preferences_OnSettingsChanged()
        {
            IsOn = Preferences.EnableDragAndDropOverride;
        }

        static DragAndDropVisualMode HierarchyDropHandler(int id, HierarchyDropFlags mode, Transform parentForDraggedObjects, bool perform)
        {
            var selectedMeshRenderers = Selection.GetFiltered<MeshRenderer>(SelectionMode.Editable);
            var selectedMeshRenderersDeep = Selection.GetFiltered<MeshRenderer>(SelectionMode.Deep | SelectionMode.Editable);

            DragAndDrop.AcceptDrag();
            Material sourceMaterial = DragAndDrop.objectReferences.FirstOrDefault(x => x is Material) as Material;

            if (sourceMaterial == null) // if no material is being dragged, let Unity do its thing
                return DragAndDropVisualMode.None;

            var gameObject = (GameObject)EditorUtility.InstanceIDToObject(id);

            if (mode != HierarchyDropFlags.DropUpon || gameObject == null)
                return DragAndDropVisualMode.Rejected;

            var droppedOnMeshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (perform)
            {
                var menu = new GenericMenu();
                if (selectedMeshRenderers.Length > 0)
                {
                    menu.AddItem(new GUIContent($"Assign Material to selection ({selectedMeshRenderers.Length})", ""), false, () =>
                    {
                        MaterialUtilities.AssignMaterialToAllRenderersAllIDs(selectedMeshRenderers, sourceMaterial);
                    });
                }

                if (selectedMeshRenderersDeep.Length > 0)
                {
                    menu.AddItem(new GUIContent($"Assign Material to deep selection ({selectedMeshRenderersDeep.Length})", ""), false, () =>
                    {
                        MaterialUtilities.AssignMaterialToAllRenderersAllIDs(selectedMeshRenderersDeep, sourceMaterial);
                    });
                }

                // shortcut: no menu when we dropped on a parent with no MeshRenderer and selection is empty
                if (gameObject.transform.childCount > 0 && menu.GetItemCount() == 0 && droppedOnMeshRenderer == null)
                {
                    var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
                    MaterialUtilities.AssignMaterialToAllRenderersAllIDs(renderers, sourceMaterial);
                    return DragAndDropVisualMode.Generic;
                }

                if (gameObject.transform.childCount > 0)
                {
                    var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
                    if (renderers.Length > 0)
                    {
                        menu.AddItem(new GUIContent($"Assign Material to {gameObject.name} hierarchy ({renderers.Length})", ""), false, () =>
                        {
                            MaterialUtilities.AssignMaterialToAllRenderersAllIDs(renderers, sourceMaterial);
                        });
                    }
                }

                // shortcut: still no menu and there is a mesh renderer on dropped object, let Unity handle it
                if (menu.GetItemCount() == 0 && droppedOnMeshRenderer != null)
                {
                    return DragAndDropVisualMode.None;
                }

                if (droppedOnMeshRenderer != null)
                {
                    menu.AddItem(new GUIContent($"Assign Material to {droppedOnMeshRenderer.name}", ""), false, () =>
                    {
                        MaterialUtilities.AssignMaterialToAllRenderersAllIDs(new MeshRenderer[] { droppedOnMeshRenderer }, sourceMaterial);
                    });
                }
                
                if (menu.GetItemCount() > 0)
                {
                    menu.ShowAsContext();
                    return DragAndDropVisualMode.Generic;
                }
                else
                {
                    return DragAndDropVisualMode.None;
                }
            }
            else
            {
                return DragAndDropVisualMode.Generic;
            }
        }
    }
}