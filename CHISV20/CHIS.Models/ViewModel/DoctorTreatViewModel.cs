using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.ViewModels
{
    public class DoctorTreatViewModel
    {
        /// <summary>
        /// 挂号信息
        /// </summary>
        public Models.CHIS_Register CustomerRegist { get; set; }

        /// <summary>
        /// 就诊信息
        /// </summary>
        public CHIS_DoctorTreat CHIS_DoctorTreat { get; set; }

        /// <summary>
        /// 就诊患者
        /// </summary>
        public Models.CHIS_Code_Customer Customer { get; set; }

        /// <summary>
        /// 患者健康基本信息
        /// </summary>
        public Models.CHIS_Code_Customer_HealthInfo CustomerHealthInfo { get; set; }


        public Models.CHIS_Code_Doctor Doctor { get; set; }
        public int FeeTypeId { get; set; }

        public Models.DataModel.CustomerClaimData OpUser { get; set; }

        /// <summary>
        /// 一体机的信息
        /// </summary>
        public Models.CHIS_DataInput_OneMachine OneMachineData { get; set; }
    }

 
    public class FeeSumaryViewModel
    {
        [System.ComponentModel.DataAnnotations.Key]
        public long TreatId { get; set; }
        public decimal TreatAmount { get; set; }
        public decimal NeedPayAmount { get; set; }
        public decimal TreatFee { get; set; }
        public decimal TransFee { get; set; } 
    }
}
