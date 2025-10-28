# Архитектура проекта

## Структура папок
```
src/
├── UI/
│   ├── IconButton.cs          # Круглая кнопка-иконка (48x48px)
│   └── Panel/MainPanel.cs     # Панель управления с автоматической подгонкой размеров
├── Config/
│   └── AppState.cs            # Централизованное состояние приложения
├── Rendering/
│   └── Overlay/
│       └── OverlayRenderer.cs # Базовый класс для рендеринга оверлеев
├── OverlayManager.cs          # Менеджер связи UI ↔ OverlayRenderer
├── LoadingExtension.cs        # Инициализация UI и OverlayManager
├── Utils/HarmonyPatcher.cs    # Harmony патчи (пока не используется)
├── JobsHousingBalanceMod.cs   # Точка входа мода
└── Properties/AssemblyInfo.cs # Метаданные сборки
```

## Реализованные компоненты

### IconButton (UI/IconButton.cs)
- **Размер**: 48x48px, круглый фон с кастомной текстурой
- **Функции**: Перетаскивание мышью, клик для показа панели
- **Иконка**: Загружается из embedded resources (`icons8-demand-48.png`)
- **Позиция**: Правый верхний угол с отступом 300px
- **Особенности**: Правильная обработка координат с учетом UI Scale

### MainPanel (UI/Panel/MainPanel.cs)
- **Размер**: Автоматическая подгонка высоты под контент (ширина 350px)
- **Фон**: "GenericPanel" + полупрозрачность (160 alpha)
- **Элементы**: 
  - Заголовок "Jobs-Housing Balance"
  - Кнопка закрытия (×) с корректной обработкой событий
  - Mode dropdown: "Hex" | "Districts" с синхронизацией через AppState
  - Hex size dropdown: "64m" | "128m" | "256m" | "512m" с enum AppState.HexSize
  - Opacity slider: Диапазон 0.1-0.8 с отображением в процентах
  - Legend: Статичная панель с 4 цветовыми индикаторами (высота 380px)
- **Перетаскивание**: За заголовок (исключая область кнопки закрытия)
- **Позиционирование**: Центрирование на экране
- **Event-driven**: Подписка на события AppState для синхронизации

### AppState (Config/AppState.cs)
- **Паттерн**: Thread-safe Singleton
- **Состояние**: Mode (Hex/Districts), HexSize (64/128/256/512), Opacity (0.1-0.8)
- **События**: OnModeChanged, OnHexSizeChanged, OnOpacityChanged
- **Методы**: Helper методы для преобразования строк ↔ enum
- **Совместимость**: .NET Framework 3.5 (исправлен Enum.TryParse)

### OverlayRenderer (Rendering/Overlay/OverlayRenderer.cs)
- **Базовый класс**: Для наследования конкретными рендерерами
- **Свойства**: IsVisible, CurrentMode, CurrentHexSize, Opacity
- **Методы**: Show/Hide/Toggle, SetMode/SetHexSize/SetOpacity
- **Virtual методы**: OnShow/OnHide/OnModeChanged/OnHexSizeChanged/OnOpacityChanged
- **Утилиты**: GetHexSizeInMeters, GetOpacityPercentage, IsHexMode/IsDistrictsMode

### OverlayManager (OverlayManager.cs)
- **Паттерн**: Thread-safe Singleton
- **Функция**: Связь между UI и OverlayRenderer
- **Подписка**: На события AppState (OnModeChanged, OnHexSizeChanged, OnOpacityChanged)
- **Управление**: Создание и уничтожение OverlayRenderer
- **API**: ToggleOverlay() для переключения видимости

### LoadingExtension (LoadingExtension.cs)
- **Функция**: Инициализация UI и OverlayManager при загрузке уровня
- **Создание**: IconButton и OverlayManager при OnLevelLoaded
- **Очистка**: Уничтожение UI и OverlayManager при OnLevelUnloading
- **Обработка ошибок**: Try-catch блоки с детальным логированием

## Архитектурные принципы

### Event-driven Architecture
- **AppState** как центральный источник истины
- **События** для связи между компонентами (UI ↔ AppState ↔ OverlayRenderer)
- **Слабая связанность** компонентов через события

### Singleton Pattern
- **AppState**: Thread-safe lazy initialization
- **OverlayManager**: Thread-safe lazy initialization
- **Единый экземпляр** для каждого менеджера

### UI Layout Strategy
- **Автоматическая подгонка** основной панели под контент
- **Фиксированные размеры** для стабильности (Legend: 380px)
- **AutoLayout** с правильными отступами и padding

### Error Handling
- **Try-catch блоки** во всех критических местах
- **Детальное логирование** с префиксом "JobsHousingBalance:"
- **Graceful degradation** при ошибках инициализации

## Планируемые компоненты
См. [TASKS.md](TASKS.md) для детального плана всех этапов.