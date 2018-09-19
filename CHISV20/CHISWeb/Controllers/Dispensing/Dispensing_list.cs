using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass.Models;
using Ass;
using System.Collections.Generic;
using CHIS.Models.ViewModel;
using CHIS.Models;
using CHIS.Models.DataModel;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    public partial class DispensingController : BaseController
    {

        public IActionResult DrugOutList()
        {
            return View(GetViewPath(nameof(DrugOutList)));
        }
        public IActionResult LoadDrugOutList(string searchText, string TimeRange = "Today", string customerName = null, string medialMainKindCode = null, int pageIndex = 1, int pageSize = 20)
        {
            DateTime? dt0 = null; DateTime? dt1 = null;
            base.initialData_TimeRange(ref dt0, ref dt1, 1, timeRange: TimeRange);

            //如果角色是药店护士
            int? registOpId = null;
            if (UserSelf.MyRoleNames.Contains("drugstore_nurse"))
            {
                registOpId = UserSelf.OpId;
            }


            var find = _db.vwCHIS_DrugStock_Out.AsNoTracking().Where(m => m.OutTime >= dt0 && m.OutTime < dt1 && m.StationId == UserSelf.StationId);

            if (registOpId.HasValue) find = find.Where(m => m.RegistOpId == registOpId);
            if (searchText.IsNotEmpty())
            {
                var drugids = base.SearchDrugIds(searchText);
                find = find.Where(m => drugids.Contains(m.DrugId));
            }
            if (customerName.IsNotEmpty())
            {
                find = find.Where(m => m.CustomerName.StartsWith(customerName));
            }
            if (medialMainKindCode.IsNotEmpty())
            {
                find = find.Where(m => m.MedialMainKindCode == medialMainKindCode);
            }

            //分页取数据
            var total = find.Count();
            find = find.OrderBy(m => m.OutTime).Skip((pageIndex - 1) * pageSize).Take(pageSize);

            //获取该出库条目的支付订单号
            var treatids = find.Select(m => m.TreatId.Value).ToList();
            var findpay = (from item in _db.CHIS_Charge_Pay_Detail_Formed.AsNoTracking()
                           where treatids.Contains(item.TreatId)
                           select new
                           {
                               item.PrescriptionNo,
                               item.PayId,
                               item.DoctorAdviceId,
                               Type = "FORMED",

                           }).Concat(
                            from item in _db.CHIS_Charge_Pay_Detail_Herb.AsNoTracking()
                            where treatids.Contains(item.TreatId)
                            select new
                            {
                                item.PrescriptionNo,
                                item.PayId,
                                item.DoctorAdviceId,
                                Type = "HERB"
                            }).Join(_db.CHIS_Charge_Pay.AsNoTracking(), a => a.PayId, g => g.PayId, (a, g) => new
                            {
                                a.PrescriptionNo,
                                a.PayId,
                                g.PayOrderId,
                                g.PayedTime,
                                a.DoctorAdviceId,
                                a.Type
                            }).ToList();

            var items = find.ToList();
            foreach (var item in items)
            {
                var mm = findpay.FirstOrDefault(m => (m.DoctorAdviceId == item.DoctorAdviceId && m.Type == item.DoctorAdviceType));
                item.PayOrderId = mm?.PayOrderId;
            }


            var model = new Ass.Mvc.PageListInfo<vwCHIS_DrugStock_Out>
            {
                DataList = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = total
            };
            return PartialView(GetViewPath(nameof(DrugOutList), "_pvDrugOutList"), model);
        }


        public IActionResult NetOrderList()
        {
            return View();
        }
        public async Task<IActionResult> LoadNetOrderList(string searchText, string TimeRange = "Today", string contactName = null, int supplierId = 3, int pageIndex = 1, int pageSize = 20)
        {
            DateTime? dt0 = null; DateTime? dt1 = null;
            base.initialData_TimeRange(ref dt0, ref dt1, 1, timeRange: TimeRange);
            //采用ado.net获取数据

            var ds = await new CHIS.DAL.Dispensing().LoadNetOrderList(searchText, dt0.Value, dt1.Value, contactName, supplierId, 1, UserSelf.StationId, pageIndex, pageSize);
            return PartialView("_pvNetOrderList2", new Ass.Mvc.PageListInfoDs(ds));

            var finda = _db.vwCHIS_Shipping_NetOrder.AsNoTracking().Where(m => m.SupplierId == 3);

            var find2 = _db.vwCHIS_Shipping_NetOrder.AsNoTracking().Where(m => m.SupplierId == 3).OrderByDescending(m => m.SendTime)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var model2 = new Ass.Mvc.PageListInfo<vwCHIS_Shipping_NetOrder>
            {
                DataList = find2,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = finda.Count()
            };
            return PartialView("_pvNetOrderList", model2);



            var find = _db.vwCHIS_Shipping_NetOrder.AsNoTracking().Where(m => m.SendTime >= dt0 && m.SendTime < dt1 && m.TotalAmount > 0 && m.SendedStatus == 1);

            //搜索本诊所内的订单            
            var station = base.UserMgr.GetAllowedStationsAndSubStationsQuery(_db, UserSelf.DoctorId, UserSelf.StationId).ToList();
            find = find.Where(m => station.Contains(m.StationId));

            if (searchText.IsNotEmpty())
            {
                if (searchText.GetStringType().IsMobile) find = find.Where(m => m.CustomerMobile == searchText);
                else find = find.Where(m => m.CustomerName == searchText || m.NetOrderNO == searchText);
            }
            if (contactName.IsNotEmpty())
            {
                if (contactName.GetStringType().IsMobile) find = find.Where(m => m.Mobile == contactName);
                else find = find.Where(m => m.ContactName.Contains(contactName));
            }

            var total = find.Count();
            var amountPrice = await find.SumAsync(m => m.TotalAmount);
            find = find.OrderByDescending(m => m.SendTime).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var model = new Ass.Mvc.PageListInfo<vwCHIS_Shipping_NetOrder>
            {
                DataList = find,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = total
            };
            base._setDebugText(string.Join(",", station));
            ViewBag.AmountPrice = amountPrice;
            return PartialView("_pvNetOrderList", model);
        }

    }
}