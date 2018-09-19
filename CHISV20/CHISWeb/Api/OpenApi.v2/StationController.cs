using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ass;
using CHIS.Models.DataModel;
using Microsoft.AspNetCore.Authorization;

namespace CHIS.Api.OpenApi.v2
{

    /// <summary>
    /// 药品
    /// </summary>
    public class StationController : CHIS.Api.v2.OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        Services.DrugService _drugSvr;
        Services.DictService _dicSvr;
        public StationController(Services.ReservationService resSvr,
            Services.WorkStationService stationSvr,
            Services.DoctorService docrSvr,
            Services.CustomerService cusSvr,
            Services.DrugService drugSvr,
            Services.DictService dicSvr
            ) //: base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
            _drugSvr = drugSvr;
            _dicSvr = dicSvr;
        }

        //=========================================================================================

        /// <summary>
        /// 获取接诊站信息
        /// </summary>
        /// <param name="stationId">接诊站Id</param> 
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetStationInfo(int? stationId)
        {
            try
            {
                if (UserSelf == null && stationId == null)
                    throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录的用户，需要输入参数:stationId");
                stationId = stationId ?? UserSelf.StationId;
                var mdd = _stationSvr.Find(stationId.Value).ahDtUtil().NewModel<Models.vwCHIS_Code_WorkStation>(m => new
                {
                    StationId = m.StationID,
                    ParentStationId = m.ParentStationID,
                    m.StationName,
                    ParentStationName = m.ParentName,
                    m.Address,
                    m.Telephone,
                    m.Fax,
                    m.LegalPerson,
                    AreaId = m.AreaID,
                    AgentId = m.AgentID,
                    m.IsEnable,
                    m.StopDate,
                    m.ShowOrder,
                    m.Lat,
                    m.Lng,
                    DrugStoreStationId= m.DrugStoreStationId??m.StationID,
                    DrugStoreStationName=_stationSvr.Find(m.DrugStoreStationId??m.StationID)?.StationName,
                    StationVPicUrl= m.StationPic.ahDtUtil().GetStationImg(imgSizeTypes.VerticalNormal),
                    StationHPicUrl=m.StationPicH.ahDtUtil().GetStationImg(imgSizeTypes.HorizNormal),
                    OtherInfo = new
                    {
                        OpId = m.OpID,
                        m.OpMan,
                        m.OpTime,
                        m.Remark,
                        m.AddressInfo,
                        m.IsCanTreat,
                        m.IsNetPlat,
                        m.IsManageUnit,
                        m.IsTestUnit,                      
                        m.StationRmk,                        
                        m.ZipCode,
                        m.StationLng,
                        m.StationLat,
                        m.HotNum,
                        m.IsNotMedicalUnit,
                        m.SickStampUri,
                        m.StationTypeId,
                        m.StationTypeName,
                        m.StationLogPicH,
                        m.StationLogPicV,
                        m.StationKeyCode,
                        m.StationPinYin,
                        m.DefStationPic
                    }
                });
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }


        /// <summary>
        /// 获取接诊站可登录的医生信息
        /// </summary>
        /// <param name="stationId">接诊站Id</param> 
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetLoginUsersOfStation(int? stationId,string searchText,int? pageInde=1,int? pageSize=20)
        {
            try
            {
                if (UserSelf == null && stationId == null)
                    throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录的用户，需要输入参数:stationId");
                stationId = stationId ?? UserSelf.StationId;
                var mdd = _stationSvr.GetDoctorsOfStation(stationId.Value, searchText, pageInde.Value, pageSize.Value);
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }

    }
}
