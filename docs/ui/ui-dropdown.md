# UIDropDown Guide

## Основные свойства
```csharp
dropdown.textScale = 0.9f;
dropdown.textColor = Color.white;
dropdown.verticalAlignment = UIVerticalAlignment.Middle;
dropdown.horizontalAlignment = UIHorizontalAlignment.Left;
dropdown.textFieldPadding = new RectOffset(8, 0, 0, 0);
dropdown.itemHeight = 26;
dropdown.itemPadding = new RectOffset(8, 8, 4, 4);
dropdown.listHeight = 180;
```

## Спрайты
```csharp
dropdown.normalBgSprite = "GenericPanel";
dropdown.hoveredBgSprite = "GenericPanelLight";
dropdown.focusedBgSprite = "GenericPanelLight";
dropdown.listBackground = "GenericPanel";
dropdown.itemHover = "GenericPanelLight";
dropdown.itemHighlight = "GenericPanelLight";
```

## Частые ошибки
- ⚠️ **Текст всегда прижат к верху - НЕ РЕШАЕТСЯ**
- ❌ Прямой доступ к внутренним компонентам через `Find<UILabel>("Label")`
- ❌ Отсутствие `textFieldPadding` для левого отступа
- ❌ Неправильные свойства: `textHorizontalAlignment`, `listColor` - не существуют

## Правильный подход
- ✅ Использование встроенных свойств UIDropDown
- ✅ Проверка на `null` перед использованием
- ✅ Отладочные логи для диагностики