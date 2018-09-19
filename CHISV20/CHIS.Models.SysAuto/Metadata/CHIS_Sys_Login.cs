using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ass;

namespace CHIS.Models
{
    [ModelMetadataType(typeof(m_Login))]
    public partial class CHIS_Sys_Login
    {
        public CHIS_Sys_Login()
        {
             
        }

        private class m_Login
        {
            public int CustomerId { get; set; }

            [MaxLength(32, ErrorMessage = "最大32字符")]
            [MinLength(8, ErrorMessage = "最小8位字符")]
            [Remote("LoginNameRegistIsAllowed", "Customer", "", AdditionalFields = "CustomerId", ErrorMessage = "该登录名已经使用了或者输入错误")]

            public string LoginName { get; set; }
        }
    }



}
