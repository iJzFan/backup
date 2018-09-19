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
    public class UtilsController : OpenApiBaseController
    {
        Services.JKWebNetService _jkSvr;
        public UtilsController(DbContext.CHISEntitiesSqlServer db
            , Services.JKWebNetService jkSvr
            ) : base(db)
        {
            _jkSvr = jkSvr;
        }

        //=========================================================================================



        #region 获取区域数据
        /// <summary>
        /// 获取区域数据
        /// </summary>
        /// <param name="parentId">上级区域Id，0则为顶级省份</param>
        [ResponseCache(Duration = 3600, VaryByQueryKeys = new string[] { "parentId" })]
        [HttpGet]
        public IActionResult JetAreas(int parentId = 0)
        {
            try
            {
                var list = _db.SYS_ChinaArea.Where(m => m.ParentAreaId == parentId).OrderBy(m => m.AreaLevelShort);
                if (list == null || list.Count() == 0) throw new Exception("没有发现城市");
                var d = MyDynamicResult(true, "");
                d.items = list;
                return Json(d);
            }
            catch (Exception ex) { return Json(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 根据areaId获取到区域具体信息
        /// </summary>
        /// <param name="areaId">区域Id</param>
        [ResponseCache(Duration = 3600, VaryByQueryKeys = new string[] { "areaId" })]
        [HttpGet]
        public IActionResult JetAreasId(int areaId)
        {
            try
            {
                var area2 = _db.SYS_ChinaArea.AsNoTracking().FirstOrDefault(m => m.AreaId == areaId);
                var area1 = _db.SYS_ChinaArea.AsNoTracking().FirstOrDefault(m => m.AreaId == area2.ParentAreaId);

                var provincs = _db.SYS_ChinaArea.AsNoTracking().Where(m => m.ParentAreaId == 0);
                var citys = _db.SYS_ChinaArea.AsNoTracking().Where(m => m.ParentAreaId == area1.ParentAreaId);
                var towns = _db.SYS_ChinaArea.AsNoTracking().Where(m => m.ParentAreaId == area2.ParentAreaId);

                var d = MyDynamicResult(true, "");
                d.level0Id = area1.ParentAreaId;
                d.level1Id = area2.ParentAreaId;
                d.level2Id = area2.AreaId;
                d.level2MergerName = area2.MergerName;
                d.provinces = provincs;
                d.citys = citys;
                d.towns = towns;
                return Json(d);

            }
            catch (Exception ex) { return Json(MyDynamicResult(ex)); }
        }

        #endregion


        #region 获取物流信息

        /// <summary>
        /// 获取订单物流信息
        /// </summary>
        /// <param name="supplierId">厂家</param>
        /// <param name="supplierOrderNo">厂商的订单号</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult HtmGetLogisticsStatus(int supplierId, string supplierOrderNo)
        {
            try
            {
                bool rlt = _jkSvr.Singel_QueryLogistics(supplierOrderNo, out string logisticsList);
                if (rlt) return Content("<ul><li>搜索物流信息失败</li></ul>");
                StringBuilder b = new StringBuilder();
                b.Append("<ul>");
                b.AppendFormat("<li>{0}</li>");
                b.Append("</ul>");
                return Content(b.ToString(),"text/html");
            }
            catch (Exception ex) { return Content("<ul><li>错误:" + ex.Message + "</li></ul>"); }
        }


        #endregion


    }
}
