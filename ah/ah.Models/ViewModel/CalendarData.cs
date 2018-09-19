using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class CalendarData : Ass.Models.BaseCalendarData
    {
        public static CalendarData Create(Ass.Models.BaseCalendarData based)
        {
            return new CalendarData()
            {
                Date = based.Date,
                LunlarString = based.LunlarString,
                TermString = based.TermString
            };
        }
        public IEnumerable<CustomerRegisterItem> RegisterItems { get; set; }
        /// <summary>
        /// 是否有等待就诊的记录
        /// </summary>
        public bool HasTreating
        {
            get
            {
                foreach (CustomerRegisterItem item in RegisterItems)
                {
                    if (item.TreatStatus == "waiting") return true;
                }
                return false;
            }
        }
    }
    public class CustomerRegisterItem
    {
        string _TreatStatus = null;
        public string TreatStatus
        {
            get
            {
                return _TreatStatus ?? (_TreatStatus = _setStatus(register));
            }
        }

        internal ah.Models.vwCHIS_Register register = null;

        public string DepartmentName
        {
            get { return this.register.DepartmentName; }
        }
        public string ClinicName
        {
            get { return this.register.StationName; }
        }
        public string DoctorName
        {
            get { return this.register.DoctorName; }
        }

        public CustomerRegisterItem(ah.Models.vwCHIS_Register vwCHIS_Register)
        {
            this.register = vwCHIS_Register;
        }
        //设置是否过期
        public static string _setStatus(ah.Models.vwCHIS_Register reg)
        {
            string rtn = reg.TreatStatus;
            if (string.IsNullOrEmpty(reg.TreatStatus)) rtn = "waiting";
            if (rtn == "waiting" && reg.RegisterDate < DateTime.Today) rtn = "outtime";
            return rtn;
        }
    }
}
