using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CHIS.Controllers
{
    public partial class ExamplesController : BaseController
    {
        public ExamplesController(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        [AllowAnonymous]
        public IActionResult Index()
        {

            return View();
        }

        [AllowAnonymous]
        public IActionResult SingleSelector()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult InputControls()
        {
            var model = new CHIS.Models.ViewModel.InputControlsDataModel
            {
                DoctorId = 29,
                DoctorId_r = 29,
                DoctorId_d = 29,
                IsMenu = true,
                IsMenu_d = true,
                IsMenu_r = true,
                IsSwitch2 = false,
                IsSwitch2_d = false,
                IsSwitch2_r = true,

                Is3Status = null,
                Is3Status_d = false,
                Is3Status_r = true,

                Is3Status1 = false,
                Is3Status1_d = true,
                Is3Status1_r = true,





                Name = "输入文字这里",
                KeyName="IsMod",
                KeyName_d="IsNew",
                KeyName_r="IsDel",
                IntNumber = 856,
                Remark = "描述文字",
                FormTypeId = 12794,
                FormTypeId_d = 12794,
                FormTypeId_r = 12794,
                FormTypeId2 = 12794,
                FormTypeId2_d = 12794,
                FormTypeId2_r = 12794,

                StopDate = DateTime.Now.AddYears(-5),
                StopDate_d = DateTime.Now.AddYears(-5),
                StopDate_r = DateTime.Now.AddYears(-5),

                StopDate2 = DateTime.Now.AddYears(-9),
                StopDate2_d = DateTime.Now.AddYears(-9).AddMinutes(56.32),
                StopDate2_r = DateTime.Now.AddYears(-9).AddMinutes(98.3),



                AreaId=24,
                AreaId_d=25,
                AreaId_r=23
            };
            return View(model);
        }
    }
}
