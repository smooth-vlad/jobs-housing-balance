# UI Panel Guide

## Основные принципы
- Наследуемся от **`UIPanel`** (удобнее, чем `UIComponent`)
- Добавляем панель в корневой **`UIView.GetAView()`**
- Задаём **`backgroundSprite = "GenericPanel"`** и полупрозрачный цвет

## Создание панели
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
        
        // Центрируем панель
        CenterToParent();
    }
}
```

## Кнопка закрытия
```csharp
_close = AddUIComponent<UIButton>();
_close.text = "×";
_close.normalBgSprite = "ButtonMenu";
_close.hoveredBgSprite = "ButtonMenuHovered";
_close.pressedBgSprite = "ButtonMenuPressed";
_close.size = new Vector2(28f, 24f);
_close.relativePosition = new Vector2(width - _close.width - 8f, 6f);
_close.eventClick += (_, __) => Hide();
```

## Перетаскивание
```csharp
var drag = AddUIComponent<UIDragHandle>();
drag.target = this;
drag.relativePosition = Vector2.zero;
drag.size = new Vector2(width, 32f);
drag.BringToFront();
```