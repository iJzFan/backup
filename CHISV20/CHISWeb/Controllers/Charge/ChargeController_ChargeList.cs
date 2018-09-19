
using Ass;
using CHIS.Models;
using CHIS.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers.Charge
{

    public partial class ChargeController : BaseController
    {
        public IActionResult MyChargeList()
        {
            return View(nameof(MyChargeList));
        }

        //基本明细
        public IActionResult LoadMyChargeList(string searchText, string TimeRange = "Today", int? stationId=null,int? opCustomerId=null, string customerName = null, int pageIndex = 1, int pageSize = 25)
        {
            var dt = DateTime.Now;
            DateTime? dt0 = null; DateTime? dt1 = null;
            base.initialData_Page(ref pageIndex, ref pageSize);
            base.initialData_TimeRange(ref dt0, ref dt1, 1, timeRange: TimeRange);

            stationId = stationId ?? UserSelf.StationId;
            opCustomerId = opCustomerId ?? UserSelf.CustomerId;                             
            var finds = db.vwCHIS_Charge_Pay.AsNoTracking().Where(m =>m.StationId==stationId && (m.sysOpId== opCustomerId||m.sysOpId==0));
            if (searchText.IsNotEmpty())
            {
                if (searchText.IsMobileNumber()) finds = finds.Where(m => m.CustomerMobile == searchText);
                else finds = finds.Where(m => m.PayOrderId == searchText);
            }
            else
            {
                finds = finds.Where(m => m.PayedTime > dt0 && m.PayedTime < dt1);
            }
            if (customerName.IsNotEmpty())
            {
                finds = finds.Where(m => m.CustomerName.StartsWith(customerName));
            }
            var count = finds.Count();
            var items = finds.OrderByDescending(m => m.PayId).Skip(pageSize * (pageIndex - 1)).Take(pageSize);

            var model = new Ass.Mvc.PageListInfo<vwCHIS_Charge_Pay>
            {
                DataList = items.ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = count
            };

            _setDebugText(dt); 
            return PartialView("_pvChargeList", model);
        }

        //详细明细
        public IActionResult ChargeListDetail(long payId)
        {
            var model = new PayDetail
            {
                Pay = db.vwCHIS_Charge_Pay.Find(payId),
                Formeds = db.vwCHIS_Charge_Pay_Detail_Formed.AsNoTracking().Where(m => m.PayId == payId),
                Herbs = db.vwCHIS_Charge_Pay_Detail_Herb.AsNoTracking().Where(m => m.PayId == payId),
                Extras = db.vwCHIS_Charge_Pay_Detail_ExtraFee.AsNoTracking().Where(m => m.PayId == payId)
            };
            return PartialView("_pvChargeListDetail", model);

        }




    }
}
