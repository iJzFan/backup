
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

        public IActionResult LoginManage()
        {
            return View();
        }

        public JsonResult LoginList(string keyword = null,bool isOnlyDoctor=false)
        {
            var find = _db.vwCHIS_Sys_Login.AsNoTracking().Where(m => string.IsNullOrEmpty(keyword) ? true : m.Mobile == keyword || m.IdCardNumber == keyword||m.Email==keyword||m.CustomerName==keyword);
            if (isOnlyDoctor) find = find.Where(m => m.DoctorId > 0);
            return FindPagedData_jqgrid(find.OrderBy(m=>m.LoginId));
        }


        public IActionResult LoginManage_Edit(string op, CHIS.Models.CHIS_Sys_Login model, long recId)
        {
            string editViewName = nameof(LoginManage_Edit);
            //   var sysUser = base.GetCurrentUserInfo();
            ViewBag.OP = op;// 初始化操作类别            
        
            switch (op.ToUpper())
            {
                case "NEWF": //新增页面 空白的数据页面
                    var modelnew = new Models.CHIS_Sys_Login() { };
                    ViewBag.OP = "NEW";
                    return View(editViewName, modelnew);
                case "NEW": // 更新新增的数据
                    try
                    {                        
                        _db.Add(model);
                        _db.SaveChanges();
                        return Json(new { state = "success" });
                    }
                    catch (Exception ex) { return Json(new { state = "error", msg = ex.Message }); }
                case "MODIFYF": //修改 查找出修改的原始实体数据
                    var modelmodify = _db.CHIS_Sys_Login.Find(recId);
                    ViewBag.OP = "MODIFY";
                    return View(editViewName, modelmodify);
                case "MODIFY": //修改后的数据
                    try
                    {
                        _db.Attach(model).State=EntityState.Modified;                       
                        //设置不更新的部分
                        if (string.IsNullOrWhiteSpace(model.LoginPassword))
                        {
                            _db.Entry(model).Property(m => m.LoginPassword).IsModified = false;//不更新此字段
                        }             
                        _db.SaveChanges();
                        return Json(new { state = "success" });
                    }
                    catch (Exception ex) { return Json(new { state = "error", msg = "修改失败" + ex.Message }); }
                case "DELETE": //删除，返回json 
                    var del = _db.CHIS_Sys_Login.Find(recId);
                    _db.Remove(del);
                    var rlt = _db.SaveChanges() > 0;
                    return Json(rlt);
                case "VIEW":
                    return View(editViewName, _db.CHIS_Sys_Login.Find(recId));
                default:
                    throw new Exception("错误的命令");
            }
        }




    }
}

