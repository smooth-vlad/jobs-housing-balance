# Задачи проекта (Project Tasks)

## Правила разработки (Development Rules)

- **Целевая версия игры**: Cities: Skylines (Original) — не Cities: Skylines II
- **UI Framework**: ColossalFramework.UI (нативный фреймворк игры для максимальной совместимости)
- **Обновление данных**: Только периодическое обновление (каждые 5-10 секунд), без хуков на события строительства
- **Совместимость**: Мод должен работать с популярными модами (Realistic Population, TM:PE, RICO) без конфликтов
- **Tooltip**: Не реализуется на данный момент

## Окружение разработки (Development Environment)

- **OS**: macOS
- **Framework**: Mono framework для разработки
- **IDE**: Cursor (VS Code fork)

### Технические параметры (Technical Settings)

- **Имя мода**: JobsHousingBalance
- **Версия**: 0.1
- **Автор**: Vladislav Gladkii
- **Фреймворк для модификации**: Harmony (будет добавлен в Task 4)
- **Путь к DLL игры**: `/Users/vladislav/Library/Application Support/Steam/steamapps/common/Cities_Skylines/Cities.app/Contents/Resources/Data/Managed/`
- **Путь к папке модов**: `~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/` (**ВАЖНО**: ~ вместо /)
- **Путь к Harmony DLL**: `~/Library/Application Support/Steam/steamapps/workshop/content/255710/2040656402/CitiesHarmony.dll`
- **Компилятор**: Mono xbuild или msbuild
- **Target Framework**: .NET Framework 3.5

## MVP: Базовая структура мода (Basic Mod Structure)

Цель: Реализовать минимальный мод, который логирует событие загрузки карты в консоль игры и инициализируется при запуске.

### Детальный план подзадач:

#### 1. Настройка проекта (.csproj) ✅
- [x] Создан `.csproj` файл для .NET Framework 3.5 (совместим с Cities: Skylines)
- [x] Добавлены ссылки на необходимые DLL:
  - `Assembly-CSharp.dll` (вместо ColossalFramework.dll и Cities.dll)
  - `ColossalManaged.dll`
  - `ICities.dll`
  - `UnityEngine.dll`
  - `UnityEngine.UI.dll`
  - `System.dll`, `System.Core.dll`
- [x] Настроены пути к DLL в `<HintPath>` элементах
- [x] Указан OutputPath для сборки в папку модов: `~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/` (**ИСПРАВЛЕНО**: было `/Library`, должно быть `~/Library`)
- [x] **Замечание:** `CitiesHarmony.dll` нужно добавить в следующих задачах (Task 4)
  - Путь: `~/Library/Application Support/Steam/steamapps/workshop/content/255710/2040656402/CitiesHarmony.dll`

#### 2. Metadata мода ✅
- [x] Создан файл `Properties/AssemblyInfo.cs`
  - [x] AssemblyTitle: "JobsHousingBalance"
  - [x] AssemblyDescription: "Visualize jobs-housing balance overlay"
  - [x] AssemblyVersion: "0.1.0.0"
  - [x] AssemblyCompany: "Vladislav Gladkii"
- [x] Установлены атрибуты качества кода (ComVisible(false), CLSCompliant(false))
- [x] Исправлено: добавлен `using System;` для CLSCompliantAttribute

#### 3. Точка входа мода (Entry Point) ✅
- [x] Создан класс `JobsHousingBalanceMod` в корне `src/`
- [x] Реализован интерфейс `ICities.IUserMod`:
  - [x] `Name` property: "Jobs Housing Balance"
  - [x] `Description` property: "Shows visual overlay of jobs vs housing balance"
- [x] Добавлены методы `OnEnabled()` и `OnDisabled()` с логированием

#### 4. Инициализация Harmony ✅
- [x] Добавить reference на CitiesHarmony.dll и CitiesHarmony.Harmony.dll из Steam Workshop:
  - Путь: `~/Library/Application Support/Steam/steamapps/workshop/content/255710/2040656402/CitiesHarmony.dll`
  - Путь: `~/Library/Application Support/Steam/steamapps/workshop/content/255710/2040656402/CitiesHarmony.Harmony.dll`
  - Mod ID: 2040656402 (Harmony)
- [x] Создан класс `HarmonyPatcher` в `src/Utils/`
- [x] В методе `OnEnabled()` применены Harmony патчи
- [x] В методе `OnDisabled()` удаляются патчи
- [x] Добавлены статические методы `ApplyPatches()` и `RemovePatches()`

#### 5. Обработчик загрузки игры ✅
- [x] Создан класс `LoadingExtension` наследующий `LoadingExtensionBase`
- [x] Реализован метод `OnLevelLoaded(LoadMode mode)`
- [x] Добавлено `Debug.Log("JobsHousingBalance: Level loaded successfully")` для проверки
- [x] Добавлен метод `OnLevelUnloading()` с логированием

#### 6. Регистрация мода ✅
- [x] `LoadingExtension` автоматически обнаруживается игрой (Cities: Skylines сам регистрирует все классы, наследующие `LoadingExtensionBase`)
- [x] Добавлено логирование успешного запуска мода
- [x] Убрана попытка ручной регистрации через `LoadingManager.AddLoadingExtension()` (неверный API)
- [x] Инициализация `HarmonyPatcher` завершена в Task 4

#### 7. Тестовая сборка и установка ✅
- [x] Собран проект с помощью msbuild: `msbuild JobsHousingBalance.csproj /p:Configuration=Release`
- [x] DLL создан в OutputPath: `~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/`
- [x] Размер DLL: 6.0KB (+ 1.8KB .pdb для отладки)
- [x] Формат: PE32 executable (DLL) for .NET/Mono
- [x] Исправлен путь: было `/Library`, стало `~/Library` (домашняя папка пользователя)
- [x] Проверка в игре: мод появился в списке модов ✅

#### 8. Тестирование в игре ✅
- [x] Запущен Cities: Skylines
- [x] Мод появился в Content Manager → Mods
- [x] Мод успешно загружается при старте игры
- [x] Логи показывают работу мода (требует дальнейшей проверки при загрузке карты)
- [x] Мод не вызывает ошибок при загрузке
- [ ] Проверка совместимости с другими модами (планируется)

### Критерии завершения MVP:
✅ Мод появляется в списке модов игры  
✅ Мод корректно загружается при старте игры  
✅ Логи показывают работу мода  
✅ Мод не вызывает ошибок или крашей  
✅ Базовая структура готова для добавления UI и функционала

**Статус:** Все задачи MVP (Tasks 1-8) **ЗАВЕРШЕНЫ**, включая инициализацию Harmony (Task 4)

## Логирование

### Подход:
Мод использует стандартное Unity логирование (`Debug.Log`, `Debug.LogWarning`, `Debug.LogError`) со всеми сообщениями, предваряемыми префиксом "JobsHousingBalance: ".

### Как просматривать логи:
Для просмотра логов мода используйте мод **ModTools** (2040656402):
1. Установите ModTools через Steam Workshop
2. В игре нажмите `F7` для открытия консоли мода
3. Фильтруйте логи по префиксу "JobsHousingBalance"
4. Все логи мода будут видны в реальном времени в консоли ModTools

### Технические детали:

**Сборка:**
```bash
/Library/Frameworks/Mono.framework/Versions/6.12.0/bin/msbuild JobsHousingBalance.csproj /p:Configuration=Release
```

**Путь установки DLL:**
```
~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/
```

**Размер DLL:** ~6KB

### Что НЕ нужно для MVP:

- Task 2: Утилиты для гексов
- Task 3: Сбор данных
- Task 4-5: Агрегация
- Task 6: Нормализация и цветовое кодирование
- Task 7: Система рендеринга оверлея
- Task 8: Главный контроллер/менеджер
- Task 9-10: UI элементы управления и панель
- Полноценный функционал визуализации

## Основные задачи инфраструктуры

### 1. Базовая структура и конфигурация мода
- Создать базовую структуру мода (metadata, AssemblyInfo, mod entry point)
- Настроить Harmony патчи для перехвата рендеринга (если необходимо)
- Определить конфигурационные параметры (размеры hex, цвета, частота обновления, размещение UI)
- Реализовать сохранение/загрузку настроек
- Инициализировать ColossalFramework.UI для создания нативных UI элементов

### 2. Утилиты для работы с гексами
- Реализовать систему координат для гексагональной сетки (аксиальные координаты q,r)
- Добавить функции конвертации координат: world → hex (pointy-top, мировые координаты)
- Реализовать алгоритм округления до ближайшего гекса (cube rounding) для преобразования world позиций в hex ячейки
- Добавить обратное преобразование hex → world для рендеринга

### 3. Модуль сбора данных
- Реализовать обход BuildingManager для получения всех зданий
- Подсчитывать количество жителей (CitizenUnits типа Home) на здание
- Подсчитывать количество рабочих мест (CitizenUnits типа Work) на здание
- Получать позицию здания и ID района
- Хранить собранные данные в удобной структуре

### 4. Агрегация по гексам
- Реализовать модуль AggregatorHex:
  - Распределять здания по hex ячейкам по их world позиции
  - Суммировать residents/jobs внутри каждой hex ячейки
  - Рассчитывать разницу `diff = jobs - residents` для каждой ячейки
  - Хранить результаты в словаре Map<(q,r), (residents, jobs, diff)>
  - Пересчитывать при обновлении данных

### 5. Агрегация по районам
- Реализовать модуль AggregatorDistricts:
  - Группировать здания по districtId
  - Суммировать residents/jobs внутри каждого района
  - Рассчитывать разницу `diff = jobs - residents` для каждого района
  - Хранить результаты в Map<districtId, (residents, jobs, diff)>

### 6. Нормализация и цветовое кодирование
- Рассчитывать максимальное абсолютное значение разности для текущего режима
- Вычислять коэффициент насыщенности t = |diff| / maxAbs
- Применять цветовое кодирование: положительные значения (синий), отрицательные (красный)
- Применять глобальную непрозрачность

### 7. Система рендеринга оверлея
- Реализовать OverlayRenderer:
  - Рендеринг hex ячеек (GL/OverlayEffect)
  - Рендеринг районов
  - Отрисовка с правильным цветом и прозрачностью
  - Отображение текстовых меток на каждом гексе/районе (показывает `jobs - residents`)
  - Обработка границ видимости камеры
  - Оптимизация отрисовки текста (могут показываться только для достаточно больших значений)

### 8. Главный контроллер/менеджер
- Создать контроллер для управления:
  - Переключение режимов (Hex/Districts)
  - Периодическое обновление данных (каждые 5-10 секунд игрового времени)
  - Управление кэшем
  - Триггеры обновления при изменении размера hex
  - Связь между UI, DataCollector, Aggregators и Renderer

### 9. UI элементы управления
- Создать dropdown для выбора режима (Hex/Districts)
- Создать dropdown для выбора размера hex (64/128/256/512; disabled при Districts)
- Создать slider для непрозрачности (0.1-0.8)
- Добавить легенду/подсказку с объяснением цветов
- Добавить кнопку включения/выключения слоя

### 10. Интеграция UI панели
- Создать и зарегистрировать UI панель в игре
- Расположить элементы управления из задачи 9
- Связать состояние с рендерингом через контроллер
- Реализовать логику показа/скрытия панели
- Создать жизненный цикл (OnCreate, OnDestroy, OnUpdate)

## Дополнительные задачи

### 11. Оптимизация производительности
- Кэшировать список зданий между обновлениями
- Кэшировать агрегированные данные по hex при изменении размера
- Оптимизировать вызовы отрисовки GL
- Добавить culling для невидимых элементов

### 12. Тестирование и валидация
- Проверить работу на малых городах
- Сверить подсчеты с данными игры
- Провести тестирование производительности на больших городах
- Проверить совместимость с Realistic Population, TM:PE, RICO

### 13. Доводка и граничные случаи
- Обработать пустые ячейки (не рисовать)
- Обработать районы без ID (districtId = 0)
- Обработать пустые районы (без зданий)

