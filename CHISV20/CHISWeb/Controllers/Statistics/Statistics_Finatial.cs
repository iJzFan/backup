using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using CHIS.Models.StatisticsModels;
using Ass;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{ 
    public partial class Statistics
    {

        /// <summary>
        /// 财务报表
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="stTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult FinanceStatement()
        {
            return View();
        }

        #region 载入明细

        public async Task<IActionResult> LoadFinaceStatement(string searchText, int? stationId, string timeRange = "Today", int pageIndex = 1, int pageSize = 20)
        {
            DateTime? sTime = null, eTime = null;
            base.initialData_TimeRange(ref sTime, ref eTime, timeRange: timeRange); //时间初始化
            if (!stationId.HasValue) stationId = UserSelf.StationId;

            var dal = new CHIS.DAL.Staticstics_Finatial();
            var ds = await dal.FinanceStatementAsync(searchText, stationId.Value, sTime.Value, eTime.Value, pageIndex, pageSize);
            var total = await dal.PayTotalAmountAsync(searchText, stationId.Value, sTime.Value, eTime.Value);

            ViewBag.TotalAmount = total;
            return PartialView("_pvFinanceStatementList", new Ass.Mvc.PageListInfoDs(ds));
        }
        #endregion 

        #region 图标统计之饼图

        /// <summary>
        /// 图表统计
        /// </summary>
        /// <param name="timeRange">非固定时间段可以用【dt0=2017-11-1;dt1=2017-12-3】 传入</param>
        /// <returns></returns>
        public IActionResult LoadFinaceChart_Type(int? stationId, string timeRange = "Today")
        {
            var model = GetFinaceChartType(stationId, timeRange);
            ViewBag.TotalAmount = model.Sum(m => m.Amount);
            return PartialView("_pvLoadFinaceChart_Type", model);
        }

        public IActionResult GetPieData_Finace(int? stationId, string timeRange = "Today")
        {
            return TryCatchFunc(dd =>
            {
                var model = GetFinaceChartType(stationId, timeRange);
                dd.pieData = model.Select(m => new { value = m.Amount, name = m.FeeProp });
                dd.seriesName = "费用类型";
                dd.legendData = model.Select(m => m.FeeProp);
                return null;
            });
        }

        private IEnumerable<ChargeType> GetFinaceChartType(int? stationId, string timeRange)
        {
            DateTime? sTime = null, eTime = null;
            base.initialData_TimeRange(ref sTime, ref eTime, timeRange: timeRange); //时间初始化       
            if (!stationId.HasValue) stationId = UserSelf.StationId;
            var model = new StatisticsCBL(this).ChargeTypeChartCBL(sTime.Value, eTime.Value, stationId.Value);
            return model;
        }

        #endregion

        #region 图标统计之线形图
        public IActionResult LoadFinaceChart_Gain(int? stationId, string timeRange = "Last3Days")
        {
            DateTime dt = DateTime.Now;
            DateTime? sTime = null, eTime = null;
            base.initialData_TimeRange(ref sTime, ref eTime, timeRange: timeRange); //时间初始化       
            if (!stationId.HasValue) stationId = UserSelf.StationId;
            var model = new StatisticsCBL(this).GainChartCBL(sTime.Value, eTime.Value, stationId.Value);
            List<TitleVal> partList = new List<TitleVal>()
                {
                     new TitleVal{Title="中药",Val=model.Sum(m => m.HerbVal) },
                     new TitleVal{Title="成药",Val=model.Sum(m => m.FormedVal) },
                     new TitleVal{Title="诊金",Val=model.Sum(m=>m.ConsultationVal) },
                     new TitleVal{Title="快递",Val=model.Sum(m=>m.ShippingVal) },
                     new TitleVal{Title="其他",Val=model.Sum(m=>m.OtherFeeVal) },
                };

            //ViewBag.Total = new TitleVal { Title = "总计", Val = model.Sum(m => m.TotalVal) };
            //ViewBag.PartList = partList;
            //return PartialView("_pvLoadFinaceChart_Gain", model);

            return TryCatchFunc(d =>
            {
                d.total = new TitleVal { Title = "总计", Val = model.Sum(m => m.TotalVal) };
                d.partList = partList;
                d.items = model;
                return null;
            });
        }

        #endregion


        public IActionResult FinanceStatementView()
        {
            return View();
        }



        #region 网上药店发药统计
        public IActionResult FinanceOfWebNet()
        {
            ViewBag.Suppliers = _db.CHIS_Code_Supplier.AsNoTracking().Where(m => m.SupplierID > 2);
            return View("FinanceOfWebNet");
        }

        public IActionResult LoadGainChartOfNetWebFinace(int? stationId, int? supplierId, string timeRange = "Last3Days")
        {
            DateTime dt = DateTime.Now;
            DateTime? sTime = null, eTime = null;
            base.initialData_TimeRange(ref sTime, ref eTime, timeRange: timeRange); //时间初始化       
            if (!stationId.HasValue) stationId = UserSelf.StationId;
            if (!supplierId.HasValue) supplierId = MPS.DefaultSupplierId;

            var model = new StatisticsCBL(this).GainOfWebNetSended(stationId.Value, supplierId.Value, sTime.Value, eTime.Value);
            List<TitleVal> partList = new List<TitleVal>()
                {
                     new TitleVal{Title="快递费",Val=model.Sum(m => m.TransFeeAmount) },
                     new TitleVal{Title="药品费",Val=model.Sum(m=>m.TotalAmount)-model.Sum(m=>m.TransFeeAmount) }
                };

            //ViewBag.Total = new TitleVal { Title = "总计", Val = model.Sum(m => m.TotalVal) };
            //ViewBag.PartList = partList;
            //return PartialView("_pvLoadFinaceChart_Gain", model);

            return TryCatchFunc(d =>
            {
                d.total = new TitleVal { Title = "总计", Val = model.Sum(m => m.TotalAmount) };
                d.partList = partList;
                d.items = model;
                return null;
            });
        }

        public async Task<IActionResult> LoadNetWebFinace(string searchText, int? stationId, int? supplierId, string timeRange = "Today", int pageIndex = 1, int pageSize = 20)
        {
            DateTime? sTime = null, eTime = null;
            base.initialData_TimeRange(ref sTime, ref eTime, timeRange: timeRange); //时间初始化
            if (!stationId.HasValue) stationId = UserSelf.StationId;
            if (!supplierId.HasValue) supplierId = MPS.DefaultSupplierId;

            var dal = new CHIS.DAL.Staticstics_Finatial();
            var ds = await dal.NetWebOfFinanceStatementAsync(searchText, stationId.Value, supplierId.Value, sTime.Value, eTime.Value, pageIndex, pageSize);
            var total = await dal.NetWebOfPayTotalAmountAsync(searchText, stationId.Value, supplierId.Value, sTime.Value, eTime.Value);
            ViewBag.TotalAmount = total;
            return PartialView("_pvNetWebFinanceStatementList ", new Ass.Mvc.PageListInfoDs(ds));
        }

        public async Task<IActionResult> ExportExcel_NetWebFinace(string searchText, int? stationId, int? supplierId, string timeRange = "Today")
        {
            DateTime? sTime = null, eTime = null;
            base.initialData_TimeRange(ref sTime, ref eTime, timeRange: timeRange); //时间初始化
            if (!stationId.HasValue) stationId = UserSelf.StationId;
            if (!supplierId.HasValue) supplierId = MPS.DefaultSupplierId;
            var dal = new CHIS.DAL.Staticstics_Finatial();
            var ds = await dal.NetWebOfFinanceStatementAsync(searchText, stationId.Value, supplierId.Value, sTime.Value, eTime.Value, 0, 0);


            var sName = _db.CHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == stationId).StationName;
            var sCoName = _db.CHIS_Code_Supplier.FirstOrDefault(m => m.SupplierID == supplierId).CompanyName;
            var sTimeStr = sTime?.ToString("yyyyMMdd");
            var eTimeStr = eTime?.AddDays(-1).ToString("yyyyMMdd");
            string fname = $"发药平台收款记录_{sName}_{sCoName}_{sTimeStr}_{eTimeStr}.xlsx";

            string tableTitle = string.Format("{0:yyyy-MM-dd}至{1:yyyy-MM-dd} {2} {3} 平台收款记录", sTimeStr, eTimeStr, sName, sCoName);

            //表头设置
            List<ExcelColumnFormat> tmap = new List<ExcelColumnFormat>
            {
                 new ExcelColumnFormat("totalAmount",  "总计"        ,null,"#.00",System.Drawing.ContentAlignment.BottomRight,"Price")
               , new ExcelColumnFormat("SendTime",     "提交时间"    ,18,"yyyy-MM-dd HH:mm:ss",null,"DateTime")
               , new ExcelColumnFormat("DoctorName",   "医生"        ,null)
               , new ExcelColumnFormat("CustomerName", "患者"        ,null)
               , new ExcelColumnFormat("Gender",       "性别"        ,null)
               , new ExcelColumnFormat("SendOrderId",  "天使订单号"  ,null)
               , new ExcelColumnFormat("NetOrderNO",   "健客订单号"  ,null,dataFormat:"Id")
               , new ExcelColumnFormat("StationName",  "接诊工作站"  ,26)
            };
            //显示的字段
            string cols = "SendTime,StationName,totalAmount,DoctorName,CustomerName,Gender,SendOrderId,NetOrderNO";
            //字段的值的处理
            Func<object, string, object> tranfunc = (object val, string colKeyName) =>
            {
                switch (colKeyName)
                {
                    case "Gender": return Ass.P.PIntV(val, 2).ToGenderString();
                    case "SendTime": return Ass.P.PDateTimeV(val)?.ToStdString();
                    case "NetOrderNO": return "JK"+Ass.P.PStr(val);
                }
                return val;
            };
            var fs = Code.Utility.FileUtils.DataTableToExcel(fname, ds.Tables[0], tmap, cols, tranfunc, true, "三方发药收款记录", tableTitle);
            //var fs = Ass.ExcelHelper.DataTableToExcel(fname, ds.Tables[0], tmap, cols, tranfunc, true, "三方发药收款记录", tableTitle);
            if (fs != null) return File(fs, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fname);
            else return Content("没有数据");
        }

        
         public async Task<IActionResult> ExportExcel_FinaceStatement(string searchText, int? stationId, int? supplierId, string timeRange = "Today")
        {
            DateTime? sTime = null, eTime = null;
            base.initialData_TimeRange(ref sTime, ref eTime, timeRange: timeRange); //时间初始化
            if (!stationId.HasValue) stationId = UserSelf.StationId;
            if (!supplierId.HasValue) supplierId = MPS.DefaultSupplierId;
            var dal = new CHIS.DAL.Staticstics_Finatial();
            var ds = await dal.FinanceStatementAsync(searchText, stationId.Value, sTime.Value, eTime.Value, 0, 0);


            var sName = _db.CHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == stationId).StationName; 
            var sTimeStr = sTime?.ToString("yyyyMMdd");
            var eTimeStr = eTime?.AddDays(-1).ToString("yyyyMMdd");
            string fname = $"{sName}收款记录_{sTimeStr}_{eTimeStr}.xlsx";

            string tableTitle = string.Format("{0:yyyy-MM-dd}至{1:yyyy-MM-dd} {2}收款记录", sTimeStr, eTimeStr, sName);

            //表头设置
            List<ExcelColumnFormat> tmap = new List<ExcelColumnFormat>
            {
                 new ExcelColumnFormat("TotalAmount",  "金额"        ,null,"#.00",System.Drawing.ContentAlignment.BottomRight,"Price")
               , new ExcelColumnFormat("PayedTime",    "支付时间"    ,18,"yyyy-MM-dd HH:mm:ss",null,"DateTime")
               , new ExcelColumnFormat("DoctorName",   "医生"        ,null)
               , new ExcelColumnFormat("CustomerName", "患者"        ,null)
               , new ExcelColumnFormat("Gender",       "性别"        ,null)
               , new ExcelColumnFormat("StationName",  "接诊工作站"  ,26)
               , new ExcelColumnFormat("FeeTypeCode",  "支付方式"  )
               , new ExcelColumnFormat("sysOpMan",     "操作人"  )
            };
            //显示的字段
            string cols = "PayedTime,StationName,TotalAmount,DoctorName,CustomerName,Gender,FeeTypeCode,sysOpMan";
            //字段的值的处理
            Func<object, string, object> tranfunc = (object val, string colKeyName) =>
            {
                switch (colKeyName)
                {
                    case "Gender": return Ass.P.PIntV(val, 2).ToGenderString();
                    case "SendTime": return Ass.P.PDateTimeV(val)?.ToStdString();
                }
                return val;
            };
           // var fs = Code.Utility.FileUtils.DataTableToExcel(fname, ds.Tables[0], tmap, cols, tranfunc, true, "三方发药收款记录", tableTitle);
            var fs = Ass.ExcelHelper.DataTableToExcel(fname, ds.Tables[0], tmap, cols, tranfunc, true, "收款记录", tableTitle);
            if (fs != null) return File(fs, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fname);
            else return Content("没有数据");
        }

        #endregion


    }

}