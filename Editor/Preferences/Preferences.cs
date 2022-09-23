using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using static Unity.SceneDressingTools.Editor.SceneViewDragAndDropOverride;
using System.IO;

namespace Unity.SceneDressingTools.Editor
{
    internal static class Preferences
    {
        internal static event Action OnSettingsChanged;

        internal static bool EnableDragAndDropOverride
        {
            get => EditorPrefs.GetBool("SceneDressingTools/Materials/DragAndDrop/EnableOverride", false);
            set
            {
                if (EditorPrefs.GetBool("SceneDressingTools/Materials/DragAndDrop/EnableOverride", false) == value)
                    return;
                EditorPrefs.SetBool("SceneDressingTools/Materials/DragAndDrop/EnableOverride", value);
                OnSettingsChanged?.Invoke();
            }
        }

        internal static AssignmentMode AssignmentMode
        {
            get => (AssignmentMode)EditorPrefs.GetInt("SceneDressingTools/Materials/DragAndDrop/AssignmentMode", 0);
            set
            {
                if ((AssignmentMode)EditorPrefs.GetInt("SceneDressingTools/Materials/DragAndDrop/AssignmentMode", 0) == value)
                    return;
                EditorPrefs.SetInt("SceneDressingTools/Materials/DragAndDrop/AssignmentMode", (int)value);
                OnSettingsChanged?.Invoke();
            }
        }

        internal static GameObjectMode GameObjectMode
        {
            get => (GameObjectMode)EditorPrefs.GetInt("SceneDressingTools/Materials/DragAndDrop/GameObjectMode", 0);
            set
            {
                if ((GameObjectMode)EditorPrefs.GetInt("SceneDressingTools/Materials/DragAndDrop/GameObjectMode", 0) == value)
                    return;
                EditorPrefs.SetInt("SceneDressingTools/Materials/DragAndDrop/GameObjectMode", (int)value);
                OnSettingsChanged?.Invoke();
            }
        }

        internal static PrefabInstanceMode PrefabInstanceMode
        {
            get => (PrefabInstanceMode)EditorPrefs.GetInt("SceneDressingTools/Materials/DragAndDrop/PrefabInstanceMode", 0);
            set
            {
                if ((PrefabInstanceMode)EditorPrefs.GetInt("SceneDressingTools/Materials/DragAndDrop/PrefabInstanceMode", 0) == value)
                    return;
                EditorPrefs.SetInt("SceneDressingTools/Materials/DragAndDrop/PrefabInstanceMode", (int)value);
                OnSettingsChanged?.Invoke();
            }
        }

        internal static bool UseKeyboardModifiers
        {
            get => EditorPrefs.GetBool("SceneDressingTools/Materials/DragAndDrop/UseShortcuts", false);
            set
            {
                if (EditorPrefs.GetBool("SceneDressingTools/Materials/DragAndDrop/UseShortcuts", false) == value)
                    return;
                EditorPrefs.SetBool("SceneDressingTools/Materials/DragAndDrop/UseShortcuts", value);
                OnSettingsChanged?.Invoke();
            }
        }

        internal static bool AutoApplyOverrides
        {
            get => EditorPrefs.GetBool("SceneDressingTools/Materials/DragAndDrop/AutoApplyOverrides", false);
            set
            {
                if (EditorPrefs.GetBool("SceneDressingTools/Materials/DragAndDrop/AutoApplyOverrides", false) == value)
                    return;
                EditorPrefs.SetBool("SceneDressingTools/Materials/DragAndDrop/AutoApplyOverrides", value);
                OnSettingsChanged?.Invoke();
            }
        }

        internal static string AssetExtractionPath
        {
            get => EditorPrefs.GetString("SceneDressingTools/AssetExtractionPath", "SceneDressing/[modelName]/[assetType]");
            set
            {
                if (EditorPrefs.GetString("SceneDressingTools/AssetExtractionPath", "SceneDressing/[modelName]/[assetType]") == value)
                    return;
                EditorPrefs.SetString("SceneDressingTools/AssetExtractionPath", value);
                OnSettingsChanged?.Invoke();
            }
        }

        static readonly GUIContent AssetExtractionPathGuiContent = new GUIContent("Asset Extraction Path", "[modelName] = name of the gameObject\n[assetType] = \"Materials\" or \"Meshes\"");

        [SettingsProvider]
        internal static SettingsProvider CreateSettingsProvider()
        {
            var settingsProvider = new SettingsProvider("Scene Dressing/Materials", SettingsScope.User)
            {
                label = "Materials",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.LabelField("Drag 'n' Drop Behaviour", EditorStyles.boldLabel);

                    EnableDragAndDropOverride = EditorGUILayout.Toggle("Enabled Override", EnableDragAndDropOverride);

                    GUI.enabled = EnableDragAndDropOverride;
                    AssignmentMode = (AssignmentMode)EditorGUILayout.EnumPopup("Assignment Mode", AssignmentMode);
                    GUI.enabled = EnableDragAndDropOverride && AssignmentMode != AssignmentMode.CopyProperties;
                    GameObjectMode = (GameObjectMode)EditorGUILayout.EnumPopup("GameObject Mode", GameObjectMode);
                    PrefabInstanceMode = (PrefabInstanceMode)EditorGUILayout.EnumPopup("Prefab Instance Mode", PrefabInstanceMode);
                    GUI.enabled = EnableDragAndDropOverride;

                    //UseKeyboardModifiers = EditorGUILayout.Toggle("Use Keyboard Modifiers", UseKeyboardModifiers);
                    AutoApplyOverrides = EditorGUILayout.Toggle("Auto Apply Overrides", AutoApplyOverrides);

                    GUI.enabled = true;
                    AssetExtractionPath = EditorGUILayout.TextField(AssetExtractionPathGuiContent, AssetExtractionPath);
                    EditorGUILayout.LabelField("Preview: " + ParsePath(AssetExtractionPath));
                },

                keywords = new HashSet<string>(new[] { "Scene Dressing", "Material" })
            };

            return settingsProvider;
        }

        static string ParsePath(string path)
        {
            path = path.Replace("[modelName]", "myGameObject");
            path = path.Replace("[assetType]", "Materials");
            path = "Assets/" + path;
            return path;
        }
    }
}