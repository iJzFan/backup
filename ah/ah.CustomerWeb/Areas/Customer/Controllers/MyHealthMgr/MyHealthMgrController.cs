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
        public MyHealthMgrController(ah.DbContext.AHMSEntitiesSqlServer db) : base(db) { }

        public ahWeb.Api.CustomerHeathMgr _api
        {
            get
            {
                return new ahWeb.Api.CustomerHeathMgr(_db);
            }
        }

        public IActionResult Index()
        {
            return View();
        }


        #region 乳腺癌健康管理
        /// <summary>
        /// 乳腺癌健康管理摘要
        /// </summary>
        /// <returns></returns>
        public IActionResult BreastSumary()
        {
            var model = _api.GetBreastMgrSumery(GetCurrentLoginUser.CustomerId);
            return View(nameof(BreastSumary), model);
        }

        //获取问卷详情
        public IActionResult QuestionDetails(Guid qid)
        {
            var model = _api.GetQuestionDetails(qid);
            return View(model);
        }

        #endregion

        public IActionResult MyQuestions(int pageIndex = 1)
        {
            var model = _api.MyQuestions(GetCurrentLoginUser.CustomerId, pageIndex);
            return View(model);
        }






    }
}
