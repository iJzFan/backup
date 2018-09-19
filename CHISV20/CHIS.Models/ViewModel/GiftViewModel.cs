using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class GiftViewModel
    {
        public int GiftId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		
        public string GiftName { get; set; }

        /// <summary> 
        /// 类别, 0实物 1满减券 2满打折券
        /// </summary>		
        public int Type { get; set; }

        /// <summary> 
        /// 兑换积分
        /// </summary>		

        public int NeedPoints { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

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

        public decimal? OrderLimit { get; set; }

        /// <summary> 
        /// 可以打多少折
        /// </summary>		

        public decimal? DiscountRate { get; set; }

        /// <summary> 
        /// 可抵减多少
        /// </summary>		

        public decimal? DiscountPrice { get; set; }

        /// <summary> 
        /// 有效期
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

        public int? AvailableDays { get; set; }

        /// <summary>
        /// 微信卡券Id
        /// </summary>
        public string WeChatCardId { get; set; }

    }
}
