using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ass;
using CHIS.Models.DataModel;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 康健人员信息操作
    /// </summary>
    public class HealthorInfo : OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        Services.DispensingService _dispSvr;
        public HealthorInfo(DbContext.CHISEntitiesSqlServer db,
            Services.ReservationService resSvr,
            Services.WorkStationService stationSvr,
            Services.DoctorService docrSvr,
            Services.CustomerService cusSvr,
            Services.DispensingService dispSvr
            ) : base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _dispSvr = dispSvr;
        }

        //=========================================================================================


        #region 注册  
        /// <summary>
        /// 快速注册健康会员
        /// </summary>
        /// <param name="model">快速注册信息</param> 
        [HttpPost]
        public async Task<dynamic> CustomerQuickRegist([FromBody] QuickRegistCustomerInfo model)
        {
            try
            {
                if (model == null) throw new Exception("没有传入数据");
                if (!ModelState.IsValid) throw new ModelStateException(ModelState);
                var opId = TryGet<int>(() => UserSelf.OpId, 0);
                var opMan = TryGet<string>(() => UserSelf.OpMan, "SYSTEM");
                var customer = await _cusSvr.QuickRegist(model, opId, opMan);
                var d = MyDynamicResult(true, "");
                d.customerId = customer.CustomerID;
                return d;

            }
            catch (Exception ex) { return MyDynamicResult(ex.InnerException ?? ex); }

        }




        #endregion




        #region 会员信息

        /// <summary>
        /// 搜索会员基本信息
        /// </summary>
        /// <param name="searchText">搜索内容</param>
        /// <param name="stationId">搜索工作站Id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        [HttpGet]
        public IQueryable<dynamic> GetCustomersBasicInfoBy(string searchText, int? stationId = null, int? pageIndex = 1, int? pageSize = 20)
        {
            int? sid = null;
            try { sid = stationId ?? UserSelf.StationId; } catch { }
            return _cusSvr.GetCustomersBy(searchText, sid, pageIndex.Value, pageSize.Value).Select(m => new
            {
                CustomerId = m.CustomerID,
                m.CustomerName,
                CustomerGender = m.Gender,
                CustomerGenderStr = (m.Gender ?? 2).ToGenderString(),
                m.CustomerMobile,
                IdCard = m.IDcard,
                CustomerPhoto = m.PhotoUrlDef.GetUrlPath(Global.ConfigSettings.CustomerImagePathRoot, null),
                m.IsVIP,
                m.VIPcode
            });
        }

        /// <summary>
        /// 搜索会员信息
        /// </summary>
        /// <param name="searchText">搜索内容</param>
        /// <param name="stationId">搜索工作站Id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        [HttpGet]
        public IQueryable<Models.vwCHIS_Code_Customer> GetCustomersBy(string searchText, int? stationId = null, int? pageIndex = 1, int? pageSize = 20)
        {
            int? sid = null;
            try { sid = stationId ?? UserSelf.StationId; } catch { }
            return _cusSvr.GetCustomersBy(searchText, sid, pageIndex.Value, pageSize.Value);
        }


        /// <summary>
        /// 获取用户及其关系人
        /// </summary>
        /// <param name="searchText">搜索内容</param>
        /// <param name="stationId">工作站</param>
        /// <param name="pageIndex">页号</param>
        /// <param name="pageSize">页容</param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<CustomerAndRelations> GetCustomersAndRelations(string searchText, int? stationId = null, int? pageIndex = 1, int? pageSize = 20)
        {
            int? sid = null;
            try { sid = stationId ?? UserSelf.StationId; } catch { }
            return _cusSvr.GetCustomerAndRelations(searchText, sid, pageIndex.Value, pageSize.Value);
        }


        #endregion

        #region  获取用户的地址

        /// <summary>
        /// 获取客户的邮寄地址列表
        /// </summary>
        /// <param name="customerId">客户Id</param>
        [HttpGet]
        public dynamic GetCustomerAddresses(int customerId)
        {
            var find = _dispSvr.GetUserAddressInfos(customerId).Select(m=>new {
                m.FullAddress,
                m.ContactName,
                m.Mobile,
                m.ZipCode,
                m.IsLegalAddress,
                m.MergerName,
                m.AddressDetail,
                m.IsDefault
            });
            var mm = MyDynamicResult(true, "");
            mm.items = find;
            return Ok(mm);
        }

        /// <summary>
        /// 获取客户的默认邮寄地址
        /// </summary>
        /// <param name="customerId">客户Id</param>
        [HttpGet]
        public dynamic GetCustomerDefaultAddress(int customerId)
        {
            var find = _dispSvr.GetUserAddressInfos(customerId).Select(m => new {
                m.FullAddress,
                m.ContactName,
                m.Mobile,
                m.ZipCode,
                m.IsLegalAddress,
                m.MergerName,
                m.AddressDetail,
                m.IsDefault
            }).FirstOrDefault(m=>m.IsDefault==true);
            var mm = MyDynamicResult(true, "");
            mm.item = find;
            return Ok(mm);
        }

        #endregion




    }
}
