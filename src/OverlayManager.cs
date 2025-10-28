using UnityEngine;
using JobsHousingBalance.Config;
using JobsHousingBalance.Rendering.Overlay;

namespace JobsHousingBalance
{
    /// <summary>
    /// Менеджер для управления OverlayRenderer и интеграции с AppState
    /// </summary>
    public class OverlayManager
    {
        #region Singleton Pattern
        
        private static OverlayManager _instance;
        private static readonly object _lock = new object();
        
        public static OverlayManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new OverlayManager();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private OverlayManager()
        {
            Debug.Log("JobsHousingBalance: OverlayManager initialized");
        }
        
        #endregion
        
        #region Fields
        
        private OverlayRenderer _overlayRenderer;
        private AppState _appState;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Получить экземпляр OverlayRenderer
        /// </summary>
        public OverlayRenderer OverlayRenderer
        {
            get
            {
                if (_overlayRenderer == null)
                {
                    _overlayRenderer = new OverlayRenderer();
                    Debug.Log("JobsHousingBalance: OverlayRenderer created");
                }
                return _overlayRenderer;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Инициализировать менеджер и подписаться на события AppState
        /// </summary>
        public void Initialize()
        {
            try
            {
                _appState = AppState.Instance;
                
                if (_appState == null)
                {
                    Debug.LogError("JobsHousingBalance: AppState.Instance returned null");
                    return;
                }
                
                // Subscribe to AppState events
                _appState.OnModeChanged += OnAppStateModeChanged;
                _appState.OnHexSizeChanged += OnAppStateHexSizeChanged;
                _appState.OnOpacityChanged += OnAppStateOpacityChanged;
                
                // Initialize overlay with current AppState values
                OverlayRenderer.UpdateFromAppState(_appState);
                
                Debug.Log("JobsHousingBalance: OverlayManager initialized and subscribed to AppState events");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to initialize OverlayManager: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Показать оверлей
        /// </summary>
        public void ShowOverlay()
        {
            OverlayRenderer.Show();
        }
        
        /// <summary>
        /// Скрыть оверлей
        /// </summary>
        public void HideOverlay()
        {
            OverlayRenderer.Hide();
        }
        
        /// <summary>
        /// Переключить видимость оверлея
        /// </summary>
        public void ToggleOverlay()
        {
            OverlayRenderer.Toggle();
        }
        
        /// <summary>
        /// Очистить ресурсы
        /// </summary>
        public void Cleanup()
        {
            if (_appState != null)
            {
                _appState.OnModeChanged -= OnAppStateModeChanged;
                _appState.OnHexSizeChanged -= OnAppStateHexSizeChanged;
                _appState.OnOpacityChanged -= OnAppStateOpacityChanged;
                
                Debug.Log("JobsHousingBalance: OverlayManager unsubscribed from AppState events");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Обработчик изменения режима в AppState
        /// </summary>
        private void OnAppStateModeChanged(AppState.Mode newMode)
        {
            OverlayRenderer.SetMode(newMode);
            Debug.Log($"JobsHousingBalance: OverlayManager - Mode changed to {newMode}");
        }
        
        /// <summary>
        /// Обработчик изменения размера гекса в AppState
        /// </summary>
        private void OnAppStateHexSizeChanged(AppState.HexSize newHexSize)
        {
            OverlayRenderer.SetHexSize(newHexSize);
            Debug.Log($"JobsHousingBalance: OverlayManager - HexSize changed to {newHexSize}");
        }
        
        /// <summary>
        /// Обработчик изменения прозрачности в AppState
        /// </summary>
        private void OnAppStateOpacityChanged(float newOpacity)
        {
            OverlayRenderer.SetOpacity(newOpacity);
            Debug.Log($"JobsHousingBalance: OverlayManager - Opacity changed to {newOpacity:F2}");
        }
        
        #endregion
    }
}
