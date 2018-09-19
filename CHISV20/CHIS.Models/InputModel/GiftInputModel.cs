using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models
{
    public class GiftInputModel
    {
        /// <summary> 
        /// 
        /// </summary>
        [Required(ErrorMessage ="礼品名不能为空")]
        [MaxLength(20,ErrorMessage ="礼品名字长度不能超过20字")]
        public string GiftName { get; set; }

        /// <summary> 
        /// 类别, 0实物 1满减券 2满打折券
        /// </summary>
        [Range(0,2)]
        public int Type { get; set; }

        /// <summary> 
        /// 兑换积分
        /// </summary>		
        [Range(0,int.MaxValue,ErrorMessage ="兑换积分不能小于0")]
        public int NeedPoints { get; set; }

        /// <summary> 
        /// 
        /// </summary>		
        [Range(0, int.MaxValue,ErrorMessage ="库存不能小于0")]
        public int Stock { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string Description { get; set; }


        /// <summary>
        /// 使用须知
        /// </summary>
        public string Instruction { get; set; }

        /// <summary> 
        /// 封面图
        /// </summary>		

        public string CoverImg { get; set; }

        /// <summary> 
        /// 满多少可以使用
        /// </summary>		
        [Range(0, 100000f,ErrorMessage ="满减限制不能小于0")]
        public decimal? OrderLimit { get; set; }

        /// <summary> 
        /// 可以打多少折
        /// </summary>		
        [Range(0, 1f,ErrorMessage ="打折范围应为0-1之间的小数")]
        public decimal? DiscountRate { get; set; }

        /// <summary> 
        /// 可抵减多少
        /// </summary>		
        [Range(0, 10000f,ErrorMessage ="抵减不能小于0")]
        public decimal? DiscountPrice { get; set; }

        /// <summary> 
        /// 活动有效期
        /// </summary>		

        public DateTime? ExpiryDate { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string Img1 { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string Img2 { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string Img3 { get; set; }

        /// <summary> 
        /// 有效期
        /// </summary>		
        [Range(0, int.MaxValue)]
        public int? AvailableDays { get; set; }

        /// <summary>
        /// 微信卡券号
        /// </summary>
        public string WeChatCardId { get; set; }
    }
}
