# Modding API Guide

## Основы моддинга

### Структура мода
- **IMod** - основной интерфейс мода
- **LoadingExtension** - расширение для загрузки
- **Harmony** - патчинг кода игры

### Жизненный цикл мода
```csharp
public class MyMod : IMod
{
    public void OnEnabled() { }
    public void OnDisabled() { }
}
```

### UI Framework
- **ColossalFramework.UI** - основной UI фреймворк
- **UIPanel** - базовый компонент панели
- **UIButton** - кнопки
- **UIDropDown** - выпадающие списки
- **UISlider** - слайдеры

### События
- **eventClick** - клик по элементу
- **eventMouseDown/Up** - события мыши
- **eventValueChanged** - изменение значения

## Лучшие практики
- Всегда проверять на `null`
- Использовать правильные спрайты
- Тестировать в игре после изменений
- Документировать найденные решения
