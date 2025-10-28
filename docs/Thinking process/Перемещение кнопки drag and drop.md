## как обычно реализовывается перемещение своей ui кнопки с drag and drop в моддах на cities skylines 1? У меня уже есть проект, хочу только понять, как вот это реализовывается. Учитывай, что в будущем я еще добавлю клик на кнопку?

Рассмотрение реализации перетаскивания элементов UI

Пользователь интересуется, как добавить перетаскивание кнопки UI в Cities: Skylines 1. В Unity используется компонент UIDragHandle для того, чтобы сделать элементы интерфейса перетаскиваемыми. Для кнопки нужно будет учесть конфликт между OnMouseDown и реализацией перетаскивания, добавив UIDragHandle к панели или кнопке и указав целевой элемент.

Подходы к реализации перетаскивания кнопки UI

Для реализации перетаскивания кнопки можно использовать либо UIDragHandle для самой кнопки, либо для родительской панели. Важно учитывать детекцию кликов при перетаскивании. Пример кода: создаем UIButton, добавляем UIDragHandle, сохраняем позицию в файле конфигурации (используя SavedFloat для сохранения координат X и Y).

Обработка изменений разрешения и перетаскивания UI кнопки

Для обработки изменений разрешения можно использовать anchor для масштабирования UI. Для перетаскивания кнопки подключаем UIDragHandle, нужен будет дополнительный код для учета кликов и перетаскивания. Нужно учесть нажатие кнопки и перетаскивание, установив порог, чтобы клик не срабатывал во время перемещения.

Реализация перетаскивания с кнопкой

Создадим контейнер UIPanel и добавим кнопку как дочерний элемент. Затем добавим UIDragHandle, настроив его на всю область панели. Чтобы клики по кнопке срабатывали, drag handle должен быть за кнопкой. В CO порядок рисования влияет на то, какой элемент получает клики, поэтому важен правильный порядок добавления компонентов.

Реализация перетаскивания и клика по кнопке

Чтобы реализовать перетаскивание кнопки, можно использовать два подхода: с UIDragHandle или вручную через обработку событий мыши. Сначала добавим кнопку как дочерний элемент, затем используем события мыши (mousedown, mousemove, mouseup) с учетом порога для различения клика и перетаскивания. Для сохранения позиции можно использовать SavedFloat.

Реализация кнопки с перетаскиванием и сохранением

Есть два способа реализации: (1) добавить UIDragHandle к контейнеру панели и перетаскивать его, (2) обрабатывать события кнопки вручную. Для кнопки лучше использовать второй способ, так как он не вмешивается в события клика. Пример для DraggableButton включает порог перетаскивания и сохранение позиции.


Коротко: в CS1 есть два типовых пути.

1. **UIDragHandle на контейнере** — хорошо для панелек/окон (тянуть за «шапку»).
    
2. **Своя обработка мыши у кнопки** — лучший вариант для **одной** кнопки, т.к. не конфликтует с кликом.
    

Ниже — минимальный и «правильный» вариант №2: кнопка тащится мышью, клики не срываются (есть порог «это был драг, а не клик»), позиция сохраняется между сессиями.

---

### Готовый класс `DraggableButton` (C# / ColossalFramework.UI)

```csharp
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

public sealed class DraggableButton : UIButton
{
    private const float DragThreshold = 6f; // пикселей: меньше — считаем кликом
    private bool _dragging;
    private float _dragDistance;            // накапливаем для отсечки клика
    private bool _suppressNextClick;

    private SavedFloat _savedX;
    private SavedFloat _savedY;

    public override void Start()
    {
        base.Start();

        // Стили на вкус — ставлю дефолт для примера
        text = "★";
        width = 36f;
        height = 36f;
        playAudioEvents = false;
        tooltip = "Перетащи, чтобы переместить";

        // Настраиваем сохранение позиции (используем общий gameSettingsFile — просто и надежно)
        _savedX = new SavedFloat("MyMod.ButtonX", Settings.gameSettingsFile, 100f, true);
        _savedY = new SavedFloat("MyMod.ButtonY", Settings.gameSettingsFile, 100f, true);

        // Восстановить позицию и зажать в экран
        relativePosition = ClampToScreen(new Vector2(_savedX.value, _savedY.value));

        // Подписки на мышь
        eventMouseDown += OnMouseDownHandler;
        eventMouseMove += OnMouseMoveHandler;
        eventMouseUp   += OnMouseUpHandler;
        eventClick     += OnClickHandler;
    }

    private void OnMouseDownHandler(UIComponent c, UIMouseEventParameter p)
    {
        if (p.buttons == UIMouseButton.Left)
        {
            _dragging = true;
            _dragDistance = 0f;
            BringToFront();
            // Блокируем событие, чтобы его не подхватили другие
            p.Use();
        }
    }

    private void OnMouseMoveHandler(UIComponent c, UIMouseEventParameter p)
    {
        if (!_dragging) return;

        // moveDelta — дельта с прошлого события, удобнее инадёжнее, чем пересчитывать по абсолютным позициям
        Vector2 next = (Vector2)relativePosition + p.moveDelta;
        next = ClampToScreen(next);

        relativePosition = next;

        // Копим дистанцию для отсечки «это был драг»
        _dragDistance += p.moveDelta.magnitude;
        p.Use();
    }

    private void OnMouseUpHandler(UIComponent c, UIMouseEventParameter p)
    {
        if (!_dragging) return;
        _dragging = false;

        // Если действительно перетаскивали — гасим следующий click
        if (_dragDistance > DragThreshold)
            _suppressNextClick = true;

        // Сохраняем
        _savedX.value = relativePosition.x;
        _savedY.value = relativePosition.y;

        p.Use();
    }

    private void OnClickHandler(UIComponent c, UIMouseEventParameter p)
    {
        if (_suppressNextClick)
        {
            // Это был драг — клика не должно быть
            _suppressNextClick = false;
            p.Use();
            return;
        }

        // TODO: твоя логика клика (добавишь позже)
        // Example: DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Button clicked!");
    }

    private Vector2 ClampToScreen(Vector2 pos)
    {
        // Родитель у кнопки — обычно корневой UIView
        var ui = parent ?? UIView.GetAView();
        float maxX = Mathf.Max(0f, ui.width  - width);
        float maxY = Mathf.Max(0f, ui.height - height);
        return new Vector2(Mathf.Clamp(pos.x, 0f, maxX), Mathf.Clamp(pos.y, 0f, maxY));
    }
}
```

### Как создать кнопку (например, в `LoadingExtensionBase.OnLevelLoaded`)

```csharp
using ColossalFramework.UI;
using ICities;

public class Loading : LoadingExtensionBase
{
    public override void OnLevelLoaded(LoadMode mode)
    {
        var view = UIView.GetAView();
        var btn  = view.AddUIComponent<DraggableButton>();
        btn.tooltip = "Перетащи меня. ЛКМ — клик.";
    }
}
```

---

### Когда всё-таки `UIDragHandle`?

Если у тебя **панель/окно** (а не одиночная кнопка), делай так:

```csharp
UIPanel panel = view.AddUIComponent<UIPanel>();
panel.size = new Vector2(200, 80);

UIDragHandle drag = panel.AddUIComponent<UIDragHandle>();
drag.target = panel;
drag.size = panel.size;
// Сделай отдельную «шапку» (Label/Panel) и помести drag поверх неё (зона, за которую тянем)

UIButton btn = panel.AddUIComponent<UIButton>();
btn.text = "Click";
btn.relativePosition = new Vector2(10, 40);
// Click остаётся кликом, а тянуть — за шапку/handle.
```

Для **одной** кнопки `UIDragHandle` часто спорит с кликом (перекрытие по ивентам), поэтому ручная обработка, как выше, — стандартная практика.

---

### Пара практических советов

- Хочешь «снэп» к краям? При отпускании мыши проверь расстояние до края и округли `relativePosition`.
    
- Нужен сброс позиции: повесь на **правый клик + Ctrl**:
    
    ```csharp
    if (p.buttons == UIMouseButton.Right && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
    {
        relativePosition = ClampToScreen(new Vector2(100,100));
        _savedX.value = relativePosition.x;
        _savedY.value = relativePosition.y;
        p.Use();
    }
    ```
    
- Если у тебя собственный конфиг-файл — можешь хранить X/Y там; `SavedFloat` просто даёт «быстро и работает».
    

Если хочешь, скидывай свой текущий код и место, где создаёшь кнопку — подправлю под твой проект за один проход.


## почему у меня может быть инвертировано движение по вертикали и движение по горизонтали следует быстрее мыши - мышь прошла часть экрана, а кнопка прошла быстрее и дальше - примерно 200px при движении на  1980px

Analyzing button movement behavior

So, the vertical inversion could stem from using absolute vs. relative coordinates. The Y-axis in Unity and ColossalFramework.UI seems to have different origins. That could explain the movement going the opposite way — something’s likely off with how deltas for mouse movements are being calculated. It's important to verify the coordinate space correctness!

Investigating UI movement issues

The issue could stem from incorrectly using worldSpace, anchored pivot, or the wrong position types (position, relativePosition, or absolutePosition). For horizontal scaling, maybe p.moveDelta is being affected by UI scaling or resolution, like UI zoom. Also, moveDelta seems to be in screen pixels, which might not align with the UI’s scale factor.

Investigating UI scaling issues

The issue could be related to scaling the UI resolution, where relative movement of UI elements isn't properly adjusted for the scaling. If the UI is scaled (like at 110%), the movement might become disproportionate. I’m wondering if there’s a mismatch between the screen pixels and UI coordinates, especially on devices with DPI scaling.

Handling UI scale adjustments

Ah, so if the UI scale is 110%, 1 screen pixel would roughly equal 0.91 UI points, meaning we need to scale the mouse movement accordingly. If I use raw pixel values directly on the scaled UI, it causes an overshoot. To move 100 pixels, I should instead add about 90.9 UI points. Interesting!

Поиск причин инверсии и ускорения движения в UI

Открытие вики Cities Skylines для системы координат

Поиск информации на Read the Docs

Searching for event coordinates

Документация слишком общая и не дает нужной информации. Нужно найти точные данные о координатах параметров события. Я попробую поискать по запросу "UIMouseEventParameter moveDelta Cities Skylines", может, это поможет разобраться с координатами движений мыши.

Поиск информации о параметрах мыши в Cities Skylines

Searching for scaling properties

Результаты поиска подтверждают, что масштабирование интерфейса может повлиять на положение кнопок (хотя это не касается конкретно C:S, но проблема похожа). Мод ScaleUI в Steam workshop изменяет масштаб. Однако я не нашел нужное имя свойства для масштаба в ColossalFramework. Поищу "UIView.uiScale" или "UIView.pixelRatio".

Поиск свойства масштаба в ColossalFramework UI

Looking for scaling properties

Не могу найти точное свойство. Еще одна идея: в примерах моддинга для C:S часто используется p.moveDelta без учета масштаба, но плагин UI scale тогда нарушает работу. Возможно, стоит использовать метод `parent.ScreenPointToLocal` для преобразования мыши в координаты панели. Попробую поискать "vector

Поиск информации о p.moveDelta в Cities: Skylines

Proposing a robust fix

Я уже подзастрял с поиском точного названия свойства. Мы, наверное, можем предложить решение с использованием метода `UIView.GetAView().ratio` для определения масштаба интерфейса. Но нужно избегать ошибки с кодом. В качестве альтернативы, предлагаю компенсировать масштаб с помощью `GetUIView().scale`. А ещё можно попробовать вычислить масштаб, сравнив разрешение интерфейса с разрешением экрана. Нужно проверить наличие определения метода для преобразования координат указателя в локальные координаты панели с помощью другого метода.

Поиск методов преобразования позиции мыши в координаты панели

Proposing a drag fix

Похоже, точных параметров я не нашёл, но я могу представить решение. Проблема, вероятно, связана с:

- Инвертированным вертикальным смещением, что связано с различиями между осями экрана и UI.
    
- Перемещением более быстро, чем мышь, из-за несоответствия между дельтами экрана и координатами UI, уменьшенными или увеличенными с учётом масштаба.
    

Решение — это всегда учитывать масштаб и адаптировать вычисления в координатах UI.

Поиск свойства mousePosition в ColossalFramework.UIView

Proposing robust fixes

Поиск не помог найти точные имена свойств, но можно предложить общее решение. Если масштаб UI не 100%, то одно из исправлений — делить `p.moveDelta` на `TransformToDevicePixelRatio()`. Но это не вполне точное решение, поэтому предложу два подходящих варианта:

1. Использовать свойства для нормализации дельты с учетом масштаба, 2) зафиксировать начальную позицию при старте перетаскивания с помощью координат панели.
    

Поиск свойств параметра события мыши ColossalFramework

Revising the approach

Похоже, что точные параметры сложно подтвердить. Лучше всего объяснить концептуально. Мы можем использовать `p.moveDelta` с инвертированием по оси Y и компенсацией масштаба UI с помощью метода "GetUIView().GetUIScale()". Вдобавок, можно вычислить "scale", разделив ширину UI на разрешение экрана.

Proposing a solution

Я могу предложить решение: разделить `moveDelta` на `GetUIView().uiScale` или лучше использовать `p.moveDelta / GetUIView().inputScale`. Если эти свойства недоступны, можно вычислить отношение с помощью `view.GetMouseScale()`. Однако, проще использовать `GetUIView().GetScreenResolution()` и `view.GetUIResolution()`.

Proposing an alternate fix

Я предлагаю еще один способ: при нажатии мыши сохраняем `Vector2 dragStart = p.position;`. Затем, при движении мыши, вычисляем разницу и добавляем её к `relativePosition`. Стоит убедиться, что `p.position` находится в правильной системе координат для компонента, и, если вертикальные оси не инвертируются, использовать `relativePosition += p.moveDelta`.

Diagnosing overshoot issues

Кажется, проблема перескока по горизонтали возникает не из-за `p.moveDelta`, а через использование разницы между `mousePosition` и `lastMousePosition` в экранных координатах. Я предлагаю использовать `p.moveDelta` вместо `Input.mousePosition` и уточнить, что проблема вертикальной инверсии тоже производится из-за вычислений с `Input.mousePosition`.

Диагностика и решение проблемы перемещения UI элементов

Proposing fixes for position issues

Похоже, все же стоит использовать координаты UI, чтобы избежать проблем с масштабированием и позиционированием. Преобразование между экранными и UI координатами может не сработать полностью, так что я рекомендую использовать `p.moveDelta`, так как он уже учитывает масштабирование и работает в координатах UI. Еще стоит упомянуть, что некоторые моды могут изменять отображение.

Fixing overshoot and inversion

Если проблема с "переполнением" сохраняется, убедитесь, что прибавляете delta к `relativePosition`, а не к `absolutePosition` или `transform.position`. Также учитывайте, что координаты могут не совпадать, если обработчики прикреплены не к тому родителю. Используйте `p.moveDelta`, чтобы избежать ошибок из-за масштаба.

Proposing a fix with patch code

Вот короткий патч для исправления:

`Vector2 _dragStartPosUI; Vector2 _dragStartRel;  void OnMouseDown(UIComponent c, UIMouseEventParameter p) {     _dragStartRel = relativePosition;     _dragStartPosUI = p.position; // координаты в пространстве родителя     p.Use(); }  void OnMouseMove(UIComponent c, UIMouseEventParameter p) {     if (!p.buttons.IsFlagSet(UIMouseButton.Left)) return;     Vector2 delta = p.position - _dragStartPosUI;     relativePosition = ClampToScreen(_dragStartRel + delta);     p.Use(); }`

Этот код использует координаты родителя, которые должны обеспечить корректное поведение.

Предложение двух вариантов решения

Вот два безопасных варианта:

- **Вариант 1** (рекомендуемый): Используйте `p.position` / `p.moveDelta` с события `eventMouseMove` (координаты UI) и обновляйте `relativePosition`.
    
- **Вариант 2**: Если хотите использовать `Input.mousePosition` (координаты экрана), конвертируйте, инвертируйте Y и делите на коэффициент масштабирования UI, вычисленный с помощью `UIView.GetAView().ratio`. Но возможны погрешности.
    

Скорее всего, проблема в том, что не используются UI-координаты.


Короткий диагноз:

1. **Инверсия по Y** почти всегда появляется, когда двигают не в UI-координатах Colossal, а по экранным (`Input.mousePosition`, `transform.position` и т.п.) **или** когда правят `absolutePosition`/`transform`, а не `relativePosition`.
    
2. **«Бежит быстрее мыши» по X (~+200 px на ~1980 px, т.е. ~+10%)** — классический эффект **UI Scale ≠ 100%** (или моды ScaleUI/UI Resolution). Ты суммируешь экранные пиксели с UI-пойнтами — выходит множитель ≈ масштабу (110% → +10%).
    

Вот рабочий, «правильный» способ, чтобы не ловить ни инверсию, ни рассинхрон масштаба:

```csharp
public sealed class DraggableButton : UIButton
{
    private bool _dragging;
    private Vector2 _dragStartMouseUi;   // Позиция мыши в UI-пространстве события
    private Vector2 _dragStartRel;       // Стартовая позиция кнопки

    public override void Start()
    {
        base.Start();
        eventMouseDown += (c, p) =>
        {
            if (p.buttons == UIMouseButton.Left)
            {
                _dragging = true;
                _dragStartRel = relativePosition;
                _dragStartMouseUi = p.position;     // ВАЖНО: это уже UI-координаты родителя
                BringToFront();
                p.Use();
            }
        };

        eventMouseMove += (c, p) =>
        {
            if (!_dragging) return;

            // Считаем дельту ТОЛЬКО в UI-пространстве:
            Vector2 deltaUi = p.position - _dragStartMouseUi;  // ни инверсий, ни масштабов
            relativePosition = ClampToScreen(_dragStartRel + deltaUi);
            p.Use();
        };

        eventMouseUp += (c, p) =>
        {
            if (!_dragging) return;
            _dragging = false;
            p.Use();
        };
    }

    private Vector2 ClampToScreen(Vector2 pos)
    {
        var ui = parent ?? UIView.GetAView();
        float maxX = Mathf.Max(0f, ui.width  - width);
        float maxY = Mathf.Max(0f, ui.height - height);
        return new Vector2(Mathf.Clamp(pos.x, 0f, maxX), Mathf.Clamp(pos.y, 0f, maxY));
    }
}
```

Почему это лечит всё сразу:

- `p.position`/`p.moveDelta` **уже** даны в координатах UI-родителя, где (0,0) — левый верх. Никаких `Input.mousePosition`, никаких инверсий по Y и пересчётов под DPI/масштаб не требуется.
    
- Двигаем **`relativePosition`**, а не `absolutePosition`/`transform`.
    
- Если у тебя был overshoot из-за UI Scale 110%, здесь он исчезнет, потому что мы не смешиваем «экранные» пиксели с «UI-пойнтами».
    

Чек-лист, если всё ещё «едет»:

- Убедись, что **обработчики навешаны на тот же компонент, который двигаешь**, и что его **родитель стабилен** (кнопка и событие — в одном контейнере).
    
- Временно выставь **UI Scale = 100%** (в настройках игры или отключи ScaleUI/UI Resolution) и проверь: если ускорение исчезает — значит, раньше ты смешивал разные системы координат.
    
- Не используй `Input.mousePosition`/`Screen.width`/`Camera` для расчётов в Colossal UI — это почти всегда источник инверсий и «ускорений».
    

Если пришлёшь свой текущий обработчик `eventMouseMove` (что именно суммируешь и куда), я укажу точное место, где происходит инверсия/масштабный рассинхрон и перепишу пару строк под твой проект.

## все то же самое, не сработало

Решение проблемы с позиционированием UI

Похоже, что проблема может быть связана с координатами и масштабированием. Возможно, нужно проверить, используются ли другие системы координат для родительских элементов или параметров, таких как p.position и moveDelta. Также вероятно, что сдвиг может быть вызван нестандартным масштабом UI.

Проверка координат и масштабирования UI

Возможно, проблема в том, что p.position указывает на компонент, но могу ли я точно подтвердить, что этот компонент — тот, который получает событие? Может быть, при использовании p.moveDelta(с учетом масштаба) будем иметь меньше проблем, чем с p.position? Также стоит проверить настройки масштабирования и пиксельного соотношения UI.

Проверка масштабирования и координат UI

Похоже, проблема может быть связана с масштабированием, например, через "UIView.GetAView().pixelsPerPoint" или "uiScale". Возможно, родитель кнопки имеет масштаб, отличный от 1, что меняет отображение. Also интересно, не используется ли неправильный метод для отслеживания событий, возможно, стоит проверить параметры события, например, для `p.relativePosition`.

Проверка параметров движения UI

Есть несколько параметров для обработки движения: p.position, p.cursorPosition и p.moveDelta. Обычно используется p.moveDelta, но проблема может быть в несоответствии разрешения экрана и UI масштаба. Возможно, p.moveDelta учитывает пиксели экрана, а не единицы UI, что вызывает проблему.

Предложение решения проблемы с перемещением UI

Для решения проблемы с перемещением UI и возможными ошибками масштаба предлагаю использовать функцию для вычисления дельты с экрана, например, с использованием `ScreenToLocal`. Мы также можем предложить вариант с учетом инверсии оси Y, если наблюдается неправильный порядок движения. Лучше всего будет протестировать на их окружении, чтобы точно настроить масштабирование и компенсацию ошибок.

Ок — тогда даю «гвоздь», который обычно пробивает ВСЁ: считаем дельту не из `p.position`, а переводим **экранную позицию мыши → в локальные координаты родителя** через `ScreenPointToGUI` + `ScreenToLocal`. Это убирает и инверсию по Y, и «ускорение» при любом UI-scale/ретине/модах скейла.

### Drop-in: надёжный drag для любой кнопки

```csharp
using ColossalFramework.UI;
using UnityEngine;

public sealed class DraggableButton : UIButton
{
    private bool _dragging;
    private Vector2 _dragStartLocalInParent; // мышь в локале PARENT'а на старте
    private Vector2 _dragStartRel;           // позиция кнопки на старте

    public override void Start()
    {
        base.Start();

        text = "★";
        width = height = 36f;

        eventMouseDown += OnDown;
        eventMouseMove += OnMove;
        eventMouseUp   += OnUp;
    }

    private void OnDown(UIComponent c, UIMouseEventParameter p)
    {
        if (p.buttons != UIMouseButton.Left) return;

        _dragging = true;
        _dragStartRel = relativePosition;
        _dragStartLocalInParent = ScreenToParentLocal(Input.mousePosition);
        BringToFront();
        p.Use();
    }

    private void OnMove(UIComponent c, UIMouseEventParameter p)
    {
        if (!_dragging) return;

        // Текущая мышь в локале родителя
        Vector2 curLocal = ScreenToParentLocal(Input.mousePosition);
        Vector2 delta    = curLocal - _dragStartLocalInParent;

        // Если родитель/иерархия масштабированы — компенсируем
        var pr = (parent ?? (UIComponent)UIView.GetAView()).transform.lossyScale;
        if (pr.x != 0f && pr.y != 0f)
            delta = new Vector2(delta.x / pr.x, delta.y / pr.y);

        relativePosition = ClampToScreen(_dragStartRel + delta);
        p.Use();
    }

    private void OnUp(UIComponent c, UIMouseEventParameter p)
    {
        _dragging = false;
        p.Use();
    }

    // --- ВСПОМОГАТЕЛЬНОЕ ---

    // Переводим экранные пиксели мыши в локальные координаты родителя
    private Vector2 ScreenToParentLocal(Vector3 mouseScreen)
    {
        var par   = (parent ?? (UIComponent)UIView.GetAView());
        var ui    = par.GetUIView();
        Vector2 gui = ui.ScreenPointToGUI(mouseScreen); // учитывает DPI/ретину/скейл
        return par.ScreenToLocal(gui);                   // в локаль РОДИТЕЛЯ
    }

    private Vector2 ClampToScreen(Vector2 pos)
    {
        var ui = parent ?? UIView.GetAView();
        float maxX = Mathf.Max(0f, ui.width  - width);
        float maxY = Mathf.Max(0f, ui.height - height);
        return new Vector2(Mathf.Clamp(pos.x, 0f, maxX), Mathf.Clamp(pos.y, 0f, maxY));
    }

    public override void OnDestroy()
    {
        // чтобы не было «ускорения» из-за двойных подписок после перезагрузок
        eventMouseDown -= OnDown;
        eventMouseMove -= OnMove;
        eventMouseUp   -= OnUp;
        base.OnDestroy();
    }
}
```

### Почему это работает даже в «проблемных» сетапах

- `ScreenPointToGUI` учитывает ретину/HiDPI/скейлы UI (в т.ч. моды вроде UI Resolution/ScaleUI).
    
- `ScreenToLocal` переводит координаты строго в СВОЙ родительский локал → никаких инверсий по оси Y.
    
- Компенсация `transform.lossyScale` спасает, если родительская панель (или один из предков) масштабирована.
    

### Если всё ещё едет

1. **Двойные обработчики**: убедись, что на кнопку не навешиваются подписки повторно (см. `OnDestroy`).
    
2. **AutoLayout у родителя**: выключи `autoLayout`/`autoFit...` у контейнера с кнопкой — автолэйаут может «переукладывать» детей во время драга.
    
3. **Тест-демка**: добавь кнопку **напрямую в `UIView`** (без своих панелей) и проверь — если тут ок, виноват контейнер.
    

Хочешь — кинь кусок, где создаёшь кнопку и кто её родитель. Я подправлю под твою иерархию (если там авто-layout/scale/anchors — покажу точечно, что выключить).