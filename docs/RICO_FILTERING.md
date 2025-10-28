# RICO Filtering - Фильтрация зданий

## Проблема
При подсчете зданий в Cities: Skylines получалось ~600 "non-residential" зданий вместо ожидаемых ~20 реальных зданий.

## Причина
В буфере `BuildingManager` содержится множество технических объектов:
- **Sub-buildings** (дочерние здания с `m_parentBuilding != 0`)
- **Технические флаги** (`Untouchable`, `Hidden`, `Upgrading`, `Downgrading`, etc.)
- **Сервисные здания** (школы, больницы, полиция, пожарные)
- **Уникальные здания** (монументы, туризм)
- **Инфраструктурные объекты** (склады, сервис-пойнты)

## Решение: RICO-only фильтрация

### Что такое RICO
**RICO** = **R**esidential, **I**ndustrial, **C**ommercial, **O**ffice
- Это основные типы зданий для баланса рабочих мест и жителей
- Исключает сервисы, уникалы и технические объекты

### Реализованная фильтрация

#### 1. Усиленная валидация зданий
```csharp
private bool IsValidBuilding(Building building)
{
    // Основные флаги существования
    if ((building.m_flags & Building.Flags.Created) == 0) return false;
    if ((building.m_flags & Building.Flags.Deleted) != 0) return false;
    
    // Исключаем технические флаги
    if ((building.m_flags & (Building.Flags.Untouchable | 
                           Building.Flags.Hidden | 
                           Building.Flags.Upgrading | 
                           Building.Flags.Downgrading | 
                           Building.Flags.Collapsed | 
                           Building.Flags.Abandoned | 
                           Building.Flags.Evacuating)) != 0) return false;
    
    // Исключаем sub-buildings
    if (building.m_parentBuilding != 0) return false;
    
    // Проверяем RICO
    var service = building.Info.GetService();
    if (!IsRicoBuilding(service, building.Info.GetSubService())) return false;
    
    return true;
}
```

#### 2. RICO проверка
```csharp
private bool IsRicoBuilding(ItemClass.Service service, ItemClass.SubService subService)
{
    switch (service)
    {
        case ItemClass.Service.Residential: return true;
        case ItemClass.Service.Commercial: return true;
        case ItemClass.Service.Industrial: return true;
        case ItemClass.Service.Office: return true;
        default: return false; // Все остальные сервисы исключаем
    }
}
```

#### 3. Диагностика типов зданий
```csharp
private void LogBuildingTypeDiagnostics()
{
    // Подсчет RICO зданий
    var residentialCount = 0;
    var commercialCount = 0;
    var industrialCount = 0;
    var officeCount = 0;
    
    // Анализ буфера зданий
    var totalBuildingsInBuffer = 0;
    var validRicoBuildings = 0;
    var excludedBuildings = 0;
    
    Debug.Log($"RICO Buildings - Residential: {residentialCount}, Commercial: {commercialCount}, Industrial: {industrialCount}, Office: {officeCount}");
    Debug.Log($"Building buffer analysis - Total: {totalBuildingsInBuffer}, Valid RICO: {validRicoBuildings}, Excluded: {excludedBuildings}");
}
```

## Результат

### До исправления
- **Total buildings**: ~600
- **Non-residential**: ~600 (включая сервисы, уникалы, технические объекты)

### После исправления
- **Total buildings**: ~20
- **RICO buildings**: ~20 (только Residential, Commercial, Industrial, Office)
- **Excluded**: ~580 (сервисы, уникалы, технические объекты)

## Исключенные типы зданий

### Сервисы (по настройке IncludeServiceUnique)
- `ItemClass.Service.Education` - школы, университеты
- `ItemClass.Service.HealthCare` - больницы, клиники
- `ItemClass.Service.PoliceDepartment` - полицейские участки
- `ItemClass.Service.FireDepartment` - пожарные станции
- `ItemClass.Service.Garbage` - мусорные станции
- `ItemClass.Service.Water` - водонапорные башни
- `ItemClass.Service.Electricity` - электростанции
- `ItemClass.Service.PublicTransport` - остановки, станции

### Уникальные здания
- `ItemClass.Service.Monument` - монументы
- `ItemClass.Service.Beautification` - декоративные объекты
- `ItemClass.Service.Tourism` - туристические объекты

### Технические объекты
- **Sub-buildings** (`m_parentBuilding != 0`)
- **Untouchable** - невидимые технические объекты
- **Hidden** - скрытые объекты
- **Upgrading/Downgrading** - здания в процессе апгрейда
- **Collapsed/Abandoned** - разрушенные/заброшенные здания
- **Evacuating** - здания в процессе эвакуации

## Преимущества RICO фильтрации

1. **Точный баланс**: Только здания, влияющие на баланс рабочих мест и жителей
2. **Производительность**: Меньше объектов для обработки
3. **Понятность**: Четкое разделение на RICO и сервисы
4. **Диагностика**: Видно что именно попадает в подсчет
5. **Гибкость**: Сервисы можно включить отдельно через настройку

## Настройки

- **IncludeServiceUnique**: Включение сервисов и уникалов в анализ (по умолчанию выключено)
- **RICO-only**: Основной режим для баланса рабочих мест и жителей
