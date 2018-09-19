using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ah.Models.ViewModel
{
   public class CustomerRegistViewModel
    {

        /// <summary>
        /// 注册类型mobile/email
        /// </summary>
        [Required(ErrorMessage ="请选择注册类型")]
        public string RegistType { get; set; } //mobile email



        /// <summary>
        /// 注册角色 doctor/customer
        /// </summary>
        [Required(ErrorMessage ="请选择注册角色")]
        public string RegistRole { get; set; } //doctor customer


        public string Email { get; set; }

        public string Mobile { get; set; }



        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 性别 1男，0女
        /// </summary>
        [Required(ErrorMessage ="健康来说，性别很重要，请选择")]
        public int Gender { get; set; }

        /// <summary>
        /// 生日 
        /// </summary>
        [Required(ErrorMessage ="健康来说，生日会计算年龄，很重要，请选择")]
        public DateTime? Birthday { get; set; }


        [Required(ErrorMessage ="必填验证码")]
        public string VCode { get; set; }

        [Required(ErrorMessage = "必填密码")]
        [MinLength(6,ErrorMessage ="最低6位密码")]
        public string RegPaswd { get; set; }

        [Compare("RegPaswd",ErrorMessage ="验证密码输入不对")]
        public string RegPaswdConfirm { get; set; }

       


    }
}
