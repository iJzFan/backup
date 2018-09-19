using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CHIS;
using System.Security.Claims;
using CHIS.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Ass;
using CHIS.Models.ViewModel;
using System.Text;

namespace CHIS.Controllers
{
    public partial class MyPanelController
    {


        public IActionResult MyConfig()
        {
            var finds = _db.CHIS_Sys_MyConfig.AsNoTracking().Where(m => m.StationId == UserSelf.StationId && m.DoctorId == UserSelf.DoctorId).ToList();
            var doctors = _docSvr.GetMyRxDoctors(UserSelf.StationId);
            var model = new Models.DataModel.MyConfig(finds, doctors);
            return View(model);
        }

        public IActionResult SetMyConfig(string secKey, string secVal)
        {
            try
            {
                var find = _db.CHIS_Sys_MyConfig.FirstOrDefault(m => m.DoctorId == UserSelf.DoctorId && m.StationId == UserSelf.StationId && m.SectionKey == secKey);
                if (find == null)
                {
                    var addm = _db.Add(new CHIS_Sys_MyConfig
                    {
                        DoctorId = UserSelf.DoctorId,
                        StationId = UserSelf.StationId,
                        SectionKey = secKey,
                        SectionValue = secVal
                    });
                }
                else
                {
                    find.SectionValue = secVal;
                    _db.Update(find);
                }
                _db.SaveChanges();
                return Json(MyDynamicResult(true, ""));
            }
            catch (Exception ex)
            {
                return Json(MyDynamicResult(ex));
            }

        }

         

    }
}
