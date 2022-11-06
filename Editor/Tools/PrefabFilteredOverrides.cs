using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.SceneDressingTools.Editor
{
    public static class PrefabFilteredOverrides
    {
        [MenuItem("Tools/Scene Dressing/Revert Selected Material Overrides")]
        public static void RevertSelectionMaterials()
        {
            var selection = Selection.GetFiltered<MeshRenderer>(SelectionMode.Deep | SelectionMode.Editable);

            var undoLvl = Undo.GetCurrentGroup();

            foreach (var meshRenderer in selection)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(meshRenderer))
                {
                    var serializedObject = new SerializedObject(meshRenderer);
                    SerializedProperty serializedProperty = serializedObject.FindProperty("m_Materials");

                    if (serializedProperty == null)
                        continue;

                    Undo.RecordObject(meshRenderer, "");
                    PrefabUtility.RevertPropertyOverride(serializedProperty, InteractionMode.AutomatedAction);
                }
            }

            Undo.SetCurrentGroupName("Revert Selection Material Overrides");
            Undo.CollapseUndoOperations(undoLvl);
        }

        [MenuItem("Tools/Scene Dressing/Apply Selected Material Overrides")]
        public static void ApplySelectionMaterials()
        {
            var selection = Selection.GetFiltered<MeshRenderer>(SelectionMode.Deep | SelectionMode.Editable);

            var undoLvl = Undo.GetCurrentGroup();

            foreach (var meshRenderer in selection)
            {
                if (PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(meshRenderer))
                {
                    var serializedObject = new SerializedObject(meshRenderer);
                    SerializedProperty serializedProperty = serializedObject.FindProperty("m_Materials");

                    if (serializedProperty == null)
                        continue;

                    Undo.RecordObject(meshRenderer, "");

                    if (serializedProperty.arraySize > 0)
                    {
                        var originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(meshRenderer);
                        var leafPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(originalSource);

                        for (var i = 0; i < serializedProperty.arraySize; i++)
                        {
                            var element = serializedProperty.GetArrayElementAtIndex(i);
                            PrefabUtility.ApplyPropertyOverride(element, leafPath, InteractionMode.AutomatedAction);
                        }
                    }
                }
            }

            Undo.SetCurrentGroupName("Apply Selection Material Overrides");
            Undo.CollapseUndoOperations(undoLvl);
        }
    }
}