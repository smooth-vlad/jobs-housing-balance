using System;
using System.Reflection;
using UnityEngine;
using ColossalFramework;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Вспомогательный класс для определения уровня образования граждан
    /// Централизует логику определения образования для избежания дублирования
    /// </summary>
    public static class CitizenEducationHelper
    {
        #region Fields
        
        // Кэш для результатов reflection (повышение производительности)
        private static PropertyInfo _educationLevelProperty;
        private static FieldInfo _educationLevelField;
        private static FieldInfo _educationField;
        private static FieldInfo _education1Field;
        private static MethodInfo _getEducationLevelMethod;
        private static PropertyInfo _workBuildingProperty;
        private static FieldInfo _workBuildingField;
        private static FieldInfo _workPlaceField;
        
        private static bool _reflectionCacheInitialized = false;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Определить уровень образования гражданина
        /// Использует реальные поля образования из Citizen, а не случайное распределение по возрасту
        /// </summary>
        public static int GetCitizenEducationLevel(Citizen citizen)
        {
            try
            {
                // Инициализируем кэш reflection при первом вызове
                if (!_reflectionCacheInitialized)
                {
                    InitializeReflectionCache();
                }
                
                // Попытка 1: Использовать свойство EducationLevel если есть
                if (_educationLevelProperty != null)
                {
                    var educationLevel = (int)_educationLevelProperty.GetValue(citizen, null);
                    if (educationLevel >= 0 && educationLevel <= 3)
                    {
                        return educationLevel;
                    }
                }
                
                // Попытка 2: Использовать поле m_educationLevel
                if (_educationLevelField != null)
                {
                    var educationLevel = (int)_educationLevelField.GetValue(citizen);
                    if (educationLevel >= 0 && educationLevel <= 3)
                    {
                        return educationLevel;
                    }
                }
                
                // Попытка 3: Использовать поле m_education
                if (_educationField != null)
                {
                    var education = (int)_educationField.GetValue(citizen);
                    if (education >= 0 && education <= 3)
                    {
                        return education;
                    }
                }
                
                // Попытка 4: Использовать поле m_education1 (битовая маска)
                if (_education1Field != null)
                {
                    var education1 = (int)_education1Field.GetValue(citizen);
                    
                    // Проверяем биты: 0b0001=1, 0b0010=2, 0b0100=4, 0b1000=8
                    if ((education1 & 0b1000) != 0) return 3; // Highly educated
                    if ((education1 & 0b0100) != 0) return 2; // Well educated
                    if ((education1 & 0b0010) != 0) return 1; // Educated
                    if ((education1 & 0b0001) != 0) return 0; // Uneducated
                }
                
                // Попытка 5: Использовать метод GetEducationLevel если есть
                if (_getEducationLevelMethod != null)
                {
                    var educationLevel = (int)_getEducationLevelMethod.Invoke(citizen, null);
                    if (educationLevel >= 0 && educationLevel <= 3)
                    {
                        return educationLevel;
                    }
                }
                
                // Fallback: если ничего не найдено, используем возраст как временное решение
                // НО с предупреждением, что это не корректно
                Debug.LogWarning("JobsHousingBalance: Could not find education field in Citizen, using age-based fallback. " +
                               "Please check Assembly-CSharp.dll with ILSpy to find correct field name.");
                
                var age = citizen.Age;
                if (age >= 6) return 3; // Пожилые - обычно высокообразованные
                if (age >= 4) return 2; // Взрослые - хорошо образованные
                if (age >= 2) return 1; // Молодые взрослые - образованные
                return 0; // Дети - необразованные
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error getting citizen education level: {ex.Message}");
                return 0; // Fallback к необразованным
            }
        }
        
        /// <summary>
        /// Получить здание, в котором работает гражданин
        /// Использует reflection для поиска поля m_workBuilding
        /// </summary>
        public static ushort GetCitizenWorkBuilding(Citizen citizen)
        {
            try
            {
                // Инициализируем кэш reflection при первом вызове
                if (!_reflectionCacheInitialized)
                {
                    InitializeReflectionCache();
                }
                
                // Попытка 1: Использовать свойство WorkBuilding если есть
                if (_workBuildingProperty != null)
                {
                    return (ushort)_workBuildingProperty.GetValue(citizen, null);
                }
                
                // Попытка 2: Использовать поле m_workBuilding
                if (_workBuildingField != null)
                {
                    return (ushort)_workBuildingField.GetValue(citizen);
                }
                
                // Попытка 3: Использовать поле m_workPlace
                if (_workPlaceField != null)
                {
                    return (ushort)_workPlaceField.GetValue(citizen);
                }
                
                Debug.LogWarning("JobsHousingBalance: Could not find work building field in Citizen. " +
                               "Please check Assembly-CSharp.dll with ILSpy to find correct field name.");
                return 0;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error getting citizen work building: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Проверить, является ли гражданин работоспособного возраста
        /// </summary>
        public static bool IsWorkingAge(Citizen citizen, bool includeTeens = false)
        {
            var age = citizen.Age;
            
            // По умолчанию считаем young adults + adults (возраст 2-5)
            // Опционально включаем teens при включенной настройке
            if (includeTeens)
            {
                return age >= 1; // teens (1) + young adults (2) + adults (3-5)
            }
            else
            {
                return age >= 2; // young adults (2) + adults (3-5)
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Инициализировать кэш reflection для повышения производительности
        /// </summary>
        private static void InitializeReflectionCache()
        {
            try
            {
                var citizenType = typeof(Citizen);
                
                // Кэшируем свойства и поля для образования
                _educationLevelProperty = citizenType.GetProperty("EducationLevel");
                _educationLevelField = citizenType.GetField("m_educationLevel", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                _educationField = citizenType.GetField("m_education", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                _education1Field = citizenType.GetField("m_education1", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                _getEducationLevelMethod = citizenType.GetMethod("GetEducationLevel", 
                    BindingFlags.Public | BindingFlags.Instance);
                
                // Кэшируем поля для рабочего здания
                _workBuildingProperty = citizenType.GetProperty("WorkBuilding");
                _workBuildingField = citizenType.GetField("m_workBuilding", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                _workPlaceField = citizenType.GetField("m_workPlace", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                _reflectionCacheInitialized = true;
                
                Debug.Log("JobsHousingBalance: Reflection cache initialized for Citizen education fields");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"JobsHousingBalance: Error initializing reflection cache: {ex.Message}");
                _reflectionCacheInitialized = true; // Помечаем как инициализированный, чтобы не повторять попытки
            }
        }
        
        #endregion
    }
}
