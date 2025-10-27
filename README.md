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

Используйте xbuild (часть Mono):

```bash
xbuild JobsHousingBalance.csproj /p:Configuration=Release
```

Или используйте msbuild (если установлен):

```bash
msbuild JobsHousingBalance.csproj /p:Configuration=Release
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

Мод ссылается на следующие DLL из игры:
- `Assembly-CSharp.dll` - основные классы игры
- `ColossalManaged.dll` - фреймворк разработчиков
- `ICities.dll` - API для модов
- `UnityEngine.dll` - Unity API
- `UnityEngine.UI.dll` - UI компоненты Unity

Эти DLL находятся в папке игры.

## Замечания

- **0Harmony.dll**: Для задач 4+ потребуется установить Harmony. Добавьте его в проект или скачайте из Steam Workshop (mod ID: 2040656402)
- **Target Framework**: .NET Framework 3.5 (совместим с Cities: Skylines)

## Текущий статус

✅ **Task 1 (Базовая структура мода) завершена:**
- Мод успешно собирается и появляется в Content Manager
- Базовые классы созданы: `JobsHousingBalanceMod` (IUserMod), `LoadingExtension`
- Мод загружается без ошибок
- Готов к дальнейшей разработке функционала

## Разработка

См. документацию в папке `docs/`:
- `README.md` - описание проекта и требований
- `ARCHITECTURE.md` - архитектура приложения
- `TASKS.md` - задачи и план разработки

## Лицензия

MIT License
