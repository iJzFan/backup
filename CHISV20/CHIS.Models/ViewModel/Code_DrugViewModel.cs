using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.ViewModels
{
    public class Code_DrugViewModel
    {
        /// <summary>
        /// 药品通用属性
        /// </summary>
        public Models.CHIS_Code_Drug_Main CHIS_Code_Drug_Main { get; set; }
        /// <summary>
        /// 药品工作站属性
        /// </summary>
        public Models.CHIS_Code_Drug_Storage CHIS_Code_Drug_Storage { get; set; } = new CHIS_Code_Drug_Storage();
        /// <summary>
        /// 药品药房属性
        /// </summary>
        public Models.CHIS_Code_Drug_Outpatient CHIS_Code_Drug_Outpatient { get; set; }

        /// <summary>
        /// 附加的药品审核信息
        /// </summary>
        public CHIS_Code_Drug_Main_Apply DrugApply { get; set; }
    }

    //public class CnHerbsViewModel
    //{
    //    /// <summary>
    //    /// 药品基本信息
    //    /// </summary>
    //    public Models.vwCHIS_Code_Drug_Main DrugMain { get; set; }

    //    /// <summary>
    //    /// 药品库存信息
    //    /// </summary>
    //    public Models.vwCHIS_DrugStock_Monitor StockInfo { get; set; }



    //}


    /// <summary>
    /// 开药主表从表数据
    /// </summary>
    /// <typeparam name="TMain"></typeparam>
    /// <typeparam name="TDetail"></typeparam>
    public class DrugMainViewModel<TMain, TDetail>
    {

        /// <summary>
        /// 主表
        /// </summary>
        public TMain Main { get; set; }
        /// <summary>
        /// 详情信息
        /// </summary>
        public IEnumerable<TDetail> Details { get; set; }



        /// <summary>
        /// 接诊信息
        /// </summary>
        public DataModel.TreatSummary TreatSummary { get; set; }

        /// <summary>
        /// 是否需要处方签名
        /// </summary>
        public bool IsNeedRxSign
        {
            get {
                if (TreatSummary == null) return false;
                return TreatSummary.IsNeedRxSign; }
        }
        /// <summary>
        /// 是否已经处方签名
        /// </summary>
        public virtual bool IsRxSigned
        {
            get { return false; }
        }
    }


    public class CnHerbsMainViewModel:DrugMainViewModel<Models.CHIS_DoctorAdvice_Herbs, Models.vwCHIS_DoctorAdvice_Herbs_Detail>
    { 
        /// <summary>
        /// 是否已经处方签名
        /// </summary>
        public override bool IsRxSigned
        {
            get { return !string.IsNullOrWhiteSpace(Main.RxDoctorSignUrl); }
        }

    }

    /// <summary>
    /// 成药前端模型
    /// </summary>
    public class FormedMainViewModel:DrugMainViewModel<Models.CHIS_DoctorAdvice_Formed, Models.vwCHIS_DoctorAdvice_Formed_Detail>
    {     
        /// <summary>
        /// 是否已经处方签名
        /// </summary>
        public override bool IsRxSigned
        {
            get { return !string.IsNullOrWhiteSpace(Main.RxDoctorSignUrl); }
        }

    }


}
