# Quick Start

## Текущее состояние
- ✅ **MVP 1 завершен**: Базовая UI система работает
- 🔄 **Этап A в процессе**: Доработка контролов и создание скелета оверлея

## Основные команды
```bash
# Сборка проекта
/Library/Frameworks/Mono.framework/Versions/6.12.0/bin/msbuild JobsHousingBalance.csproj /p:Configuration=Release

# Просмотр логов
ModTools (F7) в игре → фильтр "JobsHousingBalance"
```

## Что работает сейчас
1. **Кнопка-иконка**: Круглая кнопка 48x48px в правом верхнем углу
2. **Перетаскивание**: Кнопка перетаскивается мышью
3. **Панель управления**: 350x400px с заголовком и кнопкой закрытия
4. **Toggle-логика**: Панель показывается/скрывается по клику
5. **UI контролы**: Mode dropdown, Hex size dropdown, Opacity slider, Legend (пока заглушки)

## Структура проекта
```
src/
├── UI/
│   ├── IconButton.cs          # Кнопка-иконка (работает)
│   └── Panel/MainPanel.cs     # Панель управления (работает)
├── LoadingExtension.cs         # Инициализация UI (работает)
├── Utils/HarmonyPatcher.cs     # Harmony патчи (не используется)
└── JobsHousingBalanceMod.cs    # Точка входа (работает)
```

## Этап A - Текущие задачи
См. [TASKS.md](TASKS.md) для детального плана этапа A.

## Основные правила
- Читайте docs перед кодированием
- Исследуйте API через интернет
- Проверяйте на `null`
- Используйте отладочные логи
- См. [TECHNICAL_DETAILS.md](TECHNICAL_DETAILS.md) для деталей реализации
