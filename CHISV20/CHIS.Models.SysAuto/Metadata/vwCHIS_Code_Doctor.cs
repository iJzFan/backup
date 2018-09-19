using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ass;

namespace CHIS.Models
{
    [ModelMetadataType(typeof(m_Doctor))]
    public partial class vwCHIS_Code_Doctor
    {
        public vwCHIS_Code_Doctor() {
     
        }

        private class m_Doctor
        {
            public int DoctorId { get; set; }

            [Required(ErrorMessage = "请输入医生姓名")]
            public string DoctorName { get; set; }
            
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

    [ModelMetadataType(typeof(m1_Doctor))]
    public partial class CHIS_Code_Doctor
    {
        private class m1_Doctor
        {

            public int DoctorId { get; set; }

            [Required(ErrorMessage = "请输入医生姓名")]
            public string DoctorName { get; set; }

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

            [Required(ErrorMessage = "请选择职称")]
            public int? PostTitle { get; set; }

            [Required(ErrorMessage = "请选择职位")]
            public int? Principalship { get; set; }

            
            [MaxLength(1000,ErrorMessage ="最长1000字")]
            public string DoctorSkillRmk { get; set; }
        }
    }


}
