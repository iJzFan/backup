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
    /// 康健人员操作
    /// </summary>
    public class HealthorController : CHIS.Api.v2.OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        public HealthorController(Services.ReservationService resSvr,
            Services.WorkStationService stationSvr,
            Services.DoctorService docrSvr,
            Services.CustomerService cusSvr
            ) //: base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
        }

        //=========================================================================================


        #region 预约  
        

        /// <summary>
        /// 获取患者信息
        /// </summary>
        /// <param name="customerId">患者Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public async Task<IActionResult> GetCustomer(int customerId)
        {
            try
            {
                var cus = await _cusSvr.GetCustomerById(customerId);
                var rlt = cus.ahDtUtil().NewModel<Models.vwCHIS_Code_Customer>(m => new
                {
                    CustomerId = m.CustomerID,
                    m.CustomerName,
                    m.Gender,
                    m.Birthday,
                    m.Address,
                    m.Email,
                    m.MarriageStatus,
                    m.Remark,
                    m.Age,
                    HealthInfo = new
                    {
                        m.Height,
                        m.Weight,
                        m.BloodType,
                        m.MenstruationStartOldYear,
                        m.MenstruationEndOldYear,
                        m.Allergic,
                        m.PastMedicalHistory,
                        m.PregnancyNum,
                        m.BirthChildrenNum,
                        m.AliveChildNum,
                    },
                    m.AddressAreaId,
                    m.MergerName,
                    m.CustomerCreateDate,
                    m.CustomerMobile,
                    m.CustomerIsAuthenticated,
                    m.CustomerAuthenticatedTime,
                    m.NickName,
                    m.Religion,
                    m.Occupation,
                    m.TopSchool,
                    m.LoginName,
                    CustomerPicUrl = m.PhotoUrlDef.ahDtUtil().GetCustomerImg(m.Gender)
                });
                var jm = MyDynamicResult(rlt);
                // jm.orig = cus;
                return Ok(jm);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 获取患者使用的邮件地址列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetCustomerMailAddresses(int customerId)
        {
            try
            {
                var dd = _cusSvr.GetMyAddressInfos(customerId).Select(m => new
                {
                    m.AddressId,
                    m.Remark,
                    m.ContactName,
                    m.Mobile,
                    m.FullAddress,
                    m.ZipCode,
                    m.AreaId,
                    m.AddressDetail,
                    m.IsDefault,
                    m.MergerName,
                    m.Lng,
                    m.Lat,
                    m.AreaLevel,
                    m.IsLegalAddress
                }).ToList();
                var jm = MyDynamicResult(dd);
                jm.customerId = customerId;
                return Ok(jm);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }

        /// <summary>
        /// 获取患者使用的默认邮件地址
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetCustomerDefaultMailAddress(int customerId)
        {
            try
            {
                var dd = _cusSvr.GetMyAddressInfos(customerId).Where(m=>m.IsDefault).Select(m => new
                {
                    m.AddressId,
                    m.Remark,
                    m.ContactName,
                    m.Mobile,
                    m.FullAddress,
                    m.ZipCode,
                    m.AreaId,
                    m.AddressDetail,
                    m.IsDefault,
                    m.MergerName,
                    m.Lng,
                    m.Lat,
                    m.AreaLevel,
                    m.IsLegalAddress
                }).FirstOrDefault();
                var jm = MyDynamicResult(dd);
                jm.customerId = customerId;
                return Ok(jm);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }



        #endregion


        ///// <summary>
        ///// 获取约号的清单
        ///// </summary>
        ///// <param name="stationId">工作站</param>
        ///// <returns></returns>
        //[HttpGet]
        //[Authorize("ThirdPartAuth")]
        //public IActionResult GetRegistedListOfStation(int stationId)
        //{
        //    try
        //    {
        //        string data = null;
        //        var rlt = MyDynamicResult(data);
        //        return Ok(rlt);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new ComExceptionModel(ex));
        //    }
        //}

    }
}
