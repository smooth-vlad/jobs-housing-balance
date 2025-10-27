using ColossalFramework.UI;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace JobsHousingBalance.UI
{
    public class IconButton : UIButton
    {
        private const int BUTTON_SIZE = 48;
        
        // Dragging state
        private bool isDragging = false;
        private Vector3 dragOffset = Vector3.zero;

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

                // Create circular background with custom color and border
                var view = UIView.GetAView();
                CreateCircularBackground();

                // Ensure fully visible and above other UI
                opacity = 1f;
                isVisible = true;
                isEnabled = true;
                isInteractive = true;
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
                        var iconSize = new Vector2(32, 32);
                        var icon = AddUIComponent<UITextureSprite>();
                        icon.texture = texture;
                        icon.size = iconSize;
                        // Center icon in the 48x48 button
                        icon.relativePosition = new Vector3((size.x - iconSize.x) / 2f, (size.y - iconSize.y) / 2f);
                        icon.zOrder = this.zOrder + 1; // Above background
                        icon.isInteractive = false;
                        icon.opacity = 1f;

                        Debug.Log("JobsHousingBalance: Icon UITextureSprite created; tex=" + (icon.texture != null ? (icon.texture.width + "x" + icon.texture.height) : "null") + ", size=" + icon.size + ", pos=" + icon.relativePosition);

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
        
        private void CreateCircularBackground()
        {
            try
            {
                // Create a circular texture with fill color and border
                var bgTexture = new Texture2D(BUTTON_SIZE, BUTTON_SIZE);
                var colors = new Color32[BUTTON_SIZE * BUTTON_SIZE];
                
                var center = BUTTON_SIZE / 2f;
                var radius = BUTTON_SIZE / 2f - 2f; // Leave space for border
                var borderWidth = 2f;
                
                // Background color: 53555B
                var fillColor = new Color32(0x53, 0x55, 0x5B, 230);
                // Border color: 508A33
                var borderColor = new Color32(0x50, 0x8A, 0x33, 255);
                
                for (int y = 0; y < BUTTON_SIZE; y++)
                {
                    for (int x = 0; x < BUTTON_SIZE; x++)
                    {
                        var dx = x - center;
                        var dy = y - center;
                        var dist = Mathf.Sqrt(dx * dx + dy * dy);
                        
                        int index = y * BUTTON_SIZE + x;
                        
                        if (dist > radius + borderWidth)
                        {
                            // Outside - transparent
                            colors[index] = new Color32(0, 0, 0, 0);
                        }
                        else if (dist > radius)
                        {
                            // Border area
                            colors[index] = borderColor;
                        }
                        else
                        {
                            // Inside - fill color
                            colors[index] = fillColor;
                        }
                    }
                }
                
                bgTexture.SetPixels32(colors);
                bgTexture.Apply();
                bgTexture.wrapMode = TextureWrapMode.Clamp;
                bgTexture.filterMode = FilterMode.Bilinear;
                
                // Create atlas for circular background
                var bgAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
                var bgMaterial = new Material(Shader.Find("UI/Default"));
                bgMaterial.mainTexture = bgTexture;
                bgAtlas.material = bgMaterial;
                bgAtlas.name = "JobsHousingBalanceCircularBgAtlas";

                var bgSpriteInfo = new UITextureAtlas.SpriteInfo
                {
                    name = "CircularBg",
                    texture = bgTexture,
                    region = new Rect(0, 0, 1, 1)
                };
                bgAtlas.AddSprite(bgSpriteInfo);

                // Render background as UITextureSprite child (same approach as icon)
                var bgSprite = AddUIComponent<UITextureSprite>();
                bgSprite.texture = bgTexture;
                bgSprite.size = size;
                bgSprite.relativePosition = Vector3.zero;
                bgSprite.zOrder = this.zOrder; // Behind icon
                bgSprite.isInteractive = false;
                
                // No sprite needed for button itself since we use child sprites
                
                Debug.Log("JobsHousingBalance: Circular background created as UITextureSprite; size=" + bgSprite.size);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JobsHousingBalance: Exception creating circular background: " + ex.Message);
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

        // Mouse event handlers for dragging
        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            base.OnMouseDown(p);
            
            if (p.buttons == UIMouseButton.Left)
            {
                isDragging = true;
                // Get mouse position in UI space (relative to parent/UIView)
                Vector2 mouseInUISpace = GetUIView().ScreenPointToGUI(p.position);
                dragOffset = new Vector3(mouseInUISpace.x - relativePosition.x, mouseInUISpace.y - relativePosition.y, 0);
                Debug.Log($"JobsHousingBalance: Start drag - p.pos: ({p.position.x:F1}, {p.position.y:F1}), mouseUI: ({mouseInUISpace.x:F1}, {mouseInUISpace.y:F1}), relPos: ({relativePosition.x:F1}, {relativePosition.y:F1}), offset: ({dragOffset.x:F1}, {dragOffset.y:F1})");
            }
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            base.OnMouseMove(p);
            
            if (isDragging && p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                // Convert mouse screen position to UI space
                Vector2 mouseInUISpace = GetUIView().ScreenPointToGUI(p.position);
                
                // Calculate new relative position
                Vector3 newPosition = new Vector3(
                    mouseInUISpace.x - dragOffset.x,
                    mouseInUISpace.y - dragOffset.y,
                    0
                );
                
                // Constrain to screen bounds
                var view = UIView.GetAView();
                if (view != null)
                {
                    newPosition.x = Mathf.Clamp(newPosition.x, 0, view.fixedWidth - size.x);
                    newPosition.y = Mathf.Clamp(newPosition.y, 0, view.fixedHeight - size.y);
                }
                
                // Update relative position
                relativePosition = newPosition;
            }
        }

        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            base.OnMouseUp(p);
            
            if (isDragging)
            {
                isDragging = false;
                Debug.Log("JobsHousingBalance: Drag ended, final position: " + absolutePosition);
            }
        }
    }
}
