using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ass;
using ah.Areas.Customer.Controllers.Base;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class HomeController : BaseController
    {


        // GET: /<controller>/
        public IActionResult OrderDetail()
        {
            ViewBag.Title = "订单详情";
            return View();
        }
        public IActionResult MyOrderList()
        {
            ViewBag.Title = "订单列表";
            var wxcookie = GetWXCookie(Request.Cookies["WXInfo"]);//获取cookie的微信信息
            ViewBag.openid = wxcookie?.openid;
            ViewBag.debug = Request.Cookies["WX_State"];
            return View();
        }


        /// <summary>
        /// 生成公众号付款的预支付订单信息，并返回给前端json
        /// </summary>
        /// <param name="treatId">接诊号</param>
        /// <param name="totalAmount">校验用价格</param>
        public async Task<IActionResult> CreateWXPubPay(long treatId, decimal totalAmount)
        {
            /*
           "appId":"wx2421b1c4370ec43b",     //公众号名称，由商户传入     
           "timeStamp":"1395712654",         //时间戳，自1970年以来的秒数     
           "nonceStr":"e61463f8efa94090b1f366cccfbbb444", //随机串     
           "package":"prepay_id=u802345jgfjsdfgsdg888",     
           "signType":"MD5",         //微信签名方式：     
           "paySign":"70EA570631E4BB79628FBCA90534C63FF7FADD89" //微信签名 
             */

            try
            {
                var wxcookie = GetWXCookie(Request.Cookies["WXInfo"]);
                var openid = wxcookie.openid;
                if (openid.IsEmpty()) throw new Exception("openid无值！");
                var treat = MainDbContext.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
                //获取调用CHIS系统的地址
                string lk = string.Format(Global.Config.GetSection("Webfig:WX_PayApiUrl").Value, treatId, totalAmount);
                //获取返回的Json信息
                var jn = await Ass.Net.WebHelper.WebPost(lk);
                Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jn);
                if (jobj.GetValueString("rlt") == "false") return Content(jn, "application/json");//直接json传回
                                                                                                  //整理数据
                var md = new Models.CHIS_Charge_PayPre();
                Newtonsoft.Json.Linq.JToken item = jobj.GetValue("result").GetValue("item");

                string nonce_str = Guid.NewGuid().ToString("N");
                string notify_url = UrlRoot + "/home/WXPubPaySuccessCallBack";//返回url
                string out_trade_no = item.GetValueString("payOrderId");//订单号
                string bill_ip = base.GetClientEnvi().ClientIP; //客户请求IP
                int total = (int)(Ass.P.PDecimal(item.GetValueString("TotalAmount")) * 100);
                string body = $"微信公众号支付[{treat.CustomerId}|{treat.CustomerName}|{treatId}]";

                var wxp = Codes.Utility.WXParams.LoadWXParams("jk813");
                var wx = new Codes.Utility.WXPay(wxp);
                var rlt = await wx.CreateWXPayPubAsync(body, nonce_str, notify_url, openid, out_trade_no, total, bill_ip);
                if (rlt.rlt == false) throw new Exception(rlt.msg);

                return TryCatchFunc((dd) =>
                {
                    dd.appId = rlt.appid;
                    dd.timeStamp = rlt.timeStamp;
                    dd.nonceStr = rlt.nonce_str;
                    dd.prepay_id = rlt.prepay_id;
                    dd.package = $"prepay_id={rlt.prepay_id}";
                    dd.signType = "MD5";
                    dd.sign = rlt.sign;
                    dd.paySign = rlt.paySign;
                    dd.out_trade_no = out_trade_no;//支付订单Id
                                                   // dd.mweb_url = rlt.mweb_url;
                    return null;
                });
            }
            catch (Exception ex) { return TryCatchFunc(() => { throw ex; }); }
        }

        /// <summary>
        /// 支付成功返回调用
        /// </summary>
        public IActionResult WXPubPaySuccessCallBack()
        {
            return View();
        }

        //确认用户的公众号支付是否成功 、  其他状态
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prePayId">微信的支付Id</param>
        /// <param name="payOrderId">项目的支付Id</param>
        /// <returns></returns>
        public async Task<IActionResult> CheckWXPubPayStatusByWX(string payOrderId)
        {
            try
            {
                if (payOrderId.IsEmpty()) throw new Exception("没有输入系统支付订单号");
                //调用微信接口，确认订单真正支付成功
                var wxp = Codes.Utility.WXParams.LoadWXParams("jk813");
                var wx = new Codes.Utility.WXPay(wxp);
                var r = await wx.QueryPubPayStatus(payOrderId);//获取数据
                if (r.rlt == false) throw new Exception(r.msg);//错误则抛出错误
                //调用HIS接口，刷新后面数据库的支付数据状态    
                string lk = string.Format(Global.Config.GetSection("Webfig:WX_PayedCheckApiUrl").Value, payOrderId);
                var jn = await Ass.Net.WebHelper.WebPost(lk);
                Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jn);
                if (jobj.GetValueString("rlt") == "false") return Content(jn, "application/json");//直接json传回
                return TryCatchFunc((dd) =>
                {
                    return null;
                });
            }
            catch (Exception ex) { return TryCatchFunc(() => { throw ex; }); }
        }






    }
}
