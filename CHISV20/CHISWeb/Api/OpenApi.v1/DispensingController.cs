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
    public class DispensingController : OpenApiBaseController
    {
        Services.DispensingService _dispSvr;


        public DispensingController(DbContext.CHISEntitiesSqlServer db
            , Services.DispensingService dispSvr
            ) : base(db)
        {
            _dispSvr = dispSvr;
        }

        //=========================================================================================



        #region 获取物流信息

        /// <summary>
        /// 自动发药
        /// </summary>
        /// <param name="payOrderId">支付单号</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SendAllDrugsByPayOrderId(string payOrderId)
        {
            try
            {
                var rlt = await _dispSvr.DispenseAllDrugsByPayOrderId(payOrderId);
                return Ok(MyDynamicResult(true,"发药成功"));
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex));}
        }


        #endregion


    }
}
