using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ass;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHIS.Models
{
    [ModelMetadataType(typeof(m_DrugMain))]
    public partial class CHIS_Code_Drug_Main
    {

        public CHIS_Code_Drug_Main()
        {

        }
        private class m_DrugMain
        {

            [Required(ErrorMessage = "请输入药品编码")]
            public string DrugCode { get; set; }
            [Required(ErrorMessage = "必须要有药品名称")]
            public string DrugName { get; set; }
            [Required(ErrorMessage = "必须要有型号规格")]
            public string DrugModel { get; set; }
            [Required(ErrorMessage = "必须选择分类")]
            public int? DrugStockTypeId { get; set; }
            [Required(ErrorMessage = "必须选择医药药品或材料的主类别")]
            public string MedialMainKindCode { get; set; }


        }
    }

    [ModelMetadataType(typeof(m_DrugOutpatient))]
    public partial class CHIS_Code_Drug_Outpatient
    {
        /// <summary>
        /// 药品标准规格表达
        /// </summary>
        public string DrugModelStd
        {
            get
            {
                if (DosageUnitId > 0 && UnitBigId > 0 && UnitSmallId > 0)
                    return (DosageContent ?? 0).ToString("#0.###") +
                            CHIS.DictValues.GoodsUnit.Ins().GetName(DosageUnitId ?? 0) +
                            " " + OutpatientConvertRate.ToString("#0.##") +
                            CHIS.DictValues.GoodsUnit.Ins().GetName(UnitSmallId ?? 0) + "/" +
                            CHIS.DictValues.GoodsUnit.Ins().GetName(UnitBigId ?? 0);
                else return "";
            }
        }

        public CHIS_Code_Drug_Outpatient()
        {


        }
        private class m_DrugOutpatient
        {
            [Required(ErrorMessage = "请输入大单位(包装单位)")]
            public int? UnitBigId { get; set; }
            [Required(ErrorMessage = "请输入大小单位转换率")]
            public decimal? OutpatientConvertRate { get; set; }
            [Required(ErrorMessage = "请输入小单位(封装单位)")]
            public int? UnitSmallId { get; set; }
            [Required(ErrorMessage = "请输入封装含量数")]
            public decimal? DosageContent { get; set; }
            [Required(ErrorMessage = "请输入含量单位")]
            public int? DosageUnitId { get; set; }
        }
    }

    [ModelMetadataType(typeof(m_vwDrugMain))]
    public partial class vwCHIS_Code_Drug_Main
    {


        /// <summary>
        /// 药品标准规格表达
        /// </summary>
        public string DrugModelStd
        {
            get
            {
                if (DosageUnitId > 0 && UnitBigId > 0 && UnitSmallId > 0)
                    return (DosageContent ?? 0).ToString("#0.###") +
                            CHIS.DictValues.GoodsUnit.Ins().GetName(DosageUnitId ?? 0) +
                            " " + (OutpatientConvertRate??0).ToString("#0.##") +
                            CHIS.DictValues.GoodsUnit.Ins().GetName(UnitSmallId ?? 0) + "/" +
                            CHIS.DictValues.GoodsUnit.Ins().GetName(UnitBigId ?? 0);
                else return "";
            }
        }
 
        private class m_vwDrugMain
        {

            [Required(ErrorMessage = "请输入药品编码")]
            public string DrugCode { get; set; }
            [Required(ErrorMessage = "必须要有药品名称")]
            public string DrugName { get; set; }
            [Required(ErrorMessage = "必须要有型号规格")]
            public string DrugModel { get; set; }
            [Required(ErrorMessage = "必须选择分类")]
            public int? DrugStockTypeId { get; set; }
            [Required(ErrorMessage = "必须选择医药药品或材料的主类别")]
            public string MedialMainKindCode { get; set; }
            [Required(ErrorMessage = "请输入大单位(包装单位)")]
            public int? UnitBigId { get; set; }
            [Required(ErrorMessage = "请输入大小单位转换率")]
            public decimal? OutpatientConvertRate { get; set; }
            [Required(ErrorMessage = "请输入小单位(封装单位)")]
            public int? UnitSmallId { get; set; }
            [Required(ErrorMessage = "请输入封装含量数")]
            public decimal? DosageContent { get; set; }
            [Required(ErrorMessage = "请输入含量单位")]
            public int? DosageUnitId { get; set; }


        }

        /// <summary>
        /// 药品记录信息得分
        /// </summary>
        [NotMapped]
        public int DrugRecordScore
        {
            get
            {
                //todo 医药品的基础数据记录完善度得分
                return 5;
            }
        }
        /// <summary>
        ///  是否需要改变单位
        /// </summary>
        [NotMapped]
        public bool IsNeedChangeUnit
        {
            get
            {
                return true;
                return this.UnitBigId == this.UnitSmallId || this.UnitSmallId == this.DosageUnitId ||
                    (this.OutpatientConvertRate == 1 && this.UnitSmallId != this.UnitBigId) ||
                    (this.DosageContent == 1 && this.UnitSmallId != this.DosageUnitId);

                if (this.MedialMainKindCode == MPS.MedicalMainKindCode.XY)// 西药
                    return this.UnitBigId == this.UnitSmallId && this.UnitSmallId == this.DosageUnitId;
                if (this.MedialMainKindCode == MPS.MedicalMainKindCode.ZYC)//中成药
                    return this.UnitBigId == this.UnitSmallId && this.UnitSmallId == this.DosageUnitId;
                if (this.MedialMainKindCode == MPS.MedicalMainKindCode.ZYM)//中草药
                    return this.UnitBigId == this.UnitSmallId && this.UnitSmallId == this.DosageUnitId;
                if (this.MedialMainKindCode == MPS.MedicalMainKindCode.XY)
                    return this.UnitBigId == this.UnitSmallId && this.UnitSmallId == this.DosageUnitId;
                if (this.MedialMainKindCode == MPS.MedicalMainKindCode.XY)
                    return this.UnitBigId == this.UnitSmallId && this.UnitSmallId == this.DosageUnitId;
                return false;
            }
        }


    }
    public class Exp_vwCHIS_Code_Drug_Main : vwCHIS_Code_Drug_Main
    {
        /// <summary>
        /// 我的上一次入库价格
        /// 扩展，非原数据库含有。        
        /// </summary>
        public decimal? ExpMyLastIncomeBigPrice { get; set; }
        public decimal? ExpMyLastIncomeSmallPrice { get; set; }
    }


}
