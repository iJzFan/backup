using System;
using System.Collections.Generic;
using System.Text;
using Ass;

namespace CHIS.Models.ViewModel
{
    public class GiftOrderViewModel
    {
        public long GiftOrderId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int CustomerId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int GiftId { get; set; }

        public string GiftName { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime? DeadLine { get; set; }

        /// <summary>
        /// 使用须知
        /// </summary>
        public string Instruction { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int Count { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public long TotalPoints { get; set; }

        public string CoverImg { get; set; }

        /// <summary> 
        /// 
        /// </summary>	

        public string CustomerName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public bool? IsCompleted { get; set; }

        public string SpendingPlace { get; set; }

        public DateTime? SpendingTime { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public DateTime CreatedTime { get; set; }

        public string Shipper { get; set; }

        public string LogisticCode { get; set; }

        public string WeChatCardCode { get; set; }

        public string State { get; set; }

        public void CheckOrderState()
        {
            if (!string.IsNullOrEmpty(this.Address))
            {
                if (this.IsCompleted.HasValue && this.IsCompleted == true)
                {
                    this.State = OrderState.Shipped.GetName();//OrderState.已发货.ToString();
                }
                else
                {
                    this.State = OrderState.Unprocessed.GetName();
                }
            }
            else
            {
                if (this.DeadLine.HasValue && this.DeadLine.Value.Date <= DateTime.Now.Date)
                {
                    this.State = OrderState.Expired.GetName();
                }
                else
                if (this.IsCompleted.HasValue && this.IsCompleted == true)
                {
                    this.State = OrderState.Used.GetName();
                }
                else
                {
                    this.State = OrderState.NotUse.GetName();
                }
            }
        }
    }
}

