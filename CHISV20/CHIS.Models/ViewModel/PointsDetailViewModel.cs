using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class PointsDetailViewModel
    {
        public long Id { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int CustomerId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? GiftOrderId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string Description { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public long Points { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public DateTime CreatedTime { get; set; }
    }


}
