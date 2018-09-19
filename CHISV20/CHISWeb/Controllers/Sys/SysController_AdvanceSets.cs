using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CHIS.Code;
using CHIS.Models;
using CHIS.Codes.Utility;
using Ass;
using System.Threading.Tasks;
using CHIS.Code.MyExpands;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CHIS.Controllers.Sys
{

    public partial class SysController : BaseController
    {
        public SysController(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        public IActionResult AdvanceSets()
        {
            ViewBag.DBConnString = base.SqlDbConnectionString;
            return View();
        }


        /// <summary>
        /// 清理静态缓存数据
        /// </summary>
        public IActionResult ClearStaticData()
        {
            return TryCatchFunc(() =>
            {
                Global.ClearTempData();
                Global.Initial().Wait(Global.WAIT_MSEC);
                return null;
            });
        }
        /// <summary>
        /// 设置区域的Js文件数据
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SetAreaJsFile()
        {
            try
            {
                var filename = "data-china-area.js";
                StringBuilder bs = new StringBuilder();
                var jn = "";
                var bdb = Global.db_ChinaArea;
                var areas = bdb.Where(m => m.AreaLevel == 1).OrderBy(m => m.AreaLevelShort).Select(m => new CHIS.Models.AddrArea
                {
                    AreaId = m.AreaId,
                    AreaName = m.Name,
                    PinYin = m.PinYin,
                    MergerName = m.MergerName,
                    ParentAreaId=m.ParentAreaId.Value,
                    Children = bdb.Where(a => a.AreaLevel == 2 && a.ParentAreaId == m.AreaId).OrderBy(a => a.AreaLevelShort).Select(a => new CHIS.Models.AddrArea
                    {
                        AreaId = a.AreaId,
                        AreaName = a.Name,
                        PinYin = a.PinYin,
                        MergerName = a.MergerName,
                        ParentAreaId = a.ParentAreaId.Value,
                        Children = bdb.Where(b => b.AreaLevel == 3 && b.ParentAreaId == a.AreaId).OrderBy(b => b.AreaLevelShort).Select(b => new CHIS.Models.AddrArea
                        {
                            AreaId = b.AreaId,
                            ParentAreaId = b.ParentAreaId.Value,
                            AreaName = b.Name,
                            PinYin = b.PinYin,
                            MergerName = b.MergerName,
                        })
                    })
                });
                jn = areas.ToJson();
                var ids = Global.db_ChinaArea.Select(m => new { Id=m.AreaId,PId=m.ParentAreaId });
                bs.Append("var CHINAAREA=" + jn + ",CHINAAREAIDS="+ids.ToJson()+";");
                var path = System.IO.Path.Combine("wwwroot", "js", filename);
                await System.IO.File.WriteAllTextAsync(path, bs.ToString());
                return TryCatchFunc(() => { return null; });
            }
            catch (Exception ex) { return TryCatchFunc(() => { throw ex; }); }
        }

    }
}
