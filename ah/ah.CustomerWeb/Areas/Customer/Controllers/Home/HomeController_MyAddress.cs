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

        public IActionResult ManageAddress()
        {
            ViewBag.AddressList = MainDbContext.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == GetCurrentLoginUser.CustomerId).ToList().OrderByDescending(m => m.IsDefault);
            return View(nameof(ManageAddress), new CHIS_Code_Customer_AddressInfos());
        }

        public IActionResult GetViewManageAddressList()
        {
            var model = MainDbContext.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == GetCurrentLoginUser.CustomerId).ToList();
            return PartialView("_ManageAddress_AddressList", model.OrderByDescending(m => m.IsDefault));
        }

        public IActionResult UpsertMyAddress(CHIS_Code_Customer_AddressInfos model, bool isAjax = false)
        {
            var cusId = GetCurrentLoginUser.CustomerId;
            if (cusId == 0) throw new Exception("用户Id不正确");

            if (model.IsDefault)
            {
                MainDbContext.Database.ExecuteSqlCommand($"Update CHIS_Code_Customer_AddressInfos set IsDefault=0 where CustomerId={GetCurrentLoginUser.CustomerId}");
            }

            //新增
            if (model.AddressId == 0)
            {

                model.CustomerId = GetCurrentLoginUser.CustomerId;

                //没有地址则直接将传入地址设置成默认地址
                var address = MainDbContext.CHIS_Code_Customer_AddressInfos.FirstOrDefault(x => x.CustomerId == GetCurrentLoginUser.CustomerId);
                if(address == null)
                {
                    model.IsDefault = true;
                }

                MainDbContext.CHIS_Code_Customer_AddressInfos.Add(model);
                MainDbContext.SaveChanges();
            }
            else //修改
            {
                model.CustomerId = GetCurrentLoginUser.CustomerId;
                MainDbContext.CHIS_Code_Customer_AddressInfos.Update(model);
                MainDbContext.SaveChanges();
            }

            if (isAjax)
            {
                return Ok(new { rlt = "success" });
            }

            return RedirectToAction(nameof(ManageAddress));
        }

        public IActionResult DeleteMyAddress(int addressId)
        {
            var find = MainDbContext.CHIS_Code_Customer_AddressInfos.Find(addressId);
            if (find != null) MainDbContext.Remove(find);
            MainDbContext.SaveChanges();
            return ManageAddress();
        }

    }
}
