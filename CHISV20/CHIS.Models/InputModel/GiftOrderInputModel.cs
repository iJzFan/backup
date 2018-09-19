using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.InputModel
{
    /// <summary>
    /// 商品订单信息
    /// </summary>
    public class GiftOrderInputModel
    {

        /// <summary> 
        /// 客户的Id
        /// </summary>		
        [Required]
        public int CustomerId { get; set; }

        /// <summary> 
        /// 商品Id
        /// </summary>		
        [Required]
        public int GiftId { get; set; }

        /// <summary> 
        /// 数量
        /// </summary>		
        [Required]
        public int Count { get; set; }

        /// <summary> 
        /// 客户姓名
        /// </summary>		
        [MaxLength(20,ErrorMessage ="姓名长度不能超过20")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        [RegularExpression(@"(\d{11})|^((\d{7,8})|(\d{4}|\d{3})-(\d{7,8})|(\d{4}|\d{3})-(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1})|(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1}))$",ErrorMessage ="手机号码格式错误")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 地址信息
        /// </summary>
        public string Address { get; set; }

    }
}
