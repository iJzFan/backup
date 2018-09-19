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

namespace CHIS.Controllers.Sys
{
    [Route("sys/[controller]/[action]")]
    public partial class RolesController : BaseController
    {
        public RolesController(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        /// <summary>
        /// 业务功能页面入口
        /// </summary>
        /// <returns></returns>
        public IActionResult StationRoles()
        {
            var model = new CHIS.Api.syshis(_db).AllRolesOfStationCanSelect();
            return View("~/Views/Sys/Roles/StationRolesIndex.cshtml", model);//此处需要特别指明页面文件的位置
        }

        /// <summary>
        /// 保存工作站所选择的角色
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<JsonResult> SaveStationRoles(int stationId, List<int> roles)
        {
            try
            {
                if (stationId <= 0) throw new Exception("没有传入工作站Id");
                _db.BeginTransaction();
                try
                {
                    var finds = _db.CHIS_Sys_Rel_WorkStationRoles.AsNoTracking().Where(m => m.StationId == stationId).ToList();
                    //需要删除的Id
                    foreach (var item in finds)
                    {
                        if (!roles.Contains(item.RoleId ?? 0)) _db.CHIS_Sys_Rel_WorkStationRoles.Remove(item);
                        else roles.Remove(item.RoleId ?? 0);
                    }
                    //需要添加的Id
                    foreach (var roleid in roles) await _db.CHIS_Sys_Rel_WorkStationRoles.AddAsync(new CHIS_Sys_Rel_WorkStationRoles
                    {
                        StationId = stationId,
                        RoleId = roleid
                    });
                    await _db.SaveChangesAsync();
                    _db.CommitTran();
                }
                catch (Exception ex) { _db.RollbackTran(); throw ex; }

                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }


    }
}
