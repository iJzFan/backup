using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using ah.Models.DataModel;
using Ass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class HomeController : BaseController
    {


        //微信入口 
        [AllowAnonymous]
        public IActionResult WechatEntry(int? isdebug = 0, string returnUrl = null)
        {
            //微信数据获取code
            var appid = Global.Config.GetSection("Webfig:WX_appid").Value;
            var redirect_uri = System.Net.WebUtility.UrlEncode(Request.Scheme + "://" + Request.Host + "/Customer/Home/WechatEntry_GetOpenId");
            redirect_uri = HttpUtility.UrlEncode($"http://my.jk213.com/Customer/Home/WechatEntry_GetOpenId?returnUrl={returnUrl}");
            var response_type = "code";
            var scope = "snsapi_base";
            var state = isdebug == 1 ? "debug" : "ahjk213";
            var lk = "https://open.weixin.qq.com/connect/oauth2/authorize" + $"?appid={appid}&redirect_uri={redirect_uri}&response_type={response_type}&scope={scope}&state={state}#wechat_redirect";
            Response.Redirect(lk);//前端回调
            return Redirect(lk);// 此处回调会导致回调的是本域地址，而不能调用外域地址
        }
        [AllowAnonymous]
        public async Task<IActionResult> WechatEntry_GetOpenId(string code, string state, string returnUrl = null)
        {
            try
            {
                if (code.IsEmpty()) throw new Exception("没有获取到获取码！");

                var appid = Global.Config.GetSection("Webfig:WX_appid").Value;
                var secret = Global.Config.GetSection("Webfig:WX_secret").Value;
                var lk = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={appid}&secret={secret}&code={code}&grant_type=authorization_code ";
                /*
                 * 
            {   "access_token"  : "ACCESS_TOKEN",    
                "expires_in"    : 7200,    
                "refresh_token" : "REFRESH_TOKEN" ,    
                "openid"        : "OPENID",      
                "scope"         : "SCOPE" } 
                 */
                //服务器端执行接口
                string rlt = Ass.P.PStr(await Ass.Net.WebHelper.WebPost(lk)).Trim();

                if (!rlt.StartsWith("{")) throw new Exception("返回的不是json格式");
                Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(rlt);
                string openid = jobj.GetValueString("openid");
                if (openid.IsEmpty()) throw new Exception("获取openid失败:" + rlt);

                string accessToken = jobj.GetValueString("access_token");

                string refreshToken = jobj.GetValueString("refresh_token");

                //搜索openid
                var find = MainDbContext.CHIS_Code_Customer.FirstOrDefault(m => m.WXOpenId == openid);
                if (find == null) //绑定
                {
                    if (state == "debug")
                    {
                        var url = $"http://localhost:61448/Customer/Home/WechatBinding?openid={openid}&accessToken={accessToken}&state={state}&returnUrl={returnUrl}";
                        Response.Redirect(url);
                        return Redirect(url);
                    }
                    else return await WechatBinding(openid, accessToken, state);
                }
                else //检测是否更新，如果不用，则直接转入首页
                {
                    if (state == "debug")
                    {
                        string redirectUrl = $"http://localhost:61448/Customer/Home/LoadMobileFrontPage?openid={openid}&state={state}&accessToken={accessToken}&returnUrl={returnUrl}";
                        Response.Redirect(redirectUrl);
                        return Redirect(redirectUrl);
                    }
                    else return await LoadMobileFrontPage(openid, state, accessToken, returnUrl);
                }
            }
            catch (Exception ex)
            {
                ViewBag.IsShowBack = false;
                return View("Error", ex);
            }
        }

        //临时免登陆主界面
        [AllowAnonymous]
        public IActionResult MobileFrontPage1()
        {
            return View();
        }

        //手机首页
        [AllowAnonymous]
        public async Task<IActionResult> LoadMobileFrontPage(string openid, string state = "", string accessToken = "", string returnUrl = null)
        {
            if (openid.IsEmpty()) throw new Exception("没有传入openId");
            if (accessToken.IsNotEmpty())
            {
                try { await LoadUserInfoOfWechat(openid, accessToken); } catch { } //获取并写入微信cookie
            }

            //登录           
            var userEntity = MainDbContext.vwCHIS_Sys_Login.FirstOrDefault(m => m.WXOpenId == openid);
            if (userEntity == null) throw new Exception("没有发现登录用户信息");

            await SignInProcess(new CustomerClaimData()
            {
                LoginId = userEntity.LoginId,
                CustomerId = userEntity.CustomerId.Value,
                CustomerName = userEntity.CustomerName,
                CustomerMobile = userEntity.Mobile,
                CustomerEmail = userEntity.Email,
                Gender = userEntity.Gender,
                IDcard = userEntity.IdCardNumber
            });

            if (returnUrl != null)
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(MobileFrontPage), new { state = state });
        }
        public IActionResult MobileFrontPage(string state)
        {
            //向cookie写入测试状态
            if (state.IsEmpty()) state = Request.Cookies["WX_State"].ToString();//如果为空值，则赋值为cookie的值
            Response.Cookies.Append("WX_State", state, new Microsoft.AspNetCore.Http.CookieOptions { Path = "/" });// 写入cookie信息
            return View(nameof(MobileFrontPage));
        }

        //微信绑定
        [AllowAnonymous]
        public async Task<IActionResult> WechatBinding(string openid, string accessToken, string state = null)
        {
            try
            {
                //获取用户信息
                var wxuser = await LoadUserInfoOfWechat(openid, accessToken, true);
                return View(nameof(WechatBinding), wxuser);
            }
            catch (Exception ex)
            {
                if (ex is MyException)
                {
                    if (((MyException)ex).ErrorCode == 48001)
                    {
                        //微信数据获取code
                        var appid = Global.Config.GetSection("Webfig:WX_appid").Value;
                        var redirect_uri = System.Net.WebUtility.UrlEncode(Request.Scheme + "://" + Request.Host + "/Customer/Home/WechatEntry_GetOpenId");
                        redirect_uri = "http://my.jk213.com/Customer/Home/WechatEntry_GetOpenId";
                        var response_type = "code";
                        var scope = "snsapi_userinfo";
                        var lk = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appid}&redirect_uri={redirect_uri}&response_type={response_type}&scope={scope}&state={state}#wechat_redirect";
                        Response.Redirect(lk);//前端回调
                    }
                }
                return View("Error", ex);
            }
        }



        public async Task<Models.ViewModel.WechatBindingModel> LoadUserInfoOfWechat(string openid, string accessToken = null, bool bThrowExp = false)
        {
            if (openid.IsEmpty()) throw new Exception("没有传入openid");

            //更新wx信息
            var cus = MainDbContext.CHIS_Code_Customer.FirstOrDefault(m => m.WXOpenId == openid);
            if (cus != null)
            {
                if (accessToken.IsNotEmpty() && accessToken != cus.WXAccessToken) { cus.WXAccessToken = accessToken; MainDbContext.SaveChanges(); }
                accessToken = cus.WXAccessToken;//保持accessToken与数据库一致
            }
            else { if (accessToken.IsEmpty()) throw new Exception("需要传入accessToken"); }
            /*
            //检验获取信息的accessToken
            var url = $"https://api.weixin.qq.com/sns/auth?access_token={accessToken}&openid={openid}";
            string rlt = await Ass.Net.WebHelper.WebPost(url);
            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(rlt);
            string errcode = jobj.GetValueString("errcode");
            if (errcode != "0") { //重新更新

            }
            */



            //获取人员信息
            /*
             {    "openid":" OPENID",  
        "nickname"  : NICKNAME,   
        "sex"       : "1",   
        "province"  : "PROVINCE"   
        "city"      : "CITY",   
        "country"   : "COUNTRY",    
        "headimgurl": "http://wx.qlogo.cn/mmopen/g3MonUZtNHkdmzicIlibx6iaFqAc56vxLSUfpb6n5WKSYVY0ChQKkiaJSgQ1dZuTOgvLLrhJbERQQ4eMsv84eavHiaiceqxibJxCfHe/46",  
        "privilege" : [ "PRIVILEGE1" "PRIVILEGE2"     ],    
        "unionid"   : "o6_bmasdasdsad6_2sgVt7hMZOPfL" 
} 
             */

            var lk = $"https://api.weixin.qq.com/sns/userinfo?access_token={accessToken}&openid={openid}&lang=zh_CN";
            Global.WriteLog(lk);
            string rlt0 = await Ass.Net.WebHelper.WebPost(lk);
            Response.Cookies.Append("WXInfo", rlt0);// 写入cookie信息
            return GetWXCookie(rlt0, bThrowExp);
        }





        //微信注册页面
        [AllowAnonymous]
        public IActionResult WechatRegister(string mobile)
        {
            var model = GetWXCookie(Request.Cookies["WXInfo"], true);
            Global.WriteLog(model, isToJson: true);
            model.mobile = mobile;
            return View(nameof(WechatRegister), model);
        }
        //注册提交页面 提交后转入首页
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> WechatRegister(ah.Models.ViewModel.WechatBindingModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //强制注册，并清空旧持有者的信息数据。
                    new ahWeb.Api.Customer(_db).WXRegistCustomer(model);
                    return await LoadMobileFrontPage(model.openid);
                }
                else return View("WechatRegister", model);
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); Global.WriteLog(ex); return View("WechatRegister", model); }
        }


        //选择用户界面
        [AllowAnonymous]
        public IActionResult WechatChooseCustomer(string mobile)
        {
            if (mobile.IsEmpty())
            {
                var wxuser = GetWXCookie();
                wxuser.mobile = mobile;
                return View(nameof(WechatBinding), wxuser);
            }

            var model = GetWXCookie(Request.Cookies["WXInfo"]);
            model.mobile = mobile;
            var finds = MainDbContext.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.CustomerMobile == model.mobile);
            ViewBag.Customers = finds;
            return View(nameof(WechatChooseCustomer), model);
        }


        //选择用户并绑定 绑定后转入首页
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> WechatChooseCustomer(ah.Models.ViewModel.WechatBindingModel model)
        {
            try
            {
                new ahWeb.Api.Customer(_db).WXBindingCustomer(model);
                return await LoadMobileFrontPage(model.openid);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var finds = MainDbContext.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.CustomerMobile == model.mobile);
                ViewBag.Customers = finds;
                return View("WechatChooseCustomer", model);
            }
        }


        //根据手机号码查找用户
        [AllowAnonymous]
        public IActionResult WechatFindCustomer(string mobile, string vcode)
        {
            try
            {
                //验证码验证
                new ahWeb.Api.Customer(_db).CheckRegVCode(mobile, vcode);
                //如果没有用户 跳转到注册页面
                var finds = MainDbContext.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.CustomerMobile == mobile);
                if (finds.Count() == 0)
                {
                    return WechatRegister(mobile);
                }
                else
                {
                    ViewBag.Customers = finds;
                    return WechatChooseCustomer(mobile);
                }
            }
            catch (Exception ex)
            {
                var wxuser = GetWXCookie();
                wxuser.mobile = mobile;
                ModelState.AddModelError("", ex.Message);
                return View(nameof(WechatBinding), wxuser);
            }

        }


    }
}
