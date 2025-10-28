using System;
using UnityEngine;
using JobsHousingBalance.Config;

namespace JobsHousingBalance.Rendering.Overlay
{
    /// <summary>
    /// Базовый класс для рендеринга оверлея дисбаланса жилья и рабочих мест
    /// </summary>
    public class OverlayRenderer
    {
        #region Fields
        
        private bool _isVisible = false;
        private AppState.Mode _currentMode = AppState.Mode.Hex;
        private AppState.HexSize _currentHexSize = AppState.HexSize.Size128;
        private float _currentOpacity = 0.8f;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Видимость оверлея
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            private set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    Debug.Log($"JobsHousingBalance: Overlay visibility changed to {_isVisible}");
                }
            }
        }
        
        /// <summary>
        /// Текущий режим отображения
        /// </summary>
        public AppState.Mode CurrentMode
        {
            get => _currentMode;
            private set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    Debug.Log($"JobsHousingBalance: Overlay mode changed to {_currentMode}");
                    OnModeChanged();
                }
            }
        }
        
        /// <summary>
        /// Текущий размер гекса
        /// </summary>
        public AppState.HexSize CurrentHexSize
        {
            get => _currentHexSize;
            private set
            {
                if (_currentHexSize != value)
                {
                    _currentHexSize = value;
                    Debug.Log($"JobsHousingBalance: Overlay hex size changed to {_currentHexSize}");
                    OnHexSizeChanged();
                }
            }
        }
        
        /// <summary>
        /// Текущая прозрачность
        /// </summary>
        public float CurrentOpacity
        {
            get => _currentOpacity;
            private set
            {
                var clampedValue = Mathf.Clamp(value, 0.1f, 0.8f);
                if (Mathf.Abs(_currentOpacity - clampedValue) > 0.001f)
                {
                    _currentOpacity = clampedValue;
                    Debug.Log($"JobsHousingBalance: Overlay opacity changed to {_currentOpacity:F2}");
                    OnOpacityChanged();
                }
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Показать оверлей
        /// </summary>
        public void Show()
        {
            if (!IsVisible)
            {
                IsVisible = true;
                OnShow();
                Debug.Log("JobsHousingBalance: Overlay shown");
            }
        }
        
        /// <summary>
        /// Скрыть оверлей
        /// </summary>
        public void Hide()
        {
            if (IsVisible)
            {
                IsVisible = false;
                OnHide();
                Debug.Log("JobsHousingBalance: Overlay hidden");
            }
        }
        
        /// <summary>
        /// Переключить видимость оверлея
        /// </summary>
        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }
        
        /// <summary>
        /// Установить прозрачность
        /// </summary>
        /// <param name="opacity">Прозрачность от 0.1 до 0.8</param>
        public void SetOpacity(float opacity)
        {
            CurrentOpacity = opacity;
        }
        
        /// <summary>
        /// Установить режим отображения
        /// </summary>
        /// <param name="mode">Режим (Hex или Districts)</param>
        public void SetMode(AppState.Mode mode)
        {
            CurrentMode = mode;
        }
        
        /// <summary>
        /// Установить размер гекса
        /// </summary>
        /// <param name="hexSize">Размер гекса</param>
        public void SetHexSize(AppState.HexSize hexSize)
        {
            CurrentHexSize = hexSize;
        }
        
        /// <summary>
        /// Обновить состояние из AppState
        /// </summary>
        /// <param name="appState">Состояние приложения</param>
        public void UpdateFromAppState(AppState appState)
        {
            SetMode(appState.CurrentMode);
            SetHexSize(appState.CurrentHexSize);
            SetOpacity(appState.Opacity);
            
            Debug.Log("JobsHousingBalance: OverlayRenderer updated from AppState");
        }
        
        #endregion
        
        #region Protected Virtual Methods (Override Points)
        
        /// <summary>
        /// Вызывается при показе оверлея
        /// </summary>
        protected virtual void OnShow()
        {
            // TODO: Implement actual overlay rendering
            Debug.Log("JobsHousingBalance: OnShow() - Override this method to implement overlay rendering");
        }
        
        /// <summary>
        /// Вызывается при скрытии оверлея
        /// </summary>
        protected virtual void OnHide()
        {
            // TODO: Implement overlay cleanup
            Debug.Log("JobsHousingBalance: OnHide() - Override this method to implement overlay cleanup");
        }
        
        /// <summary>
        /// Вызывается при изменении режима
        /// </summary>
        protected virtual void OnModeChanged()
        {
            // TODO: Implement mode-specific rendering logic
            Debug.Log($"JobsHousingBalance: OnModeChanged() - Mode: {CurrentMode}");
        }
        
        /// <summary>
        /// Вызывается при изменении размера гекса
        /// </summary>
        protected virtual void OnHexSizeChanged()
        {
            // TODO: Implement hex size-specific rendering logic
            Debug.Log($"JobsHousingBalance: OnHexSizeChanged() - HexSize: {CurrentHexSize}");
        }
        
        /// <summary>
        /// Вызывается при изменении прозрачности
        /// </summary>
        protected virtual void OnOpacityChanged()
        {
            // TODO: Implement opacity update logic
            Debug.Log($"JobsHousingBalance: OnOpacityChanged() - Opacity: {CurrentOpacity:F2}");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Получить размер гекса в метрах
        /// </summary>
        /// <returns>Размер в метрах</returns>
        public int GetHexSizeInMeters()
        {
            return (int)CurrentHexSize;
        }
        
        /// <summary>
        /// Получить прозрачность в процентах
        /// </summary>
        /// <returns>Прозрачность в процентах (10-80)</returns>
        public int GetOpacityPercentage()
        {
            return Mathf.RoundToInt(CurrentOpacity * 100f);
        }
        
        /// <summary>
        /// Проверить, активен ли режим Hex
        /// </summary>
        /// <returns>True если режим Hex</returns>
        public bool IsHexMode()
        {
            return CurrentMode == AppState.Mode.Hex;
        }
        
        /// <summary>
        /// Проверить, активен ли режим Districts
        /// </summary>
        /// <returns>True если режим Districts</returns>
        public bool IsDistrictsMode()
        {
            return CurrentMode == AppState.Mode.Districts;
        }
        
        #endregion
    }
}
