using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Controllers
{

    /// <summary>
    /// 药品申请
    /// </summary>
    [Route("/Pharmacy/[controller]/[action]")]
    public class DrugSuppleController: BaseController
    {
        public readonly string baseViewRoot="~/Views/Pharmacy/DrugSupple/";
        public DrugSuppleController(DbContext.CHISEntitiesSqlServer db) : base(db)
        {

        }

        /// <summary>
        /// 诊所药品申请主页
        /// </summary>
        /// <returns></returns>
        public IActionResult DrugRequest()
        {
            return View(baseViewRoot + "DrugRequest/Index.cshtml");
        }
        /// <summary>
        /// 仓库药品申请的处理首页
        /// </summary>
        /// <returns></returns>
        public IActionResult DrugResponse()
        {
            return View(baseViewRoot + "DrugRequest/Index.cshtml");
        }

    }
}
