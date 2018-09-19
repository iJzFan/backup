using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Ass;
using CHIS.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    public partial class Statistics
    {

        /// <summary>
        /// 财务报表
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="stTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult TreatList()
        {
            return View();
        }

        /// <summary>
        /// 载入接诊清单
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId">如果DoctorId=0则表示所有，null默认本医生</param>
        /// <param name="TimeRange"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult LoadTreatList(string searchText, string treatStatus = "All", int? stationId = null, int? doctorId = null, string TimeRange = "Today", int pageIndex = 1, int pageSize = 20)
        {
            DateTime dt = DateTime.Now;
            DateTime? dt0 = null; DateTime? dt1 = null;
            base.initialData_TimeRange(ref dt0, ref dt1, 1, timeRange: TimeRange);
             
            if (stationId <= 0||stationId==null) stationId = UserSelf.StationId;


            bool bmgr = _db.CHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == stationId).IsManageUnit;

            var finds = _db.vwCHIS_DoctorTreat.AsNoTracking();
            if (bmgr) finds = finds.InStation(stationId.Value);
            else finds = finds.Where(m => m.StationId == stationId.Value);

 


            if (doctorId >= 0) finds = finds.Where(m => m.DoctorId == doctorId);

            finds = finds.Where(m => (m.FirstTreatTime >= dt0 || m.TreatTime >= dt0) && (m.FirstTreatTime < dt1 || m.TreatTime < dt1));
            if (treatStatus == "treating") finds = finds.Where(m => m.TreatStatus == 1);
            else if (treatStatus == "treated") finds = finds.Where(m => m.TreatStatus == 2);
             
            if (searchText.IsNotEmpty()) finds = finds.Where(m => m.CustomerMobile == searchText || m.CustomerName == searchText);

            base.initialData_Page(ref pageIndex, ref pageSize);
            var item = finds.OrderByDescending(m => m.TreatId).Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var model = new Ass.Mvc.PageListInfo<vwCHIS_DoctorTreat>
            {
                DataList = item.ToList(),
                RecordTotal = finds.Count(),
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            _setDebugText(dt);
            return PartialView("_pvTreatList", model);
        }




    }

}