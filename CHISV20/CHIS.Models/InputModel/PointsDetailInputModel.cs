using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.InputModel
{
    public class PointsDetailInputModel
    {
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
    }
}
