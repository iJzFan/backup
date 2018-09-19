using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ah.Models.ViewModel
{
    public class AccountLoginViewModel
    {

        [Required(ErrorMessage = "请输入登录用户名")]

        public string CustomerName { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        public string CustomerPassword { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class AHMSLoginAccountViewModel
    {
        [Required(ErrorMessage = "请输入登录手机")]

        public string LoginMobile { get; set; }

        [Required(ErrorMessage = "请输入登录密码")]
        public string LoginPassword { get; set; }
        public string ReturnUrl { get; set; }
    }
}
