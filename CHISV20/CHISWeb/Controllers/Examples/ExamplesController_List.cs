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
        [AllowAnonymous]
        public IActionResult List()
        {

            return View();
        }

        [HttpPost]
        public IActionResult List(IEnumerable<string> arr)
        {
            return View();
        }


        [HttpPost]
        public IActionResult ReturnView()
        {
            var pv = PartialView("_pvTest");
            pv.ExecuteResult(this.ControllerContext);
            var s = this.HttpContext.Response.ToString();

            return Json(new
            {
                rlt = true,
                msg = "chengong",
                html = pv,
                s = s
            });

        }
    }
}
