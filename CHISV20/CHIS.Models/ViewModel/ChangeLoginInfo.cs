using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.ViewModel
{
 
    public class ChangeLoginInfo
    {

        [Required]
        public string VarifyType { get; set; }

        public string MobileNumber { get; set; }
        public string MobileNumberVCode { get; set; }
        public string Email { get; set; }
        public string EmailVCode { get; set; }

        public string IDCard { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        public string MyPswd { get; set; }

        /// <summary>
        /// 原密码
        /// </summary>
        public string OrigPswd { get; set; }

        /// <summary>
        /// 新密码确认
        /// </summary>
        public string MyPswdConfirm { get; set; }
    }
}
