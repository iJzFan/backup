using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHIS.Models.ViewModel
{


    public class DispensingItem
    {
        /// <summary>
        /// 接诊Id
        /// </summary>
        public long TreatId { get; set; }
        /// <summary>
        /// 患者姓名
        /// </summary>
        public string CustomerName { get; set; }
        public int? CustomerGender { get; set; }
        public DateTime? CustomerBirthday { get; set; } 

        public string DoctorName { get; set; }

        public DateTime RegisterDate { get; set; }
        public DateTime TreatTime { get; set; }

        /// <summary>
        /// 发送状态
        /// </summary>
        public int DispensingStatus { get; set; }

        public int? RegistOpId { get; set; }      
    }

    public class DispensingItemViewModel
    {
        /// <summary>
        /// 接诊Id
        /// </summary>
        public long TreatId { get; set; }
        /// <summary>
        /// 患者姓名
        /// </summary>
        public string CustomerName { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string CustomerMobile { get; set; }

        public string DoctorName { get; set; }
       
        public DateTime RegisterDate { get; set; }
        public DateTime TreatTime { get; set; }

        /// <summary>
        /// 发送状态
        /// </summary>
        public int DispensingStatus { get; set; }

        public int? RegistOpId { get; set; }
        /// <summary>
        /// 发送状态名称 只读
        /// </summary>
        //public string DispensingStatusName
        //{
        //    get
        //    {
        //        switch (DispensingStatus)
        //        {
        //            case 0:return "未发";
        //            case 1:return "已发";
        //            default:return "其他";
        //        }
        //    }
        //}

    }

    public class DispensingDetailViewModel
    {

        /// <summary>
        ///  接诊
        /// </summary>
        public vwCHIS_DoctorTreat Treat { get; set; }

        /// <summary>
        /// 患者
        /// </summary>
        public vwCHIS_Code_Customer Customer { get; set; }

        /// <summary>
        /// 选择发送地址
        /// </summary>
        public vwCHIS_Code_Customer_AddressInfos SelectedAddress { get; set; }


        /// <summary>
        /// 总览的数据
        /// </summary>
        public DispensingDetailSumary DispensingDetailSumary { get; set; }

        /// <summary>
        /// 发药工具
        /// </summary>
        public DispensingUtils DispensingUtils { get; set; }

        /// <summary>
        /// 药房列表
        /// </summary>
        public Dictionary<DrugStoreItem,DispensingDetailOfStoreViewModel> DrugStoreDetails { get; set; }

    }
    public class DispensingDetailSumary
    {
        /// <summary>
        /// 总览成药处方
        /// </summary>
        public IEnumerable<CHIS_DoctorAdvice_Formed> FormedPrescription
        {
            get;set;
        }
        /// <summary>
        /// 总览中草药处方
        /// </summary>
        public IEnumerable<CHIS_DoctorAdvice_Herbs> HerbPrescription
        {
            get; set;
        }

        /// <summary>
        /// 成药处方是否需要发药
        /// </summary>
        /// <param name="preNo">处方号</param>
        /// <returns></returns>
        public bool IsNeedFormedDispense(Guid preNo)
        {
            return Formed.Any(m => m.PrescriptionNo == preNo && m.ChargeStatus == ChargeStatus.Payed && m.DispensingStatus == (int)DispensingStatus.NeedSend);
        }
        /// <summary>
        /// 中药处方是否需要发药
        /// </summary>
        /// <param name="preNo">处方号</param>
        /// <returns></returns>
        public bool IsNeedHerbDispense(Guid preNo)
        {
            return GetHerbDetailByPreNo(preNo).Any(m =>  m.ChargeStatus == ChargeStatus.Payed && m.DispensingStatus == (int)DispensingStatus.NeedSend);
        }


        /// <summary>
        /// 获取成药处方详情
        /// </summary>
        /// <param name="preNo"></param>
        /// <returns></returns>
        public IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail> GetFormedDetailByPreNo(Guid preNo)
        {
            return Formed.Where(m => m.PrescriptionNo == preNo);
        }

        /// <summary>
        /// 获取成药处方详情
        /// </summary>
        /// <param name="preNo"></param>
        /// <returns></returns>
        public IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail> GetHerbDetailByPreNo(Guid preNo)
        {
            var key = Herb.Keys.SingleOrDefault(m => m.PrescriptionNo == preNo);
            return Herb[key];    
        }

        /// <summary>
        /// 总览成药
        /// </summary>
        public IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail> Formed { get; set; }

        /// <summary>
        /// 总览中药
        /// </summary>
        public Dictionary<vwCHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>> Herb { get; set; }

    }

    /// <summary>
    /// 每个药店的详细发药内容
    /// </summary>
    public class DispensingDetailOfStoreViewModel
    {
        /// <summary>
        /// 本地工作站
        /// </summary>
        public vwCHIS_Code_WorkStation Station { get; set; }

        /// <summary>
        /// 三方供应商
        /// </summary>
        public CHIS_Code_Supplier WebSupplier { get; set; }
        /// <summary>
        /// 总览成药
        /// </summary>
        public IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail> Formed { get; set; }
        

        /// <summary>
        /// 总览中药
        /// </summary>
        public Dictionary<vwCHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>> Herb { get; set; }
    }
 

    /// <summary>
    /// 发药工具
    /// </summary>
    public class DispensingUtils
    {
        /// <summary>
        /// 成药清单
        /// </summary>
        public IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail> formeds = null;
        /// <summary>
        /// 中药主表
        /// </summary>
        public IEnumerable<vwCHIS_DoctorAdvice_Herbs> herb = null;
        /// <summary>
        /// 中药明细
        /// </summary>
        public IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail> herbs = null;
        /// <summary>
        /// 中药按包组织数据
        /// </summary>
        public Dictionary<vwCHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>> sumary_herb = null;
        public IEnumerable<CHIS_DoctorAdvice_Formed> formedPre = null;
        public IEnumerable<CHIS_DoctorAdvice_Herbs> herbPre=null;
    }



    /// <summary>
    /// 药店清单
    /// </summary>
    public class DrugStoreItem
    {
        public int DrupSourceFrom { get; set; }
        public CHIS_Code_Supplier Supplier { get; set; }
    }
    /// <summary>
    /// 网络发药详情
    /// </summary>
    
    public class PostOrderDetail
    {
        public vwCHIS_Shipping_NetOrder_Formed_Detail netFormedDetail { get; set; }

    };

}
