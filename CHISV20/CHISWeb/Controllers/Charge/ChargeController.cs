
using Alipay.AopSdk.AspnetCore;
using Alipay.AopSdk.F2FPay.AspnetCore;
using Ass;
using CHIS.Models;
using CHIS.Models.ViewModel;
using CHIS.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.IO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers.Charge
{
    [Authorize]
    public partial class ChargeController : BaseController
    {
        #region 构造注入函数   
        private IConfiguration _config;
        Codes.Utility.XPay.AliPay _aliPay;
        private CHIS.DbContext.CHISEntitiesSqlServer db;
        ChangePayService _paySvr;
        LoginService _loginSvr;
        NotifyService _notifySvr;
        WeChatService _weChatSvr;
        DoctorService _docSvr;
        AccessService _accSvr;
        public ChargeController(Codes.Utility.XPay.AliPay aliPay,
            IConfiguration config, CHIS.DbContext.CHISEntitiesSqlServer db,
            ChangePayService paySvr
            ,LoginService loginSvr
            ,NotifyService notifySvr
            ,WeChatService weChatSvr
            ,AccessService accSvr
            , DoctorService docSvr) : base(db)
        {
            _aliPay = aliPay;
            _config = config;
            this.db = db;
            _paySvr = paySvr;
            _loginSvr = loginSvr;
            _notifySvr = notifySvr;
            _weChatSvr = weChatSvr;
            _docSvr = docSvr;
            _accSvr = accSvr;
        }


        #endregion

        public IActionResult CHIS_Charge()
        {
            DateTime dt0 = new DateTime(), dt1 = new DateTime();
            var days = 1;
#if DEBUG
            days = 100;
#endif
            base.initialData_TimeRange(ref dt0, ref dt1, days);

            int stationId = UserSelf.StationId;
            int doctorId = UserSelf.DoctorId;

            int? registOpId = null;

            //如果角色是药店护士
            if(UserSelf.MyRoleNames.Contains("drugstore_nurse"))
            {
                registOpId = UserSelf.OpId;
            }

            //是否整个工作站
            var bStation = UserSelf.MyRoleNames.Contains("treat_nurse") || UserSelf.MyRoleNames.Contains("treat_nuse_adv");

            var model = new ChargeMainViewModel
            {
                NeedPayList = _paySvr.GetChargeListNeedPayModel(null, bStation, dt0, dt1, stationId, doctorId,registOpId, 1),
                PayedList = _paySvr.GetChargeListPayedModel(null, bStation, dt0, dt1, stationId, doctorId,registOpId, 1),
                PayIndexModel = _paySvr.GetPayIndexInfo()
            };

            return View(nameof(CHIS_Charge), model);
        }



        #region 支付清单表

        //已经支付
        public IActionResult GetChargeListPayed(string searchText, bool bAllClinic, int pageIndex = 1)
        {
            DateTime dt0 = new DateTime(), dt1 = new DateTime();
            var days = 0;
#if DEBUG
            days = 100; 
#endif
            base.initialData_TimeRange(ref dt0, ref dt1, days);
            int stationId = UserSelf.StationId;
            int doctorId = UserSelf.DoctorId;
            
            //如果角色是药店护士
            int? registOpId = null;
            if (UserSelf.MyRoleNames.Contains("drugstore_nurse"))
            {
                registOpId = UserSelf.OpId;
            }
            var model = _paySvr.GetChargeListPayedModel(searchText, bAllClinic, dt0, dt1, stationId, doctorId,registOpId, pageIndex);
            ViewBag.SearchText = searchText;
            ViewBag.IsAllClinic = bAllClinic;
            return PartialView("_pvChargeListPayed", model);
        }

        //待支付
        public IActionResult GetChargeListNeedPay(string searchText, bool? bAllClinic = null, int pageIndex = 1)
        {
            DateTime dt0 = new DateTime(), dt1 = new DateTime();
            var days = 1;
#if DEBUG
            days = 100;
#endif
            base.initialData_TimeRange(ref dt0, ref dt1, days);
            int stationId = UserSelf.StationId;
            int doctorId = UserSelf.DoctorId;

            //如果角色是药店护士
            int? registOpId = null;
            if (UserSelf.MyRoleNames.Contains("drugstore_nurse"))
            {
                registOpId = UserSelf.OpId;
            }

            if (!bAllClinic.HasValue) bAllClinic = UserSelf.MyRoleNames.Contains("treat_nurse") || UserSelf.MyRoleNames.Contains("treat_nurse_adv");
            var model = _paySvr.GetChargeListNeedPayModel(searchText, bAllClinic.Value, dt0, dt1, stationId, doctorId,registOpId, pageIndex);
            ViewBag.SearchText = searchText;
            ViewBag.IsAllClinic = bAllClinic;
            return PartialView("_pvChargeListNeedPay", model);
        }


        #endregion


        #region 载入收费信息


        //载入详细页面 按接诊Id计算支付
        public async Task<IActionResult> LoadCustomerNeedPayDetail(long treatId)
        {
            try
            {
                var model = await _paySvr.GetNeedPayInfoAsync(treatId);
                return PartialView("_pvChargeDetails", model);
            }
            catch(Exception ex) { return ExceptionPartialView(ex); }
        }

        //详细默认页
        public IActionResult LoadChargeIndex()
        {
            PayIndexModel model = _paySvr.GetPayIndexInfo();
            return PartialView("_pvChargeIndex", model);
        }

        //载入详细页面 按接诊Id计算支付
        public IActionResult LoadCustomerPayedDetail(long payedId)
        {
            var model = _paySvr.GetPayedInfo(payedId);
            return PartialView("_pvChargeDetailsPayed", model);
        }





        #endregion


        //修改邮寄地址
        public IActionResult ChangeTreatAddress(long treatId, long addressId)
        {
            return TryCatchFunc(() =>
            {
                if (treatId == 0) throw new Exception("没有传入接诊Id");
                if (addressId == 0) throw new Exception("没有传入邮寄地址Id");

                db.CHIS_DoctorTreat.Find(treatId).TransferAddressId = addressId;
                db.SaveChanges();
                var addr = db.vwCHIS_Code_Customer_AddressInfos.Find(addressId);
                var finds = db.CHIS_Doctor_ExtraFee.Where(m => m.TreatId == treatId);
                foreach (var item in finds)
                {
                    item.MailAddressInfoId = addr.AddressId;
                    item.MailToAreaId = addr.AreaId;
                }
                db.SaveChanges();
                return null;
            });
        }



        #region 支付版面生成

        //支付
        public async Task<IActionResult> ChargePayment(long treatId, string payRemark, bool isReOrder = false)
        {
            try
            {
                var u = UserSelf;
                //获取待支付的内容，生成二维码订单
                var xjrlt = await _paySvr.CreateCashOrder(treatId, payRemark, isReOrder,u.OpId,u.OpManFullMsg); //现金收款必须生成          
                var wxrlt = await _paySvr.CreateWX2DCode(treatId, payRemark, xjrlt.prepay,u.OpId,u.OpManFullMsg);
                var alirlt = await _paySvr.CreateAli2DCode(treatId, payRemark, xjrlt.prepay,u.OpId,u.OpManFullMsg);
              
                /*
                  rlt = true,
                  msg = "",
                  code_url = 
                  prepay_id =
                  trade_type 
                  payOrderId 
                  total_fee =
                 */
                var model = new CHIS.Models.ViewModel.PaymentViewModel();
                if (xjrlt.rlt)
                {
                    model.Amount = Ass.P.PDecimalV(xjrlt.total_fee);
                    model.PayOrderId = xjrlt.payOrderId;
                    model.IsAllowedCashPay = xjrlt.isAllowedCashPay;
                    model.TreatId = xjrlt.treatId;
                }
                else return View("Error", new Exception(wxrlt.msg));

                if (wxrlt.rlt == true)
                {
                    model.WxQrcodeString = wxrlt.code_url;
                }
                if (alirlt.rlt == true)
                {
                    model.AliQrcodeString = alirlt.code_url;
                }
                await _notifySvr.SWSNotifyPayChangeAsync(model.PayOrderId);
                return xjrlt.rlt ? View(nameof(ChargePayment), model) : View("Error", new Exception($"WX:{wxrlt.msg};Ali:{alirlt.msg}"));
            }
            catch (Exception ex)
            {
                if (ex is SuccessedException)
                {
                    return View("Successed", "该订单已经成功支付");
                }
                return View("Error", ex);
            }
        }


        //支付取消
        public async Task<IActionResult> ChargePaymentCancel(string payOrderId)
        {
            if (payOrderId.IsNotEmpty())
                await _notifySvr.SWSNofityPayedStatusAsync(payOrderId, false, "后台付款主动取消");
            return Json(new { rlt = true });
        }


        //支付成功
        public async Task<IActionResult> ChargePaymentSuccess(PaySuccessCheck model)
        {
            try
            {
                var test = "";
#if DEBUG
                test = "【测试】";
#endif

                if (!model.HasPayOrderId) throw new Exception("没有传入支付订单号");

                if (model.IsCash)
                {
                    var n = db.CHIS_Charge_Pay.Where(m => m.PayOrderId == model.PayOrderId).Count();
                    if (model.GetCashAmount - model.ReturnCashAmount != model.PayAmount) throw new Exception("支付金额错误！收入找零与支出不对");
                    if (n == 0)
                    {
                        await _paySvr.UpdatePayedAsync(model.PayOrderId, FeeTypes.Cash, $"收:{model.GetCashAmount};零:{model.ReturnCashAmount}", true, UserSelf.OpId, UserSelf.OpMan);
                        await Logger.WriteInfoAsync("CHARGE", "ChargePaymentSuccess", $"{test}更新现金支付{model.PayOrderId}");
                    }
                    goto paySuccess;//转入支付成功
                }


                WxPayRlt wxrlt = new WxPayRlt();//微信付款结果查询
                AliPayRlt alirlt = new AliPayRlt();

                if (test != "【测试】")
                {
                    //搜索腾讯的数据
                    var wxqrrlt = await new Codes.Utility.XPay.WXPay().QueryPayStatus(model.PayOrderId, "WXQR");
                    var wxpubrlt = await new Codes.Utility.XPay.WXPay().QueryPayStatus(model.PayOrderId, "WXPUB");
                    var wxh5rlt = await new Codes.Utility.XPay.WXPay().QueryPayStatus(model.PayOrderId, "WXH5");
                    wxrlt.IsQrRlt = wxqrrlt.rlt; wxrlt.QrMsg = wxqrrlt.msg;
                    wxrlt.IsPubRlt = wxpubrlt.rlt; wxrlt.PubMsg = wxpubrlt.msg;
                    wxrlt.IsH5Rlt = wxh5rlt.rlt; wxrlt.H5Msg = wxh5rlt.msg;

                    await Logger.WriteInfoAsync("Charge", "ChargePaymentSuccess", $"{wxrlt}|{alirlt}");
                    //主动搜索微信的数据
                    var aliqrrlt = await _aliPay.QueryPayStatus(model.PayOrderId, "ALIQR");
                    alirlt.IsPayed = aliqrrlt.rlt; alirlt.PayMsg = aliqrrlt.msg;
                    if (!wxrlt.IsPayed && !alirlt.IsPayed)
                    {
                        if (wxrlt.IsNotPay && alirlt.IsNotPay) throw new CodeException("PAYING", "支付中");//不发送信息给前端                      
                        throw new Exception("微信/支付宝支付失败");
                    }
                }
                else
                {
                    alirlt.IsPayed = true;//测试支付宝
                }

                if (wxrlt.IsPayed) //更新微信支付
                {
                    var n = db.CHIS_Charge_Pay.Where(m => m.PayOrderId == model.PayOrderId).Count();
                    if (n == 0)
                    {
                        await _paySvr.UpdatePayedAsync(model.PayOrderId, wxrlt.feeType, null, false, UserSelf.OpId, UserSelf.OpMan);
                        await Logger.WriteInfoAsync("CHARGE", "ChargePaymentSuccess", $"{test}手动更新微信支付{model.PayOrderId}");
                    }
                }

                if (alirlt.IsPayed)//更新支付宝操作
                {
                    var n = db.CHIS_Charge_Pay.Where(m => m.PayOrderId == model.PayOrderId).Count();
                    if (n == 0)
                    {
                        await _paySvr.UpdatePayedAsync(model.PayOrderId, alirlt.feeType, null, false, UserSelf.OpId, UserSelf.OpMan);
                        await Logger.WriteInfoAsync("CHARGE", "ChargePaymentSuccess", $"{test}手动更新支付宝支付{model.PayOrderId}");
                    }
                }

                paySuccess:
                await _notifySvr.SWSNofityPayedStatusAsync(model.PayOrderId, true, "支付成功！");
                bool bAutoSendDrug = _accSvr.GetMyConfig("DefAutoSendDrugs",nullDefVal:"True") == "True";              
                return Ok(new { rlt = true,  bAutoSendDrug,model.PayOrderId });
            }
            catch (Exception ex)
            {
                if ((ex is CodeException) && (((CodeException)ex).ExceptionCode == "PAYING"))
                {
                    //不发送信息给前端
                    return Json(new { rlt = false, msg = ex.Message });
                }
                await _notifySvr.SWSNofityPayedStatusAsync(model.PayOrderId, false, "支付失败:" + ex.Message);
                return Json(new { rlt = false, msg = ex.Message });
            }
        }

        #endregion





        #region 微信 收款 数据生成

        [AllowAnonymous]
        public async Task<IActionResult> GetWxH5PayInfo(string payOrderId, string ipAddress)
        {
            try
            {
                var ip0 = ipAddress;
                if (ipAddress.IsEmpty()) ipAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                _paySvr.CheckIsPayed(payOrderId);//检测是否支付成功过了
                var fd = _paySvr.FindPayPreInfo(payOrderId);
                //根据微信操作类获取微信的二维码  此处会抛错出来      
                var rlt = await new Codes.Utility.XPay.WXPay().GetH5PayInfoAsync(
                    Guid.NewGuid().ToString("N"),
                    $"微信支付-会员[{fd.CustomerId}]{fd.CustomerName}",
                    payOrderId,
                    (int)(fd.CHIS_Charge_PayPre.TotalAmount * 100),
                    UrlRoot + "/Charge/WXH5PaySuccessCallBack", ipAddress);
                Logger.WriteInfo($"{ip0}-{ipAddress}:{rlt}");
                CHIS.Models.PayWxH5Info rtn = new PayWxH5Info
                {
                    msg = rlt.msg,
                    rlt = rlt.rlt,
                    wxH5Url = rlt.mweb_url,
                    payOrderId = rlt.payOrderId,
                    totalAmount = rlt.total_fee
                };
                return Json(rtn);
            }
            catch (Exception ex)
            {
                CHIS.Models.PayWxH5Info rtn = new PayWxH5Info
                {
                    msg = ex.Message,
                    rlt = false,
                    status = "ERROR"
                };
                return Json(rtn);
            }
        }


        /// <summary>
        /// 二维码扫码后的返回处理地址 腾讯会调用
        /// 要允许匿名访问才可以的
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> WXQRPaySuccessCallBack()
        {

            Func<bool, string, ContentResult> wxCallBack = (bSuccess, msg) =>
              {
                  var s = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                  if (!bSuccess)
                      s = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[" + msg + "]]></return_msg></xml>";
                  return Content(s, "text/xml");
              };

            string payOrderId = "";
            string fn = "WXQRPaySuccessCallBack", op = "WxCallBack";
            try
            {
                //获取回调的参数                
                var xmldict = new Codes.Utility.XPay.WXPay().GetXmlResult(Request.Body);
                Logger.WriteInfo($"回传信息:{xmldict}", 0, op);
                payOrderId = xmldict.Get("out_trade_no");
                if (string.IsNullOrWhiteSpace(payOrderId)) throw new Exception("获取的支付单号为空");
                _paySvr.CheckIsPayed(payOrderId);//检查订单是否成功调用
                await _paySvr.UpdatePayedAsync(payOrderId, FeeTypes.WeChat_QR, null, false, 0, op);
                //调用支付的WebSocket  
                await _notifySvr.SWSNofityPayedStatusAsync(payOrderId, true, "微信支付成功!");
                Logger.WriteInfo($"订单{payOrderId}成功调用微信支付回传", 0, op);
                return wxCallBack(true, null);
            }
            catch (Exception ex)
            {
                //如果历史成功，则回调
                if (ex is SuccessedException) return wxCallBack(true, null);

                if (payOrderId.IsNotEmpty())
                {
                    await _notifySvr.SWSNofityPayedStatusAsync(payOrderId, false, ex.Message);
                }
                Logger.WriteError(new Exception($"订单{payOrderId}:" + ex.Message), 0, op);
                return wxCallBack(false, ex.Message);
            }

        }

        /// <summary>
        /// 二维码扫码后的返回处理地址 腾讯会调用
        /// 要允许匿名访问才可以的
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> WXH5PaySuccessCallBack()
        {

            Func<bool, string, ContentResult> wxCallBack = (bSuccess, msg) =>
            {
                var s = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                if (!bSuccess)
                    s = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[" + msg + "]]></return_msg></xml>";
                return Content(s, "text/xml");
            };

            string payOrderId = "";
            string fn = "WXH5PaySuccessCallBack", op = "WxH5CallBack";
            try
            {

                //获取回调的参数                
                var xmldict = new Codes.Utility.XPay.WXPay().GetXmlResult(Request.Body);
                Logger.WriteInfo($"回传Body信息:{await base.GetRequestBody(Request.Body)}", 0, op);
                Logger.WriteInfo($"回传信息:{xmldict}", 0, op);
                payOrderId = xmldict.Get("out_trade_no").Replace("_WXH5", "");
                if (string.IsNullOrWhiteSpace(payOrderId)) throw new Exception("获取的支付单号为空");
                await _paySvr.UpdatePayedAsync(payOrderId, FeeTypes.WeChat_H5, null, false, 0, op);
                //调用支付的WebSocket  
                await _notifySvr.SWSNofityPayedStatusAsync(payOrderId, true, "微信支付成功!");
                Logger.WriteInfo($"订单{payOrderId}成功调用微信支付回传", 0, op);
                return wxCallBack(true, null);
            }
            catch (Exception ex)
            {
                //如果历史成功，则回调
                if (ex is SuccessedException) return wxCallBack(true, null);
                if (payOrderId.IsNotEmpty())
                {
                    await _notifySvr.SWSNofityPayedStatusAsync(payOrderId, false, ex.Message);
                }
                Logger.WriteError(new Exception($"订单{payOrderId}:" + ex.Message), 0, op);
                return wxCallBack(false, ex.Message);
            }

        }




        /// <summary>
        /// 生成微信公众号支付
        /// </summary>
        /// <param name="treatId"></param>
        /// <param name="payRemark"></param>
        /// <returns>成功返回的item是预支付订单信息 CHIS_Charge_PayPre 可以强命名转换</returns>
        private async Task<dynamic> CreateWXPubPayInfo(long treatId, string payRemark, decimal amount)
        {
            string fn = "CreateWXPubPayInfo", op = "WxPubCallBack";
            try
            {
                var cus = db.vwCHIS_DoctorTreat.FirstOrDefault(m => m.TreatId == treatId);
                var propay = await _paySvr.CreatePayPreAsync(treatId, payRemark, 0, "WxPub");
                if (propay.PayStatus == 1) throw new Exception("该笔支付已经支付成功了。");
                if (propay.TotalAmount != amount) throw new Exception("金额核对错误");
                Logger.WriteInfo($"发起微信公众号支付{propay.PayOrderId}【￥{propay.TotalAmount}】", 0, op);
                return new { rlt = true, msg = "请求支付成功！", item = propay };
            }
            catch (Exception ex)
            {
                Logger.WriteError(new Exception("微信公众号支付失败:" + ex.Message), 0, op);
                return new { rlt = false, msg = ex.Message };
            }
        }

        public async Task<IActionResult> GetWxPay2DCode(long treatId, string payRemark)
        {
            var rlt = await _paySvr.CreateWX2DCode(treatId, payRemark);
            if (rlt.rlt) await _notifySvr.SWSNotifyPayChangeAsync(rlt.payOrderId);
            return Json(rlt);
        }




        #endregion
        #region 支付宝 收款 数据生成

        /// <summary>
        /// 二维码扫码后的返回处理地址 腾讯会调用
        /// 要允许匿名访问才可以的
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> AliQrPaySuccessCallBack()
        {
            //支付宝要求回传 success/fail
            string payOrderId = ""; string fn = nameof(AliQrPaySuccessCallBack), op = "AliCallBack";
            try
            {
                var tradeStatus = Request.Form["trade_status"];
                payOrderId = Request.Form["out_trade_no"];
                if (tradeStatus == "TRADE_SUCCESS")
                {
                    //获取回调的参数                                                  
                    payOrderId = payOrderId.Replace("_ALIQR", "");
                    if (string.IsNullOrWhiteSpace(payOrderId)) throw new Exception("获取的支付单号为空");
                    _paySvr.CheckIsPayed(payOrderId);//检查支付情况
                    await _paySvr.UpdatePayedAsync(payOrderId, FeeTypes.AliPay_QR, null, false, 0, "AliPayCallBack");
                    await _notifySvr.SWSNofityPayedStatusAsync(payOrderId, true, "支付宝支付成功！");
                    Logger.WriteInfo($"订单{payOrderId}成功调用支付宝支付回传", 0, op);
                    return Content("success");
                }
                else
                    throw new Exception($"{tradeStatus}:交易未成功/关闭/超时");
            }
            catch (Exception ex)
            {
                //如果历史成功，则回调
                if (ex is SuccessedException) return Content("success");
                if (payOrderId.IsNotEmpty() && payOrderId.IndexOf("ALIQR") == -1) await _notifySvr.SWSNofityPayedStatusAsync(payOrderId, false, ex.Message);
                Logger.WriteError(new Exception($"({payOrderId}){ex.Message}"), 0, op);
                return Content("fail");
            }

        }


        //页面点击刷新后重新获取二维码
        public async Task<IActionResult> GetAliPay2DCode(long treatId, string payRemark)
        {
            var rlt = await _paySvr.CreateAli2DCode(treatId, payRemark,null,UserSelf.OpId,UserSelf.OpManFullMsg);
            if (rlt.rlt) await _notifySvr.SWSNotifyPayChangeAsync(rlt.payOrderId);
            return Json(rlt);
        }





        #endregion
/*
        #region 支付后的通知WebSocketService
        /// <summary>
        /// 回调通知支付状态
        /// </summary>
        private async Task SWSNofityPayedStatusAsync(string payOrderId, bool bSuccess, string errMsg = "")
        {
            try
            {
                string url = _config.GetSection("WebSocketService:NotifyPayQrResult").Value;
                errMsg = System.Web.HttpUtility.UrlEncode(errMsg);//编码
                url = url += $"?payOrderId={payOrderId}&bSuccess={bSuccess}&errMsg={errMsg}";
                url = Global.Localhost2Ip(url);
                await Ass.Net.WebHelper.WebPost(url);
            }
            catch (Exception ex)
            {
                string opman = "System"; int opid = 0;
                try { opman = UserSelf.OpManFullMsg; opid = UserSelf.OpId; } catch { }
                Logger.WriteError(ex, opid, opman);
            }
        }
        private async Task SWSNotifyPayChangeAsync(string payOrderId, int? stationId = null, int? doctorId = null)
        {
            try
            {
                if (!stationId.HasValue) stationId = UserSelf.StationId;
                if (!doctorId.HasValue) doctorId = UserSelf.DoctorId;

                string url = _config.GetSection("WebSocketService:NotifyPayChanged").Value;
                var code = Ass.Data.Secret.Encript($"{stationId}|{doctorId}|{payOrderId}", Global.SYS_ENCRIPT_PWD);
                code = System.Net.WebUtility.UrlEncode(code);
                url = url += $"?code={code}";
                url = Global.Localhost2Ip(url);
                await Ass.Net.WebHelper.WebPost(url);
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
            }
        }
     
        
        #endregion

        */





        #region 手机端支付监控
        [AllowAnonymous]
        public async Task<IActionResult> BarcodePayMonitorLogin(string code)
        {
            //实现自动登录

            if (code.IsEmpty()) throw new Exception("请输入信息码");
            string[] ss = Ass.Data.Secret.Decript(code, Global.SYS_ENCRIPT_PWD).Split('|');
            var stationId = Ass.P.PInt(ss[0]);
            var doctorId = Ass.P.PInt(ss[1]);
            if (!User.Identity.IsAuthenticated || (UserSelf.StationId != stationId || UserSelf.DoctorId != doctorId))
            {
                var login = await db.vwCHIS_Sys_Login.AsNoTracking().FirstOrDefaultAsync(m => m.DoctorId == doctorId);
                var userPrincipal = await new HomeController(_db,_loginSvr,_weChatSvr,_docSvr).GetSignInPrincipalAsync(login, stationId, null);
                await HttpContext.SignInAsync(Global.AUTHENTICATION_SCHEME, userPrincipal,
                         new AuthenticationProperties
                         {
                             ExpiresUtc = DateTime.UtcNow.AddMinutes(120),
                             IsPersistent = true,
                             AllowRefresh = true
                         });
            }
            return RedirectToAction("BarcodePayMonitor", new { stationId = stationId, doctorId = doctorId });
        }
        public async Task<IActionResult> BarcodePayMonitor(int stationId, int doctorId)
        {
            var model = new PayMonitorModel()
            {
                Station = await db.CHIS_Code_WorkStation.AsNoTracking().FirstOrDefaultAsync(m => m.StationID == stationId),
                Doctor = await db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefaultAsync(m => m.DoctorId == doctorId)
            };
            return View(model);
        }
        public IActionResult GetBarcodeUrl()
        {
            return TryCatchFunc(d =>
            {
                var code = Ass.Data.Secret.Encript($"{UserSelf.StationId}|{UserSelf.DoctorId}", Global.SYS_ENCRIPT_PWD);
                code = System.Web.HttpUtility.UrlEncode(code);
                d.BarcodePayMonitorUrl = $"{base.UrlRoot}/Charge/{nameof(BarcodePayMonitorLogin)}?code={code}";
                return null;
            });
        }

        //获取支付订单的预支付信息
        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetPayOrderPreInfo(string payOrderId)
        {
            try
            {
                if (payOrderId.IsEmpty()) throw new Exception("没有传入订单号");
                if (db.CHIS_Charge_Pay.AsNoTracking().Where(m => m.PayOrderId == payOrderId).Count() > 0)
                    throw new SuccessedException("该订单已经支付成功过了！");
                var model = db.CHIS_Charge_PayPre.Where(m => m.PayOrderId == payOrderId).FirstOrDefault();
                if (model.PayStatus == 1) throw new SuccessedException("该订单已经支付成功过了！");
                if (model.PayStatus > 1) throw new Exception("支付错误:" + model.PayErrorMsg);

                var cus = db.vwCHIS_DoctorTreat.Where(m => m.TreatId == model.treatId).Select(m => new { CustomerName = m.CustomerName }).FirstOrDefault();

                return Json(new PayQrInfo
                {
                    rlt = true,
                    msg = "获取成功",
                    status = "GETPAYINFO",
                    payOrderId = model.PayOrderId,
                    wx2DCodeUrl = model.wx2DCodeUrl,
                    ali2DCodeUrl = model.ali2DCodeUrl,
                    totalAmount = (int)(model.TotalAmount * 100),
                    isAllowedCashPay = model.IsAllowedCashPay,
                    customerName = cus.CustomerName
                });
            }
            catch (Exception ex)
            {
                if (ex is SuccessedException) return Json(new PayQrInfo { rlt = false, msg = ex.Message, status = "PAYEDSUCCESS" });
                return Json(new PayQrInfo { rlt = false, msg = ex.Message, status = "ERROR" });
            }
        }


        #endregion

    }
}
