using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ass;

namespace CHIS.Api
{
    [Produces("application/json")]
    [Route("openapi/[controller]/[action]")]
    [AllowAnonymous]
    public class OpenApiBaseController : Controller
    {
        public DbContext.CHISEntitiesSqlServer _db;
        public OpenApiBaseController(DbContext.CHISEntitiesSqlServer db)
        {
            _db = db;
        }



        #region ��¼��Ϣ
        Models.UserSelf _logindata = null;
        /// <summary>
        /// ��¼�û�����Ϣ
        /// </summary>
        public Models.UserSelf UserSelf
        {
            get
            {
                return _logindata ?? (_logindata = new DtUtil.UserSelfUtil(HttpContext).GetUserSelf());
            }
        }         
        #endregion


        #region ���ܺ���


        /// <summary>
        /// ���Ի�ȡ�����򷵻� Ĭ��
        /// </summary>
        internal T TryGet<T>(Func<T> func, T defVal = default(T))
        {
            try
            {
                return func();
            }
            catch { return defVal; }
        }

        internal dynamic MyDynamicResult(bool result, string msg)
        {
            dynamic rtn = new System.Dynamic.ExpandoObject();
            rtn.rlt = result;
            rtn.code = result ? "200" : "-1";
            rtn.msg = msg;
            rtn.state = result ? "success" : "error";
            rtn.message = msg;
            return rtn;
        }
        internal dynamic MyDynamicResult(Exception ex)
        {
            dynamic rtn = new System.Dynamic.ExpandoObject();
            rtn.rlt = false;
            rtn.msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            rtn.state = "error";
            rtn.message = ex.Message;
            return rtn;
        }

        /// <summary>
        /// ��ȡ·��
        /// </summary>
        /// <param name="basePath">�������·��</param>
        /// <param name="rootPath">���ܵĸ�·��</param>
        /// <returns>ȫ·��</returns>



        #endregion



        /// <summary>
        /// Apiר�õ�PartialView
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        internal PartialViewResult ApiPartialView(string viewName, object model)
        {
            var cn = RouteData.Values["controller"].ToString();
            var vn = $"~/ApiViews/{cn}/{viewName}.cshtml";
            return PartialView(vn, model);
        }
        internal PartialViewResult ApiPartialView(Exception ex)
        {
            var vn = $"~/ApiViews/_pvException.cshtml";
            return PartialView(vn, ex);
        }

    }
}