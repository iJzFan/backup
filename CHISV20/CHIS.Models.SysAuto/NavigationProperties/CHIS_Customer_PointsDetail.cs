using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHIS.Models
{
    public partial class CHIS_Customer_PointsDetail
    {
        [ForeignKey("CustomerId")]
        public CHIS_Code_Customer Customer { get; set; }

        [ForeignKey("PointsRuleId")]
        public CHIS_Customer_PointsDetail_Rule PointsRule { get; set; }

        /// <summary>
        /// 通过积分规则生成积分明细
        /// </summary>
        /// <param name="consumerMoney">消费金额</param>
        public void CreatePointSDetail(CHIS_Customer_PointsDetail_Rule rule, decimal? consumerMoney)
        {
            this.PointsRule = rule ?? throw new NullReferenceException("积分规则不存在");

            this.Points = this.PointsRule.ConstantPoints + new Random().Next(this.PointsRule.RandomPoints);

            if (consumerMoney.HasValue)
            {
                this.Points = this.Points + (long)(consumerMoney.Value * this.PointsRule.ConsumerBackRate);
            }

            this.PointsRuleId = this.PointsRule.PointsRuleId;

            this.Description = this.PointsRule.Description;

        }
    }
}
