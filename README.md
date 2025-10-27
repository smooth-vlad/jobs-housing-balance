# Jobs Housing Balance Mod

Мод для Cities: Skylines, визуализирующий баланс рабочих мест и жилья в городе.

## Требования (Requirements)

- Cities: Skylines (оригинальная версия, не CS2)
- Mono Framework (для сборки на macOS)
- IDE с поддержкой C# (Visual Studio, Rider, Cursor)

## Установка Mono на macOS

```bash
brew install mono
```

Или скачайте с официального сайта: https://www.mono-project.com/download/

## Сборка проекта (Building)

### На macOS

Используйте msbuild с конкретным путем:

```bash
/Library/Frameworks/Mono.framework/Versions/6.12.0/bin/msbuild JobsHousingBalance.csproj /p:Configuration=Release
```

### На Windows/Visual Studio

1. Откройте `JobsHousingBalance.csproj` в Visual Studio
2. Выберите конфигурацию `Release`
3. Соберите проект (Build Solution)

### Результат сборки

DLL файл будет автоматически скопирован в:
- **macOS**: `~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/JobsHousingBalance.dll`
- **Windows**: `%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Addons\Mods\JobsHousingBalance\JobsHousingBalance.dll`

**Важно для macOS**: Используется `~/Library` (домашняя папка пользователя), а не `/Library` (системная папка)!

## Установка мода

1. Соберите проект (см. выше)
2. Запустите Cities: Skylines
3. Перейдите в меню Content Manager → Mods
4. Найдите "Jobs Housing Balance" в списке модов
5. Убедитесь, что мод включен (активная галочка)

## Проверка работы

1. Запустите Cities: Skylines
2. Загрузите город или создайте новый
3. Откройте консоль разработчика (F7 или Ctrl+Shift+D)
4. Найдите в консоли сообщение: `JobsHousingBalance: Level loaded successfully`

## Структура проекта

```
jobs-housing-balance/
├── docs/                    # Документация проекта
├── src/                     # Исходный код мода
│   ├── JobsHousingBalanceMod.cs
│   ├── LoadingExtension.cs
│   ├── Aggregation/
│   ├── Data/
│   ├── Rendering/
│   ├── UI/
│   └── Utils/
├── Properties/
│   └── AssemblyInfo.cs      # Метаданные сборки
└── JobsHousingBalance.csproj
```

## Зависимости

Мод ссылается на следующие DLL:

**Из игры (Cities: Skylines):**
- `Assembly-CSharp.dll` - основные классы игры
- `ColossalManaged.dll` - фреймворк разработчиков
- `ICities.dll` - API для модов
- `UnityEngine.dll` - Unity API
- `UnityEngine.UI.dll` - UI компоненты Unity

**Из Steam Workshop (Harmony):**
- `CitiesHarmony.dll` - обёртка Harmony для Cities: Skylines
- `CitiesHarmony.Harmony.dll` - Harmony API

Все DLL находятся в соответствующих папках установки.

## Зависимости Harmony

- **CitiesHarmony**: Мод использует CitiesHarmony из Steam Workshop:
  - Путь на macOS: `~/Library/Application Support/Steam/steamapps/workshop/content/255710/2040656402/`
  - Mod ID: 2040656402 (Harmony)
  - Используемые DLL: `CitiesHarmony.dll` и `CitiesHarmony.Harmony.dll`
  - **ВАЖНО**: Harmony должен быть установлен в игре для работы мода

## Target Framework

- .NET Framework 3.5 (совместим с Cities: Skylines)

## Просмотр логов

Мод использует стандартное Unity логирование. Для просмотра логов:

1. Установите мод **ModTools** (ID: 2040656402) через Steam Workshop
2. Запустите Cities: Skylines и загрузите мод в игру
3. В игре нажмите `F7` для открытия консоли ModTools
4. Фильтруйте логи по префиксу "JobsHousingBalance" для просмотра только логов этого мода

## Текущий статус

✅ **MVP (Tasks 1-8) завершено:**
- Мод успешно собирается и появляется в Content Manager
- Базовые классы созданы: `JobsHousingBalanceMod` (IUserMod), `LoadingExtension`
- Harmony интеграция реализована: `HarmonyPatcher` класс создан
- Мод загружается без ошибок и применяет Harmony патчи
- Готов к дальнейшей разработке функционала визуализации

## Разработка

См. документацию в папке `docs/`:
- `README.md` - описание проекта и требований
- `ARCHITECTURE.md` - архитектура приложения
- `TASKS.md` - задачи и план разработки

## Лицензия

MIT License
