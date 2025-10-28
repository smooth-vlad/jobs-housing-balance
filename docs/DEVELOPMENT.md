# Правила разработки и окружение

## Правила разработки (Development Rules)

- **Целевая версия игры**: Cities: Skylines (Original) — не Cities: Skylines II
- **UI Framework**: ColossalFramework.UI (нативный фреймворк игры для максимальной совместимости)
- **Обновление данных**: Только периодическое обновление (каждые 5-10 секунд), без хуков на события строительства
- **Совместимость**: Мод должен работать с популярными модами (Realistic Population, TM:PE, RICO) без конфликтов
- **Tooltip**: Не реализуется на данный момент

## Окружение разработки (Development Environment)

- **OS**: macOS
- **Framework**: Mono framework для разработки
- **IDE**: Cursor (VS Code fork)

### Технические параметры (Technical Settings)

- **Имя мода**: JobsHousingBalance
- **Версия**: 0.1
- **Автор**: Vladislav Gladkii
- **Фреймворк для модификации**: Harmony
- **Путь к DLL игры**: `/Users/vladislav/Library/Application Support/Steam/steamapps/common/Cities_Skylines/Cities.app/Contents/Resources/Data/Managed/`
- **Путь к папке модов**: `~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/` (**ВАЖНО**: ~ вместо /)
- **Путь к Harmony DLL**: `~/Library/Application Support/Steam/steamapps/workshop/content/255710/2040656402/CitiesHarmony.dll`
- **Компилятор**: Mono msbuild
- **Target Framework**: .NET Framework 3.5

### Команды сборки

**Сборка:**
```bash
/Library/Frameworks/Mono.framework/Versions/6.12.0/bin/msbuild JobsHousingBalance.csproj /p:Configuration=Release
```

**Путь установки DLL:**
```
~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/
```

**Размер DLL:** ~6KB + .pdb для отладки

## Текущий прогресс

### Завершенные задачи (Task 9.3)
- ✅ **Обработка кликов по кнопке** - Реализован стандартный паттерн `eventClick` с подавлением кликов после драга
- ✅ **Anti-spam защита** - Добавлен cooldown 150ms для предотвращения множественных кликов  
- ✅ **Корректное различение клик/драг** - Порог 6px, исправлен расчет расстояния драга
- ✅ **Debug логирование** - Готово для тестирования с ModTools (F7)

### Следующие задачи
- 🔄 **Task 9.4** - Создание UI панели управления
- 🔄 **Task 9.5** - Связывание клика с показом/скрытием панели

## Логирование

### Подход
Мод использует стандартное Unity логирование (`Debug.Log`, `Debug.LogWarning`, `Debug.LogError`) со всеми discovered предваряемыми префиксом "JobsHousingBalance: ".

### Как просматривать логи
Для просмотра логов мода используйте мод **ModTools** (2040656402):
1. Установите ModTools через Steam Workshop
2. В игре нажмите `F7` для открытия консоли мода
3. Фильтруйте логи по префиксу "JobsHousingBalance"
4. Все логи мода будут видны в реальном времени в консоли ModTools

