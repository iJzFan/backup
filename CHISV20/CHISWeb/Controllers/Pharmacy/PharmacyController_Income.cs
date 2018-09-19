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
    public partial class PharmacyController : BaseController
    {

        #region 入库


        #region 入库流水明细
        //入库
        public IActionResult MyPharmacy_Income(string searchText, DateTime? dt0, DateTime? dt1)
        {
            ViewBag.SearchText = searchText;
            ViewBag.SearchDate0 = dt0;
            ViewBag.SearchDate1 = dt1;
            ViewBag.FuncId = 1118;
            return View();
        }
        //入库列表
        public IActionResult LoadIncomeLists(string searchText, string timeRange = "Today", int pageIndex = 1, int pageSize = 20)
        {
            DateTime dt = DateTime.Now;
            DateTime? dt0 = null, dt1 = null;
            initialData_TimeRange(ref dt0, ref dt1, timeRange);
            base.initialData_Page(ref pageIndex, ref pageSize);
            //默认条件和搜索
            var finds = _pharSvr.QueryIncomeList(searchText, dt0.Value, dt1.Value, UserSelf.DrugStoreStationId);
            //获取总条数和分页数据
            var count = finds.Count();
            var item = finds.OrderByDescending(m => m.StockInId).Skip(pageSize * (pageIndex - 1)).Take(pageSize);

            base._setDebugText(dt);
            return PartialView("_pvIncomeDetails", new Ass.Mvc.PageListInfo<vwCHIS_DurgStock_Income>
            {
                DataList = item,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = count
            });
        }

        //删除入库项目  假删
        public IActionResult DeleteIncomeItem(long stockInId)
        {
            return TryCatchFuncW(async () =>
            {
                await _pharSvr.DeleteIncomeItem(stockInId);
                return null;
            }, true);
        }


        //导出入库记录
        public IActionResult ExportIncomeList(string searchText, DateTime dt0, DateTime dt1)
        {
            var finds = _pharSvr.QueryIncomeList(searchText, dt0, dt1, UserSelf.DrugStoreStationId);
            var file = new CHIS.Code.Utility.FileUtils().ExportExcel(finds);
            var fs = System.IO.File.OpenRead(file);
            return File(fs, "application/text", $"{DateTime.Now.ToString("我的药房入库记录yyMMddHHmmss")}.xlsx");
        }

        #endregion




        #region 入库操作

        //药品入库
        public IActionResult DurgIncome()
        {
            ViewBag.FuncId = 1120;
            if (UserSelf.IsSelfDrugStore)
                return View("MyPharmacy_DurgIncome");
            else return View("NoAccess");//没有权限
        }


        //药品基本库搜索入口
        public IActionResult SelectIncomeDrug(int? drugId)
        {
            ViewBag.drugId = drugId;
            return View("_selectIncomeDrug");
        }

        //查找我的药品
        public async Task<IActionResult> GetMyStockDrugInfos(string searchText, List<int> unitIds = null, int pageIndex = 1, int pageSize = 20)
        {
            DateTime dt = DateTime.Now;
            base.initialData_Page(ref pageIndex, ref pageSize);
            var bll = new BllCaller.DrugsBllCaller();
            var finds = bll.GetMyStockDrugInfos(searchText, unitIds);
            var count = finds.Count();
            var items = finds.OrderByDescending(m=>m.DrugCompleteScore).ThenBy(m => m.DrugId).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
            var prices = await bll.GetMyLastIncomePriceAsync(items, UserSelf.DrugStoreStationId);
            List<Exp_vwCHIS_Code_Drug_Main> expItems = new List<Exp_vwCHIS_Code_Drug_Main>();
            foreach (var item in items)
            {
                var mmBg = prices.FirstOrDefault(m => m.DrugId == item.DrugId && m.InUnitId == item.UnitBigId);
                var mmSm = prices.FirstOrDefault(m => m.DrugId == item.DrugId && m.InUnitId == item.UnitSmallId);
                var temp = item.CopyPropTo<Exp_vwCHIS_Code_Drug_Main>();
                temp.ExpMyLastIncomeBigPrice = mmBg?.IncomePrice;//最后入库的大价格
                temp.ExpMyLastIncomeSmallPrice = mmSm?.IncomePrice;//最后入库的小价格 
                expItems.Add(temp);
            }

            base._setDebugText(dt);
            ViewBag.term = searchText;
            return PartialView("_pvSelectDrugItem", new Ass.Mvc.PageListInfo<Exp_vwCHIS_Code_Drug_Main>
            {
                DataList = expItems,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = count
            });
        }


        //保存我的药品
        public async Task<IActionResult> SaveIncome(IEnumerable<Models.CHIS_DurgStock_Income> items)
        {
            return await TryCatchFuncAsync(async (dd) =>
            {
                var dt0 = DateTime.Now.AddSeconds(-15);
                await _pharSvr.SaveIncome(items, UserSelf.OpId, UserSelf.OpManFullMsg, UserSelf.DrugStoreStationId);
                var dt1 = DateTime.Now.AddSeconds(15);
                dd.CallBackUrl = $"/Pharmacy/MyPharmacy_Income?TimeRange=dt0={dt0.ToStdString()};dt1={dt1.ToStdString()}";
                return null;
            });
        }

        public IActionResult UpdateExcelFile()
        {
            return View("_UpdateFile");
        }

        //批量导入Excel
        [HttpPost]
        public async Task<IActionResult> ImportIncomeExcel(IFormFile file)
        {
            return await TryCatchFuncAsync(async (dd) =>
            {
                var finds = _pharSvr.ImportIncomeExcel(file);
                //配置价格
                var bll = new BllCaller.DrugsBllCaller();
                var prices = await bll.GetMyLastIncomePriceAsync(finds.Select(m => m.DrugId).ToList(), UserSelf.DrugStoreStationId);
                foreach (var item in finds)
                {
                    var mmBg = prices.FirstOrDefault(m => m.DrugId == item.DrugId && m.InUnitId == item.outUnitBigId);
                    var mmSm = prices.FirstOrDefault(m => m.DrugId == item.DrugId && m.InUnitId == item.outUnitSmallId);
                    var temp = item.CopyPropTo<Exp_vwCHIS_Code_Drug_Main>();
                    item.incomePriceBig = mmBg?.IncomePrice;//最后入库的大价格
                    item.incomePriceSmall = mmSm?.IncomePrice;//最后入库的小价格                                
                }
                dd.items = finds;
                return null;
            });
        }

        #endregion



        #endregion


        #region 申请新药

        public IActionResult MyDrugMenu()
        {
            ViewBag.FuncId = 1121;
            return View();
        }

        //我的申请药品清单
        //status [APPLYING/ALLOWED/REJECT]
        public IActionResult MyDrugMenuList(string searchText, string status = "", string timeRange = "Today", int pageIndex = 1, int pageSize = 20)
        {
            DateTime? dt0 = null, dt1 = null;
            initialData_TimeRange(ref dt0, ref dt1, timeRange);
            var finds = _pharSvr.QueryMyDrugMenuList(searchText, dt0.Value, dt1.Value, status, UserSelf.DrugStoreStationId);
            initialData_Page(ref pageIndex, ref pageSize);
            var count = finds.Count();
            var items = finds.OrderBy(m => m.DrugId).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return PartialView("_pvMyDrugMenuDetails", new Ass.Mvc.PageListInfo<vwCHIS_Code_Drug_Main_Apply>
            {
                DataList = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = count
            });
        }





        #endregion


        #region 库存总览部分
        //库存
        public IActionResult MyPharmacy_StockMonitor()
        {
            ViewBag.FuncId = 1119;
            return View();
        }
        public IActionResult MyPharmacy_StockMonitor2()
        {
            return View();
        }

        //库存监听列表
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchText"></param>   
        /// <param name="stockNumStatus">库存数量状态 0忽略，1超过警戒库存，2正常库存</param> 
        public IActionResult LoadStockMonitorLists(string searchText, int drugStockTypeId = 0, int stockNumStatus = 0, string stockPriceStatus="" , int pageIndex = 1, int pageSize = 25)
        {
            DateTime dt = DateTime.Now;
            var finds = _pharSvr.QueryExportStockMonitorList(searchText, drugStockTypeId, stockNumStatus,stockPriceStatus, UserSelf.DrugStoreStationId);
            initialData_Page(ref pageIndex, ref pageSize);
            var count = finds.Count();
            var items = finds.OrderBy(m => m.DrugId).Skip(pageSize * (pageIndex - 1)).Take(pageSize);

            base._setDebugText(dt);
            return PartialView("_pvStockMonitorDetails", new Ass.Mvc.PageListInfo<vwCHIS_DrugStock_Monitor>
            {
                DataList = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = count
            });
        }

        //导出监控的库存Excel数据
        public IActionResult ExportStockMonitorList(string searchText, int drugStockTypeId = 0, int stockNumStatus = 0, string stockPriceStatus="")
        {
            var finds = _pharSvr.QueryExportStockMonitorList(searchText, drugStockTypeId, stockNumStatus,stockPriceStatus, UserSelf.DrugStoreStationId);
            var file = new CHIS.Code.Utility.FileUtils().ExportExcel(finds);
            var fs = System.IO.File.OpenRead(file);
            return File(fs, "application/text", $"{DateTime.Now.ToString("我的药房库存yyMMddHHmmss")}.xlsx");
        }

        //编辑药品库存
        public IActionResult EditStockMonitorDetail(string drugStockMonitorId, decimal price, int num, int safeNum)
        {
            var model = _pharSvr.Find(drugStockMonitorId);
            //var model = new vwCHIS_DrugStock_Monitor
            //{
            //    DrugStockMonitorId = drugStockMonitorId,
            //    StockSalePrice = price,
            //    DrugStockNum = num,
            //    StockLineNum = safeNum
            //};
            return PartialView("_pvStockMonitorDetailEdit", model);
        }
        //删除监控药品
        public IActionResult DeleteDrugStockMonitorById(string drugStockMonitorId)
        {
            return TryCatchFuncW(async () =>
            {
                await _pharSvr.DeleteDrugStockMonitorById(drugStockMonitorId, UserSelf.DrugStoreStationId);
                return null;
            });

        }


        //重新设置药品库存监控数据
        public IActionResult ChangeDrugStockData(string drugStockMonitorId, decimal newPrice, int num, int safeNum, string rmk)
        {
            return TryCatchFunc(() =>
            {
                //获取权限           
                var rlt = _accSvr.GetFuncConfig(MyConfigNames.MyPharmacy_StockMonitor_MYSTOCK_IsSetPrice);                
                if (!rlt.ToBool()) throw new Exception("该权限没有被允许");
                _pharSvr.ChangeDrugStockData(drugStockMonitorId, newPrice, num, safeNum, rmk, UserSelf.OpMan);
                return null;
            });

        }



        #endregion



    }
}
