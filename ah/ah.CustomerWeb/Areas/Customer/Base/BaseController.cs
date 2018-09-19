using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Ass.Mvc;
using ah.DbContext;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public partial class BaseController : ah.Controllers.BaseController
    {
        public BaseController(AHMSEntitiesSqlServer db) : base(db) { }

        Models.DataModel.CustomerClaimData userData = null;

        public Models.DataModel.CustomerClaimData GetCurrentLoginUser
        {
            get
            {
                if (this.User == null) return null;
                if (User.Identity.IsAuthenticated)
                {
                    if (userData != null) return userData;
                    userData = new Models.DataModel.CustomerClaimData
                    {
                        CustomerName = this.User.Identity.Name


                    };
                    //其它属性
                    ClaimsPrincipal user = this.User;
                    IEnumerable<Claim> claims = user.Claims;
                    foreach (Claim cm in claims)
                    {
                        switch (cm.Type)
                        {

                            case "CustomerId": userData.CustomerId = int.Parse(cm.Value); break;
                            case "LoginId": userData.LoginId = int.Parse(cm.Value); break;
                            case "CustomerName": userData.CustomerName = cm.Value; break;
                            case "CustomerMobile": userData.CustomerMobile = cm.Value; break;
                            case "CustomerEmail": userData.CustomerEmail = cm.Value; break;
                            case "Gender": userData.Gender = int.Parse(cm.Value); break;
                            case "CustomerPWD": userData.CustomerPWD = cm.Value; break;
                            case "IDCard": userData.IDcard = cm.Value; break;
                        }
                    }
                    return userData;
                }



                return null;
            }
        }


        [AllowAnonymous]
        public async Task<IActionResult> CustomerAction(string cusName, string cusMobile, string cusEmail, string callBack, string actionType)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    bool bLogoff = false;
                    if(!string.IsNullOrWhiteSpace(cusMobile))
                    {
                        if (cusMobile != GetCurrentLoginUser.CustomerMobile) bLogoff = true;
                    }

                    if(!string.IsNullOrWhiteSpace(cusEmail))
                    {
                        if (cusEmail != GetCurrentLoginUser.CustomerEmail) bLogoff = true;
                    }

                    if (bLogoff) {
                        await HttpContext.Authentication.SignOutAsync(Global.AUTHENTICATION_SCHEME);
                        this.HttpContext.Session.Clear();
                    }
                }
                catch (Exception ex) { var e = ex; }
            }

            //写入cookie
            var cookieOption = new Microsoft.AspNetCore.Http.CookieOptions()
            {

            };
            HttpContext.Response.Cookies.Append("cusName", Ass.P.PStr(cusName), cookieOption);
            HttpContext.Response.Cookies.Append("cusMobile", Ass.P.PStr(cusMobile), cookieOption);
            HttpContext.Response.Cookies.Append("cusEmail", Ass.P.PStr(cusEmail), cookieOption);
            HttpContext.Response.Cookies.Append("callBack", Ass.P.PStr(callBack), cookieOption);
            HttpContext.Response.Cookies.Append("actionType", Ass.P.PStr(actionType), cookieOption);

            if (actionType == "customer_login")
            {
                return RedirectToAction("CustomerLogin", "Home");
            }

            if (actionType == "customer_regist")
            {
                return RedirectToAction("CustomerRegistPhone", "Home");//用户注册              
            }
            return null;
        }



        //根据callback返回
        internal RedirectResult CallBackRedirectByCookie()
        {
            //如果cookie里有callback则返回到callback
            var callback = HttpContext.Request.Cookies["callBack"];
            if (!string.IsNullOrWhiteSpace(callback))
            {
                return Redirect(callback);
            }
            return null;
        }




    }
}
