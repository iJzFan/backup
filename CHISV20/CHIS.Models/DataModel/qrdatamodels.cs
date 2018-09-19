using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.DataModel
{
    public class RxMobileInputModel
    {
        public vwCHIS_Code_WorkStation Station { get; set; }
        public vwCHIS_Code_Doctor Doctor { get; set; }






        public long RxSaveId { get; set; } = 0;

        /// <summary> 
        /// 患者Id
        /// </summary>		

        public int CustomerId { get; set; } = 0;

        /// <summary> 
        /// 患者姓名
        /// </summary>		
        [Required(ErrorMessage = "姓名不能为空")]
        [DisplayName("姓名")]
        public string CustomerName { get; set; }

        /// <summary> 
        /// 患者性别
        /// </summary>		
        [Required]
        [DisplayName("性别")]
        public string CustomerGenderStr { get; set; }

        /// <summary> 
        /// 患者身份证
        /// </summary>		
        [Required(ErrorMessage = "身份证不能为空")]
        [DisplayName("身份证")]
        public string CustomerIdCode { get; set; }

        /// <summary> 
        /// 患者联系电话/手机
        /// </summary>		
        [DisplayName("联系电话/手机")]
        [Required(ErrorMessage ="必须填写联系方式")]
        public string CustomerMobile { get; set; }

        /// <summary> 
        /// 是否同意条款
        /// </summary>		
        [Required]
        public bool IsAgreement { get; set; } = false;

        /// <summary> 
        /// 处方拍照
        /// </summary>		

        public string RxPicUrl1 { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string RxPicUrl2 { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string RxPicUrl3 { get; set; }
         
    }
}
