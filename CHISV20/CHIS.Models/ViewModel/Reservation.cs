using Ass;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.ViewModels
{

    public class RaservationDoctorWorktime
    {
        public string Starttime { get; set; }
        public string Endtime { get; set; }
        public string DoctorId { get; set; }
    }


    public class EmployeeWorkInfo
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

    public class ReservateDoctorModel
    {        
        [Required]
        [BiggerZero]
        public int CustomerId { get; set; }
        [Required]
        [BiggerZero]
        public int DepartmentId { get; set; }
        [Required]
        [BiggerZero]
        public int DoctorId { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ReservationDate { get; set; }
        [Required]
        public int ReservationSlot { get; set; }

        public int? OpId { get; set; } = 0;
        public string OpMan { get; set; } = "";
        /// <summary>
        /// 处方医生 选填 药店填写
        /// </summary>
        public int? RxDoctorId { get; set; }
    }


    /// <summary>
    /// 用户 快速注册信息
    /// </summary>
    public class QuickRegistCustomerInfo
    {
      [Required(ErrorMessage ="用户名必须填写")]
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        [Required(ErrorMessage = "必须选择性别")]
        public int Gender { get; set; }
        [Required(ErrorMessage = "必须输入生日，以判断年龄")]
        public DateTime Birthday { get; set; }
    }

}
