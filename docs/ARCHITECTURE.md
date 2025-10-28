# Архитектура проекта

## Структура папок
```
src/
├── UI/
│   ├── IconButton.cs          # Круглая кнопка-иконка (48x48px)
│   └── Panel/MainPanel.cs     # Панель управления (350x400px)
├── LoadingExtension.cs         # Инициализация UI при загрузке уровня
├── Utils/HarmonyPatcher.cs     # Harmony патчи (пока не используется)
├── JobsHousingBalanceMod.cs    # Точка входа мода
└── Properties/AssemblyInfo.cs  # Метаданные сборки
```

## Реализованные компоненты

### IconButton (UI/IconButton.cs)
- **Размер**: 48x48px, круглый фон с кастомной текстурой
- **Функции**: Перетаскивание мышью, клик для показа панели
- **Иконка**: Загружается из embedded resources (`icons8-demand-48.png`)
- **Позиция**: Правый верхний угол с отступом 300px
- **Особенности**: Правильная обработка координат с учетом UI Scale

### MainPanel (UI/Panel/MainPanel.cs)
- **Размер**: 350x400px (увеличен для контролов)
- **Фон**: "GenericPanel" + полупрозрачность (160 alpha)
- **Элементы**: 
  - Заголовок "Jobs-Housing Balance"
  - Кнопка закрытия (×) с корректной обработкой событий
  - Mode dropdown (Hex/Districts) - заглушка
  - Hex size dropdown (64m/128m/256m/512m) - заглушка  
  - Opacity slider (10-80%) - заглушка
  - Legend placeholder
- **Перетаскивание**: За заголовок (исключая область кнопки закрытия)
- **Позиционирование**: Центрирование на экране

### LoadingExtension (LoadingExtension.cs)
- **Функция**: Инициализация UI при загрузке уровня
- **Создание**: IconButton при OnLevelLoaded
- **Очистка**: Уничтожение UI при OnLevelUnloading

## Планируемые компоненты
См. [TASKS.md](TASKS.md) для детального плана всех этапов.