using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CHIS.Api
{
    [Produces("application/json")]
    [ApiExplorerSettings(IgnoreApi =true)]
    public class BaseDBController : CHIS.Controllers.BaseController
    {
        public BaseDBController(DbContext.CHISEntitiesSqlServer db) : base(db) { }

        /// <summary>
        /// Api×¨ÓÃµÄPartialView
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public PartialViewResult ApiPartialView(string viewName,object model)
        {
            var cn = RouteData.Values["controller"].ToString();          
            var vn = $"~/ApiViews/{cn}/{viewName}.cshtml";
            return PartialView(vn, model);
        }
    }

    [Produces("application/json")]
    public class BaseController :Controller
    {
        
    }
}