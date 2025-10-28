# Документация проекта

## Основные документы
- **[README.md](README.md)** - Обзор проекта и текущий статус
- **[CURRENT_STATUS.md](CURRENT_STATUS.md)** - Детальное состояние разработки
- **[ETAP_B_2_0_PLAN.md](ETAP_B_2_0_PLAN.md)** - План реализации Этап B 2.0
- **[TASKS.md](TASKS.md)** - План выполнения и задачи
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Архитектура и структура проекта
- **[TECHNICAL_DETAILS.md](TECHNICAL_DETAILS.md)** - Технические детали реализации

## Быстрый старт
- **[QUICK_START.md](QUICK_START.md)** - Команды сборки и текущее состояние

## Разработка
- **[DEVELOPMENT.md](DEVELOPMENT.md)** - Правила разработки и окружение
- **[TESTING.md](TESTING.md)** - Руководство по тестированию
- **[COMMON_MISTAKES.md](COMMON_MISTAKES.md)** - Частые ошибки и их решения
- **[RESEARCH_NOTES.md](RESEARCH_NOTES.md)** - Заметки по исследованию API
- **[DOCS_RULES.md](DOCS_RULES.md)** - Правила обновления документации

## UI компоненты
- **[UI_COMPONENTS.md](UI_COMPONENTS.md)** - Обзор UI компонентов
- **[ui/](ui/)** - Детальные гайды по UI элементам
  - [ui-dropdown.md](ui/ui-dropdown.md)
  - [ui-slider.md](ui/ui-slider.md)
  - [ui-panel.md](ui/ui-panel.md)
  - [ui-drag-drop.md](ui/ui-drag-drop.md)
  - [ui-events.md](ui/ui-events.md)
  - [ui-textures.md](ui/ui-textures.md)
  - [ui-coordinates.md](ui/ui-coordinates.md)

## API документация
- **[api/](api/)** - Документация по игровому API
  - [game-api.md](api/game-api.md)
  - [modding-api.md](api/modding-api.md)
  - [research-methodology.md](api/research-methodology.md)

## Структура документации

### По назначению
- **Обзорные**: README, CURRENT_STATUS, ARCHITECTURE
- **Планирование**: ETAP_B_2_0_PLAN, TECHNICAL_DETAILS
- **Разработка**: DEVELOPMENT, TESTING, COMMON_MISTAKES, RESEARCH_NOTES, DOCS_RULES
- **Справочные**: UI_COMPONENTS, api/, ui/

### По актуальности
- **Актуальные**: README, CURRENT_STATUS, ETAP_B_2_0_PLAN, TECHNICAL_DETAILS, TESTING
- **Справочные**: UI_COMPONENTS, api/, ui/, COMMON_MISTAKES
- **Архивные**: TASKS, RESEARCH_NOTES (исторические заметки)

## Как использовать документацию

1. **Новый разработчик**: README → QUICK_START → ARCHITECTURE → CURRENT_STATUS
2. **Текущая задача**: ETAP_B_2_0_PLAN → TECHNICAL_DETAILS → соответствующие UI гайды
3. **Решение проблем**: COMMON_MISTAKES → RESEARCH_NOTES → api/
4. **Понимание кода**: TECHNICAL_DETAILS → ARCHITECTURE → UI_COMPONENTS

## Обновление документации

- **При завершении задач**: Обновлять CURRENT_STATUS и ETAP_B_2_0_PLAN
- **При изменении архитектуры**: Обновлять ARCHITECTURE и TECHNICAL_DETAILS
- **При обнаружении проблем**: Добавлять в COMMON_MISTAKES
- **При исследовании API**: Добавлять в RESEARCH_NOTES
