using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ah.Models;
using System.Linq;
using System;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.HealthMgr.Controllers
{
    public partial class BackPanelController : BaseController
    {
        public IActionResult BreastArchives(string searchText, int pageIndex = 1)
        {
            var finds = MainDbContext.vwAHMS_HMGR_BreastMgr.AsNoTracking();
            if (!string.IsNullOrEmpty(searchText)) finds = finds.Where(m => m.CustomerMobile == searchText || m.Email == searchText);
            var model = base.PagedList<vwAHMS_HMGR_BreastMgr>(finds.OrderBy(m => m.BreastCreateTime), pageIndex, 20);
            return View(nameof(BreastArchives), model);
        }

        //乳腺癌的判定
        public IActionResult BreastJudge(int breastMgrId)
        {
            var bmgr = MainDbContext.vwAHMS_HMGR_BreastMgr.AsNoTracking().FirstOrDefault(m => m.BreastMgrId == breastMgrId);
            var quesMain = MainDbContext.AHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == bmgr.BreastQuestionId);
            var quesDetails = MainDbContext.vwAHMS_QAFlow_Main_detail.AsNoTracking().Where(m => m.QAFlowMainId == bmgr.BreastQuestionId);
            ViewBag.BreastMgr = bmgr;
            ViewBag.QuestionMain = quesMain;
            ViewBag.QuestionList = quesDetails;
            return View(nameof(BreastJudge));
        }

        //乳腺癌判定设置
        public IActionResult SetBreastJudge(int breastMgrId, int level = 0)
        {
            return TryCatchFunc(() =>
            {
                var model = MainDbContext.AHMS_HMGR_BreastMgr.Find(breastMgrId);
                model.BreastMgrLevel = level;
                model.BreastJudgeTime = DateTime.Now;
                MainDbContext.SaveChanges();
                return null;
            });
        }

        //乳腺癌的干预
        public IActionResult BreastPlan(string searchText, int pageIndex = 1)
        {
            var finds = MainDbContext.vwAHMS_HMGR_BreastMgr.Where(m => m.BreastMgrLevel > 0);
            if (!string.IsNullOrWhiteSpace(searchText)) finds = finds.Where(m => m.CustomerMobile == searchText || m.Email == searchText || m.IDcard == searchText);
            var model = PagedList<vwAHMS_HMGR_BreastMgr>(finds.OrderBy(m => m.BreastMgrId), pageIndex, 20);

            return View(nameof(BreastPlan), model);
        }

        //干预详情
        public IActionResult BreastPlanDetailOfCus(int breastMgrId)
        {
            var bmgr = MainDbContext.vwAHMS_HMGR_BreastMgr.AsNoTracking().FirstOrDefault(m => m.BreastMgrId == breastMgrId);
            ViewBag.BreastMgr = bmgr;
            ViewBag.PlanDetail = GetData_BreastPlanDetails(breastMgrId);
            return View();
        }


        public IActionResult GetBreastPlanDetails(int breastMgrId)
        {
            try
            {
                var finds = GetData_BreastPlanDetails(breastMgrId);
                return Json(new { rlt = true, msg = "获取成功", list = finds });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }
        //获取干预详情
        public IActionResult GetViewBreastPlanDetails(int breastMgrId)
        {
            var finds = GetData_BreastPlanDetails(breastMgrId);
            return PartialView(@"~/Areas/HealthMgr/Views/BackPanel/_BreastPlanDetailOfCus_PlanList.cshtml", finds);
        }

        private dynamic GetData_BreastPlanDetails(int breastMgrId)
        {
            var finds = MainDbContext.vwAHMS_HMGR_BreastMgr_Detail.AsNoTracking().Where(m => m.BreastMgrId == breastMgrId);
            finds = finds.OrderByDescending(m => m.BreastPlanStartTime);
            return finds;
        }


        //添加一条干预详情
        public IActionResult AddBreastPlanOneItem(AHMS_HMGR_BreastMgr_Detail model,bool? isProcNow=false)
        {
            return TryCatchFunc(() =>
            {
                if (model.BreastMgrId == null || model.BreastMgrId == 0) throw new Exception("没有乳腺癌管理编号");
                if (model.BreastPlanStartTime == null) throw new Exception("没有填写执行时间");
                if (string.IsNullOrEmpty(model.BreastPlanContent)) throw new Exception("没有填写执行内容");
                if (model.BreastPlanMainTypeId == null || model.BreastPlanMainTypeId == 0) throw new Exception("没有选择干预类别");

                if (isProcNow == true) model.BreastPlanStartTime = DateTime.Now.AddMinutes(10);
                 


                MainDbContext.Add(model);
                MainDbContext.SaveChanges();
                return null;
            });
        }
        //删除一条记录
        public IActionResult DeleteBreastPlanItem(long planDetailId)
        { 
            return TryCatchFunc(() =>
            {
                var delm = MainDbContext.AHMS_HMGR_BreastMgr_Detail.Find(planDetailId);
                if (delm.BreastPlanIsDeal == true) throw new Exception("已经处理的干预操作，不能删除。");
                MainDbContext.Remove(delm);
                MainDbContext.SaveChanges();
                return null;
            });
        }
        //设置该记录已经处理 
        public IActionResult SetDealBreastPlanItem(long planDetailId)
        {
            return TryCatchFunc(() =>
            {
                var model = MainDbContext.AHMS_HMGR_BreastMgr_Detail.Find(planDetailId);
                model.BreastPlanDealManId = GetCurrentLoginUser.DoctorId;
                model.BreastPlanDealMan = GetCurrentLoginUser.CustomerName;
                model.BreastPlanIsDeal = true;
                model.BreastPlanDealTime = DateTime.Now;
                MainDbContext.SaveChanges();
                return null;
            });
        }




    }
}
