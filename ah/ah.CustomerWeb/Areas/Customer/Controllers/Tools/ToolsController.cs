using ah.Models.ViewModel;
using ah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using ah.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ah.Areas.Customer.Controllers.Tools
{
    public class ToolsController : QueryController
    {
        [HttpGet("[action]")]
        public IActionResult Address()
        {
            return View();
        }
    }
}
