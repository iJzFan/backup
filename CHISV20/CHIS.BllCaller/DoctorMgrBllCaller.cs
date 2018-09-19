using Ass;
using CHIS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHIS.BllCaller
{
    public class DoctorMgrBllCaller : BaseBllCaller
    {

        public IQueryable<CHIS_Code_WorkStation> StationsOfDoctor(int doctorId, int rootStationId, bool isAllowed)
        {
            var db = CHISDbContext;
            var find = from a in db.CHIS_Code_WorkStation
                       join b in db.CHIS_Sys_Rel_DoctorStations
                       on a.StationID equals b.StationId
                       where (a.IsEnable == true && b.DoctorId == doctorId && b.StationIsEnable == isAllowed)
                       orderby a.ShowOrder
                       select a;
            return find;

        }





        public IQueryable<vwCHIS_Code_Doctor_Authenticate> GetDoctorPendingList(string searchText, DateTime? dt0, DateTime? dt1, bool? isNeedCheck, int? rootStationId)
        {
            var db = CHISDbContext;
            var finds = db.vwCHIS_Code_Doctor_Authenticate.AsNoTracking();
            if (dt0 != null && dt0 != new DateTime()) finds = finds.Where(m => m.DoctorCreateTime >= dt0.Value && m.DoctorCreateTime < dt1.Value);
            if (isNeedCheck.HasValue) finds = finds.Where(m => m.NeedCheck == (isNeedCheck.Value ? 1 : 0));
            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) finds = finds.Where(m => m.Mobile == t.String);
                else if (t.IsEmail) finds = finds.Where(m => m.Email == t.String);
                else if (t.IsIdCardNumber) finds = finds.Where(m => m.IdCardNumber == t.String);
                else finds = finds.Where(m => m.DoctorName == searchText);
            }
            //只能搜索所在工作站及其下属的医生
            var doctorIds = db.CHIS_Sys_Rel_DoctorStations.FromSql($"select * from CHIS_Sys_Rel_DoctorStations where dbo.fn_InStation({rootStationId},StationId)=1").Select(m => m.DoctorId);
            //   var doctorIds = MainDbContext.CHIS_Sys_Rel_DoctorStations.Where(m => stations.Contains(m.StationId) && m.StationIsEnable)
            return finds = finds.Where(m => doctorIds.Contains(m.DoctorId));
        }


    }
}
