using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS
{


    /// <summary>
    /// 项目基本公共数据类
    /// </summary>
    public class BasicDataHelper
    {        
        static CHIS.DbContext.CHISEntitiesSqlServer MainDbContext = new CHIS.Code.Utility.DataBaseHelper().GetMainDbContext();

        #region 科室信息

        /// <summary>
        /// 获取科室信息 - 前端SelectListItem
        /// </summary>
        public static IEnumerable<SelectListItem> GetDepartmentListItem(int stationId, DepartSelectTypes deptType = DepartSelectTypes.All, bool isEnabled = true)
        {           
            return GetDepartment(stationId,deptType,isEnabled).Select(m =>
                        new SelectListItem
                        {
                            Text = m.DepartmentName,
                            Value = m.DepartmentID.ToString()
                        });
        }

        /// <summary>
        /// 获取科室信息
        /// </summary>
        public static IQueryable<Models.CHIS_Code_Department> GetDepartment(int stationId, DepartSelectTypes deptType = DepartSelectTypes.All, bool isEnabled = true)
        {
            var finds = MainDbContext.CHIS_Code_Department.Where(m => m.StationID == stationId && m.IsEnable == isEnabled);
            switch (deptType)
            {
                case DepartSelectTypes.TreatmentDepartments:
                    finds = finds.Where(m => m.IsNotTreatDept != true); break;
                case DepartSelectTypes.NotTreatmentDepartments:
                    finds = finds.Where(m => m.IsNotTreatDept == true); break;
            }
            return finds;
        }

        #endregion

    }
}
