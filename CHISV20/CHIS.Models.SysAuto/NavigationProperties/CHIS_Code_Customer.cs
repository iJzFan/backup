using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHIS.Models
{
    public partial class CHIS_Code_Customer
    {
        [ForeignKey("CustomerId")]
        public IList<CHIS_Customer_PointsDetail> PointsDetailList { get; set; }

        [ForeignKey("CustomerId")]
        public IList<CHIS_Gift_Order> GiftOrderList { get; set; }

        /// <summary>
        /// 更改积分
        /// </summary>
        /// <param name="detail"></param>
        public void ChangePoints(CHIS_Customer_PointsDetail detail)
        {

            if (this.PointsDetailList == null)
            {
                this.PointsDetailList = new List<CHIS_Customer_PointsDetail>();
            }

            this.PointsDetailList.Add(detail);
            if (this.Points.HasValue)
            {
                this.Points = this.Points + detail.Points;
            }
            else
            {
                this.Points = detail.Points;
            }

            if (this.Points < 0)
            {
                throw new ApplicationException("积分不能小于0");
            }
        }

        /// <summary>
        /// 兑换礼品
        /// </summary>
        /// <param name="order"></param>
        public void ExchangeGift(CHIS_Gift_Order order)
        {
            if (this.GiftOrderList == null)
            {
                this.GiftOrderList = new List<CHIS_Gift_Order>();
            }

            this.GiftOrderList.Add(order);

            order.Gift.Stock = order.Gift.Stock - order.Count;

            if (order.Gift.Stock < 0)
            {
                throw new ApplicationException("库存不足");
            }

            var points = this.Points - (order.Gift.NeedPoints * order.Count);

            if (points < 0)
            {
                throw new ApplicationException("积分不足");
            }
        }
    }
}
