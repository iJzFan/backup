using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ah.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Authorization;
using ah.Models.ViewModel;
using System.Security.Claims;
using ah.Models.DataModel;
using ah.Models;
using ah.Areas.Customer.Controllers.Base;
using Microsoft.EntityFrameworkCore;
using Ass;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class HomeController : BaseController
    {
        [AllowAnonymous]
        public IActionResult CustomerRegistPhone()
        {
            return View(nameof(CustomerRegistPhone));
        }

        [AllowAnonymous]
        public IActionResult CustomerRegistEmail()
        {
            return View(nameof(CustomerRegistEmail));
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CustomerRegist(CustomerRegistViewModel model)
        {
            //注册处理
            using (var tx = MainDbContext.Database.BeginTransaction())
            {
                try
                {
                    if (!ModelState.IsValid) return View(nameof(CustomerRegist), model);


                    if (model.RegistRole != "customer") { throw new Exception("注册类型不是客户customer"); }
                    if (!string.IsNullOrEmpty(model.Email)) //邮箱注册
                    {
                        var vm = MainDbContext.CHIS_DataTemp_SendMailVCode.AsNoTracking().Where(m => m.EmailAddress == model.Email && (DateTime.Now - m.CreatTime.Value).TotalMinutes < 20).OrderByDescending(m => m.CreatTime).FirstOrDefault();
                        if (vm == null) throw new Exception("没有发现验证码信息");
                        if ((DateTime.Now - vm.CreatTime.Value).TotalSeconds > 220) throw new Exception("验证码超时");
                        if (vm.VCode != model.VCode) throw new Exception("验证码验证错误");
                        //通过了验证码后
                        if (MainDbContext.vwCHIS_Sys_Login.Any(m => m.Email == model.Email)) throw new Exception("该邮箱已经注册了");
                        var mAdd = MainDbContext.CHIS_Code_Customer.Add(new CHIS_Code_Customer
                        {
                            Email = model.Email,
                            CustomerName = string.IsNullOrWhiteSpace(model.Name) ? string.Format("u{0}", DateTime.Now.Ticks) : model.Name,
                            Gender = model.Gender,
                            Birthday = model.Birthday,
                            IsVIP=false,
                            sysSource = "邮箱快速注册",
                            OpTime = DateTime.Now,
                            sysLatestActiveTime = DateTime.Now,
                            CustomerCreateDate = DateTime.Now
                        }).Entity;
                        MainDbContext.SaveChanges();
                        //更新登录账户资料
                        var login = MainDbContext.CHIS_Sys_Login.FirstOrDefault(m => m.Email == model.Email);
                        if (login == null)
                        {
                            MainDbContext.CHIS_Sys_Login.Add(new CHIS_Sys_Login
                            {
                                CustomerId = mAdd.CustomerID,
                                Email = mAdd.Email,
                                EmailAuthenticatedTime = DateTime.Now,
                                EmailIsAuthenticated = true,
                                IsLock = false,
                                LoginPassword = model.RegPaswd
                            });
                            MainDbContext.SaveChanges();
                        }
                        else
                        {
                            login.CustomerId = mAdd.CustomerID;
                            login.EmailAuthenticatedTime = DateTime.Now;
                            login.EmailIsAuthenticated = true;
                            login.IsLock = false;
                            login.LoginPassword = model.RegPaswd;
                            MainDbContext.SaveChanges(); 
                        }
                    }
                    else if (!string.IsNullOrEmpty(model.Mobile)) //手机注册
                    {
                        var vm = MainDbContext.CHIS_DataTemp_SMS.AsNoTracking().Where(m => m.PhoneCode == model.Mobile && (DateTime.Now - m.CreatTime.Value).TotalMinutes < 20).OrderByDescending(m => m.CreatTime).FirstOrDefault();
                        if (vm == null) throw new Exception("没有发现验证码信息");
                        if ((DateTime.Now - vm.CreatTime.Value).TotalSeconds > 120) throw new Exception("验证码超时");
                        if (vm.VCode != model.VCode) throw new Exception("验证码验证错误");
                        //通过了验证码后
                        if (MainDbContext.vwCHIS_Sys_Login.Any(m => m.Mobile == model.Mobile)) throw new Exception("该手机已经注册了");
                        var mAdd = MainDbContext.CHIS_Code_Customer.Add(new CHIS_Code_Customer
                        { 
                            CustomerMobile = model.Mobile,
                            CustomerName = string.IsNullOrWhiteSpace(model.Name) ? string.Format("u{0}", DateTime.Now.Ticks) : model.Name,
                            Gender = model.Gender,
                            Birthday = model.Birthday,                            
                            IsVIP=false,
                            sysSource="手机快速注册",
                            OpTime=DateTime.Now,
                            sysLatestActiveTime=DateTime.Now,
                            CustomerCreateDate = DateTime.Now
                        }).Entity;
                        MainDbContext.SaveChanges();
                        //更新登录账户资料
                        var login = MainDbContext.CHIS_Sys_Login.FirstOrDefault(m => m.Mobile == model.Mobile);
                        if (login == null)
                        {
                            MainDbContext.CHIS_Sys_Login.Add(new CHIS_Sys_Login
                            {
                                CustomerId = mAdd.CustomerID,
                                Mobile = mAdd.CustomerMobile,
                                MobileAuthenticatedTime = DateTime.Now,
                                MobileIsAuthenticated = true,
                                LoginPassword = model.RegPaswd,
                                IsLock = false
                            });
                            MainDbContext.SaveChanges();
                        }
                        else
                        {
                            login.CustomerId = mAdd.CustomerID;
                            login.MobileAuthenticatedTime = DateTime.Now;
                            login.MobileIsAuthenticated = true;
                            login.IsLock = false;
                            login.LoginPassword = model.RegPaswd;
                            MainDbContext.SaveChanges();
                        }
                    }
                    else throw new Exception("注册没有填入手机/邮箱");
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    if (ex.InnerException != null) ex = ex.InnerException;
                    ModelState.AddModelError("", ex.Message);
                    if (!string.IsNullOrEmpty(model.Email)) //邮箱注册
                    {
                        return View(nameof(CustomerRegistEmail), model);
                    }
                    return View(nameof(CustomerRegistPhone), model);
                }
            }
            //注册成功则跳转到成功页面
            return RedirectToAction(nameof(CustomerRegistSuccess));
        }


        [AllowAnonymous]
        public IActionResult CustomerRegistSuccess()
        {
            return View();
        }

    }
}
