# Текущее состояние разработки

## MVP 1 - Завершено ✅

### Базовая структура проекта
- ✅ Настройка проекта (.csproj, AssemblyInfo)
- ✅ Точка входа (JobsHousingBalanceMod)
- ✅ Harmony патчи (HarmonyPatcher)
- ✅ LoadingExtension (OnLevelLoaded/Unloading)
- ✅ Тестирование в игре

### UI система (Task 9)
- ✅ **9.1**: Круглая кнопка-иконка (48x48px, перетаскиваемая)
- ✅ **9.2**: Перетаскивание кнопки мышью с правильной обработкой координат
- ✅ **9.3**: Обработка кликов (различение клик/драг)
- ✅ **9.4**: UI панель (350x400px, фон, заголовок, кнопка X)
- ✅ **9.5**: Toggle-логика показа/скрытия панели
- ✅ **9.6**: Исправление функциональности кнопки закрытия
- ✅ **9.7**: Интеграция в LoadingExtension
- ✅ **9.8**: Элементы управления в панели (заглушки)

## Этап A - Завершен ✅

### Задачи этапа A
- ✅ **A.1**: Доработка UI контролов (Mode, Hex size, Opacity, Legend)
- ✅ **A.2**: Создание AppState для хранения состояния
- ✅ **A.3**: Создание скелета OverlayRenderer
- ✅ **A.4**: Интеграция UI ↔ Overlay через события
- ✅ **A.5**: Правильная Legend с описанием цветов

### Реализованные компоненты
- **AppState**: Singleton с событиями для Mode, HexSize, Opacity
- **OverlayRenderer**: Базовый класс для рендеринга оверлеев
- **OverlayManager**: Менеджер для связи UI и OverlayRenderer
- **UI Controls**: Исправленные dropdowns и slider с правильными значениями
- **Legend**: Статичная легенда с цветовыми индикаторами и автоматической подгонкой размеров

## Отложенные задачи ⏸️

### Task 9.7: Сохранение позиции кнопки - ПОКА НЕ ДЕЛАЕМ
- ⏸️ При изменении позиции сохранять координаты
- ⏸️ Использовать GameSettings.AddSettingsFile
- ⏸️ При загрузке читать сохраненную позицию
- ⏸️ Дефолтная позиция если не сохранена

## Этап B 2.0 - В процессе

### Фаза 1: Подготовка инфраструктуры ✅
- ✅ **1.1**: Расширение AppState (MetricType, EducationMode, IncludeServiceUnique, IncludeTeens)
- ✅ **1.2**: Расширение BuildingSample (jobsCapacityTotal, jobsCapacityByEdu, residentsByEdu)
- ✅ **1.3**: Обновление UI Layout (новые контролы, динамическая легенда)

### Фаза 2: Сбор данных о емкости рабочих мест ✅
- ✅ **2.1**: JobsCapacityCollector - сбор capacity данных с API методов
- ✅ **2.2**: EducationDataCollector - сбор данных об образовании жителей
- ✅ **2.3**: CitizenEducationHelper - централизованная логика определения образования
- ✅ **2.4**: RICO-only фильтрация зданий (Residential, Commercial, Industrial, Office)
- ✅ **2.5**: Улучшенная валидация и диагностика зданий

### Реализованные компоненты Этапа B 2.0
- **AppState**: Новые enum'ы и свойства для Capacity/Edu-aware режимов
- **BuildingSample**: Поля для capacity и education данных
- **UI Controls**: Metric dropdown, IncludeServiceUnique checkbox, EduMode dropdown, IncludeTeens checkbox
- **Dynamic Legend**: Автоматическое обновление легенды при смене режима метрики
- **JobsCapacityCollector**: Сбор capacity данных через CalculateWorkplaceCount API
- **EducationDataCollector**: Сбор данных об образовании жителей с кэшированием
- **CitizenEducationHelper**: Централизованная логика определения образования через reflection
- **RICO Filtering**: Фильтрация только RICO зданий, исключение технических объектов
- **Building Validation**: Усиленная валидация с исключением sub-buildings и технических флагов

## Планируемые этапы
См. [ETAP_B_2_0_PLAN.md](ETAP_B_2_0_PLAN.md) для детального плана Фаз 2-4.

## Технические детали

### Реализованные особенности
- **Drag & Drop**: Правильная обработка координат с учетом UI Scale
- **Events**: Анти-спам защита, различение клик/драг
- **UI Components**: Использование правильных спрайтов и настроек
- **Memory Management**: Очистка в OnDestroy()
- **Event-driven Architecture**: AppState ↔ UI ↔ OverlayRenderer через события
- **Singleton Pattern**: Thread-safe реализация для AppState и OverlayManager
- **Auto-sizing UI**: Автоматическая подгонка размеров панелей под контент
- **Dynamic Legend**: Автоматическое обновление легенды при смене режима метрики
- **Conditional UI**: EduMode dropdown появляется только в Edu-aware режиме
- **Metric Types**: Поддержка Fact, Capacity, Edu-aware режимов
- **API Integration**: Использование CalculateWorkplaceCount для точного определения capacity
- **Reflection Caching**: Кэширование результатов reflection для повышения производительности
- **Education Detection**: Определение уровня образования через reflection с fallback на возраст
- **RICO Filtering**: Фильтрация только RICO зданий, исключение технических объектов
- **Building Validation**: Усиленная валидация с исключением sub-buildings и технических флагов
- **Diagnostics**: Детальная диагностика типов зданий для понимания что попадает в подсчет

### Технические решения
- **Mode dropdown**: "Hex" | "Districts" с правильной синхронизацией
- **Hex size dropdown**: "64m" | "128m" | "256m" | "512m" с enum AppState.HexSize
- **Opacity slider**: Диапазон 0.1-0.8 с преобразованием в проценты для отображения
- **Metric dropdown**: "Fact" | "Capacity" | "Edu-aware" с динамическим обновлением легенды
- **IncludeServiceUnique checkbox**: Включен по умолчанию (true)
- **IncludeTeens checkbox**: Выключен по умолчанию (false)
- **EduMode dropdown**: "Strict" | "Substituted", появляется только в Edu-aware режиме
- **Dynamic Legend**: Автоматическое обновление при смене режима метрики
- **UI Layout**: Использование autoLayout с фиксированными размерами для стабильности

### Известные ограничения
- UIDropDown: Текст всегда прижат к верху (не решается)
- .NET Framework 3.5: Отсутствие Enum.TryParse и ToArray() - исправлено

## Файлы проекта
- ✅ `src/Config/AppState.cs` - централизованное состояние приложения с событиями и новыми enum'ами
- ✅ `src/Data/Collector/BuildingSample.cs` - структура данных с полями для capacity и education
- ✅ `src/Data/Collector/JobsCapacityCollector.cs` - сбор capacity данных через CalculateWorkplaceCount API
- ✅ `src/Data/Collector/EducationDataCollector.cs` - сбор данных об образовании жителей с кэшированием
- ✅ `src/Data/Collector/CitizenEducationHelper.cs` - централизованная логика определения образования через reflection
- ✅ `src/Data/Collector/BuildingDataCollector.cs` - обновленный сборщик с RICO фильтрацией и диагностикой
- ✅ `src/Rendering/Overlay/OverlayRenderer.cs` - базовый класс для рендеринга оверлеев
- ✅ `src/OverlayManager.cs` - менеджер для связи UI и OverlayRenderer
- ✅ `src/UI/Panel/MainPanel.cs` - обновленная панель с новыми контролами и динамической легендой
- ✅ `src/LoadingExtension.cs` - интеграция OverlayManager в жизненный цикл мода
