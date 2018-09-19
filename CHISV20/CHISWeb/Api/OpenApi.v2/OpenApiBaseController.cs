using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ass;

namespace CHIS.Api.v2
{
    [Produces("application/json")]
    [Route("openapi_v2/[controller]/[action]")]
    public class OpenApiBaseController : Controller
    {
        //public DbContext.CHISEntitiesSqlServer _db;
        public OpenApiBaseController(
            //DbContext.CHISEntitiesSqlServer db
            )
        {
            //_db = db;
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
                try
                {
                    return _logindata ?? (_logindata = new DtUtil.UserSelfUtil(HttpContext).GetUserSelf());
                }
                catch (Exception ex) { return null; }
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

        internal dynamic MyDynamicResult(bool result, string msg, dynamic data)
        {
            dynamic rtn = new System.Dynamic.ExpandoObject();
            rtn.rlt = result;
            rtn.code = result ? "200" : "-1";
            rtn.msg = msg;
            rtn.state = result ? "success" : "error";
            rtn.message = msg;
            rtn.data = data;
            return rtn;
        }
        internal dynamic MyDynamicResult(bool result, string msg)
        {
            return MyDynamicResult(result, msg, null);
        }
        internal dynamic MyDynamicResult(string msg, dynamic data)
        {
            return MyDynamicResult(true, msg, data);
        }
        internal dynamic MyDynamicResult(dynamic data)
        {
            return MyDynamicResult(true, "", data);
        }
        internal dynamic MyDynamicResult(Exception ex)
        {
            dynamic rtn = new System.Dynamic.ExpandoObject();
            var exm = new ComExceptionModel(ex);            
            rtn.rlt = false;
            rtn.msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            rtn.state = "error";
            rtn.message = ex.Message;
            rtn.code = exm.ErrorCode;
            return rtn;
        }

        /// <summary>
        /// ��ȡ·��
        /// </summary>
        /// <param name="basePath">�������·��</param>
        /// <param name="rootPath">���ܵĸ�·��</param>
        /// <returns>ȫ·��</returns>



        #endregion


        #region ApiPartialView
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
        #endregion

    }
}