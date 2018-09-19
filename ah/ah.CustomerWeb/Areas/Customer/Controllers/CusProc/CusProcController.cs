using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ah.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Authorization;
using ah.Models.ViewModel;
using System.Security.Claims;
using ah.Models.DataModel;
using ah.Models;
using ah.Areas.Customer.Controllers.Base;
using Microsoft.EntityFrameworkCore;
using Ass;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class CusProcController : BaseController
    {
        public CusProcController(ah.DbContext.AHMSEntitiesSqlServer db) : base(db) { }
    public IActionResult QuesProc(Guid qid)
        {
            var find = MainDbContext.AHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == qid);
            find.CustomerId = GetCurrentLoginUser.CustomerId;
            MainDbContext.SaveChanges();
            //清理cookie
            HttpContext.Response.Cookies.Delete("cusName");
            HttpContext.Response.Cookies.Delete("cusMobile");
            HttpContext.Response.Cookies.Delete("cusEmail");
            HttpContext.Response.Cookies.Delete("callBack");
            HttpContext.Response.Cookies.Delete("actionType");
            return RedirectToAction("Index", "MyHealthMgr");            
        }
    }
}
