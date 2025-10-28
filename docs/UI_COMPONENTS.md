# UI Components Overview

## Основные компоненты

### UIDropDown
- **Гайд**: `ui/ui-dropdown.md`
- **Основные свойства**: `textScale`, `verticalAlignment`, `horizontalAlignment`, `textFieldPadding`
- **Спрайты**: `"GenericPanel"`, `"GenericPanelLight"`
- **Ограничение**: Текст всегда прижат к верху - НЕ РЕШАЕТСЯ

### UISlider  
- **Гайд**: `ui/ui-slider.md`
- **Правильные спрайты**: `"SliderBudget"`, `"SliderFill"`, `"ScrollbarThumb"`
- **Настройка**: `clipChildren = true`, правильный порядок создания объектов

### UIPanel
- **Гайд**: `ui/ui-panel.md`
- **Наследование**: от `UIPanel` (не `UIComponent`)
- **Фон**: `"GenericPanel"` + полупрозрачность
- **Перетаскивание**: `UIDragHandle`

### Drag & Drop
- **Гайд**: `ui/ui-drag-drop.md`
- **Для панелей**: `UIDragHandle`
- **Для кнопок**: своя обработка мыши
- **Координаты**: `ScreenToParentLocal()` для правильного преобразования

### Events
- **Гайд**: `ui/ui-events.md`
- **Клики**: `eventClick` с проверкой на drag
- **Порядок**: `MouseDown` → `MouseMove` → `MouseUp` → `Click`
- **Защита**: анти-спам cooldown

## Общие принципы
- **Текстуры**: `ui/ui-textures.md`
- **Координаты**: `ui/ui-coordinates.md`
- **Исследование API**: `api/research-methodology.md`
