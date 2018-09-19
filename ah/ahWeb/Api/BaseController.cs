using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ahWeb.Api
{
    [Produces("application/json")]
    public class BaseDBController : ah.Controllers.BaseController
    {
         
    }

    [Produces("application/json")]
    public class BaseController :Controller
    {
        
    }
}