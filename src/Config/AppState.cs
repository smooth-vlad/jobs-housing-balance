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
            
            Debug.Log("JobsHousingBalance: AppState initialized with defaults");
            Debug.Log($"JobsHousingBalance: AppState - Mode: {_currentMode}, HexSize: {_currentHexSize}, Opacity: {_opacity:F2}");
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
        
        #endregion
        
        #region Debug
        
        /// <summary>
        /// Получить текущее состояние для отладки
        /// </summary>
        public string GetDebugInfo()
        {
            return $"AppState: Mode={CurrentMode}, HexSize={CurrentHexSize}, Opacity={Opacity:F2} ({GetOpacityPercentage()}%)";
        }
        
        #endregion
    }
}
