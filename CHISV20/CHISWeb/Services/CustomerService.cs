using CHIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ass;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using CHIS.Models.ViewModel;
using CHIS.DbContext;
using CHIS.Models.StatisticsModels;
using CHIS.Models.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Dynamic.Core;

namespace CHIS.Services
{
    public class CustomerService : BaseService
    {
        PointsDetailService _pointsService;
        public CustomerService(CHISEntitiesSqlServer db
            , PointsDetailService pointsService) : base(db)
        {
            _pointsService = pointsService;
        }

        #region 搜索患者会员
        public IQueryable<Models.vwCHIS_Code_Customer> GetCustomersBy(string searchText, int? visitStationId = null, int pageIndex = 1, int pageSize = 10)
        {
            var s = searchText.GetStringType();
            if (s.IsEmail) return GetCustomersBy(6, s.String, null, visitStationId, pageIndex, pageSize);
            if (s.IsMobile) return GetCustomersBy(3, s.String, null, visitStationId, pageIndex, pageSize);
            if (s.IsIdCardNumber) return GetCustomersBy(2, s.String, null, visitStationId, pageIndex, pageSize);
            if (s.IsLoginNameLegal) return GetCustomersBy(8, s.String, null, visitStationId, pageIndex, pageSize);
            return GetCustomersBy(7, searchText, null, visitStationId, pageIndex, pageSize);
        }


        /// <summary>
        /// 根据各种搜索情况搜索注册用户
        /// 
        /// </summary>
        /// <param name = "searchtype" > 搜索类别 </ param >
        /// < param name="searchtxt">搜索文本</param>
        /// <param name = "certTypeId" > 证件类别（选择）</param>        
        public IQueryable<Models.vwCHIS_Code_Customer> GetCustomersBy(int searchtype, string searchtxt, int? certTypeId = null, int? visitStationId = null, int pageIndex = 1, int pageSize = 10)
        {
            IQueryable<Models.vwCHIS_Code_Customer> find = null, rlt = null;
            find = _db.vwCHIS_Code_Customer.AsNoTracking();
            switch (searchtype)
            {
                case 1:   //就诊卡
                    rlt = find.Where(m => m.VisitCard == searchtxt); break;
                case 2:   //身份证 或有效证件
                    if (certTypeId != null && certTypeId != MPS.IDCardId)
                        rlt = find.Where(m => m.CertificateNo == searchtxt && m.CertificateTypeId == certTypeId);
                    else
                        rlt = find.Where(m => m.IDcard == searchtxt);
                    break;
                case 3:   // 手机
                    rlt = find.Where(m => m.CustomerMobile == searchtxt); break;
                case 4:   //工作证号
                    rlt = find.Where(m => m.WorkCode == searchtxt); break;
                case 5:   //患者标识
                    rlt = find.Where(m => m.CustomerNo == searchtxt); break;
                case 6://Email
                    rlt = find.Where(m => m.Email == searchtxt); break;
                case 7://用户名
                    rlt = find.Where(m => m.CustomerName == searchtxt); break;
                case 8://登录名
                    rlt = find.Where(m => m.LoginName == searchtxt); break;
                case 98:  //就诊Id
                    var cusid = _db.CHIS_DoctorTreat.AsNoTracking().Where(m => m.TreatId.ToString() == searchtxt).First()?.CustomerId ?? 0;
                    rlt = find.Where(m => m.CustomerID == cusid); break;
                case 99:  //用户Id   
                    var custid = Ass.P.PInt(searchtxt);
                    rlt = find.Where(m => m.CustomerID == custid); break;
                default:  //按用户姓名 电话 身份证 就诊卡等综合搜索
                    rlt = find.Where(m => m.VisitCard == searchtxt ||
                     m.IDcard == searchtxt ||
                     m.CustomerMobile == searchtxt ||
                     m.WorkCode == searchtxt ||
                     m.CustomerNo.ToString() == searchtxt ||
                     m.CustomerName.Contains(searchtxt)).AsNoTracking();
                    break;
            }
            //查找的结果>1个
            if (visitStationId > 0)
            {
                var findCount = find.Count();
                if (findCount > 1)
                {
                    //获取最近500天接诊的客户Id
                    //var treatedCustomerIds = _db.CHIS_DoctorTreat.Where(m => m.StationId == visitStationId && m.TreatTime >= DateTime.Now.AddDays(500)).OrderByDescending(m => m.TreatId).Select(m => m.DoctorId).ToList().Distinct();
                    //var tops = find.Select(m => m.CustomerID).Intersect(treatedCustomerIds);
                    //if (tops.Count() > pageSize * pageIndex)
                    //    return _db.vwCHIS_Code_Customer.Where(m => tops.Contains(m.CustomerID));
                    //var last = find.Select(m => m.CustomerID).Except(tops);
                }
            }
            return rlt;
        }




        public IQueryable<Models.vwCHIS_Code_Customer> GetCustomersBy(int? cusid)
        {
            if (cusid == null || cusid == 0) return null;
            return _db.vwCHIS_Code_Customer.Where(m => m.CustomerID == cusid);
        }
        /// <summary>
        /// 查找患者会员
        /// </summary> 
        public async Task<Models.vwCHIS_Code_Customer> GetCustomerById(int cusid)
        {
            if (cusid == 0) return null;
            return await _db.vwCHIS_Code_Customer.FindAsync(cusid);
        }


        public IEnumerable<CustomerAndRelations> GetCustomerAndRelations(string searchText, int? visitStationId = null, int pageIndex = 1, int pageSize = 10)
        {
            var findCustomers = GetCustomersBy(searchText, visitStationId, pageIndex, pageSize).ToList();
            var customerIds = findCustomers.Select(m => m.CustomerID);
            var rlt = _db.CHIS_Code_CustomerRelationship.AsNoTracking()
                .Join(_db.CHIS_Code_Dict_Detail, a => a.RelativeshipTypeId, g => g.DetailID,
                    (a, g) => new
                    {
                        BelongCustomerId = a.CustomerId,
                        RelationshipTypeName = g.ItemName,
                        RelationshipTypeId = g.DetailID,
                        RelationCustomerId = a.RelationCustomerId
                    })
                .Join(_db.vwCHIS_Code_Customer, a => a.RelationCustomerId, g => g.CustomerID,
                    (a, g) => new
                    {
                        BelongCustomerId = a.BelongCustomerId,
                        CustomerID = g.CustomerID,
                        CustomerName = g.CustomerName,
                        CustomerMobile = g.CustomerMobile,
                        RelationshipTypeName = a.RelationshipTypeName,
                        GenderName = (g.Gender ?? 2).ToGenderString(),
                        Age = (g.Birthday ?? DateTime.Today).ToAgeString(),
                        Birthday = g.Birthday ?? DateTime.Today
                    })
                .Where(m => customerIds.Contains(m.BelongCustomerId)).ToList();

            List<CustomerAndRelations> rtn = new List<CustomerAndRelations>();
            return from item in findCustomers
                   select new CustomerAndRelations
                   {
                       Customer = item,
                       MyRelationships = rlt.Where(m => m.BelongCustomerId == item.CustomerID)
                   };
        }


        #endregion

        public IQueryable<vwCHIS_Code_Customer_AddressInfos> GetMyAddressInfos(int customerId)
        {
            return _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == customerId);
        }

        /// <summary>
        /// 根据接诊获取地址列表
        /// </summary>
        /// <param name="treatId">接诊号</param>
        /// <param name="selectAddressId">选择的地址表</param>
        /// <returns></returns>
        public IQueryable<vwCHIS_Code_Customer_AddressInfos> GetMyAddressInfosByTreatId(long treatId, out long? selectAddressId)
        {
            long? defId = null;
            var customerId = _db.CHIS_DoctorTreat.AsNoTracking().Single(m => m.TreatId == treatId).CustomerId;
            var exfee = _db.vwCHIS_Doctor_ExtraFee.FirstOrDefault(m => m.TreatId == treatId && m.TreatFeeTypeId == (int)(ExtraFeeTypes.TransFee));
            if (exfee == null)
            {
                defId = exfee.MailAddressInfoId;
            }
            var rlt = _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == customerId);

            var lstdef = rlt.FirstOrDefault(m => m.IsDefault == true);
            if (lstdef == null) lstdef = rlt.FirstOrDefault();
            if (lstdef != null && defId == null) defId = lstdef?.AddressId;
            selectAddressId = defId;
            return rlt;
        }


        /// <summary>
        /// 快速注册用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<CHIS_Code_Customer> QuickRegist(QuickRegistCustomerInfo model, int opId, string opMan)
        {

            if (model.CustomerMobile.IsNotEmpty())
            {
                if (model.CustomerMobile.IsMobileNumber())
                {
                    if (_db.vwCHIS_Code_Customer.Where(m => m.CustomerMobile == model.CustomerMobile).Count() > 0)
                        throw new Exception("该手机已经使用，不能快速注册");
                    if (_db.CHIS_Sys_Login.Where(m => m.Mobile == model.CustomerMobile).Count() > 0)
                        throw new Exception("该手机已经使用，不能快速注册");
                }
            }

            var cus = new vwCHIS_Code_Customer { };
            cus.CustomerName = model.CustomerName;
            cus.Gender = model.Gender;
            cus.Birthday = model.Birthday;
            cus.LoginName = string.Format("{0}.{1}.{2}",
                Ass.Data.Chinese2Spell.GetFstLettersLower(model.CustomerName),
                model.Gender, model.Birthday.ToString("yyyyMMdd"));
            if (model.CustomerMobile.IsNotEmpty() && model.CustomerMobile.GetStringType().IsMobile)
                cus.CustomerMobile = model.CustomerMobile;
            cus.sysSource = sysSources.CHIS约号快增.ToString();
            cus.IsVIP = false;

            return await CreateCustomerAsync(cus, opId, opMan);

        }


        /// <summary>
        /// 快速注册一个用户
        /// </summary>
        /// <param name="cusName">用户名</param>
        /// <param name="cusMobile">用户手机号</param>
        /// <param name="cusIdCode">用户身份证号</param>
        /// <param name="opId">操作人Id</param>
        /// <param name="opMan">操作人</param>
        /// <returns></returns>
        public async Task<CHIS_Code_Customer> CreateCustomerAsync(string cusName, string cusMobile, string cusIdCode, sysSources sysSource, int opId, string opMan)
        {
            var cus = new vwCHIS_Code_Customer
            {
                CustomerName = cusName
            };
            if (cusMobile.GetStringType().IsMobile) cus.CustomerMobile = cusMobile;
            else cus.Telephone = cusMobile;

            if (cusIdCode.GetStringType().IsIdCardNumber) cus.IDcard = cusIdCode;
            else throw new Exception("身份证错误");

            //处理性别和生日
            var a = Ass.Data.Utils.GetIdCardInfo(cusIdCode);
            cus.Gender = a.Gender;
            cus.Birthday = a.Birthday;
            cus.sysSource = sysSource.ToString();
            cus.IsVIP = false;
            cus.LoginName = string.Format("{0}.{1}.{2}",
                Ass.Data.Chinese2Spell.GetFstLettersLower(cus.CustomerName),
                cus.Gender, cus.Birthday?.ToString("yyyyMMdd"));

            return await CreateCustomerAsync(cus, opId, opMan);
        }


        /// <summary>
        /// 添加一个用户
        /// </summary>
        /// <param name="model"></param>
        /// <param name="opId"></param>
        /// <param name="opMan"></param>
        /// <returns></returns>
        public async Task<CHIS_Code_Customer> CreateCustomerAsync(vwCHIS_Code_Customer model, int opId, string opMan)
        {
            var cus = new CHIS_Code_Customer { };
            cus.CustomerName = model.CustomerName;
            cus.Gender = model.Gender;
            cus.Birthday = model.Birthday;
            cus.LoginName = model.LoginName;
            cus.CustomerMobile = model.CustomerMobile;
            cus.IDcard = model.IDcard;
            cus.Email = model.Email;
            cus.sysSource = model.sysSource;
            cus.IsVIP = model.IsVIP == true;

            var health = new CHIS_Code_Customer_HealthInfo
            {
                CustomerId = cus.CustomerID,
                Allergic = model.Allergic,
                PastMedicalHistory = model.PastMedicalHistory
            };
            return await CreateCustomerAsync(cus, health, opId, opMan);
        }

        /// <summary>
        /// 添加一个用户
        /// </summary>
        /// <param name="cus"></param>
        /// <param name="health"></param>
        /// <returns></returns>
        public async Task<CHIS_Code_Customer> CreateCustomerAsync(CHIS_Code_Customer cus, CHIS_Code_Customer_HealthInfo health = null, int opId = 0, string opMan = "")
        {
            _db.BeginTransaction();
            try
            {
                cus.CustomerCreateDate = cus.sysLatestActiveTime = cus.OpTime = DateTime.Now;
                cus.IsVIP = cus.IsVIP == true;
                cus.NamePY = Ass.Data.Chinese2Spell.GetFstAndFullLettersLower(cus.CustomerName);
                cus.OpID = opId;
                cus.CustomerCreateDate = cus.OpTime = cus.sysLatestActiveTime = DateTime.Now;
                cus.OpMan = opMan;

                cus = _db.CHIS_Code_Customer.Add(cus).Entity;
                await _db.SaveChangesAsync();
                if (health == null) health = new CHIS_Code_Customer_HealthInfo();
                health.CustomerId = cus.CustomerID;
                await _db.CHIS_Code_Customer_HealthInfo.AddAsync(health);
                await _db.SaveChangesAsync();
                var login = _db.CHIS_Sys_Login.FirstOrDefault(m => m.CustomerId == cus.CustomerID);
                if (login == null)
                {
                    await _db.CHIS_Sys_Login.AddAsync(new CHIS_Sys_Login
                    {
                        CustomerId = cus.CustomerID,
                        Email = cus.Email,
                        Mobile = cus.CustomerMobile,
                        IdCardNumber = cus.IDcard,
                        LoginName = cus.LoginName,
                        LoginPassword = "123456",
                        IsLock = false
                    });
                    await _db.SaveChangesAsync();
                }
                _db.CommitTran();
                //注册用户赠送
                _pointsService.ChangePoints(new Models.InputModel.PointsDetailInputModel
                {
                    CustomerId = cus.CustomerID,
                    Description = "注册赠送1000分",
                    Points =  1000
                });
            }
            catch (Exception ex) { _db.RollbackTran(); throw ex; }
            return cus;
        }

        /// <summary>
        /// 获取用户的图片数据
        /// </summary>
        /// <param name="customerIds">用户组</param>
        /// <returns></returns>
        public IEnumerable<Models.ViewModel.CustomerPic> GetCustomersImage(List<int> customerIds)
        {
            var list = customerIds.Distinct();
            return _db.CHIS_Code_Customer.AsNoTracking().Where(m => customerIds.Contains(m.CustomerID)).Select(m => new CustomerPic
            {
                CustomerId = m.CustomerID,                
                PicUrl = (m.CustomerPic??m.WXPic).ahDtUtil().GetCustomerImg(m.Gender)
            }).ToList();
        }





    }
}
