# Уроки разработки

## ColossalFramework.UI - Критические паттерны

### UITextureSprite вместо атласов
**Проблема**: Пользовательские текстуры в атласах дают "кашу" из пикселей.

**Решение**: Использовать `UITextureSprite`:
```csharp
var sprite = AddUIComponent<UITextureSprite>();
sprite.texture = myTexture2D;
sprite.size = new Vector2(width, height);
```

### Загрузка Texture2D из ресурсов
```csharp
var texture = new Texture2D(2, 2);  // Unity определит размер
texture.LoadImage(buffer);
texture.wrapMode = TextureWrapMode.Clamp;
texture.filterMode = FilterMode.Bilinear;
```

### UI панель (Task 9.4-9.5)
- **UIPanel** лучше UIComponent (встроенный фон)
- **"GenericPanel"** - надежный спрайт фона
- **UIView.fixedWidth/Height** для центрирования
- **Ленивая инициализация** экономит ресурсы
- **OnDestroy()** критичен для очистки памяти

### Паттерн кнопка-панель
```csharp
private MainPanel _panel;

private void OnButtonActivated()
{
    if (_panel == null)
        _panel = MainPanel.Create();
    _panel.Toggle();
}

public override void OnDestroy()
{
    if (_panel != null)
    {
        UnityEngine.Object.Destroy(_panel.gameObject);
        _panel = null;
    }
    base.OnDestroy();
}
```

### Обработка мыши для перетаскивания
- Порог драга: 6px
- Anti-spam cooldown: 150ms
- `suppressNextClick` после драга
- `ClampToScreen()` для ограничения перемещения