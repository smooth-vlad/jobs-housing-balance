using UnityEngine;
using ColossalFramework;
using ICities;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Расширение для отслеживания событий зданий и автоматического обновления данных
    /// Использует BuildingExtensionBase для подписки на события создания/сноса/перемещения зданий
    /// </summary>
    public class BuildingEventExtension : BuildingExtensionBase
    {
        #region Fields
        
        private DataUpdateTriggers _dataUpdateTriggers;
        private bool _isInitialized;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Проверить, инициализировано ли расширение
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Инициализировать расширение с системой триггеров
        /// </summary>
        public void Initialize(DataUpdateTriggers dataUpdateTriggers)
        {
            _dataUpdateTriggers = dataUpdateTriggers;
            _isInitialized = true;
            
            Debug.Log("JobsHousingBalance: BuildingEventExtension initialized");
        }
        
        #endregion
        
        #region BuildingExtensionBase Overrides
        
        /// <summary>
        /// Вызывается при создании здания
        /// </summary>
        public override void OnBuildingCreated(ushort buildingId)
        {
            if (!_isInitialized || _dataUpdateTriggers == null) return;
            
            // Валидация buildingId
            if (!IsValidBuildingId(buildingId))
            {
                Debug.LogWarning($"JobsHousingBalance: Invalid buildingId {buildingId} in OnBuildingCreated");
                return;
            }
            
            try
            {
                // Создание здания - критичное изменение, требует быстрого обновления
                _dataUpdateTriggers.MarkCriticalChanges();
                
                Debug.Log($"JobsHousingBalance: Building {buildingId} created, marking critical update");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error in OnBuildingCreated: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Вызывается при сносе здания
        /// </summary>
        public override void OnBuildingReleased(ushort buildingId)
        {
            if (!_isInitialized || _dataUpdateTriggers == null) return;
            
            // Валидация buildingId
            if (!IsValidBuildingId(buildingId))
            {
                Debug.LogWarning($"JobsHousingBalance: Invalid buildingId {buildingId} in OnBuildingReleased");
                return;
            }
            
            try
            {
                // Снос здания - критичное изменение, требует быстрого обновления
                _dataUpdateTriggers.MarkCriticalChanges();
                
                Debug.Log($"JobsHousingBalance: Building {buildingId} released, marking critical update");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error in OnBuildingReleased: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Вызывается при перемещении здания
        /// </summary>
        public override void OnBuildingRelocated(ushort buildingId)
        {
            if (!_isInitialized || _dataUpdateTriggers == null) return;
            
            // Валидация buildingId
            if (!IsValidBuildingId(buildingId))
            {
                Debug.LogWarning($"JobsHousingBalance: Invalid buildingId {buildingId} in OnBuildingRelocated");
                return;
            }
            
            try
            {
                // Перемещение здания - критичное изменение, требует быстрого обновления
                _dataUpdateTriggers.MarkCriticalChanges();
                
                Debug.Log($"JobsHousingBalance: Building {buildingId} relocated, marking critical update");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error in OnBuildingRelocated: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Проверить валидность buildingId
        /// </summary>
        private bool IsValidBuildingId(ushort buildingId)
        {
            if (buildingId == 0) return false;
            
            try
            {
                var buildingManager = BuildingManager.instance;
                if (buildingManager == null) return false;
                
                var buildings = buildingManager.m_buildings;
                if (buildings == null || buildingId >= buildings.m_size) return false;
                
                var building = buildings.m_buffer[buildingId];
                
                // Проверяем основные флаги существования
                if ((building.m_flags & Building.Flags.Created) == 0) return false;
                if ((building.m_flags & Building.Flags.Deleted) != 0) return false;
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error validating buildingId {buildingId}: {ex.Message}");
                return false;
            }
        }
        
        #endregion
    }
}
