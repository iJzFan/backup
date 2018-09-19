using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ah.Models.ViewModel
{

    public class WechatBindingModel
    {
        [Required]
        public string openid { get; set; }
        [Required(ErrorMessage = "请填写您的手机号码")]
        public string mobile { get; set; }

        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "请填写您的真实姓名")]
        public string CustomerName { get; set; }
        public string NickName { get; set; }

        [Required(ErrorMessage = "请选择您的性别")]
        public int? Gender { get; set; } 
        public string WxPicUrl { get; set; }

        [Required(ErrorMessage = "请输入生日信息，以便正确获取您的年龄")]
        public DateTime? Birthday { get; set; }

 
    }
 


}
