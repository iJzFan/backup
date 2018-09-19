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
using System.Text;

namespace CHIS.Controllers
{

    public partial class MyPanelController : BaseController
    {
        Services.DoctorService _docSvr;
        public MyPanelController(DbContext.CHISEntitiesSqlServer db
            , Services.DoctorService docSvr) : base(db)
        {
            _docSvr = docSvr;
        }
        public IActionResult ChangeLoginInfo()
        {
            var u = UserSelf;
            var model = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.LoginId == u.LoginId);
            return View(nameof(ChangeLoginInfo), model);
        }

        public IActionResult ChangeMobile()
        {
            ViewBag.Login = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == this.UserSelf.DoctorId);
            var u = UserSelf;
            var model = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.LoginId == u.LoginId);
            ViewBag.OriginalMobile = model.Mobile;
            return View(nameof(ChangeMobile));
        }

        public IActionResult ChangeMobile2(CHIS.Models.ViewModel.ChangeLoginInfo model)
        {
            try
            {
                switch (model.VarifyType.ToLower())
                {
                    case "mobile":
                        if (!new SendVCodeCBL(this).VerifyCheck(model.MobileNumber, model.MobileNumberVCode)) ModelState.AddModelError("", "手机验证码校验错误");
                        break;
                    case "email":
                        if (!new SendVCodeCBL(this).VerifyCheck(model.Email, model.EmailVCode)) ModelState.AddModelError("", "邮箱验证码校验错误");
                        break;
                    case "idcard":
                        break;
                    default: throw new Exception("验证方式传入错误");
                }
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); }
            if (!ModelState.IsValid) return ChangeMobile();
            var orig_Mobile = _db.CHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == this.UserSelf.DoctorId).Mobile;
            ViewBag.OriginalMobile = orig_Mobile;
            ViewBag.CheckCode = Ass.Data.Secret.Encript(orig_Mobile, Global.SYS_ENCRIPT_PWD);
            return View();


        }
        public async Task<IActionResult> ChangeMobileOk(string originalMobile, string checkCode, string mobileNumber, string vcode)
        {
            try
            {
                if (originalMobile.IsEmpty()) throw new Exception("传入原手机号为空");
                if (checkCode.IsEmpty() || Ass.Data.Secret.Decript(checkCode, Global.SYS_ENCRIPT_PWD) != originalMobile) throw new Exception("数据校验错误");
                if (!new SendVCodeCBL(this).VerifyCheck(mobileNumber, vcode)) throw new Exception("验证码验证错误");
                //调整数据库内的Email数据
                var rlt = await new LoginCBL(this).ChangeLoginMobileAsync(mobileNumber);
                if (!rlt) throw new Exception("更改手机失败");
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.BackLink = "/mypanel/ChangeLoginInfo";
                ViewBag.BackLinkName = "返回更改资料";
                return View("Error", ex);
            }
        }



        public IActionResult ChangeEmail()
        {
            ViewBag.Login = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == this.UserSelf.DoctorId);
            var u = UserSelf;
            var model = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.LoginId == u.LoginId);
            ViewBag.OriginalEmail = model.Email;
            return View(nameof(ChangeEmail));
        }

        public IActionResult ChangeEmail2(CHIS.Models.ViewModel.ChangeLoginInfo model)
        {
            try
            {
                switch (model.VarifyType.ToLower())
                {
                    case "mobile":
                        if (!new SendVCodeCBL(this).VerifyCheck(model.MobileNumber, model.MobileNumberVCode)) ModelState.AddModelError("", "手机验证码校验错误");
                        break;
                    case "email":
                        if (!new SendVCodeCBL(this).VerifyCheck(model.Email, model.EmailVCode)) ModelState.AddModelError("", "邮箱验证码校验错误");
                        break;
                    case "idcard":
                        break;
                    default: throw new Exception("验证方式传入错误");
                }
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); }
            if (!ModelState.IsValid) return ChangeEmail();
            var orig_email = _db.CHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == this.UserSelf.DoctorId).Email;
            ViewBag.OriginalEmail = orig_email;
            ViewBag.CheckCode = Ass.Data.Secret.Encript(orig_email, Global.SYS_ENCRIPT_PWD);
            return View();
        }
        public async Task<IActionResult> ChangeEmailOk(string originalEmail, string checkCode, string email, string vcode)
        {
            try
            {

                if (originalEmail.IsEmpty() && !string.IsNullOrEmpty(checkCode)) throw new Exception("传入原Email为空");
                if (originalEmail.IsNotEmpty() && (checkCode.IsEmpty() || Ass.Data.Secret.Decript(checkCode, Global.SYS_ENCRIPT_PWD) != originalEmail)) throw new Exception("数据校验错误");

                if (!new SendVCodeCBL(this).VerifyCheck(email, vcode)) throw new Exception("验证码验证错误");
                //调整数据库内的Email数据
                var rlt = await new LoginCBL(this).ChangeLoginEmailAsync(email);
                if (!rlt) throw new Exception("更改邮箱地址失败");
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.BackLink = "/mypanel/ChangeLoginInfo";
                ViewBag.BackLinkName = "返回更改资料";
                return View("Error", ex);
            }
        }

        [AllowAnonymous]
        public IActionResult VerifyEmail(string c)
        {
            var cc = Ass.Data.Secret.Decript(c, Global.SYS_ENCRIPT_PWD);
            return View("ChangeEmailOK");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(CHIS.Models.ViewModel.ChangeLoginInfo model)
        {
            try
            {
                if (model.MyPswd != model.MyPswdConfirm) throw new Exception("密码验证错误");
                if (model.VarifyType == "mobile")
                {
                    if (!new SendVCodeCBL(this).VerifyCheck(model.MobileNumber, model.MobileNumberVCode)) throw new Exception("手机验证码验证错误");
                    var find = _db.CHIS_Sys_Login.FirstOrDefault(m => m.Mobile == model.MobileNumber);
                    find.LoginPassword = model.MyPswd;
                    _db.SaveChanges();
                }
                else if (model.VarifyType == "email")
                {
                    if (!new SendVCodeCBL(this).VerifyCheck(model.Email, model.EmailVCode)) throw new Exception("邮箱验证码验证错误");
                    var find = _db.CHIS_Sys_Login.FirstOrDefault(m => m.Email == model.Email);
                    find.LoginPassword = model.MyPswd;
                    _db.SaveChanges();
                }
                else if (model.VarifyType == "idcard")
                {
                    if (model.IDCard.IsEmpty()) throw new Exception("没有输入身份证号码");
                    var find = _db.CHIS_Sys_Login.FirstOrDefault(m => m.IdCardNumber == model.IDCard);
                    if (find == null) throw new Exception("没有发现此身份证的登录信息");
                    if (find.LoginPassword != model.OrigPswd) throw new Exception("密码验证错误");

                    find.LoginPassword = model.MyPswd;
                    _db.SaveChanges();
                }
                else throw new Exception("不支持的修改类型");
                return View("AdoptSuccess");//返回成功页面
            }
            catch (Exception ex) { return View("Error", ex); }

        }
        public IActionResult AdoptPassword()
        {
            return View();
        }
        public IActionResult AdoptMobile()
        {
            var model = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.LoginId == UserSelf.LoginId);
            ViewBag.Mobile = model.Mobile;
            return View();
        }
        public IActionResult AdoptEmail()
        {
            var model = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.LoginId == UserSelf.LoginId);
            ViewBag.Email = model.Email;
            return View();
        }
        public IActionResult AdoptIDcard()
        {
            var model = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.LoginId == UserSelf.LoginId);
            ViewBag.IDCard = model.IdCardNumber;
            return View();
        }



        public async Task<IActionResult> SendMobileVerifyCode(string mobile)
        {
            try
            {
                mobile = mobile.Trim();
                var login = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == this.UserSelf.DoctorId);
                if (login == null) throw new Exception("没有找到登录信息");
                if (login.Mobile.IsEmpty()) throw new Exception("用户登录信息的手机号为空");
                if (login.Mobile != mobile) throw new Exception("您输入了非注册的手机号码");
                if (login.MobileIsAuthenticated != true) throw new Exception("您留的手机号码没有通过验证");
                return await new Api.VCode(_db).SendVCode("mobile", mobile);
            }
            catch (Exception ex)
            {
                return Json(new { rlt = false, msg = ex.Message });
            }
        }
        public async Task<IActionResult> SendEmailVerifyCode(string email)
        {
            try
            {
                var login = _db.vwCHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == this.UserSelf.DoctorId);
                if (login == null) throw new Exception("没有找到登录信息");
                if (login.Email.IsEmpty()) throw new Exception("用户登录信息的邮箱号为空");
                if (login.Email != email) throw new Exception("您输入了非注册的邮箱号码");
                if (login.EmailIsAuthenticated != true) throw new Exception("您留的邮箱号码没有通过验证");
                return await new Api.VCode(_db).SendVCode("email", email);
            }
            catch (Exception ex)
            {
                return Json(new { rlt = false, msg = ex.Message });
            }
        }
        public async Task<IActionResult> SendNewEmailVCode(string newEmail)
        {
            return await new Api.VCode(_db).SendNewEmailVCode(newEmail);
        }
        public async Task<IActionResult> SendMobileNewVCode(string newMobile)
        {
            return await new Api.VCode(_db).SendMobileNewVCode(newMobile);
        }



    }
}
