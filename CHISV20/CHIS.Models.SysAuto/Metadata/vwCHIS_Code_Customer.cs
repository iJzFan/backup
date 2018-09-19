using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ass;

namespace CHIS.Models
{
    [ModelMetadataType(typeof(m_Customer))]
    public partial class vwCHIS_Code_Customer
    {
        public vwCHIS_Code_Customer() {

        }

        private class m_Customer
        {
            public int CustomerID { get; set; }

            [Required(ErrorMessage = "请输入用户姓名")]
            public string CustomerName { get; set; }
            
            [Required(ErrorMessage ="请选择性别")]
            public int? Gender { get; set; }

            [Required(ErrorMessage ="请输入出生日期")]            
            public DateTime? Birthday { get; set; }


            [MaxLength(32, ErrorMessage = "最大32字符")]
            [MinLength(8, ErrorMessage = "最小8位字符")]
            [Remote("LoginNameRegistIsAllowed", "Customer","",AdditionalFields = "CustomerID", ErrorMessage = "该登录码已经使用了或者输入错误")]
            public string LoginName { get; set; }


            [EmailAddress(ErrorMessage ="请输入正确的Email地址")]
            [Remote("EmailRegistIsAllowed", "Customer", "", AdditionalFields = "CustomerID", ErrorMessage ="该邮箱已经使用了或者输入错误")]
            public string Email { get; set; }

            [MobileNumber(ErrorMessage ="请输入正确的手机号")]
            [Remote("MobileRegistIsAllowed", "Customer", "", AdditionalFields = "CustomerID", ErrorMessage = "该手机已经使用了或者输入错误")]
            public string CustomerMobile { get; set; }

 
        
            [PRCIdNumber(ErrorMessage = "请输入正确的身份证号")]
            [Remote("IdCardRegistIsAllowed", "Customer", "", AdditionalFields = "CustomerID", ErrorMessage = "该身份证已经使用了或者输入错误")]
            public string IDcard { get; set; }
        }
    }

    [ModelMetadataType(typeof(m1_Customer))]
    public partial class CHIS_Code_Customer
    {
        private class m1_Customer
        {

            [Required(ErrorMessage = "请输入用户姓名")]
            public string CustomerName { get; set; }

            [Required(ErrorMessage = "请选择性别")]
            public int? Gender { get; set; }

            [Required(ErrorMessage = "请输入出生日期")]
            public DateTime? Birthday { get; set; }



         
            [MaxLength(32,ErrorMessage ="最大32字符")]
            [MinLength(8,ErrorMessage ="最小8位字符")]
            [Remote("LoginNameRegistIsAllowed", "Customer", "", AdditionalFields = "CustomerID", ErrorMessage = "该登录名已经使用了或者输入错误")]
            public string LoginName { get; set; }


            [EmailAddress(ErrorMessage = "请输入正确的Email地址")]
            [Remote("EmailRegistIsAllowed", "Customer", "", AdditionalFields = "CustomerID", ErrorMessage = "该邮箱已经使用了或者输入错误")]
            public string Email { get; set; }

            [MobileNumber(ErrorMessage = "请输入正确的手机号")]
            [Remote("MobileRegistIsAllowed", "Customer", "", AdditionalFields = "CustomerID", ErrorMessage = "该手机已经使用了或者输入错误")]
            public string CustomerMobile { get; set; }



            [PRCIdNumber(ErrorMessage = "请输入正确的身份证号")]
            [Remote("IdCardRegistIsAllowed", "Customer", "", AdditionalFields = "CustomerID", ErrorMessage = "该身份证已经使用了或者输入错误")]
            public string IDcard { get; set; }
        }
    }


}
