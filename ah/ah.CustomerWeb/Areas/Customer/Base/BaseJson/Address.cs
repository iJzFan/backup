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

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{

    public class Address:BaseController
    {
        public Address(ah.DbContext.AHMSEntitiesSqlServer db) : base(db) { }

    /// <summary>
    /// 修改和添加用户地址
    /// </summary>
    /// <param name="addressId"></param>
    /// <param name="areaId"></param>
    /// <param name="addrDetail"></param>
    /// <param name="custName"></param>
    /// <param name="mob"></param>
    /// <param name="isDefault"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    [AutoValidateAntiforgeryToken]
        public IActionResult AddCustAddr(long? addressId, int? areaId, string addrDetail, string custName, string mob, bool isDefault)
        {
            var user = GetCurrentLoginUser;
            try
            {
                //大于0的进行修改操作，否则为新增操作
                if (addressId > 0)
                {
                    var aa = MainDbContext.CHIS_Code_Customer_AddressInfos.FirstOrDefault(m => m.AddressId == addressId);
                    if (aa == null) throw new Exception("不存在该信息");
                    if (isDefault) new CustomerCLB(this).ClearCustomerAddressInfoAsNotDefault(user.CustomerId);
                    aa.AddressDetail = addrDetail;
                    aa.AreaId = areaId;
                    aa.ContactName = custName;
                    aa.Mobile = mob;
                    aa.IsDefault = isDefault;
                    if (!aa.IsLegalAddress) throw new Exception("请填写正确地址");
                    MainDbContext.CHIS_Code_Customer_AddressInfos.Update(aa);
                    MainDbContext.SaveChanges();
                    return Json(new
                    {
                        rlt = true,
                        msg = "修改成功"
                    });
                }

                //一个用户添加十条信息，

                var AddrList = MainDbContext.CHIS_Code_Customer_AddressInfos.Where(m => m.CustomerId == user.CustomerId).ToList();
                var countAddr = AddrList.Count();
                if (countAddr > 10) throw new Exception("只能添加十条地址信息，请修改其他地址！");
                var entity = new CHIS_Code_Customer_AddressInfos();
                var custId = user.CustomerId;
                if (custId < 0) throw new Exception("用户信息错误，不能提交");
                if (isDefault) new CustomerCLB(this).ClearCustomerAddressInfoAsNotDefault(custId);
                entity.CustomerId = custId;
                entity.AddressDetail = addrDetail;
                entity.AreaId = areaId;
                entity.ContactName = custName;
                entity.Mobile = mob;
                entity.IsDefault = isDefault;
                if (!entity.IsLegalAddress) throw new Exception("请填写正确地址");
                MainDbContext.CHIS_Code_Customer_AddressInfos.Add(entity);
                MainDbContext.SaveChanges();

                return Json(new
                {
                    rlt = true,
                    msg = "添加成功"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    rlt = false,
                    msg = ex.Message
                });
            };
        }
        /// <summary>
        /// 显示地址列表
        /// </summary>
        /// <returns></returns>
        public IActionResult ShowAddrList()
        {
            var user = GetCurrentLoginUser;
            var custList = MainDbContext.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == user.CustomerId)
            .Select(m => new
            {
                AddressId = m.AddressId,
                AreaId = m.AreaId,
                ContactName = m.ContactName,
                FullAddress = m.FullAddress,
                ZipCode = m.ZipCode,
                IsDefault = m.IsDefault,
                Mobile = m.Mobile,
                AddressDetail = m.AddressDetail
            }).Take(10).OrderBy(m => m.AddressId).ToList();
            var total = custList.Count();
            return Json(new
            {
                rlt = true,
                msg = "",
                totals = total,
                items = custList
            });
        }
        //通过地址ID查询单条记录
        public IActionResult QueryAddrSingleRecord(long addrId)
        {
            try
            {
                var addesEntity = MainDbContext.CHIS_Code_Customer_AddressInfos.FirstOrDefault(m => m.AddressId == addrId);
                if (addesEntity == null) throw new Exception("不存在的地址！");
                return Json(new
                {
                    rlt = true,
                    msg = "",
                    item = addesEntity
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
        //删除地址
        public IActionResult DeleteAddress(long addressId)
        {
            try
            {
                var addressEntity = MainDbContext.CHIS_Code_Customer_AddressInfos.FirstOrDefault(m => m.AddressId == addressId);
                MainDbContext.CHIS_Code_Customer_AddressInfos.Remove(addressEntity);
                MainDbContext.SaveChanges();
                return Json(new
                {
                    rlt = true,
                    msg = ""
                });
            }
            catch (Exception ex)
            {
                return Json(new { rlt = false, msg = ex.Message });
            }
        }
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="custId"></param>
        /// <returns></returns>
        public IActionResult SetDefaultAddress(long addressId)
        {
            try
            {
                var user = GetCurrentLoginUser;
                new CustomerCLB(this).ClearCustomerAddressInfoAsNotDefault(user.CustomerId);
                var addressInfor = MainDbContext.CHIS_Code_Customer_AddressInfos.Where(m => m.AddressId == addressId).FirstOrDefault();
                if (addressInfor == null) throw new Exception("不存在该地址，不能进行修改");
                addressInfor.IsDefault = true;
                MainDbContext.SaveChanges();
                return Json(new { rlt = true, msg = "设置成功" });

            }
            catch (Exception ex)
            {
                return Json(new { rlt = false, msg = ex.Message });
            }
        }



        

    }
}
