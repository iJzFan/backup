using Ass;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Controllers
{
    public class LoginCBL : BaseCBL
    {
        public LoginCBL(BaseController c) : base(c) { }


        /// <summary>
        /// 更改登录的Email
        /// </summary>
        /// <param name="email">更改的Email</param>
        /// <param name="doctorId">医生的Id</param>
        /// <returns></returns>
        public async Task<bool> ChangeLoginEmailAsync(string email, int? doctorId = null)
        { 
            _db.BeginTransaction();
            try
            {
                if (!email.GetStringType().IsEmail) throw new Exception("传入了非Email");
                if (doctorId == null) doctorId = controller.UserSelf.DoctorId;
                var doctor = _db.vwCHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                //搜索所有email设置为空
                var finds = _db.CHIS_Code_Customer.Where(m => m.Email.ToLower() == email.ToLower());
                foreach (var item in finds) item.Email = null;
                await _db.SaveChangesAsync();

                //搜索所有登录的Email，设置为空和非验证通过
                var items = _db.CHIS_Sys_Login.Where(m => m.Email.ToLower() == email.ToLower());
                foreach (var item in items) { item.Email = null; item.EmailIsAuthenticated = null; item.EmailAuthenticatedTime = null; }
                await _db.SaveChangesAsync();

                //本账号Email设置为新的Email
                var cus = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == doctor.CustomerId);
                cus.Email = email;
                await _db.SaveChangesAsync();

                //本账号的Email设置为验证通过
                var login = _db.CHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == doctor.DoctorId);
                login.Email = email;
                login.EmailIsAuthenticated = true;
                login.EmailAuthenticatedTime = DateTime.Now;
                await _db.SaveChangesAsync();

                _db.CommitTran();
                return true;
            }
            catch (Exception ex)
            {
                _db.RollbackTran();
                throw ex;
            }

        }


        /// <summary>
        /// 更改登录的Mobile
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public async Task<bool> ChangeLoginMobileAsync(string mobile, int? doctorId = null)
        {
         
            _db.BeginTransaction();
            try
            {
                if (!mobile.GetStringType().IsMobile) throw new Exception("传入了非法电话号码");
                if (doctorId == null) doctorId = controller.UserSelf.DoctorId;
                var doctor = _db.vwCHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                //搜索所有mobile设置为空
                var finds = _db.CHIS_Code_Customer.Where(m => m.CustomerMobile.ToLower() == mobile.ToLower());
                foreach (var item in finds) item.CustomerMobile = null;
                await _db.SaveChangesAsync();

                //搜索所有登录的Email，设置为空和非验证通过
                var items = _db.CHIS_Sys_Login.Where(m => m.Mobile.ToLower() == mobile.ToLower());
                foreach (var item in items) { item.Mobile = null; item.MobileIsAuthenticated = null; item.MobileAuthenticatedTime = null; }
                await _db.SaveChangesAsync();

                //本账号Moible设置为新的手机
                var cus = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == doctor.CustomerId);
                cus.CustomerMobile = mobile;
                await _db.SaveChangesAsync();

                //本账号的Email设置为验证通过
                var login = _db.CHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == doctor.DoctorId);
                login.Mobile = mobile;
                login.MobileIsAuthenticated = true;
                login.MobileAuthenticatedTime = DateTime.Now;
              await  _db.SaveChangesAsync();

                _db.CommitTran();
                return true;
            }
            catch (Exception ex)
            {
                _db.RollbackTran();
                throw ex;
            }

        }

    }
}

