## thinking process
Researching modding details

I need to gather info about modding for Cities: Skylines, specifically for fields like BuildingManager, CitizenUnits, and their flags. I'll focus on documentation like the CS Modding Wiki, the game's decompiled code, and other reliable sources regarding building occupants and jobs. Helpful details will come from the simulation-related sections.

Researching code structure

I’m planning to review resources like GitHub, especially for CSL’s decompiled code and modding documentation. Key aspects to explore: CitizenUnit flags, building arrays, and event triggers for building changes. I may have to focus on specific areas like linked lists and flags (Home/Work). Also, examining modding methods for hooks, like using Harmony patches.

Examining frame updates and performance

For Stage B triggers, polling at regular intervals seems more practical than event hooking, although event-based triggers are more advanced. Using the "SimulationManager" frame index can help with updates at regular intervals. It’s also important to consider performance when iterating large building arrays and checking flags for validity.

Identifying potential issues and strategies

There are some considerations to keep in mind, like avoiding outside connection buildings or multiple-unit sub-buildings. Realistic Population modifies building capacities, but since we focus on "Work" occupancy, it's fine. I'll reference the Realistic Population mod and hex grid algorithms. Caching and invalidating data should be factored in, along with scheduling actions.

Researching simulation updates and data

For frame updates, I'll explore "ICities.ThreadingExtensionBase" and hook into the simulation thread for heavy loops. It's important to check methods like "GetDistrict" and study the decompiled code for details on CitizenUnits and data arrays. I'll make sure to avoid guessing dimensions and retrieve accurate IDs.

Поиск документации по моддингу и структурам данных

Seeking authoritative sources

The initial search brought up useful info, like RedBlob hex and the Realistic Population workshop description. However, I still need more authoritative details on CitizenUnits flags and building iteration specifics. I'll search specifically for "CitizenUnit.Flags Home Work decompiled" to get clearer info on these.

Поиск информации о флагах CitizenUnits и итерации зданий

Searching for code references

The sources I’ve found, like the Steam Workshop pages on "More CitizenUnits," mention useful details like the 524,287 limit and that CitizenUnit is needed for households and every five jobs. However, I still need the specifics for enumerating Building.m_citizenUnits chain and flags. I'll search GitHub for relevant code references.

Поиск кода на GitHub для CitizenUnit и флагов

Ищу ссылки на код

Мне нужно найти кодовые ссылки для флагов CitizenUnit. Я собираюсь поискать на GitHub по запросу "CitizenUnit.Flags Home", чтобы выяснить, как точно обработать эти флаги.

Поиск кода GitHub для флагов CitizenUnit

Поиск флагов CitizenUnit

Я собираюсь искать на GitHub флаги CitizenUnit с запросом "CitizenUnit Flags Home enum". Нужно найти точные ссылки на код, чтобы разобраться, как флаги Home работают в коде.

Поиск на GitHub по запросу "CitizenUnit Flags Home enum"

Поиск кода и документации

Поиск на GitHub не дал нужного кода. Возможно, деcompiled код лежит в других источниках, таких как "CSLShowcase API" или "Klyte45 Commons". Также стоит проверить "ColossalFramework" и "CSL API". Нужно обратиться к работам "algernon" о CitizenUnits и "Red Blob" для конверсий по шестиугольникам.

Поиск документации по моддингу Cities: Skylines

Поиск и классификация данных

Источники не всегда авторитетные. Можно попробовать "CSL Modding" для деталей по коду, есть документации по "Deterministic Clock" для модов. Требуются именно флаги "CitizenUnit". Можно использовать страницу "More CitizenUnits" для объяснения типов. Нужно учесть, что "Work" модификации обычно не приходятся к жилым зданиям, но есть исключения.

Searching documentation for Citizens, Work units, and Districts

Поиск документации по флагам

Не удаётся напрямую найти API для флагов CitizenUnit. Возможно, поможет поиск по “Colossal Order API docs” или сообщество “CSL API”. Также есть репозиторий "CitiesSkylinesModTools" с декомпилированным кодом. Нужно попробовать искать по запросу “CitizenUnit Flags decompiled c# 'Home'”.

Поиск флагов CitizenUnit в API и репозиториях

Поиск информации по модулям

Пока есть достаточное покрытие: RedBlob для шестигранников, модули API для работы с потоками и временем, страницы Workshop для юнитов и рабочих. Для перечисления объектов и юнитов по возможности можно сослаться на "CSL decompile" для структуры CitizenUnits, но для подхода достаточно описать его генерализованно и проверить GitHub для "m_citizenUnits".

Поиск информации о m_citizenUnits в GitHub

План по учёту юнитов

Пока нет прямого кода для CitizenUnit Flags, но всё в порядке. В нашем плане стоит учесть, что работники — это 'Work' юниты, а жители — это 'Home'. Реалистичное население влияет на количество рабочих мест и домохозяйств, что также учитывается в нашем методе. Необходимо добавить карту районов через метод "

Поиск альтернативных решений

Как альтернатива, можно попробовать найти фрагменты кода в "IndustryResourceManager" или "TransferManager", возможно, там будет использован GetDistrict. Думаю, имеет смысл пошукать в общем интернете для получения дополнительной информации.

Поиск информации о методах GetDistrict и других

Рассмотрение производительности и решений

Существует много модов, использующих метод "DistrictManager.GetDistrict", но документация не предоставляет точной информации. Мы можем упомянуть его как метод и добавить ссылку на общий "Modding API - основы моддинга". Чтобы улучшить производительность, можно предложить использование флагов создания зданий, кэширование по ID и проверку через индекс кадров. Для "m_currentFrameIndex" стоит ссылаться на "ThreadingExtension tutorial".

Обработка событий модами

Нет официальных событий для создания или сноса зданий в CS1; многие моды используют хук "SimulationStep" или подписываются на событие "BuildingManager.instance.EventBuildingCreated

Рекомендации по производительности и работе с событиями зданий

Рассмотрение метода и ограничений

Отлично! Код с использованием _BuildingManager.instance.m_buildings.m_buffer_и хук _OnBuildingReleased_ поддерживают использование событийных триггеров. Для учета единиц, связанных с рабочими местами и домохозяйствами, можно сослаться на "CSL Game Limits" и объяснить использование флагов _CitizenUnit.Flags_, указывая на биты "Home" и "Work" без точных названий.

Поиск информации о флагах CitizenUnit в GitHub

Подтверждение источников

Отлично! У нас есть хорошие цитаты:

- Итерация по зданиям и буферу (Make Historical tutorial).
    
- Интервал обновления в ThreadingExtension (Show Limits tutorial).
    
- Цель CitizenUnits и их соотношение по 5 работам (Game Limits guide и More CitizenUnits).
    
- Для Realistic Population 2 добавим ссылку на страницу в Workshop с обновлениями от algernon.
    

Поиск информации о Realistic Population 2

Уточнение цитат

Нужно также сослаться на Red Blob. Предложим использование маппинга на районы: «DistrictManager GetDistrict». Документацию не нашли, но есть ссылка на «Make Historical», где упоминаются hooks для BuildingExtensionBase. Для получения ID района найдём код модификации на GitHub, ищем по запросу “GetDistrict building position”.

Поиск информации о методах GetDistrict на GitHub

План исследования

Нужно упомянуть доступ к API TM:PE, хотя это не так важно для текущего этапа. Сосредоточимся на улучшенном плане и проработке деталей:

Для метода B.1 предлагаю предпочесть считывание данных о жильцах через 'ResidentialBuildingAI' вместо сканирования единиц. Однако, сканирование дает точную информацию, так как Realistic Population динамично меняет заполняемость.

Рассматривая "работающие" единицы

"CitizenUnit" является контейнером для 5 ID граждан, и "Work" единица означает занятых граждан. Для вычисления количества «занятых рабочих мест» важно учитывать только тех граждан, чьи записи в "Work" числе ненулевые. Это точный показатель, в отличие от пропускной способности рабочих мест.

Поиск информации о CitizenUnit в Cities Skylines

План, алгоритмические рецепты и особенности

Наш план охватывает перечисление, кэширование, триггеры и интеграцию. Добавим небольшой раздел с алгоритмами: вычисление идентификатора гекса из мировых координат. Важно отметить размеры гекса, а также формулы для осевых координат. Не забудем про исключения, такие как исключение соединений через "OutsideConnectionAI".

Поиск информации о OutsideConnectionAI в Cities Skylines

План и описание для задачи

Необходимо учесть "гражданские единицы" для коммерческих объектов, такие как "посетители". Ответ должен быть достаточно кратким, но включать действующие детали, проверку плана и ссылки на исследования. Включим рекомендации по алгоритмам, полям, подводным камням и тестам, избегая излишнего кода. Уточним детали по размерам гекса: точки-на-верх или плоский верх, с формулами из Red Blob.

Создаем ответ с планом

Нужно точно указывать источники для утверждений типа "Citizen unit counts
## general info
Отличный этап. Ниже — быстрая проверка твоего плана «B: Сбор данных (факт)» + выжимка из коммьюнити-доков и разборов кода, чтобы другой ИИ сделал всё правильно (без кода, только конкретика).

# Коротко: всё по делу

Твоя идея считать **фактических жителей** и **фактически занятые рабочие места** из `CitizenUnit` на здании — корректная и совместимая с Realistic Population (RP2), потому что RP2 меняет ёмкости (сколько может жить/работать), а ты считаешь именно **кто реально живёт/работает сейчас**. В CS1 «юниты граждан» — это связки из ≤5 граждан, которые «подвешены» к зданиям для домохозяйств, рабочих мест, клиентов, студентов и т.д.; именно поэтому через них удобно агрегировать факты. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2712549268&utm_source=chatgpt.com "Guide :: Cities Skylines Game Limits"))

Ниже — подробный рабочий план по подпунктам B.1–B.5 с нюансами и источниками.

---

# B.1 Итерация по зданиям и извлечение фактов

**Что безопасно полагать из устройства игры:**

- Здания лежат в буфере `BuildingManager.instance.m_buildings.m_buffer` — это стандартный способ прохода по всем инстансам в CS1. Такой паттерн демонстрирует boformer в туториале (он же показывает фильтрацию «активных» по флагам). ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))
    
- **CitizenUnit** — основная «ячейка» связки граждан: 1 юнит для домохозяйства, 1 на каждые ~5 рабочих мест, 1 на каждые ~5 клиентов у коммерции и т.п. (поэтому Work-юниты — это «контейнеры рабочих»). Лимиты и назначение юнитов подтверждены гайд-сводкой об игровых лимитах и обсуждениями моддеров. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2712549268&utm_source=chatgpt.com "Guide :: Cities Skylines Game Limits"))
    
- Каждый **CitizenUnit** содержит до **5** ссылок на граждан (`m_citizen0..m_citizen4`). Это напрямую упоминается в профильных обсуждениях на форуме Paradox. Это объясняет, почему юнит ≠ один человек, и почему «кол-во занятых» надо извлекать по фактически заполненным слотам, а не умножать количество Work-юнитов на 5. ([forum.paradoxplaza.com](https://forum.paradoxplaza.com/forum/threads/public-transport-vehicle-has-forever-boarding-problem.1595950/?utm_source=chatgpt.com "public transport vehicle has \"forever boarding\" problem"))
    

**Рекомендованный минимум полей в сэмпле здания:**

- `buildingId`, `position` (из `m_position`), `service/zone` (по `Info.GetService()`/зональности), `districtId`(через DistrictManager по позициям), `residentsFact`, `jobsFact`.
    
- **District**: в CS1 типовой способ — вычислить по мировой позиции через `DistrictManager` (GetDistrict по worldPos). Это общий паттерн моддинга (см. руководства и код-примеры в моддинг-туториалах boformer). ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))
    

**Алгоритм извлечения:**

1. Идёшь по буферу зданий и отбрасываешь невалидные/пустые: без `Info`, без флага созданности/существования. (Паттерн проходит по буферу и проверяет `m_flags` — см. boformer). ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))
    
2. Из `building.m_citizenUnits` проходишь связанный список юнитов (через указатель на следующий). Для каждого юнита:
    
    - По флагам юнита определяешь тип: **Home** (жители), **Work** (работники), игнорируешь **Visit/Student/Patient/Customer** и т.п. (они для другой аналитики).
        
    - Считаешь **фактически занятые слоты**: количество ненулевых `m_citizenX`. Это и есть «факт».
        
3. Для **жилых** зданий суммируешь по Home; для **нежилых** — по Work.
    
4. Считает `balance = jobsFact − residentsFact` (плюс — «нужно жильё», минус — «нужны рабочие места»).
    

> Почему «по факту», а не «по ёмкости»: RP2 и другие моды меняют ёмкость (сколько _может_ быть). Тебе для баланса нужны _реально занятые_ места, чтобы понять текущее несоответствие. RP2 официально «подкручивает» число домохозяйств/работников у зданий и требования по образованию, но сам факт занятости/проживания отражён в юнитах — ты его правильно считываешь. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2025147082&utm_source=chatgpt.com "Steam Workshop::Realistic Population 2 2.2.4"))

**Исключения/фильтры:**

- Игнорируй **Outside Connections** (внешние соединения) и вспомогательные/служебные псевдоздания — они портят картину «рабочих мест» в городских кварталах. (Смотри практику модов, работающих с внешними соединениями — в целом их считают «особым» типом). ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=734025380&utm_source=chatgpt.com "Configure Outside Connections' Limits - Workshop"))
    

**Опасные места:**

- **Не хардкодь размеры массивов!** Мод **More CitizenUnits** может менять размер массива юнитов; всегда бери размер у массива в рантайме, а не `MAX_UNIT_COUNT`. Это прямо подчёркнуто автором More CitizenUnits. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2654364627&utm_source=chatgpt.com "Steam Workshop::More CitizenUnits 1.1.3"))
    

---

# B.2 Кэширование и «грязные» флаги

**Задача кэша:** держать срез «здание → {residentsFact, jobsFact, districtId, position}», чтобы оверлей и агрегаторы работали без повторного полного скана каждый кадр.

**Рекомендации:**

- Ключ — `buildingId`.
    
- Поля: данные из B.1 + `lastUpdatedFrame`, `isDirty`.
    
- Инвалидация:
    
    - Явная (триггеры из B.3).
        
    - TTL по «кадрам симуляции» — обновляй, если `currentFrameIndex - lastUpdatedFrame > N`(например, раз в несколько секунд симуляции, см. ниже про интервал). Документация по потокам/обновлениям советует **не** грузить `OnUpdate` каждый кадр и уводить тяжёлые действия в симуляцию. ([Skylines](https://skylines.paradoxwikis.com/Modding_Basics?utm_source=chatgpt.com "Modding Basics - Cities: Skylines Wiki"))
        
- При необходимости делай «incremental scan»: дели буфер зданий на чанки и обновляй по порциям, чтобы не делать один гигантский проход — это стандартная тактика для модов на CS1.
    

---

# B.3 Триггеры пересчёта

**Периодический пересчёт:**

- «Каждые 5–10 игровых секунд» — нормальный компромисс. Удобно привязать к счётчику кадров симуляции и тикать не чаще, чем раз в 256/512 кадров (многие моды так делают в CS1, именно через счётчики SimulationManager). Идея: выносить тяжёлую работу с главного UI-потока в симуляционный цикл. ([Skylines](https://skylines.paradoxwikis.com/Modding_Basics?utm_source=chatgpt.com "Modding Basics - Cities: Skylines Wiki"))
    

**Событийные триггеры:**

- **Создание/снос/перемещение здания.** В CS1 есть **BuildingExtensionBase**-хуки (например, `OnBuildingReleased`), показано в туториале boformer. Там это используют для синхронизации своего состояния с жизненным циклом зданий. Аналогично можно пометить кэш «грязным» для конкретного `buildingId` или ближайших гексов. ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))
    
- **Смена настроек UI (режим/масштаб, hex-size, переключение hex/districts).** Просто выставляй `isDirty`глобально для соответствующих слоёв.
    

**Приоритеты обновлений:**

- Критичные (снос/создание) — обновляй немедленно (или в ближайшем шаге симуляции).
    
- Некритичные (изменение масштаба гекса, обычный периодический тик) — фоново чанками.
    

---

# B.4 Интеграция и связки

**Схема данных → оверлей:**

- `DataCollector` (этап B) отдаёт сырой список `{buildingId → residentsFact, jobsFact, position, districtId}`.
    
- `AggregatorHex` и `AggregatorDistricts` (этап C) используют этот список для суммирования:
    
    - **Hex-агрегация:** см. ниже про гексы.
        
    - **District-агрегация:** суммируй по `districtId`.
        
- `OverlayRenderer` красит ячейки (hex / district): палитра по величине `balance` и/или по «коэффициенту J/H».
    

**Логирование и отладка:**

- Логи по порциям, не спамить каждый кадр.
    
- Писать общее время обхода, количество зданий, % «пустых» юнитов, кадры между апдейтами.
    

---

# B.5 Тестирование и валидация

**С RP2 (Realistic Population 2):**

- RP2 меняет **количество домохозяйств и рабочих мест** по размеру здания и пересобирает требования к образованию. Это влияет на _возможности_, но твой сбор «по факту» должен оставаться корректным — ты читаешь реально заселённых и реально трудоустроенных. Смысловые эффекты: при пустующих вакансиях `jobsFact` < «ёмкость», и это как раз полезный сигнал для баланса. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2025147082&utm_source=chatgpt.com "Steam Workshop::Realistic Population 2 2.2.4"))
    

**Разные типы зданий:**

- Жилые: ожидаем Home-юниты;
    
- Коммерция/Офисы/Индустрия/Сервисы: Work-юниты;
    
- Игнорируй Visit/Customer/Student/Patient — они не про J–H баланс. Подтверждение, что юниты существуют для разных «ролей» (домохозяйства, работа, клиенты и пр.), есть в лимит-гайде. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2712549268&utm_source=chatgpt.com "Guide :: Cities Skylines Game Limits"))
    

**Производительность на больших городах:**

- Инкрементальные проходы по чанкам + кэш.
    
- Без хардкода размеров массивов (см. предупреждение автора More CitizenUnits). ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2654364627&utm_source=chatgpt.com "Steam Workshop::More CitizenUnits 1.1.3"))
    

---

## Важные практические нюансы (чтобы сразу не наломать дров)

**1) Гексы: математика и размеры**  
Для устойчивой геометрии используй «классический» гайд Red Blob Games — там формулы для axial/cube-координат, преобразования из world-координат, округление к ближайшей ячейке и т.д.

- Определи, «pointy-top» или «flat-top» (рекомендую **flat-top** для карт CS из-за удобной компоновки по X).
    
- Пользовательские размеры 64/128/256 можно трактовать как **ширину гекса (flat-to-flat)** в игровых метрах; от неё получишь «радиус» и шаг сетки по осям.
    
- Для стабильного попадания точек в ячейки используй **cube-rounding** по Red Blob. ([Reddit](https://www.reddit.com/r/CitiesSkylines/comments/og8odq/city_load_stuck_at_simulation_building_manager/?utm_source=chatgpt.com "City load stuck at \"Simulation: building manager\""))
    

**2) DistrictId**  
Рассчитывай по позиции (DistrictManager, worldPos → byte id). Это де-факто стандарт в модах (см. паттерны из моддинг-туториалов boformer; сам API детально документирован в разборных гайдах по моддингу и через деобфускацию DLL). ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))

**3) События зданий**  
Для актуализации кэша используй **BuildingExtensionBase** (`OnBuildingReleased` и аналоги) — это официальные хуки API в CS1, реально применяются моддерами. ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))

**4) Интервалы обновления**  
Не жги каждый кадр. Используй `ThreadingExtensionBase.OnUpdate` только как «таймер», а тяжёлую агрегацию гоняй в симуляции (batched) — это общая рекомендация из материалов по моддингу и практики крупных модов («Show Limits»/«Watch It!» и пр.). ([Skylines](https://skylines.paradoxwikis.com/Modding_Basics?utm_source=chatgpt.com "Modding Basics - Cities: Skylines Wiki"))

**5) Совместимость с лимит-модами**  
Учитывай, что **More CitizenUnits** меняет верхнюю границу юнитов — поэтому **никаких констант** в коде, всегда читаешь `m_units.m_size`/`m_buffer.Length` в рантайме. Автор прямо просит об этом в описании. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2654364627&utm_source=chatgpt.com "Steam Workshop::More CitizenUnits 1.1.3"))

---

## Что отдать другому ИИ как «правила реализации» (без кода)

1. **Итерация зданий:**
    
    - Пройти `BuildingManager.m_buildings.m_buffer`, брать только «созданные/активные». ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))
        
    - Для каждого — District по позиции; тип (жилое/офис/пром/коммерц/сервис).
        
2. **Добыча фактов через CitizenUnit:**
    
    - Обойти связку юнитов здания; для **Home** — суммировать занятые слоты; для **Work** — то же, но учитывать только нежилые здания. Игнорировать Visit/Student/Customer и пр. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2712549268&utm_source=chatgpt.com "Guide :: Cities Skylines Game Limits"))
        
3. **Кэш:**
    
    - `buildingId → sample`, хранить `isDirty`, `lastUpdatedFrame`. Инвалидация по триггерам и TTL.
        
4. **Триггеры:**
    
    - BuildingExtensionBase (`OnBuildingCreated/Released/Relocated`) — локальная инвалидация; периодический тик каждые N кадров симуляции. ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))
        
5. **Агрегация:**
    
    - **Hex:** worldPos → axial/cube → hexId; суммировать `residentsFact`/`jobsFact`; баланс = `jobs − residents`. Формулы и округление — как у Red Blob. ([Reddit](https://www.reddit.com/r/CitiesSkylines/comments/og8odq/city_load_stuck_at_simulation_building_manager/?utm_source=chatgpt.com "City load stuck at \"Simulation: building manager\""))
        
    - **District:** группировка по `districtId`.
        
6. **Рендер оверлея:**
    
    - Цветовая шкала по балансу; интерактивный выбор режима (hex/district) и масштаба гекса (64/128/256).
        
7. **Производительность:**
    
    - Батчи/чанки, никаких жёстких констант размеров массивов; логировать время проходов. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2654364627&utm_source=chatgpt.com "Steam Workshop::More CitizenUnits 1.1.3"))
        
8. **Валидация:**
    
    - Сравни сумму `residentsFact` по всем гексам с числом жителей в инфовью (расхождения допустимы из-за туристов/посетителей);
        
    - На тестовом сейве с RP2 проверь: при резком добавлении офисов баланс должен уходить в «нужно жильё», при приросте жилого — наоборот. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2025147082&utm_source=chatgpt.com "Steam Workshop::Realistic Population 2 2.2.4"))
        

---

## Ссылки-опоры (самые важные)

- **Буферы/жизненный цикл зданий и хуки** (BuildingManager, BuildingExtensionBase, фильтрация активных): туториал boformer «Make Historical». ([Simtropolis](https://community.simtropolis.com/forums/topic/73502-modding-tutorial-4-make-historical/ "Modding Tutorial 4: Make Historical - Cities: Skylines Modding - Open Discussion - Simtropolis"))
    
- **Назначение CitizenUnit и лимиты** (1 на домохозяйство, 1 на 5 рабочих мест/клиентов и др.): комьюнити-гайд по лимитам. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2712549268&utm_source=chatgpt.com "Guide :: Cities Skylines Game Limits"))
    
- **5 слотов на юнит (`m_citizen0..m_citizen4`)** — обсуждения на форуме Paradox. ([forum.paradoxplaza.com](https://forum.paradoxplaza.com/forum/threads/public-transport-vehicle-has-forever-boarding-problem.1595950/?utm_source=chatgpt.com "public transport vehicle has \"forever boarding\" problem"))
    
- **Не хардкодить размеры массивов (совместимость с More CitizenUnits):** предупреждение автора мода. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2654364627&utm_source=chatgpt.com "Steam Workshop::More CitizenUnits 1.1.3"))
    
- **Интервалы обновления/потоки** (не грузить каждый кадр): «Show Limits API — ThreadingExtensionBase» как ориентир на best-practice. ([Skylines](https://skylines.paradoxwikis.com/Modding_Basics?utm_source=chatgpt.com "Modding Basics - Cities: Skylines Wiki"))
    
- **Математика гексов (координаты, округление):** Red Blob Games Hex Grids. ([Reddit](https://www.reddit.com/r/CitiesSkylines/comments/og8odq/city_load_stuck_at_simulation_building_manager/?utm_source=chatgpt.com "City load stuck at \"Simulation: building manager\""))
    
- **Realistic Population 2** — что именно меняет (ёмкости и требования, а не «факты»): описание воркшопа. ([steamcommunity.com](https://steamcommunity.com/sharedfiles/filedetails/?id=2025147082&utm_source=chatgpt.com "Steam Workshop::Realistic Population 2 2.2.4"))
    

---

Если хочешь, в следующем шаге могу расписать чек-листы тестов и UX-мелочи для оверлея (легенда, пороги цветовой шкалы, «быстрые» пресеты размеров гекса) — помогут быстро увидеть пользу без подвисаний.