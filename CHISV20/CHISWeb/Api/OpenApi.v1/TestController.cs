using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 通用工具
    /// </summary>
    public class TestController : OpenApiBaseController
    {
        Services.JKWebNetService _jkSvr;
        public TestController(DbContext.CHISEntitiesSqlServer db
            , Services.JKWebNetService jkSvr
            ) : base(db)
        {
            _jkSvr = jkSvr;
        }

        //=========================================================================================


        [HttpGet]
        public  dynamic GetDbTime()
        {
            return _db.GetDBTime();
        }


    }
}
