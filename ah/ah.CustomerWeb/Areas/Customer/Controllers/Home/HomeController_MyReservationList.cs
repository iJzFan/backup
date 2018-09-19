using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ah.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class HomeController 
    {
    
        public IActionResult MyReservationList()
        {
            var model= GetCurrentLoginUser;
           // ViewData["Title"] = "个人中心/我的预约";
            ViewData["CustomerId"]= model.CustomerId;
            ViewData["CustomerName"] = model.CustomerName;
            return View();
        }

         
 
    }
}
