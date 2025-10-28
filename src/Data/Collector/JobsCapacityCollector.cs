using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ColossalFramework;
using JobsHousingBalance.Config;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Сборщик данных о емкости рабочих мест из BuildingAI
    /// Собирает данные о том, сколько рабочих мест может предоставить здание
    /// и какие требования к образованию для этих рабочих мест
    /// </summary>
    public class JobsCapacityCollector
    {
        #region Fields
        
        private Dictionary<ushort, CapacityData> _capacityCache;
        private bool _isRP2Detected;
        private bool _isRP2DetectionChecked;
        private int _lastCacheInvalidationFrame;
        private AppState _appState;
        
        // Константы для производительности
        private const int CacheInvalidationIntervalFrames = 512; // Инвалидация кэша каждые ~10-20 секунд
        private const int MaxCacheSize = 10000; // Максимальный размер кэша
        
        #endregion
        
        #region Nested Types
        
        /// <summary>
        /// Структура для хранения данных о емкости рабочих мест
        /// </summary>
        public struct CapacityData
        {
            public int jobsCapacityTotal;
            public int[] jobsCapacityByEdu; // Размер 4: uneducated, educated, well-educated, highly-educated
            public bool isServiceBuilding;
            public bool isUniqueBuilding;
            public int lastUpdatedFrame;
            public bool isValid;
            
            public CapacityData(int total, int[] byEdu, bool isService, bool isUnique, int frame)
            {
                jobsCapacityTotal = total;
                jobsCapacityByEdu = byEdu ?? new int[4];
                isServiceBuilding = isService;
                isUniqueBuilding = isUnique;
                lastUpdatedFrame = frame;
                isValid = true;
            }
        }
        
        #endregion
        
        #region Constructor
        
        public JobsCapacityCollector()
        {
            _capacityCache = new Dictionary<ushort, CapacityData>();
            _isRP2Detected = false;
            _isRP2DetectionChecked = false;
            _lastCacheInvalidationFrame = 0;
            _appState = AppState.Instance;
            
            Debug.Log("JobsHousingBalance: JobsCapacityCollector initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Получить данные о емкости рабочих мест для здания
        /// </summary>
        public CapacityData GetCapacityData(ushort buildingId, Building building, BuildingInfo info)
        {
            // Проверяем кэш
            if (_capacityCache.TryGetValue(buildingId, out var cachedData) && 
                IsCacheValid(cachedData))
            {
                return cachedData;
            }
            
            // Собираем новые данные
            var capacityData = CollectCapacityData(buildingId, building, info);
            
            // Обновляем кэш
            UpdateCache(buildingId, capacityData);
            
            return capacityData;
        }
        
        /// <summary>
        /// Проверить, активен ли Realistic Population 2 мод
        /// </summary>
        public bool IsRP2Active()
        {
            if (!_isRP2DetectionChecked)
            {
                DetectRP2Mod();
                _isRP2DetectionChecked = true;
            }
            return _isRP2Detected;
        }
        
        /// <summary>
        /// Принудительно инвалидировать кэш
        /// </summary>
        public void InvalidateCache()
        {
            _capacityCache.Clear();
            _lastCacheInvalidationFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            Debug.Log("JobsHousingBalance: Capacity cache invalidated");
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
            var totalCapacity = 0;
            
            foreach (var entry in _capacityCache.Values)
            {
                if (entry.isValid)
                {
                    validEntries++;
                    totalCapacity += entry.jobsCapacityTotal;
                }
            }
            
            return $"Capacity Cache: {validEntries}/{_capacityCache.Count} valid entries, " +
                   $"Total capacity: {totalCapacity}, RP2: {(IsRP2Active() ? "Detected" : "Not detected")}";
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Собрать данные о емкости рабочих мест для конкретного здания
        /// </summary>
        private CapacityData CollectCapacityData(ushort buildingId, Building building, BuildingInfo info)
        {
            if (info == null)
            {
                Debug.LogWarning($"JobsHousingBalance: Building {buildingId} has null Info, returning empty capacity");
                return new CapacityData(0, new int[4], false, false, 
                    (int)SimulationManager.instance.m_currentFrameIndex);
            }
            
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            
            // Определяем тип здания
            var isServiceBuilding = IsServiceBuilding(info);
            var isUniqueBuilding = IsUniqueBuilding(info);
            
            // Получаем данные о емкости рабочих мест
            var capacityTotal = GetJobsCapacityTotal(info, building, buildingId);
            var capacityByEdu = GetJobsCapacityByEducation(info, building, buildingId);
            
            Debug.Log($"JobsHousingBalance: Building {buildingId} capacity - Total: {capacityTotal}, " +
                     $"ByEdu: [{capacityByEdu[0]},{capacityByEdu[1]},{capacityByEdu[2]},{capacityByEdu[3]}], " +
                     $"Service: {isServiceBuilding}, Unique: {isUniqueBuilding}");
            
            return new CapacityData(capacityTotal, capacityByEdu, isServiceBuilding, isUniqueBuilding, currentFrame);
        }
        
        /// <summary>
        /// Получить общую емкость рабочих мест здания
        /// Использует реальные API методы CalculateWorkplaceCount для точных данных
        /// </summary>
        private int GetJobsCapacityTotal(BuildingInfo info, Building building, ushort buildingId)
        {
            if (info == null) return 0;
            
            // Проверяем, является ли здание жилым
            if (info.GetService() == ItemClass.Service.Residential)
            {
                return 0; // Жилые здания не предоставляют рабочие места
            }
            
            // Получаем AI здания
            var prefabAI = info.GetAI();
            if (prefabAI == null) return 0;
            
            // Используем реальные API методы для получения точных данных
            if (prefabAI is CommercialBuildingAI commercialAI)
            {
                return GetCommercialJobsCapacity(commercialAI, info, building, buildingId);
            }
            else if (prefabAI is IndustrialBuildingAI industrialAI)
            {
                return GetIndustrialJobsCapacity(industrialAI, info, building, buildingId);
            }
            else if (prefabAI is IndustrialExtractorAI extractorAI)
            {
                return GetIndustrialExtractorJobsCapacity(extractorAI, info, building, buildingId);
            }
            else if (prefabAI is OfficeBuildingAI officeAI)
            {
                return GetOfficeJobsCapacity(officeAI, info, building, buildingId);
            }
            else if (IsServiceBuilding(info) || IsUniqueBuilding(info))
            {
                // Сервисные/уникальные здания исключаем из баланса, если не включены в настройках
                if (_appState != null && !_appState.IncludeServiceUnique)
                {
                    return 0;
                }
                
                // Если включены в настройках, пытаемся получить capacity
                return GetServiceOrUniqueBuildingCapacity(prefabAI, info, building, buildingId);
            }
            
            // Для других типов зданий возвращаем 0
            return 0;
        }
        
        /// <summary>
        /// Получить емкость рабочих мест по уровням образования
        /// Использует реальные API методы CalculateWorkplaceCount для точных данных
        /// </summary>
        private int[] GetJobsCapacityByEducation(BuildingInfo info, Building building, ushort buildingId)
        {
            var capacityByEdu = new int[4]; // uneducated, educated, well-educated, highly-educated
            
            if (info == null) return capacityByEdu;
            
            // Проверяем, является ли здание жилым
            if (info.GetService() == ItemClass.Service.Residential)
            {
                return capacityByEdu; // Жилые здания не предоставляют рабочие места
            }
            
            var prefabAI = info.GetAI();
            if (prefabAI == null) return capacityByEdu;
            
            // Используем реальные API методы для получения точных данных
            if (prefabAI is CommercialBuildingAI commercialAI)
            {
                GetCommercialJobsCapacityByEducation(commercialAI, info, building, buildingId, capacityByEdu);
            }
            else if (prefabAI is IndustrialBuildingAI industrialAI)
            {
                GetIndustrialJobsCapacityByEducation(industrialAI, info, building, buildingId, capacityByEdu);
            }
            else if (prefabAI is IndustrialExtractorAI extractorAI)
            {
                GetIndustrialExtractorJobsCapacityByEducation(extractorAI, info, building, buildingId, capacityByEdu);
            }
            else if (prefabAI is OfficeBuildingAI officeAI)
            {
                GetOfficeJobsCapacityByEducation(officeAI, info, building, buildingId, capacityByEdu);
            }
            else if (IsServiceBuilding(info) || IsUniqueBuilding(info))
            {
                // Сервисные/уникальные здания исключаем из баланса, если не включены в настройках
                if (_appState != null && !_appState.IncludeServiceUnique)
                {
                    return capacityByEdu;
                }
                
                // Если включены в настройках, пытаемся получить capacity по образованию
                GetServiceOrUniqueBuildingCapacityByEducation(prefabAI, info, building, buildingId, capacityByEdu);
            }
            
            return capacityByEdu;
        }
        
        /// <summary>
        /// Получить емкость рабочих мест для коммерческих зданий
        /// Использует реальный API метод CalculateWorkplaceCount
        /// </summary>
        private int GetCommercialJobsCapacity(CommercialBuildingAI commercialAI, BuildingInfo info, Building building, ushort buildingId)
        {
            try
            {
                // Получаем реальный уровень здания и приводим к ItemClass.Level
                var level = (ItemClass.Level)building.m_level;
                
                // Создаем детерминированный рандом на основе ID здания
                var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                
                // Получаем размеры лота в клетках
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                
                // Вызываем реальный API метод для получения распределения по образованию
                int level0, level1, level2, level3;
                commercialAI.CalculateWorkplaceCount(level, randomizer, width, length, 
                    out level0, out level1, out level2, out level3);
                
                // Валидация данных - проверяем на отрицательные значения
                if (level0 < 0 || level1 < 0 || level2 < 0 || level3 < 0)
                {
                    Debug.LogWarning($"JobsHousingBalance: Negative workplace count for commercial building {buildingId}: [{level0},{level1},{level2},{level3}]");
                    return 0;
                }
                
                // Возвращаем общую емкость
                return level0 + level1 + level2 + level3;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating commercial workplace count for building {buildingId}: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Получить емкость рабочих мест для промышленных зданий
        /// Использует реальный API метод CalculateWorkplaceCount
        /// </summary>
        private int GetIndustrialJobsCapacity(IndustrialBuildingAI industrialAI, BuildingInfo info, Building building, ushort buildingId)
        {
            try
            {
                // Получаем реальный уровень здания и приводим к ItemClass.Level
                var level = (ItemClass.Level)building.m_level;
                
                // Создаем детерминированный рандом на основе ID здания
                var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                
                // Получаем размеры лота в клетках
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                
                // Вызываем реальный API метод для получения распределения по образованию
                int level0, level1, level2, level3;
                industrialAI.CalculateWorkplaceCount(level, randomizer, width, length, 
                    out level0, out level1, out level2, out level3);
                
                // Валидация данных - проверяем на отрицательные значения
                if (level0 < 0 || level1 < 0 || level2 < 0 || level3 < 0)
                {
                    Debug.LogWarning($"JobsHousingBalance: Negative workplace count for industrial building {buildingId}: [{level0},{level1},{level2},{level3}]");
                    return 0;
                }
                
                // Возвращаем общую емкость
                return level0 + level1 + level2 + level3;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating industrial workplace count for building {buildingId}: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Получить емкость рабочих мест для добывающих промышленных зданий (Industries DLC)
        /// Использует реальный API метод CalculateWorkplaceCount
        /// </summary>
        private int GetIndustrialExtractorJobsCapacity(IndustrialExtractorAI extractorAI, BuildingInfo info, Building building, ushort buildingId)
        {
            try
            {
                // Получаем реальный уровень здания и приводим к ItemClass.Level
                var level = (ItemClass.Level)building.m_level;
                
                // Создаем детерминированный рандом на основе ID здания
                var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                
                // Получаем размеры лота в клетках
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                
                // Вызываем реальный API метод для получения распределения по образованию
                int level0, level1, level2, level3;
                extractorAI.CalculateWorkplaceCount(level, randomizer, width, length, 
                    out level0, out level1, out level2, out level3);
                
                // Валидация данных - проверяем на отрицательные значения
                if (level0 < 0 || level1 < 0 || level2 < 0 || level3 < 0)
                {
                    Debug.LogWarning($"JobsHousingBalance: Negative workplace count for industrial extractor building {buildingId}: [{level0},{level1},{level2},{level3}]");
                    return 0;
                }
                
                // Возвращаем общую емкость
                return level0 + level1 + level2 + level3;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating industrial extractor workplace count for building {buildingId}: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Получить емкость рабочих мест для офисных зданий
        /// Использует реальный API метод CalculateWorkplaceCount
        /// </summary>
        private int GetOfficeJobsCapacity(OfficeBuildingAI officeAI, BuildingInfo info, Building building, ushort buildingId)
        {
            try
            {
                // Получаем реальный уровень здания и приводим к ItemClass.Level
                var level = (ItemClass.Level)building.m_level;
                
                // Создаем детерминированный рандом на основе ID здания
                var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                
                // Получаем размеры лота в клетках
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                
                // Вызываем реальный API метод для получения распределения по образованию
                int level0, level1, level2, level3;
                officeAI.CalculateWorkplaceCount(level, randomizer, width, length, 
                    out level0, out level1, out level2, out level3);
                
                // Валидация данных - проверяем на отрицательные значения
                if (level0 < 0 || level1 < 0 || level2 < 0 || level3 < 0)
                {
                    Debug.LogWarning($"JobsHousingBalance: Negative workplace count for office building {buildingId}: [{level0},{level1},{level2},{level3}]");
                    return 0;
                }
                
                // Возвращаем общую емкость
                return level0 + level1 + level2 + level3;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating office workplace count for building {buildingId}: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Получить емкость рабочих мест по образованию для сервисных/уникальных зданий
        /// Использует стратегию: CalculateWorkplaceCount -> reflection -> эвристика
        /// </summary>
        private void GetServiceOrUniqueBuildingCapacityByEducation(PrefabAI prefabAI, BuildingInfo info, 
            Building building, ushort buildingId, int[] capacityByEdu)
        {
            try
            {
                // Стратегия 1: Попытаться вызвать CalculateWorkplaceCount если есть
                if (TryGetCapacityByEducationFromCalculateWorkplaceCount(prefabAI, building, buildingId, capacityByEdu))
                {
                    Debug.Log($"JobsHousingBalance: Service/Unique building {buildingId} capacity by education from CalculateWorkplaceCount: [{capacityByEdu[0]},{capacityByEdu[1]},{capacityByEdu[2]},{capacityByEdu[3]}]");
                    return;
                }
                
                // Стратегия 2: Reflection по полям AI для получения распределения по образованию
                if (TryGetCapacityByEducationFromReflection(prefabAI, buildingId, capacityByEdu))
                {
                    Debug.Log($"JobsHousingBalance: Service/Unique building {buildingId} capacity by education from reflection: [{capacityByEdu[0]},{capacityByEdu[1]},{capacityByEdu[2]},{capacityByEdu[3]}]");
                    return;
                }
                
                // Стратегия 3: Эвристика по префабу (равномерное распределение)
                GetCapacityByEducationFromHeuristic(info, buildingId, capacityByEdu);
                Debug.Log($"JobsHousingBalance: Service/Unique building {buildingId} capacity by education from heuristic: [{capacityByEdu[0]},{capacityByEdu[1]},{capacityByEdu[2]},{capacityByEdu[3]}] (approx)");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error getting service/unique building capacity by education for building {buildingId}: {ex.Message}");
                // В случае ошибки заполняем нулями
                capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
            }
        }
        
        /// <summary>
        /// Попытаться получить capacity по образованию через CalculateWorkplaceCount
        /// </summary>
        private bool TryGetCapacityByEducationFromCalculateWorkplaceCount(PrefabAI prefabAI, Building building, 
            ushort buildingId, int[] capacityByEdu)
        {
            try
            {
                // Проверяем, есть ли у AI метод CalculateWorkplaceCount
                var method = prefabAI.GetType().GetMethod("CalculateWorkplaceCount", 
                    new System.Type[] { typeof(ItemClass.Level), typeof(ColossalFramework.Math.Randomizer), 
                                      typeof(int), typeof(int), typeof(int).MakeByRefType(), 
                                      typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), 
                                      typeof(int).MakeByRefType() });
                
                if (method != null)
                {
                    var level = (ItemClass.Level)building.m_level;
                    var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                    var width = 1; // Для сервисных зданий используем базовый размер
                    var length = 1;
                    
                    var parameters = new object[] { level, randomizer, width, length, 0, 0, 0, 0 };
                    method.Invoke(prefabAI, parameters);
                    
                    capacityByEdu[0] = (int)parameters[4];
                    capacityByEdu[1] = (int)parameters[5];
                    capacityByEdu[2] = (int)parameters[6];
                    capacityByEdu[3] = (int)parameters[7];
                    
                    if (capacityByEdu[0] >= 0 && capacityByEdu[1] >= 0 && 
                        capacityByEdu[2] >= 0 && capacityByEdu[3] >= 0)
                    {
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calling CalculateWorkplaceCount for service building {buildingId}: {ex.Message}");
            }
            
            return false;
        }
        
        /// <summary>
        /// Попытаться получить capacity по образованию через reflection по полям AI
        /// </summary>
        private bool TryGetCapacityByEducationFromReflection(PrefabAI prefabAI, ushort buildingId, int[] capacityByEdu)
        {
            try
            {
                var aiType = prefabAI.GetType();
                
                // Ищем поля с количеством рабочих мест по уровням образования
                var fields = aiType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                // Ищем поля типа m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3
                for (int i = 0; i < 4; i++)
                {
                    var fieldName = $"m_workPlaceCount{i}";
                    var field = aiType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (field != null && field.FieldType == typeof(int))
                    {
                        capacityByEdu[i] = (int)field.GetValue(prefabAI);
                    }
                    else
                    {
                        // Альтернативные имена полей
                        var altFieldNames = new string[] { 
                            $"m_workplaceCount{i}", 
                            $"m_employeeCount{i}", 
                            $"m_workerCount{i}",
                            $"workPlaceCount{i}",
                            $"workplaceCount{i}"
                        };
                        
                        foreach (var altName in altFieldNames)
                        {
                            field = aiType.GetField(altName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (field != null && field.FieldType == typeof(int))
                            {
                                capacityByEdu[i] = (int)field.GetValue(prefabAI);
                                break;
                            }
                        }
                    }
                }
                
                // Проверяем, получили ли мы хотя бы одно ненулевое значение
                if (capacityByEdu[0] > 0 || capacityByEdu[1] > 0 || capacityByEdu[2] > 0 || capacityByEdu[3] > 0)
                {
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error using reflection for service building {buildingId}: {ex.Message}");
            }
            
            return false;
        }
        
        /// <summary>
        /// Получить capacity по образованию через эвристику (равномерное распределение)
        /// </summary>
        private void GetCapacityByEducationFromHeuristic(BuildingInfo info, ushort buildingId, int[] capacityByEdu)
        {
            try
            {
                // Получаем общую capacity через эвристику
                var totalCapacity = GetCapacityFromHeuristic(info, buildingId);
                
                if (totalCapacity <= 0)
                {
                    capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
                    return;
                }
                
                // Распределяем равномерно по уровням образования
                // Для сервисных зданий обычно требуется более высокое образование
                var service = info.GetService();
                
                switch (service)
                {
                    case ItemClass.Service.Education:
                        // Школы/университеты - требуют высокое образование
                        capacityByEdu[0] = totalCapacity / 8;  // 12.5% uneducated
                        capacityByEdu[1] = totalCapacity / 4;  // 25% educated
                        capacityByEdu[2] = totalCapacity / 2;  // 50% well-educated
                        capacityByEdu[3] = totalCapacity / 8;  // 12.5% highly-educated
                        break;
                        
                    case ItemClass.Service.HealthCare:
                        // Больницы - требуют высокое образование
                        capacityByEdu[0] = totalCapacity / 10; // 10% uneducated
                        capacityByEdu[1] = totalCapacity / 5;  // 20% educated
                        capacityByEdu[2] = totalCapacity / 2;  // 50% well-educated
                        capacityByEdu[3] = totalCapacity / 5;  // 20% highly-educated
                        break;
                        
                    case ItemClass.Service.PoliceDepartment:
                    case ItemClass.Service.FireDepartment:
                        // Полиция/пожарные - среднее образование
                        capacityByEdu[0] = totalCapacity / 4;  // 25% uneducated
                        capacityByEdu[1] = totalCapacity / 2;  // 50% educated
                        capacityByEdu[2] = totalCapacity / 4;  // 25% well-educated
                        capacityByEdu[3] = 0;                 // 0% highly-educated
                        break;
                        
                    default:
                        // Остальные сервисы - равномерное распределение
                        capacityByEdu[0] = totalCapacity / 4;  // 25% uneducated
                        capacityByEdu[1] = totalCapacity / 4;  // 25% educated
                        capacityByEdu[2] = totalCapacity / 4;  // 25% well-educated
                        capacityByEdu[3] = totalCapacity / 4;  // 25% highly-educated
                        break;
                }
                
                // Корректируем округление
                var sum = capacityByEdu[0] + capacityByEdu[1] + capacityByEdu[2] + capacityByEdu[3];
                if (sum != totalCapacity)
                {
                    capacityByEdu[3] += (totalCapacity - sum); // Добавляем разницу к highly-educated
                }
                
                // Убеждаемся, что все значения неотрицательные
                for (int i = 0; i < 4; i++)
                {
                    if (capacityByEdu[i] < 0) capacityByEdu[i] = 0;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating heuristic capacity by education for building {buildingId}: {ex.Message}");
                capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
            }
        }
        
        /// <summary>
        /// Получить емкость рабочих мест для сервисных/уникальных зданий
        /// Использует стратегию: CalculateWorkplaceCount -> reflection -> эвристика
        /// </summary>
        private int GetServiceOrUniqueBuildingCapacity(PrefabAI prefabAI, BuildingInfo info, Building building, ushort buildingId)
        {
            try
            {
                // Стратегия 1: Попытаться вызвать CalculateWorkplaceCount если есть
                var capacityFromMethod = TryGetCapacityFromCalculateWorkplaceCount(prefabAI, building, buildingId);
                if (capacityFromMethod > 0)
                {
                    Debug.Log($"JobsHousingBalance: Service/Unique building {buildingId} capacity from CalculateWorkplaceCount: {capacityFromMethod}");
                    return capacityFromMethod;
                }
                
                // Стратегия 2: Reflection по полям AI
                var capacityFromReflection = TryGetCapacityFromReflection(prefabAI, buildingId);
                if (capacityFromReflection > 0)
                {
                    Debug.Log($"JobsHousingBalance: Service/Unique building {buildingId} capacity from reflection: {capacityFromReflection}");
                    return capacityFromReflection;
                }
                
                // Стратегия 3: Эвристика по префабу (размер здания)
                var capacityFromHeuristic = GetCapacityFromHeuristic(info, buildingId);
                Debug.Log($"JobsHousingBalance: Service/Unique building {buildingId} capacity from heuristic: {capacityFromHeuristic} (approx)");
                return capacityFromHeuristic;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error getting service/unique building capacity for building {buildingId}: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Попытаться получить capacity через CalculateWorkplaceCount
        /// </summary>
        private int TryGetCapacityFromCalculateWorkplaceCount(PrefabAI prefabAI, Building building, ushort buildingId)
        {
            try
            {
                // Проверяем, есть ли у AI метод CalculateWorkplaceCount
                var method = prefabAI.GetType().GetMethod("CalculateWorkplaceCount", 
                    new System.Type[] { typeof(ItemClass.Level), typeof(ColossalFramework.Math.Randomizer), 
                                      typeof(int), typeof(int), typeof(int).MakeByRefType(), 
                                      typeof(int).MakeByRefType(), typeof(int).MakeByRefType(), 
                                      typeof(int).MakeByRefType() });
                
                if (method != null)
                {
                    var level = (ItemClass.Level)building.m_level;
                    var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                    var width = 1; // Для сервисных зданий используем базовый размер
                    var length = 1;
                    
                    var parameters = new object[] { level, randomizer, width, length, 0, 0, 0, 0 };
                    method.Invoke(prefabAI, parameters);
                    
                    var level0 = (int)parameters[4];
                    var level1 = (int)parameters[5];
                    var level2 = (int)parameters[6];
                    var level3 = (int)parameters[7];
                    
                    if (level0 >= 0 && level1 >= 0 && level2 >= 0 && level3 >= 0)
                    {
                        return level0 + level1 + level2 + level3;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calling CalculateWorkplaceCount for service building {buildingId}: {ex.Message}");
            }
            
            return 0;
        }
        
        /// <summary>
        /// Попытаться получить capacity через reflection по полям AI
        /// </summary>
        private int TryGetCapacityFromReflection(PrefabAI prefabAI, ushort buildingId)
        {
            try
            {
                var aiType = prefabAI.GetType();
                
                // Ищем поля с количеством рабочих мест
                var fields = aiType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                foreach (var field in fields)
                {
                    var fieldName = field.Name.ToLower();
                    
                    // Ищем поля типа m_workPlaceCount, m_workplaceCount, m_employeeCount и т.д.
                    if ((fieldName.Contains("workplace") || fieldName.Contains("workplace") || 
                         fieldName.Contains("employee") || fieldName.Contains("worker")) &&
                        field.FieldType == typeof(int))
                    {
                        var value = (int)field.GetValue(prefabAI);
                        if (value > 0)
                        {
                            Debug.Log($"JobsHousingBalance: Found workplace field '{field.Name}' with value {value} for building {buildingId}");
                            return value;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error using reflection for service building {buildingId}: {ex.Message}");
            }
            
            return 0;
        }
        
        /// <summary>
        /// Получить capacity через эвристику по размеру здания
        /// </summary>
        private int GetCapacityFromHeuristic(BuildingInfo info, ushort buildingId)
        {
            try
            {
                // Эвристика: базовое количество рабочих мест на основе размера здания
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                var area = width * length;
                
                // Базовые значения для разных типов сервисных зданий
                var service = info.GetService();
                var subService = info.GetSubService();
                
                int baseCapacity = 0;
                
                switch (service)
                {
                    case ItemClass.Service.Education:
                        baseCapacity = area * 2; // Школы/университеты
                        break;
                    case ItemClass.Service.HealthCare:
                        baseCapacity = area * 3; // Больницы/клиники
                        break;
                    case ItemClass.Service.PoliceDepartment:
                        baseCapacity = area * 2; // Полиция
                        break;
                    case ItemClass.Service.FireDepartment:
                        baseCapacity = area * 2; // Пожарные
                        break;
                    case ItemClass.Service.Garbage:
                        baseCapacity = area * 1; // Мусор
                        break;
                    case ItemClass.Service.Water:
                        baseCapacity = area * 1; // Вода
                        break;
                    case ItemClass.Service.Electricity:
                        baseCapacity = area * 2; // Электричество
                        break;
                    case ItemClass.Service.PublicTransport:
                        baseCapacity = area * 1; // Транспорт
                        break;
                    case ItemClass.Service.Monument:
                        baseCapacity = Math.Max(1, area / 4); // Монументы
                        break;
                    case ItemClass.Service.Beautification:
                        baseCapacity = Math.Max(1, area / 8); // Красота
                        break;
                    case ItemClass.Service.Tourism:
                        baseCapacity = area * 2; // Туризм
                        break;
                    default:
                        baseCapacity = Math.Max(1, area / 2); // По умолчанию
                        break;
                }
                
                // Ограничиваем разумными пределами
                return Math.Max(1, Math.Min(baseCapacity, 50));
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating heuristic capacity for building {buildingId}: {ex.Message}");
                return 1; // Минимальное значение
            }
        }
        
        /// <summary>
        /// Получить фактическую занятость по уровням образования для здания
        /// Собирает данные через CitizenUnit с флагом Work
        /// </summary>
        public int[] GetJobsOccupancyByEducation(ushort buildingId, Building building, BuildingInfo info, CitizenUnit[] citizenUnits)
        {
            var occupancyByEdu = new int[4]; // uneducated, educated, well-educated, highly-educated
            
            if (info == null || info.GetService() == ItemClass.Service.Residential)
            {
                return occupancyByEdu; // Жилые здания не предоставляют рабочие места
            }
            
            try
            {
                // Проходим по связанному списку CitizenUnits для рабочих мест
                var unitId = building.m_citizenUnits;
                var visitedUnits = new System.Collections.Generic.HashSet<uint>();
                var maxIterations = 1000; // Защита от бесконечного цикла
                var iterationCount = 0;
                
                while (unitId != 0 && iterationCount < maxIterations)
                {
                    iterationCount++;
                    
                    if (visitedUnits.Contains(unitId))
                    {
                        Debug.LogWarning($"JobsHousingBalance: Circular reference detected in citizen units for building {buildingId}");
                        break;
                    }
                    visitedUnits.Add(unitId);
                    
                    if (unitId >= citizenUnits.Length)
                    {
                        Debug.LogWarning($"JobsHousingBalance: CitizenUnit ID {unitId} exceeds array bounds ({citizenUnits.Length})");
                        break;
                    }
                    
                    var unit = citizenUnits[unitId];
                    
                    // Проверяем, что юнит существует и имеет флаг Work
                    if ((unit.m_flags & CitizenUnit.Flags.Work) != 0)
                    {
                        // Проходим по всем гражданам в юните (до 5 граждан)
                        for (int i = 0; i < 5; i++)
                        {
                            var citizenId = unit.GetCitizen(i);
                            if (citizenId != 0)
                            {
                                // Проверяем, что citizenId не превышает размер массива
                                var citizens = CitizenManager.instance.m_citizens;
                                if (citizenId >= citizens.m_size)
                                {
                                    Debug.LogWarning($"JobsHousingBalance: Citizen ID {citizenId} exceeds array bounds ({citizens.m_size})");
                                    continue;
                                }
                                
                                var citizen = citizens.m_buffer[citizenId];
                                
                                // Проверяем, что гражданин существует и работает в этом здании
                                if ((citizen.m_flags & Citizen.Flags.Created) != 0)
                                {
                                // Проверяем, что гражданин действительно работает в этом здании
                                var workBuildingId = CitizenEducationHelper.GetCitizenWorkBuilding(citizen);
                                if (workBuildingId == buildingId)
                                {
                                    // Определяем уровень образования гражданина
                                    var educationLevel = CitizenEducationHelper.GetCitizenEducationLevel(citizen);
                                    if (educationLevel >= 0 && educationLevel <= 3)
                                    {
                                        occupancyByEdu[educationLevel]++;
                                    }
                                }
                                }
                            }
                        }
                    }
                    
                    unitId = unit.m_nextUnit;
                }
                
                Debug.Log($"JobsHousingBalance: Building {buildingId} occupancy by education: [{occupancyByEdu[0]},{occupancyByEdu[1]},{occupancyByEdu[2]},{occupancyByEdu[3]}]");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error collecting occupancy by education for building {buildingId}: {ex.Message}");
            }
            
            return occupancyByEdu;
        }
        
        /// <summary>
        /// Получить емкость рабочих мест по образованию для коммерческих зданий
        /// Использует реальный API метод CalculateWorkplaceCount
        /// </summary>
        private void GetCommercialJobsCapacityByEducation(CommercialBuildingAI commercialAI, BuildingInfo info, 
            Building building, ushort buildingId, int[] capacityByEdu)
        {
            try
            {
                // Получаем реальный уровень здания и приводим к ItemClass.Level
                var level = (ItemClass.Level)building.m_level;
                
                // Создаем детерминированный рандом на основе ID здания
                var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                
                // Получаем размеры лота в клетках
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                
                // Вызываем реальный API метод для получения распределения по образованию
                commercialAI.CalculateWorkplaceCount(level, randomizer, width, length, 
                    out capacityByEdu[0], out capacityByEdu[1], out capacityByEdu[2], out capacityByEdu[3]);
                
                // Валидация данных - проверяем на отрицательные значения
                if (capacityByEdu[0] < 0 || capacityByEdu[1] < 0 || capacityByEdu[2] < 0 || capacityByEdu[3] < 0)
                {
                    Debug.LogWarning($"JobsHousingBalance: Negative workplace education count for commercial building {buildingId}: [{capacityByEdu[0]},{capacityByEdu[1]},{capacityByEdu[2]},{capacityByEdu[3]}]");
                    capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating commercial workplace education for building {buildingId}: {ex.Message}");
                // В случае ошибки заполняем нулями
                capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
            }
        }
        
        /// <summary>
        /// Получить емкость рабочих мест по образованию для промышленных зданий
        /// Использует реальный API метод CalculateWorkplaceCount
        /// </summary>
        private void GetIndustrialJobsCapacityByEducation(IndustrialBuildingAI industrialAI, BuildingInfo info, 
            Building building, ushort buildingId, int[] capacityByEdu)
        {
            try
            {
                // Получаем реальный уровень здания и приводим к ItemClass.Level
                var level = (ItemClass.Level)building.m_level;
                
                // Создаем детерминированный рандом на основе ID здания
                var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                
                // Получаем размеры лота в клетках
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                
                // Вызываем реальный API метод для получения распределения по образованию
                industrialAI.CalculateWorkplaceCount(level, randomizer, width, length, 
                    out capacityByEdu[0], out capacityByEdu[1], out capacityByEdu[2], out capacityByEdu[3]);
                
                // Валидация данных - проверяем на отрицательные значения
                if (capacityByEdu[0] < 0 || capacityByEdu[1] < 0 || capacityByEdu[2] < 0 || capacityByEdu[3] < 0)
                {
                    Debug.LogWarning($"JobsHousingBalance: Negative workplace education count for industrial building {buildingId}: [{capacityByEdu[0]},{capacityByEdu[1]},{capacityByEdu[2]},{capacityByEdu[3]}]");
                    capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating industrial workplace education for building {buildingId}: {ex.Message}");
                // В случае ошибки заполняем нулями
                capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
            }
        }
        
        /// <summary>
        /// Получить емкость рабочих мест по образованию для добывающих промышленных зданий (Industries DLC)
        /// Использует реальный API метод CalculateWorkplaceCount
        /// </summary>
        private void GetIndustrialExtractorJobsCapacityByEducation(IndustrialExtractorAI extractorAI, BuildingInfo info, 
            Building building, ushort buildingId, int[] capacityByEdu)
        {
            try
            {
                // Получаем реальный уровень здания и приводим к ItemClass.Level
                var level = (ItemClass.Level)building.m_level;
                
                // Создаем детерминированный рандом на основе ID здания
                var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                
                // Получаем размеры лота в клетках
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                
                // Вызываем реальный API метод для получения распределения по образованию
                extractorAI.CalculateWorkplaceCount(level, randomizer, width, length, 
                    out capacityByEdu[0], out capacityByEdu[1], out capacityByEdu[2], out capacityByEdu[3]);
                
                // Валидация данных - проверяем на отрицательные значения
                if (capacityByEdu[0] < 0 || capacityByEdu[1] < 0 || capacityByEdu[2] < 0 || capacityByEdu[3] < 0)
                {
                    Debug.LogWarning($"JobsHousingBalance: Negative workplace education count for industrial extractor building {buildingId}: [{capacityByEdu[0]},{capacityByEdu[1]},{capacityByEdu[2]},{capacityByEdu[3]}]");
                    capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating industrial extractor workplace education for building {buildingId}: {ex.Message}");
                // В случае ошибки заполняем нулями
                capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
            }
        }
        
        /// <summary>
        /// Получить емкость рабочих мест по образованию для офисных зданий
        /// Использует реальный API метод CalculateWorkplaceCount
        /// </summary>
        private void GetOfficeJobsCapacityByEducation(OfficeBuildingAI officeAI, BuildingInfo info, 
            Building building, ushort buildingId, int[] capacityByEdu)
        {
            try
            {
                // Получаем реальный уровень здания и приводим к ItemClass.Level
                var level = (ItemClass.Level)building.m_level;
                
                // Создаем детерминированный рандом на основе ID здания
                var randomizer = new ColossalFramework.Math.Randomizer((uint)buildingId);
                
                // Получаем размеры лота в клетках
                var width = info.m_cellWidth;
                var length = info.m_cellLength;
                
                // Вызываем реальный API метод для получения распределения по образованию
                officeAI.CalculateWorkplaceCount(level, randomizer, width, length, 
                    out capacityByEdu[0], out capacityByEdu[1], out capacityByEdu[2], out capacityByEdu[3]);
                
                // Валидация данных - проверяем на отрицательные значения
                if (capacityByEdu[0] < 0 || capacityByEdu[1] < 0 || capacityByEdu[2] < 0 || capacityByEdu[3] < 0)
                {
                    Debug.LogWarning($"JobsHousingBalance: Negative workplace education count for office building {buildingId}: [{capacityByEdu[0]},{capacityByEdu[1]},{capacityByEdu[2]},{capacityByEdu[3]}]");
                    capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error calculating office workplace education for building {buildingId}: {ex.Message}");
                // В случае ошибки заполняем нулями
                capacityByEdu[0] = capacityByEdu[1] = capacityByEdu[2] = capacityByEdu[3] = 0;
            }
        }
        
        /// <summary>
        /// Проверить, является ли здание сервисным
        /// </summary>
        private bool IsServiceBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            return service == ItemClass.Service.Education ||
                   service == ItemClass.Service.HealthCare ||
                   service == ItemClass.Service.PoliceDepartment ||
                   service == ItemClass.Service.FireDepartment ||
                   service == ItemClass.Service.Garbage ||
                   service == ItemClass.Service.Water ||
                   service == ItemClass.Service.Electricity ||
                   service == ItemClass.Service.PublicTransport ||
                   service == ItemClass.Service.Monument ||
                   service == ItemClass.Service.Beautification ||
                       service == ItemClass.Service.Disaster ||
                       service == ItemClass.Service.Tourism;
        }
        
        /// <summary>
        /// Проверить, является ли здание уникальным
        /// </summary>
        private bool IsUniqueBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            return service == ItemClass.Service.Monument ||
                   service == ItemClass.Service.Beautification ||
                   service == ItemClass.Service.Tourism;
        }
        
        /// <summary>
        /// Обнаружить Realistic Population 2 мод
        /// </summary>
        private void DetectRP2Mod()
        {
            try
            {
                // Проверяем наличие мода Realistic Population 2 по конкретному ID
                var mods = ColossalFramework.Plugins.PluginManager.instance.GetPluginsInfo();
                
                foreach (var mod in mods)
                {
                    if (mod.isEnabled)
                    {
                        // Проверяем по конкретному ID мода RP2 (2025147082)
                        if (mod.publishedFileID.AsUInt64 == 2025147082UL)
                        {
                            _isRP2Detected = true;
                            Debug.Log("JobsHousingBalance: Realistic Population 2 mod detected by ID");
                            return;
                        }
                        
                        // Дополнительная проверка по имени (на случай изменения ID)
                        if (mod.name.Contains("Realistic Population") && 
                            (mod.name.Contains("2") || mod.name.Contains("Revisited")))
                        {
                            _isRP2Detected = true;
                            Debug.Log("JobsHousingBalance: Realistic Population 2 mod detected by name");
                            return;
                        }
                    }
                }
                
                _isRP2Detected = false;
                Debug.Log("JobsHousingBalance: Realistic Population 2 mod not detected");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error detecting RP2 mod: {ex.Message}");
                _isRP2Detected = false;
            }
        }
        
        /// <summary>
        /// Проверить, актуален ли кэш
        /// </summary>
        private bool IsCacheValid(CapacityData data)
        {
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            return data.isValid && (currentFrame - data.lastUpdatedFrame) < CacheInvalidationIntervalFrames;
        }
        
        /// <summary>
        /// Обновить кэш
        /// </summary>
        private void UpdateCache(ushort buildingId, CapacityData data)
        {
            // Ограничиваем размер кэша
            if (_capacityCache.Count >= MaxCacheSize)
            {
                // Удаляем самые старые записи
                var oldestKey = (ushort)0;
                var oldestFrame = int.MaxValue;
                
                foreach (var kvp in _capacityCache)
                {
                    if (kvp.Value.lastUpdatedFrame < oldestFrame)
                    {
                        oldestFrame = kvp.Value.lastUpdatedFrame;
                        oldestKey = kvp.Key;
                    }
                }
                
                _capacityCache.Remove(oldestKey);
            }
            
            _capacityCache[buildingId] = data;
        }
        
        #endregion
    }
}