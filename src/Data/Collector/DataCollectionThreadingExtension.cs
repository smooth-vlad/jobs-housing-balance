using System.Collections;
using UnityEngine;
using ColossalFramework;
using ICities;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Расширение для выполнения тяжелых операций сбора данных в симуляционном потоке
    /// Использует ThreadingExtensionBase для оптимизации производительности
    /// </summary>
    public class DataCollectionThreadingExtension : ThreadingExtensionBase
    {
        #region Fields
        
        private BuildingDataCollector _dataCollector;
        private DataCache _dataCache;
        private bool _isInitialized;
        private bool _isProcessing;
        private int _errorCount; // Счетчик ошибок для предотвращения бесконечных попыток
        
        // Константы для батчинга
        private const int BuildingsPerBatch = 100; // Количество зданий для обработки за батч
        private const int BatchDelayFrames = 1; // Задержка между батчами в кадрах
        private const int MaxErrorCount = 5; // Максимальное количество ошибок перед остановкой
        
        // Состояние батчинга
        private int _currentBuildingIndex;
        private int _totalBuildings;
        private bool _batchProcessingActive;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Проверить, инициализировано ли расширение
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Проверить, выполняется ли обработка
        /// </summary>
        public bool IsProcessing => _isProcessing;
        
        /// <summary>
        /// Получить прогресс обработки (0.0 - 1.0)
        /// </summary>
        public float ProcessingProgress
        {
            get
            {
                if (_totalBuildings == 0) return 0f;
                return (float)_currentBuildingIndex / _totalBuildings;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Инициализировать расширение
        /// </summary>
        public void Initialize(BuildingDataCollector dataCollector, DataCache dataCache)
        {
            _dataCollector = dataCollector;
            _dataCache = dataCache;
            _isInitialized = true;
            _isProcessing = false;
            _batchProcessingActive = false;
            
            Debug.Log("JobsHousingBalance: DataCollectionThreadingExtension initialized");
        }
        
        /// <summary>
        /// Запустить батчевую обработку данных
        /// </summary>
        public void StartBatchProcessing()
        {
            if (!_isInitialized || _isProcessing) return;
            
            try
            {
                var buildingManager = BuildingManager.instance;
                if (buildingManager == null) return;
                
                _totalBuildings = (int)buildingManager.m_buildings.m_size;
                _currentBuildingIndex = 0;
                _isProcessing = true;
                _batchProcessingActive = true;
                _errorCount = 0; // Сбрасываем счетчик ошибок при новом запуске
                
                Debug.Log($"JobsHousingBalance: Starting batch processing of {_totalBuildings} buildings");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error starting batch processing: {ex.Message}");
                _isProcessing = false;
                _batchProcessingActive = false;
            }
        }
        
        /// <summary>
        /// Остановить батчевую обработку
        /// </summary>
        public void StopBatchProcessing()
        {
            _isProcessing = false;
            _batchProcessingActive = false;
            Debug.Log("JobsHousingBalance: Batch processing stopped");
        }
        
        #endregion
        
        #region ThreadingExtensionBase Overrides
        
        /// <summary>
        /// Вызывается каждый кадр симуляции для выполнения тяжелых операций
        /// </summary>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (!_isInitialized || !_batchProcessingActive) return;
            
            try
            {
                // Выполняем батчевую обработку
                ProcessBatch();
                
                // Сбрасываем счетчик ошибок при успешном выполнении
                _errorCount = 0;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error in OnUpdate: {ex.Message}");
                
                // Увеличиваем счетчик ошибок
                _errorCount++;
                
                // Останавливаем обработку при критических ошибках или после максимального количества попыток
                if (ex is System.OutOfMemoryException || ex is System.StackOverflowException || _errorCount >= MaxErrorCount)
                {
                    Debug.LogError($"JobsHousingBalance: Critical error or too many errors ({_errorCount}), stopping batch processing");
                    StopBatchProcessing();
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Обработать один батч зданий
        /// </summary>
        private void ProcessBatch()
        {
            if (!_isProcessing || _currentBuildingIndex >= _totalBuildings)
            {
                // Обработка завершена
                CompleteBatchProcessing();
                return;
            }
            
            var buildingManager = BuildingManager.instance;
            var citizenManager = CitizenManager.instance;
            var districtManager = DistrictManager.instance;
            
            if (buildingManager == null || citizenManager == null || districtManager == null)
            {
                Debug.LogError("JobsHousingBalance: Required managers are null during batch processing");
                StopBatchProcessing();
                return;
            }
            
            var buildings = buildingManager.m_buildings;
            var citizenUnits = citizenManager.m_units;
            var processedInBatch = 0;
            
            // Обрабатываем здания в текущем батче
            while (_currentBuildingIndex < _totalBuildings && processedInBatch < BuildingsPerBatch)
            {
                var buildingId = (ushort)_currentBuildingIndex;
                var building = buildings.m_buffer[buildingId];
                
                // Проверяем валидность здания
                if (IsValidBuilding(building))
                {
                    // Собираем данные о здании
                    var sample = CollectBuildingData(buildingId, building, citizenUnits.m_buffer, districtManager);
                    
                    // Кэшируем данные
                    _dataCache.SetBuildingData(buildingId, sample);
                }
                
                _currentBuildingIndex++;
                processedInBatch++;
            }
            
            // Логируем прогресс каждые 1000 зданий
            if (_currentBuildingIndex % 1000 == 0)
            {
                Debug.Log($"JobsHousingBalance: Batch processing progress: {_currentBuildingIndex}/{_totalBuildings} " +
                         $"({ProcessingProgress:P1})");
            }
        }
        
        /// <summary>
        /// Завершить батчевую обработку
        /// </summary>
        private void CompleteBatchProcessing()
        {
            _isProcessing = false;
            _batchProcessingActive = false;
            
            // Помечаем кэш как чистый
            _dataCache.MarkCacheClean();
            
            Debug.Log($"JobsHousingBalance: Batch processing completed. Processed {_currentBuildingIndex} buildings. " +
                     $"Cache stats: {_dataCache.CacheStats}");
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
            
            return true;
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
            
            // Получаем тип здания
            var service = info.GetService();
            var subService = info.GetSubService();
            
            // Собираем данные о жителях и рабочих местах
            var residentsFact = 0;
            var jobsFact = 0;
            
            // Проходим по связанному списку CitizenUnits только если они есть
            var unitId = building.m_citizenUnits;
            while (unitId != 0)
            {
                // Проверяем, что unitId не превышает размер массива (совместимость с More CitizenUnits)
                if (unitId >= citizenUnits.Length)
                {
                    break;
                }
                
                var citizenUnit = citizenUnits[unitId];
                
                // Проверяем тип юнита по флагам
                if ((citizenUnit.m_flags & CitizenUnit.Flags.Home) != 0)
                {
                    // Жители - считаем только фактически занятые слоты
                    residentsFact += CountOccupiedCitizenSlots(citizenUnit);
                }
                else if ((citizenUnit.m_flags & CitizenUnit.Flags.Work) != 0)
                {
                    // Рабочие места - считаем только фактически занятые слоты
                    jobsFact += CountOccupiedCitizenSlots(citizenUnit);
                }
                // Игнорируем Visit, Student, Patient, Customer и другие типы
                
                // Переходим к следующему юниту в списке
                unitId = citizenUnit.m_nextUnit;
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
