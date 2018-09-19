using System;
using System.Collections.Generic;
using System.Text;

namespace Ass.Models
{
    public class BaseCalendarData
    {
        public int Day { get { return Date.Day; } }
        public bool IsToday { get { return Date.Date == DateTime.Today; } }
        public DateTime Date { get; set; }
        public string LunlarString { get; set; }
        public string TermString { get; set; }
        public string LunlarShowString
        {
            get
            {
                if (!string.IsNullOrEmpty(TermString)) return TermString;
                else return LunlarString;
            }
        }


        /// <summary>
        /// 0 星期日 1 星期一 2星期二
        /// </summary>
        public int DayOfWeekIndex
        {
            get
            {
                return (int)Date.DayOfWeek;
            }
        }
    }
}
