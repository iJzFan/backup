using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHIS.Models.ViewModel
{

    /// <summary>
    /// 医生基本信息
    /// </summary>
    public class DoctorBasicInfo
    {
        public int DoctorId { get; set; }

    }


    public class DoctorInfo
    {
        public vwCHIS_Code_Doctor Doctor { get; set; }
        public CHIS_Code_Customer Customer { get; set; }


        public List<DoctorStationInfo> StationInfos { get; set; }


        public void InitialStationInfo(List<vwCHIS_Code_Rel_DoctorDeparts> departs, List<CHIS_Sys_Rel_DoctorStationRoles> stationRoles, List<RoleItem> roleitems, List<CHIS_Code_WorkStation> stations)
        {
            stations = stations.OrderBy(m => m.IsCanTreat).ToList();
           
            if (StationInfos == null) StationInfos = new List<DoctorStationInfo>();
            //添加工作站和权限内容 科室
            foreach (var station in stations)
            {
                var roleids = stationRoles.Where(m => m.StationId == station.StationID&&m.MyStationIsEnable==true&&m.MyRoleIsEnable==true).Select(m => m.RoleId.Value);
                StationInfos.Add(new DoctorStationInfo
                {
                    StationId = station.StationID,
                    StationName = station.StationName,
                    Station=station,
                    Roles = roleitems.Where(m => roleids.Contains(m.RoleId)).ToList(),
                    Departs = departs.Where(m => m.StationID == station.StationID&&m.IsVerified==true).ToList()
                });
            } 
        }


    }
    public class DoctorStationInfo
    {
        public int StationId { get; set; }
        public string StationName { get; set; }
        public CHIS_Code_WorkStation Station { get; set; }

        public string RolesOfStation
        {
            get
            {
                if (Roles == null) return "";
                return string.Join(",", this.Roles.Select(m => m.RoleName));
            }
        }

        /// <summary>
        /// 所在工作站的权限
        /// </summary>
        public List<RoleItem> Roles { get; set; }

        public string DepartsString
        {
            get
            {
                if (this.Departs == null) return "";
                return string.Join(",", this.Departs.Select(m => m.DepartmentName));
            }
        }

        /// <summary>
        /// 所在的科室
        /// </summary>
        public List<vwCHIS_Code_Rel_DoctorDeparts> Departs { get; set; }
    }
     
}
