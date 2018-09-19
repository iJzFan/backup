using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ah.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Authorization;
using ah.Models.ViewModel;
using System.Security.Claims;
using ah.Models.DataModel;
using ah.Models;
using ah.Areas.Customer.Controllers.Base;
using Microsoft.EntityFrameworkCore;
using Ass;
using Microsoft.AspNetCore.Authentication;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{    
    public partial class HomeController : BaseController
    {
        public HomeController(ah.DbContext.AHMSEntitiesSqlServer db) : base(db) { }
    public IActionResult Index()
        {
            //如果能获取到微信的cookie，则是微信登录
            return View();
            /*
            string logintype = "pc";
            string wx_openid = null;

            try { var wx = GetWXCookie(); if (wx != null && wx.openid.IsNotEmpty()) { logintype = "wx"; wx_openid = wx.openid; } }
            catch { }

            if (logintype == "wx")
            {
                return RedirectToAction(nameof(MobileFrontPage), new { openid = wx_openid });
            }
            else
                return View(nameof(Index));
                */
            //  return RedirectToAction("MyReservationList", "Home");
        }


        //患者用户登录界面
        [AllowAnonymous]
        [HttpGet]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> CustomerLogin(bool bForceLogin = false,string returnUrl = null)
        {

            if (bForceLogin) return await CustomerLoginOut();

            //检测是否是微信端登录，如果是，则直接进入入口
            var envi = GetClientEnvi();
            Global.WriteLog(envi, isToJson: true);
            if (envi.ClientType == "wx")
            {
                return RedirectToAction("WechatEntry", new { isdebug = (Request.Cookies["WX_State"] == "debug" ? 1 : 0),returnUrl = returnUrl });
            }

            if (User.Identity.IsAuthenticated)
            {
                //如果cookie里有callback则返回到callback
                var cbk = CallBackRedirectByCookie(); if (cbk != null) return cbk;
                return RedirectToAction(nameof(MyReservationList));
            }
            Models.ViewModel.AccountLoginViewModel model = null;
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CustomerLogin(Models.ViewModel.AccountLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var rlt = await CheckLogin(model.CustomerName, model.CustomerPassword, "");
            if (!string.IsNullOrEmpty(rlt))
            {
                ModelState.AddModelError("", rlt);
                return View(model);
            }

            var cbk = CallBackRedirectByCookie(); if (cbk != null) return cbk;

            if (model.ReturnUrl.IsNotEmpty()) return Redirect(System.Net.WebUtility.UrlDecode(model.ReturnUrl));
            return RedirectToAction(nameof(MyReservationList));

        }

        //患者找回密码
        [AllowAnonymous]
        public IActionResult RetrievePassword()
        {
            return View();
        }

        /// <summary>
        /// 患者找回密码
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult CustEditPWD(string moblie, string password, string rePassWord, string mobCode)
        {
            try
            {
                if (moblie == null) throw new Exception("请输入正确的手机号码");
                var cust = MainDbContext.CHIS_Sys_Login.FirstOrDefault(m => m.Mobile == moblie);
                if (cust == null) throw new Exception("不存在该用户，不能修改");
                var code = MainDbContext.CHIS_DataTemp_SMS.OrderByDescending(m => m.CreatTime).FirstOrDefault(m => m.PhoneCode == moblie);
                if (mobCode != code.VCode) throw new Exception("验证码不正确，找回密码失败");
                if (password != rePassWord) throw new Exception("两次输入密码不一致，请重新输入");
                cust.LoginPassword = password;
                MainDbContext.SaveChanges();
                return Json(new
                {
                    rlt = true,
                    msg = "修改成功，返回登录"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    rlt = false,
                    msg = ex.Message
                });
            }


        }
        /// <summary>
        /// 生成手机6位验证码并发送给该手机
        /// <returns></returns>
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Json_SendMobCode(string mob)
        {
            try
            {
                string contents = "";
                Random rad = new Random();//实例化随机数产生器rad；
                int code = rad.Next(100000, 1000000);//用rad生成大于等于100000，小于等于999999的随机数；          
                var sms = new ah.Models.CHIS_DataTemp_SMS();
                sms.PhoneCode = mob;
                sms.VCode = code.ToString();
                sms.CreatTime = DateTime.Now;
                MainDbContext.CHIS_DataTemp_SMS.Add(sms);
                MainDbContext.SaveChanges();
                mob = mob.Replace(" ", "");
                var s = new SMS();
                var h = MainDbContext.CHIS_DataTemp_SMS.OrderByDescending(m => m.CreatTime).FirstOrDefault(m => m.PhoneCode == mob);
                contents = $"{h.VCode}找回密码验证码【天使健康】";
                string rlt = await s.PostSmsInfo(mob, contents);
                if (rlt != "true") new Exception(rlt);
                return Json(new { rlt = true, msg = "" });
            }
            catch (Exception e)
            { return Json(new { rlt = false, msg = e.Message }); }
        }

        //显示用户基本资料
        public IActionResult ShowCustomerInfo()
        {
            var user = GetCurrentLoginUser;
            var Customer = MainDbContext.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == user.CustomerId);
            var CustomerHealthInfo = MainDbContext.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == user.CustomerId);
            var model = new CustomerInfo();
            model.Customer = Customer;
            model.CustomerHealthInf = CustomerHealthInfo;
            ViewBag.IsEditCus = true;
            return View(model);
        }
        //修改用户信息和健康信息
        public IActionResult EditCustomerInfo(CustomerInfo model)
        {
            return TryCatchFunc(() =>
            {
                var user = GetCurrentLoginUser;
                if (model == null && model.Customer != null) throw new Exception("不存在该用户");
                if (model.Customer.CustomerID > 0)
                {
                    var cus = MainDbContext.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == user.CustomerId);
                    if (cus == null) throw new Exception("不存在该用户");
                    cus.NickName = model.Customer.NickName;
                    cus.CustomerName = model.Customer.CustomerName;
                    cus.Telephone = model.Customer.Telephone;
                    cus.Gender = model.Customer.Gender;
                    cus.Address = model.Customer.Address;
                    cus.IDcard = model.Customer.IDcard;
                    cus.Birthday = model.Customer.Birthday;
                    cus.sysLatestActiveTime = DateTime.Now;
                    //更新实体                
                    var health = MainDbContext.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == user.CustomerId);
                    if (health == null)
                    {
                        var healths = new ah.Models.CHIS_Code_Customer_HealthInfo();
                        healths = model.CustomerHealthInf;
                        healths.CustomerId = user.CustomerId;
                        healths.BloodType = model.CustomerHealthInf.BloodType;
                        healths.Allergic = model.CustomerHealthInf.Allergic;
                        MainDbContext.CHIS_Code_Customer_HealthInfo.Add(healths);
                    }
                    else
                    {
                        health.Height = model.CustomerHealthInf.Height;
                        health.Weight = model.CustomerHealthInf.Weight;
                        health.BloodType = model.CustomerHealthInf.BloodType;
                        health.Allergic = model.CustomerHealthInf.Allergic;
                    }
                    MainDbContext.SaveChanges();
                }
                return RedirectToAction("ShowCustomerInfo");
            });
        }


        public IActionResult CallCenter()
        {
            return View();
        }




        public IActionResult ManageMessage()
        {
            return View();
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="password"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        #region 新的登录

        //登录检查  如果成功则返回空，否则就是错误信息
        public async Task<string> CheckLogin(string loginId, string password, string code)
        {
            try
            {
                var userEntity = checkUserLogin(loginId, password);//获取用户信息并验证密码
                if (userEntity == null) throw new Exception("没有找到用户信息，登录失败");
                CustomerClaimData cusData = new CustomerClaimData();
                cusData.LoginId = userEntity.LoginId;
                cusData.CustomerId = userEntity.CustomerId.Value;
                cusData.CustomerName = userEntity.CustomerName;
                cusData.CustomerMobile = userEntity.Mobile;
                cusData.CustomerEmail = userEntity.Email;
                cusData.Gender = userEntity.Gender;
                cusData.IDcard = userEntity.IdCardNumber;

                //登录注册信息写入 ------------------------------------------------------------------
               await SignInProcess(cusData);
                return "";
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) return ex.InnerException.Message;
                else return ex.Message;
            }
        }



        private ah.Models.vwCHIS_Sys_Login checkUserLogin(string loginId, string password)
        {
            ah.Models.vwCHIS_Sys_Login userEntity = null;
            if (loginId.GetStringType().IsEmail)
                userEntity = MainDbContext.vwCHIS_Sys_Login.FirstOrDefault(t => t.Email == loginId);
            if (loginId.GetStringType().IsMobile)
                userEntity = MainDbContext.vwCHIS_Sys_Login.FirstOrDefault(t => t.Mobile == loginId);
            if (loginId.GetStringType().IsIdCardNumber)
                userEntity = MainDbContext.vwCHIS_Sys_Login.FirstOrDefault(t => t.IdCardNumber == loginId);

            if (userEntity == null) throw new Exception("账户不存在，请重新输入");
            string dbPassword = password;
            if (dbPassword == userEntity.LoginPassword)  //万能密码一小时内有效             
                return userEntity;
            else throw new Exception("密码不正确，请重新输入");
        }


        internal async Task SignInProcess(CustomerClaimData customer)
        {
            //注册登记信息
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString(), ClaimValueTypes.Integer, Global.AUTHENTICATION_ISSUER));
            claims.Add(new Claim(ClaimTypes.Name, customer.CustomerName ?? "", ClaimValueTypes.String, Global.AUTHENTICATION_ISSUER));
            claims.Add(new Claim("LoginId", customer.LoginId.ToString()));
            claims.Add(new Claim("CustomerId", customer.CustomerId.ToString()));
            claims.Add(new Claim("CustomerMobile", Ass.P.PStr(customer.CustomerMobile)));
            claims.Add(new Claim("CustomerEmail", Ass.P.PStr(customer.CustomerEmail)));
            claims.Add(new Claim("IDCard", Ass.P.PStr(customer.IDcard)));


            var userIdentity = new ClaimsIdentity(Global.AUTHENTICATION_CLAIMS_IDENTITY);//其他都可以，主要獲取時候方便
            userIdentity.AddClaims(claims);

            //驗證書
            var userPrincipal = new ClaimsPrincipal(userIdentity);



            //註冊登錄信息
            await HttpContext.SignInAsync(Global.AUTHENTICATION_SCHEME, userPrincipal,
                  new AuthenticationProperties
                  {
                      ExpiresUtc = DateTime.UtcNow.AddMinutes(60),
                      //  IsPersistent = true,
                      //  AllowRefresh = false
                  });


            //登录后的基本信息放入公共CustomerInfo里
            var dcus = MainDbContext.vwCHIS_Code_Customer.AsNoTracking().FirstOrDefault(m => m.CustomerID == customer.CustomerId);
            var coption = new Microsoft.AspNetCore.Http.CookieOptions { Domain = ".jk213.com" };
#if DEBUG
            coption.Domain = "localhost";
#endif
            Response.Cookies.Append("CUSTOMER_INFO", Ass.Data.Secret.Encript(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                CustomerName = customer.CustomerName,
                CustomerId = customer.CustomerId,
                Gender = customer.Gender,
                CustomerMobile = customer.CustomerMobile,
                CustomerEmail = dcus.Email,
                Birthday = dcus.Birthday,
                MariageStatusId = dcus.Marriage,
                MariageStatusName = dcus.MarriageStatus,
            }), "tsjk@2018"), coption);
        }

        //系统登出
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> CustomerLoginOut()
        {

            try
            {
                await HttpContext.SignOutAsync(Global.AUTHENTICATION_SCHEME);
                this.HttpContext.Session.Clear();
            }
            catch (Exception ex)
            {
                var e = ex;
            }
            return RedirectToAction("CustomerLogin");
        }
        #endregion


        [AllowAnonymous]
        public IActionResult debug()
        {
            return View();
        }
    }
}
