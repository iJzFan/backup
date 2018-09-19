using System;
using System.Reflection;
using System.Threading.Tasks;
using ah.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Ass;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Linq;

namespace ah.Services
{
    public class weixinService : BaseService
    {
        string _appid, _secret;
        HttpContext _httpContext;

        public weixinService(IHttpContextAccessor httpContext)
        {
            _appid = Global.Config.GetSection("Webfig:WX_appid").Value;
            _secret = Global.Config.GetSection("Webfig:WX_secret").Value;
            _httpContext = httpContext.HttpContext;
        }



        public async void ReGetOpenIdToSession()
        {

            var url = _httpContext.Request.GetAbsoluteUri();
            if (getSession("openid") == "")
            {
                //先要判断是否是获取code后跳转过来的
                if (getQuery("code").IsEmpty())
                {
                    //Code为空时，先获取Code
                    string GetCodeUrls = GetCodeUrl(url);
                    _httpContext.Response.Redirect(GetCodeUrls);//先跳转到微信的服务器，取得code后会跳回来这页面的
                }
                else
                {
                    //Code非空，已经获取了code后跳回来啦，现在重新获取openid                  
                    string openid = "";
                    openid = await GetOauthAccessOpenId(getQuery("code"));//重新取得用户的openid
                    _httpContext.Session.SetString("openid", openid);
                }
            }
        }
        private string getQuery(string key)
        {
            var s = _httpContext.Request.Query[key];
            return s.ToString();
        }
        private string getSession(string key)
        {
            try
            {
                if (_httpContext.Session.Keys.Contains(key)) return _httpContext.Session.GetString("openid");
                else return "";
            }
            catch { return ""; }
        }


        #region 重新获取Code的跳转链接(没有用户授权的，只能获取基本信息）
        /// <summary>重新获取Code,以后面实现带着Code重新跳回目标页面(没有用户授权的，只能获取基本信息（openid））</summary>
        /// <param name="url">目标页面</param>
        /// <returns></returns>
        private string GetCodeUrl(string url)
        {
            string CodeUrl = "";
            //对url进行编码
            url = System.Web.HttpUtility.UrlEncode(url);
            CodeUrl = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + _appid + "&redirect_uri=" + url + "?action=viewtest&response_type=code&scope=snsapi_base&state=1#wechat_redirect");
            return CodeUrl;

        }
        #endregion

        #region 以Code换取用户的openid、access_token
        /// <summary>根据Code获取用户的openid、access_token</summary>
        private async Task<string> GetOauthAccessOpenId(string code)
        {
            string Openid = "";
            string url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + _appid + "&secret=" + _secret + "&code=" + code + "&grant_type=authorization_code";
            var client = new HttpClient();
            var msg = await client.GetAsync(url);
            if (msg.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("网络错误请重试！");
            }
            var js = await msg.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<OAuth_Token>(js);
            Openid = model.openid;
            return Openid;
        }
        #endregion





    }
    internal class OAuth_Token
    {
        /// <summary>
        /// 网页授权接口调用凭证,注意：此access_token与基础支持的access_token不同
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// access_token接口调用凭证超时时间，单位（秒）
        /// </summary>
        public string expires_in { get; set; }
        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        public string refresh_token { get; set; }
        /// <summary>
        /// 用户唯一标识,请注意，在未关注公众号时，用户访问公众号的网页，也会产生一个用户和公众号唯一的OpenID
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 用户授权作用域
        /// </summary>
        public string scope { get; set; }
    }
}