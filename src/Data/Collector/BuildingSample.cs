using UnityEngine;

namespace JobsHousingBalance.Data.Collector
{
    /// <summary>
    /// Структура данных для хранения информации о здании
    /// </summary>
    public struct BuildingSample
    {
        /// <summary>
        /// ID здания для отладки и связи с игровыми данными
        /// </summary>
        public ushort buildingId;
        
        /// <summary>
        /// Позиция здания в мире
        /// </summary>
        public Vector3 position;
        
        /// <summary>
        /// ID района, к которому принадлежит здание
        /// </summary>
        public byte districtId;
        
        /// <summary>
        /// Фактическое количество жителей (из CitizenUnits с флагом Home)
        /// </summary>
        public int residentsFact;
        
        /// <summary>
        /// Фактическое количество рабочих мест (из CitizenUnits с флагом Work)
        /// </summary>
        public int jobsFact;
        
        /// <summary>
        /// Общая ёмкость рабочих мест (из BuildingAI/инфопанели)
        /// </summary>
        public int jobsCapacityTotal;
        
        /// <summary>
        /// Ёмкость рабочих мест по уровням образования (размер 4)
        /// Индекс 0: uneducated, 1: educated, 2: well-educated, 3: highly-educated
        /// </summary>
        public int[] jobsCapacityByEdu;
        
        /// <summary>
        /// Жители по уровням образования (размер 4)
        /// Индекс 0: uneducated, 1: educated, 2: well-educated, 3: highly-educated
        /// </summary>
        public int[] residentsByEdu;
        
        /// <summary>
        /// Тип здания (жилое/коммерческое/промышленное/сервисное)
        /// </summary>
        public ItemClass.Service service;
        
        /// <summary>
        /// Подтип здания (жилое/офисное/промышленное и т.д.)
        /// </summary>
        public ItemClass.SubService subService;
        
        /// <summary>
        /// Баланс рабочих мест и жителей (jobsFact - residentsFact)
        /// Положительное значение = нужно больше жилья
        /// Отрицательное значение = нужно больше рабочих мест
        /// </summary>
        public int Balance => jobsFact - residentsFact;
        
        /// <summary>
        /// Создать новый сэмпл здания
        /// </summary>
        public BuildingSample(ushort buildingId, Vector3 position, byte districtId, 
                            int residentsFact, int jobsFact, 
                            ItemClass.Service service, ItemClass.SubService subService)
        {
            this.buildingId = buildingId;
            this.position = position;
            this.districtId = districtId;
            this.residentsFact = residentsFact;
            this.jobsFact = jobsFact;
            this.service = service;
            this.subService = subService;
            
            // Инициализация новых полей значениями по умолчанию
            this.jobsCapacityTotal = 0;
            this.jobsCapacityByEdu = new int[4]; // uneducated, educated, well-educated, highly-educated
            this.residentsByEdu = new int[4];     // uneducated, educated, well-educated, highly-educated
        }
        
        /// <summary>
        /// Получить строковое представление для отладки
        /// </summary>
        public override string ToString()
        {
            var capacityInfo = jobsCapacityTotal > 0 ? $", Capacity={jobsCapacityTotal}" : "";
            var eduInfo = "";
            
            // Добавляем информацию об образовании только если есть данные
            if (jobsCapacityByEdu != null && (jobsCapacityByEdu[0] > 0 || jobsCapacityByEdu[1] > 0 || 
                jobsCapacityByEdu[2] > 0 || jobsCapacityByEdu[3] > 0))
            {
                eduInfo = $", JobsEdu=[{jobsCapacityByEdu[0]},{jobsCapacityByEdu[1]},{jobsCapacityByEdu[2]},{jobsCapacityByEdu[3]}]";
            }
            
            if (residentsByEdu != null && (residentsByEdu[0] > 0 || residentsByEdu[1] > 0 || 
                residentsByEdu[2] > 0 || residentsByEdu[3] > 0))
            {
                eduInfo += $", ResidentsEdu=[{residentsByEdu[0]},{residentsByEdu[1]},{residentsByEdu[2]},{residentsByEdu[3]}]";
            }
            
            var typeInfo = "";
            if (IsServiceBuilding) typeInfo += ", Service";
            if (IsUniqueBuilding) typeInfo += ", Unique";
            
            return $"Building {buildingId}: Pos={position:F1}, District={districtId}, " +
                   $"Residents={residentsFact}, Jobs={jobsFact}{capacityInfo}{eduInfo}, " +
                   $"Balance={Balance}, Service={service}, SubService={subService}{typeInfo}";
        }
        
        /// <summary>
        /// Проверить, является ли здание жилым
        /// </summary>
        public bool IsResidential => service == ItemClass.Service.Residential;
        
        /// <summary>
        /// Проверить, является ли здание нежилым (коммерческое, промышленное, офисное, сервисное)
        /// </summary>
        public bool IsNonResidential => service != ItemClass.Service.Residential;
        
        /// <summary>
        /// Проверить, имеет ли здание значимый баланс (не пустое)
        /// </summary>
        public bool HasSignificantBalance => residentsFact > 0 || jobsFact > 0;
        
        /// <summary>
        /// Проверить, является ли здание сервисным
        /// </summary>
        public bool IsServiceBuilding => false; // TODO: Implement proper service building detection
        
        /// <summary>
        /// Проверить, является ли здание уникальным
        /// </summary>
        public bool IsUniqueBuilding => false; // TODO: Implement proper unique building detection
        
        /// <summary>
        /// Получить общее количество жителей по всем уровням образования
        /// </summary>
        public int TotalResidentsByEdu
        {
            get
            {
                if (residentsByEdu == null) return 0;
                return residentsByEdu[0] + residentsByEdu[1] + residentsByEdu[2] + residentsByEdu[3];
            }
        }
        
        /// <summary>
        /// Получить общее количество рабочих мест по всем уровням образования
        /// </summary>
        public int TotalJobsCapacityByEdu
        {
            get
            {
                if (jobsCapacityByEdu == null) return 0;
                return jobsCapacityByEdu[0] + jobsCapacityByEdu[1] + jobsCapacityByEdu[2] + jobsCapacityByEdu[3];
            }
        }
        
        /// <summary>
        /// Проверить, есть ли данные о ёмкости рабочих мест
        /// </summary>
        public bool HasCapacityData => jobsCapacityTotal > 0 || TotalJobsCapacityByEdu > 0;
        
        /// <summary>
        /// Проверить, есть ли данные об образовании жителей
        /// </summary>
        public bool HasEducationData => TotalResidentsByEdu > 0;
    }
}
