using CHIS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHIS.BllCaller
{
    public class RolesBllCaller : BaseBllCaller
    { 
         


        /// <summary>
        /// 医生在某工作站的角色
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public IQueryable<RoleItem> GetRolesOfDoctorInStation(int stationId, int doctorId)
        {         
            var db = CHISDbContext;
            //工作站允许角色
            var roleids = db.CHIS_Sys_Rel_WorkStationRoles.AsNoTracking().Where(m => m.StationId == stationId).Select(m => m.RoleId.Value).ToList().Distinct();
            //我在该工作站允许的角色
            var selRoleIds = db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.StationId == stationId && m.DoctorId == doctorId && m.RoleId > 0 && m.MyRoleIsEnable == true).Select(m => m.RoleId.Value);
            var allIds = roleids.Intersect(selRoleIds);
            //允许角色表
            var roles = db.CHIS_SYS_Role.AsNoTracking().Where(m => allIds.Contains(m.RoleID)).Select(m => new RoleItem
            {
                RoleId = m.RoleID,
                RoleKey = m.RoleKey,
                RoleName = m.RoleName
            });         
            return roles;
        }




    }
}
