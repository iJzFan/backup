using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ah.Areas.HealthMgr.Controllers;
using Microsoft.AspNetCore.Authorization;
using ah.Models.ViewModel;
using System.Security.Claims;
using ah.Models.DataModel;
using ah.Models;
using Microsoft.EntityFrameworkCore;
using Ass.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.HealthMgr.Controllers
{
    public partial class BackPanelController : BaseController
    {
        [AllowAnonymous]
        public IActionResult Customers(string searchText, int pageIndex = 1)
        {

            var finds = MainDbContext.vwCHIS_Code_Customer.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();
                finds = finds.Where(m => m.Telephone == searchText || m.CustomerMobile == searchText || m.Email == searchText || m.IDcard == searchText);
            }

            Ass.Mvc.PageListInfo<ah.Models.vwCHIS_Code_Customer> model = PagedList(finds.OrderByDescending(m => m.CustomerCreateDate), pageIndex, 20);
            return View(nameof(Customers), model);
        }

        //用户详情
        [AllowAnonymous]
        public IActionResult CustomersInfo(int cid)
        {
            var model = MainDbContext.vwCHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == cid);
            return View(nameof(CustomersInfo),model);
        }

        [AllowAnonymous]
        public IActionResult CustomerDaily(string searchText, int pageIndex = 1)
        {

            var finds = MainDbContext.vwAHMS_Daily_Member.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();
                finds = finds.Where(m => m.Mobile == searchText || m.Email == searchText || m.MemberName.Contains(searchText));
            }

            Ass.Mvc.PageListInfo<ah.Models.vwAHMS_Daily_Member> model = PagedList(finds.OrderBy(m => m.CreateTime), pageIndex, 20);
            return View(nameof(CustomerDaily), model);
        }

        public IActionResult MemberSetOfficalCustomer(long mid)
        {
            var member = MainDbContext.AHMS_Daily_Member.Find(mid);
            var cus = MainDbContext.vwCHIS_Code_Customer.Where(m => m.Email == member.Email || m.CustomerMobile == member.Mobile);

            ViewBag.MId = mid;
            return View(cus);
        }

        public IActionResult DeleteDailyMember(int mid) // 删除一个日常用户数据
        {
            return CustomerDaily("");
        }


        [AllowAnonymous]
        public IActionResult QuestionList(string searchText, int pageIndex = 1)
        {
            var finds = MainDbContext.vwAHMS_QAFlow_Main.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();
                finds = finds.Where(m => m.Mobile == searchText || m.Email == searchText || m.Name.Contains(searchText));
            }

            Ass.Mvc.PageListInfo<ah.Models.vwAHMS_QAFlow_Main> model = PagedList(finds.OrderBy(m => m.QATime), pageIndex, 20);
            Response.Cookies.Append("searchText", Ass.P.PStr(searchText));
            Response.Cookies.Append("pageIndex", Ass.P.PStr(pageIndex));
            return View(nameof(QuestionList), model);
        }


        //问卷重新加入到客户管理中去
        public IActionResult AddQuestionToMgr(Guid qid)
        {
            var find = MainDbContext.AHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == qid);
            if (find.CustomerId > 0)
            {
                //检查乳腺癌
                var doc = MainDbContext.vwAHMS_Center_HealthDoc.FirstOrDefault(m => m.CustomerId == find.CustomerId);
                if (doc == null)
                {
                    //添加乳腺癌的管理器
                    var breastMgrAdd = MainDbContext.AHMS_HMGR_BreastMgr.Add(new AHMS_HMGR_BreastMgr
                    {
                        CustomerId = find.CustomerId,
                        BreastQuestionId = qid,
                        BreastCreateTime = DateTime.Now
                    }).Entity;
                    MainDbContext.SaveChanges();
                    //添加一个健康档案
                    MainDbContext.AHMS_Center_HealthDoc.Add(new AHMS_Center_HealthDoc
                    {
                        CustomerId = find.CustomerId,
                        BreastMgrId=breastMgrAdd.BreastMgrId
                    });
                    MainDbContext.SaveChanges();
                    //返回到列表清单
                    return RedirectToAction("QuestionList", new { pageIndex = Ass.P.PIntV(Request.Cookies["pageIndex"], 1), searchText = Ass.P.PStr(Request.Cookies["searchText"]) });
                }

                if (doc.BreastQuestionId != null)
                {
                    ViewBag.UseBreastQues = MainDbContext.vwAHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == doc.BreastQuestionId);
                    ViewBag.ReplaceBreastQues = MainDbContext.vwAHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == qid);
                    return View("QuestionList_check");
                }
                else
                {
                    //添加乳腺癌的管理器
                    var breastMgrAdd = MainDbContext.AHMS_HMGR_BreastMgr.Add(new AHMS_HMGR_BreastMgr
                    {
                        CustomerId = find.CustomerId,
                        BreastQuestionId = qid,
                        BreastCreateTime = DateTime.Now
                    }).Entity;
                    MainDbContext.SaveChanges();
                    MainDbContext.AHMS_Center_HealthDoc.Find(doc.HeathDocId).BreastMgrId = breastMgrAdd.BreastMgrId;
                    MainDbContext.SaveChanges();
                    //返回到列表清单
                    return RedirectToAction("QuestionList", new { pageIndex = Ass.P.PIntV(Request.Cookies["pageIndex"], 1), searchText = Ass.P.PStr(Request.Cookies["searchText"]) });
                }
            }


            return null;
        }

        //使用新问卷
        public IActionResult UseNewQues(Guid qid)
        {
            var qm = MainDbContext.AHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == qid);
            var breastMgr = MainDbContext.AHMS_HMGR_BreastMgr.FirstOrDefault(m => m.CustomerId == qm.CustomerId);
            breastMgr.BreastQuestionId = qid;
            MainDbContext.SaveChanges();
            return RedirectToAction("QuestionList", new { pageIndex = Ass.P.PIntV(Request.Cookies["pageIndex"], 1), searchText = Ass.P.PStr(Request.Cookies["searchText"]) });
        }
    }
}
