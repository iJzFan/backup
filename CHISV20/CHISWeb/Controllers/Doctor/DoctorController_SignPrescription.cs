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
        /// <summary>
        /// 签名处方单
        /// </summary>
        /// <returns></returns>
        public IActionResult SignPrescriptionList()
        {
            return View(GetViewPath(nameof(SignPrescriptionList)));
        }


        /// <summary>
        /// 载入需要签名的清单
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId">如果DoctorId=0则表示所有，null默认本医生</param>
        /// <param name="TimeRange"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult LoadMyRxSignList(string searchText, string signStatus = "ALL", int? stationId = null, int? doctorId = null, string TimeRange = "Today", int pageIndex = 1, int pageSize = 20)
        {
            DateTime? dt0 = null; DateTime? dt1 = null;
            base.initialData_TimeRange(ref dt0, ref dt1, 1, timeRange: TimeRange);

            stationId = stationId ?? UserSelf.StationId;
            doctorId = doctorId ?? UserSelf.DoctorId;

            var model = _treatSvr.QueryMyRxSignList(searchText, doctorId.Value, dt0, dt1, signStatus, pageIndex, pageSize);

            return PartialView(GetViewPath(nameof(SignPrescriptionList), "_pvMyRxSignList"), model);
        }


    }
}