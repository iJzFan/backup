using Ass;
using CHIS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Controllers
{
    public class SendVCodeCBL : BaseCBL
    {

        public SendVCodeCBL(BaseController c) : base(c) { }


        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="account">账号或者手机号</param>
        /// <param name="sendTemplate">验证码的模板 [code]</param>
        /// <returns></returns>
        public async Task<bool> SendVCode(string account, string sendTemplate, string vCodeProp = null, bool bCheckVrifyed = true, bool bCheckExists = true)
        {            
            var st = account.GetStringType();
            if (st.IsEmpty) throw new Exception("没有输入邮箱、手机号、身份证号");

            Func<string, IEnumerable<vwCHIS_Sys_Login>> GetLoginInfoItems = (loginName) =>
            {
                if ((loginName.GetStringType().IsEmail)) return _db.vwCHIS_Sys_Login.Where(m => m.Email == loginName).ToList();
                if (loginName.GetStringType().IsMobile) return _db.vwCHIS_Sys_Login.Where(m => m.Mobile == loginName).ToList();
                if (loginName.GetStringType().IsIdCardNumber) return _db.vwCHIS_Sys_Login.Where(m => m.IdCardNumber == loginName).ToList();
                return null;
            };

            if (bCheckExists || bCheckVrifyed) // 检测账户是否存在
            {
                var items = GetLoginInfoItems(account);
                if (items.Count() <= 0) throw new Exception("不存在该账户，请输入正确登录账号");
                if (items.Count() > 1) throw new Exception("检测到多账户，违反账户唯一性要求");

                if (bCheckVrifyed)
                {
                    var item = items.FirstOrDefault();
                    if (st.IsMobile)
                    {
                        if (item.MobileIsAuthenticated != true) throw new Exception("该手机号没有通过验证");
                    }
                    else if (st.IsEmail)
                    {
                        if (item.EmailIsAuthenticated != true) throw new Exception("该邮箱没有通过验证");
                    }
                }
            }



            if (st.IsEmail)
            {
                //6为随机数并存数据库
                var random = CHIS.Code.Utility.ComTools.GenerateRandomNumber(6);
                var emailData = new CHIS_DataTemp_SendMailVCode
                {
                    CreatTime = DateTime.Now,
                    EmailAddress = account,
                    VCode = random,
                    VCodeProp = null
                };
                await _db.CHIS_DataTemp_SendMailVCode.AddAsync(emailData);
                await _db.SaveChangesAsync();
                //向邮箱发送一份验证邮件
                CHIS.Codes.Utility.EmailHelper email = new CHIS.Codes.Utility.EmailHelper();
                string sub = "天使健康医生工作站-(验证码,不用回复)";
                string msg = sendTemplate.Replace("[code]", random);//  $"{random}本次操作验证码";
                email.SendEmail(account, msg, sub);
            }
            if (st.IsMobile)
            {
                //6为随机数并存数据库
                var random = CHIS.Code.Utility.ComTools.GenerateRandomNumber(6);
                var smsData = new CHIS_DataTemp_SMS()
                {
                    CreatTime = DateTime.Now,
                    PhoneCode = account,
                    VCodeProp = null,
                    VCode = random
                };
                await _db.CHIS_DataTemp_SMS.AddAsync(smsData);
                await _db.SaveChangesAsync();
                //向手机发送一份验证码
                Codes.Utility.SMS sms = new Codes.Utility.SMS();
                string content = sendTemplate.Replace("[code]", random);// $"{random}为本次操作验证码，有效时间为1分钟【天使健康】";
                await sms.PostSmsInfoAsync(account, content);
            }

            return true;
        }



        public async Task<bool> SendEmailHtmlAsync(string emailAcount, string htmlContent)
        {
            CHIS.Codes.Utility.EmailHelper email = new CHIS.Codes.Utility.EmailHelper();
            string sub = "天使健康医生工作站-(邮箱账号验证,不用回复)";
            await email.SendEmailHtmlAsync(emailAcount, htmlContent, sub);
            return true;
        }

        /// <summary>
        /// 验证码确认
        /// </summary> 
        public bool VerifyCheck(string account, string vcode)
        {
            var type = account.GetStringType();
            if (vcode.IsEmpty() || vcode.Length < 4) throw new Exception("验证码不够位数呀");
            if (type.IsEmail)
            {
                var find = _db.CHIS_DataTemp_SendMailVCode.AsNoTracking().Where(m => m.EmailAddress == account && m.VCodeProp == null && m.CreatTime > DateTime.Today).OrderByDescending(m => m.CreatTime).FirstOrDefault();
                if (find == null) throw new Exception("没有找到邮件验证码");
                if (!string.Equals(find.VCode, vcode, StringComparison.CurrentCultureIgnoreCase)) throw new Exception("验证码输入错误");
                return true;
            }
            if (type.IsMobile)
            {
                var find = _db.CHIS_DataTemp_SMS.AsNoTracking().Where(m => m.PhoneCode == account && m.VCodeProp == null && m.CreatTime > DateTime.Today).OrderByDescending(m => m.CreatTime).FirstOrDefault();
                if (find == null) throw new Exception("没有找到短信验证码");
                if (!string.Equals(find.VCode, vcode, StringComparison.CurrentCultureIgnoreCase)) throw new Exception("验证码输入错误");
                return true;
            }

            return false;
        }



    }
}
