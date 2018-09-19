using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using Ass;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace CHIS.Controllers
{
    public partial class P3Tools : BaseController
    {
        public P3Tools(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        #region 健客价格开放接口
        [AllowAnonymous]
        public IActionResult PriceSetForJK()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult pvLoadJKDrugList(string searchText, int pageIndex = 1, int pageSize = 20)
        {
            searchText = Ass.P.PStr(searchText);
            var dt = DateTime.Now;
            var finds = _db.vwCHIS_DrugStock_Monitor.Where(m => m.SupplierId == MPS.SupplierId_JK && m.StationId == -1);
            if (searchText.IsNotEmpty())
            {
                var drug3id = 0;
                if (int.TryParse(searchText, out drug3id))
                {
                    finds = finds.Where(m => m.ThreePartDrugId == drug3id);
                }
                else
                {
                    finds = finds.Where(m => m.CodeDock.Contains(searchText));
                }
            }
            else finds = finds.Where(m => false);

            base.initialData_Page(ref pageIndex, ref pageSize);
            var list = finds.OrderBy(m => m.DrugId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var dt1 = DateTime.Now;
            var model = new Ass.Mvc.PageListInfo<vwCHIS_DrugStock_Monitor>
            {
                DataList = list,
                RecordTotal = finds.Count(),
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            base._setDebugText(dt); 
            return PartialView("_pvJKDrugList", model);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult setJKPrice(int drug3Id, decimal price)
        {
            return TryCatchFunc(() =>
            {
                _db.Database.ExecuteSqlCommand($"sp_UpdateP3DrugPrice 3,{drug3Id},{price}");
                Logger.WriteInfoAsync("P3Tools", "健客药品价格调整", $"药品{drug3Id}价格调整为{price}");
                return null;
            });
        }

        #endregion

    }
}
