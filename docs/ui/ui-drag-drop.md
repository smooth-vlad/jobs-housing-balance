# UI Drag & Drop Guide

## Основные принципы
- Используйте `UIDragHandle` для панелей/окон
- Для одиночных кнопок - своя обработка мыши (не конфликтует с кликом)

## Drag & Drop для кнопки
```csharp
public sealed class DraggableButton : UIButton
{
    private bool _dragging;
    private Vector2 _dragStartLocalInParent;
    private Vector2 _dragStartRel;
    private const float DragThreshold = 6f;

    public override void Start()
    {
        base.Start();
        
        eventMouseDown += OnDown;
        eventMouseMove += OnMove;
        eventMouseUp += OnUp;
        eventClick += OnClick;
    }

    private void OnDown(UIComponent c, UIMouseEventParameter p)
    {
        if (p.buttons != UIMouseButton.Left) return;
        _dragging = true;
        _dragStartRel = relativePosition;
        _dragStartLocalInParent = ScreenToParentLocal(Input.mousePosition);
        BringToFront();
        p.Use();
    }

    private void OnMove(UIComponent c, UIMouseEventParameter p)
    {
        if (!_dragging) return;
        Vector2 curLocal = ScreenToParentLocal(Input.mousePosition);
        Vector2 delta = curLocal - _dragStartLocalInParent;
        relativePosition = ClampToScreen(_dragStartRel + delta);
        p.Use();
    }
}
```

## Координаты и масштабирование
- Используйте `ScreenToParentLocal()` для правильного преобразования координат
- Учитывайте `transform.lossyScale` родителя
- Всегда используйте `relativePosition`, не `absolutePosition`

## Частые ошибки
- ❌ Смешивание экранных пикселей с UI-координатами
- ❌ Использование `Input.mousePosition` без преобразования
- ❌ Отсутствие проверки на `DragThreshold`