using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ass;
using CHIS.Models.DataModel;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 公共基础数据
    /// </summary>
    public class DoctorController : OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        public DoctorController(DbContext.CHISEntitiesSqlServer db,
            Services.ReservationService resSvr,
            Services.WorkStationService stationSvr
            , Services.DoctorService docrSvr
            ) : base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _docrSvr = docrSvr;
        }

        //=========================================================================================



        #region 查询 医生

        /// <summary>
        /// 查询医生
        /// </summary>
        /// <param name="searchText"></param>    
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public dynamic SearchTreatDoctors(string searchText, int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                var finds = _docrSvr.SearchTreatDoctors(searchText, null, pageIndex.Value, pageSize.Value);
                var rlt = new Ass.Mvc.PageListInfo
                {
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    RecordTotal = finds.Item1
                };
                var d = MyDynamicResult(true, "");
                d.totalPages = rlt.PageTotal;
                d.items = finds.Item2;
                d.pageIndex = pageIndex;
                d.pageSize = pageSize;
                return d;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }

        /// <summary>
        /// 查询医生所在的接诊工作站
        /// </summary> 
        /// <param name="doctorId">医生Id</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public dynamic JetDoctorsStations(int doctorId)
        {
            try
            {
                var finds = _docrSvr.GetDoctorsStations(doctorId);
                var d = MyDynamicResult(true, "");
                d.items = finds;
                return d;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }
        /// <summary>
        /// 获取医生所在工作站的科室
        /// </summary>
        /// <param name="doctorId">医生Id</param>
        /// <param name="stationId">工作站Id</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public dynamic JetDoctorDepartmentsOfStation(int doctorId, int stationId)
        {
            try
            {
                var finds = _docrSvr.GetDoctorDepartmentsOfStation(doctorId, stationId);
                var d = MyDynamicResult(true, "");
                d.items = finds;
                return d;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }


        /// <summary>
        /// 获取医生信息
        /// </summary>
        /// <param name="doctorIds">医生Id连接字符串，逗号隔开,结果按照Id先后排序</param>
        [HttpGet]
        public dynamic JetDoctorByIds(string doctorIds)
        {
            try
            {
                var finds = _docrSvr.Find(doctorIds.ToList<int>(',')).Select(m => new DoctorSEntityV00
                {
                    DoctorId = m.DoctorId,
                    DoctorName = m.DoctorName,
                    DoctorGender = (m.Gender ?? 2).ToGenderString(),
                    PostTitleName = m.PostTitleName,
                    CustomerId = m.CustomerId.Value,
                    DoctorPhotoUrl = m.DoctorPhotoUrl.ahDtUtil().GetDoctorImg(m.Gender, imgSizeTypes.HorizThumb),
                    DoctorSkillRmk = m.DoctorSkillRmk,
                    DoctorAppId = m.DoctorAppId
                });
                var d = MyDynamicResult(true, "");
                d.items = finds;
                return d;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }



        #endregion










    }
}
