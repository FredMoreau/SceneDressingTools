#if UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Toolbars;
using static Unity.SceneDressingTools.Editor.SceneViewDragAndDropOverride;

namespace Unity.SceneDressingTools.Editor
{
    [Overlay(typeof(SceneView), "Scene Dressing")]
    //[Icon("Packages/com.unity.scene-dressing-tools/Editor/Icons/SceneViewTools@2x.png")]
    public class SceneViewToolbar : ToolbarOverlay
    {
        public static bool isDisplayed;

        SceneViewToolbar() : base(
            OverrideToggle.id,
            MaterialDragAndDropBehaviourToolbar.id,
            MaterialsOptionsDropDown.id)
        {}

        public override void OnCreated()
        {
            base.OnCreated();
            Preferences.OnSettingsChanged += UpdateToolbar;
            layoutChanged += SceneViewToolbar_layoutChanged;
            floatingChanged += SceneViewToolbar_floatingChanged;
            //SceneViewPrompt.OnToggle += SceneViewPrompt_OnToggle;
            displayedChanged += SceneViewToolbar_displayedChanged;
        }

        private void SceneViewToolbar_displayedChanged(bool state)
        {
            isDisplayed = state;
        }

        //private void SceneViewPrompt_OnToggle()
        //{
        //    displayed ^= true;
        //}

        private void SceneViewToolbar_floatingChanged(bool isFloating)
        {
            if (!isInToolbar)
                collapsed = false;
        }

        private void SceneViewToolbar_layoutChanged(Layout layout)
        {
            if (layout == Layout.VerticalToolbar)
                collapsed = true;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            Preferences.OnSettingsChanged -= UpdateToolbar;
            layoutChanged -= SceneViewToolbar_layoutChanged;
            floatingChanged -= SceneViewToolbar_floatingChanged;
        }

        void UpdateToolbar()
        {
            // FIXME : Toolbar doesn't update when docked
            //Debug.Log(root?.name);
            //Debug.Log(root.Q<EditorToolbarToggle>("overrideToggle"));

            Debug.Log(assignmentModeField);

            if (root is null)
            {
                var lvl = containerWindow.rootVisualElement;
                while (lvl.parent != null)
                {
                    //Debug.Log(lvl.name);
                    lvl = lvl.parent;
                }
                root = lvl;
            }

            //Debug.Log(root.name);
            //Debug.Log(root.Q<EditorToolbarToggle>("overrideToggle"));
            //Debug.Log(root.Q<EnumField>("assignmentMode"));

            root.Q<EditorToolbarToggle>("overrideToggle")?.SetValueWithoutNotify(Preferences.EnableDragAndDropOverride);

            assignmentModeField?.SetValueWithoutNotify(Preferences.AssignmentMode);
            assignmentModeField?.SetEnabled(Preferences.EnableDragAndDropOverride);

            //root.Q<EnumField>("assignmentMode")?.SetValueWithoutNotify(Preferences.AssignmentMode);
            root.Q<EnumField>("gameObjectMode")?.SetValueWithoutNotify(Preferences.GameObjectMode);
            root.Q<EnumField>("prefabInstanceMode")?.SetValueWithoutNotify(Preferences.PrefabInstanceMode);

            //root.Q<EnumField>("assignmentMode")?.SetEnabled(Preferences.EnableDragAndDropOverride);
            root.Q<EnumField>("gameObjectMode")?.SetEnabled(Preferences.EnableDragAndDropOverride && Preferences.AssignmentMode != AssignmentMode.CopyProperties);
            root.Q<EnumField>("prefabInstanceMode")?.SetEnabled(Preferences.EnableDragAndDropOverride && Preferences.AssignmentMode != AssignmentMode.CopyProperties);
        }

        VisualElement root;

        EnumField assignmentModeField;

        public override VisualElement CreatePanelContent()
        {
            //if (!collapsed)
            //{
            root = base.CreatePanelContent();
            //}
            //else // if collapsed bring a full blown window
            //{
            //    root = new VisualElement();
            //}

            assignmentModeField = root.Q<EnumField>("assignmentMode");
            Debug.Log(assignmentModeField);

            return root;
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class OverrideToggle : EditorToolbarToggle/*, IAccessContainerWindow*/
    {
        public const string id = "Materials/DragAndDropOverride";

        //public EditorWindow containerWindow { get; set; }

        public OverrideToggle()
        {
            name = "overrideToggle";
            icon = (Texture2D)EditorGUIUtility.IconContent("d_AnimatorOverrideController Icon").image;
            tooltip = "Enable Drag 'n' Drop Overrides";
            value = Preferences.EnableDragAndDropOverride;

            RegisterCallback<ChangeEvent<bool>>(e => Preferences.EnableDragAndDropOverride = e.newValue);
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class MaterialDragAndDropBehaviourToolbar : VisualElement/*, IAccessContainerWindow*/
    {
        public const string id = "Materials/DragAndDropBehaviour";

        //public EditorWindow containerWindow { get; set; }

        MaterialDragAndDropBehaviourToolbar()
        {
            tooltip = "";

            Image assignmentModeIcon = new Image();
            assignmentModeIcon.image = EditorGUIUtility.IconContent("d_TreeEditor.Duplicate").image;
            assignmentModeIcon.AddToClassList("unity-image");
            assignmentModeIcon.AddToClassList("unity-editor-toolbar-element__icon");
            Add(assignmentModeIcon);

            var assignmentMode = new EnumField("", Preferences.AssignmentMode)
            {
                name = "assignmentMode",
                tooltip = "Assignment Mode"
            };
            assignmentMode.RegisterCallback<ChangeEvent<string>>((e) => Preferences.AssignmentMode = (AssignmentMode)assignmentMode.value);
            Add(assignmentMode);

            Image gameObjectModeIcon = new Image();
            gameObjectModeIcon.image = EditorGUIUtility.IconContent("d_Material Icon").image;
            gameObjectModeIcon.AddToClassList("unity-image");
            gameObjectModeIcon.AddToClassList("unity-editor-toolbar-element__icon");
            Add(gameObjectModeIcon);

            var gameObjectMode = new EnumField("", Preferences.GameObjectMode)
            {
                name = "gameObjectMode",
                tooltip = "GameObject Mode"
            };
            gameObjectMode.RegisterCallback<ChangeEvent<string>>((e) => Preferences.GameObjectMode = (GameObjectMode)gameObjectMode.value);
            Add(gameObjectMode);

            Image prefabInstanceModeIcon = new Image();
            prefabInstanceModeIcon.image = EditorGUIUtility.IconContent("d_Prefab Icon").image;
            prefabInstanceModeIcon.AddToClassList("unity-image");
            prefabInstanceModeIcon.AddToClassList("unity-editor-toolbar-element__icon");
            Add(prefabInstanceModeIcon);

            var prefabInstanceMode = new EnumField("", Preferences.PrefabInstanceMode)
            {
                name = "prefabInstanceMode",
                tooltip = "Prefab Instance Mode"
            };
            prefabInstanceMode.RegisterCallback<ChangeEvent<string>>((e) => Preferences.PrefabInstanceMode = (PrefabInstanceMode)prefabInstanceMode.value);
            Add(prefabInstanceMode);
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    class MaterialsOptionsDropDown : VisualElement, IAccessContainerWindow
    {
        public const string id = "Materials/Options";

        public EditorWindow containerWindow { get; set; }

        MaterialsOptionsDropDown()
        {
            tooltip = "";
            Add(OverlayHelper.OptionsDropDown(
                EditorGUIUtility.IconContent("d_SettingsIcon@2x").image,
                "",
                OnClick));
        }

        void OnClick(MouseDownEvent evt)
        {
            OverlayHelper.MaterialsOptionsMenu(containerWindow as SceneView).DropDown(worldBound);
        }
    }

    internal static class OverlayHelper
    {
        internal static GenericMenu MaterialsOptionsMenu(SceneView sceneview)
        {
            GenericMenu menu = new GenericMenu();
            //menu.AddItem(new GUIContent("Use Keyboard Modifiers"), Preferences.UseKeyboardModifiers, () => Preferences.UseKeyboardModifiers ^= true);
            menu.AddItem(new GUIContent("Auto Apply Overrides"), Preferences.AutoApplyOverrides, () => Preferences.AutoApplyOverrides ^= true);
            menu.AddItem(new GUIContent("Preferences"), false, () => SettingsService.OpenUserPreferences("Scene Dressing/Materials"));
            return menu;
        }

        internal static VisualElement OptionsDropDown(Texture image, string tooltip, EventCallback<MouseDownEvent> mouseDownEvent)
        {
            VisualElement root = new VisualElement();

            VisualElement blk = new VisualElement() { tooltip = tooltip };
            blk.AddToClassList("unity-text-element");
            blk.AddToClassList("unity-toolbar-button");
            blk.AddToClassList("unity-editor-toolbar-element");

            Image icon = new Image();
            icon.image = image;
            icon.AddToClassList("unity-image");
            icon.AddToClassList("unity-editor-toolbar-element__icon");
            blk.Add(icon);

            VisualElement arrow = new VisualElement();
            arrow.AddToClassList("unity-icon-arrow");
            blk.Add(arrow);

            blk.RegisterCallback(mouseDownEvent);

            root.Add(blk);

            return root;
        }

        internal static VisualElement StandardDropdown(string title, Texture image, string tooltip, EventCallback<MouseDownEvent> mouseDownEvent = null)
        {
            VisualElement root = new VisualElement();

            VisualElement blk = new VisualElement() { tooltip = tooltip };
            blk.AddToClassList("unity-text-element");
            blk.AddToClassList("unity-toolbar-button");
            blk.AddToClassList("unity-editor-toolbar-element");

            Image icon = new Image();
            icon.image = image;
            icon.AddToClassList("unity-image");
            icon.AddToClassList("unity-editor-toolbar-element__icon");
            blk.Add(icon);

            TextElement textElement = new TextElement() { text = title };
            textElement.AddToClassList("unity-text-element");
            textElement.AddToClassList("unity-editor-toolbar-element__label");
            blk.Add(textElement);

            VisualElement arrow = new VisualElement();
            arrow.AddToClassList("unity-icon-arrow");
            blk.Add(arrow);

            blk.RegisterCallback(mouseDownEvent);

            root.Add(blk);

            return root;
        }
    }
}
#endif