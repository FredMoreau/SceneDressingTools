#if UNITY_2021_2_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using System.Threading.Tasks;

namespace Unity.SceneDressingTools.Editor
{
    [Overlay(typeof(SceneView), "Scene Dressing Prompt")]
    public class SceneViewPrompt : Overlay, ITransientOverlay
    {
        //public static event System.Action OnToggle;

        //public bool visible => Selection.GetFiltered<Material>(SelectionMode.Assets).Length > 0;
        public bool visible => Preferences.EnableDragAndDropOverride;

        //readonly Color bg = new Color(0, 0, 0, 0.4f);

        VisualElement root;

        public override void OnCreated()
        {
            base.OnCreated();
            //var root = containerWindow.rootVisualElement;
            //var parent = root.parent;
            //var e = parent.Q<VisualElement>("Scene Dressing Prompt");
            //root.Q<VisualElement>("unity-overlay").style.backgroundColor = bg;

            //root.style.backgroundColor = Color.clear;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
        }

        public override VisualElement CreatePanelContent()
        {
            if (root is null)
            {
                root = new VisualElement()
                {
                
                };
                root.style.flexDirection = FlexDirection.Row;

                // TODO : custom background color 

                //root.style.backgroundColor = Color.clear;

                string prompt = "<b>Scene Dressing Override is On</b>.\n" +
                            "<b>Hover</b> for more info.";

                string tooltip = "Hold Control to Assign Original to instances.\n" +
                            "Hold Alt to Replace all with Original.\n" +
                            "Hold Shift to Copy Properties\n" +
                            "Hold Alt + Control to Replace with Orignal on all Instances.\n" +
                            "Hold Control + Shift, then right click on an object to copy its Material to Clipboard.\n" +
                            "Read documentation for more shortcuts.";

                var label = new Label(prompt)
                {
                    tooltip = tooltip
                };
                root.Add(label);

                //var button = new Button(() =>
                //{
                //    OnToggle?.Invoke();
                //    root.Q<Button>().text = SceneViewToolbar.isDisplayed ? "Close" : "Open";
                //})
                //{
                //    text = SceneViewToolbar.isDisplayed ? "Close" : "Open"
                //};
                //root.Add(button);
            }

            return root;
        }
    }
}
#endif