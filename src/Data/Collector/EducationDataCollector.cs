using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ColossalFramework;
using JobsHousingBalance.Config;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Сборщик данных об образовании жителей
    /// Собирает данные о том, сколько жителей имеют определенный уровень образования
    /// </summary>
    public class EducationDataCollector
    {
        #region Fields
        
        private Dictionary<ushort, EducationData> _educationCache;
        private AppState _appState;
        private int _lastCacheInvalidationFrame;
        
        // Константы для производительности
        private const int CacheInvalidationIntervalFrames = 512; // Инвалидация кэша каждые ~10-20 секунд
        private const int MaxCacheSize = 10000; // Максимальный размер кэша
        
        #endregion
        
        #region Nested Types
        
        /// <summary>
        /// Структура для хранения данных об образовании жителей
        /// </summary>
        public struct EducationData
        {
            public int[] residentsByEdu; // Размер 4: uneducated, educated, well-educated, highly-educated
            public int lastUpdatedFrame;
            public bool isValid;
            
            public EducationData(int[] byEdu, int frame)
            {
                residentsByEdu = byEdu ?? new int[4];
                lastUpdatedFrame = frame;
                isValid = true;
            }
        }
        
        #endregion
        
        #region Constructor
        
        public EducationDataCollector()
        {
            _educationCache = new Dictionary<ushort, EducationData>();
            _lastCacheInvalidationFrame = 0;
            _appState = AppState.Instance;
            
            Debug.Log("JobsHousingBalance: EducationDataCollector initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Получить данные об образовании жителей для здания
        /// </summary>
        public EducationData GetEducationData(ushort buildingId, Building building, BuildingInfo info, CitizenUnit[] citizenUnits)
        {
            // Проверяем кэш
            if (_educationCache.TryGetValue(buildingId, out var cachedData) && 
                IsCacheValid(cachedData))
            {
                return cachedData;
            }
            
            // Собираем новые данные
            var educationData = CollectEducationData(buildingId, building, info, citizenUnits);
            
            // Обновляем кэш
            UpdateCache(buildingId, educationData);
            
            return educationData;
        }
        
        /// <summary>
        /// Принудительно инвалидировать кэш
        /// </summary>
        public void InvalidateCache()
        {
            _educationCache.Clear();
            _lastCacheInvalidationFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            Debug.Log("JobsHousingBalance: Education cache invalidated");
        }
        
        /// <summary>
        /// Проверить, нужно ли обновление кэша
        /// </summary>
        public bool NeedsCacheUpdate()
        {
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            return (currentFrame - _lastCacheInvalidationFrame) > CacheInvalidationIntervalFrames;
        }
        
        /// <summary>
        /// Обновить кэш, если необходимо
        /// </summary>
        public void UpdateCacheIfNeeded()
        {
            if (NeedsCacheUpdate())
            {
                InvalidateCache();
            }
        }
        
        /// <summary>
        /// Получить статистику кэша для отладки
        /// </summary>
        public string GetCacheStats()
        {
            var validEntries = 0;
            var totalResidents = 0;
            
            foreach (var entry in _educationCache.Values)
            {
                if (entry.isValid && entry.residentsByEdu != null)
                {
                    validEntries++;
                    totalResidents += entry.residentsByEdu[0] + entry.residentsByEdu[1] + 
                                     entry.residentsByEdu[2] + entry.residentsByEdu[3];
                }
            }
            
            return $"Education Cache: {validEntries}/{_educationCache.Count} valid entries, " +
                   $"Total residents: {totalResidents}";
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Собрать данные об образовании жителей для конкретного здания
        /// </summary>
        private EducationData CollectEducationData(ushort buildingId, Building building, 
                                                  BuildingInfo info, CitizenUnit[] citizenUnits)
        {
            var residentsByEdu = new int[4]; // uneducated, educated, well-educated, highly-educated
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            
            if (info == null || info.GetService() != ItemClass.Service.Residential)
            {
                return new EducationData(residentsByEdu, currentFrame);
            }
            
            // Проходим по связанному списку CitizenUnits для жилых зданий
            var unitId = building.m_citizenUnits;
            var visitedUnits = new System.Collections.Generic.HashSet<uint>();
            var maxIterations = 1000; // Защита от бесконечного цикла
            var iterationCount = 0;
            
            while (unitId != 0 && iterationCount < maxIterations)
            {
                iterationCount++;
                
                // Проверяем на циклические ссылки
                if (!visitedUnits.Add(unitId))
                {
                    Debug.LogWarning($"JobsHousingBalance: Circular reference detected in CitizenUnits chain for building {buildingId}");
                    break;
                }
                
                // Проверяем, что unitId не превышает размер массива
                if (unitId >= citizenUnits.Length)
                {
                    Debug.LogWarning($"JobsHousingBalance: CitizenUnit ID {unitId} exceeds array bounds ({citizenUnits.Length})");
                    break;
                }
                
                var citizenUnit = citizenUnits[unitId];
                
                // Считаем жителей только для Home-юнитов
                if ((citizenUnit.m_flags & CitizenUnit.Flags.Home) != 0)
                {
                    CountResidentsByEducation(citizenUnit, residentsByEdu);
                }
                
                // Переходим к следующему юниту в списке
                unitId = citizenUnit.m_nextUnit;
            }
            
            Debug.Log($"JobsHousingBalance: Building {buildingId} education - " +
                     $"Uneducated: {residentsByEdu[0]}, Educated: {residentsByEdu[1]}, " +
                     $"Well-educated: {residentsByEdu[2]}, Highly-educated: {residentsByEdu[3]}");
            
            return new EducationData(residentsByEdu, currentFrame);
        }
        
        /// <summary>
        /// Подсчитать жителей по уровням образования в CitizenUnit
        /// </summary>
        private void CountResidentsByEducation(CitizenUnit citizenUnit, int[] residentsByEdu)
        {
            var citizenManager = CitizenManager.instance;
            if (citizenManager == null) return;
            
            var citizens = citizenManager.m_citizens;
            
            // Проверяем каждый слот (до 5 слотов на юнит)
            for (int i = 0; i < 5; i++)
            {
                uint citizenId = 0;
                
                switch (i)
                {
                    case 0: citizenId = citizenUnit.m_citizen0; break;
                    case 1: citizenId = citizenUnit.m_citizen1; break;
                    case 2: citizenId = citizenUnit.m_citizen2; break;
                    case 3: citizenId = citizenUnit.m_citizen3; break;
                    case 4: citizenId = citizenUnit.m_citizen4; break;
                }
                
                if (citizenId != 0)
                {
                    // Проверяем, что citizenId не превышает размер массива
                    if (citizenId >= citizens.m_size)
                    {
                        Debug.LogWarning($"JobsHousingBalance: Citizen ID {citizenId} exceeds array bounds ({citizens.m_size})");
                        continue;
                    }
                    
                    var citizen = citizens.m_buffer[citizenId];
                    
                    // Проверяем, что гражданин существует
                    if ((citizen.m_flags & Citizen.Flags.Created) != 0)
                    {
                        // Определяем уровень образования гражданина
                        var educationLevel = CitizenEducationHelper.GetCitizenEducationLevel(citizen);
                        
                        // Учитываем работоспособный возраст
                        if (CitizenEducationHelper.IsWorkingAge(citizen, _appState?.IncludeTeens ?? false))
                        {
                            residentsByEdu[educationLevel]++;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Проверить, актуален ли кэш
        /// </summary>
        private bool IsCacheValid(EducationData data)
        {
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            return data.isValid && (currentFrame - data.lastUpdatedFrame) < CacheInvalidationIntervalFrames;
        }
        
        /// <summary>
        /// Обновить кэш
        /// </summary>
        private void UpdateCache(ushort buildingId, EducationData data)
        {
            // Ограничиваем размер кэша
            if (_educationCache.Count >= MaxCacheSize)
            {
                // Удаляем самые старые записи
                var oldestKey = (ushort)0;
                var oldestFrame = int.MaxValue;
                
                foreach (var kvp in _educationCache)
                {
                    if (kvp.Value.lastUpdatedFrame < oldestFrame)
                    {
                        oldestFrame = kvp.Value.lastUpdatedFrame;
                        oldestKey = kvp.Key;
                    }
                }
                
                _educationCache.Remove(oldestKey);
            }
            
            _educationCache[buildingId] = data;
        }
        
        #endregion
    }
}
