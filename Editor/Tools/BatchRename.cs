using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.SceneDressingTools.Editor
{
    public static class BatchRename
    {
        //[MenuItem("GameObject/Batch Rename"), MenuItem("Assets/Batch Rename")]
        [MenuItem("Tools/Scene Dressing/Batch Rename")]
        public static void RenameSelection()
        {
            var selection = Selection.objects;
            if (selection.Length == 0)
                return;

            var baseName = selection[0].name;
            var names = new List<string>() { baseName };

            var undoLvl = Undo.GetCurrentGroup();

            for (int i = 0; i < selection.Length; i++)
            {
                Undo.RecordObject(selection[i], "");
                var name = ObjectNames.GetUniqueName(names.ToArray(), baseName);
                selection[i].name = name;
                names.Add(name);
            }

            Undo.SetCurrentGroupName("Batch Rename");
            Undo.CollapseUndoOperations(undoLvl);
        }
    }
}