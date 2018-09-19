//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ah.Models.ViewModel
//{
//    public class DoctorPrintsViewModel
//    {
//        /// <summary>
//        /// 挂号信息
//        /// </summary>
//        public ah.Models.vwCHIS_Register Registor { get; set; }
//        /// <summary>
//        /// 医生接诊信息
//        /// </summary>
//        public ah.Models.vwCHIS_DoctorTreat DoctorTreat { get; set; }

//        public ah.Models.vwCHIS_Code_Doctor Doctor { get; set; }

//        /// <summary>
//        /// 患者信息
//        /// </summary>
//        public ah.Models.vwCHIS_Code_Customer Customer { get; set; }

//        /// <summary>
//        /// 西药信息
//        /// </summary>
//        public List<Print_ZH_Advices> Print_ZH_Advices { get; set; }

//        /// <summary>
//        /// 西药信息
//        /// </summary>
//        public List<Print_XY_Advices> Print_XY_Advices { get; set; }
//        /// <summary>
//        /// 中成药信息
//        /// </summary>
//        public List<Print_ZYC_Advices> Print_ZYC_Advices { get; set; }
//        /// <summary>
//        /// 中药方剂信息
//        /// </summary>
//        public List<Print_CnHerbs> Print_CnHerbs { get; set; }
//        /// <summary>
//        /// 检验信息
//        /// </summary>
//        public List<Print_TestItems> Print_TestItems { get; set; }
//        /// <summary>
//        /// 检查信息
//        /// </summary>
//        public List<Print_CheckItems> Print_CheckItems { get; set; }
//    }


//    /// <summary>
//    /// 综合类，不进行详细区分
//    /// </summary>
//    public class Print_ZH_Advices
//    {
//        /// <summary>
//        /// 总金额
//        /// </summary>
//        public decimal? Amount
//        {
//            get
//            {
//                return ZH_Advices.Sum(m => m.Amount);
//            }
//        }
//        public string PrescriptionNumber { get; set; }
//        public List<ah.Models.vwCHIS_Doctor_AdviceItem> ZH_Advices { get; set; }
//        public Print_ZH_Advices()
//        {
//            ZH_Advices = new List<ah.Models.vwCHIS_Doctor_AdviceItem>();
//        }
//    }


//    public class Print_XY_Advices
//    {
//        /// <summary>
//        /// 总金额
//        /// </summary>
//        public decimal? Amount { get {
//                return XY_Advices.Sum(m => m.Amount);
//            } }
//        public string PrescriptionNumber { get; set; }
//        public List<ah.Models.vwCHIS_Doctor_AdviceItem> XY_Advices { get; set; }
//        public Print_XY_Advices()
//        {
//            XY_Advices = new List<ah.Models.vwCHIS_Doctor_AdviceItem>();
//        }
//    }
//    public class Print_ZYC_Advices
//    {
//        /// <summary>
//        /// 总金额
//        /// </summary>
//        public decimal? Amount{get{return ZYC_Advices.Sum(m => m.Amount);}}
//        public string PrescriptionNumber { get; set; }
//        public List<ah.Models.vwCHIS_Doctor_AdviceItem> ZYC_Advices { get; set; }
//        public Print_ZYC_Advices()
//        {
//            ZYC_Advices = new List<ah.Models.vwCHIS_Doctor_AdviceItem>();
//        }
//    }
//    public class Print_TestItems
//    {
//        /// <summary>
//        /// 总金额
//        /// </summary>
//        public decimal? Amount { get { return TestItems.Sum(m => m.Amount); } }
//        public string PrescriptionNumber { get; set; }
//        public List<ah.Models.vwCHIS_Doctor_TestItem> TestItems { get; set; }
//        public Print_TestItems()
//        {
//            TestItems = new List<ah.Models.vwCHIS_Doctor_TestItem>();
//        }
//    }
//    public class Print_CheckItems
//    {
//        /// <summary>
//        /// 总金额
//        /// </summary>
//        public decimal? Amount { get { return CheckItems.Sum(m => m.Amount); } }
//        public string PrescriptionNumber { get; set; }
//        public List<ah.Models.vwCHIS_Doctor_CheckItem> CheckItems { get; set; }
//        public Print_CheckItems()
//        {
//            CheckItems = new List<ah.Models.vwCHIS_Doctor_CheckItem>();
//        }
//    }
//    public class Print_CnHerbs : ah.Models.ViewModel.CnHerbsViewModel
//    {
//        /// <summary>
//        /// 总金额
//        /// </summary>
//        public decimal? Amount { get { return base.MainAdvice.Amount; } }

//        public string PrescriptionNumber { get { return base.MainAdvice.PrescriptionNo; } }
//    }

//    public class CnHerbsViewModel
//    {
//        public Models.CHIS_DoctorAdvice MainAdvice { get; set; }
//        public List<Models.CHIS_DoctorAdvice_ZH_Detail> HerbsDetails { get; set; }

//    }
//}
