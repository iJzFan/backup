using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ass;

namespace CHIS
{
    public static class ExpandClass
    {
        /// <summary>
        /// 获取工作站的当日接诊量
        /// </summary>
        public static Models.StatisticsModels.TreatBasicSummary StationTodayTreatSummary(this CHIS.Models.vwCHIS_Code_WorkStation model, int doctorId)
        {
            var db = new Code.Utility.DataBaseHelper().GetMainDbContext();
            string sql = string.Format(@"
  select tt.name,sum(tt.waiting) waiting,sum(tt.treating) treating,sum(tt.treated) treated from (
   select 'ThisToday' name,0 waiting,0 treating,0 treated
   union
   select a as name,waiting,treating,treated from ( 
     select 'ThisToday' a,TreatStatus b ,count(1) n from vwCHIS_Register
     where StationId={0} and EmployeeID={1} and RegisterDate >= '{2}' and RegisterDate< '{3}' 
     group by TreatStatus) t pivot(max(n) for b in(waiting,treating,treated)) m
  ) tt group by tt.name
", model.StationID, doctorId, DateTime.Today.ToDateString(), DateTime.Today.AddDays(1).ToDateString());
            var rlt = db.SqlQuery<Models.StatisticsModels.TreatBasicSummary>(sql).FirstOrDefault();
            return rlt;
        }


    }
}
