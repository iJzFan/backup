using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers.Sys
{
    //[AllowAnonymous]
    public class AHConfigController : BaseController
    {
        public AHConfigController(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        // GET: /<controller>/
        public IActionResult AHConfig()
        {
            return View("~/Views/Sys/AHConfig/AHConfig.cshtml");
        }

        public IActionResult GetGridJson_AH_Config(string keyWord)
        {
            //jqgrid公共取值
            //  var u = base.GetCurrentUserInfo();
            int pageIndex = 1, pageSize = 0; string sort = "";
            base.getJqGridInfo(out pageIndex, out pageSize, out sort, 1, 20);
            if (string.IsNullOrWhiteSpace(sort)) sort = getForm("sort");
            try
            {
                var find = from item in _db.AH_Code_Config
                           where (string.IsNullOrEmpty(keyWord) ? true :
                           item.CfgKey.Contains(keyWord))
                           select item;

                int findTotal = find.Count();
                int totalPage = (int)Math.Ceiling(findTotal * 1.0f / pageSize);

                //排序获取当前页的数据  
                var dataList = find.OrderBy(m => m.ConfigId).
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




        public IActionResult AH_Config_Edit(string op, Models.AH_Code_Config model, int recId)
        {
            var user = UserSelf;
            //todo
            try
            {
                string editViewName = "~/Views/Sys/AHConfig/AHConfigEdit.cshtml";
                //   var sysUser = base.GetCurrentUserInfo();
                ViewBag.OP = op;// 初始化操作类别             
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面
                        var modelnew = new Models.AH_Code_Config(){};
                        
                        ViewBag.OP = "NEW";
                        return View(editViewName, modelnew);
                    case "NEW": // 更新新增的数据

                        model = new Models.AH_Code_Config(){
                            CfgSec = model.CfgSec,
                            CfgKey = model.CfgKey,
                            CfgVal = model.CfgVal,
                            IsEnable = model.IsEnable
                        };
                        _db.Add(model);
                        _db.SaveChanges();
                        ViewBag.OP = "NEW";
                        return Json(new { state = "success", message = "添加成功" });

                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        var modelmodifyCfg = _db.AH_Code_Config.Find(recId);
                        ViewBag.OP = "MODIFY";
                        return View(editViewName, modelmodifyCfg);
                    case "MODIFY": //修改后的数据
                        var mdf = _db.AH_Code_Config.FirstOrDefault(m => m.ConfigId == model.ConfigId);
                        if (mdf == null) throw new Exception("没有该数据!");
                        mdf.CfgSec = model.CfgSec;
                        mdf.CfgKey = model.CfgKey;
                        mdf.CfgVal = model.CfgVal;
                        mdf.IsEnable = model.IsEnable;
                        _db.Update(mdf);
                        _db.SaveChanges();
                        return Json(new { state = "success", message = "保存成功" });
                    case "DELETE": //删除，返回json 
                        try
                        {
                            var d = _db.AH_Code_Config.FirstOrDefault(m => m.ConfigId == recId);
                            _db.Remove<Models.AH_Code_Config>(d);
                            _db.SaveChanges();
                            return Json(new { state = "success", message = "删除成功！" });
                        }
                        catch (Exception ex) { return Json(new { state = "error", message = ex.Message }); }
                    case "VIEW":
                        var md = new Models.AH_Code_Config();
                        var viewed = _db.AH_Code_Config.FirstOrDefault(m => m.ConfigId == recId);
                        return View(editViewName, viewed);
                    default:
                        throw new Exception("错误的命令");
                }
            }
            catch (Exception ex)
            {
                // Loger.WriteError("Code", "CHIS_Code_EmployeeMsg_Edit", ex);
                return View("Error", ex);
            }

        }



    }
}
