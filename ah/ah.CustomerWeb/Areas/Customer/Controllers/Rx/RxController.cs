using ah.DbContext;
using ah.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Ass;

namespace ah.Areas.Customer.Controllers.Rx
{
    public class RxController : QueryController
    {
        Services.ReservationService _resvSvr;
        Services.weixinService _wxSvr;
        AHMSEntitiesSqlServer _db;
        public RxController(Services.ReservationService resvSvr
            , Services.weixinService wxSvr
            , AHMSEntitiesSqlServer db
            )
        {
            _resvSvr = resvSvr;
            _wxSvr = wxSvr;
            _db = db;
        }

        //[AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var customerId = Ass.P.PIntV(User.FindFirst("CustomerId").Value);          
            ResvationReturn model = new ResvationReturn();
            bool rlt = false;
            try
            {
                //string openid = HttpContext.Session.GetString("openid");
                //if (openid.IsEmpty())
                //{
                //    _wxSvr.ReGetOpenIdToSession();
                //    openid = HttpContext.Session.GetString("openid");
                //}
                //if (openid.IsEmpty()) throw new Exception("没有获取到微信OpenId");

                //var cus = _db.CHIS_Code_Customer.FirstOrDefault(m => m.WXOpenId == openid);
                //if (cus == null) throw new Exception("没有通过OpenId获取到用户信息");
                if (customerId == 0) throw new Exception("没有获取到CustomerId");
                var arr = id.Split('-');
                var reg = new ReservationInfo
                {
                    CustomerId = customerId,
                    StationId = Ass.P.PIntV(arr[0]),
                    DoctorId = Ass.P.PIntV(arr[1]),
                    RxDoctorId = Ass.P.PIntV(arr[2]),
                    ReservationDate = DateTime.Now,
                    ReservationSlot = DateTime.Now.Hour < 12 ? 1 : 2,//上午1下午2
                    OpId = 0,
                    OpMan = "微信公众号二维码快约"
                };
                model = await _resvSvr.ReservateDoctorAsync(reg);
                rlt = model.rlt;
              //  model.OpenId = openid;
            }
            catch (Exception ex) { model.rlt = rlt; model.msg = ex.Message; }
            var viewName = model.rlt ? "RxSuccess" : "RxFailed";
            return View(viewName, model);
        }
    }
}
