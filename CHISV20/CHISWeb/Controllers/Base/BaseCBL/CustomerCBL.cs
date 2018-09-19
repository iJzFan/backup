using Ass;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CHIS.Models;

namespace CHIS.Controllers
{
    public class CustomerCBL : BaseCBL
    {
        public CustomerCBL(BaseController c) : base(c) { }




        #region  根据条件搜索用户

        /// <summary>
        /// 根据各种搜索情况搜索注册用户
        /// 
        /// </summary>
        /// <param name = "searchtype" > 搜索类别 </ param >
        /// < param name="searchtxt">搜索文本</param>
        /// <param name = "certTypeId" > 证件类别（选择）</param>        
        public IQueryable<Models.vwCHIS_Code_Customer> GetCustomersBy(int searchtype, string searchtxt, int? certTypeId = null)
        {
            IQueryable<Models.vwCHIS_Code_Customer> find = null;          
            find = _db.vwCHIS_Code_Customer.AsNoTracking();
            switch (searchtype)
            {
                case 1:   //就诊卡
                    return find.Where(m => m.VisitCard == searchtxt);
                case 2:   //身份证 或有效证件
                    if (certTypeId != null && certTypeId != MPS.IDCardId)
                        return find.Where(m => m.CertificateNo == searchtxt && m.CertificateTypeId == certTypeId);
                    else
                        return find.Where(m => m.IDcard == searchtxt);
                case 3:   // 手机
                    return find.Where(m => m.CustomerMobile == searchtxt);
                case 4:   //工作证号
                    return find.Where(m => m.WorkCode == searchtxt);
                case 5:   //患者标识
                    return find.Where(m => m.CustomerNo == searchtxt);
                case 6://Email
                    return find.Where(m => m.Email == searchtxt);
                case 7://用户名
                    return find.Where(m => m.CustomerName == searchtxt);
                case 8://登录名
                    return find.Where(m => m.LoginName == searchtxt);
                case 98:  //就诊Id
                    var cusid = _db.CHIS_DoctorTreat.AsNoTracking().Where(m => m.TreatId.ToString() == searchtxt).First()?.CustomerId ?? 0;
                    return find.Where(m => m.CustomerID == cusid);
                case 99:  //用户Id   
                    var custid = Ass.P.PInt(searchtxt);
                    return find.Where(m => m.CustomerID == custid);
                default:  //按用户姓名 电话 身份证 就诊卡等综合搜索
                    return find.Where(m => m.VisitCard == searchtxt ||
                    m.IDcard == searchtxt ||
                    m.CustomerMobile == searchtxt ||
                    m.WorkCode == searchtxt ||
                    m.CustomerNo.ToString() == searchtxt ||
                    m.CustomerName.Contains(searchtxt)).AsNoTracking();
            }
        }

        public IQueryable<Models.vwCHIS_Code_Customer> GetCustomersBy(string searchText, int pageIndex = 1, int pageSize = 20)
        {
            var s = searchText.GetStringType();
            if (s.IsEmail) return GetCustomersBy(6, s.String);
            if (s.IsMobile) return GetCustomersBy(3, s.String);
            if (s.IsIdCardNumber) return GetCustomersBy(2, s.String);
            if (s.IsLoginNameLegal) return GetCustomersBy(8, s.String);
            return GetCustomersBy(7, searchText).OrderBy(m => m.CustomerID).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }


        public IQueryable<Models.vwCHIS_Code_Customer> GetCustomersBy(int? cusid)
        {
            if (cusid == null || cusid == 0) return null;
            return _db.vwCHIS_Code_Customer.Where(m => m.CustomerID == cusid);
        }



        /// <summary>
        /// 根据用户的身份证件，或者手机，或者手机+姓名 定位到唯一用户
        /// </summary>
        /// <returns></returns>
        public Models.vwCHIS_Code_Customer GetOneCustomerVW(string idcard, string name, string mobile)
        {

            Models.vwCHIS_Code_Customer rtn = null;
            if (idcard.IsNotEmpty())
            {
                var finds = _db.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.IDcard == idcard);
                int findsNum = finds.Count();
                if (findsNum == 1) return finds.FirstOrDefault();
                if (findsNum > 1) throw new Exception($"该身份证[{idcard}]对应了多个用户，系统不允许");
            }
            if (mobile.IsNotEmpty())
            {
                var finds = _db.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.Telephone == mobile);
                int findsNum = finds.Count();
                if (findsNum == 1) return finds.FirstOrDefault();
                if (findsNum > 1 && string.IsNullOrEmpty(name)) throw new Exception($"手机号码[{mobile}]有多用户对应，但没有输入用户名");

                if (findsNum > 1)
                {
                    var finds1 = _db.vwCHIS_Code_Customer.AsNoTracking().Where(m => m.Telephone == mobile && m.CustomerName == name);
                    int findsNum1 = finds1.Count();
                    if (findsNum1 == 1) return finds1.FirstOrDefault();
                    if (findsNum1 > 1) throw new Exception($"用户{name}[{mobile}]注册了多个了，请联系后台管理员，调整您的用户信息");
                }
            }
            return rtn;
        }
        public Models.CHIS_Code_Customer GetOneCustomer(string idcard, string name, string mobile)
        {
            Models.CHIS_Code_Customer rtn = null;

            if (idcard.IsNotEmpty())
            {
                var finds = _db.CHIS_Code_Customer.Where(m => m.IDcard == idcard).AsNoTracking();
                int findsNum = finds.Count();
                if (findsNum == 1) return finds.FirstOrDefault();
                if (findsNum > 1) throw new Exception($"该身份证[{idcard}]对应了多个用户，系统不允许");
            }
            if (mobile.IsNotEmpty())
            {
                var finds = _db.CHIS_Code_Customer.Where(m => m.Telephone == mobile).AsNoTracking();
                int findsNum = finds.Count();
                if (findsNum == 1) return finds.FirstOrDefault();
                if (findsNum > 1 && string.IsNullOrEmpty(name)) throw new Exception($"手机号码[{mobile}]有多用户对应，但没有输入用户名");

                if (findsNum > 1)
                {
                    var finds1 = _db.CHIS_Code_Customer.Where(m => m.Telephone == mobile && m.CustomerName == name).AsNoTracking();
                    int findsNum1 = finds1.Count();
                    if (findsNum1 == 1) return finds1.FirstOrDefault();
                    if (findsNum1 > 1) throw new Exception($"用户{name}[{mobile}]注册了多个了，请联系后台管理员，调整您的用户信息");
                }
            }
            return rtn;
        }

        /// <summary>
        /// 根据身份证/电话+姓名 返回用户集合
        /// </summary>
        /// <param name="idcard">身份证/证件</param>
        /// <param name="cusName">姓名</param>
        /// <param name="mobile">电话</param>
        /// <returns>List集合</returns>
        public List<Models.vwCHIS_Code_Customer> SearchCustomers(string idcard, string cusName, string mobile)
        {
            List<Models.vwCHIS_Code_Customer> rtn = new List<Models.vwCHIS_Code_Customer>();
            //身份证 级别高
            if (idcard.IsNotEmpty())
            {
                var finds = _db.vwCHIS_Code_Customer.Where(m => m.IDcard == idcard).AsNoTracking();
                int findsNum = finds.Count();
                if (findsNum == 1) { rtn.Add(finds.FirstOrDefault()); return rtn; };
                if (findsNum > 1) { rtn.AddRange(finds); return rtn; }
            }
            //电话+姓名
            if (mobile.IsNotEmpty())
            {
                var finds = _db.vwCHIS_Code_Customer.Where(m => m.Telephone == mobile).AsNoTracking();
                int findsNum = finds.Count();
                if (findsNum == 1) { rtn.Add(finds.FirstOrDefault()); return rtn; }
                if (findsNum > 1 && !string.IsNullOrEmpty(cusName))
                {
                    finds = finds.Where(m => m.CustomerName == cusName);
                    rtn.AddRange(finds); return rtn;
                }
                else
                {
                    rtn.AddRange(finds); return rtn;
                }
            }
            return rtn;
        }


        /// <summary>
        /// 强制删除用户所有的数据 包括会员，医生，权限等一系列数据
        /// </summary>
        /// <param name="recId"></param>
        internal void ForceDelete(int customerId)
        {
            _db.Database.ExecuteSqlCommand($"sp_Code_Customer_ForceDelete {customerId}");
        }


        public async Task<bool> UpdateCustomerTrans(CHIS.Models.CHIS_Code_Customer customer, Models.CHIS_Code_Customer_HealthInfo health, DbContext.CHISEntitiesSqlServer db = null)
        {

            db = db ?? _db;
            db.BeginTransaction();
            try
            {
                var aCust = db.CHIS_Code_Customer.AsNoTracking().First(m => m.CustomerID == customer.CustomerID);
                //添加患者需要更新的属性
                aCust.Address = customer.Address;
                aCust.AddressAreaId = customer.AddressAreaId;
                aCust.Birthday = customer.Birthday;
                aCust.CertificateNo = customer.CertificateNo;
                aCust.CertificateTypeId = customer.CertificateTypeId;
                aCust.ContactMan = customer.ContactMan;
                aCust.ContactPhone = customer.ContactPhone;
                aCust.CustomerName = customer.CustomerName;
                aCust.EduLevel = customer.EduLevel;
                aCust.Email = customer.Email;
                aCust.CustomerPic = customer.CustomerPic;
                aCust.NamePY = Ass.Data.Chinese2Spell.GetFstAndFullLettersLower(customer.CustomerName); //搜索码

                //aCust.Explain = customer.Explain;
                //aCust.FeeType = customer.FeeType;
                aCust.IDcard = customer.IDcard;
                aCust.InsuranceNo = customer.InsuranceNo;

                //aCust.LeftAmount = customer.LeftAmount;

                aCust.Marriage = customer.Marriage;
                //aCust.Nation = customer.Nation;

                aCust.Presfession = customer.Presfession;
                //aCust.PriceType = customer.PriceType;
                //aCust.Relation = customer.Relation;
                aCust.Remark = customer.Remark;
                //aCust.SaveAmount = customer.SaveAmount;
                aCust.Gender = customer.Gender;
                //aCust.Special = customer.Special;                              
                //aCust.Status = customer.Status;

                aCust.LoginName = customer.LoginName;
                aCust.Telephone = customer.Telephone;
                aCust.CustomerMobile = customer.CustomerMobile;
                // aCust.UserPWD = customer.UserPWD;
                aCust.IsVIP = customer.IsVIP == true;
                aCust.VIPcode = customer.VIPcode;
                //aCust.VisitCard = customer.VisitCard;
                //aCust.WorkCode = customer.WorkCode;
                aCust.ContactMan = customer.ContactMan;
                aCust.ContactPhone = customer.ContactPhone;
                aCust.WorkUnit = customer.WorkUnit;
                aCust.WXCode = customer.WXCode;
                //aCust.ZipCode = customer.ZipCode;
                //附加信息
                // aCust.sysSource = "WEB";
                // aCust.StationID = user.StationId;
                aCust.OpID = controller.UserSelf.OpId;
                aCust.OpMan = controller.UserSelf.OpMan;
                aCust.sysLatestActiveTime = aCust.OpTime = DateTime.Now;


                var hm = db.CHIS_Code_Customer_HealthInfo.AsNoTracking().First(m => m.CustomerId == customer.CustomerID);
                //添加患者健康基本属性
                hm.Allergic = health.Allergic;//过敏史
                hm.BirthChildrenNum = health.BirthChildrenNum;//生孩子数量
                hm.BloodType = health.BloodType;//血型
                hm.Height = health.Height;//身高
                hm.MenstruationEndOldYear = health.MenstruationEndOldYear;//月经初潮时期
                hm.MenstruationStartOldYear = health.MenstruationStartOldYear;//绝经日期
                hm.PastMedicalHistory = health.PastMedicalHistory;//既往史
                hm.PregnancyNum = health.PregnancyNum;//怀孕次数
                hm.Weight = health.Weight;//体重

                db.CHIS_Code_Customer.Update(aCust);
                db.CHIS_Code_Customer_HealthInfo.Update(hm);
                await db.SaveChangesAsync();


                //如果 关键登陆信息改了，则登陆信息也改变
                //手机和邮箱 //身份证不进行调整
                var login = db.CHIS_Sys_Login.AsNoTracking().FirstOrDefault(m => m.CustomerId == customer.CustomerID);
                if (login == null) await db.CHIS_Sys_Login.AddAsync(new Models.CHIS_Sys_Login
                {
                    CustomerId = customer.CustomerID,
                    Mobile = customer.CustomerMobile,
                    IdCardNumber = customer.IDcard,
                    Email = customer.Email,
                    IsLock = false,
                    LoginPassword = "123456"
                });
                else
                {
                    if (login.Mobile != customer.CustomerMobile && customer.CustomerMobile.GetStringType().IsMobile)
                        login.Mobile = customer.CustomerMobile;
                    if (login.Email != customer.Email && customer.Email.GetStringType().IsEmail)
                        login.Email = customer.Email;
                    if (login.IdCardNumber.IsEmpty() && customer.IDcard.GetStringType().IsIdCardNumber)
                        login.IdCardNumber = customer.IDcard;
                    db.CHIS_Sys_Login.Update(login);
                }
                await db.SaveChangesAsync();

                db.CommitTran();

                return true;
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                if (ex.InnerException != null) ex = ex.InnerException;
                throw ex;
            }

        }

       


        #endregion




        /// <summary>
        /// 登录名是否没有注册
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public object LoginNameAllowedRegisted(string loginName, int? customerId)
        {
            var str = Ass.P.PStr(loginName).ToLower();
            if (str.IsEmpty()) return true;
            if (!str.GetStringType().IsLoginNameLegal) return "传入的登录名非法，非小写字符或数字和点构成";

            if (customerId > 0)
            {
                var cus = _db.CHIS_Code_Customer.Find(customerId.Value);
                if (str.Equals(cus.LoginName, StringComparison.CurrentCultureIgnoreCase)) return true;

            }

            var num = _db.CHIS_Code_Customer.Where(m => string.Equals(m.LoginName, str, StringComparison.CurrentCultureIgnoreCase)).Count();
            if (num > 0) return "已经使用了";
            else return true;
        }
        /// <summary>
        /// 邮箱是否没有注册
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool EmailAllowedRegisted(string email, int? customerId)
        {
            var str = Ass.P.PStr(email);
            if (str.IsEmpty()) return true;
            if (!str.GetStringType().IsEmail) return false;

            if (customerId > 0)
            {
                var cus = _db.CHIS_Code_Customer.Find(customerId.Value);
                if (str.Equals(cus.Email, StringComparison.CurrentCultureIgnoreCase)) return true;
            }
            return _db.CHIS_Code_Customer.Where(m => string.Equals(m.Email, str, StringComparison.CurrentCultureIgnoreCase)).Count() == 0;
        }

        /// <summary>
        /// 手机是否没有注册
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool MobileAllowedRegisted(string mobile, int? customerId)
        {
            var str = Ass.P.PStr(mobile);
            if (str.IsEmpty()) return true;
            if (!str.GetStringType().IsMobile) return false;

            if (customerId > 0)
            {
                var cus = _db.CHIS_Code_Customer.Find(customerId.Value);
                if (str.Equals(cus.CustomerMobile, StringComparison.CurrentCultureIgnoreCase)) return true;
            }
            return _db.CHIS_Code_Customer.Where(m => string.Equals(m.CustomerMobile, str, StringComparison.CurrentCultureIgnoreCase)).Count() == 0;
        }

        /// <summary>
        /// 身份证是否没有注册
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IdCardAllowedRegisted(string idcard, int? customerId)
        {
            var str = Ass.P.PStr(idcard);
            if (str.IsEmpty()) return true;
            if (!str.GetStringType().IsIdCardNumber) return false;

            if (customerId > 0)
            {
                var cus = _db.CHIS_Code_Customer.Find(customerId.Value);
                if (str.Equals(cus.IDcard, StringComparison.CurrentCultureIgnoreCase)) return true;
            }
            return _db.CHIS_Code_Customer.Where(m => string.Equals(m.IDcard, str, StringComparison.CurrentCultureIgnoreCase)).Count() == 0;
        }

    }
}

