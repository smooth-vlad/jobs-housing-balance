using ColossalFramework.UI;
using UnityEngine;

namespace JobsHousingBalance.UI.Panel
{
    public class MainPanel : UIPanel
    {
        private UILabel _title;
        private UIButton _close;
        private UIPanel _content;

        public static MainPanel Create()
        {
            try
            {
                var uiView = UIView.GetAView();
                if (uiView == null)
                {
                    Debug.LogError("JobsHousingBalance: Failed to get UIView for MainPanel");
                    return null;
                }

                var panel = uiView.AddUIComponent(typeof(MainPanel)) as MainPanel;
                if (panel == null)
                {
                    Debug.LogError("JobsHousingBalance: Failed to create MainPanel");
                    return null;
                }

                Debug.Log("JobsHousingBalance: MainPanel created successfully");
                return panel;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JobsHousingBalance: Exception in MainPanel.Create: " + ex.Message);
                return null;
            }
        }

        public override void Start()
        {
            base.Start();

            try
            {
                // Panel size and background
                size = new Vector2(350f, 400f);
                backgroundSprite = "GenericPanel";                 // Built-in default sprite
                color = new Color32(0, 0, 0, 160);                 // Semi-transparent background
                name = "JobsHousingBalanceMainPanel";

                // Title label
                _title = AddUIComponent<UILabel>();
                _title.text = "Jobs-Housing Balance";
                _title.textScale = 1.1f;
                _title.autoSize = false;
                _title.width = width - 60f;
                _title.height = 24f;
                _title.relativePosition = new Vector2(12f, 8f);
                _title.textAlignment = UIHorizontalAlignment.Left;
                _title.textColor = Color.white;

                // Close button (X)
                _close = AddUIComponent<UIButton>();
                _close.text = "×";                                 // Text is most reliable and fast
                _close.normalBgSprite = "ButtonMenu";              // Can use other sprites from sprite list
                _close.hoveredBgSprite = "ButtonMenuHovered";
                _close.pressedBgSprite = "ButtonMenuPressed";
                _close.size = new Vector2(28f, 24f);
                _close.relativePosition = new Vector2(width - _close.width - 8f, 6f);
                _close.name = "CloseButton";
                _close.isInteractive = true;
                _close.isEnabled = true;
                _close.zOrder = 1000;                              // Ensure it's above other elements
                _close.eventClick += OnCloseButtonClicked;         // Use proper event handler

                // Center panel (can be replaced with custom positioning)
                CenterToParent();

                // (Optional) Drag panel by top "header" area
                var drag = AddUIComponent<UIDragHandle>();
                drag.target = this;
                drag.relativePosition = Vector2.zero;
                drag.size = new Vector2(width - _close.width - 16f, 32f);  // Exclude close button area
                drag.zOrder = 100;                               // Below close button

                // Content panel with vertical auto-layout
                _content = AddUIComponent<UIPanel>();
                _content.autoLayout = true;
                _content.autoLayoutDirection = LayoutDirection.Vertical;
                _content.autoLayoutPadding = new RectOffset(12, 12, 8, 8); // external margins between children
                _content.padding = new RectOffset(12, 12, 40, 12);        // internal container padding
                _content.clipChildren = true;
                _content.size = new Vector2(width, height - 48f);
                _content.relativePosition = new Vector2(0f, 32f);

                // Add UI controls
                CreateModeDropdown();
                CreateHexSizeDropdown();
                CreateOpacitySlider();
                CreateLegendPlaceholder();

                // Initially hidden
                isVisible = false;

                Debug.Log("JobsHousingBalance: MainPanel initialized");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JobsHousingBalance: Exception in MainPanel.Start: " + ex.Message);
            }
        }

        private new void CenterToParent()
        {
            var ui = UIView.GetAView();
            relativePosition = new Vector2(
                Mathf.Round((ui.fixedWidth  - width)  / 2f),
                Mathf.Round((ui.fixedHeight - height) / 2f)
            );
        }

        public new void Show()
        {
            isVisible = true;
            BringToFront();
            Debug.Log("JobsHousingBalance: MainPanel shown");
        }

        public new void Hide()
        {
            isVisible = false;
            Debug.Log("JobsHousingBalance: MainPanel hidden");
        }

        public void Toggle()
        {
            if (isVisible)
                Hide();
            else
                Show();
        }

        private void OnCloseButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            Debug.Log("JobsHousingBalance: Close button clicked");
            Hide();
            eventParam.Use();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Debug.Log("JobsHousingBalance: MainPanel destroyed");
        }

        // Helper method: horizontal "row" (Label + Control)
        private UIPanel MakeRow(UIComponent parent, string label)
        {
            var row = parent.AddUIComponent<UIPanel>();
            row.autoLayout = true;
            row.autoLayoutDirection = LayoutDirection.Horizontal;
            row.autoLayoutPadding = new RectOffset(0, 8, 0, 0);
            row.height = 28f;
            row.width = parent.width - 24f;

            var lab = row.AddUIComponent<UILabel>();
            lab.text = label;
            lab.textScale = 0.9f;
            lab.autoSize = false;
            lab.width = 110f;
            lab.height = 24f;
            lab.verticalAlignment = UIVerticalAlignment.Middle;

            return row;
        }

        // Create mode dropdown (Hex/Districts)
        private void CreateModeDropdown()
        {
            try
            {
                var row = MakeRow(_content, "Mode");
                var dropdown = row.AddUIComponent<UIDropDown>();
                
                // Configure dropdown
                dropdown.items = new[] { "Hex", "Districts" };
                dropdown.selectedIndex = 0; // Default: Hex
                dropdown.size = new Vector2(180f, 28f);
                dropdown.listHeight = 180;
                dropdown.itemHeight = 24;
                dropdown.horizontalAlignment = UIHorizontalAlignment.Left;
                
                // Add visual sprites for dropdown
                dropdown.normalBgSprite = "GenericPanel";
                dropdown.hoveredBgSprite = "GenericPanelLight";
                dropdown.focusedBgSprite = "GenericPanelLight";
                dropdown.disabledBgSprite = "GenericPanel";
                
                // Set darker background for dropdown list
                dropdown.listBackground = "GenericPanel";
                dropdown.itemHover = "GenericPanelLight";
                dropdown.itemHighlight = "GenericPanelLight";
                
                // Create trigger button (invisible overlay)
                var triggerButton = dropdown.AddUIComponent<UIButton>();
                triggerButton.size = dropdown.size;
                triggerButton.relativePosition = Vector2.zero;
                triggerButton.normalBgSprite = "";
                triggerButton.hoveredBgSprite = "";
                triggerButton.pressedBgSprite = "";
                dropdown.triggerButton = triggerButton;
                
                // Add arrow sprite (visual only, non-interactive)
                var arrow = dropdown.AddUIComponent<UISprite>();
                arrow.spriteName = "IconDownArrow";
                arrow.size = new Vector2(16f, 16f);
                arrow.relativePosition = new Vector2(dropdown.width - 20f, 6f);
                arrow.isInteractive = false;
                arrow.zOrder = 10; // Above trigger button
                
                // Configure text alignment for dropdown using correct UIDropDown properties
                dropdown.textScale = 0.9f;
                dropdown.textColor = Color.white;
                
                // Use UIDropDown's built-in properties for text alignment
                dropdown.verticalAlignment = UIVerticalAlignment.Middle;  // Vertical center
                dropdown.horizontalAlignment = UIHorizontalAlignment.Left;  // Horizontal alignment
                dropdown.textFieldPadding = new RectOffset(8, 0, 0, 0); // Left padding, no top/bottom for perfect centering
                
                // Configure dropdown list appearance
                dropdown.itemHeight = 24;  // Height of list items
                dropdown.itemPadding = new RectOffset(8, 8, 4, 4);  // Padding inside list items
                
                Debug.Log($"JobsHousingBalance: Configured dropdown with verticalAlignment={dropdown.verticalAlignment}, textFieldPadding={dropdown.textFieldPadding}");
                
                // Event handler
                dropdown.eventSelectedIndexChanged += (component, index) =>
                {
                    Debug.Log($"JobsHousingBalance: Mode changed to {dropdown.items[index]}");
                    // TODO: Handle mode change (Hex/Districts)
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to create mode dropdown: {ex.Message}");
            }
        }

        // Create hex size dropdown
        private void CreateHexSizeDropdown()
        {
            try
            {
                var row = MakeRow(_content, "Hex size");
                var dropdown = row.AddUIComponent<UIDropDown>();
                
                // Configure dropdown
                dropdown.items = new[] { "64m", "128m", "256m", "512m" };
                dropdown.selectedIndex = 1; // Default: 128m
                dropdown.size = new Vector2(180f, 28f);
                dropdown.listHeight = 180;
                dropdown.itemHeight = 24;
                dropdown.horizontalAlignment = UIHorizontalAlignment.Left;
                
                // Add visual sprites for dropdown
                dropdown.normalBgSprite = "GenericPanel";
                dropdown.hoveredBgSprite = "GenericPanelLight";
                dropdown.focusedBgSprite = "GenericPanelLight";
                dropdown.disabledBgSprite = "GenericPanel";
                
                // Set darker background for dropdown list
                dropdown.listBackground = "GenericPanel";
                dropdown.itemHover = "GenericPanelLight";
                dropdown.itemHighlight = "GenericPanelLight";
                
                // Create trigger button (invisible overlay)
                var triggerButton = dropdown.AddUIComponent<UIButton>();
                triggerButton.size = dropdown.size;
                triggerButton.relativePosition = Vector2.zero;
                triggerButton.normalBgSprite = "";
                triggerButton.hoveredBgSprite = "";
                triggerButton.pressedBgSprite = "";
                dropdown.triggerButton = triggerButton;
                
                // Add arrow sprite (visual only, non-interactive)
                var arrow = dropdown.AddUIComponent<UISprite>();
                arrow.spriteName = "IconDownArrow";
                arrow.size = new Vector2(16f, 16f);
                arrow.relativePosition = new Vector2(dropdown.width - 20f, 6f);
                arrow.isInteractive = false;
                arrow.zOrder = 10; // Above trigger button
                
                // Configure text alignment for dropdown using correct UIDropDown properties
                dropdown.textScale = 0.9f;
                dropdown.textColor = Color.white;
                
                // Use UIDropDown's built-in properties for text alignment
                dropdown.verticalAlignment = UIVerticalAlignment.Middle;  // Vertical center
                dropdown.horizontalAlignment = UIHorizontalAlignment.Left;  // Horizontal alignment
                dropdown.textFieldPadding = new RectOffset(8, 0, 0, 0); // Left padding, no top/bottom for perfect centering
                
                // Configure dropdown list appearance
                dropdown.itemHeight = 24;  // Height of list items
                dropdown.itemPadding = new RectOffset(8, 8, 4, 4);  // Padding inside list items
                
                Debug.Log($"JobsHousingBalance: Configured dropdown with verticalAlignment={dropdown.verticalAlignment}, textFieldPadding={dropdown.textFieldPadding}");
                
                // Event handler
                dropdown.eventSelectedIndexChanged += (component, index) =>
                {
                    Debug.Log($"JobsHousingBalance: Hex size changed to {dropdown.items[index]}");
                    // TODO: Apply selected hex size preset
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to create hex size dropdown: {ex.Message}");
            }
        }

        // Create opacity slider
        private void CreateOpacitySlider()
        {
            try
            {
                var row = MakeRow(_content, "Opacity");
                
                // Create slider container
                var sliderContainer = row.AddUIComponent<UIPanel>();
                sliderContainer.size = new Vector2(180f, 40f); // Increased height for value display
                sliderContainer.autoLayout = true;
                sliderContainer.autoLayoutDirection = LayoutDirection.Vertical;
                sliderContainer.autoLayoutPadding = new RectOffset(0, 0, 4, 0);
                // No background sprite - will be transparent by default
                sliderContainer.atlas = null;
                
                // Add value display label
                var valueLabel = sliderContainer.AddUIComponent<UILabel>();
                valueLabel.text = "80%";
                valueLabel.textScale = 0.8f;
                valueLabel.textAlignment = UIHorizontalAlignment.Center;
                valueLabel.textColor = Color.white;
                valueLabel.size = new Vector2(180f, 16f);
                
                // Create slider
                var slider = sliderContainer.AddUIComponent<UISlider>();
                slider.size = new Vector2(180f, 18f);
                slider.minValue = 0.1f;  // 10%
                slider.maxValue = 0.8f;   // 80%
                slider.stepSize = 0.01f;  // 1%
                slider.value = 0.8f;      // Default: 80%
                slider.clipChildren = true; // Prevent sprites from going outside bounds
                
                // BASE TRACK - use ScrollbarTrack for flat background (no effects)
                var baseTrack = slider.AddUIComponent<UISlicedSprite>();
                baseTrack.spriteName = "ScrollbarTrack";
                baseTrack.size = slider.size;
                baseTrack.relativePosition = Vector2.zero;
                
                // FILL AREA - positioned correctly relative to thumb
                var fill = slider.AddUIComponent<UISlicedSprite>();
                fill.spriteName = "SliderFill";
                var fillPercent = (slider.value - slider.minValue) / (slider.maxValue - slider.minValue);
                fill.size = new Vector2(slider.size.x * fillPercent, slider.size.y);
                fill.relativePosition = Vector2.zero;
                
                // THUMB (handle)
                var thumb = slider.AddUIComponent<UISprite>();
                thumb.spriteName = "ScrollbarThumb";
                thumb.size = new Vector2(12f, 18f);
                thumb.relativePosition = new Vector2(slider.width * 0.8f - 6f, 0f); // Position at 80%
                
                // Event handler
                slider.eventValueChanged += (component, value) =>
                {
                    // Convert 0.1-0.8 range to percentage for display
                    var percentage = Mathf.RoundToInt(value * 100f);
                    Debug.Log($"JobsHousingBalance: Opacity changed to {value:F2} ({percentage}%)");
                    
                    // Update value display
                    valueLabel.text = $"{percentage}%";
                    
                    // Update fill size
                    var currentFillPercent = (value - slider.minValue) / (slider.maxValue - slider.minValue);
                    fill.size = new Vector2(slider.size.x * currentFillPercent, slider.size.y);
                    
                    // Update thumb position
                    thumb.relativePosition = new Vector2(slider.width * currentFillPercent - 6f, 0f);
                    
                    // TODO: Update overlay opacity
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to create opacity slider: {ex.Message}");
            }
        }

        // Create legend placeholder
        private void CreateLegendPlaceholder()
        {
            try
            {
                var legend = _content.AddUIComponent<UIPanel>();
                legend.autoLayout = true;
                legend.autoLayoutDirection = LayoutDirection.Vertical;
                legend.autoLayoutPadding = new RectOffset(6, 6, 6, 6);
                legend.autoSize = true; // Make height adaptive
                legend.width = _content.width - 24f;
                legend.backgroundSprite = "GenericPanel";
                legend.color = new Color32(255, 255, 255, 25);

                // Title
                var title = legend.AddUIComponent<UILabel>();
                title.text = "Legend";
                title.textScale = 0.9f;
                title.textColor = Color.white;

                // Legend items with correct colors and descriptions
                CreateLegendItem(legend, "InfoIconHappiness", "Красный — нужны рабочие места (jobs < residents)", new Color32(255, 100, 100, 255));
                CreateLegendItem(legend, "InfoIconHappiness", "Синий — нужно жильё (jobs > residents)", new Color32(100, 100, 255, 255));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to create legend placeholder: {ex.Message}");
            }
        }

        // Helper method to create legend items
        private void CreateLegendItem(UIComponent parent, string spriteName, string text, Color32 color)
        {
            var item = parent.AddUIComponent<UIPanel>();
            item.size = new Vector2(parent.width - 12f, 20f);
            item.autoLayout = true;
            item.autoLayoutDirection = LayoutDirection.Horizontal;
            item.autoLayoutPadding = new RectOffset(0, 4, 0, 0);

            // Color dot
            var dot = item.AddUIComponent<UISprite>();
            dot.spriteName = spriteName;
            dot.size = new Vector2(16f, 16f);
            dot.color = color;

            // Text label
            var label = item.AddUIComponent<UILabel>();
            label.text = text;
            label.textScale = 0.8f;
            label.textColor = Color.white;
            label.verticalAlignment = UIVerticalAlignment.Middle;
        }
    }
}
