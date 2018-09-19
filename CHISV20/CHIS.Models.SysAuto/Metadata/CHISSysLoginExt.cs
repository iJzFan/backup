using Ass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models
{

    public partial class CHISSysLoginExt
    {

        /// <summary> 
        /// 
        /// </summary>		
        [Key]

        public long LoginExtId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		
        [Required]
        [DisplayName("关联的登录Id")]
        public long LoginExtParentLoginId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		
        [Required]
        [DisplayName("登录名")]
        public string LoginExtName { get; set; }

        /// <summary> 
        /// 
        /// </summary>		
        [Required]
        [DisplayName("手机号")]
        [MobileNumber]
        public string LoginExtMobile { get; set; }

        /// <summary> 
        /// 
        /// </summary>		
     
        [DisplayName("登录密码")]
        [Display(Prompt ="密码长度4-20字符")]
        [StringLength(20,MinimumLength =4, ErrorMessage = "密码长度在4-20字符")]
        [DataType(DataType.Password)]
        public string LoginExtPassword { get; set; }
    
        [DisplayName("密码确认")]
        [Display(Prompt = "密码长度4-20字符")]
        [StringLength(20, MinimumLength = 4,ErrorMessage ="密码长度在4-20字符")]
        [Compare("LoginExtPassword",ErrorMessage ="密码请输入一致")]
        [DataType(DataType.Password)]
        public string LoginExtPasswordConfirm { get; set; }
        /// <summary> 
        /// 
        /// </summary>		

        [Required]
        [DisplayName("角色组")]
        public string LoginExtRoleKeys { get; set; }

        /// <summary> 
        /// 
        /// </summary>		
        [Required]
        [DisplayName("是否可用")]
        public bool LoginExtEnabled { get; set; } = false;

    }
}
