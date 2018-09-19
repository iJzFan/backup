
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ass;
using CHIS.Code;
using CHIS.Codes.Utility;
using CHIS.Models;
using CHIS.Code.MyExpands;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Controllers
{
    public partial class CodeController
    {
        public IActionResult CHIS_Code_WorkStation()
        {
            //var sysUser = base.GetCurrentUserInfo();
            //ViewBag.SysUser = sysUser;
            //Loger.WriteInfo("Code", "CHIS_Code_Workstation", $"查看工作站设置。");

            return View(nameof(CHIS_Code_WorkStation));
        }

        //工作站记录查询
        public IActionResult GetGridJson_WorkStation(int stationId, string keyWord)
        { 
            var findList = stationId == 0 ? _db.vwCHIS_Code_WorkStation.Where(m=>m.StationID==0): GetSonFuncs(_db.vwCHIS_Code_WorkStation.ToList(), stationId);
            if (keyWord.GetStringType().IsNotEmpty) findList = findList.Where(m => m.StationName.Contains(keyWord) || m.Telephone == keyWord);
            return FindPagedData_jqgrid<vwCHIS_Code_WorkStation>(findList.AsQueryable().OrderBy(m => m.ParentStationID).ThenBy(m=>m.ShowOrder));             
        }

        //递归获取功能项目
        private IEnumerable<vwCHIS_Code_WorkStation> GetSonFuncs(IEnumerable<vwCHIS_Code_WorkStation> alllist, int? pid)
        {
            var query = from c in alllist
                        where c.ParentStationID == pid
                        select c;
            return query.ToList();
            //return query.ToList().Concat(query.ToList().SelectMany(t => GetSonFuncs(alllist, t.ParentStationID)));
        }



        /// <summary>
        /// 获取上级工作站树形列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetTreeSelectJson_WorkStation()
        {
            var data = _db.CHIS_Code_WorkStation.FromSql("exec [sp_Common_LoadTreeRoadToLeafs] {0},{1},{2},{3}",
                                                                    "CHIS_Code_WorkStation", "StationID", "ParentStationID", UserSelf.StationId)
                                                                    .OrderBy(m=>m.ParentStationID).ThenBy(m=>m.ShowOrder).ToList();
            var treeList = new List<TreeSelectModel>();
            foreach (var item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.StationID.ToString();
                treeModel.text = item.StationName;
                treeModel.parentId = item.ParentStationID.ToString();
                treeList.Add(treeModel);
            }

            return Content(treeList.TreeSelectJson());
        }
        /// <summary>
        /// 增删该查
        /// </summary>
        /// <param name="op"></param>
        /// <param name="model"></param>
        /// <param name="recID"></param>
        /// <returns></returns>
        //编辑的页面操作 op=NEWF/NEW/MODIFYF/MODIFY/DELETE 
        public IActionResult CHIS_Code_WorkStation_Edit(string op,
            CHIS.Models.CHIS_Code_WorkStation model, int recID = 0)
        {
            string editViewName = nameof(CHIS_Code_WorkStation_Edit);
            try
            {
                var user = UserSelf;

                ViewBag.OP = op;// 初始化操作类别
                ViewBag.SysUser = user;
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面
                        int count = _db.CHIS_Code_WorkStation.Count() + 1;
                        model = new Models.CHIS_Code_WorkStation()
                        {
                            IsEnable = true,
                            ShowOrder = count,
                            OpID = user.OpId,
                            OpMan = user.OpManFullMsg,
                            OpTime = DateTime.Now
                        };

                        ViewBag.OP = "NEW";
                        ViewBag.Count = count;
                        return View(editViewName, model);

                    case "NEW": // 更新新增的数据 
                        try
                        {
                            model.OpID = user.OpId;
                            model.OpMan = user.OpManFullMsg;
                            model.OpTime = DateTime.Now;
                            if (model.StationKeyCode.IsEmpty()) model.StationKeyCode = Ass.Data.Chinese2Spell.GetFstLettersLower(model.StationName);
                            if (model.StationPinYin.IsEmpty()) model.StationPinYin = Ass.Data.Chinese2Spell.GetFstLettersLower(model.StationName) + "|" + Ass.Data.Chinese2Spell.ConvertLower(model.StationName);
                            var result = _db.CHIS_Code_WorkStation.Add(model).Entity;
                            _db.SaveChanges();
                       

                            //Loger.WriteInfo("CODE", editViewName, $"新增工作站记录，记录ID{result.StationID}");
                            return Json(new { state = "success", message = "保存成功！" });
                        }
                        catch (Exception ex)
                        {
                            //Loger.WriteError("CODE", editViewName, ex);
                            return Json(new { state = "error", message = ex.Message });
                        }

                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        if (isNotId(recID)) throw new Exception("不是正确的编号Id格式！");
                        var modelModify = _db.CHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == recID);
                        if (modelModify == null) throw new Exception("没有找到该数据！");

                        ViewBag.OP = "MODIFY";
                        ViewBag.Count = _db.CHIS_Code_WorkStation.Count();
                        return View(editViewName, modelModify);

                    case "MODIFY": //修改后的数据
                        try
                        {
                            if (model.StationID <= 0) ModelState.AddModelError("", "操作ID不能为零");
                            model.OpID = user.OpId;
                            model.OpMan = user.OpMan;
                            model.OpTime = DateTime.Now;
                            if (model.StationKeyCode.IsEmpty()) model.StationKeyCode = Ass.Data.Chinese2Spell.GetFstLettersLower(model.StationName);
                            if (model.StationPinYin.IsEmpty()) model.StationPinYin = Ass.Data.Chinese2Spell.GetFstLettersLower(model.StationName) + "|" + Ass.Data.Chinese2Spell.ConvertLower(model.StationName);

                            using (var trans = _db.Database.BeginTransaction())
                            {
                                try
                                {
                                    //若存在序号相同的记录时，其余记录序号增1
                                    var repeat = _db.CHIS_Code_WorkStation.Where(m => m.ShowOrder == model.ShowOrder
                                    && m.StationID == model.StationID && m.StationID != model.StationID);
                                    if (repeat.Count() > 0)
                                    {
                                        int index = model.ShowOrder ?? 0;
                                        foreach (var item in repeat)
                                        {
                                            index += 1;
                                            item.ShowOrder = index;
                                        }
                                    }
                                    _db.Update(model);
                                    _db.SaveChanges();
                                    trans.Commit();

                                }
                                catch (Exception ex) { trans.Rollback(); throw ex; }
                            }
                            //Loger.WriteInfo("CODE", editViewName, $"修改工作站记录，记录ID{model.StationID}。");
                            return Json(new { state = "success", message = "保存成功" });
                        }
                        catch (Exception ex)
                        {
                            //Loger.WriteError("CODE", editViewName, ex);
                            return Json(new { state = "error", message = ex.Message });
                        }

                    case "DELETE": //删除记录
                        try
                        {
                            var d = _db.CHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == recID);
                            _db.Remove<Models.CHIS_Code_WorkStation>(d);
                            _db.SaveChanges();
                            //Loger.WriteInfo("CODE", editViewName, $"删除工作站记录，记录ID{recID}。");
                            return Json(new { state = "success", message = "删除成功！" });
                        }
                        catch (Exception ex) { return Json(new { state = "error", message = ex.Message }); }

                    case "VIEW": //查看 查找出原始实体数据
                        if (isNotId(recID)) throw new Exception("不是正确的编号Id格式！");
                        var modelView = _db.CHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == recID);
                        if (modelView == null) throw new Exception("没有找到该数据！");

                        return View(editViewName, modelView);

                    default:
                        throw new Exception("未知的命令");
                }
            }
            catch (Exception ex)
            {
                //Loger.WriteError("CODE", editViewName, ex);
                return View("ErrorBlank", ex);
            }
        }
        ///// <summary>
        ///// 工作站仓库记录导出
        ///// </summary>
        ///// <returns></returns>
        //public IActionResult Export_Workstation(string keyWord)
        //{
        //    var user = base.GetCurrentUserInfo();
        //    int pageIndex = 1, pageSize = 0; string sort = "";
        //    base.getJqGridInfo(out pageIndex, out pageSize, out sort, 1, user.TableRecordsPerPage);

        //    if (string.IsNullOrWhiteSpace(sort)) sort = getQuery("sort");
        //    try
        //    {
        //        var find = from item in MainDbContext.vwCHIS_Code_WorkStation
        //                   where (string.IsNullOrEmpty(keyWord) ? true :
        //                     item.StationName.Contains(keyWord) || item.LegalPerson.Contains(keyWord) || item.Address.Contains(keyWord))
        //                   select item;
        //        var dataList = find.OrderBy(m => m.ShowOrder, sort);

        //        List<CHIS.Codes.Utility.ExcelField> map = new List<Codes.Utility.ExcelField>()
        //        {
        //            new ExcelField("StationName","工作站名称") ,
        //            new ExcelField("Address","工作站地址") ,
        //            new ExcelField("Telephone","电话") ,
        //            new ExcelField("Fax","传真") ,
        //            new ExcelField("LegalPerson","法人") ,
        //            new ExcelField("ParentName","上级工作站") ,
        //            new ExcelField("IsEnable","可用状态","[可用,True][停用,False]"),
        //            new ExcelField("StopDate","停用日期"),
        //            new ExcelField("ShowOrder","显示顺序"),
        //            new ExcelField("OpMan","操作人"),
        //            new ExcelField("OpTime","操作时间"),
        //            new ExcelField("Remark","备注")
        //        };

        //        Loger.WriteInfo("CODE", "Export_Workstation", "导出工作站列表");
        //        System.IO.Stream sm = CHIS.Codes.Utility.Excel.CreateExcelStream(dataList, "工作站列表", map);
        //        DateTime dt = DateTime.Now;
        //        string dateTime = dt.ToString("yyMMddHHmmssfff");
        //        string filename = "Export" + dateTime + ".xls";
        //        return File(sm, "application/vnd.ms-excel", filename);
        //    }
        //    catch (Exception ex)
        //    {
        //        Loger.WriteError("CODE", "Export_Workstation", ex);
        //        return View("ErrorBlank", ex);
        //    }
        //}
    }
}

