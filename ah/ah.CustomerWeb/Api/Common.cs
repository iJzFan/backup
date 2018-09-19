using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ah.Models;
using ah.DbContext;

namespace ahWeb.Api
{
    public class Common : BaseDBController
    {
        public Common(AHMSEntitiesSqlServer db) : base(db) { }

        public int LoadBaseCalendarDat(int year, int month)
        {
            return 1;

        }
        public IEnumerable<Ass.Models.BaseCalendarData> LoadBaseCalendarData(int year, int month)
        {
            List<Ass.Models.BaseCalendarData> rtn = new List<Ass.Models.BaseCalendarData>();
            DateTime firstDay = new DateTime(year, month, 1);
            for (int i = 0; i < (int)firstDay.DayOfWeek; i++)
            {
                rtn.Add(null);
            }
            DateTime endDay = firstDay.AddMonths(1);
            for (DateTime dt = firstDay; dt < endDay; dt = dt.AddDays(1))
            {
                var LunlarDate = new Yi.ChineseCalendar(dt);
                rtn.Add(new Ass.Models.BaseCalendarData()
                {
                    Date = dt,
                    LunlarString = LunlarDate.ChineseDayString,
                    TermString = LunlarDate.ChineseTwentyFourDay?.TermString
                });
            }
            return rtn;
        }

        public IEnumerable<ah.Models.ViewModel.CalendarData> LoadCalendarData(int year, int month, int customerId)
        {
            string ym = string.Format("{0:0000}{1:00}", year, month);
            var finds = MainDbContext.vwCHIS_Register.AsNoTracking();
            finds = from item in finds where item.RegisterDate.HasValue && item.RegisterDate.Value.ToString("yyyyMM") == ym && item.CustomerID == customerId select item;
            var lst = finds.ToList();

            var cdt = LoadBaseCalendarData(year, month);
            var rtn = from item in cdt
                      select item == null ? null : new ah.Models.ViewModel.CalendarData
                      {
                          Date = item.Date,
                          LunlarString = item.LunlarString,
                          TermString = item.TermString,
                          RegisterItems = lst.Where(m => m.RegisterDate.Value.Date == item.Date).Select(m => new ah.Models.ViewModel.CustomerRegisterItem(m))
                      };

            return rtn;
        }

        public IEnumerable<dynamic> LoadRegisterOfDate(DateTime dt, int? customerId)
        {
            var finds = MainDbContext.vwCHIS_Register.AsNoTracking().Where(m => m.RegisterDate.HasValue && m.RegisterDate.Value.Date == dt.Date && m.CustomerID == customerId);
            if (customerId > 0) finds = finds.Where(m => m.CustomerID == customerId);
            var ff = finds.ToList().OrderBy(m => m.RegisterSlot).Select(m => new
            {
                stationName = m.StationName,
                departmentName = m.DepartmentName,
                employeeName = m.DoctorName,
                CustomerName = m.CustomerName,
                registerDate = m.RegisterDate.Value.ToString("yyyy-MM-dd"),
                SlotName = ah.Code.PrjHelper.TransSlot(m.RegisterSlot),
                TreatStatusCode = ah.Models.ViewModel.CustomerRegisterItem._setStatus(m),
                TreatStatus = ah.Code.PrjHelper.TransTreatStatus(ah.Models.ViewModel.CustomerRegisterItem._setStatus(m)),
                RegisterFromName = m.RegisterFromName
            });
            return ff;
        }


        #region 获取区域数据
        /// <summary>
        /// 获取区域数据
        /// </summary>
        /// <param name="parentId">上级区域Id，0则为顶级省份</param>
        [ResponseCache(Duration = 3600)]
        [AllowAnonymous]
        public IActionResult GetAreas(int parentId = 0)
        {
            try
            {
                var list = MainDbContext.SYS_ChinaArea.Where(m => m.ParentAreaId == parentId).OrderBy(m => m.AreaLevelShort);
                if (list == null || list.Count() == 0) throw new Exception("没有发现城市");
                return Json(new { rlt = true, items = list });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        [AllowAnonymous]
        public IActionResult GetAreasId(int areaId)
        {
            try
            {
                var area2 = MainDbContext.SYS_ChinaArea.AsNoTracking().FirstOrDefault(m => m.AreaId == areaId);
                var area1 = MainDbContext.SYS_ChinaArea.AsNoTracking().FirstOrDefault(m => m.AreaId == area2.ParentAreaId);

                var provincs = MainDbContext.SYS_ChinaArea.AsNoTracking().Where(m => m.ParentAreaId == 0);
                var citys = MainDbContext.SYS_ChinaArea.AsNoTracking().Where(m => m.ParentAreaId == area1.ParentAreaId);
                var towns = MainDbContext.SYS_ChinaArea.AsNoTracking().Where(m => m.ParentAreaId == area2.ParentAreaId);


                return Json(new
                {
                    level0Id = area1.ParentAreaId,
                    level1Id = area2.ParentAreaId,
                    level2Id = area2.AreaId,
                    level2MergerName = area2.MergerName,
                    provinces = provincs,
                    citys = citys,
                    towns = towns
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    msg = ex.Message
                });
            }

        }

        #endregion


         

    }
}