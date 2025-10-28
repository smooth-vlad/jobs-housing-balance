using System;
using UnityEngine;

namespace JobsHousingBalance.Config
{
    /// <summary>
    /// Централизованное хранилище состояния приложения
    /// </summary>
    public class AppState
    {
        #region Enums
        
        public enum Mode
        {
            Hex,
            Districts
        }
        
        public enum HexSize
        {
            Size64 = 64,
            Size128 = 128,
            Size256 = 256,
            Size512 = 512
        }
        
        public enum MetricType
        {
            Fact,           // По факту - реально живущие и работающие через CitizenUnit
            Capacity,       // Ёмкость - сколько может работать по данным BuildingAI
            EduAware        // С учётом образования - 4 уровня образования
        }
        
        public enum EducationMode
        {
            Strict,         // Сравнение 1:1 без перелива уровней
            Substituted     // Перелив сверху вниз - более образованные могут закрывать низкоуровневые позиции
        }
        
        #endregion
        
        #region Singleton Pattern
        
        private static AppState _instance;
        private static readonly object _lock = new object();
        
        public static AppState Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppState();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private AppState()
        {
            // Initialize with default values
            _currentMode = Mode.Hex;
            _currentHexSize = HexSize.Size128;
            _opacity = 0.8f; // 80%
            _currentMetricType = MetricType.Fact;
            _currentEducationMode = EducationMode.Strict;
            _includeServiceUnique = true;  // По умолчанию включено согласно документации
            _includeTeens = false;         // По умолчанию выключено
            
            Debug.Log("JobsHousingBalance: AppState initialized with defaults");
            Debug.Log($"JobsHousingBalance: AppState - Mode: {_currentMode}, HexSize: {_currentHexSize}, Opacity: {_opacity:F2}");
            Debug.Log($"JobsHousingBalance: AppState - MetricType: {_currentMetricType}, EducationMode: {_currentEducationMode}");
            Debug.Log($"JobsHousingBalance: AppState - IncludeServiceUnique: {_includeServiceUnique}, IncludeTeens: {_includeTeens}");
        }
        
        #endregion
        
        #region Properties
        
        private Mode _currentMode;
        public Mode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    OnModeChanged?.Invoke(value);
                    Debug.Log($"JobsHousingBalance: Mode changed to {value}");
                }
            }
        }
        
        private HexSize _currentHexSize;
        public HexSize CurrentHexSize
        {
            get => _currentHexSize;
            set
            {
                if (_currentHexSize != value)
                {
                    _currentHexSize = value;
                    OnHexSizeChanged?.Invoke(value);
                    Debug.Log($"JobsHousingBalance: HexSize changed to {value}");
                }
            }
        }
        
        private float _opacity;
        public float Opacity
        {
            get => _opacity;
            set
            {
                // Clamp value to valid range
                var clampedValue = Mathf.Clamp(value, 0.1f, 0.8f);
                if (Math.Abs(_opacity - clampedValue) > 0.001f)
                {
                    _opacity = clampedValue;
                    OnOpacityChanged?.Invoke(clampedValue);
                    Debug.Log($"JobsHousingBalance: Opacity changed to {clampedValue:F2}");
                }
            }
        }
        
        private MetricType _currentMetricType;
        public MetricType CurrentMetricType
        {
            get => _currentMetricType;
            set
            {
                if (_currentMetricType != value)
                {
                    _currentMetricType = value;
                    OnMetricTypeChanged?.Invoke(value);
                    Debug.Log($"JobsHousingBalance: MetricType changed to {value}");
                }
            }
        }
        
        private EducationMode _currentEducationMode;
        public EducationMode CurrentEducationMode
        {
            get => _currentEducationMode;
            set
            {
                if (_currentEducationMode != value)
                {
                    _currentEducationMode = value;
                    OnEducationModeChanged?.Invoke(value);
                    Debug.Log($"JobsHousingBalance: EducationMode changed to {value}");
                }
            }
        }
        
        private bool _includeServiceUnique;
        public bool IncludeServiceUnique
        {
            get => _includeServiceUnique;
            set
            {
                if (_includeServiceUnique != value)
                {
                    _includeServiceUnique = value;
                    OnIncludeServiceUniqueChanged?.Invoke(value);
                    Debug.Log($"JobsHousingBalance: IncludeServiceUnique changed to {value}");
                }
            }
        }
        
        private bool _includeTeens;
        public bool IncludeTeens
        {
            get => _includeTeens;
            set
            {
                if (_includeTeens != value)
                {
                    _includeTeens = value;
                    OnIncludeTeensChanged?.Invoke(value);
                    Debug.Log($"JobsHousingBalance: IncludeTeens changed to {value}");
                }
            }
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Событие изменения режима отображения
        /// </summary>
        public event Action<Mode> OnModeChanged;
        
        /// <summary>
        /// Событие изменения размера гекса
        /// </summary>
        public event Action<HexSize> OnHexSizeChanged;
        
        /// <summary>
        /// Событие изменения прозрачности
        /// </summary>
        public event Action<float> OnOpacityChanged;
        
        /// <summary>
        /// Событие изменения типа метрики
        /// </summary>
        public event Action<MetricType> OnMetricTypeChanged;
        
        /// <summary>
        /// Событие изменения режима образования
        /// </summary>
        public event Action<EducationMode> OnEducationModeChanged;
        
        /// <summary>
        /// Событие изменения включения сервисных/уникальных зданий
        /// </summary>
        public event Action<bool> OnIncludeServiceUniqueChanged;
        
        /// <summary>
        /// Событие изменения включения подростков
        /// </summary>
        public event Action<bool> OnIncludeTeensChanged;
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Получить строковое представление текущего режима
        /// </summary>
        public string GetModeString()
        {
            return CurrentMode.ToString();
        }
        
        /// <summary>
        /// Получить строковое представление текущего размера гекса
        /// </summary>
        public string GetHexSizeString()
        {
            return $"{(int)CurrentHexSize}m";
        }
        
        /// <summary>
        /// Получить прозрачность в процентах для отображения
        /// </summary>
        public int GetOpacityPercentage()
        {
            return Mathf.RoundToInt(Opacity * 100f);
        }
        
        /// <summary>
        /// Установить режим по строке
        /// </summary>
        public void SetModeFromString(string modeString)
        {
            try
            {
                var mode = (Mode)System.Enum.Parse(typeof(Mode), modeString);
                CurrentMode = mode;
            }
            catch (System.ArgumentException)
            {
                Debug.LogWarning($"JobsHousingBalance: Invalid mode string: {modeString}");
            }
        }
        
        /// <summary>
        /// Установить размер гекса по строке (например "128m")
        /// </summary>
        public void SetHexSizeFromString(string hexSizeString)
        {
            if (hexSizeString.EndsWith("m") && 
                int.TryParse(hexSizeString.Substring(0, hexSizeString.Length - 1), out var size))
            {
                try
                {
                    var hexSize = (HexSize)System.Enum.Parse(typeof(HexSize), $"Size{size}");
                    CurrentHexSize = hexSize;
                }
                catch (System.ArgumentException)
                {
                    Debug.LogWarning($"JobsHousingBalance: Invalid hex size: {size}");
                }
            }
            else
            {
                Debug.LogWarning($"JobsHousingBalance: Invalid hex size string: {hexSizeString}");
            }
        }
        
        /// <summary>
        /// Установить прозрачность из процентов
        /// </summary>
        public void SetOpacityFromPercentage(int percentage)
        {
            Opacity = percentage / 100f;
        }
        
        /// <summary>
        /// Получить строковое представление текущего типа метрики
        /// </summary>
        public string GetMetricTypeString()
        {
            switch (CurrentMetricType)
            {
                case MetricType.Fact:
                    return "Fact";
                case MetricType.Capacity:
                    return "Capacity";
                case MetricType.EduAware:
                    return "Edu-aware";
                default:
                    return "Unknown";
            }
        }
        
        /// <summary>
        /// Получить строковое представление текущего режима образования
        /// </summary>
        public string GetEducationModeString()
        {
            switch (CurrentEducationMode)
            {
                case EducationMode.Strict:
                    return "Strict";
                case EducationMode.Substituted:
                    return "Substituted";
                default:
                    return "Unknown";
            }
        }
        
        /// <summary>
        /// Установить тип метрики по строке
        /// </summary>
        public void SetMetricTypeFromString(string metricTypeString)
        {
            try
            {
                MetricType metricType;
                switch (metricTypeString)
                {
                    case "Fact":
                        metricType = MetricType.Fact;
                        break;
                    case "Capacity":
                        metricType = MetricType.Capacity;
                        break;
                    case "Edu-aware":
                        metricType = MetricType.EduAware;
                        break;
                    default:
                        throw new ArgumentException($"Unknown metric type: {metricTypeString}");
                }
                CurrentMetricType = metricType;
            }
            catch (ArgumentException ex)
            {
                Debug.LogWarning($"JobsHousingBalance: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Установить режим образования по строке
        /// </summary>
        public void SetEducationModeFromString(string educationModeString)
        {
            try
            {
                EducationMode educationMode;
                switch (educationModeString)
                {
                    case "Strict":
                        educationMode = EducationMode.Strict;
                        break;
                    case "Substituted":
                        educationMode = EducationMode.Substituted;
                        break;
                    default:
                        throw new ArgumentException($"Unknown education mode: {educationModeString}");
                }
                CurrentEducationMode = educationMode;
            }
            catch (ArgumentException ex)
            {
                Debug.LogWarning($"JobsHousingBalance: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Debug
        
        /// <summary>
        /// Получить текущее состояние для отладки
        /// </summary>
        public string GetDebugInfo()
        {
            return $"AppState: Mode={CurrentMode}, HexSize={CurrentHexSize}, Opacity={Opacity:F2} ({GetOpacityPercentage()}%), " +
                   $"MetricType={CurrentMetricType}, EducationMode={CurrentEducationMode}, " +
                   $"IncludeServiceUnique={IncludeServiceUnique}, IncludeTeens={IncludeTeens}";
        }
        
        #endregion
    }
}
