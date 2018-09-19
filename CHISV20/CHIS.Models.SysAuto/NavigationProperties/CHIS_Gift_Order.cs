using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHIS.Models
{
    public partial class CHIS_Gift_Order
    {
        [ForeignKey("GiftId")]
        public CHIS_Code_Gift Gift { get; set; }

        [ForeignKey("CustomerId")]
        public CHIS_Code_Customer Customer { get; set; }

        public void Create()
        {
            this.CreatedTime = DateTime.Now;
        }


        /// <summary>
        /// 兑换礼品
        /// </summary>
        /// <param name="description"></param>
        /// <param name="giftOrderId"></param>
        public void ExchangeGift(string description, long giftOrderId)
        {
            this.TotalPoints = this.Gift.NeedPoints * this.Count;

            this.Gift.Stock = this.Gift.Stock - this.Count;

            if (this.Gift.Type < 1)
            {
                if (string.IsNullOrWhiteSpace(this.Address) || string.IsNullOrWhiteSpace(this.PhoneNumber) || string.IsNullOrWhiteSpace(this.CustomerName))
                {
                    throw new ApplicationException("兑换实物需要姓名,电话,地址信息");
                }
            }


            if (this.Gift.Stock < 0)
            {
                throw new ApplicationException("库存不足");
            }

            var points = this.Customer.Points.Value - (this.Gift.NeedPoints * this.Count);

            if (points < 0)
            {
                throw new ApplicationException("积分不足");
            }

            if (this.Gift.AvailableDays.HasValue)
            {
                this.DeadLine = this.CreatedTime.AddDays((double)this.Gift.AvailableDays);
            }

            if (this.Gift.ExpiryDate.HasValue)
            {
                this.DeadLine = this.Gift.ExpiryDate.Value;
            }

            this.Customer.ChangePoints(new CHIS_Customer_PointsDetail
            {
                CreatedTime = DateTime.Now,
                CustomerId = Customer.CustomerID,
                Description = description,
                GiftOrderId = giftOrderId,
                Points = -this.TotalPoints
            });
        }

       /// <summary>
       /// 使用虚拟物品
       /// </summary>
       /// <param name="spendingPlace">消费地点</param>
        public void UseGift(string spendingPlace)
        {
            if(this.IsCompleted == true)
            {
                throw new ApplicationException("该礼券已使用");
            }

            if (this.Gift.Type < 1)
            {
                throw new ApplicationException("不支持的商品类型");
            }

            if (this.DeadLine.HasValue)
            {
                if(this.DeadLine.Value.Date<= DateTime.Now.Date)
                {
                    throw new ApplicationException("礼券已过期");
                }
            }

            this.SpendingPlace = spendingPlace;

            this.SpendingTime = DateTime.Now;

            this.IsCompleted = true;
        }

        /// <summary>
        /// 发货
        /// </summary>
        public void Ship(string shipper,string logisticCode)
        {
            this.Shipper = shipper;

            this.LogisticCode = logisticCode;

            this.SpendingTime = DateTime.Now;

            this.IsCompleted = true;

        }
    }
}
