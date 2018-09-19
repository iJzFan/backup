using CHIS.Code.JwtAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Api.OpenApi.v2
{
    /// <summary>
    /// 系统操作
    /// </summary>
    [Route("api_sys/[controller]/[action]")]
    public class SystemController : CHIS.Api.v2.OpenApiBaseController
    {

        private OAuthService _jwtAuth;
        Services.LoginService _loginSvr;
        Services.DictService _dicSvr;
        Services.TreatService _treatSvr;
        public SystemController(OAuthService jwtAuth
            , Services.LoginService loginSvr
            , Services.TreatService treatSvr
                    , Services.DictService dicSvr)
        {
            _jwtAuth = jwtAuth;
            _loginSvr = loginSvr;
            _dicSvr = dicSvr;
            _treatSvr = treatSvr;
        }

        /// <summary>
        /// 获取开发Token
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetOAuthToken(string appId, string secretKey)
        {
            if (appId == ThirthPartTestUser.AppId && secretKey == ThirthPartTestUser.SecretKey)
            {
                var data = _jwtAuth.GenerateToken(appId, ThirthPartTestUser.Name);
                var rlt = MyDynamicResult(data);
                rlt.Headers = "angel-auth"; //必须用中间划线
                return Ok(rlt);
            }
            return Ok(MyDynamicResult(false, "获取失败"));
        }

        /// <summary>
        /// 根据手机号获取用户可登陆的药店工作站        
        /// </summary>
        /// <param name="mobileNumber">登陆者手机号</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetUserCanLoginDrugStoreStation([Required]string mobileNumber)
        {
            try
            {
                var data = _loginSvr.GetUserCanLoginStation(mobileNumber, true).ToList();
                var rlt = MyDynamicResult(data);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }


        /// <summary>
        /// 用户登录并获取令牌
        /// </summary>
        /// <param name="mobileNumber">用户手机</param>
        /// <param name="code">密码或手机验证码</param>
        /// <param name="stationId">登录工作站</param>
        /// <param name="loginType">登录类型 PWD/VCODE (可选)默认PWD</param>
        /// <param name="timeout">过期时长天数 (可选)默认7天</param>
        /// <returns></returns>
        [HttpPost]
        // [Authorize("ThirdPartAuth")]
        [AllowAnonymous]
        public IActionResult GetUserLoginToken(string mobileNumber, string code, int stationId, string loginType = "PWD", int? timeout = 7)
        {
            try
            {
                var loginRlt = _loginSvr.UserLoginStation(mobileNumber, code, stationId, loginType);
                var data = _jwtAuth.GenerateUserLoginToken(loginRlt, new TimeSpan(timeout.Value, 0, 0, 0));
                var rlt = MyDynamicResult(data);
                rlt.Headers = "angel-auth";
                rlt.LoginUser = loginRlt.ahDtUtil().NewModel<Models.UserSelfEx>(m => new
                {
                    LoginUserName = m.DoctorName,
                    m.Gender,
                    m.Birthday,
                    m.StationName,
                    m.CustomerId,
                    m.DoctorId,
                    m.DrugStoreStationId,
                    m.StationId,
                    DoctorPicUrl =m.DoctorPicUrl,
                    m.PostTitleName
                });
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 获取字典信息
        /// </summary>
        /// <param name="dictId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetDict(int dictId)
        {
            try
            {
                var data = _dicSvr.GetDictById(dictId);
                var rlt = MyDynamicResult(data);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 获取字典大类列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetDictMain(int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                var data = _dicSvr.GetDictMain(pageIndex.Value, pageSize.Value).Select(m => new
                {
                    DictId = m.DictID,
                    m.DictKey,
                    m.DictName,
                    m.IsEnable,
                    m.StopDate,
                    m.IsValueCode,
                    m.Remark
                });
                var rlt = MyDynamicResult(data);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 获取字典内容
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetDictByMainKey(string mainKey)
        {
            try
            {
                var data = _dicSvr.GetDictByMainKey(mainKey).Select(m => new
                {
                    m.DetailID,
                    m.DictKey,
                    m.ItemName,
                    m.IsDefault
                });
                var rlt = MyDynamicResult(data);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }


        /// <summary>
        /// 获取地址信息
        /// </summary>
        /// <param name="areaId">区域Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetAreaById(int areaId)
        {
            try
            {
                var data = _dicSvr.GetAreaById(areaId);
                var rlt = MyDynamicResult(data);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }
        /// <summary>
        /// 获取地址信息
        /// </summary>
        /// <param name="parentAreaId">父区域Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetSubAreaById(int parentAreaId)
        {
            try
            {
                var data = _dicSvr.GetSubArea(parentAreaId);
                var rlt = MyDynamicResult(data);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }


    }

    internal class ThirthPartTestUser
    {
        public static string AppId = "ah0001";

        public static string SecretKey = "123654";

        public static string Name = "天使健康对外API";
    }
}
