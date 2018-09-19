using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class RxDrugSaveViewModel
    {
        public RxUserViewModel NewRxUser { get; set; }

        public IEnumerable<RxUserViewModel> RxUserList { get; set; }
    }

    public class RxUserViewModel
    {
        public IEnumerable<long> RxSaveDrugsId { get; set; }

        public IEnumerable<RxDrugViewModel> DrugList { get; set; }

        public long RxSaveId { get; set; }

        /// <summary> 
        /// 患者Id
        /// </summary>		
        public int CustomerId { get; set; }

        /// <summary> 
        /// 患者姓名
        /// </summary>
        [Required(ErrorMessage ="姓名不能为空")]
        public string CustomerName { get; set; }

        /// <summary> 
        /// 患者性别
        /// </summary>		
        
        public string CustomerGenderStr { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string SendDrugMan { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string PhotoUrlDef { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public DateTime SendTime { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string CheckDrugMan { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public DateTime CheckTime { get; set; }

        /// <summary> 
        /// 患者身份证
        /// </summary>
        [Required(ErrorMessage = "身份证不能为空")]
        public string CustomerIdCode { get; set; }

        /// <summary> 
        /// 患者联系电话/手机
        /// </summary>
        [Required(ErrorMessage = "手机号不能为空")]
        [RegularExpression(@"(\d{11})|^((\d{7,8})|(\d{4}|\d{3})-(\d{7,8})|(\d{4}|\d{3})-(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1})|(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1}))$", ErrorMessage = "格式错误")]
        public string CustomerMobile { get; set; }

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

    public class RxDrugViewModel
    {
        public long RxSaveDrugsId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? DrugId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string DrugName { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string DrugModel { get; set; } = " ";

        /// <summary> 
        /// 
        /// </summary>		

        public string DrugManufacture { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string DrugPiNo { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public DateTime? DrugDeadTime { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? DrugQty { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string DrugUnitName { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        //public long RxSaveId { get; set; }
    }
}
