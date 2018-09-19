using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using Ass;
using CHIS.Controllers;
using CHIS;

namespace CHIS.Api
{
    public partial class Common : BaseDBController
    {
        public Common(DbContext.CHISEntitiesSqlServer db) : base(db) { }

        public int LoadBaseCalendarDat(int year, int month)
        {
            return 1;

        }
        public IEnumerable<Ass.Models.BaseCalendarData> LoadBaseCalendarData(int year, int month)
        {
            List<Ass.Models.BaseCalendarData> rtn = new List<Ass.Models.BaseCalendarData>();
            DateTime firstDay = new DateTime(year, month, 1);
            for (int i = 0; i < (int)firstDay.DayOfWeek; i++)
            {
                rtn.Add(null);
            }
            DateTime endDay = firstDay.AddMonths(1);
            for (DateTime dt = firstDay; dt < endDay; dt = dt.AddDays(1))
            {
                var LunlarDate = new Yi.ChineseCalendar(dt);
                rtn.Add(new Ass.Models.BaseCalendarData()
                {
                    Date = dt,
                    LunlarString = LunlarDate.ChineseDayString,
                    TermString = LunlarDate.ChineseTwentyFourDay?.TermString
                });
            }
            return rtn;
        }

        public IEnumerable<CHIS.Models.ViewModel.CalendarData> LoadCalendarData(int year, int month, int customerId)
        {
            string ym = string.Format("{0:0000}{1:00}", year, month);
            var finds = _db.vwCHIS_Register.AsNoTracking();
            finds = from item in finds where item.RegisterDate.HasValue && item.RegisterDate.Value.ToString("yyyyMM") == ym select item;
            var lst = finds.ToList();

            var cdt = LoadBaseCalendarData(year, month);
            var rtn = from item in cdt
                      select item == null ? null : new CHIS.Models.ViewModel.CalendarData
                      {
                          Date = item.Date,
                          LunlarString = item.LunlarString,
                          TermString = item.TermString,
                          RegisterItems = lst.Where(m => m.RegisterDate.Value.Date == item.Date).Select(m => new CHIS.Models.ViewModel.CustomerRegisterItem(m))
                      };

            return rtn;
        }

        public IEnumerable<dynamic> LoadRegisterOfDate(DateTime dt)
        {
            var finds = _db.vwCHIS_Register.AsNoTracking().Where(m => m.RegisterDate.HasValue && m.RegisterDate.Value.Date == dt.Date).ToList().OrderBy(m => m.RegisterSlot).Select(m => new
            {
                stationName = m.StationName,
                departmentName = m.DepartmentName,
                doctorName = m.DoctorName,
                registerDate = m.RegisterDate.Value.ToString("yyyy-MM-dd"),
                SlotName = CHIS.Code.PrjHelper.TransSlot(m.RegisterSlot),
                TreatStatusCode = CHIS.Models.ViewModel.CustomerRegisterItem._setStatus(m),
                TreatStatus = CHIS.Code.PrjHelper.TransTreatStatus(CHIS.Models.ViewModel.CustomerRegisterItem._setStatus(m)),
                RegisterFromName = m.RegisterFromName
            });
            return finds;
        }



        #region 获取人员信息

        /// <summary>
        /// 获取医生信息
        /// </summary>
        /// <param name="searchtext">使用电话或者身份证搜索</param>
        /// <returns></returns>
        public IEnumerable<CHIS.Models.vwCHIS_Code_Doctor> GetDoctors(string searchtext, int id = 0)
        {
            if (id > 0) return _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => m.DoctorId == id);
            if (string.IsNullOrEmpty(searchtext)) return null;
            return _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => m.Telephone == searchtext || m.CustomerMobile == searchtext || m.IDcard == searchtext || m.DoctorName == searchtext).ToList<vwCHIS_Code_Doctor>();
        }


        /// <summary>
        /// 获取医生的用户
        /// </summary>
        /// <param name="searchtext">使用电话或者身份证搜索或者电子邮箱</param>
        /// <returns></returns>
        public IEnumerable<CHIS.Models.vwCHIS_Code_Customer> GetCustomers(string searchtext, int id = 0)
        {
            if (id > 0) return _db.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.CustomerID == id);
            if (string.IsNullOrEmpty(searchtext)) return null;
            searchtext = searchtext.Trim();
            var rlt = _db.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.Telephone == searchtext || m.CustomerMobile == searchtext || m.IDcard == searchtext || m.CustomerName == searchtext ||
             m.Email == searchtext);
            return rlt;
        }
        public async Task<IActionResult> IsDoctor(int customerId)
        {
            return await TryCatchFuncAsync(async (d) =>
            {
                var doc = await _db.CHIS_Code_Doctor.AsNoTracking().FirstOrDefaultAsync(m => m.CustomerId == customerId);
                d.isDoctor = doc != null;
                d.doctorId = (doc==null?0:doc.DoctorId);
                return null;
            });
        }
        /// <summary>
        /// 获取医生的用户
        /// </summary>
        /// <param name="searchtext">使用电话或者身份证搜索或者电子邮箱</param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetCustomersMasked(string searchtext, int id = 0)
        {
            if (id > 0) return _db.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.CustomerID == id);
            if (string.IsNullOrEmpty(searchtext)) return null;
            searchtext = searchtext.Trim();
            return _db.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.Telephone == searchtext || m.CustomerMobile == searchtext || m.IDcard == searchtext || m.CustomerName == searchtext ||
            m.Email == searchtext).ToList().Select(m => new
            {
                CustomerId = m.CustomerID,
                CustomerName = m.CustomerName,
                Age = m.Age,
                Gender = m.Gender?.ToGenderString(),
                Telephone = m.Telephone.ToMarkString(Ass.Models.MaskType.MobileCode),
                CustomerMobile = m.CustomerMobile.ToMarkString(Ass.Models.MaskType.MobileCode),
                Email = m.Email.ToMarkString(Ass.Models.MaskType.EmailCode),
                IDcard = m.IDcard.ToMarkString(Ass.Models.MaskType.IDCode)
            });
        }

        /// <summary>
        /// 重新设置客户的信息
        /// </summary>
        /// <param name="mobileOrEmail">手机或Email</param>
        /// <param name="customerId">如果传入了CustomerId，则过滤其他的账号</param>
        /// <param name="vcode">验证码</param>
        /// <returns></returns>
        public IActionResult ResetOtherCustomerInfo(string mobileOrEmailOrIdCard, string type, int? customerId = 0, string vcode = "")
        {
            return TryCatchFunc(() =>
            {
                if (string.IsNullOrWhiteSpace(mobileOrEmailOrIdCard)) throw new Exception("没有传入数据");
                var s = mobileOrEmailOrIdCard.Trim();
                if (type == "mobile")
                {
                    var finds = _db.CHIS_Code_Customer.Where(m => (m.CustomerMobile == s || m.Telephone == s) && (customerId > 0 ? m.CustomerID != customerId : true));
                    foreach (var item in finds) { item.CustomerMobile = null; item.Telephone = null; }
                    _db.SaveChanges();
                    var finds2 = _db.CHIS_Sys_Login.Where(m => m.Mobile == s && (customerId > 0 ? m.CustomerId != customerId : true));
                    foreach (var item in finds2) { item.Mobile = null; item.MobileAuthenticatedTime = null; item.MobileIsAuthenticated = null; }
                    _db.SaveChanges();
                }
                if (type == "email")
                {
                    var finds = _db.CHIS_Code_Customer.Where(m => (m.Email.ToLower() == s.ToLower()) && (customerId > 0 ? m.CustomerID != customerId : true));
                    foreach (var item in finds) { item.Email = null; }
                    _db.SaveChanges();
                    var finds2 = _db.CHIS_Sys_Login.Where(m => m.Email.ToLower() == s.ToLower() && (customerId > 0 ? m.CustomerId != customerId : true));
                    foreach (var item in finds2) { item.Email = null; item.EmailIsAuthenticated = null; item.EmailAuthenticatedTime = null; }
                    _db.SaveChanges();
                }
                if (type == "idcard")
                {
                    var finds = _db.CHIS_Code_Customer.Where(m => (m.IDcard.ToLower() == s.ToLower()) && (customerId > 0 ? m.CustomerID != customerId : true));
                    foreach (var item in finds) { item.IDcard = null; item.IDCardAImg = null; item.IDCardBImg = null; }
                    _db.SaveChanges();
                    var finds2 = _db.CHIS_Sys_Login.Where(m => m.IdCardNumber.ToLower() == s.ToLower() && (customerId > 0 ? m.CustomerId != customerId : true));
                    foreach (var item in finds2) { item.IdCardNumber = null; item.IdCardNumberIsAuthenticated = null; item.IdCardNumberAuthenticatedTime = null; }
                    _db.SaveChanges();
                }
                return null;
            }, bUseTrans: true);
        }

        /// <summary>
        /// 清理指定用户的关键数据
        /// </summary>
        /// <param name="customerId">用户Id</param>
        /// <param name="txt">手机、Email，身份证号</param>
        /// <param name="type">email/mobile/idcard</param>
        /// <returns></returns>
        public IActionResult ClearCustomerInfo(int customerId, string txt, string type)
        {

            return TryCatchFunc(() =>
            {
                var s = txt.Trim();
                var find = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == customerId);
                if (type == "mobile")
                {
                    find.CustomerMobile = null; find.Telephone = null;
                    _db.SaveChanges();
                    var finds2 = _db.CHIS_Sys_Login.Where(m => m.Mobile == s && (customerId > 0 ? m.CustomerId != customerId : true));
                    foreach (var item in finds2) { item.Mobile = null; item.MobileAuthenticatedTime = null; item.MobileIsAuthenticated = null; }
                    _db.SaveChanges();
                }
                if (type == "email")
                {
                    find.Email = null;
                    _db.SaveChanges();
                    var finds2 = _db.CHIS_Sys_Login.Where(m => m.Email.ToLower() == s.ToLower() && (customerId > 0 ? m.CustomerId != customerId : true));
                    foreach (var item in finds2) { item.Email = null; item.EmailIsAuthenticated = null; item.EmailAuthenticatedTime = null; }
                    _db.SaveChanges();
                }
                if (type == "idcard")
                {
                    find.IDcard = null; find.IDCardAImg = null; find.IDCardBImg = null;
                    _db.SaveChanges();
                    var finds2 = _db.CHIS_Sys_Login.Where(m => m.IdCardNumber.ToLower() == s.ToLower() && (customerId > 0 ? m.CustomerId != customerId : true));
                    foreach (var item in finds2) { item.IdCardNumber = null; item.IdCardNumberIsAuthenticated = null; item.IdCardNumberAuthenticatedTime = null; }
                    _db.SaveChanges();
                }
                return null;
            }, bUseTrans: true);
        }


        /// <summary>
        /// 获取不是医生的用户信息
        /// </summary>
        /// <param name="searchtext">使用电话或者身份证搜索</param>
        /// <returns></returns>
        public IEnumerable<CHIS.Models.vwCHIS_Code_Customer> GetCustomersNoDoctor(string searchtext, int id = 0)
        {
            // 1.首先去vwCHIS_Code_Customer查询到findList;
            // 2.判断doctor中是否存在该用户ID的，如果存在
            //   2.1 在findLsit中把该ID去除掉；
            // 3.2如果不存在，则在findList中查询；

            var findList = _db.vwCHIS_Code_Customer.Where(m => m.CustomerName != null).ToList();
            var d = _db.vwCHIS_Code_Doctor.Where(m => m.CustomerId == id);
            if (d.Count() > 0)
            {
                findList.Remove(findList.Where(o => o.CustomerID == id).Single());
            }
            else { return findList; }
            //if (id > 0) return MainDbContext.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.CustomerID == id);
            if (string.IsNullOrEmpty(searchtext)) return null;
            var data = from item in findList where (item.Telephone == searchtext || item.IDcard == searchtext || item.CustomerName == searchtext) select item;
            return data;
        }


        #endregion



    }
}