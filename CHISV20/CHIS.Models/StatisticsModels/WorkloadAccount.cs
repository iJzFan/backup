using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CHIS.Models.StatisticsModels
{
    //医生工作量统计
    public class DocWorkstationStatistics
    {
        public string doctorName { get; set; }//医生名称列表
        public long doctorId { get; set; }//医生ID列表
        public int registerNum { get; set; }//医生被预约的数
        public decimal totalAmout { get; set; } //总收入
        public int treatNum { get; set; } //接诊量
    }

    //工作站工作量统计
    public class WorkstationStatistics
    {
        public int StationId { get; set; }
        public int registerNum { get; set; }
        public int doctorNum { get; set; }
        public decimal totalAmout { get; set; }
        public int treatNum { get; set; }
        public int drugNum { get; set; }
        public string StationName { get; set; }

    }
    
    //财务统计-收费类型图表
    public class ChargeType
    {public string  FeeProp{ get; set; } //收费类型名称
       public decimal Amount { get; set; } //收费类型总金额       
    }
    //收益报表-线性报表
    public class ChargeGain
    {
        public DateTime payDate { get; set; } //支付成功时间
        public string payDateStr { get {
                if (payDate.Year == DateTime.Now.Year) return payDate.ToString("MM-dd");
                else return payDate.ToString("yyyy-MM-dd"); } }
        public decimal TotalVal { get { return FormedVal + HerbVal + ShippingVal + ConsultationVal + OtherFeeVal; } } //每日总收益
        public decimal FormedVal { get; set; }//每日成药总收益
        public decimal HerbVal { get; set; }//每日中药总收益
        public decimal ShippingVal { get; set; }
        public decimal ConsultationVal { get; set; }

        /// <summary>
        /// 其他费用
        /// </summary>
        public decimal OtherFeeVal { get; set; }
    }
    public class FeeGainItem
    {
        /// <summary>
        /// 格式为yyyy-MM-dd
        /// </summary>
       public string payDate { get; set; }


        public DateTime PayDateV { get { return Ass.P.PDateTimeV(payDate,new DateTime()).Value; } }
        public string FeeTypeName { get; set; }
        public string FeeTypeKey { get; set; }
        public decimal TotalVal { get; set; }
    }

    public class FeeGainOfNetWebItem
    {
        /// <summary>
        /// 格式为yyyy-MM-dd
        /// </summary>
        public string sendDate { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TransFeeAmount { get; set; }
    }

    public class TitleVal
    {
        public string Title { get; set; }
        public decimal Val { get; set; }
    }


}
