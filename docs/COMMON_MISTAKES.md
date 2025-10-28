# Common Mistakes

## UIDropDown
- ❌ `textHorizontalAlignment`, `listColor` - не существуют
- ❌ Прямой доступ к внутренним компонентам через `Find<UILabel>("Label")`
- ❌ Отсутствие `textFieldPadding` для левого отступа
- ⚠️ **Текст всегда прижат к верху - НЕ РЕШАЕТСЯ**

## UISlider
- ❌ Использование `"ScrollbarTrack"` вместо `"SliderBudget"`
- ❌ Неправильный порядок создания объектов (трек, ползунок)
- ❌ Отсутствие `clipChildren = true`
- ❌ Смешивание спрайтов слайдера и скроллбара

## UI Panel
- ❌ Наследование от `UIComponent` вместо `UIPanel`
- ❌ Отсутствие `backgroundSprite = "GenericPanel"`
- ❌ Неправильная настройка `zOrder` для кнопки закрытия

## Drag & Drop
- ❌ Смешивание экранных пикселей с UI-координатами
- ❌ Использование `Input.mousePosition` без преобразования
- ❌ Отсутствие проверки на `DragThreshold`
- ❌ Использование `absolutePosition` вместо `relativePosition`

## Events
- ❌ Отсутствие проверки на `DragThreshold`
- ❌ Не использование `p.Use()` для блокировки события
- ❌ Отсутствие анти-спам защиты

## UI Layout & Sizing
- ❌ **autoFitChildrenVertically** нестабильно работает с многострочным текстом
- ❌ Фиксированные размеры без учета контента приводят к обрезанию
- ❌ Смешивание `autoSize = true` с фиксированными размерами
- ✅ **Решение**: Использовать фиксированные размеры для стабильности (Legend: 380px)
- ✅ **Решение**: Увеличивать отступы через `autoLayoutPadding` для комфортного отображения

## .NET Framework 3.5 Compatibility
- ❌ **Enum.TryParse** не существует в .NET 3.5
- ❌ **ToArray()** extension method не работает с `IList<UIComponent>`
- ✅ **Решение**: Использовать `System.Enum.Parse` вместо `Enum.TryParse`
- ✅ **Решение**: Создавать `List<UIComponent>` вручную для итерации

## Event-driven Architecture
- ❌ Отсутствие отписки от событий в `OnDestroy()` приводит к memory leaks
- ❌ Подписка на события без проверки на `null`
- ❌ Смешивание прямых вызовов методов с событиями
- ✅ **Решение**: Всегда отписываться в `OnDestroy()` и проверять на `null`

## Singleton Pattern
- ❌ Не thread-safe реализация singleton
- ❌ Отсутствие `lock` для lazy initialization
- ❌ Публичный конструктор вместо приватного
- ✅ **Решение**: Thread-safe lazy initialization с `lock` и приватным конструктором

## Общие
- ❌ Написание кода без исследования API
- ❌ Использование несуществующих свойств
- ❌ Отсутствие проверки на `null`
- ❌ Игнорирование примеров из реальных модов
- ❌ **Файл заблокирован игрой**: Нужно закрывать Cities: Skylines перед сборкой
