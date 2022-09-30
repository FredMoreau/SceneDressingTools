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
    [Overlay(typeof(SceneView), "Material Clipboard")]
    public class SceneViewMaterialClipboard : Overlay, ITransientOverlay
    {
        public bool visible => Preferences.EnableDragAndDropOverride && SceneViewDragAndDropOverride.MaterialClipboard != null;

        readonly Color bg = new Color(0, 0, 0, 0.4f);

        VisualElement root;
        static Texture2D thumbnail;
        Image materialClipboardThumbnail;
        Label materialClipboardLabel;

        public override void OnCreated()
        {
            base.OnCreated();
            SceneViewDragAndDropOverride.OnMaterialClipboardChanged += SceneViewDragAndDropOverride_OnMaterialClipboardChanged;
        }

        private async void SceneViewDragAndDropOverride_OnMaterialClipboardChanged(Material material)
        {
            if (root == null)
                await Task.Delay(1000);

            materialClipboardLabel.text = material.name;
            if (AssetPreview.GetAssetPreview(material) == null)
            {
                //Task<Texture2D> thumbnailLoading = GetMaterialThumbnail(material);
                //thumbnail = await thumbnailLoading;
                // Replacing with a more brute force approach
                materialClipboardThumbnail.image = null;
                do
                {
                    thumbnail = AssetPreview.GetAssetPreview(material);
                    await Task.Delay(500);
                }
                while (thumbnail == null);

                await Task.Delay(3000);
                materialClipboardThumbnail.image = thumbnail;
            }
            else
            {
                thumbnail = AssetPreview.GetAssetPreview(material);
                materialClipboardThumbnail.image = thumbnail;
            }
        }

        async Task<Texture2D> GetMaterialThumbnail(Material material)
        {
            var id = material.GetInstanceID();
            while (AssetPreview.IsLoadingAssetPreview(id))
            {
                await Task.Delay(1000);
            }
            return AssetPreview.GetAssetPreview(material);
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            SceneViewDragAndDropOverride.OnMaterialClipboardChanged -= SceneViewDragAndDropOverride_OnMaterialClipboardChanged;
        }

        public override VisualElement CreatePanelContent()
        {
            if (root is null)
            {
                root = new VisualElement()
                {
                
                };

                materialClipboardThumbnail = new Image()
                {
                    image = thumbnail,
                    tooltip = "Hold Control + Shift, then middle click on an object to assign."
                };
                //root.Add(materialClipboardThumbnail);

                var editBtn = new Button(() =>
                {
                    EditorUtility.OpenPropertyEditor(SceneViewDragAndDropOverride.MaterialClipboard);
                })
                {
                    text = "fetching preview..."
                };
                editBtn.Add(materialClipboardThumbnail);
                root.Add(editBtn);

                materialClipboardLabel = new Label()
                {
                    
                };
                //materialClipboardLabel.style.fontSize = 24;
                materialClipboardLabel.style.flexGrow = 1;
                materialClipboardLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                root.Add(materialClipboardLabel);
            }

            return root;
        }
    }
}
#endif