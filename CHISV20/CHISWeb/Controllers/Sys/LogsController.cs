using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Ass;
using System;
using CHIS.Services;
using CHIS.Models.ViewModel;

namespace CHIS.Controllers.Sys
{
    public partial class LogsController : BaseController
    {
        private MongoLogService _mongoLog;

        public LogsController(DbContext.CHISEntitiesSqlServer db, MongoLogService mongoLog) : base(db) { _mongoLog = mongoLog; }
        /// <summary>
        /// 业务功能页面入口
        /// </summary>

        //初始框架页面
        public IActionResult Logs_Manage()
        {
            return View("~/Views/Sys/Logs/Logs_Manage.cshtml");//此处需要特别指明页面文件的位置
        }

        public IActionResult LogList(string logType, DateTime? startDate, DateTime? endDate)
        {
            var sdate = startDate ?? DateTime.Now.Date;
            var edate = endDate ?? DateTime.Now.Date.AddDays(1);

            var db = new Code.Utility.DataBaseHelper().GetLogDbContext();
            var finds = db.CHIS_Sys_Logs.AsNoTracking().Where(m => m.LogTime >= sdate && m.LogTime < edate);
            if (logType.IsNotEmpty()) finds = finds.Where(m => m.LogType == logType);
            return base.FindPagedData_jqgrid(finds.OrderByDescending(m => m.LogTime));
        }

        public IActionResult SysLogs()
        {
            var u = GetUserSelf();

            if (!(u.MyRoleIds.Contains(1) || u.CustomerId == 5551))
            {
                return View("NoAccess");
            }

            return View("~/Views/Sys/Logs/SysLogs.cshtml");
        }

        public IActionResult MainLogs(string searchText, string logLevel, string TimeRange = null, int pageIndex = 1, int pageSize = 20)
        {
            var u = GetUserSelf();

            if (!(u.MyRoleIds.Contains(1) || u.CustomerId == 5551))
            {
                return View("NoAccess");
            }

            DateTime? start = null;
            DateTime? end = null;

            base.initialData_TimeRange(ref start, ref end, 1, timeRange: TimeRange);

            var (List, Count) = _mongoLog.GetBy(searchText, logLevel, start, end, pageIndex, pageSize);

            return PartialView("~/Views/Sys/Logs/_pvMainLogs.cshtml", new PaginatedItemsViewModel<ChisLog>(pageIndex, pageSize, Count, List));
        }

        public IActionResult LogDetail(string id)
        {
            var u = GetUserSelf();

            if (!(u.MyRoleIds.Contains(1) || u.CustomerId == 5551))
            {
                return View("NoAccess");
            }

            var model = _mongoLog.Get(id);

            return PartialView("~/Views/Sys/Logs/_pvLogDetail.cshtml", model);
        }

    }
}
