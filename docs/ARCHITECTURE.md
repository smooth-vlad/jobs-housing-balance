# Архитектура проекта

## Структура папок
```
src/
├── UI/
│   ├── IconButton.cs          # Круглая кнопка-иконка
│   └── Panel/MainPanel.cs     # Панель управления
├── LoadingExtension.cs         # Инициализация UI
├── Utils/HarmonyPatcher.cs     # Harmony патчи
└── JobsHousingBalanceMod.cs    # Точка входа
```

## UI Компоненты

### IconButton
- Размер: 48x48px, круглый фон
- Перетаскиваемая, кликабельная
- Иконка из embedded resources
- Позиция: правый верхний угол

### MainPanel  
- Размер: 300x400px
- Фон: "GenericPanel" + полупрозрачность
- Заголовок + кнопка закрытия (X)
- Перетаскивание за заголовок
- Центрирование на экране

### Интеграция
- Ленивая инициализация панели
- Toggle-логика показа/скрытия
- Очистка памяти в OnDestroy()

## Будущие компоненты
- DataCollector: сбор данных о зданиях
- AggregatorHex/Districts: агрегация по сетке/районам
- OverlayRenderer: визуализация на карте