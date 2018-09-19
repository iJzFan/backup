using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ass;

namespace CHIS.Controllers
{
    public partial class HomeController : BaseController
    {
        [AllowAnonymous]
        public IActionResult Regist()
        {
            return View();
        }


        [AllowAnonymous]
        public IActionResult RegistProc(CHIS.Models.ViewModel.CustomerRegistViewModel model)
        {
            try
            {
                if (model.Mobile.IsEmpty() && model.Email.IsEmpty()) ModelState.AddModelError("", "手机号或者邮箱号不能为空");
                if (!ModelState.IsValid) { return View("Regist", model); }
                //注册操作

                //可能发现多个用户信息或者多个医生信息

                new DoctorCBL(this).RegistDoctorBasic(model);

                return View("Regist_Success");
            }
            catch (Exception ex)
            {
                ViewBag.BackLink = "/home/regist";
                ViewBag.BackLinkName = "返回注册";
                return View("Error", ex);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CustomerRegistDoctor(string regtype, string account)
        {
            return await TryCatchFuncAsync( async() =>
            {
                if (account.IsEmpty()) throw new Exception("传入账号错误");
                Models.CHIS_Code_Customer cus = null;
                if (regtype == "mobile")
                {
                    var cusid = _db.CHIS_Sys_Login.FirstOrDefault(m => m.Mobile == account).CustomerId;
                    cus = _db.CHIS_Code_Customer.Find(cusid);
                }
                else if (regtype == "email")
                {
                    var cusid = _db.CHIS_Sys_Login.FirstOrDefault(m => m.Email == account).CustomerId;
                    cus = _db.CHIS_Code_Customer.Find(cusid);
                }
                else throw new Exception("不支持的注册方式");
                if (cus == null) throw new Exception("没有发现客户信息");
                bool rlt = await new DoctorCBL(this).CustomerRegistDoctor(cus);
                return View("Regist_Success");
            }, bExceptionView: true, backLink: "/home/regist", backLinkName: "返回注册");

        }

        [AllowAnonymous]
        public IActionResult Regist_Success()
        {
            return View();
        }
    }
}
