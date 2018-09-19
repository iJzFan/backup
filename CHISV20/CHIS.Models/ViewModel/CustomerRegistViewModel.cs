using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.ViewModel
{
   public class CustomerRegistViewModel
    {
        [Required(ErrorMessage ="请选择注册类型")]
        public string RegistType { get; set; } //mobile email

        [Required(ErrorMessage ="请选择注册角色")]
        public string RegistRole { get; set; } //doctor customer


        public string Email { get; set; }

        public string Mobile { get; set; }


        [Required(ErrorMessage ="必填验证码")]
        public string VCode { get; set; }

        [Required(ErrorMessage = "必填密码")]
        public string RegPaswd { get; set; }
        public string RegPaswdConfirm { get; set; }

       


    }
}
