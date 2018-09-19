using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ass;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Authentication;

namespace CHIS.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    public class BaseController : Controller
    {
        internal DbContext.CHISEntitiesSqlServer _db;
        public BaseController(DbContext.CHISEntitiesSqlServer db)
        {
            _db = db;
        }

        #region 属性
        /// <summary>
        /// 获取客户端IP
        /// </summary>
        public string GetClientIP
        {
            get
            {
                var ip0 = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault().ToString();
                if (System.Text.RegularExpressions.Regex.IsMatch(ip0, @"^[0|1|2]\d+\.\d+\.\d+\.\d+$")) return ip0;
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
        internal async Task<string> GetRequestBody(System.IO.Stream body)
        {
            return await new System.IO.StreamReader(body).ReadToEndAsync();
        }
        #endregion


        #region DbContext 数据库   

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string SqlDbConnectionString
        {
            get
            {
                return new Code.Utility.DataBaseHelper().ConnectionString;
            }
        }

        #endregion

        #region 日志 与 测试
        Code.Managers.IMyLogger _logger = null;
        public Code.Managers.IMyLogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = new Code.Managers.MyLogger();
                    try { _logger.UserInfos = UserSelf; } catch { }
                }
                return _logger;
            }
        }

        #region 设置显示的Debug字符
        StringBuilder debugbd = new StringBuilder();
        public void _setDebugText(DateTime? start = null, string text = null)
        {
#if DEBUG
            if (start.HasValue) debugbd.AppendFormat("耗时{0}毫秒;", (DateTime.Now - start.Value).TotalMilliseconds);
            ViewBag.DebugText = debugbd.ToString();
#endif
        }
        public void _setDebugText(string text)
        {
#if DEBUG
            if (text.IsNotEmpty()) debugbd.Append(text);
            ViewBag.DebugText = debugbd.ToString();
#endif
        }
        public void _setDebugText(string key, string text)
        {
#if DEBUG
            if (text.IsNotEmpty()) debugbd.AppendFormat("{0}={1}", key, text);
            ViewBag.DebugText = debugbd.ToString();
#endif
        }
        #endregion

        #endregion


        #region 人员与权限信息
        Code.Managers.UserFrameManager _userMgr; Models.UserSelf _logindata = null;
        /// <summary>
        /// 用户框架管理器
        /// </summary>
        public Code.Managers.UserFrameManager UserMgr
        {

            get
            {
                return _userMgr ?? (_userMgr = new Code.Managers.UserFrameManager(this.UserSelf));
            }
        } 
        public Models.UserSelf UserSelf
        {
            get
            {
                return _logindata ?? (_logindata = new DtUtil.UserSelfUtil(HttpContext).GetUserSelf());
            }
        }         
        /// <summary>
        /// 获取用户登录数据 优先返回登录用户数据
        /// </summary>
        /// <param name="bSysAuto">是否返回系统用户 true 则返回id=0 name=system</param>        
        public Models.UserSelf GetUserSelf(bool bSysAuto = false)
        {
            if (!bSysAuto) return UserSelf;
            try { return UserSelf; }
            catch
            {
                return new Models.UserSelf
                {
                    LoginId = 0,
                    OpId = 0,
                    DoctorId = 0,
                    OpMan = "System",
                    StationId = 0,
                    LoginTime = DateTime.Now
                };
            }
        }

        #endregion

        #region 登录工具

        public async Task UserSignInAsync(System.Security.Claims.ClaimsPrincipal userPrincipal, int expiresMiniutes = 120)
        {
            await HttpContext.SignInAsync(Global.AUTHENTICATION_SCHEME, userPrincipal,
                             new AuthenticationProperties
                             {
                                 ExpiresUtc = DateTime.UtcNow.AddMinutes(expiresMiniutes),
                                 IsPersistent = true,
                                 AllowRefresh = true
                             });

        }

        #endregion



        #region jqgrid查询工具
        /// <summary>
        /// 适用于jqgrid的搜索获取分页数据
        /// </summary>
        public IQueryable<T> FindPagedData_jqgrid<T>(IOrderedQueryable<T> findList, out int pageIndex, out int pageSize, out int totalPage, out int findTotal) where T : class
        {
            string sort = "";
            getJqGridInfo(out pageIndex, out pageSize, out sort, 1, UserSelf.TableRecordsPerPage);
            findTotal = findList.Count();
            totalPage = (int)Math.Ceiling(findTotal * 1.0f / pageSize);
            var dataList = findList.
                          Skip(pageSize * (pageIndex - 1)).
                          Take(pageSize).AsQueryable();
            return dataList;
        }
        public JsonResult FindPagedData_jqgrid<T>(IOrderedQueryable<T> findList, Func<T, dynamic> selector = null) where T : class
        {
            //排序获取当前页的数据   
            int pageIndex, pageSize, totalPage, findTotal;
            var dataList = FindPagedData_jqgrid(findList, out pageIndex, out pageSize, out totalPage, out findTotal);

            object v0 = dataList.ToList();
            if (selector != null) v0 = dataList.ToList().AsQueryable().Select(selector);



            return Json(new
            {
                page = pageIndex,
                total = totalPage,
                records = findTotal,
                rows = v0
            });
        }
        #endregion



        #region 编辑返回函数
        /// <summary>
        /// 返回Action，一般来说，通过则返回null.如果遇到特别返回，则直接返回。
        /// </summary>
        /// <param name="bUseTrans">是否采用数据库事务</param>  
        ///<param name="bExceptionView">是否错误输出错误页面</param>
        ///<param name="success">成功后的操作</param>
        public IActionResult TryCatchFunc(Func<IActionResult> func, bool bUseTrans = false, bool bExceptionView = false, string backLink = null, string backLinkName = null, Action success = null)
        {
            bool bSuccess = false;
            IActionResult rlt = null;
            if (bUseTrans) //如果使用数据库事务
            {

                using (var tx = _db.Database.BeginTransaction())
                {
                    try
                    {
                        rlt = func();
                        tx.Commit();
                        // return Json(new { rlt = true, state = "success", message = "操作成功", msg = "操作成功" });
                        bSuccess = true;
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
                if (bSuccess)
                {
                    success?.Invoke();//成功后调用
                    if (rlt != null) return rlt; //如果有返回，则直接返回
                    return Json(MyDynamicResult(true, "操作成功"));
                }
            }

            //采用一般返回
            try
            {
                rlt = func();
                success?.Invoke();//成功后调用
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
        public IActionResult TryCatchFunc(Func<dynamic, IActionResult> func, bool bUseTrans = false, bool bExceptionView = false, string backLink = null, string backLinkName = null, Action success = null)
        {
            bool bSuccess = false;
            IActionResult rlt = null;
            var dd = MyDynamicResult(true, "操作成功");
            if (bUseTrans) //如果使用数据库事务
            {
                using (var tx = _db.Database.BeginTransaction())
                {
                    try
                    {
                        rlt = func(dd);
                        tx.Commit();
                        bSuccess = true;
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
                if (bSuccess)
                {
                    success?.Invoke();//成功后调用
                    if (rlt != null) return rlt; //如果有返回，则直接返回
                    return Json(dd);
                }
            }

            //采用一般返回
            try
            {
                rlt = func(dd);
                success?.Invoke();//成功后调用
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


        public IActionResult TryCatchFuncW(Func<Task<IActionResult>> funcAsync, bool bExceptionView = false, string backLink = null, string backLinkName = null, Action success = null)
        {
            IActionResult rlt = null;
            //采用一般返回
            try
            {
                rlt = funcAsync().Result;
                success?.Invoke();//成功后调用
                if (rlt != null) return rlt; //如果有返回，则直接返回                                            
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


        public async Task<IActionResult> TryCatchFuncAsync(Func<Task<IActionResult>> funcAsync, bool bExceptionView = false, string backLink = null, string backLinkName = null, Action success = null)
        {
            IActionResult rlt = null;
            //采用一般返回
            try
            {
                rlt = await funcAsync();
                success?.Invoke();//成功后调用
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

        public async Task<IActionResult> TryCatchFuncAsync(Func<dynamic, Task<IActionResult>> funcAsync, bool bExceptionView = false, string backLink = null, string backLinkName = null, Action success = null)
        {
            IActionResult rlt = null;
            var dd = MyDynamicResult(true, "操作成功");

            //采用一般返回
            try
            {
                rlt = await funcAsync(dd);
                success?.Invoke();//成功后调用
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
        internal dynamic MyDynamicResult(Exception ex)
        {
            dynamic rtn = new System.Dynamic.ExpandoObject();
            rtn.rlt = false;
            rtn.msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            rtn.state = "error";
            rtn.message = ex.Message;
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



        #region 小函数

        /// <summary>
        /// 尝试获取，否则返回 默认
        /// </summary>
        public T TryGet<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch { return default(T); }
        }





        /// <summary>
        /// 判断是否不是Id（空数据或者零）
        /// </summary>    
        internal bool isNotId(object obj)
        {
            string rlt = string.Format("{0}", obj);
            return (string.IsNullOrWhiteSpace(rlt)) || rlt == "0";
        }

        internal void getJqGridInfo(out int pageIndex, out int pageSize, out string sort, int defPageIndex = 1, int defPageSize = 20)
        {
            pageIndex = Ass.P.PIntV(getRequestParms("page"), defPageIndex); //页序
            pageSize = Ass.P.PIntV(getRequestParms("rows"), defPageSize); //页规格
            string sx = getRequestParms("sidx");
            sort = string.IsNullOrWhiteSpace(sx) ? "" : sx + " " + getRequestParms("sord");//排序

        }

        /// <summary>
        /// 获取jqGrid 传入后台sort的信息
        /// </summary>
        /// <returns></returns>
        internal string getJqGridSort()
        {
            string sx = getRequestParms("sidx");
            if (string.IsNullOrWhiteSpace(sx)) return "";
            else return sx + " " + getRequestParms("sord");//排序
        }


        /// <summary>
        /// 获取Query的内容，处理了空格
        /// </summary>
        internal string getQuery(string key)
        {
            try
            {
                return $"{Request.Query[key]}".Trim();
            }
            catch { return ""; }

        }
        /// <summary>
        /// 获取Form内容，处理了空格
        /// </summary>
        internal string getForm(string key)
        {
            try
            {
                return $"{Request.Form[key]}".Trim();
            }
            catch { return ""; }
        }

        /// <summary>
        /// 获取Query的内容，为空时获取Form的内容
        /// </summary>
        internal string getRequestParms(string key)
        {
            var v = getQuery(key);
            return v == "" ? getForm(key) : v;
        }



        /// <summary>
        /// 初始化数据，时间范围
        /// </summary>
        /// <param name="tstart">起始时间</param>
        /// <param name="tend">结束时间</param>
        /// <param name="defBeforeNowDays">默认起始提前于当前的天数</param>
        /// <param name="bEndDayInclude">是否包含结束日期</param>
        internal void initialData_TimeRange(ref DateTime? tstart, ref DateTime? tend, int defBeforeNowDays = 7, bool bEndDayInclude = true, string timeRange = null)
        {
            var dt0 = tstart == null ? new DateTime() : tstart.Value;
            var dt1 = tend == null ? new DateTime() : tend.Value;
            initialData_TimeRange(ref dt0, ref dt1, defBeforeNowDays, bEndDayInclude, timeRange);
            tstart = dt0; tend = dt1;
        }
        internal void initialData_TimeRange(ref DateTime tstart, ref DateTime tend, int defBeforeNowDays = 7, bool bEndDayInclude = true, string timeRange = null)
        {
            if (tstart == new DateTime())
            {
                tstart = DateTime.Now.AddDays(0 - defBeforeNowDays).Date;
            }
            if (tend == new DateTime())
            {
                tend = DateTime.Now;
            }
            if (timeRange != null)
            {
                var rlt = timeRange.ahDtUtil().TimeRange();
                tstart = rlt.Item1;
                tend = rlt.Item2;
            }
        }
        internal void initialData_TimeRange(ref DateTime tstart, ref DateTime tend, string timeRange)
        {
            initialData_TimeRange(ref tstart, ref tend, 7, true, timeRange);
        }
        internal void initialData_TimeRange(ref DateTime? tstart, ref DateTime? tend, string timeRange)
        {
            initialData_TimeRange(ref tstart, ref tend, 7, true, timeRange: timeRange);
        }



        /// <summary>
        /// 初始化数据，页面数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        internal void initialData_Page(ref int pageIndex, ref int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 5) pageIndex = 20;
        }

        #endregion

        #region 公共类基础数据
        //public BLL.Sys.LogBll Loger = new BLL.Sys.LogBll();

        public static string _urlRoot = null;
        internal string UrlRoot
        {
            get
            {
                if (_urlRoot.IsEmpty())
                {
                    var url = Request.Scheme + "://" + Request.Host;
                    if (url.ContainsIgnoreCase("localhost"))
                    {
                        url = Global.Config.GetSection("RdSettings:UrlRoot").Value;
                        if (url.ContainsIgnoreCase("localhost"))
                        {
                            var addrs = Dns.GetHostAddresses(Dns.GetHostName());
                            var iplocal = addrs.FirstOrDefault(m => !m.IsIPv6LinkLocal).ToString();
                            url = url.Replace("localhost", iplocal);
                        }
                    }
                    _urlRoot = url;
                }
                return _urlRoot;
            }
        }

        #endregion



        /// <summary>
        /// 获取View的路径
        /// </summary>
        /// <param name="viewName">文件名 默认Index</param>
        /// <param name="viewDictName">所在文件夹名</param>
        /// <returns></returns>
        public string GetViewPath(string viewDictName,string viewName="Index")
        {
            var cn = RouteData.Values["controller"].ToString();
            var vn = $"~/Views/{cn}/{viewDictName}/{viewName}.cshtml";
            return vn;
        }




        public override PartialViewResult PartialView(string viewName, object model)
        {
            try
            {
                ViewBag.UserSelf = UserSelf;
            }
            catch (Exception ex)
            {
                var e = ex;
                ViewBag.UserSelf = null;
            }
            return base.PartialView(viewName, model);
        }

        public override ViewResult View(string viewName, object model)
        {
            /*
             * 重写视图传递数据，传递User的基本数据
             */
            try
            {
                ViewBag.UserSelf = UserSelf;
            }
            catch (Exception ex)
            {
                var e = ex;
                ViewBag.UserSelf = null;
            }

            //if (Ass.Net.ClientInfo.Get(Request).IsLessIE9)
            //    return base.View("~/Views/shared/EnviRequire.cshtml", null);

            // if (string.IsNullOrEmpty(viewName)) return base.View(); 
            return base.View(viewName, model);
        }

        /// <summary>
        /// 错误分布视图
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public PartialViewResult ExceptionPartialView(Exception ex)
        {
            if (ex.InnerException != null) ex = ex.InnerException;
            return PartialView("partialError", ex);
        }




        /// <summary>
        /// 用户通用查询
        /// 99 CustomerId,2 身份证
        /// </summary>
        /// <param name="searchtype"></param>
        /// <param name="searchtxt"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public IQueryable<Models.vwCHIS_Code_Customer> GetPatientsBy(int searchtype, string searchtxt, string s = null)
        {
            return new CustomerCBL(this).GetCustomersBy(searchtype, searchtxt, P.PIntN(s));
        }


        /// <summary>
        /// 药品信息查询 避免大数据库的全文搜索
        /// </summary>
        public IList<int> SearchDrugIds(string searchText)
        {
            int drugId = 0;
            if (int.TryParse(searchText, out drugId))
                return new List<int>() { drugId };
            else
                return _db.CHIS_Code_Drug_Main.AsNoTracking().Where(m => m.CodeDock.Contains(searchText)).Select(m => m.DrugId).ToList();
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            //设置根地址
            string s = context.HttpContext.Request.Host.Host;
            if (Global.ConfigSettings.WebHost != s) Global.ConfigSettings.WebHost = s;
        }



        #region WebApi的调用
        /// <summary>
        /// Api调用
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> ApiPostAsync(string url, Dictionary<string, string> data)
        {
            string result = string.Empty;
            //设置HttpClientHandler的AutomaticDecompression  
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            //创建HttpClient（注意传入HttpClientHandler）  
            using (var http = new HttpClient(handler))
            {
                //使用FormUrlEncodedContent做HttpContent  
                /*
                var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {  
//传递单个值  
                  {"", data}//键名必须为空  
//传递对象  
//{"name","hello"},  
//{"age","16"}  
                 });
                */
                var content = new FormUrlEncodedContent(data);
                //await异步等待回应  
                var response = await http.PostAsync(url, content);
                //确保HTTP成功状态值  
                response.EnsureSuccessStatusCode();
                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）  
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

        /// <summary>
        /// Get方式获取Api返回信息
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns>返回json数据</returns>
        public async Task<string> ApiGetAsync(string url)
        {
            //创建HttpClient（注意传入HttpClientHandler）  
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            using (var http = new HttpClient(handler))
            {
                //await异步等待回应  
                var response = await http.GetAsync(url);
                //确保HTTP成功状态值  
                response.EnsureSuccessStatusCode();

                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）  
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>  
        /// HttpClient实现Put请求(异步)  
        /// </summary>  
        public async Task<string> ApiPutAsync(string url, Dictionary<string, string> data)
        {
            //设置HttpClientHandler的AutomaticDecompression  
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            //创建HttpClient（注意传入HttpClientHandler）  
            using (var http = new HttpClient(handler))
            {
                //使用FormUrlEncodedContent做HttpContent                  
                var content = new FormUrlEncodedContent(data);

                //await异步等待回应  
                var response = await http.PutAsync(url, content);
                //确保HTTP成功状态值  
                response.EnsureSuccessStatusCode();
                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）  
                return await response.Content.ReadAsStringAsync();
            }
        }



        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url">url包含参数</param>
        public async Task<T> HttpPostWithDecompression<T>(string request, string apiUrl, int timeout) where T : new()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Proxy = null;
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            var data = Encoding.UTF8.GetBytes(request);
            using (var httpClient = new HttpClient(handler))
            {
                httpClient.BaseAddress = new Uri(apiUrl);
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var cont = new StringContent(request);
                //var content = new FormUrlEncodedContent(JsonConvert.DeserializeObject<Dictionary<string, string>>(request));
                //被上面这个注释掉的代码，这种偷懒转Dic的方式给坑苦了，不能这么用啊！！！
                var response = await httpClient.PostAsync(apiUrl, cont);
                string result = response.Content.ReadAsStringAsync().Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
        }
        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url">url包含参数</param>
        public async Task<T> doGet<T>(string url) where T : new()
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip
            };
            using (var http = new HttpClient(handler))
            {
                var response = await http.GetAsync(url);
                //确保HTTP成功状态值
                response.EnsureSuccessStatusCode();
                string responseStr = response.Content.ReadAsStringAsync().Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseStr);
            }
        }
        #endregion


    }



}
