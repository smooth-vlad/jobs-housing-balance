# Технические детали реализации

## Реализованные компоненты

### IconButton.cs
**Расположение**: `src/UI/IconButton.cs`

**Ключевые особенности**:
- Кастомная круглая текстура с цветами `#53555B` (фон) и `#508A33` (граница)
- Иконка загружается из embedded resources (`icons8-demand-48.png`)
- Правильная обработка drag & drop с учетом UI Scale
- Анти-спам защита для кликов
- Ленивая инициализация MainPanel

**Технические детали**:
```csharp
// Drag threshold для различения клик/драг
private const float DragThreshold = 6f;

// Правильное преобразование координат
private Vector2 ScreenToParentLocal(Vector3 mouseScreen)

// Создание кастомной круглой текстуры
private void CreateCircularBackground()
```

### MainPanel.cs
**Расположение**: `src/UI/Panel/MainPanel.cs`

**Ключевые особенности**:
- Размер 350x400px (увеличен для контролов)
- Автоматическая компоновка через `autoLayout`
- Правильная настройка z-order для предотвращения перекрытий
- Drag handle исключает область кнопки закрытия

**Текущие контролы (заглушки)**:
- Mode dropdown: `["Small", "Medium", "Large"]` → нужно `["Hex", "Districts"]`
- Hex size dropdown: `["Small", "Medium", "Large"]` → нужно `["64m", "128m", "256m", "512m"]`
- Opacity slider: 0-100% → нужно 0.1-0.8 с преобразованием
- Legend: placeholder → нужно правильное описание

**Технические детали**:
```csharp
// Правильная настройка dropdown
dropdown.verticalAlignment = UIVerticalAlignment.Middle;
dropdown.textFieldPadding = new RectOffset(8, 0, 0, 0);

// Правильные спрайты для slider
baseTrack.spriteName = "ScrollbarTrack";
fill.spriteName = "SliderFill";
thumb.spriteName = "ScrollbarThumb";
```

### LoadingExtension.cs
**Расположение**: `src/LoadingExtension.cs`

**Функции**:
- Создание IconButton при загрузке уровня
- Очистка UI при выгрузке уровня
- Обработка ошибок инициализации

## Планируемые компоненты (Этап A)

### AppState.cs
**Расположение**: `src/Config/AppState.cs` (создать)

**Назначение**: Централизованное хранение состояния приложения

**Поля**:
```csharp
public enum Mode { Hex, Districts }
public enum HexSize { Size64 = 64, Size128 = 128, Size256 = 256, Size512 = 512 }

public Mode CurrentMode { get; set; }
public HexSize CurrentHexSize { get; set; }
public float Opacity { get; set; } // 0.1 - 0.8
```

**События**:
```csharp
public event Action<Mode> ModeChanged;
public event Action<HexSize> HexSizeChanged;
public event Action<float> OpacityChanged;
```

### OverlayRenderer.cs
**Расположение**: `src/Rendering/OverlayRenderer.cs` (создать)

**Назначение**: Скелет системы визуализации

**Методы**:
```csharp
public void Show()
public void Hide()
public void SetOpacity(float opacity)
public void SetMode(Mode mode)
public void SetHexSize(HexSize size)
```

**Интеграция**: Связь с AppState через события

## Известные проблемы и решения

### UIDropDown - Текст прижат к верху
**Проблема**: Стандартные способы вертикального выравнивания не работают
**Решение**: Использовать `textFieldPadding` только для горизонтальных отступов
**Статус**: Не решается, работаем с ограничением

### Drag & Drop - Координаты
**Проблема**: Смешивание экранных пикселей с UI-координатами
**Решение**: Использовать `ScreenToParentLocal()` с учетом UI Scale
**Статус**: Решено

### UISlider - Спрайты
**Проблема**: Неправильные спрайты создают "прямоугольник" в углу
**Решение**: Использовать `"ScrollbarTrack"`, `"SliderFill"`, `"ScrollbarThumb"`
**Статус**: Решено

## Этап A - Детали реализации
См. [TASKS.md](TASKS.md) для детального плана этапа A.
