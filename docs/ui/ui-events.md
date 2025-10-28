# UI Events Guide

## Обработка кликов
- Используйте `eventClick` для кнопок
- Глушите клик, если был drag больше порога
- Добавляйте анти-спам cooldown

## Правильная обработка кликов
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

## Порядок событий
1. `eventMouseDown` - начало drag
2. `eventMouseMove` - отслеживание движения
3. `eventMouseUp` - конец drag
4. `eventClick` - клик (если не было drag)

## Частые ошибки
- ❌ Отсутствие проверки на `DragThreshold`
- ❌ Не использование `p.Use()` для блокировки события
- ❌ Отсутствие анти-спам защиты