# UI Textures Guide

## UITextureSprite вместо атласов
**Проблема**: Пользовательские текстуры в атласах дают "кашу" из пикселей.

**Решение**: Использовать `UITextureSprite`:
```csharp
var sprite = AddUIComponent<UITextureSprite>();
sprite.texture = myTexture2D;
sprite.size = new Vector2(width, height);
```

## Загрузка Texture2D из ресурсов
```csharp
var texture = new Texture2D(2, 2);  // Unity определит размер
texture.LoadImage(buffer);
texture.wrapMode = TextureWrapMode.Clamp;
texture.filterMode = FilterMode.Bilinear;
```

## Встроенные спрайты
- **Панели**: `"GenericPanel"`, `"GenericPanelLight"`
- **Кнопки**: `"ButtonMenu"`, `"ButtonMenuHovered"`, `"ButtonMenuPressed"`
- **Слайдеры**: `"SliderBudget"`, `"SliderFill"`, `"ScrollbarThumb"`
- **Скроллбары**: `"ScrollbarTrack"`, `"ScrollbarThumb"`

## Частые ошибки
- ❌ Использование пользовательских текстур в атласах
- ❌ Неправильные имена спрайтов для компонентов
- ❌ Отсутствие настройки `wrapMode` и `filterMode`