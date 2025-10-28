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
            
            return $"Buildings: {samples.Count} total ({residential} residential, {nonResidential} non-residential), " +
                   $"With balance: {withBalance}, Residents: {_totalResidents}, Jobs: {_totalJobs}, " +
                   $"Balance: {TotalBalance}, Last update: frame {_lastUpdatedFrame}";
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
                
                _buildingSamples.Clear();
                _totalResidents = 0;
                _totalJobs = 0;
                
                var processedBuildings = 0;
                var skippedBuildings = 0;
                
                // Итерация по всем зданиям
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
                    
                    // Ограничиваем количество зданий за кадр для производительности
                    if (processedBuildings % MaxBuildingsPerFrame == 0)
                    {
                        // В реальной реализации здесь можно было бы использовать корутины
                        // или разбить на несколько кадров, но для MVP делаем все сразу
                    }
                }
                
                _isDataValid = true;
                _lastUpdatedFrame = (int)SimulationManager.instance.m_currentFrameIndex;
                
                var duration = System.DateTime.Now - startTime;
                Debug.Log($"JobsHousingBalance: Data collection completed in {duration.TotalMilliseconds:F1}ms. " +
                         $"Processed: {processedBuildings}, Skipped: {skippedBuildings}, " +
                         $"Total residents: {_totalResidents}, Total jobs: {_totalJobs}");
                
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
        /// </summary>
        private bool IsValidBuilding(Building building)
        {
            // Проверяем основные флаги существования
            if ((building.m_flags & Building.Flags.Created) == 0) return false;
            if ((building.m_flags & Building.Flags.Deleted) != 0) return false;
            
            // Проверяем наличие информации о здании
            if (building.Info == null) return false;
            
            // Исключаем внешние соединения (Outside Connections)
            if (building.Info.GetAI() is OutsideConnectionAI) return false;
            
            // Убираем проверку на m_citizenUnits == 0, так как это может быть валидным состоянием
            // для новых зданий или зданий без жителей/рабочих мест
            
            return true;
        }
        
        /// <summary>
        /// Проверить, является ли здание жилым
        /// </summary>
        private bool IsResidentialBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            return service == ItemClass.Service.Residential;
        }
        
        /// <summary>
        /// Проверить, является ли здание коммерческим
        /// </summary>
        private bool IsCommercialBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            return service == ItemClass.Service.Commercial;
        }
        
        /// <summary>
        /// Проверить, является ли здание промышленным
        /// </summary>
        private bool IsIndustrialBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            return service == ItemClass.Service.Industrial;
        }
        
        /// <summary>
        /// Проверить, является ли здание офисным
        /// </summary>
        private bool IsOfficeBuilding(BuildingInfo info)
        {
            if (info == null) return false;
            
            var service = info.GetService();
            var subService = info.GetSubService();
            
            // Офисы могут быть как Commercial, так и Industrial с определенными SubService
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
            
            return new BuildingSample(buildingId, position, districtId, 
                                    residentsFact, jobsFact, service, subService);
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
        
        #endregion
    }
}
