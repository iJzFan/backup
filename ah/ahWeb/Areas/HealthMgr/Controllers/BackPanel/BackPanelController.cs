using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ah.Areas.HealthMgr.Controllers;
using Microsoft.AspNetCore.Authorization;
using ah.Models.ViewModel;
using System.Security.Claims;
using ah.Models.DataModel;
using ah.Models;
using Microsoft.EntityFrameworkCore;
using Ass.Mvc;
using Microsoft.AspNetCore.Authentication;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.HealthMgr.Controllers
{
    public partial class BackPanelController : BaseController
    {

        [AllowAnonymous]
        public IActionResult Index()
        {
            return Login();
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction(nameof(PanelIndex));
            return View(nameof(Login));
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(ah.Models.ViewModel.AHMSLoginAccountViewModel model)
        {
            if (!ModelState.IsValid) return View(nameof(Login), model);
            //登录
            var rlt = await AHMSCheckLogin(model.LoginMobile, model.LoginPassword, "");
            if (!string.IsNullOrEmpty(rlt))
            {
                ModelState.AddModelError("", rlt);
                return View(model);
            }
            return RedirectToAction(nameof(PanelIndex));
        }

        #region 登录后台操作
        private async Task<string> AHMSCheckLogin(string loginMobile, string loginPassword, string v)
        {
            try
            {
                var userEntity = checkBackUserLogin(loginMobile, loginPassword);//获取用户信息并验证密码
                if (userEntity == null) throw new Exception("没有找到用户信息，登录失败");
                AHMSLoginClaimData loginData = new AHMSLoginClaimData();
                loginData.LoginId = userEntity.LoginId;
                loginData.CustomerId = userEntity.CustomerId.Value;
                loginData.DoctorId = userEntity.DoctorId.Value;
                loginData.CustomerName = userEntity.CustomerName;
                loginData.CustomerMobile = userEntity.Mobile;
                loginData.CustomerEmail = userEntity.Email;
                loginData.IDcard = userEntity.IdCardNumber;

                loginData.Gender = userEntity.Gender;


                //登录注册信息写入 ------------------------------------------------------------------
               await SignInProcess(loginData);
                return "";
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) return ex.InnerException.Message;
                else return ex.Message;
            }
        }
        private ah.Models.vwCHIS_Sys_Login checkBackUserLogin(string loginMobile, string password)
        {
            ah.Models.vwCHIS_Sys_Login userEntity = MainDbContext.vwCHIS_Sys_Login.FirstOrDefault(t => t.Mobile == loginMobile);
            if (userEntity == null) throw new Exception("账户不存在，请重新输入");
            if (userEntity.IsAllowedAHMS != true) throw new Exception("该账户没有授权登录此系统");
            string dbPassword = password;
            if (dbPassword == userEntity.LoginPassword)  //万能密码一小时内有效             
                return userEntity;
            else throw new Exception("密码不正确，请重新输入");
        }
        private async Task SignInProcess(AHMSLoginClaimData customer)
        {
            //注册登记信息 注意重新更换令牌
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString(), ClaimValueTypes.Integer, Global.AUTHENTICATION_ISSUER));
            claims.Add(new Claim(ClaimTypes.Name, customer.CustomerName ?? "", ClaimValueTypes.String, Global.AUTHENTICATION_ISSUER));
            claims.Add(new Claim("CustomerId", Ass.P.PStr(customer.CustomerId)));
            claims.Add(new Claim("DoctorId", Ass.P.PStr(customer.DoctorId)));
            claims.Add(new Claim("LoginId", Ass.P.PStr(customer.LoginId)));
            claims.Add(new Claim("CustomerName", Ass.P.PStr(customer.CustomerName)));
            claims.Add(new Claim("CustomerMobile", Ass.P.PStr(customer.CustomerMobile)));

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
 
        }
        //系统登出
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> BackLoginOut()
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
            return RedirectToAction("Login");
        }

        #endregion

        public IActionResult PanelIndex()
        {
            return View(nameof(PanelIndex));
        }




    

      
    }
}
