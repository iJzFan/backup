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
            //���������
            var stationId = getQuery("s");
            var doctorId = getQuery("d");
            var rxDoctorId = getQuery("rxd");

            //��ȡɨ���û���Ϣ Ȼ���ȡ���ݿ��ڵ���Ϣ


            //���һ��ԤԼ�����سɹ�����Ϣ

            return View();
        }
    }
}