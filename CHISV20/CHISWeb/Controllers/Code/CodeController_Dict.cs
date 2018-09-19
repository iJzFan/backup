using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ass;
using Microsoft.AspNetCore.Mvc.ModelBinding;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    public partial class CodeController
    {
        //初始框架页面
        public async Task<IActionResult> CHIS_Code_Dict_Main()
        {
            return await Task.FromResult(View());
        }

        //获取主表
        public IActionResult CHIS_Code_DictMainItems()
        {
            var lst = _db.CHIS_Code_Dict_Main.AsNoTracking();
            return Json(new { dictMains = lst });
        }


        //编辑主表 页面操作 op=NEWF/NEW/MODIFYF/MODIFY/DELETE 
        public async Task<IActionResult> CHIS_Code_DictMain_Edit(string op, Models.CHIS_Code_Dict_Main model, string recid = null)
        {
            string editViewName = nameof(CHIS_Code_DictMain_Edit);
            var u = UserSelf;//用户   
            if (model != null)
            {
                model.OpID = u.OpId; model.OpMan = u.OpManFullMsg; model.OpTime = DateTime.Now;
            }
            switch (ViewBag.OP = op.ToUpper())
            {
                case "NEWF": //新增页面 空白的数据页面                   
                    return TryCatchFunc(() =>
                    {
                        model = new Models.CHIS_Code_Dict_Main()
                        {
                            IsEnable = true,
                            IsValueCode = false,
                            OpID = u.OpId,
                            OpMan = u.OpMan,
                            OpTime = DateTime.Now
                        };
                        ViewBag.OP = "NEW";
                        return View(editViewName, model);
                    }, bExceptionView: true);
                case "NEW": // 更新新增的数据                  
                    return TryCatchFunc(() =>
                    {
                        //初始化主键 ID
                        model.DictID = _db.CHIS_Code_Dict_Main.Max(m => m.DictID) + 1;
                        model.OpTime = DateTime.Now;
                        if (!ModelState.IsValid) return View(editViewName, model);
                        var rlt = _db.Add(model).Entity;
                        _db.SaveChanges();
                        if (rlt.DictID <= 0) throw new Exception("没有更新成功");
                        return null;
                    });
                case "MODIFYF": //修改 查找出修改的原始实体数据                    
                    return TryCatchFunc(() =>
                    {
                        if (isNotId(recid)) throw new Exception("不是正确的编号Id格式！");
                        var modelmodify = _db.CHIS_Code_Dict_Main.FirstOrDefault(m => m.DictID.ToString() == recid);
                        if (modelmodify == null) throw new Exception("没有找到该数据！");
                        ViewBag.OP = "MODIFY";
                        return View(editViewName, modelmodify);
                    }, bExceptionView: true);
                case "MODIFY": //修改后的数据               
                    return TryCatchFunc(() =>
                    {
                        model.OpTime = DateTime.Now;
                        _db.Update(model);
                        _db.SaveChanges();
                        return null;
                    });
                case "DELETE": //删除，返回json                     
                    return await TryCatchFuncAsync(async () =>
                   {
                       var ids = recid.ToList<int>();//需要删除的Id集合
                        var find = _db.CHIS_Code_Dict_Main.Where(m => ids.Contains(m.DictID));
                       _db.RemoveRange(find);//删除集合
                        _db.SaveChanges();
                       await Global._Initial_Dict();
                       return null;
                   });
                default:
                    return View("Error", new Exception("没有定义页面命令！"));
            }
        }


        //获取从表
        public IActionResult CHIS_Code_DictDetails(int dictParentId = 1, string searchText = null)
        {
            var finds = _db.CHIS_Code_Dict_Detail.Where(m => m.DictID == dictParentId);
            if (!string.IsNullOrEmpty(searchText)) finds = finds.Where(m => m.ItemKey.Contains(searchText) || m.ItemName.Contains(searchText));
            return FindPagedData_jqgrid(finds.OrderBy(m => m.ShowOrder).ThenBy(m => m.DetailID));
        }


        //编辑从表 的页面操作 op=NEWF/NEW/MODIFYF/MODIFY/DELETE 
        public IActionResult CHIS_Code_DictDetail_Edit(
            string op,
            Models.CHIS_Code_Dict_Detail model,
            string recid = null, int? parentId = null)
        {
            string editViewName = nameof(CHIS_Code_DictDetail_Edit);
            var u = UserSelf;

            //定义一个修改排序的过程委托
            Action<Models.CHIS_Code_Dict_Detail> updateShowOrder = (entity) =>
           {
               if (entity.ShowOrder > 0)  //重新排序整理
               {
                   var finds = _db.CHIS_Code_Dict_Detail.Where(m => m.DictID == parentId && m.ShowOrder >= entity.ShowOrder && m.ShowOrder < 1000 && m.DetailID != entity.DetailID).OrderBy(m => m.ShowOrder);
                   int i = entity.ShowOrder.Value + 1;
                   foreach (var item in finds)
                   {
                       item.ShowOrder = i++;
                   }
                   _db.SaveChanges();
               }
           };

            switch (ViewBag.OP = op.ToUpper())
            {
                case "NEWF": //新增页面 空白的数据页面          
                    return TryCatchFunc(() =>
                    {
                        if (!(parentId > 0)) throw new Exception("没有传入主表Id");
                        var parent = _db.CHIS_Code_Dict_Main.AsNoTracking().FirstOrDefault(m => m.DictID == parentId);
                        ViewBag.CHIS_Code_Dict_Main = parent;
                        var modeladd = new Models.CHIS_Code_Dict_Detail
                        {
                            DictID = parent.DictID,
                            IsEnable = true,
                            ItemKey = parent.DictKey + "_",
                            ItemValue = parent.DictKey + "_",
                            ShowOrder = 1000
                        };
                        ViewBag.OP = "NEW";
                        return View(editViewName, modeladd);
                    }, bExceptionView: true);
                case "NEW": // 更新新增的数据                               
                    return TryCatchFunc(() =>
                    {
                        if (!(parentId > 0)) ModelState.AddModelError("", "请输入主表Id");
                        if (string.IsNullOrEmpty(model.ItemKey)) ModelState.AddModelError("", "请输入字典键");
                        if (!(ModelState.IsValid)) { throw new Exception(GetErrorOfModelState(ModelState)); }
                        model.OpID = u.OpId; model.OpMan = u.OpManFullMsg; model.OpTime = DateTime.Now;
                        model.DictID = parentId.Value;
                        model.DetailID = _db.CHIS_Code_Dict_Detail.AsNoTracking().Max(m => m.DetailID) + 1;//设置键值
                        model = _db.Add(model).Entity;
                        _db.SaveChanges();
                        updateShowOrder(model);//重新排序整理                           
                        return null;
                    }, true, success: () => { Global._Initial_Dict(); });
                case "MODIFYF": //修改 查找出修改的原始实体数据                  
                    return TryCatchFunc(() =>
                    {
                        if (string.IsNullOrEmpty(recid)) throw new Exception("没有输入主键Id");
                        var modelmodify = _db.CHIS_Code_Dict_Detail.AsNoTracking().FirstOrDefault(m => m.DetailID.ToString() == recid);
                        var parent = _db.CHIS_Code_Dict_Main.AsNoTracking().FirstOrDefault(m => m.DictID == modelmodify.DictID);
                        ViewBag.CHIS_Code_Dict_Main = parent;
                        ViewBag.OP = "MODIFY";
                        return View(editViewName, modelmodify);
                    }, bExceptionView: true);
                case "MODIFY": //修改后的数据                  
                    return TryCatchFunc(() =>
                    {
                        if (!(parentId > 0)) ModelState.AddModelError("", "请输入主表Id");
                        if (string.IsNullOrEmpty(model.ItemKey)) ModelState.AddModelError("", "请输入字典键");
                        if (!(ModelState.IsValid)) { throw new Exception(GetErrorOfModelState(ModelState)); }
                        model = _db.Update(model).Entity;
                        _db.SaveChanges();
                        updateShowOrder(model);//重新排序整理                           
                        return null;
                    }, bUseTrans: true, success: () => { Global._Initial_Dict(); });
                case "DELETE": //删除，返回json    
                    return TryCatchFunc(() =>
                    {
                        var ids = recid.ToList<int>();
                        var finds = _db.CHIS_Code_Dict_Detail.Where(m => ids.Contains(m.DetailID));
                        _db.RemoveRange(finds);
                        _db.SaveChanges();
                        return null;
                    }, bUseTrans: true);
                case "VIEW":
                    return TryCatchFunc(() =>
                    {
                        var parent = _db.CHIS_Code_Dict_Main.AsNoTracking().FirstOrDefault(m => m.DictID == parentId);
                        var modelmodify = _db.CHIS_Code_Dict_Detail.AsNoTracking().FirstOrDefault(m => m.DetailID.ToString() == recid);
                        ViewBag.CHIS_Code_Dict_Main = parent;
                        return View(editViewName, modelmodify);
                    });
                default:
                    return View("Error", new Exception("没有定义页面命令！"));
            }

        }

    }
}
