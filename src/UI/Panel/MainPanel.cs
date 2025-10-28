using ColossalFramework.UI;
using UnityEngine;
using JobsHousingBalance.Config;

namespace JobsHousingBalance.UI.Panel
{
    public class MainPanel : UIPanel
    {
        private UILabel _title;
        private UIButton _close;
        private UIPanel _content;
        
        // UI Controls
        private UIDropDown _modeDropdown;
        private UIDropDown _hexSizeDropdown;
        private UISlider _opacitySlider;
        private UILabel _opacityValueLabel;
        private UIPanel _legendPanel;
        
        // AppState reference
        private AppState _appState;

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
                // Initialize AppState
                _appState = AppState.Instance;
                
                // Subscribe to AppState events
                SubscribeToAppStateEvents();
                // Panel size and background - use auto-sizing
                width = 350f; // Fixed width
                autoFitChildrenVertically = true; // Auto-fit height based on content
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
                _content.autoLayoutPadding = new RectOffset(12, 12, 12, 12); // external margins between children
                _content.padding = new RectOffset(12, 12, 40, 12);        // internal container padding
                _content.clipChildren = true;
                _content.width = width; // Fixed width
                _content.autoFitChildrenVertically = true; // Auto-fit height based on content
                _content.relativePosition = new Vector2(0f, 32f);

                // Add UI controls
                CreateModeDropdown();
                CreateHexSizeDropdown();
                CreateOpacitySlider();
                CreateLegendPlaceholder();
                
                // Legend is static - no need to initialize with mode

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

        /// <summary>
        /// Подписаться на события AppState
        /// </summary>
        private void SubscribeToAppStateEvents()
        {
            if (_appState != null)
            {
                _appState.OnModeChanged += OnAppStateModeChanged;
                _appState.OnHexSizeChanged += OnAppStateHexSizeChanged;
                _appState.OnOpacityChanged += OnAppStateOpacityChanged;
                
                Debug.Log("JobsHousingBalance: MainPanel subscribed to AppState events");
            }
        }
        
        /// <summary>
        /// Отписаться от событий AppState
        /// </summary>
        private void UnsubscribeFromAppStateEvents()
        {
            if (_appState != null)
            {
                _appState.OnModeChanged -= OnAppStateModeChanged;
                _appState.OnHexSizeChanged -= OnAppStateHexSizeChanged;
                _appState.OnOpacityChanged -= OnAppStateOpacityChanged;
                
                Debug.Log("JobsHousingBalance: MainPanel unsubscribed from AppState events");
            }
        }
        
        /// <summary>
        /// Обработчик изменения режима в AppState
        /// </summary>
        private void OnAppStateModeChanged(AppState.Mode newMode)
        {
            if (_modeDropdown != null)
            {
                var modeString = newMode.ToString();
                var index = System.Array.IndexOf(_modeDropdown.items, modeString);
                if (index >= 0 && index != _modeDropdown.selectedIndex)
                {
                    _modeDropdown.selectedIndex = index;
                    Debug.Log($"JobsHousingBalance: UI Mode dropdown updated to {modeString}");
                }
            }
            
            // Legend is static - no need to update it based on mode
            Debug.Log($"JobsHousingBalance: Mode changed to {newMode}, Legend remains static");
        }
        
        /// <summary>
        /// Обработчик изменения размера гекса в AppState
        /// </summary>
        private void OnAppStateHexSizeChanged(AppState.HexSize newHexSize)
        {
            if (_hexSizeDropdown != null)
            {
                var hexSizeString = $"{(int)newHexSize}m";
                var index = System.Array.IndexOf(_hexSizeDropdown.items, hexSizeString);
                if (index >= 0 && index != _hexSizeDropdown.selectedIndex)
                {
                    _hexSizeDropdown.selectedIndex = index;
                    Debug.Log($"JobsHousingBalance: UI Hex size dropdown updated to {hexSizeString}");
                }
            }
        }
        
        /// <summary>
        /// Обработчик изменения прозрачности в AppState
        /// </summary>
        private void OnAppStateOpacityChanged(float newOpacity)
        {
            if (_opacitySlider != null && _opacityValueLabel != null)
            {
                if (Mathf.Abs(_opacitySlider.value - newOpacity) > 0.001f)
                {
                    _opacitySlider.value = newOpacity;
                    var percentage = Mathf.RoundToInt(newOpacity * 100f);
                    _opacityValueLabel.text = $"{percentage}%";
                    Debug.Log($"JobsHousingBalance: UI Opacity slider updated to {newOpacity:F2} ({percentage}%)");
                }
            }
        }

        public override void OnDestroy()
        {
            // Unsubscribe from events before destroying
            UnsubscribeFromAppStateEvents();
            
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
                _modeDropdown = row.AddUIComponent<UIDropDown>();
                
                // Configure dropdown
                _modeDropdown.items = new[] { "Hex", "Districts" };
                _modeDropdown.selectedIndex = 0; // Default: Hex
                _modeDropdown.size = new Vector2(180f, 28f);
                _modeDropdown.listHeight = 180;
                _modeDropdown.itemHeight = 24;
                _modeDropdown.horizontalAlignment = UIHorizontalAlignment.Left;
                
                // Add visual sprites for dropdown
                _modeDropdown.normalBgSprite = "GenericPanel";
                _modeDropdown.hoveredBgSprite = "GenericPanelLight";
                _modeDropdown.focusedBgSprite = "GenericPanelLight";
                _modeDropdown.disabledBgSprite = "GenericPanel";
                
                // Set darker background for dropdown list
                _modeDropdown.listBackground = "GenericPanel";
                _modeDropdown.itemHover = "GenericPanelLight";
                _modeDropdown.itemHighlight = "GenericPanelLight";
                
                // Create trigger button (invisible overlay)
                var triggerButton = _modeDropdown.AddUIComponent<UIButton>();
                triggerButton.size = _modeDropdown.size;
                triggerButton.relativePosition = Vector2.zero;
                triggerButton.normalBgSprite = "";
                triggerButton.hoveredBgSprite = "";
                triggerButton.pressedBgSprite = "";
                _modeDropdown.triggerButton = triggerButton;
                
                // Add arrow sprite (visual only, non-interactive)
                var arrow = _modeDropdown.AddUIComponent<UISprite>();
                arrow.spriteName = "IconDownArrow";
                arrow.size = new Vector2(16f, 16f);
                arrow.relativePosition = new Vector2(_modeDropdown.width - 20f, 6f);
                arrow.isInteractive = false;
                arrow.zOrder = 10; // Above trigger button
                
                // Configure text alignment for dropdown using correct UIDropDown properties
                _modeDropdown.textScale = 0.9f;
                _modeDropdown.textColor = Color.white;
                
                // Use UIDropDown's built-in properties for text alignment
                _modeDropdown.verticalAlignment = UIVerticalAlignment.Middle;  // Vertical center
                _modeDropdown.horizontalAlignment = UIHorizontalAlignment.Left;  // Horizontal alignment
                _modeDropdown.textFieldPadding = new RectOffset(8, 0, 0, 0); // Left padding, no top/bottom for perfect centering
                
                // Configure dropdown list appearance
                _modeDropdown.itemHeight = 24;  // Height of list items
                _modeDropdown.itemPadding = new RectOffset(8, 8, 4, 4);  // Padding inside list items
                
                Debug.Log($"JobsHousingBalance: Configured mode dropdown with verticalAlignment={_modeDropdown.verticalAlignment}, textFieldPadding={_modeDropdown.textFieldPadding}");
                
                // Event handler - update AppState when user changes mode
                _modeDropdown.eventSelectedIndexChanged += (component, index) =>
                {
                    var selectedMode = _modeDropdown.items[index];
                    Debug.Log($"JobsHousingBalance: Mode changed to {selectedMode}");
                    
                    // Update AppState
                    if (_appState != null)
                    {
                        _appState.SetModeFromString(selectedMode);
                    }
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
                _hexSizeDropdown = row.AddUIComponent<UIDropDown>();
                
                // Configure dropdown
                _hexSizeDropdown.items = new[] { "64m", "128m", "256m", "512m" };
                _hexSizeDropdown.selectedIndex = 1; // Default: 128m
                _hexSizeDropdown.size = new Vector2(180f, 28f);
                _hexSizeDropdown.listHeight = 180;
                _hexSizeDropdown.itemHeight = 24;
                _hexSizeDropdown.horizontalAlignment = UIHorizontalAlignment.Left;
                
                // Add visual sprites for dropdown
                _hexSizeDropdown.normalBgSprite = "GenericPanel";
                _hexSizeDropdown.hoveredBgSprite = "GenericPanelLight";
                _hexSizeDropdown.focusedBgSprite = "GenericPanelLight";
                _hexSizeDropdown.disabledBgSprite = "GenericPanel";
                
                // Set darker background for dropdown list
                _hexSizeDropdown.listBackground = "GenericPanel";
                _hexSizeDropdown.itemHover = "GenericPanelLight";
                _hexSizeDropdown.itemHighlight = "GenericPanelLight";
                
                // Create trigger button (invisible overlay)
                var triggerButton = _hexSizeDropdown.AddUIComponent<UIButton>();
                triggerButton.size = _hexSizeDropdown.size;
                triggerButton.relativePosition = Vector2.zero;
                triggerButton.normalBgSprite = "";
                triggerButton.hoveredBgSprite = "";
                triggerButton.pressedBgSprite = "";
                _hexSizeDropdown.triggerButton = triggerButton;
                
                // Add arrow sprite (visual only, non-interactive)
                var arrow = _hexSizeDropdown.AddUIComponent<UISprite>();
                arrow.spriteName = "IconDownArrow";
                arrow.size = new Vector2(16f, 16f);
                arrow.relativePosition = new Vector2(_hexSizeDropdown.width - 20f, 6f);
                arrow.isInteractive = false;
                arrow.zOrder = 10; // Above trigger button
                
                // Configure text alignment for dropdown using correct UIDropDown properties
                _hexSizeDropdown.textScale = 0.9f;
                _hexSizeDropdown.textColor = Color.white;
                
                // Use UIDropDown's built-in properties for text alignment
                _hexSizeDropdown.verticalAlignment = UIVerticalAlignment.Middle;  // Vertical center
                _hexSizeDropdown.horizontalAlignment = UIHorizontalAlignment.Left;  // Horizontal alignment
                _hexSizeDropdown.textFieldPadding = new RectOffset(8, 0, 0, 0); // Left padding, no top/bottom for perfect centering
                
                // Configure dropdown list appearance
                _hexSizeDropdown.itemHeight = 24;  // Height of list items
                _hexSizeDropdown.itemPadding = new RectOffset(8, 8, 4, 4);  // Padding inside list items
                
                Debug.Log($"JobsHousingBalance: Configured hex size dropdown with verticalAlignment={_hexSizeDropdown.verticalAlignment}, textFieldPadding={_hexSizeDropdown.textFieldPadding}");
                
                // Event handler - update AppState when user changes hex size
                _hexSizeDropdown.eventSelectedIndexChanged += (component, index) =>
                {
                    var selectedHexSize = _hexSizeDropdown.items[index];
                    Debug.Log($"JobsHousingBalance: Hex size changed to {selectedHexSize}");
                    
                    // Update AppState
                    if (_appState != null)
                    {
                        _appState.SetHexSizeFromString(selectedHexSize);
                    }
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
                _opacityValueLabel = sliderContainer.AddUIComponent<UILabel>();
                _opacityValueLabel.text = "80%";
                _opacityValueLabel.textScale = 0.8f;
                _opacityValueLabel.textAlignment = UIHorizontalAlignment.Center;
                _opacityValueLabel.textColor = Color.white;
                _opacityValueLabel.size = new Vector2(180f, 16f);
                
                // Create slider
                _opacitySlider = sliderContainer.AddUIComponent<UISlider>();
                _opacitySlider.size = new Vector2(180f, 18f);
                _opacitySlider.minValue = 0.1f;  // 10%
                _opacitySlider.maxValue = 0.8f;   // 80%
                _opacitySlider.stepSize = 0.01f;  // 1%
                _opacitySlider.value = 0.8f;      // Default: 80%
                _opacitySlider.clipChildren = true; // Prevent sprites from going outside bounds
                
                // BASE TRACK - use ScrollbarTrack for flat background (no effects)
                var baseTrack = _opacitySlider.AddUIComponent<UISlicedSprite>();
                baseTrack.spriteName = "ScrollbarTrack";
                baseTrack.size = _opacitySlider.size;
                baseTrack.relativePosition = Vector2.zero;
                
                // FILL AREA - positioned correctly relative to thumb
                var fill = _opacitySlider.AddUIComponent<UISlicedSprite>();
                fill.spriteName = "SliderFill";
                var fillPercent = (_opacitySlider.value - _opacitySlider.minValue) / (_opacitySlider.maxValue - _opacitySlider.minValue);
                fill.size = new Vector2(_opacitySlider.size.x * fillPercent, _opacitySlider.size.y);
                fill.relativePosition = Vector2.zero;
                
                // THUMB (handle)
                var thumb = _opacitySlider.AddUIComponent<UISprite>();
                thumb.spriteName = "ScrollbarThumb";
                thumb.size = new Vector2(12f, 18f);
                thumb.relativePosition = new Vector2(_opacitySlider.width * 0.8f - 6f, 0f); // Position at 80%
                
                // Event handler - update AppState when user changes opacity
                _opacitySlider.eventValueChanged += (component, value) =>
                {
                    // Convert 0.1-0.8 range to percentage for display
                    var percentage = Mathf.RoundToInt(value * 100f);
                    Debug.Log($"JobsHousingBalance: Opacity changed to {value:F2} ({percentage}%)");
                    
                    // Update value display
                    _opacityValueLabel.text = $"{percentage}%";
                    
                    // Update fill size
                    var currentFillPercent = (value - _opacitySlider.minValue) / (_opacitySlider.maxValue - _opacitySlider.minValue);
                    fill.size = new Vector2(_opacitySlider.size.x * currentFillPercent, _opacitySlider.size.y);
                    
                    // Update thumb position
                    thumb.relativePosition = new Vector2(_opacitySlider.width * currentFillPercent - 6f, 0f);
                    
                    // Update AppState
                    if (_appState != null)
                    {
                        _appState.Opacity = value;
                    }
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to create opacity slider: {ex.Message}");
            }
        }

        // Create static legend (same for all modes)
        private void CreateLegendPlaceholder()
        {
            try
            {
                _legendPanel = _content.AddUIComponent<UIPanel>();
                _legendPanel.autoLayout = true;
                _legendPanel.autoLayoutDirection = LayoutDirection.Vertical;
                _legendPanel.autoLayoutPadding = new RectOffset(12, 12, 12, 16); // More padding all around
                _legendPanel.width = _content.width - 24f;
                _legendPanel.height = 380f; // Sufficient height for all 4 legend items
                _legendPanel.backgroundSprite = "GenericPanel";
                _legendPanel.color = new Color32(0, 0, 0, 180); // Dark background with good opacity

                // Title
                var title = _legendPanel.AddUIComponent<UILabel>();
                title.text = "Legend";
                title.textScale = 1.0f;
                title.textColor = Color.white;
                title.textAlignment = UIHorizontalAlignment.Center;
                title.autoSize = true; // Auto-size based on text content
                title.width = _legendPanel.width - 16f;

                // Add separator line
                var separator = _legendPanel.AddUIComponent<UISprite>();
                separator.spriteName = "GenericPanel";
                separator.size = new Vector2(_legendPanel.width - 16f, 1f);
                separator.color = new Color32(255, 255, 255, 200); // More visible separator

                // Legend items with correct colors and descriptions
                CreateLegendItem(_legendPanel, "InfoIconHappiness", "Красный — нужны рабочие места", 
                    "jobs < residents", new Color32(255, 100, 100, 255));
                CreateLegendItem(_legendPanel, "InfoIconHappiness", "Синий — нужно жильё", 
                    "jobs > residents", new Color32(100, 100, 255, 255));
                CreateLegendItem(_legendPanel, "InfoIconHappiness", "Зелёный — баланс", 
                    "jobs ≈ residents", new Color32(100, 255, 100, 255));
                CreateLegendItem(_legendPanel, "InfoIconHappiness", "Серый — нет данных", 
                    "insufficient data", new Color32(150, 150, 150, 255));
                
                Debug.Log("JobsHousingBalance: Static legend created (same for all modes)");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to create legend: {ex.Message}");
            }
        }

        // Helper method to create legend items with main text and subtitle
        private void CreateLegendItem(UIComponent parent, string spriteName, string mainText, string subtitle, Color32 color)
        {
            var item = parent.AddUIComponent<UIPanel>();
            item.width = parent.width - 24f;
            item.height = 36f; // Increased height for comfortable two-line display
            item.autoLayout = true;
            item.autoLayoutDirection = LayoutDirection.Horizontal;
            item.autoLayoutPadding = new RectOffset(0, 8, 2, 4); // Comfortable padding around item

            // Color dot
            var dot = item.AddUIComponent<UISprite>();
            dot.spriteName = spriteName;
            dot.size = new Vector2(16f, 16f);
            dot.color = color;

            // Text container
            var textContainer = item.AddUIComponent<UIPanel>();
            textContainer.autoLayout = true;
            textContainer.autoLayoutDirection = LayoutDirection.Vertical;
            textContainer.autoLayoutPadding = new RectOffset(0, 0, 2, 2); // Small padding between main text and subtitle
            textContainer.width = item.width - 22f;
            textContainer.height = 28f; // Fixed height for text container

            // Main text label
            var mainLabel = textContainer.AddUIComponent<UILabel>();
            mainLabel.text = mainText;
            mainLabel.textScale = 0.85f;
            mainLabel.textColor = Color.white;
            mainLabel.size = new Vector2(textContainer.width, 14f); // Fixed height for main text

            // Subtitle label
            var subtitleLabel = textContainer.AddUIComponent<UILabel>();
            subtitleLabel.text = subtitle;
            subtitleLabel.textScale = 0.75f;
            subtitleLabel.textColor = new Color32(220, 220, 220, 255); // Brighter subtitle for better contrast
            subtitleLabel.size = new Vector2(textContainer.width, 12f); // Fixed height for subtitle
        }
    }
}
