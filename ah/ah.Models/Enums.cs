using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ass;

namespace ah
{

    #region SingleDict

    public class DbData
    {
        //数据字典特别数据
        public readonly static int Web_StationID = 4;//网络平台工作总站
        public readonly static int Herbs_DRUGID = 1;//这是中药自开药
        public readonly static string Herbs_MedicalMainKindCode_Fang = "ZYF";//中药药材类型ID
        public readonly static string Herbs_MedicalMainKindCode_Cheng = "ZYC";//这是中成药ID
        public readonly static int DictId_Complain = 50;//主诉

        public readonly static int SystemMachineId = 26;//系统机器用户Id
        public readonly static int SystemMachineEmployeeId = 22;//系统机器虚拟雇员Id

        public readonly static int IDCardId = 12943;//身份证Id
        public readonly static int InsuranceNo = 12967;//社保卡号Id
    }


    #endregion

    #region 患者人员信息
    /// <summary>
    /// 信息来源
    /// </summary>
    public enum sysSources
    {
        CHIS系统,
        微信,
        一体机,
        Excel导入,
        预约系统
    }

    #endregion

    public class FeeTypes
    {
        public static string Cash = "CASH";
        public static string WeChat_QR = "WXQR";
        public static string WeChat_Pub = "WXPUB";
        public static string AliPay_QR = "ALIQR";

        public static bool IsValidFeeType(string type)
        {
            if (type == Cash || type == WeChat_QR || type == WeChat_Pub || type == AliPay_QR) return true;
            return false;
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
        V20Web= 12968
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
        Local=0,
        WebNet=1,
        ThreePartStore=2
    }


    public enum DepartSelectTypes
    {
        TreatmentDepartments,
        NotTreatmentDepartments,
        All
    }

    public enum ExtraFeeTypes
    {
        TransFee= 12969,
        DoctorTreatFee= 12970,
    }

    public enum RecentType
    {
        All,
        Station,
        Doctor
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
        Unprocessed,
        [EnumName("未领取")]
        NotGet,
    }
}
