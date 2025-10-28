using System;
using UnityEngine;
using ColossalFramework;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Система триггеров для автоматического обновления данных о зданиях
    /// Поддерживает как периодические обновления, так и событийные триггеры
    /// </summary>
    public class DataUpdateTriggers : MonoBehaviour
    {
        #region Fields
        
        private BuildingDataCollector _dataCollector;
        private DataCache _dataCache;
        private BuildingEventExtension _buildingEventExtension;
        private DataCollectionThreadingExtension _threadingExtension;
        private int _lastUpdateFrame;
        private bool _isInitialized;
        
        // Константы для интервалов обновления
        private const int PeriodicUpdateIntervalFrames = 256; // ~5-10 секунд
        private const int CriticalUpdateIntervalFrames = 64; // ~1-2 секунды для критичных изменений
        
        // Флаги для отслеживания состояния
        private bool _hasCriticalChanges;
        private int _lastCriticalUpdateFrame;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Проверить, инициализированы ли триггеры
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Получить статистику обновлений
        /// </summary>
        public string UpdateStats
        {
            get
            {
                var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
                var framesSinceLastUpdate = currentFrame - _lastUpdateFrame;
                var framesSinceCriticalUpdate = currentFrame - _lastCriticalUpdateFrame;
                
                return $"Last update: {framesSinceLastUpdate} frames ago, " +
                       $"Last critical update: {framesSinceCriticalUpdate} frames ago, " +
                       $"Has critical changes: {_hasCriticalChanges}";
            }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (!_isInitialized) return;
            
            CheckForUpdates();
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Инициализировать систему триггеров
        /// </summary>
        public void Initialize()
        {
            try
            {
                _dataCollector = new BuildingDataCollector();
                _dataCache = new DataCache();
                
                // Создаем и инициализируем BuildingEventExtension
                _buildingEventExtension = new BuildingEventExtension();
                _buildingEventExtension.Initialize(this);
                
                // Создаем и инициализируем ThreadingExtension для тяжелых операций
                _threadingExtension = new DataCollectionThreadingExtension();
                _threadingExtension.Initialize(_dataCollector, _dataCache);
                
                _lastUpdateFrame = (int)SimulationManager.instance.m_currentFrameIndex;
                _lastCriticalUpdateFrame = _lastUpdateFrame;
                _hasCriticalChanges = false;
                _isInitialized = true;
                
                Debug.Log("JobsHousingBalance: DataUpdateTriggers initialized with BuildingEventExtension and ThreadingExtension");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Failed to initialize DataUpdateTriggers: {ex.Message}");
                _isInitialized = false;
            }
        }
        
        /// <summary>
        /// Принудительно обновить данные
        /// </summary>
        public void ForceUpdate()
        {
            if (!_isInitialized) return;
            
            try
            {
                Debug.Log("JobsHousingBalance: Force updating data...");
                
                // Используем ThreadingExtension для тяжелых операций
                if (_threadingExtension != null && !_threadingExtension.IsProcessing)
                {
                    _threadingExtension.StartBatchProcessing();
                }
                else
                {
                    // Fallback на обычный метод если ThreadingExtension недоступен
                    _dataCollector.ForceRefresh();
                    _dataCache.MarkCacheClean();
                }
                
                _lastUpdateFrame = (int)SimulationManager.instance.m_currentFrameIndex;
                _hasCriticalChanges = false;
                
                Debug.Log($"JobsHousingBalance: Force update completed. {_dataCollector.GetDebugStats()}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error during force update: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Пометить изменения как критичные (требуют быстрого обновления)
        /// </summary>
        public void MarkCriticalChanges()
        {
            _hasCriticalChanges = true;
            Debug.Log("JobsHousingBalance: Critical changes detected, scheduling fast update");
        }
        
        /// <summary>
        /// Пометить изменения как некритичные (обычное обновление)
        /// </summary>
        public void MarkNonCriticalChanges()
        {
            _dataCache.InvalidateCache();
            Debug.Log("JobsHousingBalance: Non-critical changes detected, invalidating cache");
        }
        
        /// <summary>
        /// Получить актуальные данные о зданиях
        /// </summary>
        public BuildingDataCollector GetDataCollector()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("JobsHousingBalance: DataUpdateTriggers not initialized");
                return null;
            }
            
            // Проверяем, нужны ли обновления
            if (_dataCollector.NeedsUpdate())
            {
                _dataCollector.UpdateIfNeeded();
            }
            
            return _dataCollector;
        }
        
        /// <summary>
        /// Получить кэш данных
        /// </summary>
        public DataCache GetDataCache()
        {
            return _dataCache;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Проверить необходимость обновления данных
        /// </summary>
        private void CheckForUpdates()
        {
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            
            // Проверяем критичные изменения
            if (_hasCriticalChanges)
            {
                var framesSinceCriticalUpdate = currentFrame - _lastCriticalUpdateFrame;
                if (framesSinceCriticalUpdate >= CriticalUpdateIntervalFrames)
                {
                    PerformCriticalUpdate();
                }
            }
            
            // Проверяем обычные периодические обновления
            var framesSinceLastUpdate = currentFrame - _lastUpdateFrame;
            if (framesSinceLastUpdate >= PeriodicUpdateIntervalFrames)
            {
                PerformPeriodicUpdate();
            }
            
            // Очищаем устаревшие записи кэша
            if (framesSinceLastUpdate % (PeriodicUpdateIntervalFrames * 2) == 0)
            {
                _dataCache.CleanupExpiredEntries();
            }
        }
        
        /// <summary>
        /// Выполнить критичное обновление (быстрое)
        /// </summary>
        private void PerformCriticalUpdate()
        {
            try
            {
                Debug.Log("JobsHousingBalance: Performing critical update...");
                _dataCollector.ForceRefresh();
                _dataCache.MarkCacheClean();
                _lastCriticalUpdateFrame = (int)SimulationManager.instance.m_currentFrameIndex;
                _hasCriticalChanges = false;
                
                Debug.Log($"JobsHousingBalance: Critical update completed. {_dataCollector.GetDebugStats()}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error during critical update: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Выполнить периодическое обновление (обычное)
        /// </summary>
        private void PerformPeriodicUpdate()
        {
            try
            {
                Debug.Log("JobsHousingBalance: Performing periodic update...");
                _dataCollector.UpdateIfNeeded();
                _dataCache.MarkCacheClean();
                _lastUpdateFrame = (int)SimulationManager.instance.m_currentFrameIndex;
                
                Debug.Log($"JobsHousingBalance: Periodic update completed. {_dataCollector.GetDebugStats()}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error during periodic update: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Очистить ресурсы
        /// </summary>
        private void Cleanup()
        {
            if (_dataCache != null)
            {
                _dataCache.ClearCache();
            }
            
            if (_buildingEventExtension != null)
            {
                // BuildingEventExtension не требует специальной очистки
                _buildingEventExtension = null;
            }
            
            if (_threadingExtension != null)
            {
                // Останавливаем обработку если она выполняется
                if (_threadingExtension.IsProcessing)
                {
                    _threadingExtension.StopBatchProcessing();
                }
                _threadingExtension = null;
            }
            
            _isInitialized = false;
            Debug.Log("JobsHousingBalance: DataUpdateTriggers cleaned up");
        }
        
        #endregion
    }
}
