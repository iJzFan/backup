using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;

namespace CHIS.Controllers
{
    public partial class HomeController : BaseController
    {
        Services.LoginService _loginSvr;

        Services.WeChatService _weChatSvr;

        Services.DoctorService _docSvr;

        public HomeController(DbContext.CHISEntitiesSqlServer db
            , Services.LoginService loginSvr,
            Services.WeChatService weChatSvr,
            Services.DoctorService docSvr
            ) : base(db) { _loginSvr = loginSvr; _weChatSvr = weChatSvr; _docSvr = docSvr; }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Home", new { area = "" });
            //return View();
        }
        [AllowAnonymous]
        public IActionResult About()
        {
            //var model =  DbContextSqlServer.CHIS_Code_Customer;    
            ////var model = "";  
            //return View(model);
            return View();
        }
        [AllowAnonymous]
        public IActionResult Contact()
        {
            //var model = DbContextMySql.CHIS_Code_Customer.Where(m=>!string.IsNullOrWhiteSpace(m.IDcard));
            ////var model = "";
            //return View(model);
            return View();
        }



        #region 各种首页

        //工作站站医生
        public async Task<IActionResult> DoctorIndex(string viewName = "DoctorIndex", object model = null)
        {
            //此处要做一个登录后的信息校验         
            var cbl = new DoctorCBL(this);
            var b0 = await cbl.IsNeedCompleteDoctorInfo(this.UserSelf.DoctorId);
            if (b0)
            {
                return RedirectToAction("DoctorInfos", "MyPanel");
            }
            else if (cbl.IsCheckingOccupationInfo(this.UserSelf.DoctorId))
            {
                return RedirectToAction("CheckingOccupationInfo", "MyPanel");
            }
            else
            {
                await Logger.WriteInfoAsync("Home", "Login", $"用户({UserSelf.DoctorId},{UserSelf.DoctorName})登入系统,工作站({UserSelf.StationId},{UserSelf.StationName})。");
                return View(viewName, model);
            }
        }
        //网络平台
        public Task<IActionResult> DoctorIndex_NetPlat()
        {
            return DoctorIndex("DoctorIndex_NetPlat");
        }

        //通用首页
        public Task<IActionResult> DoctorIndex_Normal()
        {
            return DoctorIndex("DoctorIndex_Normal");
        }
        //药店那要药剂师首页
        public async Task<IActionResult> DrugStoreIndex_Nurse()
        {
            var u = GetUserSelf();
            var rxDoctor = _docSvr.GetMyDefRxDoctor(u.StationId);            
            ViewBag.QrCodeUrl =rxDoctor==null?"":(await _weChatSvr.CreateQRCodeUrl(u.StationId, u.DoctorId, rxDoctor.DoctorId));
            return await DoctorIndex("DrugStoreIndex_Nurse");
        }

        #endregion


        #region 自定义的错误页面
        [AllowAnonymous]
        public IActionResult Error()
        {
            var ex = HttpContext.Features.Get<IExceptionHandlerPathFeature>().Error;//获取错误数据项目
            if (ex is ChkException)
            {
                return View("ChkExceptionError", (ChkException)ex);
            }
            else return View(ex);
        }

        [AllowAnonymous]
        public IActionResult Unaccess()
        {
            return View("ChkExceptionError", new ChkException("UNACCESS", "该功能为非授权功能！"));
        }

        #endregion
    }
}
