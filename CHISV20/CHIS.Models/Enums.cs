using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ass;

namespace CHIS
{

    #region 患者人员信息
    /// <summary>
    /// 信息来源
    /// </summary>
    public enum sysSources
    {
        CHIS系统,
        CHIS约号快增,
        微信,
        一体机,
        Excel导入,
        预约系统,
        医生注册,
        处方药记录快录
    }

    #endregion

    public class FeeTypes
    {
        public static string Cash = "CASH";
        public static string WeChat_QR = "WXQR";
        public static string WeChat_Pub = "WXPUB";
        public static string WeChat_H5 = "WXH5";
        public static string AliPay_QR = "ALIQR";


        public static bool IsValidFeeType(string type)
        {
            if (type == Cash || type == WeChat_QR || type == WeChat_Pub || type == WeChat_H5 || type == AliPay_QR) return true;
            return false;
        }

        /// <summary>
        /// 转换成为名称
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ToName(string val)
        {
            if (val == FeeTypes.Cash) return "现金";
            else if (val == FeeTypes.WeChat_QR) return "微信二维码";
            else if (val == FeeTypes.WeChat_Pub) return "微信公众号";
            else if (val == FeeTypes.WeChat_H5) return "微信H5支付";
            else if (val == FeeTypes.AliPay_QR) return "支付宝二维码";
            else return "";
        }
        public static string ToName(object val)
        {
            return ToName(Ass.P.PStr(val));
        }
    }

    public class ChargeStatus
    {
        public static readonly int NeedPay = 0;
        public static readonly int Payed = 1;
        public static readonly int NeedRefund = 2;
        public static readonly int Refund = 3;
    }

    public class RefundStatus
    {
        /// <summary>
        /// 建立
        /// </summary>
        public static readonly int Create = 0;
        /// <summary>
        /// 申请成功
        /// </summary>
        public static readonly int ApplySuccess = 1;
        /// <summary>
        /// 申请失败
        /// </summary>
        public static readonly int ApplyFailed = 2;
        /// <summary>
        /// 退款成功
        /// </summary>
        public static readonly int RefundSuccess = 3;
        /// <summary>
        /// 退款失败
        /// </summary>
        public static readonly int RefundFailed = 4;
    }


    public class PayExceptionType
    {
        /// <summary>
        /// 支付提交给三方，并支付成功
        /// </summary>
        public static readonly int PayTo3PartSuccess = 101;

        /// <summary>
        /// 支付已经完成并成功
        /// </summary>
        public static readonly int PayedFinishAndSuccess = 1;
    }

    /// <summary>
    /// 身份证件类别
    /// </summary>
    public enum CertificateTypes
    {
        IdCard = 12943,
        HongKongMacauLaissezPasser = 12944,
        MTP = 12945,//台胞证 Mainland travel permit for Taiwan residents
        Passport = 12946,//护照
        DrivingLicense = 12947
    }

    /// <summary>
    /// 挂号来源
    /// </summary>
    public enum RegistFrom
    {
        WeChartPub = 12953,
        App = 12954,
        CHIS = 12955,
        OneMachine = 12956,
        V20Web = 12968
    }

    /// <summary>
    /// 转诊状态
    /// </summary>
    public enum TransferTreatStatus
    {
        [EnumName("未转诊")]
        UnTransfer = 0,
        [EnumName("转诊已发")]
        PostTransfer = 1,
        [EnumName("转诊已接")]
        GetTransfer = 2
    }


    public enum RegisterTreatType
    {
        Normal = 12960,
        TransferTreat = 12961
    }

    /// <summary>
    /// 药品来源
    /// </summary>
    public enum DrugSourceFrom
    {
        Local = 0,
        WebNet = 1,
        ThreePartStore = 2
    }

    /// <summary>
    /// 发药状态
    /// </summary>
    public enum DispensingStatus
    {
        /// <summary>
        /// 待发
        /// </summary>
        NeedSend = 0,
        /// <summary>
        /// 已发送
        /// </summary>
        Sended = 1,
        /// <summary>
        /// 待退
        /// </summary>
        NeedReturn = 2,
        /// <summary>
        /// 已退
        /// </summary>
        IsReturned = 3
    }

    public enum DepartSelectTypes
    {
        TreatmentDepartments,
        NotTreatmentDepartments,
        All
    }

    public enum ExtraFeeTypes
    {
        TransFee = 12969,
        DoctorTreatFee = 12970,
    }

    /// <summary>
    /// 订单发送状态
    /// </summary>
    public enum SendState
    {
        SendSucces = 1,
        SendFail = 2
    }


    public enum OrderState
    {
        [EnumName("未使用")]
        NotUse,
        [EnumName("已使用")]
        Used,
        [EnumName("已过期")]
        Expired,
        [EnumName("已发货")]
        Shipped,
        [EnumName("待发货")]
        Unprocessed
    }
}
