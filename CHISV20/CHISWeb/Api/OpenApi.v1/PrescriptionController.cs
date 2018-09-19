using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ass;
using CHIS.Models.DataModel;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 处方相关
    /// </summary>
    public class PrescriptionController : OpenApiBaseController
    {

        Services.ReservationService _resSvr;
 
    
        Services.PrescriptionService _presSvr;
        public PrescriptionController(DbContext.CHISEntitiesSqlServer db,                            
            Services.PrescriptionService presSvr
            ) : base(db)
        {
            _presSvr = presSvr;
    
        }

        //=========================================================================================


        #region 获取处方信息

        /// <summary>
        /// 获取用户的处方信息
        /// </summary>
        /// <param name="searchText">输入条件 手机、邮箱、身份证、登录号、处方号、接诊号</param>
        /// <param name="dateTimeFrom">选填 开始时间</param>
        /// <param name="dateTimeEnd">选填 结束时间 间隔不超过3个月</param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<PrescriptionSearchItem> GetPrescriptionOfCustomer(string searchText, DateTime? dateTimeFrom, DateTime? dateTimeEnd)
        {
            return _presSvr.GetPrescriptionOfCustomer(searchText, dateTimeFrom, dateTimeEnd);
        }

        /// <summary>
        /// 获取用户的处方信息
        /// </summary>
        /// <param name="searchText">输入条件 手机、邮箱、身份证、登录号、处方号、接诊号</param>
        /// <param name="dateTimeFrom">选填 开始时间</param>
        /// <param name="dateTimeEnd">选填 结束时间 间隔不超过3个月</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult JetPrescriptionOfCustomer(string searchText, DateTime? dateTimeFrom, DateTime? dateTimeEnd)
        {
            try
            {
                var items = GetPrescriptionOfCustomer(searchText, dateTimeFrom, dateTimeEnd);
                var d = MyDynamicResult(true, "");
                d.items = items;
                return Json(d);
            }
            catch (Exception ex) { return Json(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 获取用户的处方信息
        /// </summary>
        /// <param name="searchText">输入条件 手机、邮箱、身份证、登录号、处方号、接诊号</param>
        /// <param name="dateTimeFrom">选填 开始时间</param>
        /// <param name="dateTimeEnd">选填 结束时间 间隔不超过3个月</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult HtmPrescriptionOfCustomer(string searchText, DateTime? dateTimeFrom, DateTime? dateTimeEnd)
        {
            try
            {             
                var items = GetPrescriptionOfCustomer(searchText, dateTimeFrom, dateTimeEnd);          
                return ApiPartialView("_pvPrescriptionOfCustomer", items);
            }
            catch (Exception ex) {    return ApiPartialView(ex);}
        }



        #endregion






    }
}
