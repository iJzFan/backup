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
        public IActionResult FunctionTest()
        {

           var finds= _db.vwCHIS_Doctor_OnOffDutyData.Where(m => (m.ScheduleDate - DateTime.Now).Days == 0);
            return View();
        }
        
        [AllowAnonymous]
        public IActionResult MyTree()
        {
            return View();
        }

    }
}
 