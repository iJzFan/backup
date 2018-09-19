using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ass;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 工作站工具
    /// </summary>
    public class StationController : OpenApiBaseController
    {
        Services.WorkStationService _staSvr;
        public StationController(DbContext.CHISEntitiesSqlServer db
            , Services.WorkStationService staSvr
            ) : base(db)
        {
            _staSvr = staSvr;
        }

        //=========================================================================================


        /// <summary>
        /// 获取工作站
        /// </summary>
        /// <param name="stationIds">Id连接字符串，逗号隔开,结果按照Id先后排序</param>
        [HttpGet]
        public dynamic JetTreatStationsByIds(string stationIds)
        {
            try
            {
                var finds = _staSvr.Find(stationIds.ToList<int>(',')).Select(m => new
                {
                    StationId = m.StationID,
                    m.StationName,
                    StationImgUrl = m.StationPicH.ahDtUtil().GetStationImg(imgSizeTypes.HorizThumb)
                });
                var d = MyDynamicResult(true, "");
                d.items = finds;
                return d;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }





    }
}
