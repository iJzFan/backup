using CHIS.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Api
{
    public class VCode : BaseDBController
    {
        public VCode(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        /// <summary>
        /// 给已经登记的邮箱、手机发送验证码
        /// </summary>
        /// <param name="type">mobile/email</param>
        /// <param name="account">账户，手机号/邮箱地址</param>
        /// <returns></returns>
        public async Task<IActionResult> SendVCode(string type, string account)
        {
            try
            {
                var template = "[code]本次操作验证码";
                if (type == "mobile") template = "[code]为本次操作验证码，有效时间为1分钟【天使健康】";
                await new SendVCodeCBL(this).SendVCode(account, template);
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        /// <summary>
        /// 发送手机验证码（手机未登记)
        /// </summary> 
        [AllowAnonymous]
        public async Task<IActionResult> SendMobileNewVCode(string newMobile)
        {
            try
            {
                var template = "[code]您更换的新手机操作验证码，有效时间为1分钟【天使健康】";
                await new SendVCodeCBL(this).SendVCode(newMobile, template, null, false, false);
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        /// <summary>
        /// 发送手机注册验证码
        /// </summary>
        /// <param name="mobile">手机号</param>   
        [AllowAnonymous]
        public async Task<IActionResult> SendMobileVCode_regist(string mobile)
        {
            try
            {
                var template = "[code] 您的注册手机验证码，有效时间为1分钟。【天使健康】";
                await new SendVCodeCBL(this).SendVCode(mobile, template, null, false, false);
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        /// <summary>
        /// 发送邮箱注册验证码
        /// </summary> 
        [AllowAnonymous]
        public async Task<IActionResult> SendEmailVCode_regist(string email)
        {
            try
            {
                var template = "[code] 您的注册邮箱验证码，有效时间为1分钟。【天使健康】";
                await new SendVCodeCBL(this).SendVCode(email, template, null, false, false);
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        [AllowAnonymous]
        public async Task<IActionResult> SendNewEmailVCode(string newemail)
        {
            try
            {
                var template = "[code]您更换的新登录邮箱操作验证码，有效时间为1分钟【天使健康】";
                await new SendVCodeCBL(this).SendVCode(newemail, template, null, false, false);
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        /// <summary>
        /// 发送未登记邮箱的验证邮件
        /// </summary> 
        [AllowAnonymous]
        public async Task<IActionResult> SendEmailVCode(string newEmail)
        {
            try
            {
                StringBuilder b = new StringBuilder();
                var u = UserSelf;
                var cus = UserMgr.GetMyLoginData(u.LoginId);
                b.AppendFormat("尊敬的用户 <b> {0} </b>:<br>", cus.CustomerName);
                b.Append("您正在更换新的邮箱，请点击下面的链接，确认您的邮箱。<br><br>");

                string c = string.Format("origemail={0},newemail={1},datetime={2:yyyyMMddTHHmmss}", cus.Email, newEmail, DateTime.Now);
                string cc = Ass.Data.Secret.Encript(c, Global.SYS_ENCRIPT_PWD);
                string link = "http://www/MyPanel/VerifyEmail?c=" + cc;

                b.AppendFormat("<a href='{0}'>{0}</a><br><br>", link);
                b.Append("天使健康开发团队<br>");
                b.AppendFormat("{0:yyyy年MM月dd日 HH:mm:ss}", DateTime.Now);
                await new SendVCodeCBL(this).SendEmailHtmlAsync(newEmail, b.ToString());
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }


        /// <summary>
        /// 获取手机号是否可以注册
        /// </summary>
        /// <param name="mobile">手机号</param> 
        [AllowAnonymous]
        public dynamic MobileCanRegist(string mobile)
        {
            var have = _db.CHIS_Sys_Login.FirstOrDefault(m => m.Mobile == mobile);
            if (have != null)
            {
                if (have.CustomerId == null && have.DoctorId == null)
                    return new { rlt = false, state = "not_customer_doctor" };
                else if (have.CustomerId == null)
                    return new { rlt = false, state = "not_customer" };
                else if (have.DoctorId == null)
                    return new { rlt = false, state = "not_doctor" };
                else return new { rlt = false, state = "existed" };
            }
            else
                return new { rlt = true, state = "null" };
        }
        /// <summary>
        /// 电子信箱是否可注册
        /// </summary>
        /// <param name="email">电子信箱</param> 
        [AllowAnonymous]
        public dynamic EmailCanRegist(string email)
        {       
            var have = _db.CHIS_Sys_Login.FirstOrDefault(m => m.Email == email);
            if (have != null)
            {
                if (have.CustomerId == null && have.DoctorId == null)
                    return new { rlt = false, state = "not_customer_doctor" };
                else if (have.CustomerId == null)
                    return new { rlt = false, state = "not_customer" };
                else if (have.DoctorId == null)
                    return new { rlt = false, state = "not_doctor" };
                else return new { rlt = false, state = "existed" };
            }
            else
                return new { rlt = true, state = "null" };
        }

    }
}
