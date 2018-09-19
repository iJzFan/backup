using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Ass;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 公共基础数据
    /// </summary>
    public class CommonController : OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;

        public CommonController(DbContext.CHISEntitiesSqlServer db,
            Services.ReservationService resSvr,
            Services.WorkStationService stationSvr

            ) : base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
        }

        //=========================================================================================


        /// <summary>
        /// 获取工作站里的医生
        /// </summary>
        /// <param name="stationId">工作站Id</param>
        /// <param name="searchText">搜索项</param>
        /// <param name="pageIndex">页码 可选，默认1</param>
        /// <param name="pageSize">页容 可选，默认10</param>
        /// <returns>Json内容</returns>
        [HttpGet]
        public dynamic JetDoctorOfStation(int? stationId, string searchText, int? pageIndex = 1, int? pageSize = 10)
        {
            try
            {
                if (!stationId.HasValue) stationId = TryGet<int?>(() => UserSelf.StationId);
                if (!stationId.HasValue) throw new Exception("没有找到工作站Id");
                var rlt = _stationSvr.GetDoctorsOfStation(stationId.Value, searchText, pageIndex.Value, pageSize.Value);
                var station = _stationSvr.Find(stationId.Value);
                var dd = MyDynamicResult(true, "获取成功");
                dd.station = new
                {
                    station.StationName,
                    station.StationLat,
                    station.StationLng,
                    defStationPic = station.DefStationPic.GetUrlPath(Global.ConfigSettings.StationImagePathRoot)
                };
                dd.doctors = rlt;
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }


        /// <summary>
        /// 获取工作站里的所有成员
        /// </summary>
        /// <param name="stationId">工作站Id</param>
        /// <param name="searchText">搜索项</param>
        /// <param name="pageIndex">页码 可选，默认1</param>
        /// <param name="pageSize">页容 可选，默认10</param>
        /// <returns>Json内容</returns>
        [HttpGet]
        public dynamic JetStaffOfStation(int? stationId, string searchText, int? pageIndex = 1, int? pageSize = 10)
        {
            try
            {
                if (!stationId.HasValue) stationId = TryGet<int?>(() => UserSelf.StationId);
                if (!stationId.HasValue) throw new Exception("没有找到工作站Id");
                var rlt = _stationSvr.GetStaffOfStation(stationId.Value, searchText, pageIndex.Value, pageSize.Value);
                var station = _stationSvr.Find(stationId.Value);
                var dd = MyDynamicResult(true, "获取成功");
                dd.station = new
                {
                    station.StationName,
                    station.StationLat,
                    station.StationLng,
                    defStationPic = station.DefStationPic.GetUrlPath(Global.ConfigSettings.StationImagePathRoot)
                };
                dd.doctors = rlt;
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }


        /// <summary>
        /// 获取工作站里的除了医生的所有成员
        /// </summary>
        /// <param name="stationId">工作站Id</param>
        /// <param name="searchText">搜索项</param>
        /// <param name="principalshipId">职能定位 可选</param>
        /// <param name="pageIndex">页码 可选，默认1</param>
        /// <param name="pageSize">页容 可选，默认10</param>
        /// <returns>Json内容</returns>
        [HttpGet]
        public dynamic JetNotDoctorOfStation(int? stationId, string searchText, int? principalshipId = null, int? pageIndex = 1, int? pageSize = 10)
        {
            try
            {
                if (!stationId.HasValue) stationId = TryGet<int?>(() => UserSelf.StationId);
                if (!stationId.HasValue) throw new Exception("没有找到工作站Id");
                var rlt = _stationSvr.GetNotDoctorOfStation(stationId.Value, searchText, principalshipId, pageIndex.Value, pageSize.Value);
                var station = _stationSvr.Find(stationId.Value);
                var dd = MyDynamicResult(true, "获取成功");
                dd.station = new
                {
                    station.StationName,
                    station.StationLat,
                    station.StationLng,
                    defStationPic = station.DefStationPic.GetUrlPath(Global.ConfigSettings.StationImagePathRoot)
                };
                dd.doctors = rlt;
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }

        /// <summary>
        /// 搜索该工作站的该医生信息
        /// </summary>
        /// <param name="stationId">工作站</param>
        /// <param name="doctorId">医生</param>
        /// <returns></returns>
    
         
        [HttpGet]
        public dynamic GetDoctorOfStation(int stationId,int doctorId)
        {
            var rlt = _stationSvr.GetDoctorOfStation(stationId,doctorId);
            return rlt;
        }



        /// <summary>
        /// 获取科室所在的所有医生
        /// </summary>
        /// <param name="departId">科室Id</param>
        [HttpGet]
        public dynamic JetDoctorsInDepart(int departId)
        {
            try
            {
                var items = _stationSvr.GetDoctorsInDepart(departId);
                var dd = MyDynamicResult(true, "");
                dd.items = items;
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }


        /// <summary>
        /// 获取科室所在的所有成员
        /// </summary>
        /// <param name="departId">科室Id</param>
        [HttpGet]
        public dynamic JetStaffInDepart(int departId)
        {
            try
            {
                var items = _stationSvr.GetStaffInDepart(departId);
                var dd = MyDynamicResult(true, "");
                dd.items = items;
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }


        /// <summary>
        /// 获取接诊站的所有科室信息
        /// </summary>
        /// <param name="stationId">接诊站Id</param>
        [HttpGet]
        public dynamic JetDepartmentsOfTreatStation(int stationId)
        {
            try
            {
                var station = _stationSvr.Find(stationId);
                var items = _stationSvr.GetTreatDepartmentsOfTreatStation(stationId);
                var dd = MyDynamicResult(true, "");
                dd.stationId = stationId;
                dd.statioName = station.StationName;
                dd.defStationPic = station.DefStationPic.ahDtUtil().GetStationImg(imgSizeTypes.HorizNormal); 
                dd.items = items;        
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }
        /// <summary>
        /// 获取接诊站的所有接诊业务科室信息
        /// </summary>
        /// <param name="stationId">接诊站Id</param>
        [HttpGet]
        public dynamic JetTreatDepartmentsOfTreatStation(int stationId)
        {
            try
            {
                var items = _stationSvr.GetTreatDepartmentsOfTreatStation(stationId);
                var dd = MyDynamicResult(true, "");
                dd.items = items;
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }



        





    }
}
