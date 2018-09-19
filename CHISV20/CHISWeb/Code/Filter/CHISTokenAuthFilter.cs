using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CHIS.Code.Filter
{
    public class CHISTokenAuth: Attribute,IActionFilter
    {
        private IConfiguration _config;

        public CHISTokenAuth(IConfiguration config)
        {
            _config = config;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers.Values;


            var scheme = _config.GetSection("CHISToken:Scheme").Value;
            var parameter = _config.GetSection("CHISToken:Parameter").Value;

            StringValues token = new StringValues(scheme + ' ' + parameter);

 
            if (!headers.Contains(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            context.HttpContext.User = new ClaimsPrincipal(new []{new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name, "天使健康会员中心") })});

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

    }
}
