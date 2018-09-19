using Ass;
using CHIS.Code.Filter;
using CHIS.Controllers;
using CHIS.Models.DataModel;
using CHIS.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Api
{
    public partial class Customer : BaseDBController
    {
        Services.CustomerService _cusSvr;
        Services.FollowListService _followListService;
        public Customer(DbContext.CHISEntitiesSqlServer db, Services.CustomerService cusSvr, Services.FollowListService followListService) : base(db) { _cusSvr = cusSvr; _followListService = followListService; }


        /// <summary>
        /// 搜索会员信息
        /// </summary>
        /// <param name="searchText">搜索内容</param>
        /// <param name="bWithShip">是否包含关系人</param>
        /// <param name="stationId">搜索工作站Id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        [AllowAnonymous]
        public IQueryable<Models.vwCHIS_Code_Customer> GetCustomersBy(string searchText, int? stationId = null, int pageIndex = 1, int pageSize = 20)
        {
            int? sid = null;
            try { sid = stationId ?? UserSelf.StationId; } catch { }
            return _cusSvr.GetCustomersBy(searchText, sid, pageIndex, pageSize);
        }

        [AllowAnonymous]
        public IEnumerable<CustomerAndRelations> GetCustomersAndRelations(string searchText, int? stationId = null, int pageIndex = 1, int pageSize = 20)
        {
            int? sid = null;
            try { sid = stationId ?? UserSelf.StationId; } catch { }
            return _cusSvr.GetCustomerAndRelations(searchText, sid, pageIndex, pageSize);
        }


        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public IActionResult GetDefCustomerLoginName(string name, int gender, DateTime birthday)
        {
            return TryCatchFunc((dd) =>
            {
                if (name.IsEmpty() || birthday == new DateTime()) throw new Exception("请填写好姓名，性别，生日后获取");
                string s = string.Format("{0}.{1}.{2}",
                    Ass.Utils.GetPinYinCode(name),
                    gender,
                    birthday.ToString("yyMMdd")).ToLower();
                dd.loginName = s;
                return null;
            });
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> LoginNameRegistIsAllowed(string loginName, int? customerId)
        {
            if (loginName.IsEmpty())
            {
                loginName = Request.Query["Customer.LoginName"].FirstOrDefault();
            }
            if (customerId == null)
            {
                customerId = Ass.P.PIntN(Request.Query["Customer.CustomerId"].FirstOrDefault());
            }
            return Json(new CustomerCBL(this).LoginNameAllowedRegisted(loginName, customerId));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> EmailRegistIsAllowed(string email, int? customerId)
        {
            if (email.IsEmpty())
            {
                email = Request.Query["Customer.Email"].FirstOrDefault();
            }
            if (customerId == null)
            {
                customerId = Ass.P.PIntN(Request.Query["Customer.CustomerId"].FirstOrDefault());
            }
            return Json(new CustomerCBL(this).EmailAllowedRegisted(email, customerId));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> MobileRegistIsAllowed(string mobile, int? customerId)
        {
            if (mobile.IsEmpty())
            {
                mobile = Request.Query["Customer.CustomerMobile"].FirstOrDefault();
            }
            if (customerId == null)
            {
                customerId = Ass.P.PIntN(Request.Query["Customer.CustomerId"].FirstOrDefault());
            }
            return Json(new CustomerCBL(this).MobileAllowedRegisted(mobile, customerId));
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> IdCardRegistIsAllowed(string idcard, int? customerId)
        {
            if (idcard.IsEmpty())
            {
                idcard = Request.Query["Customer.IDcard"].FirstOrDefault();
            }
            if (customerId == null)
            {
                customerId = Ass.P.PIntN(Request.Query["Customer.CustomerId"].FirstOrDefault());
            }
            return Json(new CustomerCBL(this).IdCardAllowedRegisted(idcard, customerId));
        }

        [HttpGet]
        [AllowAnonymous]
        [TypeFilter(typeof(CHISTokenAuth))]
        public IActionResult FollowList(int customerId)
        {
            var model = _followListService.Get(customerId);

            return Ok(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [TypeFilter(typeof(CHISTokenAuth))]
        public IActionResult FollowList([FromBody]FollowListViewModel model)
        {
            _followListService.Update(model);

            return Ok();
        }
    }
}
