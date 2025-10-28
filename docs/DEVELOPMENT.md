# Правила разработки

## Окружение
- **OS**: macOS
- **IDE**: Cursor
- **Framework**: Mono (.NET Framework 3.5)
- **UI**: ColossalFramework.UI

## Команды сборки
```bash
/Library/Frameworks/Mono.framework/Versions/6.12.0/bin/msbuild JobsHousingBalance.csproj /p:Configuration=Release
```

**Пути:**
- DLL игры: `/Users/vladislav/Library/Application Support/Steam/steamapps/common/Cities_Skylines/Cities.app/Contents/Resources/Data/Managed/`
- Папка модов: `~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/`
- Harmony DLL: `~/Library/Application Support/Steam/steamapps/workshop/content/255710/2040656402/CitiesHarmony.dll`

## Логирование
- Префикс: "JobsHousingBalance: "
- Просмотр: ModTools (F7) в игре
- Фильтр по префиксу "JobsHousingBalance"

## Текущий прогресс
- ✅ **Task 9.4-9.6**: UI панель с toggle-логикой и исправленной кнопкой закрытия
- 🔄 **Task 9.7**: Сохранение позиции кнопки
- 🔄 **Task 9.8**: Элементы управления в панели