using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ass;
using ah.Areas.Customer.Controllers.Base;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class HomeController : BaseController
    {
        public IActionResult MyFollowList()
        {
            return View();
        }
        public IActionResult MyFollowRecantList()
        {
            return View();
        }
    }
}
