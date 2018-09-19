using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CHIS;
using System.Security.Claims;
using CHIS.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Ass;
using CHIS.Models.ViewModel;
using Microsoft.AspNetCore.Authentication;

namespace CHIS.Controllers
{
    public partial class HomeController : BaseController
    {


        #region Login 的登录登出
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectDefaultIndexPage();
            }
            Models.ViewModel.HisLoginViewModel model = null;
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Models.ViewModel.HisLoginViewModel model)
        {
            try
            {
                bool rlt = false;
                //解密传入的登录数据
                Func<string, HisLoginViewModel, (bool, HisLoginViewModel)> decriptModel = (type, m) =>
                 {
                     if (type == "STEP2" || type == "STEPEXT")
                     {
                        //验证解密数据
                        string[] dd = Ass.Data.Secret.Decript(m.LoadEncipt, Ass.Data.Secret.GetDynamicEncriptCode(m.BaseTimeTicks)).Split('|');
                         if (dd[0] == m.LoginName)
                         {
                             m.LoginPassword = dd[1];
                             rlt = true;
                         }
                         else throw new Exception("数据加密校验错误 002");
                     }
                     return (rlt, m);
                 };



                vwCHIS_Sys_Login login = null;

                //判断传入的数据，是否是初次传入(有密码，没有加密串)
                //使用传入的数据后，对密码会进行MD5加密处理，对用户名和密码进行3des加密处理
                string sendType = "";
                if (!string.IsNullOrEmpty(model.LoginPassword) && !string.IsNullOrEmpty(model.LoadEncipt) && !model.IsNeedLoginExt) sendType = "STEP2";
                if (!string.IsNullOrEmpty(model.LoginPassword) && string.IsNullOrEmpty(model.LoadEncipt)) sendType = "STEP1";
                if (model.IsNeedLoginExt) sendType = "STEPEXT";
                if (string.IsNullOrEmpty(sendType)) throw new Exception("数据加密校验错误 001");
                if (sendType == "STEP1" || sendType == "STEPEXT")
                {
                    if (!ModelState.IsValid) return View(model);
                    var mrlt = decriptModel(sendType, model);
                    login = CheckHisLogin_PwdCheck(mrlt.Item2, mrlt.Item1);
                }

                var stations = GetUserAllowedStations(model.LoginName);
                if (stations.Count() == 0) { ModelState.AddModelError("", "该用户没有工作站信息"); return View(); }
                if (model.StationId == 0)
                {
                    if (stations.Count() > 1)
                    {
                        if (sendType == "STEP1")
                        {
                            ViewBag.Stations = stations;
                            ViewBag.StationTree = new Code.Managers.UserFrameManager().GetStationsTree(stations);
                            model.BaseTimeTicks = DateTime.Now.Ticks;
                            model.LoadEncipt = Ass.Data.Secret.Encript(string.Format("{0}|{1}", model.LoginName, Ass.Data.Secret.MD5(model.LoginPassword))
                                , Ass.Data.Secret.GetDynamicEncriptCode(DateTime.Now.Ticks));//数据加密
                            model.LoginPassword = Ass.Data.Secret.Encript(model.LoginPassword, Global.SYS_ENCRIPT_PWD);//密码加密
                            ViewBag.DoctorId = GetLoginInfoName(model.LoginName).DoctorId;//医生的Id
                            return View("selectStation", model);
                        }

                    }
                    if (stations.Count() == 1) model.StationId = stations.FirstOrDefault().StationID;
                }

                //如果需要重登录系统
                if (login != null && login.NeedLoginExt)
                {
                    ViewBag.loginExtItems = _loginSvr.GetLoginExtsOfThis(login.LoginId).Where(m => m.LoginExtEnabled)
                                                .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                                                {
                                                    Value = m.LoginExtMobile,
                                                    Text = $"{m.LoginExtName}({m.LoginExtMobile})"
                                                });
                    ViewBag.loginStationStoreInfo = _loginSvr.GetStationStoreInfo(model.StationId, login.DoctorId.Value);

                    if (sendType == "STEP1")
                    {
                        model.BaseTimeTicks = DateTime.Now.Ticks;
                        model.LoadEncipt = Ass.Data.Secret.Encript(string.Format("{0}|{1}", model.LoginName, Ass.Data.Secret.MD5(model.LoginPassword))
                            , Ass.Data.Secret.GetDynamicEncriptCode(DateTime.Now.Ticks));//数据加密
                        model.LoginPassword = Ass.Data.Secret.Encript(model.LoginPassword, Global.SYS_ENCRIPT_PWD);//密码加密               
                    }
                    model.IsNeedLoginExt = true;

                    if (model.LoginExtPwd.IsEmpty() && model.LoginExtMobile.IsEmpty())
                    {
                        return View("loginExtInput", model);
                    }
                    else
                    {
                        if (!_loginSvr.CheckLoginExtPwd(login.LoginId, model.LoginExtMobile, model.LoginExtPwd, out Exception exx))
                        {
                            if (exx != null) ModelState.AddModelError("", exx.Message);
                            else ModelState.AddModelError("", "错误的密码！");
                            return View("loginExtInput", model);
                        }
                    }
                }


                if (!stations.Select(m => m.StationID).Contains(model.StationId))
                { ModelState.AddModelError("", "该用户没有被授权到指定工作站"); return View(); }




                if (sendType == "STEP1") model.LoginPassword = Ass.Data.Secret.MD5(model.LoginPassword);

                model = decriptModel(sendType, model).Item2;
                var customerId = await CheckHisLogin(model);
                if (customerId <= 0) throw new Exception("获取用户数据失败");



                if (customerId > 0)
                {
                    await Logger.WriteInfoAsync("HomeLogin", "Login", $"用户({model.LoginName})登录成功,工作站({model.StationId})");
                    return RedirectToAction("LoginedDefault");         //登录到默认页面
                }

                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var rlt = ex.Message;
                rlt = rlt.Replace("ChisAdmin", "[数据账户]").Replace("CHIS", "[天使数据库]");
                ModelState.AddModelError("", rlt);
                await Logger.WriteErrorAsync("HomeLogin", "Login", ex);
                return View(model);
            }


        }

        [HttpGet]
        public IActionResult ChangeStation()
        {
            var stations = UserMgr.GetAllowedStations(UserSelf.DoctorId);
            ViewBag.Stations = stations;
            ViewBag.StationTree = new Code.Managers.UserFrameManager().GetStationsTree(stations);
            ViewBag.Action = "ReLogin";
            ViewBag.DoctorId = UserSelf.DoctorId;
            return View("selectStation", new HisLoginViewModel
            {
                IsReLogin = true
            });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReLogin(Models.ViewModel.HisLoginViewModel model)
        {
            try
            {
                if (model.StationId <= 0) throw new Exception();
                var login = UserMgr.GetMyLoginData(UserSelf.LoginId);
                if (!model.DepartId.HasValue) model.DepartId = findDepartId(login.DoctorId.Value, model.StationId);
                await SignInProcess(login, model.StationId, model.DepartId, model.LoginExtMobile);
                await Logger.WriteInfoAsync("Home", "Login", $"用户({login.CustomerId},{login.CustomerName})切换登录到工作站({model.StationId})");

                return RedirectToAction("LoginedDefault");         //登录到默认页面
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Login", model);
            }
        }

        //修改接诊科室
        public Task<IActionResult> ChangeDepart(int departId)
        {
            Models.ViewModel.HisLoginViewModel model = new HisLoginViewModel()
            {
                DepartId = departId,
                StationId = UserSelf.StationId
            };
            return ReLogin(model);
        }


        public IActionResult LoginedDefault()
        {
            return View();
        }

        //返回登录后的首页
        public IActionResult RedirectDefaultIndexPage()
        {
            TempData["IsMenuLoad"] = base.getQuery("IsMenuLoad");
            var docCanTreat = UserMgr.IsMenuAllowed("PationtVisitV20");//允许访问接诊页面           
            var station = UserMgr.GetMyStation();

            //医生
            if (UserSelf.IsCanTreat && docCanTreat)
            {
                if (UserSelf.MyRoleNames.Contains("ds_assistant_doctor") ||
                    UserSelf.MyRoleNames.Contains("ds_doctor"))
                {
                    return RedirectToAction("DrugStoreIndex_Nurse");   //药店医生或者药店助理医生
                }

                if (station.IsNetPlat) return RedirectToAction("DoctorIndex_NetPlat");
                else return RedirectToAction("DoctorIndex"); //避免浏览器缓存买有刷新菜单 
                                                          


            }



            return RedirectToAction("DoctorIndex_Normal");
        }



        private vwCHIS_Sys_Login CheckHisLogin_PwdCheck(HisLoginViewModel model, bool pwdEncrypt = true)
        {
            var login = GetLoginInfoName(model.LoginName);
            if (login == null) throw new Exception("没有找到该登录信息。");
            if (model.IsPasswordVCode) //验证码登录
            {
                if (model.LoginName.GetStringType().IsMobile)
                {
                    var vcode = _db.CHIS_DataTemp_SMS.AsNoTracking()
                        .Where(m => m.PhoneCode == model.LoginName && m.VCodeProp == "DPWD" && m.CreatTime > DateTime.Now.AddMinutes(-10))
                        .OrderByDescending(m => m.CreatTime).FirstOrDefault();
                    if (vcode == null) throw new Exception("没有获取到动态密码的信息");
                    if ((DateTime.Now - vcode.CreatTime.Value).TotalSeconds > 240) throw new Exception("验证动态密码超时");
                    if (model.LoginPassword != (pwdEncrypt ? Ass.Data.Secret.MD5(vcode.VCode) : vcode.VCode)) throw new Exception("验证动态密码错误！");

                }
                else if (model.LoginName.GetStringType().IsEmail)
                {
                    var vcode = _db.CHIS_DataTemp_SendMailVCode.AsNoTracking()
                        .Where(m => m.EmailAddress == model.LoginName && m.VCodeProp == "DPWD" && m.CreatTime > DateTime.Now.AddMinutes(-10))
                        .OrderByDescending(m => m.CreatTime).FirstOrDefault();
                    if (vcode == null) throw new Exception("没有获取到动态密码的信息");
                    if ((DateTime.Now - vcode.CreatTime.Value).TotalSeconds > 120) throw new Exception("验证动态密码超时");
                    if (model.LoginPassword != (pwdEncrypt ? Ass.Data.Secret.MD5(vcode.VCode) : vcode.VCode)) throw new Exception("验证动态密码错误！");
                }
                else throw new Exception("非手机号码或邮箱，不能使用动态密码登录。");
            }
            else
            {
                //密码是明码，后续改成md5加密码
                if (login.LoginPassword.IsEmpty()) throw new Exception("没有设置密码");

                if ((pwdEncrypt ? Ass.Data.Secret.MD5(login.LoginPassword) : login.LoginPassword) != model.LoginPassword) throw new Exception("密码错误！");
            }

            return login;
        }
        private async Task<int> CheckHisLogin(HisLoginViewModel model)
        {
            var login = CheckHisLogin_PwdCheck(model);
            await SignInProcess(login, model.StationId, findDepartId(login.DoctorId.Value, model.StationId), model.LoginExtMobile);
            return login.CustomerId.Value;
        }

        private int? findDepartId(int doctorId, int stationId) //获取部门Id
        {
            var station = _db.CHIS_Code_WorkStation.Find(stationId);
            if (station.IsCanTreat)
            {
                var finds = _db.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationID == stationId).ToList();
                if (finds.Count() == 0) return null;
                else return finds.First().DepartId;
            }
            return null;
        }


        internal async Task<ClaimsPrincipal> GetSignInPrincipalAsync(vwCHIS_Sys_Login login, int stationId, int? departId, string loginExtMobile = "")
        {
            //登录注册信息写入 ------------------------------------------------------------------
            //注册登记信息
            var claims = new List<Claim>();



            Action<string, object> claimsAdd = (key, val) =>
            {
                string v = "";
                if (val is DateTime || val is DateTime?) v = ((DateTime)val).Ticks.ToString();
                else if (val is IEnumerable<int>) v = string.Join(",", (IEnumerable<int>)val);
                else if (val is IEnumerable<string>) v = string.Join(",", (IEnumerable<string>)val);
                else v = Ass.P.PStr(val);
                claims.Add(new Claim(key, v));
            };

            claims.Add(new Claim(ClaimTypes.NameIdentifier, login.CustomerId.ToString(), ClaimValueTypes.Integer, Global.AUTHENTICATION_ISSUER));
            claims.Add(new Claim(ClaimTypes.Name, login.CustomerName ?? "", ClaimValueTypes.String, Global.AUTHENTICATION_ISSUER));
            // claims.Add(new Claim(ClaimTypes.Role, userLoginData.RoleName ?? "", ClaimValueTypes.String, Global.AUTHENTICATION_ISSUER));             

            claimsAdd("LoginId", login.LoginId);
            claimsAdd("OpId", login.CustomerId);
            claimsAdd("DoctorId", login.DoctorId);
            claimsAdd("OpMan", login.CustomerName);
            var cus = await _db.vwCHIS_Code_Customer.AsNoTracking().FirstOrDefaultAsync(m => m.CustomerID == login.CustomerId);
            claimsAdd("Gender", cus.Gender);
            claimsAdd("Birthday", cus.Birthday ?? DateTime.Today);

            var docr = await _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefaultAsync(m => m.CustomerId == login.CustomerId && m.DoctorId == login.DoctorId);
            claimsAdd("PostTitleName", docr.PostTitleName);
            claimsAdd("PhotoUrlDef", docr.PhotoUrlDef);
            claimsAdd("DoctorAppId", docr.DoctorAppId);//app端的用户Id

            var ws = _db.CHIS_Code_WorkStation.Find(stationId);
            claimsAdd("StationId", stationId);
            claimsAdd("DrugStoreStationId", ws.DrugStoreStationId ?? stationId);//药品药房Id
            claimsAdd("StationName", ws.StationName);//工作站名称
            claimsAdd("StationTypeId", ws.StationTypeId);
            claimsAdd("LoginTime", DateTime.Now);
            claimsAdd("IsCanTreat", ws.IsCanTreat);
            claimsAdd("IsManageUnit", ws.IsManageUnit);
            var stationIds = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.StationIsEnable && m.DoctorId == login.DoctorId).Select(m => m.StationId);
            claimsAdd("MyAllowStationIds", stationIds);
            var sonStationIds = _db.CHIS_Code_WorkStation.AsNoTracking().Where(m => m.ParentStationID == stationId).Select(m => m.StationID);
            claimsAdd("MySonStations", sonStationIds);


            departId = departId ?? findDepartId(login.DoctorId.Value, stationId);
            var depart = departId.HasValue ? _db.CHIS_Code_Department.Find(departId) : null;
            claimsAdd("SelectedDepartmentId", departId);//选择的部门    
            claimsAdd("SelectedDepartmentName", depart?.DepartmentName);


            var myroleids = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == login.DoctorId && m.StationId == stationId && m.MyRoleIsEnable && m.MyStationIsEnable).Select(m => m.RoleId.Value);
            claimsAdd("MyRoleIds", myroleids);
            var myrolekeys = _db.CHIS_SYS_Role.AsNoTracking().Where(m => myroleids.Contains(m.RoleID)).Select(m => m.RoleKey);
            claimsAdd("MyRoleNames", myrolekeys);


            //辅助登录
            if (login.NeedLoginExt)
            {
                var loginExt = _loginSvr.GetLoginExt(loginExtMobile, login.LoginId);
                if (loginExt == null) throw new Exception("登录信息没有获取到");
                if (!loginExt.LoginExtEnabled) throw new Exception("该用户已禁用");

                //辅助登录
                claimsAdd("LoginExtId", loginExt.LoginExtId);
                claimsAdd("LoginExtMobile", loginExt.LoginExtMobile);
                claimsAdd("LoginExtName", loginExt.LoginExtName);
                claimsAdd("LoginExtFuncKeys", _loginSvr.GetLoginExtFuncKeys(loginExt.LoginExtId));
            }
            else
            {
                claimsAdd("LoginExtId", 0);
                claimsAdd("LoginExtMobile", "");
                claimsAdd("LoginExtName", "");
                claimsAdd("LoginExtFuncKeys", "");
            }


            var userIdentity = new ClaimsIdentity(Global.AUTHENTICATION_CLAIMS_IDENTITY);//其他都可以，主要獲取時候方便
            userIdentity.AddClaims(claims);
            //驗證書
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            return userPrincipal;
        }

        private async Task SignInProcess(vwCHIS_Sys_Login login, int stationId, int? departId, string loginExtMobile)
        {

            var userPrincipal = await GetSignInPrincipalAsync(login, stationId, departId, loginExtMobile);
            await HttpContext.SignInAsync(Global.AUTHENTICATION_SCHEME, userPrincipal,
                     new AuthenticationProperties
                     {
                         ExpiresUtc = DateTime.UtcNow.AddDays(120),
                         IsPersistent = true,
                         AllowRefresh = true
                     });

        }


        /// <summary>
        /// 获取用户允许的工作站信息
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public IEnumerable<vwCHIS_Code_WorkStation> GetUserAllowedStations(string loginName)
        {
            var doctor = GetLoginInfoName(loginName);
            if (doctor == null) throw new Exception("没有找到该登录用户");
            if (doctor.IsLock == true) throw new Exception("该用户已经被锁定:" + doctor.WhyLock);//锁定用户
            if (doctor.DoctorId == null || doctor.DoctorId <= 0) throw new Exception("该用户非医生用户。");
            return new Code.Managers.UserFrameManager().GetAllowedStations(doctor.DoctorId.Value);
        }

        /// <summary>
        /// 根据登录名获取数据库内的登录信息视图实体
        /// </summary>
        public vwCHIS_Sys_Login GetLoginInfoName(string loginName)
        {
            loginName = Ass.P.PStr(loginName);
            var dm = _db.vwCHIS_Sys_Login.AsNoTracking();
            if ((loginName.GetStringType().IsEmail)) return dm.FirstOrDefault(m => m.Email == loginName);
            if (loginName.GetStringType().IsMobile) return dm.FirstOrDefault(m => m.Mobile == loginName);
            if (loginName.GetStringType().IsIdCardNumber) return dm.FirstOrDefault(m => m.IdCardNumber == loginName);
            return dm.FirstOrDefault(m => m.LoginName == loginName);
        }
        /// <summary>
        /// 验证是否存在账户
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public IEnumerable<vwCHIS_Sys_Login> GetLoginInfoItems(string loginName)
        {
            loginName = Ass.P.PStr(loginName);
            var dm = _db.vwCHIS_Sys_Login.AsNoTracking();
            if ((loginName.GetStringType().IsEmail)) return dm.Where(m => m.Email == loginName).ToList();
            if (loginName.GetStringType().IsMobile) return dm.Where(m => m.Mobile == loginName).ToList();
            if (loginName.GetStringType().IsIdCardNumber) return dm.Where(m => m.IdCardNumber == loginName).ToList();
            return dm.Where(m => m.LoginName == loginName);
        }

        //系统登出
        [HttpGet]
        [HttpPost]
        [AllowAnonymous]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> OutLogin()
        {

            try
            {
                await Logger.WriteInfoAsync("HomeLogin", "Logout", $"OpId:{UserSelf.OpId}({UserSelf.OpManFullMsg})主动退出系统");
                await HttpContext.SignOutAsync(Global.AUTHENTICATION_SCHEME);
                this.HttpContext.Session.Clear();
            }
            catch { }
            return RedirectToAction("Login", "Home");
        }


        [AllowAnonymous]
        public JsonResult SendLoginVCode(string loginName)
        {
            try
            {
                var items = GetLoginInfoItems(loginName);
                if (items.Count() <= 0) throw new Exception("不存在该账户，请输入正确登录账号");
                var st = loginName.GetStringType();
                if (st.IsEmpty) throw new Exception("没有输入邮箱、手机号、身份证号");
                if (st.IsEmail)
                {
                    //6为随机数并存数据库
                    var random = CHIS.Code.Utility.ComTools.GenerateRandomNumber(6);
                    var emailData = new CHIS_DataTemp_SendMailVCode
                    {
                        CreatTime = DateTime.Now,
                        EmailAddress = loginName,
                        VCode = random,
                        VCodeProp = "DPWD"
                    };
                    _db.CHIS_DataTemp_SendMailVCode.Add(emailData);
                    _db.SaveChanges();
                    //向邮箱发送一份验证邮件
                    CHIS.Codes.Utility.EmailHelper email = new CHIS.Codes.Utility.EmailHelper();
                    string sub = "天使健康医生工作站";
                    string msg = $"{random}本次登录验证码";
                    email.SendEmail(loginName, msg, sub);

                }
                if (st.IsMobile)
                {
                    //6为随机数并存数据库
                    var random = CHIS.Code.Utility.ComTools.GenerateRandomNumber(6);
                    var smsData = new CHIS_DataTemp_SMS()
                    {
                        CreatTime = DateTime.Now,
                        PhoneCode = loginName,
                        VCodeProp = "DPWD",
                        VCode = random
                    };
                    _db.CHIS_DataTemp_SMS.Add(smsData);
                    _db.SaveChanges();
                    //向手机发送一份验证码
                    Codes.Utility.SMS sms = new Codes.Utility.SMS();
                    string content = $"{random}为动态登录验证码，有效时间为1分钟【天使健康】";
                    sms.PostSmsInfoAsync(loginName, content).ToString();


                }
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }


        #endregion




    }
}
