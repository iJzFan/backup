using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.ViewModel
{
    public class DoctorPrintsViewModel
    {
        /// <summary>
        /// 挂号信息
        /// </summary>
        public CHIS.Models.vwCHIS_Register Registor { get; set; }
        /// <summary>
        /// 医生接诊信息
        /// </summary>
        public CHIS.Models.vwCHIS_DoctorTreat DoctorTreat { get; set; }

        public CHIS.Models.vwCHIS_Code_Doctor Doctor { get; set; }

        /// <summary>
        /// 患者信息
        /// </summary>
        public CHIS.Models.vwCHIS_Code_Customer Customer { get; set; }

        /// <summary>
        /// 西药信息
        /// </summary>
        public List<Print_ZH_Advices> Print_ZH_Advices { get; set; }

        /// <summary>
        /// 西药信息
        /// </summary>
        public List<Print_XY_Advices> Print_XY_Advices { get; set; }
        /// <summary>
        /// 中成药信息
        /// </summary>
        public List<Print_ZYC_Advices> Print_ZYC_Advices { get; set; }
        /// <summary>
        /// 中药方剂信息
        /// </summary>
        public List<Print_CnHerbs> Print_CnHerbs { get; set; }
 
    }


    /// <summary>
    /// 综合类，不进行详细区分
    /// </summary>
    public class Print_ZH_Advices
    {
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? Amount
        {
            get
            {
                return ZH_Advices.Sum(m => m.Amount);
            }
        }
        public string PrescriptionNumber { get; set; }
        public List<CHIS.Models.vwCHIS_DoctorAdvice_Formed_Detail> ZH_Advices { get; set; }
        public Print_ZH_Advices()
        {
            ZH_Advices = new List<CHIS.Models.vwCHIS_DoctorAdvice_Formed_Detail>();
        }
    }


    public class Print_XY_Advices
    {
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? Amount { get {
                return XY_Advices.Sum(m => m.Amount);
            } }
        public string PrescriptionNumber { get; set; }
        public List<CHIS.Models.vwCHIS_DoctorAdvice_Formed_Detail> XY_Advices { get; set; }
        public Print_XY_Advices()
        {
            XY_Advices = new List<CHIS.Models.vwCHIS_DoctorAdvice_Formed_Detail>();
        }
    }
    public class Print_ZYC_Advices
    {
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? Amount{get{return ZYC_Advices.Sum(m => m.Amount);}}
        public string PrescriptionNumber { get; set; }
        public List<CHIS.Models.vwCHIS_DoctorAdvice_Herbs_Detail> ZYC_Advices { get; set; }
        public Print_ZYC_Advices()
        {
            ZYC_Advices = new List<CHIS.Models.vwCHIS_DoctorAdvice_Herbs_Detail>();
        }
    }
    
 
    public class Print_CnHerbs : CHIS.Models.ViewModel.CnHerbsViewModel
    {
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? Amount { get { return base.MainAdvice.Amount; } }

        public Guid PrescriptionNumber { get { return base.MainAdvice.PrescriptionNo; } }
    }

    public class CnHerbsViewModel
    {
        public Models.CHIS_DoctorAdvice_Herbs MainAdvice { get; set; }
        public List<Models.CHIS_DoctorAdvice_Herbs_Detail> HerbsDetails { get; set; }

    }
}
