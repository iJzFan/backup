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

namespace CHIS.Controllers
{
    public partial class CodeController
    { 
        public IActionResult CHIS_Code_Department()
        {
            //var sysUser = base.GetCurrentUserInfo();
            //ViewBag.SysUser = sysUser;
            //Loger.WriteInfo("Code", "CHIS_Code_Department", $"查看工作站科室设置。");
            return View(nameof(CHIS_Code_Department));
        }


        /// <summary>
        /// 工作站科室记录查询
        /// </summary>
        /// <param name="stationID">工作站ID</param>
        /// <param name="keyWord">查询关键字</param>
        /// <returns></returns>
        public IActionResult GetGridJson_Department(int stationID, string keyWord)
        {
            var find = from item in _db.vwCHIS_Code_Department
                       where item.StationID == stationID &&
                             (string.IsNullOrEmpty(keyWord) ? true : item.DepartmentName.Contains(keyWord))
                       select item;
            return base.FindPagedData_jqgrid(find.OrderBy(m => m.StationID).ThenBy(m => m.ShowOrder));

        }
        /// <summary>
        /// 编辑的页面操作
        /// </summary>
        /// <param name="op">op=NEWF/NEW/MODIFYF/MODIFY/DELETE</param>
        /// <param name="model">数据模型</param>
        /// <param name="recID">记录ID</param>
        /// <returns></returns>
        public IActionResult CHIS_Code_Department_Edit(string op,
            CHIS.Models.CHIS_Code_Department model, int recID = 0,int stationId=0)
        {
            string editViewName = nameof(CHIS_Code_Department_Edit);
            try
            {
                //var user = base.GetCurrentUserInfo();
                var user = UserSelf;
                ViewBag.OP = op;// 初始化操作类别
                ViewBag.SysUser = user;
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面
                        int count = _db.CHIS_Code_Department.Where(m => m.StationID == stationId).Count() + 1;
                        model = new Models.CHIS_Code_Department()
                        {
                            IsEnable = true,                      
                            StationID = stationId,
                            ParentID = 0,
                            ShowOrder = (short)count,
                            IsNotTreatDept=false,
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
                            var result = _db.Add(model);
                            _db.SaveChanges();
                            return Json(new { state = "success", message = "保存成功！" });
                        }
                        catch (Exception ex)
                        {
                            //Loger.WriteError("CODE", editViewName, ex);
                            return Json(new { state = "error", message = ex.Message });
                        }

                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        if (isNotId(recID)) throw new Exception("不是正确的编号Id格式！");
                        var modelModify = _db.CHIS_Code_Department.FirstOrDefault(m => m.DepartmentID == recID);
                        if (modelModify == null) throw new Exception("没有找到该数据！");
                        //查找当前工作站科室个数
                        var find = from a in _db.CHIS_Code_Department
                                   join b in _db.CHIS_Code_Department
                                   on a.StationID equals b.StationID
                                   where b.DepartmentID == recID
                                   select a.DepartmentID;

                        ViewBag.OP = "MODIFY";
                        ViewBag.Count = find.Count();
                        return View(editViewName, modelModify);

                    case "MODIFY": //修改后的数据
                        try
                        {
                            if (model.DepartmentID <= 0) ModelState.AddModelError("", "操作ID不能为零");
                            model.OpID = user.OpId;
                            model.OpMan = user.OpMan;
                            model.OpTime = DateTime.Now;
                            using (var trans = _db.Database.BeginTransaction())
                            {
                                try
                                {
                                    //若同一工作站存在序号相同的记录时，其余记录序号增1
                                    var repeat = _db.CHIS_Code_Department.Where(m => m.ShowOrder == model.ShowOrder
                                    && m.StationID == model.StationID && m.StationID != model.StationID);

                                    if (repeat.Count() > 0)
                                    {
                                        short index = model.ShowOrder ?? 0;
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
                                catch
                                {
                                    trans.Rollback();
                                    throw new Exception("更新实体时错误！");
                                }
                            }

                            //Loger.WriteInfo("CODE", editViewName, $"修改工作站科室记录，记录ID{model.DepartmentID}。");
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
                            var d = _db.CHIS_Code_Department.FirstOrDefault(m => m.DepartmentID == recID);
                            _db.Remove<Models.CHIS_Code_Department>(d);
                            _db.SaveChanges();
                            return Json(new { state = "success", message = "删除成功！" });
                        }
                        catch (Exception ex) { return Json(new { state = "error", message = ex.Message }); }

                    case "VIEW": //查看 查找出原始实体数据
                        if (isNotId(recID)) throw new Exception("不是正确的编号Id格式！");
                        var modelView = _db.CHIS_Code_Department.FirstOrDefault(m => m.DepartmentID == recID);
                        if (modelView == null) throw new Exception("没有找到该数据！");

                        return View(editViewName, modelView);

                    default:
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //Loger.WriteError("CODE", editViewName, ex);
                return View("ErrorBlank", ex);
            }
        }

        /// <summary>
        /// 修改工作站父级节点
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public IActionResult UpateWorkStationParent(int id, int pid)
        {
            try
            {
                var model = _db.CHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == id);
                model.ParentStationID = pid;
                _db.Update(model);
                _db.SaveChanges();
                //Loger.WriteInfo("Code", "CHIS_Code_Department_Edit", $"修改工作站父级ID，记录ID{id}。");
                return Json(new { state = "success", message = "保存成功！" });
            }
            catch (Exception ex)
            {
                //Loger.WriteError("Code", "/CHIS_Code_Department_Edit/UpateWorkStationParent", ex);
                return View("ErrorBlank", ex);
            }
        }

        ///// <summary>
        ///// 工作站科室记录导出
        ///// </summary>
        ///// <returns></returns>
        //public IActionResult Export_Department(int stationID, string keyWord)
        //{
        //    //var user = base.GetCurrentUserInfo();
        //    var user = UserLoginData;
        //    int pageIndex = 1, pageSize = 0; string sort = "";
        //    base.getJqGridInfo(out pageIndex, out pageSize, out sort, 1, user.TableRecordsPerPage);

        //    if (string.IsNullOrWhiteSpace(sort)) sort = getQuery("sort");
        //    try
        //    {
        //        var find = from item in MainDbContext.vwCHIS_Code_Department
        //                   where (stationID == 0 ? true : item.StationID == stationID) &&
        //                         (string.IsNullOrEmpty(keyWord) ? true : item.DepartmentCode == keyWord || item.DepartmentName == keyWord)
        //                   select item;

        //        var dataList = find.OrderBy(m => m.ShowOrder/*, sort*/);

        //        List<CHIS.Codes.Utility.ExcelField> map = new List<Codes.Utility.ExcelField>()
        //        {
        //            new ExcelField("DepartmentCode","科室编号") ,
        //            new ExcelField("DepartmentName","科室名称") ,
        //            new ExcelField("StationName","工作站名称"),
        //            new ExcelField("ParentName","上级部门"),
        //            new ExcelField("ShowOrder","显示顺序"),
        //            new ExcelField("IsEnable","可用状态","[可用,True][停用,False]"),
        //            new ExcelField("StopDate","停用日期"),
        //            new ExcelField("OpMan","操作人"),
        //            new ExcelField("OpTime","操作时间"),
        //            new ExcelField("Remark","备注")
        //        };

        //        //Loger.WriteInfo("CODE", "Export_Department", "导出工作站科室列表");
        //        System.IO.Stream sm = CHIS.Codes.Utility.Excel.CreateExcelStream(dataList, "工作站科室列表", map);
        //        DateTime dt = DateTime.Now;
        //        string dateTime = dt.ToString("yyMMddHHmmssfff");
        //        string filename = "Export" + dateTime + ".xls";
        //        return File(sm, "application/vnd.ms-excel", filename);
        //    }
        //    catch (Exception ex)
        //    {
        //        //Loger.WriteError("CODE", "Export_Department", ex);
        //        return View("ErrorBlank", ex);
        //    }
        //}

    }
}
