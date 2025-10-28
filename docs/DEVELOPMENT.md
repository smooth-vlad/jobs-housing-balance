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
- ✅ **MVP 1**: Базовая структура проекта и UI система
- ✅ **Этап A**: Полная реализация UI контролов и архитектуры
  - ✅ **A.1**: Исправленные UI контролы (Mode, Hex size, Opacity, Legend)
  - ✅ **A.2**: AppState с событиями и singleton pattern
  - ✅ **A.3**: OverlayRenderer базовый класс
  - ✅ **A.4**: Event-driven интеграция UI ↔ Overlay
  - ✅ **A.5**: Статичная Legend с цветовыми индикаторами
- 🔄 **Этап B**: Реализация Hex overlay (планируется)
- 🔄 **Этап C**: Реализация Districts overlay (планируется)

## Архитектурные решения
- **Event-driven**: AppState ↔ UI ↔ OverlayRenderer через события
- **Singleton**: Thread-safe реализация для AppState и OverlayManager
- **UI Layout**: Автоматическая подгонка размеров с фиксированными элементами
- **Error Handling**: Try-catch блоки с детальным логированием