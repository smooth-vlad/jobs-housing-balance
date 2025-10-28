# UISlider Guide

## Правильные спрайты
- **Фон**: `"SliderBudget"` или `"SliderBase"` - НЕ `"ScrollbarTrack"`
- **Заливка**: `"SliderFill"` - для отображения прогресса
- **Ползунок**: `"ScrollbarThumb"` - для ручки слайдера

## Настройка
```csharp
slider.clipChildren = true; // чтобы ничто не «торчало» за пределы
slider.size = new Vector2(180f, 18f);
slider.minValue = 0f;
slider.maxValue = 100f;
slider.stepSize = 1f;
slider.value = 33f;

// БАЗА (трек)
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

## Частые ошибки
- ❌ Использование `"ScrollbarTrack"` вместо `"SliderBudget"`
- ❌ Неправильный порядок создания объектов
- ❌ Отсутствие `clipChildren = true`
- ❌ Смешивание спрайтов слайдера и скроллбара