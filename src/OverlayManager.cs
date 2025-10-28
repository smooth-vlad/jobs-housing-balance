using UnityEngine;
using JobsHousingBalance.Config;
using JobsHousingBalance.Rendering.Overlay;
using JobsHousingBalance.Data.Collector;

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
        private DataUpdateTriggers _dataUpdateTriggers;
        
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
        
        /// <summary>
        /// Получить систему триггеров обновления данных
        /// </summary>
        public DataUpdateTriggers DataUpdateTriggers
        {
            get
            {
                if (_dataUpdateTriggers == null)
                {
                    // Создаем GameObject для DataUpdateTriggers
                    var go = new GameObject("JobsHousingBalance_DataUpdateTriggers");
                    _dataUpdateTriggers = go.AddComponent<DataUpdateTriggers>();
                    Debug.Log("JobsHousingBalance: DataUpdateTriggers created");
                }
                return _dataUpdateTriggers;
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
                
                // Initialize data collection system
                DataUpdateTriggers.Initialize();
                
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
        /// Получить актуальные данные о зданиях
        /// </summary>
        public BuildingDataCollector GetBuildingData()
        {
            return DataUpdateTriggers?.GetDataCollector();
        }
        
        /// <summary>
        /// Получить статистику системы сбора данных
        /// </summary>
        public string GetDataStats()
        {
            var collector = GetBuildingData();
            var triggers = DataUpdateTriggers;
            
            if (collector == null || triggers == null)
            {
                return "Data collection system not initialized";
            }
            
            return $"Collector: {collector.GetDebugStats()}\n" +
                   $"Triggers: {triggers.UpdateStats}\n" +
                   $"Cache: {triggers.GetDataCache()?.CacheStats ?? "Not available"}";
        }
        
        /// <summary>
        /// Принудительно обновить данные
        /// </summary>
        public void ForceDataUpdate()
        {
            DataUpdateTriggers?.ForceUpdate();
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
            
            if (_dataUpdateTriggers != null)
            {
                if (_dataUpdateTriggers.gameObject != null)
                {
                    Object.Destroy(_dataUpdateTriggers.gameObject);
                }
                _dataUpdateTriggers = null;
                Debug.Log("JobsHousingBalance: DataUpdateTriggers destroyed");
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
            
            // Смена режима требует обновления данных (Hex vs Districts)
            DataUpdateTriggers?.MarkNonCriticalChanges();
            
            Debug.Log($"JobsHousingBalance: OverlayManager - Mode changed to {newMode}");
        }
        
        /// <summary>
        /// Обработчик изменения размера гекса в AppState
        /// </summary>
        private void OnAppStateHexSizeChanged(AppState.HexSize newHexSize)
        {
            OverlayRenderer.SetHexSize(newHexSize);
            
            // Смена размера гекса требует обновления агрегации
            DataUpdateTriggers?.MarkNonCriticalChanges();
            
            Debug.Log($"JobsHousingBalance: OverlayManager - HexSize changed to {newHexSize}");
        }
        
        /// <summary>
        /// Обработчик изменения прозрачности в AppState
        /// </summary>
        private void OnAppStateOpacityChanged(float newOpacity)
        {
            OverlayRenderer.SetOpacity(newOpacity);
            
            // Изменение прозрачности не требует пересчета данных
            Debug.Log($"JobsHousingBalance: OverlayManager - Opacity changed to {newOpacity:F2}");
        }
        
        #endregion
    }
}
