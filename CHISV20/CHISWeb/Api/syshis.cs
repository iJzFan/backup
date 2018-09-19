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
        /// ��ȡ�ҵ����й���վJson���� ,���������{id,pId,name}
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
                        name = "���й���վ"
                    });
                }
                return Json(list);
            }
            else
            { return Json(find); }
        }

        /// <summary>
        /// ��ȡ�ҵĹ���վJson���ݣ��������¼� ,���������{id,pId,name}
        /// ���й���վ��������ȷ��Ȩ��û����ȷ��Ȩ��
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
        /// ��ȡ�ӵĹ���վ
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
        /// ��������Ĺ���վ
        /// </summary>
        /// <param name="searchText">�����ؼ���</param>
        /// <returns></returns>
        public IEnumerable<vwCHIS_Code_WorkStation> TreatStationOfSearch(string searchText)
        {
            return new BllCaller.StationDepartmentBllCaller().TreatStationOfSearch(searchText);
        }

        /// <summary>
        /// /api/syshis/DepartsOfStation
        /// ��ȡ����վ��Ӧ�Ŀ�����Ϣ ͨ��ֻ���ؽ��ﲿ��
        /// </summary>                
        /// <param name="bAllDepart">true �������в��� false:���ؽ��ﲿ��</param>
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
        /// ��ȡ����վ��ҽ��
        /// </summary>
        public IList<vwCHIS_Code_Doctor> QueryDoctorsOfStation(int stationId)
        {
            var departs = DepartsOfStation(stationId).Select(m => m.DepartmentID).ToList();
            var doctors = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => departs.Contains(m.DepartId.Value) && m.IsVerified == true).Select(m => m.DoctorId).ToList();
            var items = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctors.Contains(m.DoctorId) && m.IsEnable != false);
            return items.ToList();
        }


        /// <summary>
        /// ���ݹ���վ��𣬲������еĿ���ѡ��Ľ�ɫ
        /// </summary>
        /// <param name="stationTypeId">����վ���Id</param>
        public IEnumerable<CHIS_SYS_Role> AllRolesOfStationCanSelect(int stationId = 0)
        {
            //todo ���ݹ���վ�Ƿ��Ƕ������������н�ɫ�ǳ��������ߣ���ɸѡ
            //var station = MainDbContext.CHIS_Code_WorkStation.AsNoTracking().FirstOrDefault(m => m.StationID == stationId);
            //if (station == null) return null;
            //if ((station.ParentStationID ?? 0) == 0)
            //{

            //}
            return _db.CHIS_SYS_Role.AsNoTracking().Where(m => m.IsEnable != false).ToList();
        }
        /// <summary>
        /// ����Ĺ���վ����ѡ������н�ɫ
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
        /// ��ȡҽ�����ڹ���վ�Ľ�ɫ
        /// </summary> 
        public List<int> DoctorRolesOfStation(int doctorId, int stationId)
        {
            return _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId && m.RoleId > 0)
                 .Select(m => m.RoleId ?? 0).ToList();
        }


        /// <summary>
        /// һ��ҽ���Ĺ���վ�����ڹ���վ�Ľ�ɫ
        /// </summary>
        public IEnumerable<vwCHIS_Sys_Rel_DoctorStationRoles> StationsRolesOfDoctor(int doctorId)
        {
            return _db.vwCHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId);
        }
        /// <summary>
        /// ����(���ɾ�����߸���)һ��ҽ�����ڹ���վ�Ľ�ɫ
        /// </summary>
        /// <returns></returns>
        public bool UpsertDoctorStationRoles(int doctorId, int stationId, List<int> roles)
        {
            if (roles.Count == 0) throw new Exception("û����Ҫ��ӵĽ�ɫ");

            //Ѱ����Ա����Ĺ���վ
            var f0 = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId && m.StationId == stationId);
            if (f0 == null)
            {
                f0 = _db.CHIS_Sys_Rel_DoctorStations.Add(new CHIS_Sys_Rel_DoctorStations { DoctorId = doctorId, StationId = stationId, StationIsEnable = true }).Entity;
                _db.SaveChanges();
            }

            // Ѱ�ҹ���վ����Ľ�ɫ
            var wr = _db.CHIS_Sys_Rel_WorkStationRoles.AsNoTracking().Where(m => m.StationId == stationId).Select(m => m.RoleId).ToList();
            foreach (var sr in roles) if (!wr.Contains(sr)) throw new Exception("���½�ɫ��Ϣ�в��������Ŀ");

            var f1 = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId).ToList();
            foreach (var item in f1)
            {
                int roleId = item.RoleId ?? 0;
                if (roleId > 0)
                {
                    if (roles.Contains(roleId)) roles.Remove(roleId);//������������
                    else _db.Remove(item);//������ɾ��
                }
            }
            //ʣ����������ӵ���Ŀ
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



        //ɾ��ҽ�����ڹ���վ��Ȩ�ޣ�����Ȩ��
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


        #region ���ܲ˵�
        /// <summary>
        /// ��ȡ���й��ܲ˵�
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