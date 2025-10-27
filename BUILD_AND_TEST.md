# Сборка и тестирование мода (Build and Test Guide)

## ✅ РЕШЕНО: Мод теперь работает!

**Проблема была:** Мод собирался в неправильную папку (`/Library` вместо `~/Library`)

**Решение:** Изменили OutputPath в `.csproj` на `~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/`

**Результат:** Мод успешно появился в Content Manager и загружается без ошибок.

---

## Архив: Проблема: Мод не появляется в списке модов (решено)

### Возможные причины:

1. **Мод не зарегистрирован правильно** - добавили регистрацию `LoadingExtension` в `OnEnabled()`
2. **Зависимости не найдены** - нужно проверить логи игры
3. **Неправильная структура папки** - может потребоваться дополнительная регистрация
4. **DLL не загружается** - возможно проблема с версией .NET или зависимостями

## Диагностика проблемы

### 1. Проверить логи игры

Логи Cities: Skylines обычно находятся в:
```
~/Library/Application Support/Colossal Order/Cities_Skylines/
```

Найдите файл `output_log.txt` или `Cities.log` и проверьте:
- Есть ли ошибки загрузки DLL
- Появляется ли сообщение "JobsHousingBalance: Mod enabled"
- Есть ли ошибки зависимостей

### 2. Проверить структуру папки

Мод должен находиться в:
```
/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/
├── JobsHousingBalance.dll
└── JobsHousingBalance.pdb (опционально)
```

### 3. Проверить Content Manager

1. Запустите Cities: Skylines
2. Перейдите в меню **Content Manager**
3. Выберите вкладку **Mods**
4. Убедитесь что включен режим **All** (показывает все моды)
5. Проверьте есть ли мод в списке

### 4. Проверить что IUserMod реализован правильно

Мод должен реализовывать интерфейс `ICities.IUserMod` и иметь:
- `Name` property
- `Description` property
- `OnEnabled()` method
- `OnDisabled()` method

## Пересборка после изменений

После изменения кода пересоберите мод:

```bash
# Найти msbuild
which msbuild
# или
which xbuild

# Собрать проект
msbuild JobsHousingBalance.csproj /p:Configuration=Release

# Проверить что DLL обновлен
ls -lh "/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/JobsHousingBalance/"
```

## Следующие шаги для диагностики

1. **Проверить логи игры** - главное! Там будет точная ошибка
2. **Проверить Content Manager** - убедиться что мод не в списке
3. **Проверить DLL** - что файл обновился после пересборки
4. **Временная диагностика**: добавить больше логов для отладки

## Альтернативный подход

Если мод все еще не работает, можно попробовать:

1. Создать простейший мод-заглушку для проверки базовой функциональности
2. Проверить что другие моды (из Workshop) работают
3. Проверить версию Cities: Skylines
4. Попробовать запустить без других модов
