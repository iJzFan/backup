using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.ViewModel
{

    /// <summary>
    /// 新增挂号的实体模型
    /// </summary>
    public class NewRegistViewModel
    {
        public int StationId { get; set; }
        [Required(ErrorMessage ="必须要输入会员Id")]
        public long CustomerId { get; set; }

        [Required(ErrorMessage ="请选择科室")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage ="请选择医生")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage ="请选择约诊日期")]
        public DateTime ReservationDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 约诊操作来源的操作者Id
        /// </summary>
        public int? RegistOpId { get; set; }
        public string RegistOpMan { get; set; }

        /// <summary>
        /// 班段
        /// </summary>
        [Required(ErrorMessage ="请选择班段")]
        public int? ReservationSlot { get; set; }

        /// <summary>
        /// 过敏史
        /// </summary>
        public string Allergic { get; set; }
        /// <summary>
        /// 现病史
        /// </summary>
        public string PastMedicalHistory { get; set; }

        public vwCHIS_Code_Customer Customer { get; set; }
    }

    public class RegistNewCustomerViewModel
    {
        public string CustomerKeyText { get; set; }
        public vwCHIS_Code_Customer Customer { get; set; }
    }
}
