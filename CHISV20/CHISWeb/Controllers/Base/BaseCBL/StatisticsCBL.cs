using Ass;

using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;
using CHIS;

namespace CHIS.Controllers
{
    public partial class StatisticsCBL : BaseCBL
    {
        public StatisticsCBL(BaseController c) : base(c)
        {

        }

        #region 
        //医生工作站统计执行的存储过程
        public object TreatWorkOfDoctorCBL(DateTime dt0, DateTime dt1, int doctorId)
        {
            var u = controller.UserSelf;

            string starTime = dt0.ToString("yyyy-MM-dd");
            string endTime = dt1.ToString("yyyy-MM-dd");
            var finds = _db.SqlQuery<CHIS.Models.StatisticsModels.DocWorkstationStatistics>(string.Format("exec sp_Statistics_Doctor_Workload {0},'{1}','{2}',{3}", u.StationId, starTime, endTime, doctorId));
            return finds;
        }

        #endregion

        #region   
        //医生工作站统计的存储过程执行
        public object TreatWorkOfStationCBL(DateTime sTime, DateTime eTime)
        {
            var u = controller.UserSelf;
            string starTime = sTime.ToString("yyyy-MM-dd");
            string endTime = eTime.ToString("yyyy-MM-dd");
            var finds = _db.SqlQuery<CHIS.Models.StatisticsModels.WorkstationStatistics>(string.Format("exec sp_Statistics_WorkStation_Workload {0},'{1}','{2}'", u.StationId, starTime, endTime));
            return finds;
        }

        #endregion
        #region 财务报表-收费类型
        public IEnumerable<CHIS.Models.StatisticsModels.ChargeType> ChargeTypeChartCBL(DateTime sTime, DateTime eTime, int stationId)
        {
            string starTime = sTime.ToString("yyyy-MM-dd");
            string endTime = eTime.ToString("yyyy-MM-dd");
            var finds = _db.SqlQuery<CHIS.Models.StatisticsModels.ChargeType>(string.Format("exec sp_Statistics_Finance_Chart_ChargeType '{0}','{1}',{2}", starTime, endTime, stationId));
            return finds;
        }
        #endregion

        #region 收益统计—线性报表
        public IEnumerable<CHIS.Models.StatisticsModels.ChargeGain> GainChartCBL(DateTime sTime, DateTime eTime, int stationId)
        {
            sTime = sTime.Date;
            string starTime = sTime.ToString("yyyy-MM-dd");
            string endTime = eTime.ToString("yyyy-MM-dd");
            var finds = _db.SqlQuery<CHIS.Models.StatisticsModels.FeeGainItem>(string.Format("exec sp_Statistics_Finance_Chart_Gains '{0}','{1}',{2}", starTime, endTime, stationId));
            //获取基本数据后，重新整理计算
            List<CHIS.Models.StatisticsModels.ChargeGain> rlt = new List<Models.StatisticsModels.ChargeGain>();
            var days = (eTime - sTime).Days;
            for (int i = 0; i < days; i++)
            {
                rlt.Add(new Models.StatisticsModels.ChargeGain { payDate = sTime.AddDays(i) });
            }
            foreach (var item in finds)
            {
                var rltItem = rlt.First(m => m.payDate == item.PayDateV);
                switch (item.FeeTypeKey)
                {
                    case "Herbs": rltItem.HerbVal += item.TotalVal; break;
                    case "Formed": rltItem.FormedVal += item.TotalVal; break;
                    case "ExtraFeeType_YF": rltItem.ShippingVal += item.TotalVal; break;//快递费
                    case "ExtraFeeType_ZJ": rltItem.ConsultationVal += item.TotalVal; break;//诊金
                    default: rltItem.OtherFeeVal += item.TotalVal; break;
                }
            }
            return rlt;
        }


        #endregion


        #region 网上药店发药报表
        public IEnumerable<CHIS.Models.StatisticsModels.FeeGainOfNetWebItem> GainOfWebNetSended(int stationId, int supplierId, DateTime dt0, DateTime dt1)
        {
            dt0 = dt0.Date;
            string starTime = dt0.ToString("yyyy-MM-dd");
            string endTime = dt1.ToString("yyyy-MM-dd");
            var sql =
                @"select m.sendDate ,sum(m.TotalAmount) as TotalAmount,sum(m.ContainTransFee) as TransFeeAmount from (
	              select CONVERT(varchar(10), a.SendTime,120) as sendDate, 
	              a.* from [CHIS_Shipping_NetOrder] a where (a.SendTime>='{0}' and a.SendTime<'{1}') and a.SendedStatus=1 and a.SupplierId={2} and dbo.fn_InStation({3},a.StationId)=1
	              ) m group by m.sendDate";
            sql = string.Format(sql, starTime, endTime, supplierId, stationId);

            var finds = _db.SqlQuery<CHIS.Models.StatisticsModels.FeeGainOfNetWebItem>(sql);

            //获取基本数据后，重新整理计算
            List<CHIS.Models.StatisticsModels.FeeGainOfNetWebItem> rlt = new List<Models.StatisticsModels.FeeGainOfNetWebItem>();
            var days = (dt1 - dt0).Days;
            for (int i = 0; i < days; i++)
            {
                rlt.Add(new Models.StatisticsModels.FeeGainOfNetWebItem { sendDate = dt0.AddDays(i).ToString("yyyy-MM-dd") });
            }
            foreach (var item in finds)
            {
                var rltItem = rlt.First(m => m.sendDate == item.sendDate);
                rltItem.TotalAmount = item.TotalAmount;
                rltItem.TransFeeAmount = item.TransFeeAmount;              
            }
            return rlt;
        }


        #endregion


    }
}
