using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Ass;
using Microsoft.AspNetCore.Authorization;

namespace CHISWeb.Controllers.Tools
{
    public partial class ToolsController : CHIS.Controllers.BaseController
    {
        [AllowAnonymous]
        public IActionResult RegScan()
        {
            //传入的数据
            var stationId = getQuery("s");
            var doctorId = getQuery("d");
            var rxDoctorId = getQuery("rxd");

            //获取扫描用户信息 然后获取数据库内的信息


            //添加一个预约，返回成功的信息

            return View();
        }
    }
}