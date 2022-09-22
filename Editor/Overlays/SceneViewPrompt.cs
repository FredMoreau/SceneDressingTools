#if UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace Unity.SceneDressingTools.Editor
{
    [Overlay(typeof(SceneView), "Scene Dressing Prompt")]
    public class SceneViewPrompt : Overlay, ITransientOverlay
    {
        public static event System.Action OnToggle;

        public bool visible => Selection.GetFiltered<Material>(SelectionMode.Assets).Length > 0;

        public override void OnCreated()
        {
            base.OnCreated();
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();

            string prompt = Preferences.UseKeyboardModifiers ?
                "Use popup menu to Automatically Apply overrides to Prefabs.\n" +
                        "Hold <b>Control</b> to replace material in the whole scene.\n" +
                        "Hold <b>Alt</b> to propagate material assignments to all objects using the same Mesh.\n" +
                        "Hold <b>Alt + Control</b> to propagate to objects using the same Mesh and same Material.\n" +
                        //"Hold <b>Shift</b> to Automatically Apply overrides to Leaf Prefabs.\n" +
                        "Hold <b>Shift only</b> to copy/paste material properties.\n" :
                        "Hold <b>Ctrl (Cmd)</b> for more options when assigning a <i>Material</i> in <i>Scene View</i>.";

            var label = new Label("Scene Dressing allows overriding the material assignement Drag and Drop behaviour.")
            {

            };
            root.Add(label);

            var button = new Button(() =>
            {
                OnToggle?.Invoke();
                root.Q<Button>().text = SceneViewToolbar.isDisplayed ? "Close" : "Open";
            })
            {
                text = SceneViewToolbar.isDisplayed ? "Close" : "Open"
            };
            root.Add(button);

            return root;
        }
    }
}
#endif