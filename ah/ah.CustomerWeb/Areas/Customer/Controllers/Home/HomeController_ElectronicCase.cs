using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class HomeController
    {
        // GET: /<controller>/
        [AllowAnonymous]
        public IActionResult ElectronicCase()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult ElectronicDetail()
        {
            return View();
        }
    }
}
