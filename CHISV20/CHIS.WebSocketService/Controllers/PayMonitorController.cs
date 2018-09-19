using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CHIS.WebSocketService.Models;
using CHIS.WebSocketService.Middlewares;
using Microsoft.Net.Http.Headers;
using CHIS.Models.ViewModel;

namespace CHIS.WebSocketService.Controllers
{
    public class PayMonitorController : BaseController
    {
        PayStationWebSocketHandler _wsHandler;
        PayStationWebSocketConnectionMgr _wsconnMgr;
        PayQrWebSocketHandler _wsqrHandler;
        PayQrWebSocketConnectionMgr _wsqrConnMgr;
        public PayMonitorController(PayStationWebSocketHandler wsHandler,
            PayStationWebSocketConnectionMgr wsconnMgr,
            PayQrWebSocketHandler wsqrHandler,
            PayQrWebSocketConnectionMgr wsqrConnMgr
            )
        {
            _wsHandler = wsHandler;
            _wsconnMgr = wsconnMgr;
            _wsqrHandler = wsqrHandler;
            _wsqrConnMgr = wsqrConnMgr;
        }

        #region 测试与Demo
        public IActionResult Demo()
        {
            return View();
        }


        public IActionResult Test()
        {
            return View();
        }

        public IActionResult RdReadme()
        {
            return View();
        }

        public IActionResult TestSet()
        {
            return View(new PaySetModel
            {
                StationId = 10,
                DoctorId = 9,
                payOrderId = "201711291509-10-2055-507"
            });
        }

        [HttpPost]
        public IActionResult TestSet(PaySetModel model)
        {
            _wsHandler.SetNewPayOrderId(model.GetEncriptCode());
            ViewBag.RltMsg = "设置成功！";
            return View(model);
        }
        #endregion

        #region 正式的接口
        //通知支付改变了
        public IActionResult NotifyPayChanged(string code)
        {
            //code 里面包含 StationId,DoctorId,PayOrderId
            //格式为   StationId|DoctorId|PayOrderId
            //返回string：true/false
            //向Socket监视器里，写入当前监视信息
            //code= System.Net.WebUtility.UrlDecode(code);
            _wsHandler.SetNewPayOrderId(code);
            var rtn = "true";
            return Content(rtn);
        }
        public async Task<IActionResult> NotifyPayQrResult(string payOrderId, bool bSuccess, string errMsg)
        {
            await _wsqrHandler.SetOrderPayedStatus(payOrderId, bSuccess, errMsg);
            return Content("true");
        }




        public async Task<IActionResult> QrUnionScaned(string code)
        {
            try
            {
                //探测二维码
                string[] a = Ass.Data.Secret.Decript(code, Global.SYS_ENCRIPT_PWD).Split('|');
                string md5 = a[0];
                int amount = Ass.P.PInt(a[1]);
                string payOrderId = a[2];
                var socket = _wsqrConnMgr.GetItem(_wsqrConnMgr.GetId(payOrderId));
                //md5 检测
                string _md5 = Ass.Data.Secret.MD5($"{socket.WxQrUrl}|{socket.AliQrUrl}|{amount}|{payOrderId}");
                if (_md5 != md5) throw new Exception("MD5校验错误");
                //检测扫码方式
                var scanType = "";
                //侦测扫码方式 通过分析UserAgent
                string userAgent = Request.Headers[HeaderNames.UserAgent].ToString();
                if (userAgent.IndexOf("alipayclient", StringComparison.CurrentCultureIgnoreCase) > 0) scanType = "ALI";
                else if (userAgent.IndexOf("micromessenger", StringComparison.CurrentCultureIgnoreCase) > 0) scanType = "WX";
                switch (scanType)
                {
                    case "WX":
                        if (string.IsNullOrWhiteSpace(socket.WxQrUrl)) throw new Exception("微信支付链接不存在！");
                        else
                        {                            
                            return View("PayCheck", new PayRedirectInfo
                            {
                                payType = scanType,
                                payAmount = socket.PayAmount,
                                payOrderId = socket.PayOrderId,
                                payAliQrUrl = socket.AliQrUrl,
                                payWxQrUrl = socket.WxQrUrl,
                                payWxH5CreateUrl = "/PayMonitor/WxH5PayCreate?payOrderId="+payOrderId,//获取微信的H5发起地址                              
                                ClientIP=base.ClientIP
                            });
                        }
                    case "ALI":
                        if (string.IsNullOrWhiteSpace(socket.AliQrUrl)) throw new Exception("支付宝链接不存在！");
                        else
                        {
                            HttpContext.Response.Redirect(socket.AliQrUrl);
                            return View("PayCheck", socket.AliQrUrl);
                        }                     
                    default:        
                        break;
                  
                }                
                return View("PayCheck", new PayRedirectInfo
                {
                    payType = "",
                    payAmount = socket.PayAmount,
                    payOrderId = socket.PayOrderId,
                    payAliQrUrl = socket.AliQrUrl,
                    payWxQrUrl = socket.WxQrUrl,
                    payWxH5CreateUrl = "/PayMonitor/WxH5PayCreate?payOrderId=" + payOrderId, //获取微信的H5发起地址
                    ClientIP = base.ClientIP
                });
            }
            catch (Exception ex)
            {
                return Content("错误：" + ex.Message);
            }
        }
        public async Task<IActionResult> WxH5PayCreate(string payOrderId)
        {
            try
            {
                CHIS.Models.PayWxH5Info payWxH5 = null; //获取微信的H5发起地址                
                var ipAddress = ClientIP;
                var socket = _wsqrConnMgr.GetItem(_wsqrConnMgr.GetId(payOrderId));
                payWxH5 = await _wsqrHandler.GetWxPayH5Info(socket, ipAddress);
                //return Content($"{ipAddress}{payWxH5.wxH5Url}");
                Response.Redirect(payWxH5.wxH5Url);
                return new EmptyResult();
            }
            catch(Exception ex)
            {
                return Content("支付错误：",ex.Message);
            }
        }

        #endregion


        #region 后台监视
        public IActionResult PayMonitorSumary()
        {
            ViewBag.MonitorNum = _wsconnMgr.MonitorSocketCount;
            ViewBag.QrSocketNum = _wsqrConnMgr.MonitorSocketCount;
            ViewBag.QrSocket = _wsqrConnMgr;
            return View();
        }

        #endregion


    }
}
