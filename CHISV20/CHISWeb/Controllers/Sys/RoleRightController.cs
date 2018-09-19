
//using CHIS.BLL.Sys;
//using CHIS.Code;
//using CHIS.Models;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.AspNetCore.Mvc;

//namespace CHIS.Controllers.Sys
//{
//    public class RoleRightController : BaseController
//    {
//        private RoleRightBLL roleRightBLL = new RoleRightBLL();
//        private FunctionBLL functionBLL = new FunctionBLL();
//        private MenuControlBLL menuControlBLL = new MenuControlBLL();

//        public ActionResult GetPermissionTree(string roleId)
//        {
//            var moduledata = functionBLL.GetList();
//            moduledata = moduledata.Where(m => m.IsEnable && m.IsMenu && m.IsRight).ToList();
//            var buttondata = menuControlBLL.GetList();
//            var authorizedata = new List<CHIS_Sys_Rel_RoleFunctions>();
//            if (!string.IsNullOrEmpty(roleId))
//            {
//                authorizedata = roleRightBLL.GetList(int.Parse(roleId));
//            }
//            var treeList = new List<TreeViewModel>();
//            foreach (CHIS_SYS_Function item in moduledata)
//            {
//                TreeViewModel tree = new TreeViewModel();
//                bool hasChildren = moduledata.Count(t => t.ParentFunctionID == item.FunctionID) == 0 ? false : true;
//                tree.id = item.FunctionID.ToString();
//                tree.text = item.FunctionName;
//                tree.value = item.FunctionKey;
//                tree.parentId = item.ParentFunctionID.ToString();
//                tree.isexpand = true;
//                tree.complete = true;
//                tree.showcheck = true;
//                tree.checkstate = authorizedata.Count(t => t.FunctionId == item.FunctionID);
//                tree.hasChildren = true;
//                tree.img = item.Icon == "" ? "" : item.Icon;
//                treeList.Add(tree);
//            }
//            foreach (CHIS_SYS_MenuControl item in buttondata)
//            {
//                TreeViewModel tree = new TreeViewModel();
//                bool hasChildren = buttondata.Count(t => t.ParentMenuID == item.MenuID) == 0 ? false : true;
//                string parentMenuID = item.ParentMenuID.ToString() == "" ? "0" : "0";//按钮父级，空值转为0
//                tree.id = item.MenuID.ToString();
//                tree.text = item.MenName;
//                tree.value = item.MenuCode;
//                tree.parentId = parentMenuID == "0" ? item.FunctionID.ToString() : item.ParentMenuID.ToString();
//                tree.isexpand = true;
//                tree.complete = true;
//                tree.showcheck = true;
//                tree.checkstate = authorizedata.Count(t => t.FunctionId == item.MenuID);
//                tree.hasChildren = hasChildren;
//                tree.img = item.Icon == "" ? "" : item.Icon;
//                treeList.Add(tree);
//            }
//            return Content(treeList.TreeViewJson());
//        }
//    }
//}
