/*******************************************************************************
 * Copyright © 2017 ah 版权所有
 * Author: Rex
 * Description: 操作Model 本类存放简单数据类型
*********************************************************************************/
using CHIS.Models;
using System;
using Ass;

namespace CHIS.Models
{
    /// <summary>
    /// 用户登录基本信息
    /// </summary>
    public class UserSelf
    {

        #region 登录数据
        /// <summary>
        /// 登录数据库Id
        /// </summary>
        public long LoginId { get; set; }
        #endregion


        #region 人员数据
        /// <summary>
        /// Customer的Id号
        /// </summary>
        public int OpId { get; set; }
        public int CustomerId { get { return OpId; } }
        /// <summary>
        /// 医生的Id号
        /// </summary>
        public int DoctorId { get; set; }
        /// <summary>
        /// 操作者姓名 医生名=用户名=OpMan
        /// </summary>
        public string OpMan { get; set; }
        /// <summary>
        /// 用户名=医生名=OpMan
        /// </summary>
        public string CustomerName { get { return OpMan; } }
        /// <summary>
        /// 医生名=用户名=OpMan
        /// </summary>
        public string DoctorName { get { return OpMan; } }
        /// <summary>
        /// 操作者全信息 只读
        /// </summary>
        public string OpManFullMsg { get { return string.Format("{0}{1}", OpMan, DoctorName.IsEmpty() ? "" : "(" + DoctorName + ")"); } }

        /// <summary>
        /// 性别
        /// </summary>
        public int? Gender { get; set; }
        public string GenderText { get { return Gender?.ToGenderString(); } }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime Birthday { get; set; }
        public string AgeString { get { return Birthday.ToAgeString(); } }

        #endregion


        #region 医生数据扩展
        /// <summary>
        /// 职务
        /// </summary>
        public string PostTitleName { get; set; }
        /// <summary>
        /// 默认头像
        /// </summary>
        public string PhotoUrlDef { get; set; }

        #endregion

        #region 工作站数据


        /// <summary>
        /// 工作站Id
        /// </summary>
        public int StationId { get; set; }

        /// <summary>
        /// 使用药房的工作站Id
        /// </summary>
        public int DrugStoreStationId { get; set; }

        /// <summary>
        /// 我的药房是否是自身药房
        /// </summary>
        public bool IsSelfDrugStore
        {
            get { return StationId == DrugStoreStationId && StationId > 0; }
        }
        /// <summary>
        /// 工作站名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 工作站类别Id
        /// </summary>
        public int StationTypeId { get; set; }
        /// <summary>
        /// 我的子工作站
        /// </summary>
        public int[] MySonStations { get; set; }
        /// <summary>
        /// 是否有子工作站
        /// </summary>
        public bool HasChildStation { get { return MySonStations != null && MySonStations.Length > 0; } }
        /// <summary>
        /// 是否可接诊
        /// </summary>
        public bool IsCanTreat { get; set; }
        /// <summary>
        /// 是否是管理单位
        /// </summary>
        public bool IsManageUnit { get; set; }

        /// <summary>
        /// 我允许业务的工作站
        /// </summary>
        public int[] MyAllowStationIds { get; set; }

        #endregion

        #region 部门信息
        public int? SelectedDepartmentId { get; set; }
        /// <summary>
        /// 选择的部门
        /// </summary>
        public string SelectedDepartmentName { get; set; }

        #endregion



        #region 权限数据
        /// <summary>
        /// 允许的角色Id
        /// </summary>
        public int[] MyRoleIds { get; set; }
        public string[] MyRoleNames { get; set; }


        #endregion
        #region 辅助登录信息
        /// <summary>
        /// 辅助登录的姓名
        /// </summary>
        public string LoginExtName { get; set; }
        /// <summary>
        /// 辅助登录手机号
        /// </summary>
        public string LoginExtMobile { get; set; }
        /// <summary>
        /// 辅助登录的Id
        /// </summary>
        public int LoginExtId { get; set; }
        /// <summary>
        /// 附注登录的授权功能项
        /// </summary>
        public string LoginExtFuncKeys { get; set; }

        /// <summary>
        /// 辅助登录后，该功能是否可用
        /// </summary>
        /// <param name="fnKey">功能键名</param> 
        public bool IsExtFuncAllowed(string fnKey)
        {
            var ks = "," + LoginExtFuncKeys.ToLower() + ",";
            return ks.Contains("," + fnKey.ToLower() + ",");
        }


        #endregion


        public string IPAddress { get; set; }
        public string IPAddressName { get; set; }
        public DateTime LoginTime { get; set; }


        #region 扩展信息

        /// <summary>
        /// 是否获取的是空操作者
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return OpId <= 0 && StationId <= 0;
            }
        }



        /// <summary>
        /// 药房默认Id
        /// </summary>
        public int OutPatientStoreId { get; set; }
        /// <summary>
        /// 每页默认记录数
        /// </summary>
        public int TableRecordsPerPage { get; set; } = 20;

        /// <summary>
        /// 用户使用输入法 PINYIN/WUBI/EN 默认拼音
        /// </summary>
        public string UseIME { get; set; }

        /// <summary>
        /// App端的用户Id
        /// </summary>
        public string DoctorAppId { get; set; }
        #endregion
    }


    public class UserSelfEx : UserSelf
    {
        public string DoctorPicUrl { get; set; }
    }

}
