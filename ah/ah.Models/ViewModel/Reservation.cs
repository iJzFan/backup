using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ah.Models.ViewModel
{

    public class RaservationDoctorWorktime
    {
        public string Starttime { get; set; }
        public string Endtime { get; set; }
        public string DoctorID { get; set; }
    }


    public class DoctorWorkInfo
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime WorkDate { get; set; }
        public int SlotIndex { get; set; }
        public TimeSpan SlotTimeStart { get; set; }
        public TimeSpan SlotTimeEnd { get; set; }
        public string Work_Vacation { get; set; }
        public int AllowRegNum { get; set; }
        public int ReservateLimitNum { get; set; }
        public int ReservatedNum { get; set; }
        public bool IsWork { get { return Work_Vacation == "WORK"; } }
    }


    /// <summary>
    /// 预约结果
    /// </summary>
    public class ReservationRlt
    {
        public bool rlt { get; set; }
        public string msg { get; set; }
        public string rltCode { get; set; }
        public string registStatus { get; set; }
        public long registerId { get; set; }
        public int? registerSeq { get; set; }
        public vwCHIS_Code_Customer customer { get; set; }
        public string stationName { get; set; }
        public string departmentName { get; set; }
        public vwCHIS_Code_Doctor doctor { get; set; }
        public DateTime reservationDate { get; set; }
        public string timeInfo { get; set; }

        public Exception ex { get; set; }
    }

    public class ReservationInfo
    {
        public int DepartmentId { get; set; }
        public int StationId { get; set; }
        public int DoctorId { get; set; }
        public int RxDoctorId { get; set; }
        public int CustomerId { get; set; }
        public DateTime ReservationDate { get; set; }
        public int ReservationSlot { get; set; }
        public int OpId { get; set; }
        public string OpMan { get; set; }
    }
    public class ResvationReturn
    {
        public bool rlt { get; set; }
        public string msg { get; set; }
        public string state { get; set; }
        public string rltCode { get; set; }
        public string registStatus { get; set; }
        public long registerId { get; set; }
        public vwCHIS_Code_Customer customer { get; set; }
        public string stationName { get; set; }
        public string departmentName { get; set; }
        public vwCHIS_Code_Doctor employee { get; set; }
        public string reservationDate { get; set; }
        public string timeInfo { get; set; }
        public string OpenId { get; set; }
    }

    /// <summary>
    /// 医生基础类00
    /// </summary>
    public class DoctorSEntityV00
    {
        public int DoctorOpId { get { return CustomerId; } }
        public int CustomerId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorGender { get; set; }
        public string PostTitleName { get; set; }
        public string DoctorSkillRmk { get; set; }
        public string DoctorPhotoUrl { get; set; }

        /// <summary>
        /// 医生App端的Id
        /// </summary>
        public string DoctorAppId { get; set; }
    }

    /// <summary>
    /// 医生扩展类01
    /// </summary>
    public class DoctorSEntityV01 : DoctorSEntityV00
    {
        /// <summary>
        /// 默认科室
        /// </summary>
        public int? DefDepartmentId { get; set; }
    }
}
