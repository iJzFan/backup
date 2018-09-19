using CHIS.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.MyExpands
{
    public class MyActionResult:IActionResult
    {
        BaseController _c = null;
        public MyActionResult(BaseController c)
        {
            this._c = c;
        }

        public MyActionResult AddCrossDomainHeader()
        {
            _c.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*"; //允许跨域访问    
            return this;
        }


        public Task ExecuteResultAsync(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
