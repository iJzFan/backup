using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass.Models;
using Ass;
using System.Collections.Generic;
using CHIS.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    public partial class DoctorController
    {

        public IActionResult MyTreatList()
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
        public IActionResult LoadMyTreatList(string searchText, string treatStatus = "ALL", string stationWhich = "ALL", string dctrBuzType = "ALL", int? stationId = null, int? doctorId = null, string TimeRange = "Today", int pageIndex = 1, int pageSize = 20)
        {
            DateTime? dt0 = null; DateTime? dt1 = null;
            base.initialData_TimeRange(ref dt0, ref dt1, 1, timeRange: TimeRange);
            stationId = stationId ?? UserSelf.StationId;
            doctorId = doctorId ?? UserSelf.DoctorId;
            var model = _treatSvr.GetTreatList(searchText, dt0.Value, dt1.Value, stationId.Value, doctorId.Value, treatStatus, stationWhich, dctrBuzType, pageIndex, pageSize);
            return PartialView("_pvMyTreatList", model);
        }

        public IActionResult MyTreatInfo(long registId, long? treatId)
        {
            var u = UserSelf;

            treatId = treatId ?? (_db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.RegisterID == registId)?.TreatId);
            var model = _treatSvr.GetTreatDetail(treatId.Value);
            return PartialView("_pvMyTreatInfo", model);
        }




        public IActionResult SetTreated(long treatId)
        {
            return TryCatchFunc(() =>
            {
                var m = _db.CHIS_DoctorTreat.Find(treatId);
                if (m.FstDiagnosis.HasValue) m.TreatStatus = 2;
                else throw new Exception("没有标记诊断内容,请先接诊");
                _db.SaveChanges();
                return null;
            });
        }




    }
}