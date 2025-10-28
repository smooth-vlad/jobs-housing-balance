# Руководство по тестированию

## Текущий статус (Фаза 1 завершена)

### ✅ Что должно работать
- UI панель открывается с новыми контролами
- Переключение между режимами метрик
- Динамическая легенда обновляется
- EduMode dropdown появляется/скрывается
- Checkbox'ы работают корректно

### ⚠️ Что НЕ работает (это нормально)
- Capacity режим показывает те же данные, что и Fact
- Edu-aware режим показывает те же данные, что и Fact
- Нет данных об образовании в логах

## Как тестировать

### 1. Запуск мода
```
1. Запустить Cities: Skylines
2. Загрузить любой город
3. Найти кнопку мода JobsHousingBalance
4. Кликнуть для открытия панели
```

### 2. Проверка UI элементов
```
✅ Панель открывается без ошибок
✅ Видны новые контролы:
   - Metric dropdown (Fact/Capacity/Edu-aware)
   - IncludeServiceUnique checkbox (включен по умолчанию)
   - EduMode dropdown (скрыт, пока не выбран Edu-aware)
   - IncludeTeens checkbox (выключен по умолчанию)
✅ Легенда отображается корректно
```

### 3. Тестирование переключений
```
✅ Metric dropdown: переключение между Fact/Capacity/Edu-aware
✅ EduMode dropdown: появляется только при выборе Edu-aware
✅ Checkbox'ы: включение/выключение работает
✅ Легенда: меняется при смене режима метрики
```

## Логи для мониторинга

### Где смотреть логи
```
Cities: Skylines → Mods → JobsHousingBalance → Logs
Или в файле: ~/Library/Application Support/Colossal Order/Cities_Skylines/logs/
```

### Ожидаемые логи (успешный запуск)
```
JobsHousingBalance: AppState initialized with defaults
JobsHousingBalance: AppState - Mode: Hex, HexSize: Size128, Opacity: 0.80
JobsHousingBalance: AppState - MetricType: Fact, EducationMode: Strict
JobsHousingBalance: AppState - IncludeServiceUnique: True, IncludeTeens: False
JobsHousingBalance: MainPanel created successfully
JobsHousingBalance: Metric dropdown created
JobsHousingBalance: IncludeServiceUnique checkbox created
JobsHousingBalance: EduMode dropdown created
JobsHousingBalance: IncludeTeens checkbox created
JobsHousingBalance: Dynamic legend created
```

### Логи при переключении режимов
```
JobsHousingBalance: MetricType changed to Capacity
JobsHousingBalance: EduMode dropdown visibility set to False
JobsHousingBalance: Legend updated for Capacity

JobsHousingBalance: MetricType changed to EduAware
JobsHousingBalance: EduMode dropdown visibility set to True
JobsHousingBalance: Legend updated for EduAware
```

## Признаки проблем

### Критические ошибки
```
❌ JobsHousingBalance: Failed to create MainPanel
❌ JobsHousingBalance: Exception in MainPanel.Create: [ошибка]
❌ JobsHousingBalance: Failed to create [контрол]
```

### Проблемы с UI
```
❌ Панель не открывается
❌ Отсутствуют новые контролы
❌ EduMode dropdown не скрывается/не появляется
❌ Легенда не меняется при переключении режимов
❌ Ошибки компиляции при запуске игры
```

### Проблемы с данными (ожидаемо на этом этапе)
```
⚠️ Capacity режим показывает те же данные, что и Fact
⚠️ Edu-aware режим показывает те же данные, что и Fact
⚠️ Нет данных об образовании в логах
```

## Пошаговый тест

### Тест 1: Базовый запуск
```
1. Открыть игру
2. Открыть панель мода
3. Проверить: панель открылась без ошибок
4. Проверить: видны все новые контролы
```

### Тест 2: Переключение режимов
```
1. Переключить Metric на "Capacity"
2. Проверить: EduMode dropdown скрылся
3. Проверить: легенда изменилась на Capacity версию
4. Переключить Metric на "Edu-aware"
5. Проверить: EduMode dropdown появился
6. Проверить: легенда изменилась на Edu-aware версию
```

### Тест 3: Checkbox'ы
```
1. Включить/выключить IncludeServiceUnique
2. Проверить: состояние сохраняется
3. Включить/выключить IncludeTeens
4. Проверить: состояние сохраняется
```

## Ожидаемое поведение данных

### Fact режим (работает как раньше)
- Показывает реальные данные из CitizenUnit
- Красный = нужно больше рабочих мест
- Синий = нужно больше жилья

### Capacity режим (пока не работает)
- Должен показывать потенциальную ёмкость
- Пока показывает те же данные, что и Fact
- Это нормально до Фазы 2!

### Edu-aware режим (пока не работает)
- Должен учитывать образование
- Пока показывает те же данные, что и Fact
- Это нормально до Фазы 3!

## Критерии успеха Фазы 1

```
✅ UI работает без ошибок
✅ Все новые контролы отображаются
✅ Переключения работают корректно
✅ Легенда обновляется динамически
✅ Нет критических ошибок в логах
✅ Мод не падает при переключении режимов
```

## Следующие фазы

После успешного тестирования Фазы 1 можно переходить к:
- **Фаза 2**: Capacity режим (JobsCapacityCollector)
- **Фаза 3**: Education режим (EducationDataCollector)
- **Фаза 4**: Интеграция и тестирование
