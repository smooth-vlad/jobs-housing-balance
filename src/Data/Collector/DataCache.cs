using System.Collections.Generic;
using UnityEngine;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Система кэширования данных о зданиях с поддержкой инвалидации
    /// </summary>
    public class DataCache
    {
        #region Fields
        
        private Dictionary<ushort, CachedBuildingData> _buildingCache;
        private bool _isDirty;
        private int _lastUpdatedFrame;
        private int _cacheHits;
        private int _cacheMisses;
        private int _lastCleanupFrame; // Последний кадр очистки кэша
        
        // Константы для TTL кэша
        private const int CacheTTLFrames = 512; // Время жизни кэша в кадрах (~10-20 секунд)
        private const int CleanupIntervalFrames = 1024; // Интервал очистки кэша (~20-40 секунд)
        private const int MaxCacheSize = 10000; // Максимальный размер кэша для предотвращения утечек памяти
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Проверить, является ли кэш грязным (требует обновления)
        /// </summary>
        public bool IsDirty => _isDirty;
        
        /// <summary>
        /// Получить количество элементов в кэше
        /// </summary>
        public int CacheSize => _buildingCache?.Count ?? 0;
        
        /// <summary>
        /// Получить статистику кэша
        /// </summary>
        public string CacheStats => $"Cache: {CacheSize} items, Hits: {_cacheHits}, Misses: {_cacheMisses}, " +
                                   $"Hit rate: {(_cacheHits + _cacheMisses > 0 ? (_cacheHits * 100f / (_cacheHits + _cacheMisses)) : 0):F1}%";
        
        #endregion
        
        #region Constructor
        
        public DataCache()
        {
            _buildingCache = new Dictionary<ushort, CachedBuildingData>();
            _isDirty = true;
            _lastUpdatedFrame = 0;
            _lastCleanupFrame = 0;
            _cacheHits = 0;
            _cacheMisses = 0;
            
            Debug.Log("JobsHousingBalance: DataCache initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Получить данные здания из кэша
        /// </summary>
        public bool TryGetBuildingData(ushort buildingId, out BuildingSample sample)
        {
            // Проверяем необходимость периодической очистки кэша
            PerformPeriodicCleanup();
            
            if (_buildingCache.TryGetValue(buildingId, out var cachedData))
            {
                // Проверяем TTL кэша
                if (IsCacheEntryValid(cachedData))
                {
                    sample = cachedData.Sample;
                    _cacheHits++;
                    return true;
                }
                else
                {
                    // Удаляем устаревшую запись
                    _buildingCache.Remove(buildingId);
                }
            }
            
            sample = default(BuildingSample);
            _cacheMisses++;
            return false;
        }
        
        /// <summary>
        /// Добавить или обновить данные здания в кэше
        /// </summary>
        public void SetBuildingData(ushort buildingId, BuildingSample sample)
        {
            var cachedData = new CachedBuildingData
            {
                Sample = sample,
                LastUpdatedFrame = (int)SimulationManager.instance.m_currentFrameIndex
            };
            
            _buildingCache[buildingId] = cachedData;
        }
        
        /// <summary>
        /// Пометить кэш как грязный (требует обновления)
        /// </summary>
        public void InvalidateCache()
        {
            _isDirty = true;
            Debug.Log("JobsHousingBalance: DataCache invalidated");
        }
        
        /// <summary>
        /// Пометить кэш как чистый (данные актуальны)
        /// </summary>
        public void MarkCacheClean()
        {
            _isDirty = false;
            _lastUpdatedFrame = (int)SimulationManager.instance.m_currentFrameIndex;
        }
        
        /// <summary>
        /// Проверить, нужно ли обновление кэша
        /// </summary>
        public bool NeedsUpdate()
        {
            if (_isDirty) return true;
            
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            return (currentFrame - _lastUpdatedFrame) > CacheTTLFrames;
        }
        
        /// <summary>
        /// Очистить весь кэш
        /// </summary>
        public void ClearCache()
        {
            _buildingCache.Clear();
            _isDirty = true;
            _cacheHits = 0;
            _cacheMisses = 0;
            
            Debug.Log("JobsHousingBalance: DataCache cleared");
        }
        
        /// <summary>
        /// Очистить устаревшие записи из кэша
        /// </summary>
        public void CleanupExpiredEntries()
        {
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            var keysToRemove = new List<ushort>();
            
            foreach (var kvp in _buildingCache)
            {
                if ((currentFrame - kvp.Value.LastUpdatedFrame) > CacheTTLFrames)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _buildingCache.Remove(key);
            }
            
            if (keysToRemove.Count > 0)
            {
                Debug.Log($"JobsHousingBalance: Cleaned up {keysToRemove.Count} expired cache entries");
            }
        }
        
        /// <summary>
        /// Получить все данные из кэша
        /// </summary>
        public List<BuildingSample> GetAllCachedData()
        {
            var samples = new List<BuildingSample>();
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            
            foreach (var kvp in _buildingCache)
            {
                if ((currentFrame - kvp.Value.LastUpdatedFrame) <= CacheTTLFrames)
                {
                    samples.Add(kvp.Value.Sample);
                }
            }
            
            return samples;
        }
        
        /// <summary>
        /// Получить данные для конкретного района
        /// </summary>
        public List<BuildingSample> GetCachedDataForDistrict(byte districtId)
        {
            var samples = new List<BuildingSample>();
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            
            foreach (var kvp in _buildingCache)
            {
                var cachedData = kvp.Value;
                if ((currentFrame - cachedData.LastUpdatedFrame) <= CacheTTLFrames &&
                    cachedData.Sample.districtId == districtId)
                {
                    samples.Add(cachedData.Sample);
                }
            }
            
            return samples;
        }
        
        /// <summary>
        /// Выполнить периодическую очистку кэша от устаревших записей
        /// </summary>
        public void PerformPeriodicCleanup()
        {
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            
            // Проверяем, нужно ли выполнять очистку
            if (currentFrame - _lastCleanupFrame < CleanupIntervalFrames)
            {
                return;
            }
            
            _lastCleanupFrame = currentFrame;
            
            try
            {
                var keysToRemove = new List<ushort>();
                var removedCount = 0;
                
                // Находим устаревшие записи
                foreach (var kvp in _buildingCache)
                {
                    var cachedData = kvp.Value;
                    if ((currentFrame - cachedData.LastUpdatedFrame) > CacheTTLFrames)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                
                // Удаляем устаревшие записи
                foreach (var key in keysToRemove)
                {
                    _buildingCache.Remove(key);
                    removedCount++;
                }
                
                // Если кэш слишком большой, удаляем самые старые записи
                if (_buildingCache.Count > MaxCacheSize)
                {
                    var sortedEntries = new List<KeyValuePair<ushort, CachedBuildingData>>(_buildingCache);
                    sortedEntries.Sort((a, b) => a.Value.LastUpdatedFrame.CompareTo(b.Value.LastUpdatedFrame));
                    
                    var excessCount = _buildingCache.Count - MaxCacheSize;
                    for (int i = 0; i < excessCount; i++)
                    {
                        _buildingCache.Remove(sortedEntries[i].Key);
                        removedCount++;
                    }
                }
                
                if (removedCount > 0)
                {
                    Debug.Log($"JobsHousingBalance: Cache cleanup removed {removedCount} entries. Cache size: {_buildingCache.Count}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JobsHousingBalance: Error during cache cleanup: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Проверить, действительна ли запись кэша
        /// </summary>
        private bool IsCacheEntryValid(CachedBuildingData cachedData)
        {
            var currentFrame = (int)SimulationManager.instance.m_currentFrameIndex;
            return (currentFrame - cachedData.LastUpdatedFrame) <= CacheTTLFrames;
        }
        
        #endregion
        
        #region Nested Types
        
        /// <summary>
        /// Структура для хранения кэшированных данных здания
        /// </summary>
        private struct CachedBuildingData
        {
            public BuildingSample Sample;
            public int LastUpdatedFrame;
        }
        
        #endregion
    }
}
