using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
//using CHIS.BLL.Sys;
using CHIS.Code;
using CHIS.Models;
using System.Linq;
using CHIS.Codes.Utility;
using System.Collections.Generic;
using Ass;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers.Sys
{

    public partial class RoleController : BaseController
    {
        public RoleController(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        /// <summary>
        /// 业务功能页面入口
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            //Loger.WriteInfo("Role", "RoleIndex", $"查看系统角色设置。");
            return View("~/Views/Sys/Roles/RoleIndex.cshtml");
        }

        /// <summary>
        /// 业务功能网格记录查询
        /// </summary>
        /// <param name="keyWord">搜索关键字</param>
        /// <returns></returns>
        public IActionResult GetGridJson_Sys_Role(string keyWord)
        {
            //jqgrid公共取值
            //  var u = base.GetCurrentUserInfo();
            int pageIndex = 1, pageSize = 0; string sort = "";
            base.getJqGridInfo(out pageIndex, out pageSize, out sort, 1, 20);
            if (string.IsNullOrWhiteSpace(sort)) sort = getForm("sort");
            try
            {
                var find = from item in _db.CHIS_SYS_Role
                           where (string.IsNullOrEmpty(keyWord) ? true :
                           item.RoleName.Contains(keyWord) || item.Remark.Contains(keyWord))
                           select item;

                int findTotal = find.Count();
                int totalPage = (int)Math.Ceiling(findTotal * 1.0f / pageSize);

                //排序获取当前页的数据  
                var dataList = find.OrderBy(m => m.RoleKey).
                          Skip(pageSize * (pageIndex - 1)).
                          Take(pageSize).AsQueryable().ToList();

                return Json(new
                {
                    page = pageIndex,
                    total = totalPage,
                    records = findTotal,
                    rows = dataList
                });
            }
            catch (Exception ex)
            {
                // Loger.WriteError("Role", "GetGridJson_Role", ex);
                return View("ErrorBlank", ex);
            }
        }


        /// <summary>
        /// 编辑页面操作
        /// </summary>
        /// <param name="op">op=NEWF/NEW/MODIFYF/MODIFY/DELETE </param>
        /// <param name="model">数据模型</param>
        /// <param name="functionIds">功能权限字符串</param>
        /// <param name="recID">记录ID</param>
        /// <returns></returns>
        public IActionResult CHIS_SYS_Role_Edit(string op,
            CHIS.Models.CHIS_SYS_Role model, string functionIds, int recID = 0, List<RoleFuncValue> roleFuncValue = null)
        {
            string editViewName = "~/Views/Sys/Roles/RoleForm.cshtml";
            try
            {
                var user = base.UserSelf;// base.GetCurrentUserInfo();

                ViewBag.OP = op;// 初始化操作类别               
                IEnumerable<int> functionRight = functionIds.ToList<int>();
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面
                        model = new Models.CHIS_SYS_Role()
                        {
                            IsEnable = true,
                            OpID = user.OpId,
                            OpMan = user.OpManFullMsg,
                            OpTime = DateTime.Now
                        };
                        //获取角色详细内容
                        ViewBag.RoleFuncDetails = _db.SqlQuery<CHIS.Models.DataModel.sp_Sys_GetRoleFuncDetailsByRoleId>($"exec sp_Sys_GetRoleFuncDetailsByRoleId {recID}");
                        ViewBag.OP = "NEW";
                        return View(editViewName, model);

                    case "NEW": // 更新新增的数据 
                        try
                        {
                            model.OpID = user.OpId;
                            model.OpMan = user.OpManFullMsg;
                            model.OpTime = DateTime.Now;

                            using (var trans = _db.Database.BeginTransaction())
                            {
                                try
                                {
                                    var result = _db.Add<Models.CHIS_SYS_Role>(model).Entity;
                                    _db.SaveChanges();
                                    if (result.OpID > 0)
                                    {
                                        UpdateRoleFunctionRight(result.RoleID, functionRight);
                                    }
                                    _db.SaveChanges();
                                    trans.Commit();
                                    // Loger.WriteInfo("Role", editViewName, $"新增系统角色设置，记录ID{result.RoleID}");
                                    return Json(new { state = "success", message = "保存成功！" });
                                }
                                catch (Exception ex)
                                {
                                    trans.Rollback(); throw ex;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Loger.WriteError("Role", editViewName, ex);
                            return Json(new { state = "error", message = ex.Message });
                        }

                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        if (isNotId(recID)) throw new Exception("不是正确的编号Id格式！");
                        var modelModify = _db.CHIS_SYS_Role.FirstOrDefault(m => m.RoleID == recID);
                        if (modelModify == null) throw new Exception("没有找到该数据！");
                        //获取角色详细内容
                        ViewBag.RoleFuncDetails = _db.SqlQuery<CHIS.Models.DataModel.sp_Sys_GetRoleFuncDetailsByRoleId>($"exec sp_Sys_GetRoleFuncDetailsByRoleId {recID}");
                        ViewBag.OP = "MODIFY";
                        return View(editViewName, modelModify);

                    case "MODIFY": //修改后的数据
                        try
                        {
                            if (model.RoleID <= 0) ModelState.AddModelError("", "记录ID不能为零");
                            model.OpID = user.OpId;
                            model.OpMan = user.OpMan;
                            model.OpTime = DateTime.Now;

                            using (var trans = _db.Database.BeginTransaction())
                            {
                                try
                                {
                                    var result = _db.Update<Models.CHIS_SYS_Role>(model).State == EntityState.Modified;
                                    _db.SaveChanges();
                                    if (result)
                                    {
                                        UpdateRoleFunctionRight(model.RoleID, functionRight);
                                        UpdateRoleDetails(model.RoleID, roleFuncValue);
                                        _db.SaveChanges();

                                        trans.Commit();
                                        //  Loger.WriteInfo("Role", editViewName, $"修改系统角色设置，记录ID{model.RoleID}");
                                        return Json(new { state = "success", message = "保存成功" });
                                    }
                                    else
                                    {
                                        View(editViewName, model);
                                        throw new Exception("未知命令");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    trans.Rollback(); throw ex;
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            //    Loger.WriteError("Role", editViewName, ex);
                            return Json(new { state = "error", message = ex.Message });
                        }

                    case "DELETE": //删除记录
                        try
                        {
                            if (recID == 1) throw new Exception("系统总管理员角色，不可删除！");
                            using (var trans = _db.Database.BeginTransaction())
                            {
                                try
                                {
                                    var del0 = _db.CHIS_SYS_Role.AsNoTracking().Where(m => m.RoleID == recID);
                                    _db.RemoveRange(del0);
                                    var del1 = _db.CHIS_Sys_Rel_RoleFunctions.AsNoTracking().Where(m => m.RoleId == recID);
                                    _db.RemoveRange(del1);
                                    _db.SaveChanges();
                                    trans.Commit();
                                }
                                catch
                                {
                                    trans.Rollback();
                                    throw new Exception("删除记录出现错误！");
                                }
                            }
                            // Loger.WriteInfo("Role", editViewName, $"删除系统角色设置，记录ID:{recID}");
                            return Json(new { state = "success", message = "删除成功！" });
                        }
                        catch (Exception ex) { return Json(new { state = "error", message = ex.Message }); }

                    case "VIEW": //查看 查找出原始实体数据
                        if (isNotId(recID)) throw new Exception("不是正确的编号Id格式！");
                        var modelView = _db.CHIS_SYS_Role.FirstOrDefault(m => m.RoleID == recID);
                        if (modelView == null) throw new Exception("没有找到该数据！");
                        ViewBag.RoleFuncDetails = _db.SqlQuery<CHIS.Models.DataModel.sp_Sys_GetRoleFuncDetailsByRoleId>($"exec sp_Sys_GetRoleFuncDetailsByRoleId {recID}");
                        return View(editViewName, modelView);

                    default:
                        throw new Exception("未知的命令");
                }
            }
            catch (Exception ex)
            {
                //    Loger.WriteError("Role", editViewName, ex);
                return View("ErrorBlank", ex);
            }
        }

        //更新角色的详细功能
        private void UpdateRoleDetails(int roleId, List<RoleFuncValue> roleFuncValue)
        {
            if (roleFuncValue != null && roleFuncValue.Count > 0)
            {
                var finds = _db.CHIS_Sys_Rel_RoleFuncDetails.AsNoTracking().Where(m => m.RoleId == roleId).ToList();                
                foreach (var item in roleFuncValue)
                {
                    var have = finds.FirstOrDefault(m => m.FuncDetailId == item.funcDetailId);
                    if (have == null) //如果不存在
                    {
                        _db.CHIS_Sys_Rel_RoleFuncDetails.Add(new CHIS_Sys_Rel_RoleFuncDetails
                        {
                            RoleId = roleId,
                            FuncDetailId = item.funcDetailId,
                            SetValue = item.val
                        });                        
                    }
                    else if (have != null && have.SetValue == item.val) continue;
                    else
                    {
                        have.SetValue = item.val;
                        _db.CHIS_Sys_Rel_RoleFuncDetails.Update(have);                        
                    }
                }
            }
            //随后删除不属于角色的功能项
            var myFunctionIds = _db.CHIS_Sys_Rel_RoleFunctions.AsNoTracking().Where(m => m.RoleId == roleId).Select(m => m.FunctionId).ToList();
            var myFuncDetailIds = _db.CHIS_Sys_FuncDetail.AsNoTracking().Where(m => myFunctionIds.Contains(m.BelongFunctionId)).Select(m => m.FuncDetailId).ToList();
            var deletes = from item in _db.CHIS_Sys_Rel_RoleFuncDetails.AsNoTracking()
                    where item.RoleId == roleId && !myFuncDetailIds.Contains(item.FuncDetailId ?? 0)
                    select item;
            _db.CHIS_Sys_Rel_RoleFuncDetails.RemoveRange(deletes);
            _db.SaveChanges();
        }

        /// <summary>
        /// 保存角色业务功能权限信息
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="newRight">选定业务功能集合</param>
        /// <returns></returns>
        private bool UpdateRoleFunctionRight(int roleId, IEnumerable<int> newRight)
        {
            var user = base.UserSelf;// GetCurrentUserInfo();
            var find = _db.CHIS_Sys_Rel_RoleFunctions.Where(m => m.RoleId == roleId);
            var oldRight = from item in find select item.FunctionId;  //原业务功能元素列表
            foreach (var item in find)
            {
                if (newRight.Contains(item.FunctionId)) continue;
                else
                {
                    bool brlt = _db.Remove<Models.CHIS_Sys_Rel_RoleFunctions>(item).State == Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
            }

            foreach (var functionId in newRight)
            {
                if (oldRight.Contains(functionId)) continue;
                else
                    _db.Add(new Models.CHIS_Sys_Rel_RoleFunctions
                    {
                        RoleId = roleId,
                        FunctionId = functionId,
                        OpID = user.OpId,
                        OpMan = user.OpManFullMsg,
                        OpTime = DateTime.Now
                    });
            }

            return true;
        }


        #region 获取角色允许的功能权限树
        //获取角色允许的功能权限树
        public ActionResult GetPermissionTree(int roleId)
        {
            var authorizedata = _db.CHIS_Sys_Rel_RoleFunctions.Where(m => m.RoleId == roleId);
            var funcs = _db.CHIS_SYS_Function.Where(m => m.IsEnable == true && m.IsRight == true && m.IsV20 == true).ToList();
            var rlt = funcs.Select(m => new RoleTreeData
            {
                id = m.FunctionID.ToString(),
                text = m.FunctionName,
                value = m.FunctionKey,
                parentId = (m.ParentFunctionID ?? 0).ToString(),
                img = m.Icon,
                checkstate = authorizedata.Any(t => t.FunctionId == m.FunctionID) ? 1 : 0,
                hasChildren = funcs.Any(t => t.ParentFunctionID == m.FunctionID),
                isexpand = true,
                complete = true,
                showcheck = true
            }).ToList();
            var rtn = getRoleTreeData(rlt, new RoleTreeData());
            return Json(rtn.ChildNodes);
        }
        //递归获取
        RoleTreeData getRoleTreeData(List<RoleTreeData> datas, RoleTreeData nowTreeNode, string parentId = "0")
        {
            nowTreeNode.ChildNodes = datas.Where(m => m.parentId == parentId).ToList();
            foreach (var mm in nowTreeNode.ChildNodes)
            {
                getRoleTreeData(datas, mm, mm.id);
            }
            return nowTreeNode;
        }
        #endregion

    }
    class RoleTreeData
    {
        public List<RoleTreeData> ChildNodes { get; set; } = new List<RoleTreeData>();
        public string id { get; set; }
        public string parentId { get; set; }
        public string text { get; set; }

        public string value { get; set; }

        public bool isexpand { get; set; } = true;
        public bool complete { get; set; } = true;
        public bool showcheck { get; set; } = true;
        public int checkstate { get; set; }
        public bool hasChildren { get; set; }
        public string img { get; set; }
    }

    public class RoleFuncValue
    {
        public int funcDetailId { get; set; }
        public string val { get; set; }
    }
}
