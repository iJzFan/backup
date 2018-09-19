using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Ass;
using Ass.Mvc;
using ah;

namespace ah.Controllers
{
    public class BaseController : Controller
    {
        internal ah.DbContext.AHMSEntitiesSqlServer _db;
        public BaseController(ah.DbContext.AHMSEntitiesSqlServer db)
        {
            _db = db;
        }

        #region DbContext 数据库   
        protected override void Dispose(bool disposing)
        {
            if (_dbContextSqlServer != null)
            {
                _dbContextSqlServer.Dispose(); _dbContextSqlServer = null;
            }
            if (_dbContextMySql != null)
            {
                _dbContextMySql.Dispose(); _dbContextMySql = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 实际使用的数据上下文(主上下文)
        /// </summary>
        public ah.DbContext.AHMSEntitiesSqlServer MainDbContext
        {
            get
            {
                return DbContextSqlServer; //使用SqlServer
            }
        }





        ah.DbContext.AHMSEntitiesSqlServer _dbContextSqlServer = null;
        ah.DbContext.CHISEntitiesMySql _dbContextMySql = null;


        private ah.DbContext.AHMSEntitiesSqlServer DbContextSqlServer //MSSqlServer
        {
            get
            {
                string connStr = Global.Config.GetSection("ConnectionStrings:SqlConnection").Value;//根据appsettings.json的配置来获取连接字符串
                return _dbContextSqlServer ?? (_dbContextSqlServer = new DbContext.AHMSEntitiesSqlServer(connStr));
            }
        }


        #endregion



        #region 编辑返回函数
        /// <summary>
        /// 返回Action，一般来说，通过则返回null.如果遇到特别返回，则直接返回。
        /// </summary>
        /// <param name="bUseTrans">是否采用数据库事务</param>  
        ///<param name="bExceptionView">是否错误输出错误页面</param>
        public IActionResult TryCatchFunc(Func<IActionResult> func, bool bUseTrans = false, bool bExceptionView = false, string backLink = null, string backLinkName = null)
        {
            if (bUseTrans) //如果使用数据库事务
            {
                using (var tx = MainDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var rlt = func();
                        if (rlt != null) return rlt; //如果有返回，则直接返回
                        tx.Commit();
                        // return Json(new { rlt = true, state = "success", message = "操作成功", msg = "操作成功" });
                        return Json(MyDynamicResult(true, "操作成功"));
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        Exception exx = (ex.InnerException != null) ? ex.InnerException : ex;
                        if (bExceptionView)
                        {
                            ViewBag.BackLink = backLink;
                            ViewBag.BackLinkName = backLinkName;
                            return View("Error", exx);
                        }

                        //return Json(new { rlt = false, msg = exx.Message, state = "error", message = exx.Message });
                        return Json(MyDynamicResult(false, exx.Message));
                    }
                }
            }

            //采用一般返回
            try
            {
                var rlt = func();
                if (rlt != null) return rlt; //如果有返回，则直接返回
                                             //  return Json(new { rlt = true, state = "success", message = "操作成功", msg = "操作成功" });
                return Json(MyDynamicResult(true, "操作成功"));
            }
            catch (Exception ex)
            {
                Exception exx = (ex.InnerException != null) ? ex.InnerException : ex;
                if (bExceptionView)
                {
                    ViewBag.BackLink = backLink;
                    ViewBag.BackLinkName = backLinkName;
                    return View("Error", exx);
                }
                //return Json(new { rlt = false, msg = exx.Message, state = "error", message = exx.Message });
                return Json(MyDynamicResult(false, exx.Message));
            }
        }

        /// <summary>
        /// 返回Action，一般来说，通过则返回null.如果遇到特别返回，则直接返回。
        /// </summary>
        /// <param name="bUseTrans">是否采用数据库事务</param>  
        ///<param name="bExceptionView">是否错误输出错误页面</param>
        public IActionResult TryCatchFunc(Func<dynamic, IActionResult> func, bool bUseTrans = false, bool bExceptionView = false, string backLink = null, string backLinkName = null)
        {
            if (bUseTrans) //如果使用数据库事务
            {
                using (var tx = MainDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var dd = MyDynamicResult(true, "操作成功");
                        var rlt = func(dd);
                        if (rlt != null) return rlt; //如果有返回，则直接返回
                        tx.Commit();
                        // return Json(new { rlt = true, state = "success", message = "操作成功", msg = "操作成功" });
                        return Json(dd);
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        Exception exx = (ex.InnerException != null) ? ex.InnerException : ex;
                        if (bExceptionView)
                        {
                            ViewBag.BackLink = backLink;
                            ViewBag.BackLinkName = backLinkName;
                            return View("Error", exx);
                        }

                        //return Json(new { rlt = false, msg = exx.Message, state = "error", message = exx.Message });
                        return Json(MyDynamicResult(false, exx.Message));
                    }
                }
            }

            //采用一般返回
            try
            {
                var dd = MyDynamicResult(true, "操作成功");
                var rlt = func(dd);
                if (rlt != null) return rlt; //如果有返回，则直接返回
                                             //  return Json(new { rlt = true, state = "success", message = "操作成功", msg = "操作成功" });
                return Json(dd);
            }
            catch (Exception ex)
            {
                Exception exx = (ex.InnerException != null) ? ex.InnerException : ex;
                if (bExceptionView)
                {
                    ViewBag.BackLink = backLink;
                    ViewBag.BackLinkName = backLinkName;
                    return View("Error", exx);
                }
                //return Json(new { rlt = false, msg = exx.Message, state = "error", message = exx.Message });
                return Json(MyDynamicResult(false, exx.Message));
            }
        }














        /// <summary>
        /// 返回可以增加成员的匿名对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public dynamic MyDynamicResult(bool result, string msg)
        {
            dynamic rtn = new System.Dynamic.ExpandoObject();
            rtn.rlt = result;
            rtn.msg = msg;
            rtn.state = result ? "success" : "error";
            rtn.message = msg;
            return rtn;
        }


        public IActionResult CommonEdit(string op, Func<IActionResult> funcNewFind, Func<IActionResult> funcNew,
            Func<IActionResult> funcModifyFind, Func<IActionResult> funcModify, Func<IActionResult> funcDelete)
        {
            switch (ViewBag.OP = op.ToUpper())
            {
                case "NEWF": //新增页面 空白的数据页面                   
                    return TryCatchFunc(funcNewFind, bExceptionView: true);
                case "NEW": // 更新新增的数据                  
                    return TryCatchFunc(funcNew);
                case "MODIFYF": //修改 查找出修改的原始实体数据                    
                    return TryCatchFunc(funcModifyFind, bExceptionView: true);
                case "MODIFY": //修改后的数据               
                    return TryCatchFunc(funcModify);
                case "DELETE": //删除，返回json                     
                    return TryCatchFunc(funcDelete, bUseTrans: true);
                default:
                    return View("Error", new Exception("没有此页面命令！"));
            }
        }


        /// <summary>
        /// 获取所有的模型内的错误
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        internal string GetErrorOfModelState(ModelStateDictionary modelState)
        {
            List<string> errors = new List<string>();
            foreach (var item in modelState.Values)
                if (item.ValidationState == ModelValidationState.Invalid)
                {
                    foreach (var err in item.Errors)
                        errors.Add(err.ErrorMessage);
                }

            return string.Join("\r\n", errors);

        }



        #endregion

        #region 获取基本cookie信息

        /// <summary>
        /// 获取Cookie里面的基本用户信息
        /// </summary>
        public ah.Models.ViewModel.CUSTOMER_INFO GetCookieCustomerInfo()
        {
            string s = Request.Cookies["CUSTOMER_INFO"];
            //解密
            if (string.IsNullOrWhiteSpace(s)) return new Models.ViewModel.CUSTOMER_INFO();
            string c = Ass.Data.Secret.Decript(s, "tsjk@2018");
            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(c);
            return new Models.ViewModel.CUSTOMER_INFO
            {
                CustomerId = Ass.P.PIntV(jobj.GetValueString("CustomerId"), 0),
                CustomerMobile = jobj.GetValueString("CustomerMobile"),
                CustomerName = jobj.GetValueString("CustomerName"),
                Gender = Ass.P.PIntN(jobj.GetValueString("Gender")),
                Birthday = Ass.P.PDateTimeV(jobj.GetValueString("Birthday")),
                CustomerEmail = jobj.GetValueString("CustomerEmail"),
                MariageStatusId = Ass.P.PIntN(jobj.GetValueString("MariageStatusId")),
                MariageStatusName = jobj.GetValueString("MariageStatusName")
            };
        }

        protected Models.ViewModel.WechatBindingModel GetWXCookie(string jscookie = null, bool bThrowExp = false)
        {
            try
            {
                if (jscookie.IsEmpty()) jscookie = Request.Cookies["WXInfo"];
                Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jscookie);
                var errcode = Ass.P.PIntV(jobj.GetValueString("errcode"), 0);
                if (errcode > 0) throw new MyException(errcode, jobj.GetValueString("errmsg"));
                var gender = jobj.GetValueString("sex");

                return new Models.ViewModel.WechatBindingModel
                {
                    openid = jobj.GetValueString("openid"),
                    NickName = jobj.GetValueString("nickname"),
                    Gender = gender == "2" ? 0 : (gender == "1" ? 1 : 2),
                    WxPicUrl = jobj.GetValueString("headimgurl")
                };
            }
            catch (Exception ex) { if (bThrowExp) throw ex; else return null; }
        }


        #endregion


        #region 基本环境信息

        /// <summary>
        /// 获取客户端信息
        /// </summary>
        /// <returns></returns>
        public ClientEnvi GetClientEnvi()
        {
            var rtn = new ClientEnvi();
            var iswx = Request.Headers["User-Agent"].ToString().ToLower().Contains("micromessenger");
            if (iswx) rtn.ClientType = "wx";


            rtn.ClientIP = getClientIP();
            return rtn;
        }



        #endregion

        #region 公共类基础数据
        //public BLL.Sys.LogBll Loger = new BLL.Sys.LogBll();

        internal string UrlRoot
        {
            get
            {
                var url = Request.Scheme + "://" + Request.Host;
                if (url.ContainsIgnoreCase("localhost"))
                {
                    url = Global.Config.GetSection("RdSettings:UrlRoot").Value;
                }
                return url;
            }
        }

        #endregion

        #region 数据分页
        internal PageListInfo<T> PagedList<T>(IOrderedQueryable<T> orderedQueryable, int pageIndex, int pageSize) where T : class
        {
            if (pageIndex < 1) pageIndex = 1;
            var total = orderedQueryable.Count();
            var list = orderedQueryable.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return new PageListInfo<T>
            {
                DataList = list,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = total
            };
        }

        #endregion


        #region 重写

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cusinfo = GetCookieCustomerInfo();
            try { var wx = GetWXCookie(); if (wx != null && wx.openid.IsNotEmpty()) { cusinfo.LoginType = "wx"; } }
            catch { }

            ViewBag.CUSTOMER_INFO = cusinfo; //写入基本信息



            base.OnActionExecuting(context);
            //设置跟地址
            string s = context.HttpContext.Request.Host.Host;
            if (Global.ConfigSettings.WebHost != s) Global.ConfigSettings.WebHost = s;
        }
        #endregion


        #region 私有方法
        private string getClientIP()
        {
            return (HttpContext.Connection.RemoteIpAddress??System.Net.IPAddress.None).MapToIPv4().ToString();
        }

        #endregion

    }
}
