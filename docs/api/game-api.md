# Game API Guide

## Основные игровые API

### Системы игры
- **DistrictManager** - управление районами
- **BuildingManager** - управление зданиями
- **CitizenManager** - управление гражданами
- **VehicleManager** - управление транспортом

### Получение данных
```csharp
// Получение менеджера
var districtManager = Singleton<DistrictManager>.instance;

// Получение данных района
var districtData = districtManager.m_districts.m_buffer[districtId];
```

### Частые паттерны
- Использование `Singleton<T>.instance` для доступа к менеджерам
- Проверка на `null` перед использованием
- Использование `m_buffer` для доступа к данным

## Источники
- Официальная документация Cities: Skylines
- Анализ существующих модов
- Steam Workshop примеры
