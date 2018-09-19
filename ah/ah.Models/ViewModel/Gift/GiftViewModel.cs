using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class GiftViewModel
    {
        public int GiftId { get; set; }

        public string GiftName { get; set; }

        //0实物 1满减券 2满打折券
        public int Type { get; set; }

        public int NeedPoints { get; set; }

        public int Stock { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// 使用须知
        /// </summary>
        public string Instruction { get; set; }

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

        public string CoverImg { get; set; }

        public string Img1 { get; set; }

        public string Img2 { get; set; }

        public string Img3 { get; set; }

        public int? AvailableDays { get; set; }

        public string State { get; set; }
    }
}
