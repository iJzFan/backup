using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class AccountLoginViewModel
    {


        [Required(ErrorMessage = "请输入登录用户名")]

        public string AccountName { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        public string AccountPassword { get; set; }
    }

    public class HisLoginViewModel
    {


        [Required(ErrorMessage = "请输入登录手机或者邮箱或者身份证号")]

        public string LoginName { get; set; }

        [Required(ErrorMessage = "请输入密码或登录验证码")]
        public string LoginPassword { get; set; }

        /// <summary>
        /// 密码是否是验证码
        /// </summary>
        public bool IsPasswordVCode { get; set; } = false;

        /// <summary>
        /// 登录的工作站Id
        /// </summary>
        public int StationId { get; set; }


        /// <summary>
        /// 选择的科室
        /// </summary>
        public int? DepartId { get; set; }


        /// <summary>
        /// 是否重登陆
        /// </summary>
        public bool IsReLogin { get; set; } = false;

        public string LoadEncipt { get; set; }
        public long BaseTimeTicks { get; set; }

        public bool IsNeedLoginExt { get; set; } = false;
        public string LoginExtMobile { get; set; }
        public string LoginExtPwd { get; set; }
    }

    /// <summary>
    /// 工作站药店信息
    /// </summary>
    public class StationStoreInfo
    {
        public int StationId { get; set; }
        public string StationName { get; set; }
        public string StationLogPicH { get; set; }
        public string StationLogPicV { get; set; }
        public string StoreName { get; set; }
        public int DoctorId { get; set; }
    }
}
