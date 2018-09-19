using CHIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ass;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using CHIS.Models.ViewModel;
using CHIS.DbContext;
using CHIS.Models.StatisticsModels;
using CHIS.Models.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Hosting;

namespace CHIS.Services
{
    public class WorkStationService : BaseService
    {
        IHostingEnvironment _env;

        public WorkStationService(CHISEntitiesSqlServer db, IHostingEnvironment env
            , DoctorService docSvr) : base(db)
        {
            this._env = env;

        }



        /// <summary>
        /// 工作站的获取接诊部门
        /// </summary>
        /// <param name="stationId">工作站Id</param>
        /// <returns></returns>
        public IQueryable<vwCHIS_Code_Department> GetTreatDepartmentOfStation(int stationId)
        {
            var departs = _db.vwCHIS_Code_Department.AsNoTracking().Where(m => m.StationID == stationId && m.IsNotTreatDept != true);
            return departs;
        }


        /// <summary>
        /// 获取工作站的医生成员
        /// </summary>
        /// <param name="stationId"></param>
        public IQueryable<DoctorSEntityV01> GetDoctorsOfStation(int stationId, string searchText = null, int pageIndex = 1, int pageSize = 10)
        {
            var station = _db.CHIS_Code_WorkStation.AsNoTracking().SingleOrDefault(m => m.StationID == stationId);
            if (station == null) throw new Exception("没有发现该工作站");
            var doctorIds = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.StationId == stationId && m.StationIsEnable).Select(m => m.DoctorId).ToList().Distinct();
            var docDeparts = _db.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.StationID == stationId && m.IsEnable == true && m.IsVerified).ToList();
            var rlt = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctorIds.Contains(m.DoctorId));
            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) rlt = rlt.Where(m => m.CustomerMobile == t.String);
                else if (t.IsEmail) rlt = rlt.Where(m => m.Email == t.String);
                else if (t.IsIdCardNumber) rlt = rlt.Where(m => m.IDcard == t.String);
                else rlt = rlt.Where(m => m.NamePY.ToLower().Contains(t.String.ToLower()));
            }
            rlt = rlt.Where(m => m.IsDoctor);
            //过滤测试人员 非测试工作站
            if (_env.IsProduction() && (stationId != MPS.RdTestTreatStationId || !station.IsTestUnit))
            {
                rlt = rlt.Where(m => m.IsForTest != true);
            }


            rlt = rlt.OrderBy(m => m.DoctorId).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var rtn = rlt.Select(m => new DoctorSEntityV01
            {
                DoctorId = m.DoctorId,
                DoctorName = m.DoctorName,
                DoctorGender = (m.Gender ?? 2).ToGenderString(),
                PostTitleName = m.PostTitleName,
                CustomerId = m.CustomerId.Value,
                DoctorPhotoUrl = m.DoctorPhotoUrl.GetUrlPath(Global.ConfigSettings.DoctorImagePathRoot, null),
                DoctorSkillRmk = m.DoctorSkillRmk,
                DoctorAppId = m.DoctorAppId.IsEmpty() ? "0" : m.DoctorAppId,
                DefDepartmentId = docDeparts.Where(a => a.DoctorId == m.DoctorId).FirstOrDefault().DepartId
            });
            return rtn;
        }

        /// <summary>
        ///搜索工作站的医生
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public DoctorSEntityV01 GetDoctorOfStation(int stationId, int doctorId)
        {
            var have = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Any(m => m.StationId == stationId && m.StationIsEnable && m.DoctorId == doctorId);
            if (!have) throw new Exception("该工作站没有此医生");
            var docDeparts = _db.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.StationID == stationId && m.IsEnable == true && m.IsVerified).ToList();
            var rlt = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => m.DoctorId == doctorId)
            .Select(m => new DoctorSEntityV01
            {
                DoctorId = m.DoctorId,
                DoctorName = m.DoctorName,
                DoctorGender = (m.Gender ?? 2).ToGenderString(),
                PostTitleName = m.PostTitleName,
                CustomerId = m.CustomerId.Value,
                DoctorPhotoUrl = m.DoctorPhotoUrl.GetUrlPath(Global.ConfigSettings.DoctorImagePathRoot, null),
                DoctorSkillRmk = m.DoctorSkillRmk,
                DoctorAppId = m.DoctorAppId.IsEmpty() ? "0" : m.DoctorAppId,
                //DefDepartmentId = docDeparts.Where(a => a.DoctorId == m.DoctorId).FirstOrDefault().DepartId
            }).FirstOrDefault();

            if (rlt != null) //fix wanning
            {
                rlt.DefDepartmentId = docDeparts.FirstOrDefault(a => a.DoctorId == rlt.DoctorId)?.DepartId;
            }

            return rlt;
        }




        public vwCHIS_Code_Department GetDepartmentById(int departmentId)
        {
            return _db.vwCHIS_Code_Department.Find(departmentId);
        }





        /// <summary>
        /// 获取工作站的所有成员
        /// </summary>
        /// <param name="stationId"></param>
        public IQueryable<DoctorSEntityV01> GetStaffOfStation(int stationId, string searchText = null, int pageIndex = 1, int pageSize = 10)
        {
            var doctorIds = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.StationId == stationId && m.StationIsEnable).Select(m => m.DoctorId).ToList().Distinct();
            var rlt = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctorIds.Contains(m.DoctorId));
            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) rlt = rlt.Where(m => m.CustomerMobile == t.String);
                else if (t.IsEmail) rlt = rlt.Where(m => m.Email == t.String);
                else if (t.IsIdCardNumber) rlt = rlt.Where(m => m.IDcard == t.String);
                else rlt = rlt.Where(m => m.NamePY.ToLower().Contains(t.String.ToLower()));
            }

            //过滤测试人员
            if (_env.IsProduction() && stationId != MPS.RdTestTreatStationId)
            {
                rlt = rlt.Where(m => m.IsForTest != true);
            }

            rlt = rlt.OrderBy(m => m.DoctorId).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var rtn = rlt.Select(m => new DoctorSEntityV01
            {
                DoctorId = m.DoctorId,
                DoctorName = m.DoctorName,
                DoctorGender = (m.Gender ?? 2).ToGenderString(),
                PostTitleName = m.PostTitleName,
                CustomerId = m.CustomerId.Value
            });
            return rtn;
        }

        /// <summary>
        /// 获取工作站的除了医生的所有成员
        /// </summary>
        /// <param name="stationId"></param>
        public IQueryable<DoctorSEntityV01> GetNotDoctorOfStation(int stationId, string searchText = null, int? principalshipId = null, int pageIndex = 1, int pageSize = 10)
        {
            var doctorIds = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.StationId == stationId && m.StationIsEnable).Select(m => m.DoctorId).ToList().Distinct();
            var rlt = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctorIds.Contains(m.DoctorId));
            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) rlt = rlt.Where(m => m.CustomerMobile == t.String);
                else if (t.IsEmail) rlt = rlt.Where(m => m.Email == t.String);
                else if (t.IsIdCardNumber) rlt = rlt.Where(m => m.IDcard == t.String);
                else rlt = rlt.Where(m => m.NamePY.ToLower().Contains(t.String.ToLower()));
            }

            //过滤掉医生
            rlt = rlt.Where(m => !m.IsDoctor);

            if (principalshipId.HasValue)
            {
                rlt = rlt.Where(m => m.Principalship == principalshipId);
            }

            //过滤测试人员
            if (_env.IsProduction())
            {
                rlt = rlt.Where(m => m.IsForTest != true);
            }

            rlt = rlt.OrderBy(m => m.DoctorId).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var rtn = rlt.Select(m => new DoctorSEntityV01
            {
                DoctorId = m.DoctorId,
                DoctorName = m.DoctorName,
                DoctorGender = (m.Gender ?? 2).ToGenderString(),
                PostTitleName = m.PostTitleName,
                CustomerId = m.CustomerId.Value
            });
            return rtn;
        }


        /// <summary>
        /// 获取接诊站的所有科室信息
        /// </summary>
        /// <param name="stationId">接诊站Id</param>
        public IQueryable<vwCHIS_Code_Department> GetDepartmentsOfTreatStation(int stationId)
        {
            var b = _db.CHIS_Code_WorkStation.Find(stationId).IsCanTreat == true;
            if (!b) throw new Exception("非接诊工作站，不能查询接诊科室");
            return _db.vwCHIS_Code_Department.AsNoTracking().Where(m => m.StationID == stationId && m.IsEnable);
        }
        /// <summary>
        /// 获取接诊站的所有接诊科室信息
        /// </summary>
        /// <param name="stationId">接诊站Id</param>
        public IQueryable<vwCHIS_Code_Department> GetTreatDepartmentsOfTreatStation(int stationId)
        {
            return GetDepartmentsOfTreatStation(stationId).Where(m => m.IsNotTreatDept != true);
        }


        public vwCHIS_Code_WorkStation Find(int stationId)
        {
            return _db.vwCHIS_Code_WorkStation.Find(stationId);
        }

        /// <summary>
        /// 获取我的中心药房的Id
        /// </summary>
        /// <param name="myStationId"></param>
        /// <returns></returns>
        public vwCHIS_Code_WorkStation FindMyCenterDrugStation(int myStationId)
        {
            var ms = Find(myStationId).CenterDrugStoreStationId ?? myStationId;
            return Find(ms);
        }

        /// <summary>
        /// 获取工作站信息
        /// </summary>
        /// <param name="stationIds"></param>
        /// <returns></returns>
        public IQueryable<vwCHIS_Code_WorkStation> Find(List<int> stationIds)
        {
            return _db.vwCHIS_Code_WorkStation.AsNoTracking()
              .Where(m => stationIds.Contains(m.StationID))
              .OrderBy(m => stationIds.IndexOf(m.StationID));
        }

        /// <summary>
        /// 获取部门的所有医生成员
        /// </summary>
        /// <param name="departId"></param>
        /// <returns></returns>
        public IList<vwCHIS_Code_Doctor> GetDoctorsInDepart(int departId)
        {
            var doctors = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DepartId == departId && m.IsVerified == true).Select(m => m.DoctorId).ToList();
            var items = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctors.Contains(m.DoctorId) && m.IsEnable != false);
            items = items.Where(m => m.IsDoctor);
            var testDeparts = _db.CHIS_Code_Department.AsNoTracking().Where(m => m.StationID == MPS.RdTestTreatStationId).Select(m => m.DepartmentID);
            if (_env.IsProduction() && !testDeparts.Contains(departId)) items = items.Where(m => m.IsForTest == false);
            return items.ToList();
        }



        /// <summary>
        /// 获取部门的所有成员
        /// </summary>
        /// <param name="departId"></param>
        /// <returns></returns>
        public IList<vwCHIS_Code_Doctor> GetStaffInDepart(int departId)
        {
            var doctors = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DepartId == departId && m.IsVerified == true).Select(m => m.DoctorId).ToList();
            var items = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctors.Contains(m.DoctorId) && m.IsEnable != false);
            var testDeparts = _db.CHIS_Code_Department.AsNoTracking().Where(m => m.StationID == MPS.RdTestTreatStationId).Select(m => m.DepartmentID);
            if (_env.IsProduction() && !testDeparts.Contains(departId)) items = items.Where(m => m.IsForTest != true);
            return items.ToList();
        }


        /// <summary>
        /// 获取工作站的视图
        /// </summary>
        public IQueryable<vwWorkStation> QueryViewWorkStation()
        {
            var rtn = from _a in _db.CHIS_Code_WorkStation.AsNoTracking()
                      join b in _db.CHIS_Code_WorkStation.AsNoTracking() on _a.ParentStationID ?? 0 equals b.StationID into g
                      from _p in g.DefaultIfEmpty()
                      join c in _db.SYS_ChinaArea.AsNoTracking() on _a.AreaID ?? 0 equals c.AreaId into garea
                      from _z in garea.DefaultIfEmpty()
                      join d in _db.CHIS_Code_Dict_Detail.AsNoTracking() on _a.StationTypeId ?? 0 equals d.DetailID into gtype
                      from _d0 in gtype.DefaultIfEmpty()
                      select new vwWorkStation
                      {
                          StationId = _a.StationID,
                          ParentStationId = _a.ParentStationID,
                          StationName = _a.StationName,
                          ParentStationName = _p.StationName,
                          IsEnable = _a.IsEnable,
                          IsCanTreat = _a.IsCanTreat,
                          StationKeyCode = _a.StationKeyCode,
                          StationPinYin = _a.StationPinYin,
                          StationRmk = _a.StationRmk,
                          StationPicHUrl = _a.StationPicH.ahDtUtil().GetStationImg(imgSizeTypes.HorizNormal),
                          StationPicVUrl = _a.StationPic.ahDtUtil().GetStationImg(imgSizeTypes.VerticalNormal),
                          StationAddress = _z.MergerName + _a.Address,
                          StationTypeName = _d0.ItemName,
                          StationTypeId = _a.StationTypeId,
                          Lat = _a.Lat,
                          Lng = _a.Lng
                      };
            return rtn.AsNoTracking();
        }

        /// <summary>
        /// 搜索工作站
        /// </summary>
        /// <param name="searchText">搜索内容</param>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        /// <returns></returns>
        public (int, IEnumerable<StationInfo>) SearchTreatStation(string searchText, double? lat, double? lng, int pageIndex = 1, int pageSize = 20)
        {
            var wfind = QueryViewWorkStation().AsNoTracking().Where(m => m.IsEnable && m.IsCanTreat/* && m.StationTypeId==CHIS.DictValues.StationType.k_StationType_TreatClinic*/);
            if (searchText.IsNotEmpty())
            {
                var sr = Ass.P.PStr(searchText).ToLower();
                wfind = wfind.Where(m => m.StationName.Contains(sr) ||
                                    m.StationKeyCode.Contains(sr) ||
                                    m.StationPinYin.Contains(sr));
            }
            var fd = wfind.Select(m => new StationInfo
            {
                StationId = m.StationId,
                Lat = m.Lat,
                Lng = m.Lng,
                StationAddress = m.StationAddress,
                StationName = m.StationName,
                StationPicHUrl = m.StationPicHUrl,
                StationPicVUrl = m.StationPicVUrl,
                StationRmk = m.StationRmk,
                DiffOfMeVal = DtUtil.GetDistance(m.Lat ?? 0, m.Lng ?? 0, lat ?? 0, lng ?? 0),
                DiffOfMe = (!m.Lat.HasValue || !lat.HasValue) ? "未知" : null
            });
            var totalNum = fd.Count();
            if (lat.HasValue && lng.HasValue)
            {
                fd = fd.OrderBy(m => m.DiffOfMeVal);
            }
            fd = fd.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
            return (totalNum, fd);
        }

        /// <summary>
        /// 获取我的同等级的工作站
        /// </summary> 
        public IEnumerable<CHIS_Code_WorkStation> GetMyLevelWorkStations(int stationId)
        {
            if (stationId == 0) return _db.CHIS_Code_WorkStation.AsNoTracking().Where(m => false);
            var my = _db.CHIS_Code_WorkStation.Find(stationId);
            return _db.CHIS_Code_WorkStation.AsNoTracking().Where(m => m.ParentStationID == my.ParentStationID);
        }

        /// <summary>
        /// 获取管理站
        /// </summary>
        public IEnumerable<CHIS_Code_WorkStation> GetManagerStation(bool? isCenterDrugStore = null)
        {
            var rlt = _db.CHIS_Code_WorkStation.AsNoTracking().Where(m => m.IsManageUnit);
            if (isCenterDrugStore.HasValue) rlt = rlt.Where(m => m.IsCenterDrugStore == isCenterDrugStore.Value);
            return rlt;
        }

























    }
}
