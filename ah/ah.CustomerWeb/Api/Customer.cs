using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass;
using ah.Areas.Customer.Controllers.Base;
using ah.Models;
using ah.Models.ViewModel;
using ah.DbContext;

namespace ahWeb.Api
{
    [Authorize]
    public class Customer : BaseDBController
    {
        public Customer(AHMSEntitiesSqlServer db) : base(db) { }
        /// <summary>
        /// 是否客户已经注册
        /// </summary>
        /// <param name="regAccount">注册账号 Email/Mobile</param>
        /// <returns></returns>
        [AllowAnonymous]
        public bool IsCustomerRegisted(string regAccount)
        {
            var t = regAccount.GetStringType();
            if (t.IsEmail)
            {
                var n = MainDbContext.vwCHIS_Sys_Login.Where(m => m.Email == regAccount).Count();
                if (n > 0) return true;
            }
            if (t.IsMobile)
            {
                var n = MainDbContext.vwCHIS_Sys_Login.Where(m => m.Mobile == regAccount).Count();
                if (n > 0) return true;
            }
            return false;
        }



        /// <summary>
        /// 向注册账户发送验证码 在120秒内不重复发送
        /// </summary>
        /// <param name="regAccount"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<JsonResult> SendRegVCode(string regAccount)
        {
            int NO_REPEAT_SEC = 120;//在80秒内不重复发送
            try
            {
                regAccount = Ass.P.PStr(regAccount).Trim();
                if (regAccount.GetStringType().IsMobile)
                {
                    var now = DateTime.Now;
                    var h = MainDbContext.CHIS_DataTemp_SMS.AsNoTracking().Where(m => m.PhoneCode == regAccount).OrderByDescending(m => m.SMSId).FirstOrDefault();
                    if (h != null && (now - h.CreatTime.Value).TotalSeconds < NO_REPEAT_SEC)
                        return Json(new { rlt = true, msg = $"手机验证码已经发送，{NO_REPEAT_SEC}秒内不用重复请求。" });
                    string contents = "";
                    var random = ah.Code.Utility.ComTools.GenerateRandomNumber(6, true);
                    var sms = new ah.Models.CHIS_DataTemp_SMS();
                    sms.PhoneCode = regAccount;
                    sms.VCode = random.ToString();
                    sms.CreatTime = now;
                    MainDbContext.CHIS_DataTemp_SMS.Add(sms);
                    MainDbContext.SaveChanges();
                    var s = new SMS();
                    h = MainDbContext.CHIS_DataTemp_SMS.OrderByDescending(m => m.CreatTime).FirstOrDefault(m => m.PhoneCode == regAccount);
                    contents = $"{h.VCode} , 您的注册手机验证码【天使健康】";
                    string rlt = await s.PostSmsInfo(regAccount, contents);
                    if (rlt != "true") new Exception(rlt);
                    return Json(new { rlt = true, msg = "手机验证码发送成功" });
                }

                if (regAccount.GetStringType().IsEmail)
                {
                    var now = DateTime.Now;
                    var h = MainDbContext.CHIS_DataTemp_SendMailVCode.AsNoTracking().Where(m => m.EmailAddress == regAccount).OrderByDescending(m => m.SendMailId).FirstOrDefault();                    
                    if (h != null && (now - h.CreatTime.Value).TotalSeconds < NO_REPEAT_SEC)
                        return Json(new { rlt = true, msg = $"邮箱验证码已经发送，{NO_REPEAT_SEC}秒内不用重复请求。" });

                    //6为随机数并存数据库
                    var random = ah.Code.Utility.ComTools.GenerateRandomNumber(6, true);
                    var emailData = new CHIS_DataTemp_SendMailVCode
                    {
                        CreatTime = now,
                        EmailAddress = regAccount,
                        VCode = random,
                        VCodeProp = null
                    };
                    MainDbContext.CHIS_DataTemp_SendMailVCode.Add(emailData);
                    MainDbContext.SaveChanges();
                    //向邮箱发送一份验证邮件
                    EmailHelper email = new EmailHelper();
                    string sub = "天使健康医生工作站-(验证码,不用回复)";
                    string msg = "您的邮箱验证码为:[code]".Replace("[code]", random);//  $"{random}本次操作验证码";
                    email.SendEmail(regAccount, msg, sub);
                    return Json(new { rlt = true, msg = "" });
                }

                throw new Exception("传入非法账户号");
            }
            catch (Exception e)
            { return Json(new { rlt = false, msg = e.Message }); }
        }


        /// <summary>
        /// 验证注册信息
        /// </summary>
        /// <param name="regAccount"></param>
        public void CheckRegVCode(string regAccount, string vcode)
        {
            if (string.IsNullOrWhiteSpace(vcode)) throw new Exception("没有填写验证码");
            if (string.IsNullOrWhiteSpace(regAccount)) throw new Exception("没有填写账号");
            var a = regAccount.GetStringType();
            if (a.IsEmail) //邮箱注册
            {
                var vm = MainDbContext.CHIS_DataTemp_SendMailVCode.AsNoTracking().Where(m => m.EmailAddress == regAccount && (DateTime.Now - m.CreatTime.Value).TotalMinutes < 20).OrderByDescending(m => m.CreatTime).FirstOrDefault();
                if (vm == null) throw new Exception("没有发现验证码信息");
                if ((DateTime.Now - vm.CreatTime.Value).TotalSeconds > 220) throw new Exception("验证码超时");
                if (vm.VCode != vcode) throw new Exception("验证码验证错误");
            }
            else if (a.IsMobile) //手机注册
            {
                var vm = MainDbContext.CHIS_DataTemp_SMS.AsNoTracking().Where(m => m.PhoneCode == regAccount && (DateTime.Now - m.CreatTime.Value).TotalMinutes < 20).OrderByDescending(m => m.CreatTime).FirstOrDefault();
                if (vm == null) throw new Exception("没有发现验证码信息");
                if ((DateTime.Now - vm.CreatTime.Value).TotalSeconds > 120) throw new Exception("验证码超时");
                if (vm.VCode != vcode) throw new Exception("验证码验证错误");
            }
            else throw new Exception("注册没有填入正确格式的 手机/邮箱 号码");
        }








        /// <summary>
        /// 通过手机号码/邮箱/身份证查询用户的基本信息(item 数组) 
        /// </summary>
        /// <param name="key">搜索关键字</param>
        /// <param name="bMask">是否屏蔽敏感字段为*</param>
        /// <returns>Json 数据在customer里</returns>
        [AllowAnonymous]
        public IActionResult Json_CustomerInfoByKey(string key, bool bMask = false, bool bNameMask = false)
        {
            return TryCatchFunc((c) =>
             {
                 key = Ass.P.PStr(key).Replace("\n", "");
                 var cust = MainDbContext.vwCHIS_Code_Customer.Where(m => m.CustomerMobile == key || m.Email == key || m.IDcard == key).ToList().Select(m => new
                 {
                     CustomerName = bNameMask ? m.CustomerName.ToMarkString(Ass.Models.MaskType.UserName) : m.CustomerName,
                     CustomerId = m.CustomerID,
                     Gender = m.Gender,
                     GenderName = m.Gender?.ToGenderString(),
                     MobileNumber = bMask ? m.CustomerMobile.ToMarkString(Ass.Models.MaskType.MobileCode) : m.CustomerMobile,
                     Email = bMask ? m.Email.ToMarkString(Ass.Models.MaskType.EmailCode) : m.Email,
                     CustomerImage = m.PhotoUrlDef,
                     Birthday = m.Birthday,
                     Age = m.Birthday?.ToAgeString(),
                     Optime = m.OpTime,
                     IDCardNumber = bMask ? m.IDcard.ToMarkString(Ass.Models.MaskType.IDCode) : m.IDcard
                 }).FirstOrDefault();
                 if (cust == null) throw new Exception("不存在用户!");
                 c.customer = cust;
                 return null;
             });

        }

        //快速注册
        [AllowAnonymous]
        public IActionResult Json_CustomerQuickRegist(string account, string customerName, int gender, DateTime birthday, string vcode = null)
        {
            return TryCatchFunc((dd) =>
            {

                if (account.GetStringType().IsEmail || account.GetStringType().IsMobile)
                {
                    if (vcode.IsEmpty()) throw new Exception("手机或者邮箱注册必须输入验证码");
                    CheckRegVCode(account, vcode);//验证信息
                }


                var cus = new CHIS_Code_Customer
                {
                    CustomerName = customerName,
                    Gender = gender,
                    Birthday = birthday,
                    IsVIP=false,
                    sysLatestActiveTime=DateTime.Now,
                    OpTime=DateTime.Now,
                    CustomerCreateDate = DateTime.Now //创建时间
                };

                if (account.GetStringType().IsMobile)
                {
                    var count = MainDbContext.CHIS_Sys_Login.Where(m => m.Mobile == account).Count() + MainDbContext.CHIS_Code_Customer.Where(m => m.CustomerMobile == account).Count();
                    if (count > 0) throw new Exception("该手机号已经注册了");
                    cus.CustomerMobile = account;
                    var add0 = MainDbContext.Add(cus).Entity;
                    MainDbContext.SaveChanges();
                    //增加注册用户
                    var login = MainDbContext.CHIS_Sys_Login.FirstOrDefault(m => m.Mobile == account);
                    if (login == null)
                    {
                        MainDbContext.Add(new CHIS_Sys_Login
                        {
                            Mobile = account,
                            CustomerId = add0.CustomerID,
                            IsLock = false
                        });
                        MainDbContext.SaveChanges();
                        cus = add0;
                    }
                }

                else if (account.GetStringType().IsEmail)
                {
                    var count = MainDbContext.CHIS_Sys_Login.Where(m => m.Email == account).Count() + MainDbContext.CHIS_Code_Customer.Where(m => m.Email == account).Count();
                    if (count > 0) throw new Exception("该邮件地址已经注册了");
                    cus.Email = account;
                    var add1 = MainDbContext.Add(cus).Entity;
                    MainDbContext.SaveChanges();
                    //增加注册用户
                    var login = MainDbContext.CHIS_Sys_Login.FirstOrDefault(m => m.Email == account);
                    if (login == null)
                    {
                        MainDbContext.Add(new CHIS_Sys_Login
                        {
                            Email = account,
                            CustomerId = add1.CustomerID,
                            IsLock = false
                        });
                        MainDbContext.SaveChanges();
                        cus = add1;
                    }
                }
                else if (account.GetStringType().IsIdCardNumber)
                {
                    var count = MainDbContext.CHIS_Sys_Login.Where(m => m.IdCardNumber == account).Count() + MainDbContext.CHIS_Code_Customer.Where(m => m.IDcard == account).Count();
                    if (count > 0) throw new Exception("该身份证已经注册了");
                    cus.IDcard = account;
                    var add2 = MainDbContext.Add(cus).Entity;
                    MainDbContext.SaveChanges();
                    //增加注册用户
                    var login = MainDbContext.CHIS_Sys_Login.FirstOrDefault(m => m.IdCardNumber == account);
                    if (login == null)
                    {
                        MainDbContext.Add(new CHIS_Sys_Login
                        {
                            IdCardNumber = account,
                            CustomerId = add2.CustomerID,
                            IsLock = false
                        });
                        MainDbContext.SaveChanges();
                        cus = add2;
                    }
                }
                else throw new Exception("输入错误账号");
                dd.customerId = cus.CustomerID;
                return null;
            }, bUseTrans: true);

        }

        /// <summary>
        /// 微信注册 新用户
        /// </summary>
        public bool WXRegistCustomer(ah.Models.ViewModel.WechatBindingModel model)
        {
            var mobile = model.mobile;
            if (mobile.IsEmpty()) throw new Exception("手机号码为空");
            if (model.openid.IsEmpty()) throw new Exception("微信openid为空");
            if (!model.Gender.HasValue) throw new Exception("没有选择性别");
            if (model.CustomerName.IsEmpty()) throw new Exception("没有输入用户名");
            if (!model.Birthday.HasValue) throw new Exception("没有传入出生日期");
            if (!(mobile.GetStringType().IsMobile)) throw new Exception("输入的账号非手机号码");

            using (var tx = MainDbContext.Database.BeginTransaction())
            {
                try
                {
                    var cus = new CHIS_Code_Customer
                    {
                        NickName = model.NickName,
                        CustomerName = model.CustomerName,
                        Gender = model.Gender,
                        Birthday = model.Birthday,
                        WXPic = model.WxPicUrl,
                        WXOpenId = model.openid,
                        CustomerMobile = mobile,
                        IsVIP=false,
                        CustomerCreateDate=DateTime.Now,
                        OpTime=DateTime.Now,
                        sysLatestActiveTime=DateTime.Now
                    };

                    var count = MainDbContext.CHIS_Sys_Login.Where(m => m.Mobile == mobile).Count() + MainDbContext.CHIS_Code_Customer.Where(m => m.CustomerMobile == mobile).Count();
                    if (count > 0)
                    {
                        //强制清空手机账号
                        var find0 = MainDbContext.CHIS_Code_Customer.Where(m => m.CustomerMobile == mobile);
                        foreach (var fd0 in find0) fd0.CustomerMobile = null;
                        var find1 = MainDbContext.CHIS_Sys_Login.Where(m => m.Mobile == mobile);
                        foreach (var fd1 in find1) { fd1.Mobile = null; fd1.MobileAuthenticatedTime = null; fd1.MobileIsAuthenticated = false; }
                        MainDbContext.SaveChanges();
                    }

                    var add0 = MainDbContext.Add(cus).Entity;
                    MainDbContext.SaveChanges();
                    //增加注册用户
                    var login = MainDbContext.CHIS_Sys_Login.FirstOrDefault(m => m.Mobile == mobile);
                    if (login == null)
                    {
                        MainDbContext.Add(new CHIS_Sys_Login
                        {
                            Mobile = mobile,
                            CustomerId = add0.CustomerID,
                            IsLock = false
                        });
                        MainDbContext.SaveChanges();
                        cus = add0;
                    }

                    tx.Commit();
                }
                catch (Exception ex) { tx.Rollback(); throw ex; }
                return true;


            }




        }


        /// <summary>
        /// 微信绑定 旧用户
        /// </summary>
        public bool WXBindingCustomer(ah.Models.ViewModel.WechatBindingModel model)
        {
            if (!model.CustomerId.HasValue) throw new Exception("没有选择绑定的用户");

            foreach (var item in MainDbContext.CHIS_Code_Customer.Where(m => m.WXOpenId == model.openid)) item.WXOpenId = null;
            MainDbContext.SaveChanges();

            var cus = MainDbContext.CHIS_Code_Customer.Find(model.CustomerId);
            cus.WXOpenId = model.openid;
            cus.WXPic = model.WxPicUrl;
            MainDbContext.SaveChanges();
            return true;
        }


    }
}