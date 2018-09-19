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
         
        public IActionResult AddRxDoctor(int doctorId)
        {
            return TryCatchFunc(() =>
            {
                _docSvr.AddRxDoctor(UserSelf.StationId, doctorId);
                return null;
            });
        }

        public IActionResult GetMyRxDoctors()
        {
            var model = _docSvr.GetMyRxDoctors(UserSelf.StationId);
            return PartialView("MyConfig_RxDoctors",model);
        }
        public IActionResult DelRxDoctor(int doctorId)
        {
            return TryCatchFunc(() =>
            {
                _docSvr.DeleteRxDoctor(UserSelf.StationId, doctorId);
                return null;
            });
        }
        public IActionResult SetDefRxDoctor(int doctorId)
        {
            return TryCatchFunc(() =>
            {
                _docSvr.SetRxDoctorIsDefault(UserSelf.StationId, doctorId);
                return null;
            });
        }
    }
}
