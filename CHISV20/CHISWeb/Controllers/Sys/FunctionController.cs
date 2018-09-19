using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CHIS.Code;
using CHIS.Models;
using CHIS.Codes.Utility;
using Ass;
using System.Threading.Tasks;
using CHIS.Code.MyExpands;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Controllers.Sys
{

    public class FunctionController : BaseController
    {
        public FunctionController(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        /// <summary>
        /// 业务功能页面入口
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View("~/Views/Sys/Function/FunctionIndex.cshtml");//此处需要特别指明页面文件的位置
        }


        /// <summary>
        /// 业务功能网格记录查询
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns> 
        public IActionResult GetGridJson_Function(string keyWord)
        {
            var data = (from item in _db.CHIS_SYS_Function
                        where (string.IsNullOrEmpty(keyWord) ? true :
                        item.FunctionName.Contains(keyWord) || item.FunctionKey.Contains(keyWord))
                        orderby item.ParentFunctionID, item.FunctionIndex
                        select item).ToList();

            var treeList = new List<TreeGridModel>();
            foreach (var item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentFunctionID == item.FunctionID) == 0 ? false : true;
                treeModel.id = item.FunctionID.ToString();
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = (data.Count() == 1 ? "0" : item.ParentFunctionID.ToString());  //单选一条记录时，忽略父ID
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }


        /// <summary>
        /// 获取业务功能树型列表
        /// </summary>
        /// <returns></returns> 
        public IActionResult GetTreeSelectJson()
        {
            var data = from item in _db.CHIS_SYS_Function.AsNoTracking()
                       select item;

            var treeList = new List<TreeSelectModel>();
            foreach (CHIS_SYS_Function item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.FunctionID.ToString();
                treeModel.text = item.FunctionName;
                treeModel.parentId = item.ParentFunctionID.ToString();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        /// <summary>
        /// 获取当前级别业务功能个数
        /// </summary>
        /// <param name="parent">父级功能ID</param>
        /// <returns></returns>
        public IActionResult Json_FunctionCount(int parentId = 0)
        {
            try
            {
                int count = _db.CHIS_SYS_Function.Where(m => m.ParentFunctionID == parentId).Count();
                return Json(count);
            }
            catch (Exception ex)
            {

                return View("ErrorBlank", ex);
            }
        }

        /// <summary>
        /// 获取当前级别业务功能个数
        /// </summary>
        /// <param name="parent">父级功能ID</param>
        /// <returns></returns>
        public IActionResult Json_CheckFunctionKey(string functionKey, int functionId)
        {
            try
            {
                int count = _db.CHIS_SYS_Function.Where(
                     m => m.FunctionKey == functionKey && m.FunctionID != functionId).Count();
                return Json(count);
            }
            catch (Exception ex)
            {
                return View("ErrorBlank", ex);
            }
        }




        /// <summary>
        /// 编辑页面操作
        /// </summary>
        /// <param name="op">op=NEWF/NEW/MODIFYF/MODIFY/DELETE </param>
        /// <param name="model">数据模型</param>
        /// <param name="recID">记录ID</param>
        /// <returns></returns>
        public IActionResult CHIS_SYS_Function_Edit(string op,
            CHIS.Models.CHIS_SYS_Function model, int recID = 0)
        {
            string editViewName = "~/Views/Sys/Function/FunctionForm.cshtml";
            try
            {

                var user = base.UserSelf;

                ViewBag.OP = op;// 初始化操作类别
                ViewBag.SysUser = user;
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面
                        int count = _db.CHIS_SYS_Function.Where(m => m.ParentFunctionID == 0).Count() + 1;
                        model = new Models.CHIS_SYS_Function()
                        {
                            IsEnable = true,
                            ParentFunctionID = 0,
                            FunctionIndex = (short)count,
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
                            return Json(new { state = "error", message = ex.Message });
                        }

                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        if (isNotId(recID)) throw new Exception("不是正确的编号Id格式！");
                        var modelModify = _db.CHIS_SYS_Function.FirstOrDefault(m => m.FunctionID == recID);
                        if (modelModify == null) throw new Exception("没有找到该数据！");
                        //查找当前业务功能个数
                        int maxCount = _db.CHIS_SYS_Function.Where(m => m.ParentFunctionID == modelModify.ParentFunctionID).Count();
                        ViewBag.OP = "MODIFY";
                        ViewBag.Count = maxCount;
                        return View(editViewName, modelModify);

                    case "MODIFY": //修改后的数据
                        try
                        {
                            if (model.FunctionID <= 0) ModelState.AddModelError("", "记录ID不能为零");
                            model.OpID = user.OpId;
                            model.OpMan = user.OpMan;
                            model.OpTime = DateTime.Now;
                            using (var trans = _db.Database.BeginTransaction())
                            {
                                try
                                {
                                    //若同一项目类别存在序号相同的记录时，其余记录序号增1
                                    var repeat = _db.CHIS_SYS_Function.Where(m =>
                                    m.ParentFunctionID == model.ParentFunctionID && /*相同父级*/
                                    m.FunctionID != model.FunctionID && /*非我条目*/
                                    m.FunctionIndex >= model.FunctionIndex);/*大于我的排序的条目*/

                                    if (repeat.Count() > 0)
                                    {
                                        int index = model.FunctionIndex ?? 0;
                                        foreach (var item in repeat)
                                        {
                                            index += 1;
                                            var getModel = _db.CHIS_SYS_Function.Find(item.FunctionID);
                                            getModel.FunctionIndex = index;
                                        }
                                        _db.SaveChanges();
                                    }
                                    _db.Update(model);
                                    int rlt = _db.SaveChanges();
                                    trans.Commit();
                                }
                                catch
                                {
                                    trans.Rollback();
                                    throw new Exception("更新实体时错误！");
                                }
                            }
                            return Json(new { state = "success", message = "保存成功" });

                        }
                        catch (Exception ex)
                        {
                            return Json(new { state = "error", message = ex.Message });
                        }

                    case "DELETE": //删除记录
                        try
                        {
                            using (var trans = _db.Database.BeginTransaction())
                            {
                                try
                                {
                                    var delmodel = _db.CHIS_SYS_Function.AsNoTracking().FirstOrDefault(m => m.FunctionID == recID);
                                    _db.Remove<Models.CHIS_SYS_Function>(delmodel);
                                    var delm2 = _db.CHIS_Sys_Rel_RoleFunctions.AsNoTracking().Where(m => m.FunctionId == recID);
                                    _db.RemoveRange(delm2);
                                    _db.SaveChanges();
                                    trans.Commit();
                                }
                                catch
                                {
                                    trans.Rollback();
                                    throw new Exception("更新实体时错误！");
                                }
                            }

                            return Json(new { state = "success", message = "删除成功！" });
                        }
                        catch (Exception ex) { return Json(new { state = "error", message = ex.Message }); }

                    case "VIEW": //查看 查找出原始实体数据
                        if (isNotId(recID)) throw new Exception("不是正确的编号Id格式！");
                        var modelView = _db.CHIS_SYS_Function.FirstOrDefault(m => m.FunctionID == recID);
                        if (modelView == null) throw new Exception("没有找到该数据！");
                        int maxcount = _db.CHIS_SYS_Function.Where(m => m.ParentFunctionID == modelView.ParentFunctionID).Count();
                        ViewBag.Count = maxcount;
                        return View(editViewName, modelView);

                    default:
                        throw new Exception("没有发现操作类型");
                }
            }
            catch (Exception ex)
            {
                return View("ErrorBlank", ex);
            }
        }


        #region 功能菜单内部的详细功能


        //功能管理
        public IActionResult FunctionDetails()
        {
            return View("~/Views/Sys/Function/FunctionDetails.cshtml");
        }

        /// <summary>
        /// 业务功能网格记录查询
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public IActionResult GetGridJson_FuncDetails(string keyWord, int functionId = 0)
        {
            var finds = from item in _db.CHIS_Sys_FuncDetail where item.BelongFunctionId == functionId select item;
            return FindPagedData_jqgrid(finds.OrderBy(m => m.GroupKey));
        }

        public IActionResult GetGridJson_FuncDetails_Edit(string op,
        CHIS.Models.CHIS_Sys_FuncDetail model, int recId = 0,int? BelongFunctionId=null)
        {
            string editViewName = "~/Views/Sys/Function/FunctionDetailsEdit.cshtml";
            ViewBag.OP = op;
            try
            { 
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面                     
                        ViewBag.OP = "NEW";
                        if (!(BelongFunctionId >0)) throw new Exception("传入的所属板块Id错误。");
                        return View(editViewName, new CHIS_Sys_FuncDetail { BelongFunctionId=BelongFunctionId.Value });
                    case "NEW": // 更新新增的数据 
                        try
                        {                 
                            var result = _db.Add(model);
                            _db.SaveChanges();
                            return Json(new { state = "success", message = "新增保存成功！" });
                        }
                        catch (Exception ex){return Json(new { state = "error", message = ex.Message });}

                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        if (isNotId(recId)) throw new Exception("不是正确的编号Id格式！");
                        var modelModify = _db.CHIS_Sys_FuncDetail.FirstOrDefault(m => m.FuncDetailId == recId);
                        if (modelModify == null) throw new Exception("没有找到该数据！");
                        ViewBag.OP = "MODIFY";
                        return View(editViewName, modelModify);
                    case "MODIFY": //修改后的数据
                        try
                        {
                            if (model.FuncDetailId <= 0) ModelState.AddModelError("", "记录ID不能为零");
                            _db.Update(model);
                            _db.SaveChanges();                             
                            return Json(new { state = "success", message = "修改保存成功" });
                        }
                        catch (Exception ex){return Json(MyDynamicResult(ex));}

                    case "DELETE": //删除记录
                        try
                        {
                            var del = _db.CHIS_Sys_FuncDetail.Find(recId);
                            _db.Remove(del);
                            _db.SaveChanges(); 
                            return Json(new { state = "success", message = "删除成功！" });
                        }
                        catch (Exception ex) { return Json(new { state = "error", message = ex.Message }); }

                    case "VIEW": //查看 查找出原始实体数据
                        if (isNotId(recId)) throw new Exception("不是正确的编号Id格式！");
                        var modelView = _db.CHIS_Sys_FuncDetail.FirstOrDefault(m => m.FuncDetailId == recId);                     
                        return View(editViewName, modelView);

                    default:
                        throw new Exception("没有发现操作类型");
                }
            }
            catch (Exception ex)
            {
                return View("Error", ex);
            }
        }


        #endregion



    }
}
