using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class RolesAndDepartsOfStation
    {
        public int StationId { get; set; }
        public List<RoleItem> Roles { get; set; }
        public List<vwCHIS_Code_Department> Departs { get; set; }

        /// <summary>
        /// 选择的角色
        /// </summary>
        public List<int> SelectedRoles { get; set; }
        /// <summary>
        /// 选择的部门
        /// </summary>
        public List<int> SelectedDeparts { get; set; }
    }

    public class StationRolesDepartsItem
    {
        public int StationId { get; set; }
        public IEnumerable<int> Roles { get; set; }
        public IEnumerable<int> Departs { get; set; }
    }


    public class DepartInfoMin
    {
        public int DepartId { get; set; }
        public string DepartmentName { get; set; }
    }
    public class StationInfoMin
    {
        public int StationId { get; set; }
        /// <summary>
        /// 工作站名
        /// </summary>
        public string StationName { get; set; }
    }
    public class StationInfo
    {
        public int StationId { get; set; }
        /// <summary>
        /// 工作站名
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public double? Lng { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string StationAddress { get; set; }

        /// <summary>
        /// 工作站横图
        /// </summary>
        public string StationPicHUrl { get; set; }
        /// <summary>
        /// 工作站纵向图
        /// </summary>
        public string StationPicVUrl { get; set; }
        /// <summary>
        /// 工作站介绍
        /// </summary>
        public string StationRmk { get; set; }


        string diffOfMe = "";
        // [JsonIgnore]
        /// <summary>
        /// 距离多少米
        /// </summary>
        public string DiffOfMe
        {
            get
            {
                if (string.IsNullOrEmpty(diffOfMe))
                    diffOfMe = DiffOfMeVal.ToString("#0.#米");
                return diffOfMe;
            }
            set { diffOfMe = value; }
        }
        [JsonIgnore]
        public double DiffOfMeVal { get; set; }
    }
    public class vwWorkStation
    {

        public int StationId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? ParentStationId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationName { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationAddress { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string Telephone { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string Fax { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string LegalPerson { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? AreaID { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? AgentID { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public bool IsEnable { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public DateTime? StopDate { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? ShowOrder { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? OpID { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string OpMan { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public DateTime? OpTime { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string Remark { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string ParentStationName { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string AddressInfo { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public bool IsCanTreat { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public bool IsNetPlat { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public bool IsManageUnit { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public bool IsTestUnit { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationPicVUrl { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationRmk { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationPicHUrl { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? ZipCode { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public double? StationLng { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public double? StationLat { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int HotNum { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public double? Lat { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public double? Lng { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public bool IsNotMedicalUnit { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public byte[] SickStampUri { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public int? StationTypeId { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationTypeName { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationLogPicH { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationLogPicV { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationKeyCode { get; set; }

        /// <summary> 
        /// 
        /// </summary>		

        public string StationPinYin { get; set; }
    }
    public class StationInfo2
    {
        public int StationId { get; set; }
        /// <summary>
        /// 工作站名
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public double? Lng { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string StationAddress { get; set; }

        /// <summary>
        /// 工作站横图
        /// </summary>
        public string StationPicHUrl { get; set; }
        /// <summary>
        /// 工作站纵向图
        /// </summary>
        public string StationPicVUrl { get; set; }
        /// <summary>
        /// 工作站介绍
        /// </summary>
        public string StationRmk { get; set; }
         public string StationTypeName { get; set; }
        public int? StationTypeId { get; set; }
    }


}
