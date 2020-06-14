using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelephoneCallsBTK.Model
{
    public class StoryNumber
    {
        /// <summary>
        /// Телефон
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Наименование услуги
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Направление
        /// </summary>
        public string Direction { get; set; }
        /// <summary>
        /// Номер вызываемого/вызывающего абонента 
        /// </summary>
        public string CalledCallerNumber { get; set; }
        /// <summary>
        /// Дата,Время начала
        /// </summary>
        public string DateStartTime { get; set; }
        /// <summary>
        /// Прод.(мин)/Кол.услуг
        /// </summary>
        public string Duration { get; set; }
        /// <summary>
        /// Стоимость
        /// </summary>
        public string Coast { get; set; }
    }

}
