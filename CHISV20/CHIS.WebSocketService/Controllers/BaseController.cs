using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CHIS.WebSocketService.Controllers
{
    public class BaseController : Controller
    {

        #region 属性
        /// <summary>
        /// 获取客户端IP
        /// </summary>
        public string ClientIP
        {
            get
            {
                var ip0 = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault().ToString().Split(',')[0];
                if (!string.IsNullOrEmpty(ip0)) return ip0;           
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
        #endregion
    }
}