# Технические детали реализации

## Реализованные компоненты

### JobsCapacityCollector.cs
**Расположение**: `src/Data/Collector/JobsCapacityCollector.cs`

**Ключевые особенности**:
- Сбор capacity данных через CalculateWorkplaceCount API методы
- Поддержка всех типов RICO зданий (Commercial, Industrial, Office)
- Трехуровневая стратегия для сервисных зданий: API → reflection → эвристика
- Детекция Realistic Population 2 мода
- Сбор фактической занятости по уровням образования

**Технические детали**:
```csharp
// Использование реального API для capacity
commercialAI.CalculateWorkplaceCount(level, randomizer, width, length, 
    out int uneducated, out int educated, out int wellEducated, out int highlyEducated);

// Трехуровневая стратегия для сервисных зданий
private void GetServiceOrUniqueBuildingCapacityByEducation(PrefabAI prefabAI, ...)

// Детекция RP2 мода по publishedFileID
private bool DetectRP2Mod()
```

### EducationDataCollector.cs
**Расположение**: `src/Data/Collector/EducationDataCollector.cs`

**Ключевые особенности**:
- Сбор данных об образовании жителей с кэшированием
- Поддержка настройки IncludeTeens
- Кэш с инвалидацией по времени (CacheInvalidationIntervalFrames)

**Технические детали**:
```csharp
// Кэширование данных об образовании
private Dictionary<ushort, EducationData> _educationCache;

// Определение работоспособного возраста
private bool IsWorkingAge(Citizen citizen, bool includeTeens)
```

### CitizenEducationHelper.cs
**Расположение**: `src/Data/Collector/CitizenEducationHelper.cs`

**Ключевые особенности**:
- Централизованная логика определения образования граждан
- Кэширование результатов reflection для производительности
- Множественные fallback стратегии для определения образования
- Определение рабочего здания гражданина

**Технические детали**:
```csharp
// Кэш reflection результатов
private static PropertyInfo _educationLevelProperty;
private static FieldInfo _educationLevelField;
// ... другие поля

// Множественные стратегии определения образования
public static int GetCitizenEducationLevel(Citizen citizen)
```

### BuildingDataCollector.cs (обновлен)
**Расположение**: `src/Data/Collector/BuildingDataCollector.cs`

**Ключевые изменения**:
- RICO-only фильтрация зданий
- Усиленная валидация с исключением технических объектов
- Диагностика типов зданий
- Интеграция новых коллекторов

**Технические детали**:
```csharp
// RICO фильтрация
private bool IsRicoBuilding(ItemClass.Service service, ItemClass.SubService subService)

// Усиленная валидация
if ((building.m_flags & (Building.Flags.Untouchable | Building.Flags.Hidden | ...)) != 0) return false;
if (building.m_parentBuilding != 0) return false; // исключаем sub-buildings

// Диагностика
private void LogBuildingTypeDiagnostics()
```

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
- Размер 350px (ширина), автоматическая высота
- Автоматическая компоновка через `autoLayout`
- Правильная настройка z-order для предотвращения перекрытий
- Drag handle исключает область кнопки закрытия
- Динамическая легенда, обновляющаяся при смене режима метрики

**Реализованные контролы**:
- Mode dropdown: `["Hex", "Districts"]` ✅
- Hex size dropdown: `["64m", "128m", "256m", "512m"]` ✅
- Opacity slider: 0.1-0.8 с преобразованием в проценты ✅
- Metric dropdown: `["Fact", "Capacity", "Edu-aware"]` ✅
- IncludeServiceUnique checkbox: включен по умолчанию ✅
- EduMode dropdown: `["Strict", "Substituted"]`, условная видимость ✅
- IncludeTeens checkbox: выключен по умолчанию ✅
- Dynamic Legend: автоматическое обновление при смене режима ✅

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

## Реализованные компоненты (Этап B 2.0 - Фаза 1)

### AppState.cs
**Расположение**: `src/Config/AppState.cs` ✅

**Назначение**: Централизованное хранение состояния приложения

**Реализованные enum'ы**:
```csharp
public enum Mode { Hex, Districts }
public enum HexSize { Size64 = 64, Size128 = 128, Size256 = 256, Size512 = 512 }
public enum MetricType { Fact, Capacity, EduAware }
public enum EducationMode { Strict, Substituted }
```

**Реализованные свойства**:
```csharp
public Mode CurrentMode { get; set; }
public HexSize CurrentHexSize { get; set; }
public float Opacity { get; set; } // 0.1 - 0.8
public MetricType CurrentMetricType { get; set; }
public EducationMode CurrentEducationMode { get; set; }
public bool IncludeServiceUnique { get; set; } // true по умолчанию
public bool IncludeTeens { get; set; } // false по умолчанию
```

**Реализованные события**:
```csharp
public event Action<Mode> OnModeChanged;
public event Action<HexSize> OnHexSizeChanged;
public event Action<float> OnOpacityChanged;
public event Action<MetricType> OnMetricTypeChanged;
public event Action<EducationMode> OnEducationModeChanged;
public event Action<bool> OnIncludeServiceUniqueChanged;
public event Action<bool> OnIncludeTeensChanged;
```

### BuildingSample.cs
**Расположение**: `src/Data/Collector/BuildingSample.cs` ✅

**Назначение**: Структура данных для хранения информации о здании

**Реализованные поля**:
```csharp
public ushort buildingId;
public Vector3 position;
public byte districtId;
public int residentsFact;           // Фактическое количество жителей
public int jobsFact;               // Фактическое количество рабочих мест
public int jobsCapacityTotal;      // Общая ёмкость рабочих мест
public int[] jobsCapacityByEdu;    // Ёмкость по уровням образования (размер 4)
public int[] residentsByEdu;       // Жители по уровням образования (размер 4)
public ItemClass.Service service;
public ItemClass.SubService subService;
```

**Helper свойства**:
```csharp
public int Balance => jobsFact - residentsFact;
public bool IsResidential => service == ItemClass.Service.Residential;
public bool IsNonResidential => service != ItemClass.Service.Residential;
public bool HasSignificantBalance => residentsFact > 0 || jobsFact > 0;
public bool IsServiceBuilding => false; // TODO: Implement proper detection
public bool IsUniqueBuilding => false; // TODO: Implement proper detection
public int TotalResidentsByEdu { get; }
public int TotalJobsCapacityByEdu { get; }
public bool HasCapacityData => jobsCapacityTotal > 0 || TotalJobsCapacityByEdu > 0;
public bool HasEducationData => TotalResidentsByEdu > 0;
```

### OverlayRenderer.cs
**Расположение**: `src/Rendering/Overlay/OverlayRenderer.cs` ✅

**Назначение**: Базовый класс для рендеринга оверлеев

**Реализованные методы**:
```csharp
public void Show()
public void Hide()
public void SetOpacity(float opacity)
public void SetMode(Mode mode)
public void SetHexSize(HexSize size)
```

**Интеграция**: Связь с AppState через события

## Планируемые компоненты (Этап B 2.0 - Фазы 2-4)

### JobsCapacityCollector.cs
**Расположение**: `src/Data/Collector/JobsCapacityCollector.cs` (планируется)

**Назначение**: Сбор данных о потенциальной ёмкости рабочих мест

**Планируемые методы**:
```csharp
public int GetJobsCapacity(ushort buildingId)
public int[] GetJobsCapacityByEducation(ushort buildingId)
public bool IsRP2Active()
public void CacheCapacityData(ushort buildingId)
```

### EducationDataCollector.cs
**Расположение**: `src/Data/Collector/EducationDataCollector.cs` (планируется)

**Назначение**: Сбор данных об образовании жителей

**Планируемые методы**:
```csharp
public int[] GetResidentsByEducation(ushort buildingId)
public Citizen.Education GetCitizenEducation(uint citizenId)
public bool ShouldIncludeTeen(uint citizenId)
```

### EducationMatcher.cs
**Расположение**: `src/Data/Collector/EducationMatcher.cs` (планируется)

**Назначение**: Логика сопоставления по образованию

**Планируемые методы**:
```csharp
public int CalculateEducationBalance(int[] jobsCapacity, int[] residents, EducationMode mode)
public int CalculateStrictBalance(int[] jobsCapacity, int[] residents)
public int CalculateSubstitutedBalance(int[] jobsCapacity, int[] residents)
```

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

### UIComponent - spriteName property
**Проблема**: `UIComponent` не содержит `spriteName` property
**Решение**: Приводить к `UISprite` перед обращением к `spriteName`
**Статус**: Решено

### UIPanel - RemoveAllChildren method
**Проблема**: `UIPanel` не содержит `RemoveAllChildren` method
**Решение**: Использовать цикл с `RemoveUIComponent` и `Destroy`
**Статус**: Решено

## Этап B 2.0 - Детали реализации
См. [ETAP_B_2_0_PLAN.md](ETAP_B_2_0_PLAN.md) для детального плана Фаз 2-4.
