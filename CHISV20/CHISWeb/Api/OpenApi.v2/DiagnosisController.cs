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
    public class DiagnosisController : CHIS.Api.v2.OpenApiBaseController
    {

        private OAuthService _jwtAuth;
        Services.LoginService _loginSvr;
        Services.DictService _dicSvr;
        Services.TreatService _treatSvr;
        public DiagnosisController(OAuthService jwtAuth
            , Services.LoginService loginSvr
            ,Services.TreatService treatSvr
                    , Services.DictService dicSvr)
        {
            _jwtAuth = jwtAuth;
            _loginSvr = loginSvr;
            _dicSvr = dicSvr;
            _treatSvr = treatSvr;
        }
         

        /// <summary>
        /// 获取诊断信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetDiagnosis(string searchText,int? pageIndex=1,int? pageSize=20)
        {
            try
            {
                var data = _treatSvr.GetDiagnosis(searchText, pageIndex.Value, pageSize.Value);
                var rlt = MyDynamicResult(data);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 获取我的历史诊断
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetMyHistoryDiagnosis(string searchText, int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                if (UserSelf == null)
                    throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录用户不支持");
                var data = _treatSvr.GetMyHistoryDiagnosis(searchText,UserSelf.DoctorId,UserSelf.StationId, pageIndex.Value, pageSize.Value).ToList();
                var mdd = data.Select(m => new{
                    m.SelDiagnosisId,                  
                    m.DiagnosisId,
                    DiagnosisName=_treatSvr.GetDiagnosisById(m.DiagnosisId).DiagnoisisName,
                    m.SelectNum,
                    m.LatestTime
                });
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }

    }
 
}
