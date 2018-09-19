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

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.HealthMgr.Controllers
{
    public partial class BreastController : BaseController
    {

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Questionnaire(Guid? qid)
        {
            ah.Models.ViewModel.QuestionaireViewModel model = new Models.ViewModel.QuestionaireViewModel();
            if (qid.HasValue)
            {
                var qmain = MainDbContext.AHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == qid);
                var qdetail = MainDbContext.AHMS_QAFlow_Main_detail.Where(m => m.QAFlowMainId == qid);
                model.QAMain = qmain;
                model.QADetails = qdetail;
            }
            else
            {
                model.QAMain = new Models.AHMS_QAFlow_Main()
                {
                    QANameKey = "RXC",
                    //todo:后续可以将默认问卷版本号放在数据库里
                    QAVer = "1.0",//默认版本号 
                    QAFlowMainId = Guid.NewGuid(),
                    QATime = DateTime.Now
                };
            }
            return View(nameof(Questionnaire) + "_" + model.QAMain.QAVer, model);
        }

        [AllowAnonymous]
        public IActionResult QuestionaireSave(ah.Models.ViewModel.QuestionaireViewModel model, IEnumerable<Ass.KeyString> answers)
        {
            try
            {
                if (answers.Count() == 0 || answers.Any(m => string.IsNullOrWhiteSpace(m.String))) throw new Exception("请填写完整问卷");
                if (model.TotalQuesNum != answers.Count()) throw new Exception("问卷总题数错误");

                using (var tx = MainDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //保存问卷
                        var qamain = MainDbContext.AHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == model.QAMain.QAFlowMainId);
                        if (qamain == null)
                        {
                            qamain = model.QAMain;
                            var qamainadd = MainDbContext.Add(qamain).Entity;
                            MainDbContext.AHMS_Daily_Member.Add(new Models.AHMS_Daily_Member
                            {
                                MemberName = qamain.Name,
                                Gender = qamain.Gender,
                                Mobile = qamain.Mobile,
                                Email = qamain.Email,
                                Birthday = qamain.Birthday,
                                MaritalStatusId = qamain.MaritalStatus,
                                CreateTime = DateTime.Now,
                                CustomerId = qamain.CustomerId
                            });
                            MainDbContext.SaveChanges();
                            foreach (var item in answers)
                            {
                                MainDbContext.AHMS_QAFlow_Main_detail.Add(new AHMS_QAFlow_Main_detail()
                                {
                                    QAFlowMainId = qamainadd.QAFlowMainId,
                                    QAItemNameId = item.Key,
                                    QAItemValue = item.String
                                });
                            }
                        }
                        MainDbContext.SaveChanges();
                        tx.Commit();
                    }
                    catch (Exception ex) { tx.Rollback(); throw ex; }
                }
                //返回确认页面                              
                return View(nameof(QuestionaireSave), model.QAMain);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "错误:" + ex.Message);
                return View(nameof(Questionnaire) + "_" + model.QAMain.QAVer, model);
            }

        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult QuestionaireSave2(ah.Models.AHMS_QAFlow_Main model)
        {
            return View(model);
        }


    }
}
