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
            DragAndDrop.AcceptDrag();
            Material sourceMaterial = DragAndDrop.objectReferences.FirstOrDefault(x => x is Material) as Material;

            if (sourceMaterial == null) // if no material is being dragged, let Unity do its thing
                return DragAndDropVisualMode.None;

            var gameObject = (GameObject)EditorUtility.InstanceIDToObject(id);

            if (mode != HierarchyDropFlags.DropUpon || gameObject == null)
                return DragAndDropVisualMode.Rejected;

            if (perform)
            {
                if (gameObject.transform.childCount == 0) // if no child let Unity do its thing
                {
                    return DragAndDropVisualMode.None;
                }
                else
                {
                    var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
                    MaterialUtilities.AssignMaterialToAllRenderersAllIDs(renderers, sourceMaterial);
                }
                return DragAndDropVisualMode.Generic;
            }
            else
            {
                return DragAndDropVisualMode.Generic;
            }
        }
    }
}