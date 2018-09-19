using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.DataModel
{
    public class DoctorTreatItem : vwCHIS_DoctorTreat
    {
        /// <summary>
        /// 是否需要处方签名
        /// </summary>
        public bool IsNeedRxSign { get { return RxDoctorId > 0; } }
        /// <summary>
        /// 处方签名医生Id
        /// </summary>
        public int? RxDoctorId { get; set; }
        /// <summary>
        /// 处方签名医生
        /// </summary>
        public string RxDoctorName { get; set; }

        /// <summary>
        /// 是否需要处方医生签名 处方医生是否都已经签名
        /// </summary>
        public bool? IsNeedRxDoctorSign { get; set; } = null;
    }


    /// <summary>
    /// 处方信息
    /// </summary>
    public class PrescriptionItem : CHIS_DoctorAdvice_Formed
    {
        public string PresTypeName { get; set; }
        public long RegisterId { get; set; }
        public DateTime? RegisterDate { get; set; }


        public string StationName { get; set; }
        public string DepartmentName { get; set; }

        public string DoctorName { get; set; }
        public DateTime? FirstTreatTime { get; set; }
        public DateTime? TreatTime { get; set; }
        public int TreatStatus { get; set; }

        /// <summary>
        /// 主诊断
        /// </summary>
        public string Diagnosis1 { get; set; }
        public int FstDiagnosis { get; set; }

        public long CustomerId { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerName { get; set; }
        public int? CustomerGender { get; set; }
        public DateTime? CustomerBirthday { get; set; }

        public int? RxDoctorId { get; set; }
        public string RxDoctorName { get; set; }


        /// <summary>
        ///  是否需要处方签名
        /// </summary>
        public bool IsNeedRxSign
        {
            get
            {
                return RxDoctorId.HasValue;
            }
        }

        /// <summary>
        /// 是否处方医生已经签名
        /// </summary>
        public bool? IsRxSigned
        {
            get
            {
                if (!RxDoctorId.HasValue) return null;
                return !string.IsNullOrWhiteSpace(this.RxDoctorSignUrl);
            }
        }
    }


    /// <summary>
    /// 接诊信息
    /// </summary>
    public class TreatSummary
    {
        /// <summary>
        /// 接诊Id
        /// </summary>
        public long TreatId { get; set; }
        /// <summary>
        /// 是否需要处方签名
        /// </summary>
        public bool IsNeedRxSign { get { return RxDoctorId.HasValue; } }
        /// <summary>
        /// 处方签名医生Id
        /// </summary>
        public int? RxDoctorId { get; set; }
        /// <summary>
        /// 处方签名医生姓名
        /// </summary>
        public string RxDoctorName { get; set; }
    }






    #region 医生处置模型

    public class TreatSaveBackData
    {
        public long TreatId { get; set; }
        public long RegistId { get; set; }
        public List<Guid> FormedPrescriptionKeyIds { get; set; }
        public List<Guid> HerbPrescriptionKeyIds { get; set; }
    }

    #region 完整的接诊数据=========================================
    /// <summary>
    /// 接诊全模型数据
    /// </summary>
    public class DataTreat
    {
        /// <summary>
        /// 接诊数据
        /// </summary>
        public CHIS_DoctorTreat DoctorTreatData { get; set; }

        /// <summary>
        ///  成药处方
        /// </summary>
        public IEnumerable<DataFormed> FormedPrescriptions { get; set; }
        /// <summary>
        /// 中药处方
        /// </summary>
        public IEnumerable<DataHerb> HerbPrescriptions { get; set; }

        /// <summary>
        /// 附加费用
        /// </summary>
        public IEnumerable<CHIS_Doctor_ExtraFee> ExtraFees { get; set; }
    }


    /// <summary>
    /// 数据 成药
    /// </summary>
    public class DataFormed
    {
        /// <summary>
        /// 主表
        /// </summary>
        public CHIS_DoctorAdvice_Formed Main { get; set; }
        /// <summary>
        /// 详细
        /// </summary>
        public IEnumerable<CHIS_DoctorAdvice_Formed_Detail> Detail { get; set; }
    }
    /// <summary>
    /// 数据 中药
    /// </summary>
    public class DataHerb
    {   /// <summary>
        /// 主表
        /// </summary>
        public CHIS_DoctorAdvice_Herbs Main { get; set; }
        /// <summary>
        /// 详细
        /// </summary>
        public IEnumerable<CHIS_DoctorAdvice_Herbs_Detail> Detail { get; set; }
    }

    #endregion


    #region 简版接诊模型========================================
    public class DataTreatForDrugStore
    {
        /// <summary>
        /// 接诊数据
        /// </summary>
        public DoctorTreatV0Input DoctorTreatData { get; set; }
        /// <summary>
        /// 处方号，如果没带，则删除其他接诊后新增
        /// </summary>
        public Guid? FormedPrescriptionNo { get; set; }

        /// <summary>
        /// 成药处方内容
        /// </summary>
        public IEnumerable<InputFormedAdviceItemV0> FormedDrugAdvices { get; set; }

        public long CustomerMailAddressId { get; set; }
    }

    /// <summary>
    /// 接诊数据模型 简版
    /// </summary>
    public class DataTreatV0Input
    {
        /// <summary>
        /// 接诊数据
        /// </summary>
        public DoctorTreatV0Input DoctorTreatData { get; set; }

        /// <summary>
        ///  成药处方
        /// </summary>
        public IEnumerable<DataFormedV0Input> FormedPrescriptions { get; set; }
        /// <summary>
        /// 中药处方
        /// </summary>
        public IEnumerable<DataHerbV0Input> HerbPrescriptions { get; set; }


    }

    public class DoctorTreatV0Input
    {
        public long TreatId { get; set; }
        /// <summary>
        /// 主诉
        /// </summary>
        public string Complain { get; set; }
        /// <summary>
        /// 现病史
        /// </summary>
        public string PresentIllness { get; set; }
        /// <summary>
        /// 检查
        /// </summary>
        public string Examination { get; set; }
        /// <summary>
        /// 主诊断Id
        /// </summary>
        public int Diagnosis1Id { get; set; }
        /// <summary>
        /// 约号Id
        /// </summary>
        public int RegisterId { get; set; }
        /// <summary>
        /// 患者Id
        /// </summary>
        public int CustomerId { get; set; }
        /// <summary>
        /// 医生Id
        /// </summary>
        public int DoctorId { get; set; }
        /// <summary>
        /// 接诊工作站
        /// </summary>
        public int StationId { get; set; }
    }

    #region Formed V0
    public class DataFormedV0Input
    {
        /// <summary>
        /// 主表
        /// </summary>
        public FormedMainV0Input Main { get; set; }
        /// <summary>
        /// 详细
        /// </summary>
        public IEnumerable<InputFormedAdviceItemV0> Detail { get; set; }
    }

    public class FormedMainV0Input
    {
        public Guid PrescriptionNo { get; set; }
    }
    /// <summary>
    /// 输入的成药一条药记录
    /// </summary>
    public class InputFormedAdviceItemV0
    {       /// <summary>
            /// 记录Id,新增则为0，否则大于0
            /// </summary>
        public long AdviceFormedId { get; set; }

        /// <summary>
        /// 药品Id
        /// </summary>
        [Required]
        public int DrugId { get; set; }


        /// <summary> 
        /// 组号
        /// </summary>		

        public short? GroupNum { get; set; }

        /// <summary> 
        /// 数量
        /// </summary>		
        [Required]
        public int Qty { get; set; }

        /// <summary> 
        /// 数量单位
        /// </summary>		
        [Required]
        public int UnitId { get; set; }
        /// <summary>
        /// 用药说明
        /// </summary>
        public string GivenRemark { get; set; }
        /// <summary> 
        /// 药品库来源
        /// </summary>		

        public string StockFromId { get; set; }

    }
    #endregion


    #region Herb V0
    public class DataHerbV0Input
    {
        /// <summary>
        /// 主表
        /// </summary>
        public HerbMainV0Input Main { get; set; }
        /// <summary>
        /// 详细
        /// </summary>
        public IEnumerable<InputHerbAdviceItemV0> Detail { get; set; }
    }
    public class HerbMainV0Input
    {
        /// <summary>
        /// 处方单号
        /// </summary>
        public Guid PrescriptionNo { get; set; }
        /// <summary>
        /// 中药方 名
        /// </summary>
        public string HerbTitle { get; set; }

        /// <summary> 
        /// 用法Id
        /// </summary>		

        public int? GivenTakeTypeId { get; set; }

        /// <summary> 
        /// 用法手写
        /// </summary>		

        public string GivenRemark { get; set; }

        /// <summary> 
        /// 医嘱
        /// </summary>		

        public string DoctorAdvice { get; set; }



        /// <summary> 
        /// 数量 单位 剂
        /// </summary>		

        public int Qty { get; set; }



    }
    public class InputHerbAdviceItemV0
    {
        /// <summary> 
        /// Id
        /// </summary>		


        public long HerbAdviceId { get; set; }


        public int DrugId { get; set; }



        /// <summary> 
        /// 
        /// </summary>		

        public int Qty { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int UnitId { get; set; }



        /// <summary> 
        /// 单味药用法
        /// </summary>		

        public int? HerbUseTypeId { get; set; }



        /// <summary> 
        /// 库存获取Id
        /// </summary>		

        public string StockFromId { get; set; }
    }
    #endregion


    #endregion











    #endregion
}
