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
        public IActionResult ChangePassword()
        {
            var user = GetCurrentLoginUser;
            var find = MainDbContext.vwCHIS_Sys_Login.AsNoTracking().FirstOrDefault(m => m.LoginId == user.LoginId);

            return View(nameof(ChangePassword), new ah.Models.CustomerBaseInfo
            {
                CustomerId = find.CustomerId.Value,
                LoginId = find.LoginId,
                CustomerName = find.CustomerName,
                Gender = find.Gender,
                Mobile = find.Mobile,
                Email = find.Email,
                IdCard = find.IdCardNumber,
                Password = find.LoginPassword
            });
        }

        [HttpPost]
        public IActionResult ChangePassword(CustomerBaseInfo model)
        {
            try
            {
                if (model.NewPwd.IsEmpty()) throw new Exception("没有输入新密码");
                if (model.NewPwd != model.NewPwdConfirm) throw new Exception("新密码与确认密码不相同");
                if (model.LoginId == 0) throw new Exception("没有发现登录Id");
                var find = MainDbContext.vwCHIS_Sys_Login.Find(model.LoginId);
                if (find.LoginPassword != model.OrigPwd) throw new Exception("输入的原密码错误");
                find.LoginPassword = model.NewPwd;//调整密码
                MainDbContext.SaveChanges();
                return View("ChangePassword_Success");
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); return ChangePassword(); }

        }

        /// <summary>
        /// 个人中心修改密码
        /// </summary>
        /// <returns></returns>
        /// 
        public IActionResult Json_EditPassWord(string loginName, string password, string rePassword)
        {
            try
            {
                var user = GetCurrentLoginUser;
                var updateEntity = MainDbContext.CHIS_Sys_Login.FirstOrDefault(m => m.LoginId == user.LoginId);
                if (password == null || rePassword == null) throw new Exception("密码不能为空");
                if (password.Trim() != rePassword.Trim()) throw new Exception("两次密码不匹配");
                updateEntity.LoginPassword = password;
                MainDbContext.SaveChanges();
                return Json(new
                {
                    rlt = true,
                    msg = "修改成功"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    rlt = false,
                    msg = ex.Message
                });
            }
        }

    }
}
