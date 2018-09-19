using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ah.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
           // return RedirectToAction("CustomerLogin", "Home",new { area = "Customer" });
            return View();
        }

        public IActionResult About()
        {
            //var model =  DbContextSqlServer.CHIS_Code_Customer;    
            ////var model = "";  
            //return View(model);
            return View();
        }

        public IActionResult Contact()
        {
            //var model = DbContextMySql.CHIS_Code_Customer.Where(m=>!string.IsNullOrWhiteSpace(m.IDcard));
            ////var model = "";
            //return View(model);
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
