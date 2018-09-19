using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass;
using ah.Areas.Customer.Controllers.Base;
using ah.Models;
using ah.DbContext;

namespace ahWeb.Api
{
    /// <summary>
    /// 用户健康管理数据
    /// </summary>
    [AllowAnonymous]
    public class CustomerHeathMgr : BaseDBController
    {
        public CustomerHeathMgr(AHMSEntitiesSqlServer db) : base(db) { }
        /// <summary>
        /// 获取我的问卷列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public Ass.Mvc.PageListInfo<vwAHMS_QAFlow_Main> MyQuestions(int customerId, int pageIndex = 1)
        {
            var finds = MainDbContext.vwAHMS_QAFlow_Main.Where(m => m.CustomerId == customerId);
            return PagedList<vwAHMS_QAFlow_Main>(finds.OrderByDescending(m => m.QATime), pageIndex, 20);
        }
        /// <summary>
        /// 获取问卷的详细信息
        /// </summary>
        /// <param name="qid"></param>
        /// <returns></returns>
        public ah.Models.ViewModel.vwQuestionaireModel GetQuestionDetails(Guid qid)
        {
            var main = MainDbContext.vwAHMS_QAFlow_Main.AsNoTracking().FirstOrDefault(m => m.QAFlowMainId == qid);
            var details = MainDbContext.vwAHMS_QAFlow_Main_detail.AsNoTracking().Where(m => m.QAFlowMainId == qid);
            return new ah.Models.ViewModel.vwQuestionaireModel
            {
                QAMain = main,
                QADetails = details
            };
        }
        /// <summary>
        /// 获取乳腺癌当前管理信息
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public ah.Models.ViewModel.BreastMgrSumery GetBreastMgrSumery(int customerId)
        {
            var center = MyHealthMgrCenter(customerId);            
            var bmgr = MainDbContext.vwAHMS_HMGR_BreastMgr.AsNoTracking().FirstOrDefault(m=>m.BreastMgrId==center.BreastMgrId);
            var bmgrdetail = MainDbContext.vwAHMS_HMGR_BreastMgr_Detail.AsNoTracking().Where(m => m.BreastMgrId == bmgr.BreastMgrId).OrderByDescending(m=>m.BreastPlanStartTime).Take(20);

            return new ah.Models.ViewModel.BreastMgrSumery
            {
                BreastMgrMain = bmgr,
                BreastMgrDetails=bmgrdetail
            };
        }



        /// <summary>
        /// 获取管理中心的管理信息
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public vwAHMS_Center_HealthDoc MyHealthMgrCenter(int customerId)
        {
            var find= MainDbContext.vwAHMS_Center_HealthDoc.FirstOrDefault(m => m.CustomerId == customerId);
            return find;
        }

    }
}