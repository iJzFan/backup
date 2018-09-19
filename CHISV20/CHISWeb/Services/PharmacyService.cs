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
using CHIS.Code.Managers;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CHIS.Services
{
    public class PharmacyService : BaseService
    {
        IMyLogger _logger;
        public PharmacyService(CHISEntitiesSqlServer db, IMyLogger logger) : base(db)
        {
            _logger = logger;
        }

        public async Task<bool> DeleteIncomeItem(long stockInId)
        {
            var find = _db.CHIS_DurgStock_Income.Find(stockInId);
            find.IsDeleted = true;
            await _db.SaveChangesAsync();
            var rlt = ChangeDrugMonitor(find.DrugId, find.StationId, (0 - find.Num), find.InUnitId, out int stockNumOrig, out int stockNum);//改变减掉对应的库存
            _db.Update(rlt); await _db.SaveChangesAsync();
            var n = _db.vwCHIS_DrugStock_Monitor.AsNoTracking().FirstOrDefault(m => m.StationId == find.StationId && m.DrugId == find.DrugId);
            await _logger.WriteInfoAsync("我的药房入库", "删除", $"删除资料{stockInId},变动后库存:{n.DrugStockNum}{n.StockUnitName}");
            return true;
        }


        public IQueryable<vwCHIS_DurgStock_Income> QueryIncomeList(string searchText, DateTime dt0, DateTime dt1, int stationId)
        {
            searchText = Ass.P.PStr(searchText);//过滤条件           
            //默认条件和搜索
            var finds = _db.vwCHIS_DurgStock_Income.AsNoTracking().Where(m => m.StationId == stationId &&
            m.IsDeleted == false && m.InTime > dt0 && m.InTime < dt1);
            //附加条件和搜索
            if (searchText.IsNotEmpty())
            {
                int drugId = 0;
                if (int.TryParse(searchText, out drugId))
                {
                    finds = finds.Where(m => m.DrugId == drugId);
                }
                else
                {
                    //  finds = finds.Where(m => m.CodeDock.Contains(searchText));
                    //finds = finds.FromSql($"select * from vwCHIS_DurgStock_Income where charindex('{searchText}',codedock)>0");
                    finds = finds.Where(m => m.CodeDock.Contains(searchText));
                }

            }
            return finds;
        }


        //修改药品监控表数据
        internal CHIS_DrugStock_Monitor ChangeDrugMonitor(int drugId, int stationId, int num, int inUnitId, out int stockNumPre, out int stockNum, vwCHIS_Code_Drug_Main drug = null)
        {
            stockNumPre = 0;
            if (drug == null) drug = _db.vwCHIS_Code_Drug_Main.AsNoTracking().FirstOrDefault(m => m.DrugId == drugId);
            var find = _db.CHIS_DrugStock_Monitor.AsNoTracking().FirstOrDefault(m => m.StationId == stationId && m.DrugId == drugId);
            if (find == null)
            {
                find = _db.CHIS_DrugStock_Monitor.Add(new CHIS_DrugStock_Monitor
                {
                    DrugStockMonitorId = $"{stationId}.{drugId}",
                    DrugId = drugId,
                    DrugStockNum = inUnitId == drug.UnitBigId && inUnitId != drug.UnitSmallId ? (int)(num * drug.OutpatientConvertRate) : num,
                    StationId = stationId,
                    StockUnitId = drug.UnitSmallId.Value,
                    StockSalePrice = 0,
                    StockDrugIsEnable = true,
                    StockLineNum = 100
                }).Entity;
                _db.SaveChanges();
                stockNum = find.DrugStockNum;
                return null;
            }
            else
            {
                stockNumPre = find.DrugStockNum;
                if (find.StockUnitId == inUnitId) find.DrugStockNum += num;
                else if (find.StockUnitId == drug.UnitSmallId && inUnitId == drug.UnitBigId) find.DrugStockNum += (int)(num * drug.OutpatientConvertRate);
                stockNum = find.DrugStockNum;
                return find;
            }
        }

        /// <summary>
        /// 保存库存
        /// </summary>
        /// <param name="items"></param>
        /// <param name="opId"></param>
        /// <param name="opMan"></param>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public async Task<bool> SaveIncome(IEnumerable<Models.CHIS_DurgStock_Income> items, int opId, string opMan, int stationId)
        {
            foreach (var item in items)
            {
                if (item.BatNo.IsEmpty()) throw new Exception("必须填写批号");
                if (item.DeadlineTime < DateTime.Now) throw new Exception("过期时间不得小于当前");
                if (item.ProduceTime > item.DeadlineTime) throw new Exception("生产时间不得比过期时间晚");
                if (item.ProduceTime < DateTime.Now.AddYears(-8)) throw new Exception("生产时间不得早于8年前");
                if (item.DrugId == 0) throw new Exception("没有发现药品Id");
                if (item.Num <= 0) throw new Exception("入库数量必须大于0");
                if (item.InUnitId == 0) throw new Exception("一定要输入入库单位");
                item.InTime = DateTime.Now;
                item.OpMan = opMan;
                item.OpTime = DateTime.Now;
                item.StationId = stationId;
            }

            _db.BeginTransaction();
            try
            {
                //获取所有药品信息
                var drugids = items.Select(m => m.DrugId).Distinct();
                var drugs = _db.vwCHIS_Code_Drug_Main.AsNoTracking().Where(m => drugids.Contains(m.DrugId)).ToList();

                //更新药品监控表的信息
                List<CHIS_DrugStock_Monitor> updates = new List<CHIS_DrugStock_Monitor>();
                foreach (var item in items)
                {
                    // Console.WriteLine(item.DrugId);
                    var drug = drugs.FirstOrDefault(m => m.DrugId == item.DrugId);
                    var drugOfMonitor = this.ChangeDrugMonitor(item.DrugId, item.StationId, item.Num, item.InUnitId, out int stockNumPre, out int stockNum, drug);
                    if (drugOfMonitor != null) updates.Add(drugOfMonitor);
                    item.StockNum = stockNum;//设置库存数据
                    item.StockNumPre = stockNumPre;//库存原数据h
                }
                _db.UpdateRange(updates);//批量更新库存监控数据
                _db.AddRange(items);//批量写入入库数据
                await _db.SaveChangesAsync();
                _db.CommitTran();
                return true;
            }
            catch (Exception ex) { _db.RollbackTran(); throw ex; }
        }

        /// <summary>
        /// 批量导入文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public IEnumerable<Models.DataModel.ExcelIncomeImportModel> ImportIncomeExcel(IFormFile file)
        {

            string sWebRootFolder = Global.ConfigSettings.ImportFilePath;
            string sFileName = $"{Guid.NewGuid()}.xlsx";
            FileInfo f = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            string fn = f.ToString();
            try
            {
                using (FileStream fs = new FileStream(fn, FileMode.Create))
                {
                    file.CopyTo((Stream)fs);
                    fs.Flush();
                }
                var items = new CHIS.Code.Utility.FileUtils().ImportExcel<Models.DataModel.ExcelIncomeImportModel>(fn);

                //合法性检查与数据重新整理                    
                foreach (var item in items)
                {
                    item.IncomeUnitId = Global.UnitName2Id(item.IncomeUnitName);
                    item.CheckData();
                }

                //重新整理数据
                var drugsids = items.Select(m => m.DrugId);
                var drugs = _db.vwCHIS_Code_Drug_Main.Where(m => drugsids.Contains(m.DrugId));
                foreach (var item in items)
                {
                    var drug = drugs.FirstOrDefault(m => m.DrugId == item.DrugId);
                    item.DrugName = drug.DrugName;
                    item.DrugModel = drug.DrugModel;
                    item.ManufacturerOrigin = drug.ManufacturerOrigin;
                    item.DrugOrigPlace = drug.OriginPlace;
                    item.DrugAlias = drug.Alias;
                    item.outUnitBigId = drug.UnitBigId;
                    item.outUnitBigName = drug.OutUnitBigName;
                    item.outUnitSmallId = drug.UnitSmallId;
                    item.outUnitSmallName = drug.OutUnitSmallName;
                    item.UnitConvertRate = drug.OutpatientConvertRate;
                }

                return items;
            }
            catch (Exception ex)
            {
                if (f.Exists) System.IO.File.Delete(fn);
                throw ex;
            }


        }

        //获取我的申请的清单
        public IQueryable<vwCHIS_Code_Drug_Main_Apply> QueryMyDrugMenuList(string searchText, DateTime dt0, DateTime dt1, string status, int stationId)
        {
            searchText = Ass.P.PStr(searchText);
            //本工作站的本地药品
            var finds = _db.vwCHIS_Code_Drug_Main_Apply.AsNoTracking().Where(m => m.StationId == stationId && m.ApplyTime > dt0 && m.ApplyTime < dt1);
            if (status.IsNotEmpty()) finds = finds.Where(m => m.Status == status);

            if (searchText.IsNotEmpty())
            {
                int drugId = 0;
                if (int.TryParse(searchText, out drugId))
                {
                    finds = finds.Where(m => m.DrugId == drugId);
                }
                else finds = finds.Where(m => m.CodeDock.Contains(searchText));
            }
            return finds;
        }

        public IQueryable<vwCHIS_DrugStock_Monitor> QueryExportStockMonitorList(string searchText, int drugStockTypeId, int stockNumStatus, string stockPriceStatus, int stationId)
        {
            searchText = Ass.P.PStr(searchText);
            //本工作站的本地药品
            var finds = _db.vwCHIS_DrugStock_Monitor.AsNoTracking().Where(m => m.StationId == stationId && m.SourceFrom == (int)DrugSourceFrom.Local);
            if (drugStockTypeId > 0) finds = finds.Where(m => m.DrugStockTypeId == drugStockTypeId);
            //根据条件搜索         
            if (stockNumStatus > 0)
            {
                if (stockNumStatus == 1) finds = finds.Where(m => m.DrugStockNum < m.StockLineNum);
                if (stockNumStatus == 2) finds = finds.Where(m => m.DrugStockNum >= m.StockLineNum);
            }

            if (stockPriceStatus.IsNotEmpty())
            {
                var rs = new Ass.RangeString(stockPriceStatus);
                if (rs.Status == RangeStringStatus.LESS)
                    finds = finds.Where(m => m.StockSalePrice <= rs.Max);
                else if (rs.Status == RangeStringStatus.BETWEEN)
                    finds = finds.Where(m => m.StockSalePrice > rs.Min && m.StockSalePrice <= rs.Max);
                else if (rs.Status == RangeStringStatus.ABOVE)
                    finds = finds.Where(m => m.StockSalePrice > rs.Min);
                else if (rs.Status == RangeStringStatus.EQUAL)
                    finds = finds.Where(m => m.StockSalePrice == rs.EqualVal);
            }

            if (searchText.IsNotEmpty())
            {
                long drugId = 0;
                if (long.TryParse(searchText, out drugId))
                {
                    if (searchText.Length > 10) finds = finds.Where(m => m.BarCode == searchText);
                    else finds = finds.Where(m => m.DrugId == drugId);
                }
                else finds = finds.Where(m => m.CodeDock.Contains(searchText));
            }
            return finds;
        }


        public vwCHIS_DrugStock_Monitor Find(string drugStockMonitorId)
        {
            var model = _db.vwCHIS_DrugStock_Monitor.FirstOrDefault(m => m.DrugStockMonitorId == drugStockMonitorId);
            return model;
        }
        public async Task<bool> DeleteDrugStockMonitorById(string drugStockMonitorId, int stationId)
        {
            var model = _db.CHIS_DrugStock_Monitor.FirstOrDefault(m => m.DrugStockMonitorId == drugStockMonitorId);
            if (model.StationId != stationId) throw new Exception("非工作站不能删除。");
            _db.CHIS_DrugStock_Monitor.Remove(model);
            await _db.SaveChangesAsync();
            return true;
        }
        //重新设置药品库存监控数据
        public bool ChangeDrugStockData(string drugStockMonitorId, decimal newPrice, int num, int safeNum, string rmk, string opMan)
        {
            var find = _db.CHIS_DrugStock_Monitor.FirstOrDefault(m => m.DrugStockMonitorId == drugStockMonitorId);
            var origPrice = find.StockSalePrice;
            find.StockSalePrice = newPrice;
            find.DrugStockNum = num;
            find.StockLineNum = safeNum;
            _db.CHIS_DrugStock_Monitor_Log.Add(new CHIS_DrugStock_Monitor_Log
            {
                DrugId = find.DrugId,
                DrugStockMonitorId = find.DrugStockMonitorId,
                StationId = find.StationId,
                NewPrice = newPrice,
                OrigiPrice = origPrice,
                OpMan = opMan,
                LogTime = DateTime.Now,
                NewNum = num,
                OrigiNum = find.DrugStockNum,
                NewSafeLineNum = safeNum,
                OrigiSafeLineNum = find.StockLineNum ?? 100,
                OpRmk = rmk
            });
            _db.SaveChanges();
            return true;
        }


    }
}
