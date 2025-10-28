# Research Notes

## UIDropDown - Выравнивание текста

### Проблема
Текст в UIDropDown всегда прижат к верхней части элемента. Стандартные способы вертикального выравнивания не работают.

### Попытки решения (все не сработали)
- ❌ `verticalAlignment = UIVerticalAlignment.Middle`
- ❌ `textFieldPadding = new RectOffset(8, 0, 0, 0)`
- ❌ Увеличение высоты dropdown'а
- ❌ Поиск внутренних компонентов через `Find<UILabel>("Label")`
- ❌ Отключение `autoSize`
- ❌ `foregroundSpriteMode = UIForegroundSpriteMode.Stretch`

### Что работает
```csharp
// Горизонтальное выравнивание и отступы работают
dropdown.textScale = 0.9f;
dropdown.textColor = Color.white;
dropdown.horizontalAlignment = UIHorizontalAlignment.Left;
dropdown.textFieldPadding = new RectOffset(8, 0, 0, 0); // Левый отступ работает

// Настройка списка работает
dropdown.itemHeight = 24;
dropdown.itemPadding = new RectOffset(8, 8, 4, 4);
```

### Источники
- [Simtropolis Community](https://community.simtropolis.com/forums/topic/757985-ui-scrollbars-dropdowns/)

## UI Elements - Создание панели

### Правильный подход
```csharp
public sealed class MainPanel : UIPanel
{
    public override void Start()
    {
        base.Start();
        
        // Размер и фон панели
        size = new Vector2(300f, 400f);
        backgroundSprite = "GenericPanel";
        color = new Color32(0, 0, 0, 160);
        
        // Кнопка закрытия
        _close = AddUIComponent<UIButton>();
        _close.text = "×";
        _close.normalBgSprite = "ButtonMenu";
        _close.eventClick += (_, __) => Hide();
        
        // Перетаскивание
        var drag = AddUIComponent<UIDragHandle>();
        drag.target = this;
        drag.size = new Vector2(width, 32f);
    }
}
```

### Ключевые принципы
- Наследование от `UIPanel` (не `UIComponent`)
- Использование `"GenericPanel"` для фона
- Правильная настройка `zOrder` для кнопки закрытия
- `UIDragHandle` для перетаскивания

## Drag & Drop - Координаты

### Проблема
Инверсия по Y и ускорение движения из-за смешивания систем координат.

### Решение
```csharp
private Vector2 ScreenToParentLocal(Vector3 mouseScreen)
{
    var par = (parent ?? (UIComponent)UIView.GetAView());
    var ui = par.GetUIView();
    Vector2 gui = ui.ScreenPointToGUI(mouseScreen); // учитывает DPI/UI Scale
    return par.ScreenToLocal(gui);                   // в локаль РОДИТЕЛЯ
}
```

### Причины проблем
- Смешивание экранных пикселей с UI-координатами
- UI Scale ≠ 100% (110% → +10% движения)
- Использование `Input.mousePosition` без преобразования

## UISlider - Правильные спрайты

### Проблема
"Прямоугольник" в верхнем правом углу из-за неправильных спрайтов.

### Решение
```csharp
// БАЗА (трек) — используем «SliderBudget», а НЕ ScrollbarTrack
var baseTrack = slider.AddUIComponent<UISlicedSprite>();
baseTrack.spriteName = "SliderBudget";
baseTrack.size = slider.size;
slider.trackObject = baseTrack;

// ПОЛЗУНОК
var thumb = slider.AddUIComponent<UISprite>();
thumb.spriteName = "ScrollbarThumb";
thumb.size = new Vector2(12f, 18f);
slider.thumbObject = thumb;
```

### Ключевые моменты
- `clipChildren = true` на слайдере
- Использование `"SliderBudget"` вместо `"ScrollbarTrack"`
- Правильный порядок создания объектов
- Проверка `zOrder` для выпадающих списков

## Events - Обработка кликов

### Правильный подход
```csharp
private bool _suppressNextClick;
private float _nextClickAt;
private const float ClickCooldown = 0.15f;

eventClick += (c, p) =>
{
    // Если был drag - не триггерим клик
    if (_suppressNextClick) { 
        _suppressNextClick = false; 
        p.Use(); 
        return; 
    }

    // Анти-даблклик/спам
    if (Time.realtimeSinceStartup < _nextClickAt) { 
        p.Use(); 
        return; 
    }
    _nextClickAt = Time.realtimeSinceStartup + ClickCooldown;

    OnButtonActivated();
    p.Use();
};
```

### Порядок событий
1. `eventMouseDown` - начало drag
2. `eventMouseMove` - отслеживание движения  
3. `eventMouseUp` - конец drag
4. `eventClick` - клик (если не было drag)
