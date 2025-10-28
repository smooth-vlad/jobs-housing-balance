using ColossalFramework.UI;
using UnityEngine;

namespace JobsHousingBalance.UI.Panel
{
    public class MainPanel : UIPanel
    {
        private UILabel _title;
        private UIButton _close;

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
                size = new Vector2(300f, 400f);
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
    }
}
