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
        }
        
        /// <summary>
        /// Получить строковое представление для отладки
        /// </summary>
        public override string ToString()
        {
            return $"Building {buildingId}: Pos={position:F1}, District={districtId}, " +
                   $"Residents={residentsFact}, Jobs={jobsFact}, Balance={Balance}, " +
                   $"Service={service}, SubService={subService}";
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
    }
}
