using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass.Models;
using Ass;
using System.Collections.Generic;
using CHIS.Models.ViewModels;
using CHIS.Models.ViewModel;
using Microsoft.Extensions.Caching.Memory;
using CHIS.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    public partial class DoctorController : BaseController
    {
        // GET: /<controller>/
        [AllowAnonymous]
        //患者就诊
        public IActionResult Patientvisit()
        {
            ViewBag.FuncId = 108;
            return View();
        }


        //诊断详细
        public IActionResult PatientDetail(long registId, long? treatId)
        {

            var u = UserSelf;

            var reg = _db.CHIS_Register.AsNoTracking().FirstOrDefault(m => m.RegisterID == registId);
            var cus = _db.CHIS_Code_Customer.AsNoTracking().FirstOrDefault(m => m.CustomerID == reg.CustomerID);
            var treat = (treatId.HasValue && treatId > 0) ? _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId) :
                        _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.RegisterID == reg.RegisterID);
            var healthInfo = _db.CHIS_Code_Customer_HealthInfo.AsNoTracking().FirstOrDefault(m => m.CustomerId == reg.CustomerID);
            if (healthInfo == null)
            {
                var addEntry = _db.CHIS_Code_Customer_HealthInfo.Add(new Models.CHIS_Code_Customer_HealthInfo { CustomerId = reg.CustomerID.Value });
                _db.SaveChanges();
                healthInfo = addEntry.Entity;
            }

            //新增
            if (treat == null)
            {
                //载入一个数据
                var tr = new Models.CHIS_DoctorTreat()
                {
                    RegisterID = reg.RegisterID,
                    CustomerId = reg.CustomerID.Value,
                    StationId = u.StationId,
                    TreatStatus = 1,
                    TreatCustomerAge = cus.Birthday?.ToAge(),
                    OpID = u.OpId,
                    OpMan = u.OpMan,
                    OpTime = DateTime.Now,
                    DoctorId = u.DoctorId,
                    Department = reg.Department,
                    TreatTime = DateTime.Now,
                    FirstTreatTime = DateTime.Now
                };
                //设置初始数据
                if (healthInfo != null)
                {
                    tr.Height = healthInfo.Height;
                    tr.Weight = healthInfo.Weight;
                    tr.PregnancyNum = healthInfo.PregnancyNum;
                    tr.BirthChildrenNum = healthInfo.BirthChildrenNum;
                    tr.AliveChildrenNum = healthInfo.AliveChildNum;
                    tr.MenstruationEndOldYear = healthInfo.MenstruationEndOldYear;
                    tr.MenstruationStartOldYear = healthInfo.MenstruationStartOldYear;
                }
                _db.CHIS_DoctorTreat.Add(tr);
                _db.SaveChanges();
                treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == tr.TreatId);
            }
            _db.CHIS_DoctorTreat.FirstOrDefault(m => m.TreatId == treat.TreatId).TreatStatus = 1;//设置状态为在诊
            _db.SaveChanges();

            var treatSmy = _treatSvr.GetTreatSummary(treat.TreatId);

            //其他附加费
            var fees = _db.vwCHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treat.TreatId).ToList();

            //成药数据

            var formedmains = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treat.TreatId).ToList();
            var formedList = new List<CHIS.Models.ViewModels.FormedMainViewModel>();
            foreach (var item in formedmains)
            {
                formedList.Add(new Models.ViewModels.FormedMainViewModel
                {
                    TreatSummary = treatSmy,
                    Main = item,
                    Details = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.PrescriptionNo == item.PrescriptionNo).OrderBy(m => m.GroupNum).ThenBy(m => m.AdviceFormedId).ToList()
                });
            }


            //中药数据
            var herbmains = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treat.TreatId).ToList();
            var herbList = new List<Models.ViewModels.CnHerbsMainViewModel>();
            foreach (var item in herbmains)
            {
                herbList.Add(new Models.ViewModels.CnHerbsMainViewModel
                {
                    TreatSummary = treatSmy,
                    Main = item,
                    Details = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.PrescriptionNo == item.PrescriptionNo).ToList()
                });
            }

            var depart = _db.vwCHIS_Code_Department.AsNoTracking().FirstOrDefault(m => m.DepartmentID == reg.Department);

            var sickNote = _db.CHIS_Doctor_SickNote.SingleOrDefault(m => m.TreatId == treat.TreatId);
            if (sickNote == null)
            {
                sickNote = new CHIS_Doctor_SickNote
                {
                    TreatId = treat.TreatId,
                    CustomerId = cus.CustomerID,
                    DoctorId = UserSelf.DoctorId,
                    DoctorName = UserSelf.DoctorName,
                    StationId = UserSelf.StationId,
                    StationName = UserSelf.StationName,
                    TimeStart = DateTime.Today,
                    TimeEnd = DateTime.Today.AddDays(1),
                    CustomerName = cus.CustomerName,
                    CustomerGender = cus.Gender.Value
                };
            }

            var model = new Models.ViewModels.PatientDetailViewModel()
            {
                DoctorTreat = treat,
                CHIS_Code_Customer = _db.vwCHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == reg.CustomerID),
                CHIS_Register = reg,
                RegistDepartment = depart,
                CustomerHealthInfo = healthInfo,
                SpecialTreat = LoadSpecialTreatData(depart.SpetialDepartTypeVal, treat.TreatId, reg.RegisterID),
                TreatExtraFees = fees,
                FormedList = formedList.OrderByDescending(m=>m.Main.CreateTime),// 成药处方
                HerbList = herbList,//中药处方
                FeeSumary = _treatSvr.GetTreatFeeSumary(treat.TreatId), //获取费用
                SickNote = sickNote
            };
            return View(model);
        }

        public IActionResult Json_PrescriptionNum(long treatId)
        {
            return TryCatchFunc((d) =>
            {
                d.formedNum = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId).Count();
                d.herbNum = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId).Count();
                return null;
            });
        }

        //获取特殊接诊的数据
        private SpecialTreat LoadSpecialTreatData(string spetialDepartTypeVal, long treatId, long registId)
        {
            SpecialTreat rtn = null;
            if (spetialDepartTypeVal.IsNotEmpty() && treatId > 0)
            {
                switch (spetialDepartTypeVal)
                {
                    case "PSYCH":
                        var treatExt = _db.CHIS_Doctor_TreatExt.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
                        var rtn1 = new Models.ViewModels.SpecialTreat_Psych { DoctorTreatExt = treatExt };//设置特别接诊数据
                        rtn1.SpetialDepartTypeVal = spetialDepartTypeVal;
                        var reg = _db.CHIS_Register.AsNoTracking().FirstOrDefault(m => m.RegisterID == registId);
                        if (reg.PropName == "PsychPretreatQsId")
                        {
                            rtn1.QsData = _db.vwCHIS_Data_PsychPretreatQs.AsNoTracking().FirstOrDefault(m => m.PsychPretreatQsId == long.Parse(reg.PropValue));
                        }
                        return rtn1;
                }
            }
            return rtn;
        }

        #region  附加费获取

        //获取其他费用清单
        public IActionResult getOtherFees(long treatId)
        {
            var fees = _db.vwCHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            return Json(new { rlt = true, items = fees });
        }

        //刷新用户地址
        public IActionResult RefreshUserAddress(long treatId)
        {
            try
            {
                var b = _dispensingSvr.SetTreatMailAddress(treatId);
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }
        //刷新后获取地址信息
        public IActionResult getOtherFeesAfterRefresh(long treatId)
        {
            try
            {
                var b = _dispensingSvr.SetTreatMailAddress(treatId);
            }
            catch { }
            var fees = _db.vwCHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            return Json(new { rlt = true, items = fees });
        }

        #endregion


        public IActionResult patientvisitCopy()
        {
            return View();
        }

        //查询病人检查的清单
        public IActionResult Json_PatientList(int type, string searchText, int pageIndex = 1, bool isIncludeToday = false)
        {
            //todo 待诊 做假数据
            var pageSize = 6;
            DateTime machineFromDt = DateTime.Now.AddHours(-25), machineToDt = DateTime.Now;
            var u = UserSelf;

            /*只能搜索今天的接诊信息*/
            var itemlist = _db.vwCHIS_Register.AsNoTracking().Where(m => m.StationID == u.StationId && m.EmployeeID == u.DoctorId);
            var itemlist0 = itemlist;
            if (type == 2 && isIncludeToday)//如果是已诊，并包含今天所有已诊
            {
                itemlist = itemlist.Where(m => m.TreatTime > DateTime.Now.AddDays(-2) && (m.TreatTime.Value - DateTime.Today).Days == 0);
                itemlist0 = itemlist.Where(m => m.RegisterDate.Value.Date == DateTime.Today);
            }
            else
            {
                itemlist = itemlist.Where(m => m.RegisterDate.Value.Date == DateTime.Today);
                itemlist0 = itemlist.Where(m => m.RegisterDate.Value.Date == DateTime.Today);
            }


            var items = itemlist;
            if (type == 0) items = items.Where(m => m.TreatStatus == "waiting");
            if (type == 1) items = items.Where(m => m.TreatStatus == "treating");
            if (type == 2) items = items.Where(m => m.TreatStatus == "treated");


            var finds = string.IsNullOrWhiteSpace(searchText) ? items : items.Where(m => m.CustomerName.Contains(searchText) || m.Telephone == searchText);
            if (type > 0) finds = finds.OrderByDescending(m => m.TreatTime);
            else finds = finds.OrderBy(m => m.RegisterDate).ThenBy(m => m.RegisterSeq);
            var selItems = finds.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList().Select(m => new
            {
                registId = m.RegisterID,
                registDepartmentId = m.Department,
                registDepartmentName = m.DepartmentName,
                treatId = m.TreatId,
                customerId = m.CustomerID,
                customerName = m.CustomerName,
                mobile = (m.Telephone ?? "").ToMarkString(MaskType.MobileCode),
                visitTime = m.RegisterDate?.ToString("yyyy-MM-dd"),
                registSlot = m.RegisterSlot.ahDtUtil().ToSlotStr(),
                sourceFrom = m.RegisterFromName,
                Gender = m.Gender?.ToGenderString(),
                Age = m.Birthday?.ToAgeString(),
                UserImageUrl = m.PhotoUrlDef.ahDtUtil().GetCustomerImg(m.Gender),
                TreatTime = m.TreatTime?.ToString("yyyy-MM-dd HH:mm"),
                Diagnosis1 = m.Diagnosis1,
                TimeMark = (m.TreatTime?.Date == DateTime.Today.AddDays(-1)) ? "昨" : "",
                IsVip = m.IsVIP
            });

            long waitingCount = itemlist0.Where(m => m.TreatStatus == "waiting").Count(),
                treatingCount = itemlist0.Where(m => m.TreatStatus == "treating").Count(),
                treatedCount = itemlist.Where(m => m.TreatStatus == "treated").Count(),
                machineCount = new CHIS.Controllers.OneMachineCBL(this).OneMachineList(machineFromDt, machineToDt, pageSize, pageIndex, true).RecordTotal;
            if (type == 0) waitingCount = finds.Count();
            if (type == 1) treatingCount = finds.Count();
            if (type == 2) treatedCount = finds.Count();




            if (type < 3)
            {
                return Json(new
                {
                    rlt = true,
                    msg = "",
                    totalRecords = finds.Count(),
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    items = selItems,
                    waitingCount = waitingCount,
                    treatingCount = treatingCount,
                    treatedCount = treatedCount,
                    machineCount = machineCount
                });
            }
            //一体机
            if (type == 3)
            {
                var rlt = new CHIS.Controllers.OneMachineCBL(this).OneMachineList(machineFromDt, machineToDt, pageSize, pageIndex);
                return Json(new
                {
                    rlt = true,
                    msg = "",
                    totalRecords = rlt.RecordTotal,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    items = rlt.DataList,
                    waitingCount = waitingCount,
                    treatingCount = treatingCount,
                    treatedCount = treatedCount,
                    machineCount = rlt.RecordTotal
                });
            }


            return null;
        }



        //type=HISTORY 历史 /TOMORROW 明日/AFTERTOMORROW 明日之后
        public IActionResult Json_PatientHistoryOrFuture(string type, int pageIndex = 1, string searchText = null)
        {
            //todo 需要进行人员的约束

            Func<string, string> treatStatusString = (string treatStatus) =>
            {
                if (treatStatus == "waiting") return "待接诊";
                if (treatStatus == "treating") return "未完诊";
                return "";
            };

            var pageSize = 10;
            var finds = query_PatientHistoryOrFuture(type, searchText);
            //分页
            var selItems = (type == "HISTORY" ? finds.OrderByDescending(m => m.RegisterDate) : finds.OrderBy(m => m.RegisterDate))
                          .ThenBy(m => m.RegisterSeq).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                          .Select(m => new
                          {
                              RegisterID = m.RegisterID,
                              RegistDepartmentId = m.Department,
                              RegistDepartmentName = m.DepartmentName,
                              CustomerId = m.CustomerID,
                              CustomerName = m.CustomerName,
                              Gender = m.Gender?.ToGenderString(),
                              Age = m.Birthday?.ToAgeString(),
                              TreatId = m.TreatId,
                              Mobile = m.Telephone.ToMarkString(MaskType.MobileCode),
                              CreateDate = m.CustomerCreateDate?.ToStdString(),
                              RegistData = m.RegisterDate?.Date.ToDateString(),
                              CustomerPic = m.CustomerPhoto,
                              TreatStatus = treatStatusString(m.TreatStatus),
                              IsVip = m.IsVIP
                          });
            return Json(new
            {
                rlt = true,
                msg = "",
                totalRecords = finds.Count(),
                pageIndex = pageIndex,
                pageSize = pageSize,
                items = selItems
            });
        }

        //获取历史或者未来的清单数据统计
        public IActionResult Json_PatientHistoryOrFutureNumbers()
        {
            return Json(new
            {
                rlt = true,
                WaitingHistoryNum = query_PatientHistoryOrFuture("HISTORY").Count(),
                WaitingTomorrowNum = query_PatientHistoryOrFuture("TOMORROW").Count(),
                WaitingAfterTomorrowNum = query_PatientHistoryOrFuture("AFTERTOMORROW").Count()
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">HISTORY 默认40天内，否则2年内 ，TOMORROW、AFTERTOMORROW</param>
        /// <param name="searchTxt"></param>
        /// <returns></returns>
        private IQueryable<Models.vwCHIS_Register> query_PatientHistoryOrFuture(string type, string searchTxt = null)
        {
            var u = UserSelf;
            var finds = _db.vwCHIS_Register.AsNoTracking().Where(m => (m.TreatStatus == "waiting" || m.TreatStatus == "treating") && m.EmployeeID == u.DoctorId && m.StationID == u.StationId);
            var dt = DateTime.Now.AddHours(-25);
            var dtStart = string.IsNullOrWhiteSpace(searchTxt) ? DateTime.Now.AddDays(-40) : DateTime.Now.AddYears(-2);
            switch (type.ToUpper())
            {
                case "HISTORY":
                    finds = finds.Where(m => m.RegisterDate < dt && m.RegisterDate > dtStart).Where(m => (m.TreatStatus == "treating") || (m.TreatStatus == "waiting" && m.RegisterDate < dt && m.RegisterDate > dt.Date.AddDays(-1)));
                    break;
                case "TOMORROW":
                    dt = DateTime.Now.AddDays(1).Date;
                    finds = finds.Where(m => (m.RegisterDate.Value - dt).Days == 0); break;
                case "AFTERTOMORROW":
                    dt = DateTime.Now.AddDays(2).Date;
                    finds = finds.Where(m => m.RegisterDate >= dt); break;
            }
            //附加搜索条件
            if (!string.IsNullOrWhiteSpace(searchTxt)) { finds = finds.Where(m => m.CustomerName == searchTxt || m.IDcard == searchTxt || m.Telephone == searchTxt); }
            return finds;
        }


        //更新接诊后台数据
        public IActionResult UpdateInputData(string key, string val, long? treatId)
        {
            try
            {
                if (!treatId.HasValue) throw new Exception("没有传入接诊号");
                if (string.IsNullOrWhiteSpace(key)) throw new Exception("没有传入修改字段Key");
                var treat = _db.CHIS_DoctorTreat.FirstOrDefault(m => m.TreatId == treatId);
                var health = _db.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == treat.CustomerId);
                //校验数据             
                if (health == null)
                {
                    var healthEntry = _db.CHIS_Code_Customer_HealthInfo.Add(new Models.CHIS_Code_Customer_HealthInfo()
                    {
                        CustomerId = treat.CustomerId
                    });
                    _db.SaveChanges();
                    health = healthEntry.Entity;
                }

                //更新数据
                switch (key)
                {
                    case "PastMedicalHistory": health.PastMedicalHistory = val; break;
                    case "Allergic": health.Allergic = val; break;
                    case "Height": treat.Height = health.Height = Ass.P.PIntN(val); break;
                    case "Weight": treat.Weight = health.Weight = Ass.P.PDecimalN(val); break;
                    case "Temperature": treat.Temperature = Ass.P.PDecimalN(val); break;
                    case "RespiratoryPerMinute": treat.RespiratoryPerMinute = Ass.P.PIntN(val); break;

                    case "Pulse": treat.Pulse = Ass.P.PIntN(val); break;
                    case "BloodPress_H": treat.BloodPress_H = Ass.P.PIntN(val); break;
                    case "BloodPress_L": treat.BloodPress_L = Ass.P.PIntN(val); break;

                    case "MenstruationStartOldYear": treat.MenstruationStartOldYear = Ass.P.PIntN(val); break;
                    case "MenstruationCircleDay": treat.MenstruationCircleDay = Ass.P.PIntN(val); break;
                    case "MenstruationDays": treat.MenstruationDays = Ass.P.PIntN(val); break;
                    case "MenstruationDay": treat.MenstruationDay = Ass.P.PIntN(val); break;

                    case "PregnancyNum": treat.PregnancyNum = Ass.P.PIntN(val); break;
                    case "BirthChildrenNum": treat.BirthChildrenNum = Ass.P.PIntN(val); break;
                    case "AliveChildrenNum": treat.AliveChildrenNum = health.AliveChildNum = (short?)Ass.P.PIntN(val); break;

                }
                _db.SaveChanges();
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }


        //取消接诊
        public IActionResult CancelRegist(long registId, long treatId = 0)
        {
            return TryCatchFunc(() =>
            {
                var reg = _db.CHIS_Register.AsNoTracking().FirstOrDefault(m => m.RegisterID == registId);
                if ((reg.TreatId ?? 0) == treatId && treatId > 0)
                {
                    var treat = _db.CHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
                    if (treat.FstDiagnosis > 0) throw new Exception("已经做出了接诊诊断，不能删除");
                    _db.Remove(treat);
                }
                _db.Remove(reg);
                _db.SaveChanges();
                return null;
            });
        }









        public IActionResult TempSaveTreat(long treatId, Models.ViewModels.PatientDetailViewModel viewdata)
        {
            try
            {
                if (treatId == 0) throw new Exception("没有传入接诊号");
                if (treatId != viewdata.DoctorTreat.TreatId) throw new Exception("传入的接诊号不正确");

                var find = _db.CHIS_DoctorTreat.Find(treatId);
                find.Complain = viewdata.DoctorTreat.Complain;
                find.Examination = viewdata.DoctorTreat.Examination;
                find.PresentIllness = viewdata.DoctorTreat.PresentIllness;
                find.FstDiagnosis = viewdata.DoctorTreat.FstDiagnosis;
                find.SecDiagnosis = viewdata.DoctorTreat.SecDiagnosis;
                find.FstIsDiag = viewdata.DoctorTreat.FstIsDiag;
                find.SecIsDiag = viewdata.DoctorTreat.SecIsDiag;

                find.TreatDoctorMethod = viewdata.DoctorTreat.TreatDoctorMethod;
                find.TreatDoctorUseDrip = viewdata.DoctorTreat.TreatDoctorUseDrip;
                find.TreatDoctorUseDrug = viewdata.DoctorTreat.TreatDoctorUseDrug;
                find.TreatDoctorFollowUp = viewdata.DoctorTreat.TreatDoctorFollowUp;
                _db.SaveChanges();

                //扩展接诊数据
                if (viewdata.SpecialTreat != null && viewdata.SpecialTreat.DoctorTreatExt != null)
                {
                    viewdata.SpecialTreat.DoctorTreatExt.TreatId = treatId;
                    _db.Upsert(viewdata.SpecialTreat.DoctorTreatExt);//本身就有保存
                }
                //保存诊断数据
                _treatSvr.AddHistoryDiagnosis(find.DoctorId, find.StationId, find.FstDiagnosis.Value);
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        //取消接诊
        public IActionResult CancelTreat(long treatId)
        {
            try
            {
                var find = _db.CHIS_DoctorTreat.FirstOrDefault(m => m.TreatId == treatId);
                if (find.TreatStatus > 1) throw new Exception("已经接诊完毕，不能取消本次接诊");
                find.TreatStatus = 0;
                _db.SaveChanges();
                return Json(new { rlt = true });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        //完成接诊
        public IActionResult TreatFinished(long treatId, Models.ViewModels.PatientDetailViewModel viewdata)
        {
            try
            {
                if (treatId == 0) throw new Exception("没有传入接诊号");
                if (treatId != viewdata.DoctorTreat.TreatId) throw new Exception("传入的接诊号不正确");
                if (!(viewdata.DoctorTreat.FstDiagnosis > 0)) throw new Exception("没有传入诊断");

                var find = _db.CHIS_DoctorTreat.Find(treatId);
                find.Complain = viewdata.DoctorTreat.Complain;
                find.Examination = viewdata.DoctorTreat.Examination;
                find.PresentIllness = viewdata.DoctorTreat.PresentIllness;
                find.FstDiagnosis = viewdata.DoctorTreat.FstDiagnosis;
                find.SecDiagnosis = viewdata.DoctorTreat.SecDiagnosis;

                find.TreatDoctorMethod = viewdata.DoctorTreat.TreatDoctorMethod;
                find.TreatDoctorUseDrip = viewdata.DoctorTreat.TreatDoctorUseDrip;
                find.TreatDoctorUseDrug = viewdata.DoctorTreat.TreatDoctorUseDrug;
                find.TreatDoctorFollowUp = viewdata.DoctorTreat.TreatDoctorFollowUp;

                find.TreatTime = DateTime.Now;
                find.TreatStatus = 2;
                _db.SaveChanges();
                //扩展接诊数据
                if (viewdata.SpecialTreat != null && viewdata.SpecialTreat.DoctorTreatExt != null)
                {
                    viewdata.SpecialTreat.DoctorTreatExt.TreatId = treatId;
                    _db.Upsert(viewdata.SpecialTreat.DoctorTreatExt);//本身就有保存
                }
                //保存诊断数据
                _treatSvr.AddHistoryDiagnosis(find.DoctorId, find.StationId, find.FstDiagnosis.Value);
                return Json(new { rlt = true, treatId = treatId });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }






        public JsonResult Json_RefreshDrugInfo(int drugId)
        {
            try
            {
                var drug = _db.CHIS_Code_Drug_Main.Find(drugId);
                if (!drug.ThreePartDrugId.HasValue) throw new Exception("该药品没有第三方信息");
                var druginfo = _jkSvr.QueryDrugInfo(drug.ThreePartDrugId.Value);
                var finds = _db.CHIS_DrugStock_Monitor.Where(m => m.DrugId == drug.DrugId);
                foreach (var drugStock in finds)
                {
                    if (drugStock.StockSalePrice != druginfo.ourPriceM) //目前只是价格不一致的时候进行重新调整价格
                    {
                        drugStock.StockSalePrice = druginfo.ourPriceM;
                    }
                }
                _db.SaveChanges();
                return Json(new { rlt = true, price = druginfo.ourPriceM.ToString("0.00") });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }




        public IActionResult CheckDrugIsAvaliable(int threePartDrugId, DateTime? refreshTime)
        {
            return TryCatchFunc(() =>
            {
                //更新基本数据
                if (refreshTime == null || (DateTime.Now - refreshTime.Value).TotalDays > 90)
                {
                    var dd = _jkSvr.QueryDrugInfo(threePartDrugId);
                    var drug = _db.CHIS_Code_Drug_Main.FirstOrDefault(m => m.SourceFrom == (int)DrugSourceFrom.WebNet && m.SupplierId == MPS.SupplierId_JK && m.ThreePartDrugId == threePartDrugId);
                    drug.DrugPicUrl = Global.ConfigSettings.JKImageRoot + dd.thumbnailUrl;
                    if (dd.prescriptionType == "2") drug.DrugRxType = "OTC_R";
                    if (dd.prescriptionType == "3") drug.DrugRxType = "OTC_G";
                    if (dd.prescriptionType == "4" || dd.prescriptionType == "5") drug.DrugRxType = "RX";
                    if (dd.prescriptionType == "9") drug.MedialMainKindCode = MPS.MedicalMainKindCode.MT;
                    if (dd.introduction.IsNotEmpty()) drug.UseRemark = dd.introduction;
                    drug.ThreePartDrugRefreshTime = DateTime.Now;
                    _db.SaveChanges();
                }
                var rlt = _jkSvr.CheckDrugIsAvaliable(threePartDrugId);
                if (rlt) return null;
                else throw new Exception("该药品暂不支持");
            });
        }




        #region 西药处方的处理操作

        public IActionResult NewFormedPrescription(long treatId)
        {
            if (treatId == 0) throw new Exception("没有传入TreatId");
            var model = new Models.ViewModels.FormedMainViewModel
            {
                Main = new Models.CHIS_DoctorAdvice_Formed
                {
                    TreatId = treatId,
                    Amount = 0.00m,
                    CreateTime = DateTime.Now
                },
                TreatSummary = _treatSvr.GetTreatSummary(treatId),
                Details = new List<Models.vwCHIS_DoctorAdvice_Formed_Detail>()
            };
            return PartialView("_pvFormedMain", model);
        }
        public IActionResult SelectFormedDrug()
        {
            ViewBag.FORMEDSELECT = HttpContext.Request.Cookies["FORMEDSELECT"];
            return View("_selectFormedDrug");
        }

        public IActionResult GetSelectedDrugs(IList<DrugStockIndexItem> drugs)
        {
            return Json(_drugSrv.GetStockDrugsInfo(drugs));
        }

        //药品总览        

        [ResponseCache(CacheProfileName = "Default")]
        public IActionResult SelectMultipleFormedDrug(List<int> drugs)
        {
            ViewBag.FORMEDSELECT = HttpContext.Request.Cookies["FORMEDSELECT"];
            ViewBag.SelectedDrugs = _drugSrv.GetDrugs(drugs).ToList();
            return View(GetViewPath(nameof(SelectMultipleFormedDrug), "_pvFormedAddMultiple")
                , new Models.ViewModel.SelectDrugsViewModel
            {
                SelectedDrugs = drugs
            });
        }

        //采用缓存技术，载入诊所的药品
        [ResponseCache(Duration = 120, VaryByQueryKeys = new string[] { "stationId" })]
        public IActionResult LoadMyClinicDrugs(int? stationId)
        {
            if (!stationId.HasValue) stationId = UserSelf.DrugStoreStationId;
            string cacheKey = $"MyclinicDrugs_{UserSelf.DrugStoreStationId}";
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<DrugSelectItem> result))
            {
                result = _drugSrv.GetAllDrugsOfClinic(UserSelf.DrugStoreStationId).ToList();
                _memoryCache.Set(cacheKey, result);
                //设置相对过期时间1分钟
                _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1)));
                /*
                 //设置绝对过期时间2分钟
                 _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                     .SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
                 //移除缓存
                 _memoryCache.Remove(cacheKey);
                 //缓存优先级 （程序压力大时，会根据优先级自动回收）
                 _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                     .SetPriority(CacheItemPriority.NeverRemove));
                 //缓存回调 10秒过期会回调
                 _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                     .SetAbsoluteExpiration(TimeSpan.FromSeconds(10))
                     .RegisterPostEvictionCallback((key, value, reason, substate) =>
                     {
                         Console.WriteLine($"键{key}值{value}改变，因为{reason}");
                     }));
                 //缓存回调 根据Token过期
                 var cts = new CancellationTokenSource();
                 _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                     .AddExpirationToken(new CancellationChangeToken(cts.Token))
                     .RegisterPostEvictionCallback((key, value, reason, substate) =>
                     {
                         Console.WriteLine($"键{key}值{value}改变，因为{reason}");
                     }));
                 cts.Cancel();
                 */
            }
            return PartialView(GetViewPath(nameof(SelectMultipleFormedDrug), "_pvFormedAddMultipleMyClinic"),
                result);
        }


        //药品总览   药品
        public IActionResult SetMultipleFormedDrug(int DrugId, string stockFromId, string DrugName, string DrugModel, string ManufacturerOrigin, string type)
        {
            var model = new Models.vwCHIS_DoctorAdvice_Formed_Detail
            {
                DrugId = DrugId,
                DrugName = DrugName,
                DrugModel = DrugModel,
                ManufacturerOrigin = ManufacturerOrigin,
                StockFromId = stockFromId
            };
            ViewBag.Type = type;
            return View(GetViewPath(nameof(SelectMultipleFormedDrug), "_pvFormedAddMultipleDetail"), model);
        }
        //获取来源cookies
        public string GetFormedCookies()
        {
            return HttpContext.Request.Cookies["FORMEDSELECT"];
        }
        //西药部分信息
        public IActionResult Json_GetDrugInfos(string term, IEnumerable<string> drugFrom = null, bool? isIncludeZero = false, int pageIndex = 1, int maxRows = 100, int pageSize = 10)
        {
            DateTime dt = DateTime.Now;
            var finds = new DoctorCBL(this).query_GetDrugInfos(term, ref drugFrom, "").Where(m => m.MedialMainKindCode != MPS.MedicalMainKindCode.ZYM);
            if (isIncludeZero == false) finds = finds.Where(m => m.StockSalePrice > 0);
            var items = finds.OrderByDescending(m => m.DrugCompleteScore).ThenBy(m => m.DrugId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            DateTime dt1 = DateTime.Now;

            var model = new Ass.Mvc.PageListInfo<Models.vwCHIS_DrugStock_Monitor>();
            model.DataList = items;
            model.PageIndex = pageIndex;
            model.PageSize = pageSize;
            model.RecordTotal = finds.Count();
            ViewBag.term = term;
            ControllerContext.HttpContext.Response.Cookies.Append("FORMEDSELECT", string.Join(",", drugFrom));
            base._setDebugText(dt);
            return PartialView(GetViewPath(nameof(SelectMultipleFormedDrug), "_pvSelectFormedDetail"), model);
        }
        public IActionResult AddMultipleFormedDrug(List<int> drugs)
        {
            var model = new List<Models.vwCHIS_DoctorAdvice_Formed_Detail>();
            foreach (var drugId in drugs)
            {
                model.Add(createOneFormedDrug(drugId));
            }
            return PartialView("_pvMultipleFormedDetail", model);
        }

        private vwCHIS_DoctorAdvice_Formed_Detail createOneFormedDrug(int drugId)
        {
            //设置默认出药单位
            bool bUseBigUnit = _accSvr.GetFuncConfig(MyConfigNames.PationtVisitV20_PRES_DefDrugUnit).ToString() == "BIG";
            var rlt = _accSvr.GetMyConfig("DefDrugUnit");
            if (rlt.IsNotEmpty()) bUseBigUnit = rlt == "BIG";

            var drugstock = _db.vwCHIS_DrugStock_Monitor.AsNoTracking().FirstOrDefault(m => m.DrugId == drugId && (m.StationId == UserSelf.DrugStoreStationId || m.StationId == -1));
            var drugprop = _db.vwCHIS_Code_Drug_Main.AsNoTracking().FirstOrDefault(m => m.DrugId == drugId);
            var model = new Models.vwCHIS_DoctorAdvice_Formed_Detail
            {
                DrugId = drugId,
                DrugName = drugstock.DrugName,
                DrugModel = drugstock.DrugModel,
                DrugPicUrl = drugstock.DrugPicUrl,
                MedialMainKindCode = drugstock.MedialMainKindCode,
                StockFromId = drugstock.DrugStockMonitorId,
                ChargeStatus = 0,
                DrugRxType = drugstock.DrugRxType,

                // 大/小/含量单位
                UnitBigId = drugstock.UnitBigId,
                OutUnitBigName = drugstock.OutUnitBigName,
                UnitSmallId = drugstock.UnitSmallId,
                OutUnitSmallName = drugstock.OutUnitSmallName,
                DosageUnitId = drugstock.DosageUnitId,
                DosageUnitName = drugstock.DosageUnitName,
                //转换率
                OutpatientConvertRate = drugstock.OutpatientConvertRate,
                DosageContent = drugstock.DosageContent,
                //出药单位               
                UnitId = bUseBigUnit ? drugstock.UnitBigId.Value : drugstock.StockUnitId,//出药单位Id
                UnitName = drugstock.StockUnitName,

                GivenTakeTypeId = drugprop.DefDrugGivenTakeTypeId,
                GivenTimeTypeId = drugprop.DefDrugGivenTimeTypeId,
                GivenWhereTypeId = drugprop.DefDrugGivenWhereTypeId,
                GivenDosage = drugprop.DosageContent?.ToString("#0.##"),
                PrescribeStyle = MPS.Drug_包装,
                GivenRemark = "按说明书使用",

                Price = drugstock.StockSalePrice,
                Qty = 1,
                Amount = drugstock.StockSalePrice,

            };
            return model;
        }

        public IActionResult AddOneFormedDrug(int drugId)
        {
            var model = createOneFormedDrug(drugId);
            return PartialView("_pvFormedDetail", model);
        }

        public IActionResult SaveFormedPrescription(Models.ViewModels.FormedMainViewModel model)
        {
            return TryCatchFunc((Func<dynamic, IActionResult>)((dd) =>
            {
                //数据校验
                if (model.Main.TreatId == 0) throw new Exception("没有传入接诊Id");
                if (model.Details.Count() == 0) throw new Exception("没有传入任何药品");

                //初始化数据
                var isNew = AssExpands.IsIdEmpty(model.Main.PrescriptionNo);
                var keyId = isNew ? Guid.NewGuid() : model.Main.PrescriptionNo;

                foreach (var item in model.Details)
                {
                    if (item.DrugId == 0) throw new Exception("没有传入药品Id");
                    if (item.Qty < 1) throw new Exception("没有传入药品数量");
                    if (item.UnitId < 1) throw new Exception("没有传入发药单位");
                }

                var drugs = model.Details.Select(m => m.DrugId).Distinct();
                var stock = _db.vwCHIS_DrugStock_Monitor.AsNoTracking().Where(m => drugs.Contains(m.DrugId) && (m.StationId == UserSelf.DrugStoreStationId || m.StationId == -1)).ToList();//获取所有库存内的药品数据


                #region 计算代理
                Func<int, vwCHIS_DrugStock_Monitor> findStockFrom = (int drugId) =>
               {
                   var drug = stock.FirstOrDefault(m => m.DrugId == drugId);
                   return drug;
               };

                Func<int, int, decimal> calcPrice = (int drugId, int unitId) =>
               {
                   //根据药品和出药单位计算价格
                   var drug = findStockFrom(drugId);
                   if (unitId == drug.StockUnitId) return drug.StockSalePrice;
                   else
                   {
                       if (unitId == drug.UnitSmallId) return drug.StockSalePrice / drug.OutpatientConvertRate.Value;
                       if (unitId == drug.UnitBigId) return drug.StockSalePrice * drug.OutpatientConvertRate.Value;
                   }
                   throw new Exception("计算出库价格出现未知错误！");
               };

                #endregion




                Models.CHIS_DoctorAdvice_Formed main = null;
                if (isNew)
                {
                    //新增
                    model.Main.CreateTime = DateTime.Now;
                    model.Main.PrescriptionNo = keyId;

                    main = _db.AddSave(model.Main);
                }
                else
                {
                    main = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().FirstOrDefault((System.Linq.Expressions.Expression<Func<CHIS_DoctorAdvice_Formed, bool>>)(m => m.PrescriptionNo == keyId));
                }

                var dbfinds = _db.CHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.PrescriptionNo == keyId).ToList();
                foreach (var item in dbfinds)
                {
                    if (!model.Details.Any(m => m.AdviceFormedId == item.AdviceFormedId)) _db.Remove(item);
                }
                _db.SaveChanges();//删除多余的

                foreach (var item in model.Details)
                {
                    var price = calcPrice(item.DrugId, item.UnitId);

                    if (item.AdviceFormedId > 0)
                    {
                        var mm = _db.CHIS_DoctorAdvice_Formed_Detail.Find(item.AdviceFormedId);
                        mm.DrugId = mm.DrugId;
                        mm.Qty = item.Qty;
                        mm.UnitId = item.UnitId;
                        mm.PrescriptionNo = keyId;
                        mm.TreatId = main.TreatId;
                        mm.Price = price;
                        mm.Amount = price * item.Qty;

                        mm.Advice = item.Advice;
                        mm.GivenDays = item.GivenDays;
                        mm.GivenDosage = item.GivenDosage;
                        mm.GivenNum = item.GivenNum;
                        mm.GivenRemark = item.GivenRemark;
                        mm.GivenTakeTypeId = item.GivenTakeTypeId;
                        mm.GivenTimeTypeId = item.GivenTimeTypeId;
                        mm.GivenWhereTypeId = item.GivenWhereTypeId;
                        mm.GroupNum = item.GroupNum;
                        mm.InfusedNum = item.InfusedNum;
                        mm.IsAid = item.IsAid;
                        mm.IsSkinTest = item.IsSkinTest;
                        mm.IsLock = false;
                        mm.PrescribeStyle = item.PrescribeStyle;
                        mm.Kind = item.Kind;
                    }
                    else _db.CHIS_DoctorAdvice_Formed_Detail.Add(new Models.CHIS_DoctorAdvice_Formed_Detail
                    {
                        DrugId = item.DrugId,
                        StockFromId = findStockFrom(item.DrugId).DrugStockMonitorId,
                        Qty = item.Qty,
                        UnitId = item.UnitId,
                        PrescriptionNo = keyId,
                        TreatId = main.TreatId,
                        Price = price,
                        Amount = price * item.Qty,

                        Advice = item.Advice,
                        GivenDays = item.GivenDays,
                        GivenDosage = item.GivenDosage,
                        GivenNum = item.GivenNum,
                        GivenRemark = item.GivenRemark,
                        GivenTakeTypeId = item.GivenTakeTypeId,
                        GivenTimeTypeId = item.GivenTimeTypeId,
                        GivenWhereTypeId = item.GivenWhereTypeId,
                        GroupNum = item.GroupNum,
                        InfusedNum = item.InfusedNum,
                        IsAid = item.IsAid,
                        IsSkinTest = item.IsSkinTest,
                        IsLock = false,
                        PrescribeStyle = item.PrescribeStyle,
                        Kind = item.Kind
                    });
                }
                _db.SaveChanges();

                //计算总价
                main = _db.CHIS_DoctorAdvice_Formed.Find(keyId);
                main.Amount = _db.CHIS_DoctorAdvice_Formed_Detail.Where((System.Linq.Expressions.Expression<Func<CHIS_DoctorAdvice_Formed_Detail, bool>>)(m => m.PrescriptionNo == main.PrescriptionNo)).Sum(m => m.Amount);
                _db.SaveChanges();
                dd.SuccessThenCallUrl = "/Doctor/LoadFomedPrescription?prescriptionNo=" + model.Main.PrescriptionNo;
                return null;
            }), bUseTrans: true);
        }
        public IActionResult LoadFomedPrescription(Guid prescriptionNo)
        {
            if (prescriptionNo.IsIdEmpty()) throw new Exception("传入的处方号错误");
            var main = _db.CHIS_DoctorAdvice_Formed.FirstOrDefault(m => m.PrescriptionNo == prescriptionNo);
            //传回数据               
            return PartialView("_pvFormedMain", new Models.ViewModels.FormedMainViewModel
            {
                TreatSummary = _treatSvr.GetTreatSummary(main.TreatId),
                Main = main,
                Details = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.PrescriptionNo == prescriptionNo).OrderBy(m => m.GroupNum).ThenBy(m => m.AdviceFormedId)
            });
        }

        //设置为同组
        public IActionResult SetDrugsAsOneGroup(IEnumerable<long> adviceIds, long treatId, Guid prescriptionNo)
        {
            return TryCatchFunc((dd) =>
            {
                var num = new short[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }.ToList();
                var items = _db.CHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.PrescriptionNo == prescriptionNo).Select(m => m.GroupNum).Distinct().ToList();
                num.RemoveAll(m => items.Contains(m));
                var number = num.FirstOrDefault();//找到最先的一个
                var updates = _db.CHIS_DoctorAdvice_Formed_Detail.Where(m => adviceIds.Contains(m.AdviceFormedId) && m.TreatId == treatId);
                foreach (var rcd in updates) rcd.GroupNum = number;
                _db.SaveChanges();
                dd.SuccessThenCallUrl = "/doctor/LoadFomedPrescription?prescriptionNo=" + prescriptionNo;
                return null;
            });
        }

        //删除单个药
        public IActionResult DeleteOneAdvice(long adviceFormedId, long treatId)
        {
            return TryCatchFunc((dd) =>
            {
                if (adviceFormedId <= 0) throw new Exception("请传入正确的药品编号");
                if (treatId <= 0) throw new Exception("请传入正确的接诊Id");

                var find = _db.CHIS_DoctorAdvice_Formed_Detail.FirstOrDefault(m => m.TreatId == treatId && m.AdviceFormedId == adviceFormedId);
                var prescriptionNo = find.PrescriptionNo;
                _db.CHIS_DoctorAdvice_Formed_Detail.Remove(find);
                _db.SaveChanges();
                dd.SuccessThenCallUrl = "/doctor/LoadFomedPrescription?prescriptionNo=" + prescriptionNo;
                return null;
            });
        }


        //删除成药处方
        public IActionResult DeleteFomedPrescription(long treatId, Guid prescriptionNo)
        {
            return TryCatchFunc(() =>
            {
                var find = _db.CHIS_DoctorAdvice_Formed.Find(prescriptionNo);
                if (find.ChargeStatus > 0) throw new Exception("该处方不允许删除");
                _db.Remove(find);
                var finds = _db.CHIS_DoctorAdvice_Formed_Detail.Where(m => m.PrescriptionNo == prescriptionNo && m.TreatId == treatId);
                foreach (var item in finds) _db.Remove(item);
                _db.SaveChanges();
                return null;
            }, bUseTrans: true);


        }



        #endregion



        #region 中药处方操作


        //中药部分信息
        //drugSourceFrom 0本地 1网络平台 2第三方
        public IActionResult Json_GetHerbsInfos(string term, IEnumerable<string> drugFrom = null, int pageIndex = 1, int maxRows = 100, int pageSize = 10)
        {
            var finds = new DoctorCBL(this).query_GetDrugInfos(term, ref drugFrom, "ZYM");
            var findlist = finds.OrderByDescending(m => m.DrugCompleteScore).ThenBy(m => m.DrugId).Take(maxRows).ToList();
            var items = findlist.Select(m => new
            {
                value = m.DrugId,
                label = m.DrugName
            });
            return Json(new { rlt = true, items = items });
        }

        //新增中药处方
        public IActionResult NewHerbPrescription(long treatId)
        {
            if (treatId == 0) throw new Exception("没有传入TreatId");
            var model = new Models.ViewModels.CnHerbsMainViewModel
            {
                TreatSummary = _treatSvr.GetTreatSummary(treatId),
                Main = new Models.CHIS_DoctorAdvice_Herbs
                {
                    Qty = 1,
                    TreatId = treatId,
                    Price = 0.00m,
                    Amount = 0.00m
                },
                Details = new List<Models.vwCHIS_DoctorAdvice_Herbs_Detail>()
            };
            return PartialView("_pvHerbMain", model);
        }



        /// <summary>
        /// 通过中药的ID查询药品信息
        /// </summary>
        /// <param name="drugId"></param>
        /// <returns>PartialView（_AddHerbal）</returns>
        public IActionResult SingHerbalInfor(int drugId)
        {

            var herb = _db.vwCHIS_Code_Drug_Main.Find(drugId);
            var stock = _db.vwCHIS_DrugStock_Monitor.FirstOrDefault(m => m.DrugId == drugId && m.StationId == UserSelf.DrugStoreStationId);
            var model = new CHIS.Models.vwCHIS_DoctorAdvice_Herbs_Detail
            {
                CnHerbId = herb.DrugId,
                Price = stock.StockSalePrice,
                Qty = 1,
                Amount = stock.StockSalePrice,
                UnitId = stock.StockUnitId,
                DrugName = herb.DrugName,
                UnitName = stock.StockUnitName,
                DrugPicUrl = herb.DrugPicUrl,
                StockFromId = stock.DrugStockMonitorId,
            };
            return PartialView("_pvHerbDetail", model);
        }


        //保存中药处方
        public IActionResult SaveHerbAdvice(Models.CHIS_DoctorAdvice_Herbs main, IEnumerable<Models.CHIS_DoctorAdvice_Herbs_Detail> details)
        {
            return TryCatchFunc((Func<dynamic, IActionResult>)((dd) =>
            {
                //数据检查
                if (main.Qty == 0) throw new Exception("没有输入中药数量");
                //   if (main.Price == 0) throw new Exception("没有传入单价");
                if (details.Count() == 0) throw new Exception("没有传入每味药品");

                List<CHIS_DoctorAdvice_Herbs_Detail> _details = new List<CHIS_DoctorAdvice_Herbs_Detail>();//需要操作的药品
                List<int> drugIds = new List<int>();//存放添加的药品id，主要是加快获取数据库内得药品信息
                foreach (var item in details)
                {
                    if (item.CnHerbId == 0) throw new Exception("没有传入单味中药Id");
                    if (item.Qty == 0) throw new Exception("没有传入单味中药数量");
                    //   if (item.Price == 0) throw new Exception("没有传入单味中药价格");

                    //初始化基本的数据
                    drugIds.Add(item.CnHerbId);
                    if (_details.Any(m => m.CnHerbId == item.CnHerbId))
                        _details.FirstOrDefault(m => m.CnHerbId == item.CnHerbId).Qty += item.Qty;//不添加重复的药品                    
                    else _details.Add(item);
                }

                //后端重新计算价格
                decimal price = 0m;//用于计算总价
                bool isMainNew = AssExpands.IsIdEmpty(main.PrescriptionNo);//是否是新增判断
                var mainId = isMainNew ? Guid.NewGuid() : main.PrescriptionNo;//计算的主Id
                var addDrugs = _db.vwCHIS_DrugStock_Monitor.Where(m => m.StationId == UserSelf.DrugStoreStationId && drugIds.Contains(m.DrugId) && m.StockDrugIsEnable == true).ToList();//获取添加药品基本信息基准
                var dbDetails = _db.CHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.PrescriptionNo == mainId).ToList();//获取数据库内的详细药品信息                
                foreach (var item in _details)
                {
                    var p = addDrugs.FirstOrDefault(m => m.DrugId == item.CnHerbId);
                    item.Price = p.StockSalePrice;
                    item.Amount = item.Price * item.Qty;
                    item.PrescriptionNo = mainId;
                    item.TreatId = main.TreatId;
                    item.UnitId = (item.UnitId == 0) ? CHIS.MPS.Unit_g : item.UnitId;
                    item.StockFromId = p.DrugStockMonitorId;
                    price += item.Amount;
                }

                //重新计算总价
                main.Price = price;
                main.Amount = Math.Round((price * main.Qty), 2);//保留2位小数
                main.PrescriptionNo = mainId;

                if (isMainNew)
                {
                    var add = _db.AddSave(main);
                    foreach (var herb in _details)
                    {
                        herb.PrescriptionNo = add.PrescriptionNo;
                        _db.Add(herb);
                    }
                }
                else
                {
                    //更新
                    _db.Update(main);
                    //删除不添加到数据库内的信息
                    foreach (var dbherb in dbDetails)
                    {
                        if (!drugIds.Contains(dbherb.CnHerbId)) _db.Remove(dbherb);//删除
                    }
                    _db.SaveChanges();
                    foreach (var herb in _details)
                    {
                        var find = dbDetails.FirstOrDefault(m => m.CnHerbId == herb.CnHerbId);
                        if (find == null) _db.Add(herb);//没有则增加
                        else
                        {
                            herb.Id = find.Id;//设置为数据库内的Id
                            _db.Update(herb);//更新数据库
                        }
                    }
                }
                _db.SaveChanges();
                dd.SuccessThenCallUrl = "/Doctor/LoadHerbDescription?prescriptionNo=" + main.PrescriptionNo;
                return null;
            }), bUseTrans: true);
        }

        [HttpPost]
        public IActionResult LoadHerbDescription(Guid prescriptionNo)
        {
            if (prescriptionNo.IsIdEmpty()) throw new Exception("传入的处方号错误");
            //传回数据               
            var main = _db.CHIS_DoctorAdvice_Herbs.FirstOrDefault(m => m.PrescriptionNo == prescriptionNo);
            return PartialView("_pvHerbMain", new Models.ViewModels.CnHerbsMainViewModel
            {
                TreatSummary = _treatSvr.GetTreatSummary(main.TreatId),
                Main = main,
                Details = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.PrescriptionNo == prescriptionNo)
            });
        }

        //删除中药处方
        public IActionResult DeleteHerbAdvice(Guid prescriptionNo)
        {
            return TryCatchFunc(() =>
            {
                var find = _db.CHIS_DoctorAdvice_Herbs.Find(prescriptionNo);
                _db.Remove(find);
                var finds = _db.CHIS_DoctorAdvice_Herbs_Detail.Where(m => m.PrescriptionNo == prescriptionNo);
                foreach (var item in finds) _db.Remove(item);
                _db.SaveChanges();
                return null;
            }, bUseTrans: true);


        }





        #endregion


        #region 其他费用
        //添加收费
        public IActionResult AddExtraFees()
        {
            return View("_pvAddExtraFees");
        }
        //添加附加费
        public IActionResult SaveExtraFee(Models.CHIS_Doctor_ExtraFee extraFee, long treatId)
        {
            return TryCatchFunc((dd) =>
            {
                if (treatId == 0) throw new Exception("请传入接诊Id");
                if (extraFee.Qty == 0) throw new Exception("没有传入数量");
                if (extraFee.TreatFeePrice == 0) throw new Exception("没有传入价格");

                var find = _db.CHIS_Doctor_ExtraFee.Find(extraFee.ExtraFeeId);
                if (find == null)
                {
                    extraFee.TreatId = treatId;
                    extraFee.Amount = extraFee.Qty * extraFee.TreatFeePrice;
                    _db.CHIS_Doctor_ExtraFee.Add(extraFee);
                    _db.SaveChanges();
                }
                else
                {
                    find.TreatFeeTypeId = extraFee.TreatFeeTypeId;
                    find.Qty = extraFee.Qty;
                    find.TreatFeeOriginalPrice = extraFee.TreatFeeOriginalPrice;
                    find.TreatFeePrice = extraFee.TreatFeePrice;
                    find.Amount = extraFee.TreatFeePrice * extraFee.Qty;
                    find.FeeRemark = extraFee.FeeRemark;

                    _db.SaveChanges();
                }
                dd.SuccessThenCallUrl = "/Doctor/LoadExtraFee?treatId=" + treatId;//成功后载入
                return null;
            });

        }

        //删除附加费
        public IActionResult RemoveExtraFee(long extraFeeId)
        {
            return TryCatchFunc(() =>
            {
                if (extraFeeId == 0) throw new Exception("没有传入正确的Id");
                var find = _db.CHIS_Doctor_ExtraFee.Find(extraFeeId);
                if (find == null) throw new Exception("没有找到数据记录");
                if (find.ChargeStatus > 0) throw new Exception("该附加费已有后续业务关联，不能删除");
                _db.Remove(find);
                _db.SaveChanges();
                return null;
            });

        }
        //载入附加费
        public IActionResult LoadExtraFee(long treatId)
        {
            var model = _db.vwCHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treatId);
            return PartialView("_pvExtraFees", model);
        }



        //获取附加费信息
        public IActionResult LoadExtraFeeInfo(int extraFeeTypeId, long treatId, int fromAreaId = 0, long toAddressId = 0)
        {

            var entity = new Models.CHIS_Doctor_ExtraFee
            {
                Qty = 1,
                TreatFeeTypeId = extraFeeTypeId,
                TreatId = treatId,
                TreatFeeOriginalPrice = 0,
                TreatFeePrice = 0
            };

            //邮费信息
            if (extraFeeTypeId == MPS.Fee_快递)
            {
                if (toAddressId == 0)
                {
                    var cusid = _db.CHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId).CustomerId;
                    var addr = _db.vwCHIS_Code_Customer_AddressInfos.Where(m => m.CustomerId == cusid).OrderBy(m => m.IsDefault).FirstOrDefault();
                    toAddressId = addr.AddressId;
                }
                if (fromAreaId == 0) fromAreaId = MPS.CenterAreaId_JK;
                int toAreaId = 0; decimal feeOrig = 0;
                var fee = _dispensingSvr.GetTransFee(treatId, toAddressId, fromAreaId, out toAreaId, out feeOrig);

                entity.TreatFeeOriginalPrice = feeOrig;
                entity.TreatFeePrice = fee;
                entity.MailFromAreaId = fromAreaId; //货品出发地址
                entity.MailAddressInfoId = toAddressId;//货品到达地址
                entity.MailToAreaId = toAreaId;//货品到达区域
            }

            //诊金
            if (extraFeeTypeId == MPS.Fee_诊金)
            {
                var docId = _db.CHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId).DoctorId;
                var treatFee = _db.CHIS_Code_Doctor.Find(docId).TreatFee;
                entity.TreatFeeOriginalPrice = treatFee;
                entity.TreatFeePrice = treatFee;
            }


            return Json(new { item = entity });
        }




        #endregion



        #region 总价操作

        public IActionResult LoadFeeSumary(long treatId)
        {
            var model = _treatSvr.GetTreatFeeSumary(treatId);
            return PartialView("_pvFeeSumary", model);
        }

 


        #endregion


        [HttpPost]
        public IActionResult SpecialTreat(string key, int treatId)
        {
            dynamic model = null;
            string vname = "";
            switch (Ass.P.PStr(key).ToUpper())
            {
                case "PSYCH": vname = "_pvSt_PSYCH"; break;//心理学问卷
            }
            if (vname.IsNotEmpty()) return PartialView(vname, model);
            else return Content("");
        }

        public async Task<IActionResult> SaveSickNote(CHIS_Doctor_SickNote model)
        {
            try
            {
                if (model.TreatId == 0) throw new Exception("必须传入接诊Id");
                if (model.TimeStart > model.TimeEnd) throw new Exception("传入正确的请假时间段");
                if (model.CustomerId == 0) throw new Exception("必须传入会员Id");
                if (model.StationId == 0) { model.StationId = UserSelf.StationId; model.StationName = UserSelf.StationName; }
                if (model.DoctorId == 0) { model.DoctorId = UserSelf.DoctorId; model.DoctorName = UserSelf.DoctorName; }
                if (model.SickNoteDoctorAdvice.IsEmpty()) model.SickNoteDoctorAdvice = "";


                var find = _db.CHIS_Doctor_SickNote.AsNoTracking().Where(m => m.TreatId == model.TreatId).Count();
                if (model.SickNoteId.IsEmpty() && find == 0)
                {
                    //更新会员正确数据
                    var cus = _db.CHIS_Code_Customer.Find(model.CustomerId);
                    if (cus.CustomerName != model.CustomerName) cus.CustomerName = model.CustomerName;
                    if (cus.Gender != model.CustomerGender) cus.Gender = model.CustomerGender;
                    await _db.SaveChangesAsync();

                    model.SickNoteId = $"{model.StationId}-{model.DoctorId}-{DateTime.Now.ToString("yyyyMMddHHmm")}";
                    model.DoctorName = UserSelf.DoctorName;
                    model.FirstPrintTime = DateTime.Now;
                    model.IsPrinted = true;
                    model.PrintNum += 1;
                    model.StationName = UserSelf.StationName;
                    model.LastPrintTime = DateTime.Now;
                    model = (await _db.CHIS_Doctor_SickNote.AddAsync(model)).Entity;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var m = _db.CHIS_Doctor_SickNote.FirstOrDefault(a => a.SickNoteId == model.SickNoteId || a.TreatId == model.TreatId);
                    m.LastPrintTime = DateTime.Now;
                    m.IsPrinted = true;
                    m.PrintNum += 1;
                    m.DoctorName = UserSelf.DoctorName;
                    m.DoctorId = UserSelf.DoctorId;
                    m.StationId = UserSelf.StationId;
                    m.StationName = UserSelf.StationName;
                    m.SickNoteDoctorAdvice = model.SickNoteDoctorAdvice;
                    await _db.SaveChangesAsync();
                }

                return TryCatchFunc((d) =>
                {
                    d.SickNoteId = model.SickNoteId;
                    d.DoctorId = model.DoctorId;
                    d.StationId = model.StationId;
                    d.TreatId = model.TreatId;
                    return null;
                });
            }
            catch (Exception ex) { return TryCatchFunc(() => { throw ex; }); }
        }













    }
}