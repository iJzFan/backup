using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ah.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Authorization;
using ah.Models.ViewModel;
using System.Security.Claims;
using ah.Models.DataModel;
using ah.Models;
using ah.Areas.Customer.Controllers.Base;
using Microsoft.EntityFrameworkCore;
using Ass;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class MyHealthMgrController : BaseController
    {

        #region 健康记录Index
        public IActionResult HealthBookIndex()
        {
            return View();
        }

#endregion

        #region Allergic 过敏记录
        public IActionResult AllergyRecord(ah.Models.AHMS_Customer_AllergicHistory model = null)
        {

            ViewBag.AlleryRecords = GetAlleryRecords();
            ViewBag.MyAllergics = MainDbContext.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == GetCurrentLoginUser.CustomerId).Allergic;
            return View(nameof(AllergyRecord), model);
        }

        public IActionResult AllergyRecordList()
        {
            var model = GetAlleryRecords();
            return PartialView("_AllergyRecord_History", model);
        }
        private dynamic GetAlleryRecords()
        {
            var model = MainDbContext.vwAHMS_Customer_AllergicHistory.AsNoTracking().Where(m => m.CustomerId == GetCurrentLoginUser.CustomerId).OrderByDescending(m => m.AllergicDate);

            //List<string> l = new List<string>();
            //var top20 = model.Take(100).Select(m => m.Allergens);
            //foreach (var item in top20) l.AddRange(item.Split(',', '、', '，', '。', '.'));
            //myAllergics = DistinctFreq(l);
            return model;
        }

        private string DistinctFreq(IEnumerable<string> list)
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            foreach (string key in list)
            {
                if (d.ContainsKey(key)) d[key] = d[key] + 1;
                else d.Add(key, 1);
            }
            var lst = d.OrderByDescending(m => m.Value).Select(m => m.Key);
            return string.Join(",", lst);
        }

        public IActionResult UpsertAllergyRecord(ah.Models.AHMS_Customer_AllergicHistory model, List<string> AllergicBodyPartList)
        {
            try
            {
                if (model.CustomerId == null || model.CustomerId == 0) model.CustomerId = GetCurrentLoginUser.CustomerId;
                if (model.RecMan.IsEmpty()) model.RecMan = GetCurrentLoginUser.CustomerName;
                model.AllergicBodyParts = string.Join(",", AllergicBodyPartList.Where(m => !string.IsNullOrWhiteSpace(m)));
                if (model.AllergicBodyParts.IsEmpty()) throw new Exception("没有选择过敏部位");
                if (model.AllergicHistoryId == 0)
                {
                    MainDbContext.Add(model);
                    MainDbContext.SaveChanges();
                }
                else
                {
                    MainDbContext.Update(model);
                    MainDbContext.SaveChanges();
                }
                MainDbContext.Database.ExecuteSqlCommand($"exec sp_Update_Allergic {model.CustomerId}");//更新
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); }
            return AllergyRecord();
        }
        public IActionResult DeleteAllergyRecord(long id)
        {
            return TryCatchFunc(() =>
            {
                var d = MainDbContext.AHMS_Customer_AllergicHistory.Find(id);
                MainDbContext.Remove(d);
                MainDbContext.SaveChanges();
                MainDbContext.Database.ExecuteSqlCommand($"exec sp_Update_Allergic {d.CustomerId}");//更新
                return null;
            });
        }

#endregion
    }
}
