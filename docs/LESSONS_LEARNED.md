# Уроки и важные находки (Lessons Learned)

## ColossalFramework.UI - Критические моменты

### 1. Использование UITextureSprite вместо атласов
**Проблема:** При добавлении пользовательских текстур в defaultAtlas (или другие существующие атласы) возникает искажение отображения - "каша" из пикселей других спрайтов.

**Решение:** Использовать `UITextureSprite` для пользовательских элементов:
```csharp
var sprite = AddUIComponent<UITextureSprite>();
sprite.texture = myTexture2D;
sprite.size = new Vector2(width, height);
sprite.relativePosition = Vector3.zero;
```
Это позволяет рендерить текстуру напрямую без добавления в атлас.

### 2. Загрузка Texture2D из embedded resources
- Правильный размер при создании: `new Texture2D(2, 2)` - Unity автоматически определит реальный размер
- Всегда добавлять фильтрацию: `texture.wrapMode = TextureWrapMode.Clamp; texture.filterMode = FilterMode.Bilinear;`
- Использовать `Texture2D.LoadImage(byte[])` для PNG

### 3. Программное создание круглых текстур
- Использовать евклидово расстояние для определения пикселей внутри круга
- Центр: `BUTTON_SIZE / 2f`
- Радиус: `BUTTON_SIZE / 2f - borderWidth`
- Устанавливать `wrapMode = Clamp` и `filterMode = Bilinear` для качественного рендеринга

### 4. Позиционирование элементов
- `absolutePosition` - абсолютная позиция на экране
- `relativePosition` - позиция относительно родителя
- Для центрирования: `new Vector3((parent.x - child.x) / 2f, (parent.y - child.y) / 2f)`
- `zOrder` - порядок отрисовки (больше = выше)

### 5. Debugging UI элементов
- Если элемент создаётся, но не виден - проверить `isVisible = true`, `opacity > 0`, `zOrder`
- Использовать `clipChildren = false` если нужно отображать элементы вне границ родителя
- В ModTools (F7) искать элемент в Scene Explorer по имени

### 6. Загрузка embedded resources
- В `.csproj czyn` должен быть `<EmbeddedResource Include="Resources\icon.png" />`
- Полное имя ресурса: `Namespace.Resources.iconерпut.on.txt файл должен бытьв.txt файл должен быть.png`
- Доступ: `Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)`

## Проблемы и решения

**Проблема:** Кнопка не видна или видна только при наведении
- **Причина:** Отсутствие фонового спрайта или неправильный atlas
- **Решение:** Использовать `UITextureSprite` для фона или создать собственный атлас

**Проблема:** Искажённая иконка
- **Причина:** Неправильный `region` в SpriteInfo или добавление в существующий атлас
- **Решение:** Использовать `UITextureSprite` напрямую

**Проблема:** DLL не копируется при сборке
- **Причина:** Игра использует DLL (заблокирован файл)
- **Решение:** Полностью закрыть игру перед пересборкой

**Проблема:** Элемент не отображается несмотря на корректные логи
- **Причина:** Перекрытие другими элементами или `clipChildren = true`
- **Решение:** Проверить `zOrder` и `clipChildren`, убедиться что элемент внутри `UIView`

## API Reference
- `UIView.GetAView()` - получить корневой UI контейнер
- `AddUIComponent<T>()` - добавить дочерний UI компонент
- `UITextureSprite` - рендеринг произвольных текстур без атласа
- `Color32(r, g, b, a)` - цвет в формате RGBA (0-255)
- `ResourceLoader` - НЕ СУЩЕСТВУЕТ в стандартном API

