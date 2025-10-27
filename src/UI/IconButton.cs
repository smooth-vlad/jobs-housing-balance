using ColossalFramework.UI;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace JobsHousingBalance.UI
{
    public class IconButton : UIButton
    {
        private const int BUTTON_SIZE = 48;

        public static IconButton Create()
        {
            try
            {
                var uiView = UIView.GetAView();
                if (uiView == null)
                {
                    Debug.LogError("JobsHousingBalance: Failed to get UIView");
                    return null;
                }

                var button = uiView.AddUIComponent(typeof(IconButton)) as IconButton;
                if (button == null)
                {
                    Debug.LogError("JobsHousingBalance: Failed to create IconButton");
                    return null;
                }

                return button;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JobsHousingBalance: Exception in IconButton.Create: " + ex.Message);
                return null;
            }
        }

        public override void Start()
        {
            base.Start();

            try
            {
                // Set basic properties
                size = new Vector2(BUTTON_SIZE, BUTTON_SIZE);
                name = "JobsHousingBalanceIconButton";
                tooltip = "Jobs-Housing Balance";

                // Use game's default atlas for a visible background
                var view = UIView.GetAView();
                atlas = view.defaultAtlas;
                normalBgSprite = "ButtonMenu";
                hoveredBgSprite = "ButtonMenuFocused";
                pressedBgSprite = "ButtonMenuPressed";

                // Ensure fully visible and above other UI
                color = Color.white;
                opacity = 1f;
                isVisible = true;
                isEnabled = true;
                zOrder = 10000;
                clipChildren = false;

                // Position - adaptive: top right corner with padding
                // Calculate position from screen width, accounting for button size
                var padding = 300f;
                var x = view.fixedWidth - size.x - padding;
                var y = 10f;
                absolutePosition = new Vector3(x, y);

                // Log position for debugging
                Debug.Log("JobsHousingBalance: Button position: " + absolutePosition);
                Debug.Log("JobsHousingBalance: Screen size: " + view.fixedWidth + "x" + view.fixedHeight);
                Debug.Log("JobsHousingBalance: Button size: " + size);

                // Load icon from embedded resources
                LoadIconFromResources();

                Debug.Log("JobsHousingBalance: Icon button initialized");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JobsHousingBalance: Exception in IconButton.Start: " + ex.Message);
            }
        }

        private void LoadIconFromResources()
        {
            try
            {
                // Background sprites already set in Start()
                
                // Try to load icon from embedded resources
                string resourceName = "JobsHousingBalance.Resources.icons8-demand-48.png";
                Assembly assembly = Assembly.GetExecutingAssembly();
                
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        Texture2D texture = new Texture2D(2, 2);
                        texture.LoadImage(buffer);
                        texture.wrapMode = TextureWrapMode.Clamp;
                        texture.filterMode = FilterMode.Bilinear;
                        Debug.Log("JobsHousingBalance: Texture loaded, size: " + texture.width + "x" + texture.height);

                        // Render icon using UITextureSprite (no atlas) to avoid packing/render issues
                        var icon = AddUIComponent<UITextureSprite>();
                        icon.texture = texture;
                        icon.size = size;
                        icon.relativePosition = Vector3.zero;
                        icon.zOrder = this.zOrder + 1;
                        icon.isInteractive = false;
                        icon.opacity = 1f;

                        Debug.Log("JobsHousingBalance: UITextureSprite created; tex=" + (icon.texture != null ? (icon.texture.width + "x" + icon.texture.height) : "null") + ", size=" + icon.size + ", zOrder=" + icon.zOrder);

                        Debug.Log("JobsHousingBalance: Icon loaded from embedded resources");
                        return;
                    }
                    else
                    {
                        Debug.LogWarning("JobsHousingBalance: Could not find embedded resource: " + resourceName);
                    }
                }

                // Fallback: add text label
                AddFallbackLabel();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JobsHousingBalance: Exception loading icon from resources: " + ex.Message);
                AddFallbackLabel();
            }
        }
        
        private void AddTestBackground() { }

        private void AddFallbackLabel()
        {
            try
            {
                var label = AddUIComponent<UILabel>();
                label.name = "IconLabel";
                label.text = "JH";
                label.textAlignment = UIHorizontalAlignment.Center;
                label.verticalAlignment = UIVerticalAlignment.Middle;
                label.size = size;
                label.relativePosition = Vector3.zero;
                label.textScale = 1.2f;
                label.textColor = Color.white;

                Debug.Log("JobsHousingBalance: Fallback label added");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JobsHousingBalance: Exception adding fallback label: " + ex.Message);
            }
        }
    }
}
