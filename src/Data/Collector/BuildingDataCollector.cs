using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColossalFramework;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Сборщик данных о зданиях и их жителях/рабочих местах
    /// Собирает фактические данные из CitizenUnits для совместимости с Realistic Population
    /// </summary>
    public class BuildingDataCollector
    {
        #region Fields
        
        private List<BuildingSample> _buildingSamples;
        private bool _isDataValid;
        private int _lastUpdatedFrame;
        private int _totalResidents;
        private int _totalJobs;
        private JobsCapacityCollector _capacityCollector;
        private EducationDataCollector _educationCollector;
        
        // Константы для производительности
        private const int UpdateIntervalFrames = 256; // Обновление каждые ~5-10 секунд
        private const int MaxBuildingsPerFrame = 50; // Максимум зданий для обработки за кадр
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Получить список сэмплов зданий
        /// </summary>
        public List<BuildingSample> BuildingSamples
        {
            get
            {
                if (!_isDataValid)
                {
                    RefreshData();
                }
                return _buildingSamples ?? new List<BuildingSample>();
            }
        }
        
        /// <summary>
        /// Проверить, актуальны ли данные
        /// </summary>
        public bool IsDataValid => _isDataValid;
        
        /// <summary>
        /// Получить общее количество жителей
        /// </summary>
        public int TotalResidents
        {
            get
            {
                if (!_isDataValid) RefreshData();
                return _totalResidents;
            }
        }
        
        /// <summary>
        /// Получить общее количество рабочих мест
        /// </summary>
        public int TotalJobs
        {
            get
            {
                if (!_isDataValid) RefreshData();
                return _totalJobs;
            }
        }
        
        /// <summary>
        /// Получить общий баланс (jobs - residents)
        /// </summary>
        public int TotalBalance => TotalJobs - TotalResidents;
        
        #endregion
        
        #region Constructor
        
        public BuildingDataCollector()
        {
            _buildingSamples = new List<BuildingSample>();
            _isDataValid = false;
            _lastUpdatedFrame = 0;
            _totalResidents = 0;
            _totalJobs = 0;
            _capacityCollector = new JobsCapacityCollector();
            _educationCollector = new EducationDataCollector();
            
            Debug.Log("JobsHousingBalance: BuildingDataCollector initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Принудительно обновить данные
        /// </summary>
        public void ForceRefresh()
        {
            _isDataValid = false;
            RefreshData();
        }
        
        /// <summary>
        /// Проверить, нужно ли обновление данных
        /// </summary>
        public bool NeedsUpdate()
        {
            if (!_isDataValid) return true;
            
            var currentFrame = SimulationManager.instance.m_currentFrameIndex;
            return (currentFrame - _lastUpdatedFrame) > UpdateIntervalFrames;
        }
        
        /// <summary>
        /// Обновить данные, если необходимо
        /// </summary>
        public void UpdateIfNeeded()
        {
            if (NeedsUpdate())
            {
                RefreshData();
            }
        }
        
        /// <summary>
        /// Получить сэмплы зданий для конкретного района
        /// </summary>
        public List<BuildingSample> GetSamplesForDistrict(byte districtId)
        {
            return BuildingSamples.Where(sample => sample.districtId == districtId).ToList();
        }
        
        /// <summary>
        /// Получить сэмплы жилых зданий
        /// </summary>
        public List<BuildingSample> GetResidentialSamples()
        {
            return BuildingSamples.Where(sample => sample.IsResidential).ToList();
        }
        
        /// <summary>
        /// Получить сэмплы нежилых зданий
        /// </summary>
        public List<BuildingSample> GetNonResidentialSamples()
        {
            return BuildingSamples.Where(sample => sample.IsNonResidential).ToList();
        }
        
        /// <summary>
        /// Получить статистику для отладки
        /// </summary>
        public string GetDebugStats()
        {
            var samples = BuildingSamples;
            var residential = samples.Count(s => s.IsResidential);
            var nonResidential = samples.Count(s => s.IsNonResidential);
            var withBalance = samples.Count(s => s.HasSignificantBalance);
            var withCapacity = samples.Count(s => s.HasCapacityData);
            
            return $"Buildings: {samples.Count} total ({residential} residential, {nonResidential} non-residential), " +
                   $"With balance: {withBalance}, With capacity: {withCapacity}, Residents: {_totalResidents}, Jobs: {_totalJobs}, " +
                   $"Balance: {TotalBalance}, Last update: frame {_lastUpdatedFrame}, " +
                   $"RP2: {(_capacityCollector.IsRP2Active() ? "Detected" : "Not detected")}, " +
                   $"Education cache: {_educationCollector.GetCacheStats()}";
        }
        
        /// <summary>
        /// Получить информацию о Realistic Population 2 моде
        /// </summary>
        public bool IsRP2Active()
        {
            return _capacityCollector.IsRP2Active();
        }
        
        /// <summary>
        /// Получить статистику кэша емкости
        /// </summary>
        public string GetCapacityCacheStats()
        {
            return _capacityCollector.GetCacheStats();
        }
        
        /// <summary>
        /// Получить статистику кэша образования
        /// </summary>
        public string GetEducationCacheStats()
        {
            return _educationCollector.GetCacheStats();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Обновить данные о зданиях
        /// </summary>
        private void RefreshData()
        {
            try
            {
                var startTime = System.DateTime.Now;
                var buildingManager = BuildingManager.instance;
                var citizenManager = CitizenManager.instance;
                var districtManager = DistrictManager.instance;
                
                if (buildingManager == null || citizenManager == null || districtManager == null)
                {
                    Debug.LogError("JobsHousingBalance: Required managers are null");
                    return;
                }
                
                var buildings = buildingManager.m_buildings;
                var citizens = citizenManager.m_citizens;
                var citizenUnits = citizenManager.m_units;
                
                // Получаем размеры массивов в рантайме (совместимость с More CitizenUnits)
                var buildingCount = buildings.m_size;
                var citizenUnitCount = citizenUnits.m_size;
                
                Debug.Log($"JobsHousingBalance: Starting data collection - Buildings: {buildingCount}, CitizenUnits: {citizenUnitCount}");
                
                // Обновляем кэши, если необходимо
                _capacityCollector.UpdateCacheIfNeeded();
                _educationCollector.UpdateCacheIfNeeded();
                
                _buildingSamples.Clear();
                _totalResidents = 0;
                _totalJobs = 0;
                
                var processedBuildings = 0;
                var skippedBuildings = 0;
                
                // Итерация по всем зданиям с чанковой обработкой
                for (ushort buildingId = 1; buildingId < buildingCount; buildingId++)
                {
                    var building = buildings.m_buffer[buildingId];
                    
                    // Пропускаем невалидные здания
                    if (!IsValidBuilding(building))
                    {
                        skippedBuildings++;
                        continue;
                    }
                    
                    // Получаем данные о здании
                    var sample = CollectBuildingData(buildingId, building, citizenUnits.m_buffer, districtManager);
                    _buildingSamples.Add(sample);
                    
                    _totalResidents += sample.residentsFact;
                    _totalJobs += sample.jobsFact;
                    
                    processedBuildings++;
                    
                    // Чанковая обработка для производительности - каждые 50 зданий
                    if (processedBuildings % MaxBuildingsPerFrame == 0)
                    {
                        // В реальной реализации здесь можно было бы использовать корутины
                        // или разбить на несколько кадров, но для MVP делаем все сразу
                        // В будущем можно добавить yield return null для пропуска кадров
                    }
                }
                
                _isDataValid = true;
                _lastUpdatedFrame = (int)SimulationManager.instance.m_currentFrameIndex;
                
                var duration = System.DateTime.Now - startTime;
                Debug.Log($"JobsHousingBalance: Data collection completed in {duration.TotalMilliseconds:F1}ms. " +
                         $"Processed: {processedBuildings}, Skipped: {skippedBuildings}, " +
                         $"Total residents: {_totalResidents}, Total jobs: {_totalJobs}");
                
                // Диагностика типов зданий
                LogBuildingTypeDiagnostics();
                
                Debug.Log($"JobsHousingBalance: Debug stats - {GetDebugStats()}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error during data collection: {ex.Message}\n{ex.StackTrace}");
                _isDataValid = false;
            }
        }
        
        /// <summary>
        /// Проверить, является ли здание валидным для обработки
        /// Фильтрует только RICO здания (Residential, Commercial, Industrial, Office)
        /// </summary>
        private bool IsValidBuilding(Building building)
        {
            // Проверяем основные флаги существования
            if ((building.m_flags & Building.Flags.Created) == 0) return false;
            if ((building.m_flags & Building.Flags.Deleted) != 0) return false;
            
            // Проверяем наличие информации о здании
            if (building.Info == null) return false;
            
            // Исключаем технические флаги
            if ((building.m_flags & (Building.Flags.Untouchable | 
                                   Building.Flags.Hidden | 
                                   Building.Flags.Upgrading | 
                                   Building.Flags.Downgrading | 
                                   Building.Flags.Collapsed | 
                                   Building.Flags.Abandoned | 
                                   Building.Flags.Evacuating)) != 0) return false;
            
            // Исключаем sub-buildings (дочерние здания)
            if (building.m_parentBuilding != 0) return false;
            
            // Исключаем внешние соединения (Outside Connections)
            if (building.Info.GetAI() is OutsideConnectionAI) return false;
            
            // Проверяем, что это RICO здание
            var service = building.Info.GetService();
            if (!IsRicoBuilding(service, building.Info.GetSubService())) return false;
            
            // Дополнительная валидация: проверяем корректность позиции
            if (building.m_position.x < -8192f || building.m_position.x > 8192f ||
                building.m_position.z < -8192f || building.m_position.z > 8192f)
            {
                Debug.LogWarning($"JobsHousingBalance: Building has invalid position: {building.m_position}");
                return false;
            }
            
            // Проверяем корректность уровня здания
            if (building.m_level < 0 || building.m_level > 5)
            {
                Debug.LogWarning($"JobsHousingBalance: Building has invalid level: {building.m_level}");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Проверить, является ли здание RICO зданием (Residential, Commercial, Industrial, Office)
        /// </summary>
        private bool IsRicoBuilding(ItemClass.Service service, ItemClass.SubService subService)
        {
            switch (service)
            {
                case ItemClass.Service.Residential:
                    return true; // Все жилые здания
                    
                case ItemClass.Service.Commercial:
                    return true; // Все коммерческие здания
                    
                case ItemClass.Service.Industrial:
                    return true; // Все промышленные здания
                    
                case ItemClass.Service.Office:
                    return true; // Все офисные здания
                    
                default:
                    return false; // Все остальные сервисы исключаем
            }
        }
        
        /// <summary>
        /// Проверить, является ли здание жилым (RICO)
        /// </summary>
        private bool IsResidentialBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            return service == ItemClass.Service.Residential;
        }
        
        /// <summary>
        /// Проверить, является ли здание коммерческим (RICO)
        /// </summary>
        private bool IsCommercialBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            return service == ItemClass.Service.Commercial;
        }
        
        /// <summary>
        /// Проверить, является ли здание промышленным (RICO)
        /// </summary>
        private bool IsIndustrialBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            return service == ItemClass.Service.Industrial;
        }
        
        /// <summary>
        /// Проверить, является ли здание офисным (RICO)
        /// </summary>
        private bool IsOfficeBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            var subService = info.GetSubService();
            
            // Офисы могут быть Commercial с SubService CommercialHigh
            // или Industrial с SubService IndustrialGeneric (IT кластер)
            return (service == ItemClass.Service.Commercial && subService == ItemClass.SubService.CommercialHigh) ||
                   (service == ItemClass.Service.Industrial && subService == ItemClass.SubService.IndustrialGeneric);
        }
        
        /// <summary>
        /// Собрать данные о конкретном здании
        /// </summary>
        private BuildingSample CollectBuildingData(ushort buildingId, Building building, 
                                                  CitizenUnit[] citizenUnits, DistrictManager districtManager)
        {
            var position = building.m_position;
            var districtId = districtManager.GetDistrict(position);
            var info = building.Info;
            
            // Проверяем на null для предотвращения NullReferenceException
            if (info == null)
            {
                Debug.LogWarning($"JobsHousingBalance: Building {buildingId} has null Info, skipping");
                return new BuildingSample(buildingId, position, districtId, 0, 0, 
                                        ItemClass.Service.None, ItemClass.SubService.None);
            }
            
            // Получаем тип здания
            var service = info.GetService();
            var subService = info.GetSubService();
            
            // Определяем тип здания для правильного подсчета
            var isResidential = IsResidentialBuilding(info);
            var isCommercial = IsCommercialBuilding(info);
            var isIndustrial = IsIndustrialBuilding(info);
            var isOffice = IsOfficeBuilding(info);
            
            // Собираем данные о жителях и рабочих местах
            var residentsFact = 0;
            var jobsFact = 0;
            
            // Проходим по связанному списку CitizenUnits только если они есть
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
                
                // Проверяем, что unitId не превышает размер массива (совместимость с More CitizenUnits)
                if (unitId >= citizenUnits.Length)
                {
                    Debug.LogWarning($"JobsHousingBalance: CitizenUnit ID {unitId} exceeds array bounds ({citizenUnits.Length})");
                    break;
                }
                
                var citizenUnit = citizenUnits[unitId];
                
                // Считаем жителей только для жилых зданий
                if (isResidential && (citizenUnit.m_flags & CitizenUnit.Flags.Home) != 0)
                {
                    residentsFact += CountOccupiedCitizenSlots(citizenUnit);
                }
                // Считаем рабочие места только для нежилых зданий
                else if ((isCommercial || isIndustrial || isOffice) && (citizenUnit.m_flags & CitizenUnit.Flags.Work) != 0)
                {
                    jobsFact += CountOccupiedCitizenSlots(citizenUnit);
                }
                // Игнорируем Visit, Student, Patient, Customer и другие типы
                
                // Переходим к следующему юниту в списке
                unitId = citizenUnit.m_nextUnit;
            }
            
            // Предупреждение если достигли лимита итераций
            if (iterationCount >= maxIterations)
            {
                Debug.LogWarning($"JobsHousingBalance: Maximum iterations reached for building {buildingId}, possible infinite loop");
            }
            
            // Получаем данные о емкости рабочих мест
            var capacityData = _capacityCollector.GetCapacityData(buildingId, building, info);
            
            // Получаем данные об образовании жителей
            var educationData = _educationCollector.GetEducationData(buildingId, building, info, citizenUnits);
            
            // Получаем фактическую занятость по уровням образования
            var occupancyByEdu = _capacityCollector.GetJobsOccupancyByEducation(buildingId, building, info, citizenUnits);
            
            // Валидация полученных данных
            ValidateCapacityData(capacityData, buildingId);
            ValidateEducationData(educationData, buildingId);
            ValidateOccupancyData(occupancyByEdu, buildingId);
            
            // Создаем BuildingSample с данными о емкости и образовании
            var sample = new BuildingSample(buildingId, position, districtId, 
                                    residentsFact, jobsFact, service, subService);
            
            // Заполняем данные о емкости
            sample.jobsCapacityTotal = capacityData.jobsCapacityTotal;
            sample.jobsCapacityByEdu = capacityData.jobsCapacityByEdu;
            
            // Заполняем данные об образовании жителей
            sample.residentsByEdu = educationData.residentsByEdu;
            
            // Заполняем данные о фактической занятости по образованию
            sample.jobsOccupancyByEdu = occupancyByEdu;
            
            return sample;
        }
        
        /// <summary>
        /// Подсчитать количество занятых слотов в CitizenUnit
        /// </summary>
        private int CountOccupiedCitizenSlots(CitizenUnit citizenUnit)
        {
            var count = 0;
            
            // Проверяем каждый слот (до 5 слотов на юнит)
            if (citizenUnit.m_citizen0 != 0) count++;
            if (citizenUnit.m_citizen1 != 0) count++;
            if (citizenUnit.m_citizen2 != 0) count++;
            if (citizenUnit.m_citizen3 != 0) count++;
            if (citizenUnit.m_citizen4 != 0) count++;
            
            return count;
        }
        
        /// <summary>
        /// Логировать диагностику типов зданий для понимания что попадает в подсчет
        /// </summary>
        private void LogBuildingTypeDiagnostics()
        {
            try
            {
                // Подсчет по типам RICO
                var residentialCount = 0;
                var commercialCount = 0;
                var industrialCount = 0;
                var officeCount = 0;
                
                // Подсчет по сервисам (для понимания что исключается)
                var serviceCounts = new System.Collections.Generic.Dictionary<ItemClass.Service, int>();
                
                foreach (var sample in _buildingSamples)
                {
                    switch (sample.service)
                    {
                        case ItemClass.Service.Residential:
                            residentialCount++;
                            break;
                        case ItemClass.Service.Commercial:
                            commercialCount++;
                            break;
                        case ItemClass.Service.Industrial:
                            industrialCount++;
                            break;
                        case ItemClass.Service.Office:
                            officeCount++;
                            break;
                        default:
                            // Подсчитываем исключенные сервисы
                            if (!serviceCounts.ContainsKey(sample.service))
                                serviceCounts[sample.service] = 0;
                            serviceCounts[sample.service]++;
                            break;
                    }
                }
                
                Debug.Log($"JobsHousingBalance: RICO Buildings - " +
                         $"Residential: {residentialCount}, Commercial: {commercialCount}, " +
                         $"Industrial: {industrialCount}, Office: {officeCount}");
                
                if (serviceCounts.Count > 0)
                {
                    var excludedServices = string.Join(", ", serviceCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToArray());
                    Debug.Log($"JobsHousingBalance: Excluded services: {excludedServices}");
                }
                
                // Дополнительная диагностика: проверяем общее количество зданий в буфере
                var buildingManager = BuildingManager.instance;
                var totalBuildingsInBuffer = 0;
                var validRicoBuildings = 0;
                var excludedBuildings = 0;
                
                for (ushort i = 1; i < buildingManager.m_buildings.m_size; i++)
                {
                    var building = buildingManager.m_buildings.m_buffer[i];
                    if ((building.m_flags & Building.Flags.Created) != 0)
                    {
                        totalBuildingsInBuffer++;
                        
                        if (IsValidBuilding(building))
                        {
                            validRicoBuildings++;
                        }
                        else
                        {
                            excludedBuildings++;
                        }
                    }
                }
                
                Debug.Log($"JobsHousingBalance: Building buffer analysis - " +
                         $"Total in buffer: {totalBuildingsInBuffer}, " +
                         $"Valid RICO: {validRicoBuildings}, " +
                         $"Excluded: {excludedBuildings}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error in building type diagnostics: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Валидировать данные о емкости рабочих мест
        /// </summary>
        private void ValidateCapacityData(JobsCapacityCollector.CapacityData capacityData, ushort buildingId)
        {
            // Проверяем корректность общей емкости
            if (capacityData.jobsCapacityTotal < 0)
            {
                Debug.LogWarning($"JobsHousingBalance: Building {buildingId} has negative total capacity: {capacityData.jobsCapacityTotal}");
            }
            
            // Проверяем корректность распределения по образованию
            if (capacityData.jobsCapacityByEdu != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (capacityData.jobsCapacityByEdu[i] < 0)
                    {
                        Debug.LogWarning($"JobsHousingBalance: Building {buildingId} has negative capacity for education level {i}: {capacityData.jobsCapacityByEdu[i]}");
                    }
                }
                
                // Проверяем, что сумма по уровням образования не превышает общую емкость
                var sumByEdu = capacityData.jobsCapacityByEdu[0] + capacityData.jobsCapacityByEdu[1] + 
                              capacityData.jobsCapacityByEdu[2] + capacityData.jobsCapacityByEdu[3];
                if (sumByEdu > capacityData.jobsCapacityTotal + 10) // Допускаем небольшую погрешность
                {
                    Debug.LogWarning($"JobsHousingBalance: Building {buildingId} capacity by education ({sumByEdu}) exceeds total capacity ({capacityData.jobsCapacityTotal})");
                }
            }
        }
        
        /// <summary>
        /// Валидировать данные об образовании жителей
        /// </summary>
        private void ValidateEducationData(EducationDataCollector.EducationData educationData, ushort buildingId)
        {
            if (educationData.residentsByEdu != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (educationData.residentsByEdu[i] < 0)
                    {
                        Debug.LogWarning($"JobsHousingBalance: Building {buildingId} has negative residents for education level {i}: {educationData.residentsByEdu[i]}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Валидировать данные о фактической занятости по образованию
        /// </summary>
        private void ValidateOccupancyData(int[] occupancyByEdu, ushort buildingId)
        {
            if (occupancyByEdu != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (occupancyByEdu[i] < 0)
                    {
                        Debug.LogWarning($"JobsHousingBalance: Building {buildingId} has negative occupancy for education level {i}: {occupancyByEdu[i]}");
                    }
                }
            }
        }
        
        #endregion
    }
}
