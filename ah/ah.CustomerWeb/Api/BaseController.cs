using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ah.DbContext;

namespace ahWeb.Api
{
    [Produces("application/json")]
    public class BaseDBController : ah.Controllers.BaseController
    {
         public BaseDBController(AHMSEntitiesSqlServer db) : base(db) { }
    }

    [Produces("application/json")]
    public class BaseController :Controller
    {
        
    }
}