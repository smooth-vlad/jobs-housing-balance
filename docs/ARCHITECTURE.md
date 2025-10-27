# Архитектура проекта (Project Architecture)

## Технические требования (Technical Requirements)
- **Целевая версия**: Cities: Skylines (Original)
- **UI Framework**: ColossalFramework.UI
- **Обновление данных**: Периодическое (5-10 секунд)
- **Совместимость**: Realistic Population, TM:PE, RICO

## Структура папок (Folder Structure)

```
src/
├── Data/
│   └── Collector/          # Сбор данных о зданиях и жителях
│
├── Aggregation/
│   ├── Hex/               # Агрегация по гексагональной сетке
│   └── Districts/         # Агрегация по районам
│
├── Rendering/
│   └── Overlay/           # Визуализация оверлея на карте
│
├── UI/
│   ├── Panel/             # Основная панель управления
│   └── Controls/          # UI элементы (dropdowns, sliders, etc.)
│
├── Utils/                 # Утилиты (hex-координаты, нормализация, etc.)
│
└── Config/                # Конфигурационные файлы
```

## Компоненты (Components)

### DataCollector
**Расположение:** `src/Data/Collector/`

Собирает данные о зданиях из игрового менеджера:
- Обход `BuildingManager.m_buildings`
- Подсчёт `residents` (Home) и `jobs` (Work) из `CitizenUnits`
- Получение позиции здания и номера района

### AggregatorHex
**Расположение:** `src/Aggregation/Hex/`

Агрегирует данные по гексагональной сетке:
- Преобразование позиции `Vector3(x,z)` → аксиальные координаты (q,r)
- Суммирование `residents/jobs` в каждой ячейке
- Расчет разницы `diff = jobs - residents` для отображения
- Хранение словаря `Map<(q,r) -> (residents, jobs, diff)>`

### AggregatorDistricts
**Расположение:** `src/Aggregation/Districts/`

Агрегирует данные по районам:
- Группировка по `byte districtId`
- Суммирование `residents/jobs` внутри каждого района
- Расчет разницы `diff = jobs - residents` для отображения

### Normalizer
**Расположение:** `src/Utils/` (или отдельный компонент)

Вычисляет нормализацию для цветовой шкалы:
- `maxAbs = max(|jobs − residents|)` для текущего режима
- Коэффициент насыщенности цвета

### OverlayRenderer
**Расположение:** `src/Rendering/Overlay/`

Отрисовывает оверлей на карте:
- Рисует гексагоны/заливки районов полу-прозрачным цветом
- Использует GL/OverlayEffect
- Отображает числовые метки на каждом гексе/районе (показывает `jobs - residents`)
- Отображает легенду в UI панели

### UI Panel
**Расположение:** `src/UI/Panel/` и `src/UI/Controls/`

Пользовательский интерфейс:
- Переключатели режима (Hex/Districts)
- Выбор размера гекса
- Ползунок непрозрачности
- Кнопка вкл/выкл слоя
