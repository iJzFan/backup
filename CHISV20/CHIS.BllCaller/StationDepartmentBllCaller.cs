using CHIS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHIS.BllCaller
{
    public class StationDepartmentBllCaller : BaseBllCaller
    {
        public IQueryable<vwCHIS_Code_WorkStation> GetSonStations(int pStationId, bool bWithRoot, bool bNotMedicalUnit)
        {
            var db = CHISDbContext;
            var rlt = db.vwCHIS_Code_WorkStation.AsNoTracking();
            rlt = bWithRoot ? rlt.Where(m => m.ParentStationID == pStationId || m.StationID == pStationId) :
                              rlt.Where(m => m.ParentStationID == pStationId);
            rlt = rlt.Where(m => m.IsNotMedicalUnit == bNotMedicalUnit);
            return rlt;
        }

        public IQueryable<vwCHIS_Code_WorkStation> TreatStationOfSearch(string searchText)
        {
            var db = CHISDbContext;
            return from item in db.vwCHIS_Code_WorkStation.AsNoTracking()
                   where string.IsNullOrWhiteSpace(searchText) ? false : item.StationName.Contains(searchText) &&
                   item.IsEnable == true && item.IsManageUnit != true
                   select item;
        }

        /// <summary>
        /// /api/syshis/DepartsOfStation
        /// 获取工作站对应的科室信息 通常只返回接诊部门
        /// </summary>                
        /// <param name="bAllDepart">true 返回所有部门 false:返回接诊部门</param>
        public IQueryable<vwCHIS_Code_Department> DepartsOfStation(int stationId, bool bAllDepart)
        {
            var db = CHISDbContext;
            var rlt = db.vwCHIS_Code_Department.AsNoTracking().Where(m => m.StationID == stationId);
            if (bAllDepart == false) rlt = rlt.Where(m => m.IsNotTreatDept != true);
            return rlt;
        }

         

 




    }
}
