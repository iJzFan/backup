using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using Ass;
using CHIS;
using CHIS.Models.ViewModel;

namespace CHIS.Api
{
    public class syshis : BaseDBController
    {
        public syshis(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        /// <summary>
        /// 获取我的所有工作站Json数据 ,用于填充树{id,pId,name}
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public IActionResult StationsOfMyAll(int mode = 0)
        {
            var doctorId = base.UserSelf.DoctorId;
            var topStationId = base.UserSelf.StationId;
            var find = new BllCaller.DoctorMgrBllCaller().StationsOfDoctor(doctorId, topStationId, true)
                       .Select(a => new
                       {
                           id = a.StationID,
                           pId = a.ParentStationID ?? 0,
                           name = a.StationName
                       });


            if (mode == 0)
            {
                var list = find.ToList();
                if (list.Count > 1)
                {
                    list.Add(new
                    {
                        id = 0,
                        pId = 0,
                        name = "所有工作站"
                    });
                }
                return Json(list);
            }
            else
            { return Json(find); }
        }

        /// <summary>
        /// 获取我的工作站Json数据，本级和下级 ,用于填充树{id,pId,name}
        /// 所有工作站，包括明确授权和没有明确授权的
        /// </summary>
        public IActionResult StationsOfMy()
        {

            var find = _db.SqlQuery<CHIS_Code_WorkStation>(string.Format("exec [sp_Common_LoadTreeRoadToLeafs] '{0}','{1}','{2}',{3}",
                                                                    "CHIS_Code_WorkStation", "StationID", "ParentStationID", UserSelf.StationId))
                                                                    .OrderBy(m => m.ParentStationID).ThenBy(m => m.ShowOrder)
                .Select(item => new
                {
                    id = item.StationID,
                    pId = item.ParentStationID ?? 0,
                    name = item.StationName,
                    isTestUnit = item.IsTestUnit
                });
            return Json(find);
        }


        /// <summary>
        /// 获取子的工作站
        /// </summary>
        /// <param name="rootStationId"></param>
        /// <returns></returns>

        public IActionResult GetSonStations(int pStationId, bool bWithRoot = false, bool bNotMedicalUnit = false)
        {
            return TryCatchFunc(dd =>
            {
                dd.SonStations = new BllCaller.StationDepartmentBllCaller().GetSonStations(pStationId, bWithRoot, bNotMedicalUnit).Select(m => new
                {
                    StationId = m.StationID,
                    ParentStationId = m.ParentStationID,
                    StationName = m.StationName,
                    IsCanTreat = m.IsCanTreat,
                    IsManageUnit = m.IsManageUnit,
                    IsEnable = m.IsEnable,
                    id = m.StationID,
                    pId = m.ParentStationID ?? 0,
                    name = m.StationName,
                    isParent = !(m.IsCanTreat)
                });
                return null;
            });
        }



        /// <summary>
        /// 搜索接诊的工作站
        /// </summary>
        /// <param name="searchText">搜索关键字</param>
        /// <returns></returns>
        public IEnumerable<vwCHIS_Code_WorkStation> TreatStationOfSearch(string searchText)
        {
            return new BllCaller.StationDepartmentBllCaller().TreatStationOfSearch(searchText);
        }

        /// <summary>
        /// /api/syshis/DepartsOfStation
        /// 获取工作站对应的科室信息 通常只返回接诊部门
        /// </summary>                
        /// <param name="bAllDepart">true 返回所有部门 false:返回接诊部门</param>
        public IEnumerable<vwCHIS_Code_Department> DepartsOfStation(int stationId, bool bAllDepart = false)
        {
            return new BllCaller.StationDepartmentBllCaller().DepartsOfStation(stationId, bAllDepart);
        }




        public IActionResult GetRolesAndDepartsOfStation(int stationId, int doctorId)
        {
            var model = new RolesAndDepartsOfStation(); 
            var roleids = _db.CHIS_Sys_Rel_WorkStationRoles.AsNoTracking().Where(m => m.StationId == stationId).Select(m => m.RoleId).ToList().Distinct();
            var roles = _db.CHIS_SYS_Role.AsNoTracking().Where(m => roleids.Contains(m.RoleID)).Select(m => new RoleItem
            {
                RoleId = m.RoleID,
                RoleKey = m.RoleKey,
                RoleName = m.RoleName
            });

            var selRoleIds = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.StationId == stationId && m.DoctorId == doctorId && m.RoleId > 0 && m.MyRoleIsEnable == true).Select(m => m.RoleId.Value);
            var selDeparts = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId && m.DepartId > 0 && m.IsVerified == true).Select(m => m.DepartId.Value);

            model.Roles = roles.ToList();
            model.Departs = DepartsOfStation(stationId).ToList();
            model.SelectedRoles = selRoleIds.ToList();
            model.SelectedDeparts = selDeparts.ToList();
            return ApiPartialView("_pvRolesAndDepartsOfStation", model);
        }


        /// <summary>
        /// 获取工作站的医生
        /// </summary>
        public IList<vwCHIS_Code_Doctor> QueryDoctorsOfStation(int stationId)
        {
            var departs = DepartsOfStation(stationId).Select(m => m.DepartmentID).ToList();
            var doctors = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => departs.Contains(m.DepartId.Value) && m.IsVerified == true).Select(m => m.DoctorId).ToList();
            var items = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctors.Contains(m.DoctorId) && m.IsEnable != false);
            return items.ToList();
        }


        /// <summary>
        /// 根据工作站类别，查找所有的可以选择的角色
        /// </summary>
        /// <param name="stationTypeId">工作站类别Id</param>
        public IEnumerable<CHIS_SYS_Role> AllRolesOfStationCanSelect(int stationId = 0)
        {
            //todo 根据工作站是否是顶级，并且现有角色是超级管理者，来筛选
            //var station = MainDbContext.CHIS_Code_WorkStation.AsNoTracking().FirstOrDefault(m => m.StationID == stationId);
            //if (station == null) return null;
            //if ((station.ParentStationID ?? 0) == 0)
            //{

            //}
            return _db.CHIS_SYS_Role.AsNoTracking().Where(m => m.IsEnable != false).ToList();
        }
        /// <summary>
        /// 具体的工作站可以选择的所有角色
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public IEnumerable<vwCHIS_Sys_Rel_WorkStationRoles> AllRolesOfStationOnlySelect(int stationId)
        {
            if (stationId > 0)
            {

                var roleids = _db.vwCHIS_Sys_Rel_WorkStationRoles.AsNoTracking().Where(m => m.StationId == stationId).ToList();
                return roleids;
                // return MainDbContext.CHIS_SYS_Role.AsNoTracking().Where(m => roleids.Contains(m.RoleID));
            }
            else return null;
        }
        /// <summary>
        /// 获取医生所在工作站的角色
        /// </summary> 
        public List<int> DoctorRolesOfStation(int doctorId, int stationId)
        {
            return _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId && m.RoleId > 0)
                 .Select(m => m.RoleId ?? 0).ToList();
        }


        /// <summary>
        /// 一个医生的工作站和所在工作站的角色
        /// </summary>
        public IEnumerable<vwCHIS_Sys_Rel_DoctorStationRoles> StationsRolesOfDoctor(int doctorId)
        {
            return _db.vwCHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId);
        }
        /// <summary>
        /// 更新(添加删除或者更新)一个医生所在工作站的角色
        /// </summary>
        /// <returns></returns>
        public bool UpsertDoctorStationRoles(int doctorId, int stationId, List<int> roles)
        {
            if (roles.Count == 0) throw new Exception("没有需要添加的角色");

            //寻找人员允许的工作站
            var f0 = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId && m.StationId == stationId);
            if (f0 == null)
            {
                f0 = _db.CHIS_Sys_Rel_DoctorStations.Add(new CHIS_Sys_Rel_DoctorStations { DoctorId = doctorId, StationId = stationId, StationIsEnable = true }).Entity;
                _db.SaveChanges();
            }

            // 寻找工作站允许的角色
            var wr = _db.CHIS_Sys_Rel_WorkStationRoles.AsNoTracking().Where(m => m.StationId == stationId).Select(m => m.RoleId).ToList();
            foreach (var sr in roles) if (!wr.Contains(sr)) throw new Exception("更新角色信息有不允许的项目");

            var f1 = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId).ToList();
            foreach (var item in f1)
            {
                int roleId = item.RoleId ?? 0;
                if (roleId > 0)
                {
                    if (roles.Contains(roleId)) roles.Remove(roleId);//如果存在则忽略
                    else _db.Remove(item);//否则则删除
                }
            }
            //剩余的则是增加的项目
            foreach (var role in roles) _db.Add(new CHIS_Sys_Rel_DoctorStationRoles
            {
                DoctorId = doctorId,
                StationId = stationId,
                RoleId = role,
                MyRoleIsEnable = true
            });
            _db.SaveChanges();
            return true;
        }



        //删除医生所在工作站的权限，所有权限
        public IActionResult DeleteDoctorAccessOfStation(int doctorId, int stationId)
        {
            return TryCatchFunc(() =>
            {
                var finds = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId);
                var fs = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId);
                var fds = _db.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationID == stationId).Select(m => m.DoctorDepartsId);
                var fdss = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => fds.Contains(m.DoctorDepartsId));
                _db.RemoveRange(finds);
                _db.RemoveRange(fs);
                _db.RemoveRange(fdss);
                _db.SaveChanges();
                return null;
            }, true);
        }


        #region 功能菜单
        /// <summary>
        /// 获取所有功能菜单
        /// </summary>
        public IActionResult AllFunctions()
        {
            var find = _db.SqlQuery<CHIS_SYS_Function>(string.Format("exec [sp_Common_LoadTreeRoadToLeafs] '{0}','{1}','{2}',{3}",
                                                                    "CHIS_SYS_Function", "FunctionID", "ParentFunctionID", 0))
                                                                    .OrderBy(m => m.ParentFunctionID).ThenBy(m => m.FunctionIndex).Where(m => m.IsV20 == true)
                .Select(item => new
                {
                    id = item.FunctionID,
                    pId = item.ParentFunctionID ?? 0,
                    name = item.FunctionName,
                    isTestUnit = item.IsV20
                });
            return Json(find);
        }


        #endregion



    }
}