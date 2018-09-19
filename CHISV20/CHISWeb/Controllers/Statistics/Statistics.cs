using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
  [AllowAnonymous]
    public partial class Statistics : BaseController
    {

        public Statistics(DbContext.CHISEntitiesSqlServer db) : base(db) { }

        [AllowAnonymous]
        //医生工作量统计
        // timeRange: Today,ThisWeek,ThisMonth,ThisQuarter,ThisYear,dt0=xxx;dt1=xxx
        public IActionResult TreatWorkOfDoctor(string timeRange,string stTime,string endTime,int doctorId=0 //,int pageIndex=1,int pageSize=20
            )
        {
            DateTime? dt0 = Convert.ToDateTime(stTime), dt1 = Convert.ToDateTime(endTime);
            base.initialData_TimeRange(ref dt0, ref dt1, timeRange: timeRange); //时间初始化
          //  base.initialData_Page(ref pageIndex, ref pageSize); //分页的初始化

            var model = new StatisticsCBL(this).TreatWorkOfDoctorCBL(dt0.Value,dt1.Value,doctorId);
            return View(model);
        }
        /* 管理者统计各子工作站的工作量，例如，天使自营店下的五株工作站、费丽山工作站等的工作量统计
         * 工作站的工作量统计
         **/
        [AllowAnonymous]
        public IActionResult TreatWorkOfStation(string timeRange, string stTime, string endTime //,int pageIndex=1,int pageSize=20
           )
        {
            DateTime? dt0 = Convert.ToDateTime(stTime), dt1 = Convert.ToDateTime(endTime);
            base.initialData_TimeRange(ref dt0, ref dt1, timeRange: timeRange); //时间初始化
                                                                                //  base.initialData_Page(ref pageIndex, ref pageSize); //分页的初始
            var model = new StatisticsCBL(this).TreatWorkOfStationCBL(dt0.Value, dt1.Value);
            return View(model);
        }
        public IActionResult TreatWorkOfStationView()
        {
            return View();
        }
        //管理者统计各子工作站的工作量，例如，天使自营店下的五株工作站、费丽山工作站等的工作量统计
        public IActionResult TreatWorkOfTotalStationView()
        {
            return View();
        }
         

         



    }

}