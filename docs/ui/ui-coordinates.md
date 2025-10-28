# UI Coordinates Guide

## Системы координат
- **Экранные**: `Input.mousePosition` (пиксели экрана)
- **UI**: `p.position`, `p.moveDelta` (координаты UI)
- **Локальные**: `relativePosition` (относительно родителя)

## Правильное преобразование координат
```csharp
private Vector2 ScreenToParentLocal(Vector3 mouseScreen)
{
    var par = (parent ?? (UIComponent)UIView.GetAView());
    var ui = par.GetUIView();
    Vector2 gui = ui.ScreenPointToGUI(mouseScreen); // учитывает DPI/UI Scale
    return par.ScreenToLocal(gui);                   // в локаль РОДИТЕЛЯ
}
```

## Учет масштабирования
```csharp
// Компенсация масштаба родителя
var pr = (parent ?? (UIComponent)UIView.GetAView()).transform.lossyScale;
if (pr.x != 0f && pr.y != 0f) 
    delta = new Vector2(delta.x / pr.x, delta.y / pr.y);
```

## Частые ошибки
- ❌ Смешивание экранных пикселей с UI-координатами
- ❌ Использование `Input.mousePosition` без преобразования
- ❌ Игнорирование UI Scale (110% → +10% движения)
- ❌ Использование `absolutePosition` вместо `relativePosition`