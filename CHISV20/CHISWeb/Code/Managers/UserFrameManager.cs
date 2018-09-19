using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ass.Data;
using CHIS.Models;
using Microsoft.EntityFrameworkCore;
using Ass;

namespace CHIS.Code.Managers
{

    public class UserAccessInfo
    {
        #region 登录信息 人员信息
        /// <summary>
        /// 登录信息
        /// </summary>
        public Models.vwCHIS_Sys_Login MyLogin { get; set; }

        /// <summary>
        /// 医生信息
        /// </summary>
        public vwCHIS_Code_Doctor Doctor { get; set; }

        #endregion

        #region 菜单信息
        /// <summary>
        /// 顶级菜单项
        /// </summary>
        public IEnumerable<CHIS_SYS_Function> MyTopMainMenus { get; set; }

        /// <summary>
        /// 选择后的菜单项
        /// </summary>
        public Ass.Data.TreeEntity<Models.CHIS_SYS_Function> MySelectMenuTree { get; set; }


        /// <summary>
        /// 所有的菜单项
        /// </summary>
        public Ass.Data.TreeEntity<Models.CHIS_SYS_Function> AllMenuTree { get; set; }

        /// <summary>
        /// 判断该菜单是否授权允许
        /// </summary>
        /// <param name="funcKey"></param>
        /// <returns></returns>
        public bool IsMenuAllowed(string funcKey)
        {
            return AllMenuTree.HasItems(m => m.ThisItem.FunctionKey == funcKey && m.ThisItem.IsEnable == true);
        }

        /// <summary>
        /// 我的导航功能项
        /// </summary>
        public IEnumerable<Models.CHIS_SYS_Function> MyNavFuncs
        {
            get; set;
        }

        /// <summary>
        /// 我的角色
        /// </summary>
        public IEnumerable<Models.RoleItem> MyRoles { get; set; }

        #endregion

        #region 工作站信息
        public StationInfo MyStationInfo { get; set; }


        #endregion

        /// <summary>
        /// 系统处理时间
        /// </summary>
        public string sysProcessTime = null;


        public int DoctorId { get { return Doctor.DoctorId; } }
        public int StationId { get { return MyStationInfo.Station.StationID; } }
    }

    public class StationInfo
    {

        /// <summary>
        /// 当前工作站信息
        /// </summary>
        public vwCHIS_Code_WorkStation Station { get; set; }
        /// <summary>
        /// 明确设定权限的工作站
        /// </summary>
        public IEnumerable<vwCHIS_Code_WorkStation> AllowedStations { get; set; }

        /// <summary>
        /// 我的子级工作站
        /// </summary>
        public IEnumerable<vwCHIS_Code_WorkStation> MySubStations { get; set; }

        /// <summary>
        /// 明确允许的工作站的权限树
        /// </summary>
        public Ass.Data.TreeEntity<vwCHIS_Code_WorkStationEx> StationsTree { get; set; }

        /// <summary>
        /// 该工作站是否被允许
        /// </summary>
        public bool IsStationAllowed(int stationId)
        {
            return AllowedStations.Where(m => m.StationID == stationId).Count() > 0;
        }
        /// <summary>
        /// 允许的工作站的Id字符串，逗号隔开
        /// </summary>
        public string AllowedStationsString
        {
            get
            {
                return string.Join(",", AllowedStations.Select(m => m.StationID).ToList());
            }
        }


        /// <summary>
        /// 我的允许科室
        /// </summary>
        public IEnumerable<CHIS.Models.vwCHIS_Code_Rel_DoctorDeparts> MyAllDepartments { get; set; }


        /// <summary>
        /// 获取工作站允许的科室
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public IEnumerable<CHIS.Models.vwCHIS_Code_Rel_DoctorDeparts> GetAllDepartsOfStation(int? stationId = null)
        {
            if (!stationId.HasValue) stationId = this.Station?.StationID;
            return MyAllDepartments.Where(m => m.StationID == stationId);
        }

        /// <summary>
        /// 选择的部门，可以为空
        /// </summary>
        public CHIS.Models.vwCHIS_Code_Department SelectedDepartment { get; set; }



    }

    public class UserFrameManager : BaseInject
    {
        public UserSelf UserSelf { get; set; }

        public UserFrameManager(UserSelf ud)
        {
            this.UserSelf = ud;
        }
        public UserFrameManager() { }


        /// <summary>
        /// 获取我的登录数据
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        public vwCHIS_Sys_Login GetMyLoginData(long? loginId = null)
        {
            if (loginId == null) loginId = this.UserSelf.LoginId;
            return MainDbContext.vwCHIS_Sys_Login.AsNoTracking().FirstOrDefault(m => m.LoginId == loginId);
        }

        /// <summary>
        /// 菜单是否授权
        /// </summary>
        /// <param name="menuKey"></param>
        /// <returns></returns>
        public bool IsMenuAllowed(string menuKey)
        {
            var func = MainDbContext.CHIS_SYS_Function.AsNoTracking().FirstOrDefault(m => m.FunctionKey == menuKey);
            if (func == null) return false;
            var roles = MainDbContext.CHIS_Sys_Rel_RoleFunctions.Where(m => m.FunctionId == func.FunctionID).Select(m => m.RoleId);
            return UserSelf.MyRoleIds.Intersect(roles).Count() > 0;//交集内有数据
        }

        public vwCHIS_Code_WorkStation GetMyStation()
        {
            return MainDbContext.vwCHIS_Code_WorkStation.AsNoTracking().FirstOrDefault(m => m.StationID == UserSelf.StationId);
        }



        public IEnumerable<vwCHIS_Code_WorkStation> GetAllowedStations(int doctorId)
        {
            var finds = MainDbContext.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationIsEnable != false).Select(m => m.StationId);
            var allowedStations = MainDbContext.vwCHIS_Code_WorkStation.Where(m => finds.Contains(m.StationID) && m.IsEnable != false).ToList();
            return allowedStations;
        }
        public IQueryable<int> GetAllowedStationsQuery(DbContext.CHISEntitiesSqlServer db, int doctorId)
        {
            var finds = db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.DoctorId == doctorId).Select(m => m.StationId);
            var allowedStations = db.vwCHIS_Code_WorkStation.Where(m => finds.Contains(m.StationID) && m.IsEnable != false).Select(m => m.StationID);
            return allowedStations;
        }

        public TreeEntity<vwCHIS_Code_WorkStationEx> GetStationsTree(IEnumerable<vwCHIS_Code_WorkStation> allowedStations)
        {
            var allowedStationTreeDatas = MainDbContext.vwCHIS_Code_WorkStationEx.FromSql<vwCHIS_Code_WorkStationEx>("exec sp_Common_LoadTreeMap {0},{1},{2},{3}",
               "vwCHIS_Code_WorkStation", "StationID", "ParentStationID", string.Join(",", allowedStations.Select(m => m.StationID).ToList()));

            var allowdStationTree = new Ass.Data.TreeEntity<vwCHIS_Code_WorkStationEx>().Inital(allowedStationTreeDatas.ToList(),
                m => m.ParentStationID.ToString(),
                m => m.StationID.ToString());
            return allowdStationTree;
        }
        public List<int> GetAllowedStationsAndSubStations(int doctorId)
        {
            var allowedStationIds = GetAllowedStations(doctorId).Select(m => m.StationID).ToList();
            var allowedStationTreeDatas = MainDbContext.vwCHIS_Code_WorkStationEx.FromSql<vwCHIS_Code_WorkStationEx>("exec [sp_Common_LoadTreeMapExtends] {0},{1},{2},{3}",
              "vwCHIS_Code_WorkStation", "StationID", "ParentStationID", string.Join(",", allowedStationIds)).Where(m => m.sysIsOriginalNode == 1).Select(m => m.StationID);
            return allowedStationTreeDatas.ToList();
        }
        public IQueryable<int> GetAllowedStationsAndSubStationsQuery(DbContext.CHISEntitiesSqlServer db, int doctorId, int rootStationId, bool bContainThisStation = true, bool bAllStation = true)
        {
            //返回所有诊所的信息
            if (bAllStation)
            {
                var r = db.StationIds.FromSql("exec sp_GetStationAndSubStations {0},{1}", rootStationId, bContainThisStation ? 1 : 0).Select(m => m.StationId);
                return r;
            }

            //返回我所允许的诊所的信息
            var stationids = db.CHIS_Sys_Rel_DoctorStationRoles.Where(m => m.DoctorId == doctorId && m.MyStationIsEnable != false && m.StationId > 0).Select(m => m.StationId.Value);

            var subStations = db.StationIds.FromSql("exec sp_GetStationAndSubStations {0},{1}", rootStationId, bContainThisStation ? 1 : 0);

            var rlt = from item in subStations
                      where stationids.Contains(item.StationId)
                      select item.StationId;
            return rlt;
        }

        public IQueryable<int> GetStations(int stationId, bool isContainMe = true)
        {
            var allowedStationTreeDatas = MainDbContext.CHIS_Code_WorkStation.FromSql<CHIS_Code_WorkStation>("exec sp_Common_LoadTreeRoadToLeafs {0},{1},{2},{3}",
              "CHIS_Code_WorkStation", "StationID", "ParentStationID", stationId).Select(m => m.StationID);
            var rlt = allowedStationTreeDatas;//.ToList();
            if (!isContainMe) rlt.Where(m => m != stationId);// .Remove(stationId);
            return rlt;
        }
        public IQueryable<CHIS_Code_WorkStation> GetStationList(int rootStationId, bool isContainMe = true)
        {
            var allowedStationTreeDatas = MainDbContext.CHIS_Code_WorkStation.FromSql<CHIS_Code_WorkStation>("exec sp_Common_LoadTreeRoadToLeafs {0},{1},{2},{3}",
              "CHIS_Code_WorkStation", "StationID", "ParentStationID", rootStationId);
            var rlt = allowedStationTreeDatas;//.ToList();
            if (!isContainMe) rlt.Where(m => m.StationID != rootStationId);// .Remove(stationId);
            return rlt;
        }

        public IQueryable<vwCHIS_Code_WorkStation> GetSonStations(int? stationId = null, bool bWithRoot = false)
        {
            if (stationId == null) stationId = UserSelf.StationId;
            var finds = MainDbContext.vwCHIS_Code_WorkStation.AsNoTracking();
            if (bWithRoot) return finds.Where(m => m.ParentStationID == stationId || m.StationID == stationId);
            else return finds.Where(m => m.ParentStationID == stationId);
        }

  
        public UserAccessInfo GetUserAccessInfo()
        {
            return GetUserAccessInfo(UserSelf.CustomerId, UserSelf.StationId, UserSelf.SelectedDepartmentId, 0).Result;
        }
        public UserAccessInfo GetUserAccessInfo(int? selectedDepartId, int parentFuncId = 0)
        {
            return GetUserAccessInfo(UserSelf.CustomerId, UserSelf.StationId, selectedDepartId, parentFuncId).Result;
        }
        public async Task<UserAccessInfo> GetUserAccessInfo(long customerId, int stationId, int? selectedDepartId, int parentFuncId = 0)
        {
            DateTime now = DateTime.Now;
            var doctor = await MainDbContext.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefaultAsync(m => m.CustomerId == customerId);
            var station = await MainDbContext.vwCHIS_Code_WorkStation.AsNoTracking().FirstOrDefaultAsync(m => m.StationID == stationId);
            var allowedStations = GetAllowedStations(doctor.DoctorId);
            var allowdStationTree = GetStationsTree(allowedStations);
            var subStation = MainDbContext.vwCHIS_Code_WorkStation.AsNoTracking().Where(m => m.ParentStationID == stationId);
            var myallDeparts = MainDbContext.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctor.DoctorId);
            var selDepart = await MainDbContext.vwCHIS_Code_Department.AsNoTracking().FirstOrDefaultAsync(m => m.DepartmentID == (selectedDepartId ?? 0));

            //获取用户的的角色
            //获取需要授权的菜单
            //var relfinds = MainDbContext.vwCHIS_Sys_Rel_EmployeeStationRoles.Where(m => m.StationId == stationId && m.EmployeeId == doctor.DoctorId &&
            //         m.MyRoleIsEnable != false && m.EmployeeIsEnable != false && m.MyStationIsEnable != false)
            //        .Select(m => m.RoleId).ToList();

            //var funcKeys = MainDbContext.CHIS_Sys_Rel_RoleFunctions.Where(m => relfinds.Contains(m.RoleId)).Select(m => m.FunctionId);
            ////获取不需要授权的用户菜单
            //var normal = from item in MainDbContext.CHIS_SYS_Function
            //             where item.IsEnable && item.IsMenu && !item.IsRight
            //             select item.FunctionID;
            //funcKeys = funcKeys.Concat(normal);

            var sql = @"select RoleID,RoleKey,RoleName from CHIS_SYS_Role where RoleID in (
  select RoleId from [CHIS_Sys_Rel_DoctorStationRoles] where stationid={0} and DoctorId={1})";
            var myRoles = MainDbContext.SqlQuery<RoleItem>(string.Format(sql, stationId, doctor.DoctorId));//角色

            var rights = MainDbContext.CHIS_SYS_Function.FromSql("exec [sp_Sys_LoadAccessFunctionsOfUser] {0},{1}", customerId, stationId).Where(m => m.IsV20 == true);



            var topMenu = from item in rights
                          where item.ParentFunctionID == null || item.ParentFunctionID == 0
                          orderby item.FunctionIndex
                          select item;

            var tree = new Ass.Data.TreeEntity<Models.CHIS_SYS_Function>()
                .Inital(rights.OrderBy(m => m.FunctionIndex).ToList(),
                                    m => m.ParentFunctionID?.ToString(),
                                    m => m.FunctionID.ToString());

            Ass.Data.TreeEntity<Models.CHIS_SYS_Function> subtree = null;
            if (parentFuncId > 0)
            {
                subtree = tree.SubItems.Find(m => m.ThisItem.FunctionID == parentFuncId);
            }

            var login = await MainDbContext.vwCHIS_Sys_Login.AsNoTracking().FirstOrDefaultAsync(m => m.DoctorId == doctor.DoctorId);



            return new UserAccessInfo
            {
                Doctor = doctor,
                MyStationInfo = new StationInfo
                {
                    Station = station,
                    AllowedStations = allowedStations,
                    StationsTree = allowdStationTree,
                    MyAllDepartments = myallDeparts,
                    SelectedDepartment = selDepart,
                    MySubStations = subStation
                },
                MyTopMainMenus = topMenu,
                MyLogin = login,
                AllMenuTree = tree,
                MyNavFuncs = rights.Where(m => m.IsNavMenu == true).OrderBy(m => m.NavNum),
                MySelectMenuTree = subtree,
                MyRoles = myRoles,
                sysProcessTime = (DateTime.Now - now).TotalMilliseconds + "毫秒"
            };
        }

        //递归获取功能项目
        private IEnumerable<CHIS_SYS_Function> GetSonFuncs(IEnumerable<CHIS_SYS_Function> alllist, int? parentFuncId)
        {
            var query = from c in alllist
                        where c.ParentFunctionID == parentFuncId
                        select c;
            return query.ToList().Concat(query.ToList().SelectMany(t => GetSonFuncs(alllist, t.ParentFunctionID)));
        }
    }



}
