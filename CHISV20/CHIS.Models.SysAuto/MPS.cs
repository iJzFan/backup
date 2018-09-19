using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS
{

    /// <summary>
    /// 数据库内的数据常量
    /// </summary>
    public class MPS
    {
        public static class MedicalMainKindCode
        {
            /// <summary>
            /// 西药
            /// </summary>
            public readonly static string XY = "XY";

            /// <summary>
            /// 中草药
            /// </summary>
            public readonly static string  ZYM = "ZYM";//中药药材类型ID
            /// <summary>
            /// 中成药
            /// </summary>
            public readonly static string ZYC = "ZYC";

            /// <summary>
            /// 综合类
            /// </summary>
            public readonly static string ZHL = "ZHL";

            /// <summary>
            /// 处置类
            /// </summary>
            public readonly static string CZ = "CZ";
            /// <summary>
            /// 材料类
            /// </summary>
            public readonly static string MT = "MT";

        }

        #region 特殊接诊类别
        /// <summary>
        /// 心理学
        /// </summary>
        public readonly static string SpetialDepartType_PSYCH = "PSYCH";
#endregion

        //数据字典特别数据



        public readonly static int DictId_Complain = 50;//主诉

        public readonly static int SystemMachineId = 26;//系统机器用户Id
        public readonly static int SystemMachineEmployeeId = 22;//系统机器虚拟雇员Id


        public readonly static int InsuranceNo = 12967;//社保卡号Id


        /// <summary>
        /// 身份证Id
        /// </summary>
        public readonly static int IDCardId = 12943;
        /// <summary>
        /// 网络平台工作总站
        /// </summary>
        public readonly static int Web_StationID = 4;
        /// <summary>
        /// 网上工作站Id
        /// </summary>
        public readonly static int NetStationId = 9;
        /// <summary>
        /// 测试接诊工作站
        /// </summary>
        public readonly static int RdTestTreatStationId = 10;

        /// <summary>
        /// 网上工作站 测试
        /// </summary>
        public readonly static int TestNetStationId = 6;

        #region Role 接诊站医生
        /// <summary>
        /// 全责医生
        /// </summary>
        public readonly static string RoleTreatAllDoctor = "treat_doctor_all";
        public readonly static int RoleTreatAllDoctorId = 10;
        /// <summary>
        /// 一般医生
        /// </summary>
        public readonly static string RoleTreatDoctor = "treat_doctor";
        public readonly static int RoleTreatDoctorId = 12;


        public readonly static string RoleTreatNurse = "treat_nurse";
        public readonly static int RoleTreatNurseId = 9;

        public readonly static string RoleTreatNurseAdv = "treat_nurse_adv";
        public readonly static int RoleTreatNurseAdvId = 4;

        #endregion


        /// <summary>
        /// 单位 (克)
        /// </summary>
        public readonly static int Unit_g = 42;


        public readonly static string Drug_含量 = "DOSAGE";
        public readonly static string Drug_封装 = "FORMED";
        public readonly static string Drug_包装 = "PACKAGE";

        public readonly static int Fee_诊金 = 12970;
        public readonly static int Fee_快递 = 12969;

        /// <summary>
        /// 剑客物流中心
        /// </summary>
        public readonly static int CenterAreaId_JK = 1966;

        /// <summary>
        /// 供应商Id 健客
        /// </summary>
        public readonly static int SupplierId_JK = 3;
        public readonly static int DefaultSupplierId = 3;



        public static string ts_DispensingStatus(int dispensingStatus)
        {
            switch (dispensingStatus)
            {
                case 0:return "待发";
                case 1: return "已发";
                case 2: return "待退";
                case 3: return "已退";
                default:return "未知";
            }
        }

    }

    public class ExceptionCodes
    {
        /// <summary>
        /// 药品没有找到
        /// </summary>
        public readonly static string DrugNotFound = "DRUG_NOT_FOUND";
    }
}
