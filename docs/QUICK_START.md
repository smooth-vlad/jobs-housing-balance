# Quick Start

## Основные команды
```bash
# Сборка проекта
/Library/Frameworks/Mono.framework/Versions/6.12.0/bin/msbuild JobsHousingBalance.csproj /p:Configuration=Release

# Просмотр логов
ModTools (F7) в игре → фильтр "JobsHousingBalance"
```

## Структура проекта
- `src/UI/` - UI компоненты
- `src/LoadingExtension.cs` - инициализация
- `src/JobsHousingBalanceMod.cs` - точка входа

## Основные правила
- Читайте docs перед кодированием
- Исследуйте API через интернет
- Проверяйте на `null`
- Используйте отладочные логи
