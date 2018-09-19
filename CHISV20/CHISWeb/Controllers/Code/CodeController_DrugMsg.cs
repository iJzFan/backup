using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ass;
//using System.Data.Entity;
using CHIS.Codes.Utility;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    public partial class CodeController
    {
        public IActionResult CHIS_Code_DrugMsg(int pageIndex = 1)
        {
            return View();
        }

        /// <summary>
        /// 获取药品单，有分页
        /// </summary>
        /// <param name="drugTypeId">药品类别的字典Id</param> 
        /// <param name="searchTxt">药品名称、简拼、简写</param>
        public IActionResult getDrugListByDrugStockTypeId(int drugStockTypeId, string searchTxt = null, string applyStatus = null)
        {
            IQueryable<vwCHIS_Code_Drug_Main> finds;
            if (applyStatus.IsNotEmpty())
            {
                var sql = $"select d.* from CHIS_Code_Drug_Main_Apply a left join vwCHIS_Code_Drug_Main d on a.DrugId = d.DrugId where a.[Status]='{applyStatus}'";
                finds = _db.vwCHIS_Code_Drug_Main.AsNoTracking().FromSql(sql);
            }
            else
            {
                finds = _db.vwCHIS_Code_Drug_Main.AsNoTracking();
            }

            if (drugStockTypeId > 0) finds = finds.Where(m => m.DrugStockTypeId == drugStockTypeId);
            if (!string.IsNullOrWhiteSpace(searchTxt))
            {
                var t = searchTxt.GetStringType();
                if (t.IsNumber)
                {
                    if (searchTxt.Length > 7) finds = finds.Where(m => m.BarCode == searchTxt);
                    else
                    {
                        int drugId = 0;
                        if (int.TryParse(searchTxt, out drugId)) finds = finds.Where(m => m.DrugId == drugId);
                    }
                }
                else
                    finds = finds.Where(m => m.CodeDock.Contains(searchTxt));
            }
            if (drugStockTypeId == 0 && searchTxt.IsEmpty() && applyStatus.IsEmpty()) finds = finds.Where(m => false);
            return FindPagedData_jqgrid(finds.OrderByDescending(m => m.sysOpTime));
        }


        //编辑的页面操作 op=NEWF/NEW/MODIFYF/MODIFY/DELETE ,mold为新增的方法，其中1为西药和成药，2为中药,3为材料，4为处置项，
        public IActionResult CHIS_Code_DrugMsg_Edit(string op, int mold,
            CHIS.Models.ViewModels.Code_DrugViewModel model,//主表，仓库，药房属性
            string recid = null)
        {
            var now = DateTime.Now;
            string editViewName = nameof(CHIS_Code_DrugMsg_Edit);
            //判断返回的页面
            //switch (mold)
            string typeTitle = "药品";
            switch (mold)
            {
                case 1:
                    typeTitle = "成药";

                    break;
                case 2:
                    typeTitle = "中草药";
                    break;
                case 3:
                    typeTitle = "材料";

                    break;
                case 4:
                    typeTitle = "处置";

                    break;
                default:
                    typeTitle = "药品";
                    break;
            }
            ViewBag.TypeTitle = typeTitle;

            // string editViewName= nameof(CHIS_Code_DrugMsg_Edit);
            var u = UserSelf;
            ViewBag.OP = op;// 初始化操作类别         
            switch (ViewBag.OP = op.ToUpper())
            {
                case "NEWF": //新增页面 空白的数据页面
                    return TryCatchFunc(() =>
                    {
                        var modelNew = _drugSvr.GetNewModel(u.OpId, u.OpMan);
                        if (mold == 3 || mold == 4 && modelNew.CHIS_Code_Drug_Outpatient.UnitSmallId > 0)
                        {
                            modelNew.CHIS_Code_Drug_Outpatient.DosageUnitId = modelNew.CHIS_Code_Drug_Outpatient.UnitSmallId;
                        }
                        ViewBag.OP = "NEW";
                        return View(editViewName, modelNew);
                    });
                case "NEW": // 更新新增的数据
                    return TryCatchFunc(() =>
                    {
                        _drugSvr.SaveNewModel(model, null, u.OpId, u.OpMan);
                        return null;
                    });
                case "MODIFYF": //修改 查找出修改的原始实体数据
                    return TryCatchFunc(() =>
                    {
                        now = DateTime.Now;
                        var modelModify = _drugSvr.ModifyInitialModel(recid);
                        ViewBag.OP = "MODIFY";
                        base._setDebugText(now);
                        return View(editViewName, modelModify);
                    });
                case "MODIFY": //修改后的数据
                    return TryCatchFunc(() =>
                    {
                        _drugSvr.SaveModifyModel(model, u.OpId, u.OpMan);
                        return null;
                    });
                case "DELETE": //删除，返回json        
                    return TryCatchFuncW(async () =>
                    {
                        await _drugSvr.DeleteAsync(recid);
                        return null;
                    });
                case "VIEW":
                    return TryCatchFunc(() =>
                    {
                        var modelModify = _drugSvr.ModifyInitialModel(recid);
                        return View(editViewName, modelModify);
                    });
                default:
                    return View("Error", new Exception("没有定义页面命令！"));
            }

        }


        //申请新药
        public IActionResult ApplyDrug(string op, CHIS.Models.ViewModels.Code_DrugViewModel model, string recid = null)
        {
            return TryCatchFunc(() =>
            {
                switch (op)
                {
                    case "NEW":
                        _drugSvr.SaveNewModel(model, new CHIS_Code_Drug_Main_Apply
                        {
                            ApplyTime = DateTime.Now,
                            OpMan = UserSelf.OpManFullMsg,
                            StationId = UserSelf.StationId,
                            Status = "APPLYING" //申请中                            
                        }, UserSelf.OpId, UserSelf.OpMan); return null;
                    case "MODIFY":
                        break;
                    case "DELETE":
                        break;
                    default: throw new Exception("错误的操作码");
                }
                return null;
            });
        }
        //审核新药
        public IActionResult PendingDrug(int drugId, string status, string rejectReson)
        {
            return TryCatchFunc(() =>
            {
                var drug = _db.CHIS_Code_Drug_Main_Apply.FirstOrDefault(m => m.DrugId == drugId);
                drug.Status = status;
                drug.RejectReson = rejectReson;
                drug.AdjustOpMan = UserSelf.OpManFullMsg;
                drug.AdjustTime = DateTime.Now;
                Logger.WriteInfoAsync("药品基础信息", "PendingDrug", $"设置药品{drug.DrugId}{rejectReson}");
                _db.SaveChanges();

                var dg = _db.CHIS_Code_Drug_Main.FirstOrDefault(m => m.DrugId == drugId);
                dg.IsEnable = status == "ALLOWED";//是否通过审核
                _db.SaveChanges();
                return null;
            });
        }



        public IActionResult ImportDrugBat()
        {
            return View("CHIS_Code_DrugImportBat");
        }
        public async Task<IActionResult> JKImportDrugs(IFormFile importExcel)
        {
            try
            {
                DateTime dt = DateTime.Now;
                string sWebRootFolder = Global.ConfigSettings.ImportFilePath;
                string sFileName = $"{Guid.NewGuid()}.xlsx";
                FileInfo f = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
                string fn = f.ToString();
                using (FileStream fs = new FileStream(fn, FileMode.Create))
                {
                    importExcel.CopyTo((Stream)fs);
                    fs.Flush();
                }
                var items = new CHIS.Code.Utility.FileUtils().ImportExcel<Models.TEMP_Excel_JKImport>(fn);

                //合法性检查与数据重新整理                    
                foreach (var item in items)
                {
                    item.UnitId = Global.UnitName2Id(item.UnitName);
                    item.CheckData();
                }
                await _db.Database.ExecuteSqlCommandAsync("truncate table TEMP_Excel_JKImport");//重置表格
                await _db.AddRangeAsync(items);//一次写入大量表格数据
                _db.SaveChanges();
                //运行处理后的存储过程
                await _db.Database.ExecuteSqlCommandAsync("exec sp_TEMP_Excel_JKImport");
                base._setDebugText(dt);
                return View("Success");
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }

        }


        public IActionResult DrugNormalEdit(int drugId)
        {
            var model = _db.vwCHIS_Code_Drug_Main.AsNoTracking().FirstOrDefault(m => m.DrugId == drugId);
            return View("CHIS_Code_DrugMsg_NormalEdit", model);
        }



        [HttpPost]
        public IActionResult DrugNormalEdit(vwCHIS_Code_Drug_Main drugOutpatient)
        {
            var model = _drugSvr.DrugNormalEdit(drugOutpatient);            
            ViewBag.BackLink = "/Pharmacy/DurgIncome?drugId=" + model.DrugId;
            ViewBag.BackLinkName = "入库该药品";
            return View("Success");
        }









    }
}