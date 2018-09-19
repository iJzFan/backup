using Ass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHIS.Models.ViewModel
{


    public class ChargeMainViewModel
    {
        /// <summary>
        /// 待缴费清单
        /// </summary>
        public Ass.Mvc.PageListInfo<ChargeCustomerItem> NeedPayList { get; set; }

        /// <summary>
        /// 已缴费清单
        /// </summary>
        public Ass.Mvc.PageListInfo<PayedItem> PayedList { get; set; }


        /// <summary>
        /// 默认缴费首页信息
        /// </summary>
        public PayIndexModel PayIndexModel { get; set; }
    }

    /// <summary>
    /// 需要缴费信息
    /// </summary>
    public class NeedPayModel
    {
        /// <summary>
        /// 接诊Id
        /// </summary>
        public long TreatId { get { return TreatInfo.TreatId; } }
        /// <summary>
        /// 接诊信息
        /// </summary>
        public vwCHIS_DoctorTreat TreatInfo { get; set; }
        /// <summary>
        /// 病人信息
        /// </summary>
        public vwCHIS_Code_Customer Customer { get; set; }

        /// <summary>
        /// 接诊医生信息
        /// </summary>
        public vwCHIS_Code_Doctor TreatDoctor { get; set; }

        /// <summary>
        /// 默认地址信息
        /// </summary>
        public vwCHIS_Code_Customer_AddressInfos SelectAddress { get; set; }


        /// <summary>
        /// 成药处方
        /// </summary>
        public Dictionary<CHIS_DoctorAdvice_Formed, IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail>> FormedPrescriptions { get; set; }

        /// <summary>
        /// 中草药处方
        /// </summary>
        public Dictionary<CHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>> HerbPrescriptions { get; set; }

        /// <summary>
        /// 附加费用
        /// </summary>
        public IEnumerable<vwCHIS_Doctor_ExtraFee> ExtraFees { get; set; }


        /// <summary>
        /// 是否需要邮寄
        /// </summary>
        public bool IsNeedMail
        {
            get
            {
                var rlt = FormedPrescriptions.Values.Any(m => m.Any(t => t.SourceFrom == 1)) ||
                    HerbPrescriptions.Values.Any(m => m.Any(t => t.SourceFrom == 1));
                return rlt;
            }
        }
    }



    /// <summary>
    /// 已经缴费信息
    /// </summary>
    public class PayedModel
    {
        public vwCHIS_Charge_Pay Pay { get; set; }

        /// <summary>
        /// 接诊Id
        /// </summary>
        public long TreatId { get { return TreatInfo.TreatId; } }
        /// <summary>
        /// 接诊信息
        /// </summary>
        public vwCHIS_DoctorTreat TreatInfo { get; set; }
        /// <summary>
        /// 病人信息
        /// </summary>
        public vwCHIS_Code_Customer Customer { get; set; }

        /// <summary>
        /// 接诊医生信息
        /// </summary>
        public vwCHIS_Code_Doctor TreatDoctor { get; set; }

        /// <summary>
        /// 默认地址信息
        /// </summary>
        public vwCHIS_Code_Customer_AddressInfos SelectAddress { get; set; }


        /// <summary>
        /// 支付的附加费用
        /// </summary>
        public IEnumerable<vwCHIS_Charge_Pay_Detail_ExtraFee> ExtraFees { get; set; }

        /// <summary>
        /// 中草药处方
        /// </summary>
        public Dictionary<CHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_Charge_Pay_Detail_Herb>> HerbPrescriptions { get; set; }

        /// <summary>
        /// 成药处方
        /// </summary>
        public Dictionary<CHIS_DoctorAdvice_Formed, IEnumerable<vwCHIS_Charge_Pay_Detail_Formed>> FormedPrescriptions { get; set; }

    }



    /// <summary>
    /// 默认缴费首页信息
    /// </summary>
    public class PayIndexModel
    {

    }

    public class PayMonitorModel
    {
        public CHIS_Code_WorkStation Station { get; set; }
        public vwCHIS_Code_Doctor Doctor { get; set; }
    }

    public class ChargeCustomerItem
    {
        public long TreatId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPic { get; set; }

        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? TreatTime { get; set; }
        public string TreatStationName { get; set; }
        public decimal TreatAmount { get; set; }
        public decimal NeedPayAmount { get; set; }


    }

    /// <summary>
    /// 已经支付的项目
    /// </summary>
    public class PayedItem
    {
        public string PayOrderId { get; set; }
        public string CustomerName { get; set; }

        public decimal TotalAmount { get; set; }

        public string FeeTypeCodeName { get; set; }

        public string PayRemark { get; set; }
        public long TreatId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerPic { get; set; }

        public int? Gender { get; set; }
        public DateTime? TreatTime { get; set; }

        public DateTime PayedTime { get; set; }

        public long PayId { get; set; }

        public string FeeTypeCode { get; set; }


    }


    /// <summary>
    /// 支付实体
    /// </summary>
    public class PaymentViewModel
    {
        public long TreatId { get; set; }
        /// <summary>
        /// 微信支付的订单号
        /// </summary>
        public string PayOrderId { get; set; }
        /// <summary>
        /// 是否允许现金支付
        /// </summary>
        public bool IsAllowedCashPay { get; set; } = false;

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 微信二维码
        /// </summary>
        public string WxQrcodeString { get; set; }
        /// <summary>
        /// 支付宝二维码
        /// </summary>
        public string AliQrcodeString { get; set; }
    }



    public class PayDetail
    {
        public vwCHIS_Charge_Pay Pay { get; set; }
        public IEnumerable<vwCHIS_Charge_Pay_Detail_Formed> Formeds { get; set; }
        public IEnumerable<vwCHIS_Charge_Pay_Detail_Herb> Herbs { get; set; }
        public IEnumerable<vwCHIS_Charge_Pay_Detail_ExtraFee> Extras { get; set; }
    }


    /// <summary>
    /// 微信明细条
    /// </summary>
    public class WXPayDetailItem
    {
        /// <summary>
        /// 明细内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 小计价
        /// </summary>
        public decimal Amount { get; set; }
    }


    public class PaySuccessCheck
    {
        /// <summary>
        /// 是否有支付Id
        /// </summary>
        public bool HasPayOrderId
        {
            get
            {
                return PayOrderId.IsNotEmpty();
            }
        }

        /// <summary>
        /// 支付Id
        /// </summary>
        public string PayOrderId { get; set; }
        /// <summary>
        /// 支付说明
        /// </summary>
        public string payRemark { get; set; }
        /// <summary>
        /// 是否是现金
        /// </summary>
        public bool IsCash { get; set; }

        /// <summary>
        /// 收入金额 分
        /// </summary>
        public int GetCashAmount { get; set; }
        /// <summary>
        /// 找零金额 分
        /// </summary>
        public int ReturnCashAmount { get; set; }

        /// <summary>
        /// 支付金额 分
        /// </summary>
        public int PayAmount { get; set; }
    }

    /// <summary>
    /// 现金支付
    /// </summary>
    public class CashPay 
    { 
        /// <summary>
        /// 支付Id
        /// </summary>
        public string PayOrderId { get; set; }
        /// <summary>
        /// 支付说明
        /// </summary>
        public string payRemark { get; set; }
 

        /// <summary>
        /// 收入金额 分
        /// </summary>
        public int GetCashAmount { get; set; }
        /// <summary>
        /// 找零金额 分
        /// </summary>
        public int ReturnCashAmount { get; set; }

        /// <summary>
        /// 支付金额 分
        /// </summary>
        public int PayAmount { get; set; }
    }

    public class PayRedirectInfo
    {
        public decimal payAmount { get; set; }
        public string payOrderId { get; set; }
        public string payWxQrUrl { get; set; }
        /// <summary>
        /// 微信H5的调用
        /// </summary>
        public string payWxH5CreateUrl { get; set; }
        public string payAliQrUrl { get; set; }
        public string payType { get; set; }
        public string ClientIP { get; set; }
    }
    public class PayPreInfo
    {
        public CHIS.Models.CHIS_Charge_PayPre CHIS_Charge_PayPre { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
    }

    public class WxPayRlt
    {
        public bool IsQrRlt { get; set; }
        public string QrMsg { get; set; }
        public bool IsPubRlt { get; set; }
        public string PubMsg { get; set; }
        public bool IsH5Rlt { get; set; }
        public string H5Msg { get; set; }
        public bool IsPayed { get { return IsQrRlt || IsPubRlt || IsH5Rlt; } }
        public string feeType
        {
            get
            {
                if (IsQrRlt) return FeeTypes.WeChat_QR;
                if (IsPubRlt) return FeeTypes.WeChat_Pub;
                if (IsH5Rlt) return FeeTypes.WeChat_H5;
                return string.Empty;
            }
        }
        public bool IsNotPay
        {
            get
            {
                return (string.IsNullOrEmpty(QrMsg) || QrMsg.IndexOf("NOTPAY") >= 0 || QrMsg.IndexOf("ORDERNOTEXIST") >= 0) &&
                       (string.IsNullOrEmpty(PubMsg) || PubMsg.IndexOf("NOTPAY") >= 0 || PubMsg.IndexOf("ORDERNOTEXIST") >= 0) &&
                       (string.IsNullOrEmpty(H5Msg) || H5Msg.IndexOf("NOTPAY") >= 0 || H5Msg.IndexOf("ORDERNOTEXIST") >= 0);
            }
        }

        public override string ToString()
        {
            return $"WXQR({IsQrRlt}){QrMsg};WXPUB({IsPubRlt}){PubMsg};WXH5({IsH5Rlt}){H5Msg}";
        }
    }

    public class AliPayRlt
    {
        public bool IsPayed { get; set; }
        public string PayMsg { get; set; }
        public string feeType
        {
            get
            {
                if (IsPayed) return FeeTypes.AliPay_QR;
                return "";
            }
        }

        public bool IsNotPay
        {
            get
            {
                return (PayMsg == "Business Failed");
            }
        }
        public override string ToString()
        {
            return $"ALIQR({IsPayed}){PayMsg}";
        }
    }

}
