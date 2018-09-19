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
    public class DoctorController : CHIS.Api.v2.OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        Services.DrugService _drugSvr;
        Services.DictService _dicSvr;
        public DoctorController(Services.ReservationService resSvr,
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
        /// 获取医生信息
        /// </summary>
        /// <param name="doctorId">医生Id</param> 
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetDoctorInfo(int doctorId)
        {
            try
            {
                var mdd = _docrSvr.Find(doctorId).ahDtUtil().NewModel<Models.vwCHIS_Code_Doctor>(m => new
                {
                    m.DoctorId,
                    m.DoctorName,
                    m.Gender,
                    m.Birthday,
                    m.EmployeeCode,
                    Mobile = m.CustomerMobile,
                    EduLevelName = _dicSvr.FindName(m.EduLevel),
                    m.IsDoctorOfPerson,
                    DoctorPicUrl = m.PhotoUrlDef.ahDtUtil().GetDoctorImg(m.Gender),
                    m.PostTitleName,
                    m.DoctorSkillRmk,
                    m.DoctorIsAuthenticated,
                    m.DoctorAuthenticatedTime,
                    m.DoctorCreateTime,
                    m.TreatFee,
                    OtherInfo = new
                    {
                        m.IsForTest,
                        m.IsDoctor,
                        PrincipalShip = _dicSvr.FindName(m.Principalship),
                        m.FileType,
                        m.IsEnable,
                        m.StopDate,
                        m.DutyState,
                        PostTitleId= m.PostTitle,
                        m.OnDutyDate,
                        m.ApprovalDate,
                        m.OutDutyDate,
                        m.MasterTech,
                        ClassGroupId = m.ClassGroupID,
                        m.IsShiftClass,
                        m.Explain,
                        m.Remark,
                        m.sysAppId,
                        
                        m.CustomerNo,
                        m.IDcard,
                        m.InsuranceNo,
                        m.CertificateTypeId,
                        m.CertificateNo,
                        m.CustomerId,
                        m.Email,
                        m.Presfession,
                        MarriageStatus = _dicSvr.FindName(m.Marriage),
                        m.CustomerIsAuthenticated,
                        m.CustomerAuthenticatedTime,
                        m.IsChecking,
                        m.UCusId,
                        m.LoginName,
                        m.DoctorAppId,
                    }
                });
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }


    }
}
