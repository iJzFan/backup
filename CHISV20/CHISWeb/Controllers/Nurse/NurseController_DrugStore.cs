using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using CHIS.Models.ViewModel;
using Ass;
using CHIS;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using CHIS.Models.DataModel;

namespace CHIS.Controllers
{
    public partial class NurseController : BaseController
    {

        public IActionResult DrugStoreNurseRegister()
        {
            ViewBag.FuncId = 8;
            ViewBag.RxDoctors = _docSvr.GetMyRxDoctors(UserSelf.StationId);
            ViewBag.Doctors = _staSvr.GetDoctorsOfStation(UserSelf.StationId);

            return View("DrugStoreNurseRegister");
        }

        public IActionResult DrugStoreNurseRegister_pvDoctorInfo(int doctorId)
        {
            var doc = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            var cus = _db.CHIS_Code_Customer.AsNoTracking().FirstOrDefault(m => m.CustomerID == doc.CustomerId);
            var dep = _db.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doc.DoctorId).OrderBy(m => m.StationID).ToList();
            var stationRoles = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doc.DoctorId).OrderBy(m => m.StationId).ToList();

            var roleids = stationRoles.Select(m => m.RoleId.Value).Distinct().ToList();
            var roles = _db.CHIS_SYS_Role.AsNoTracking().Where(m => roleids.Contains(m.RoleID)).Select(m => new RoleItem
            {
                RoleId = m.RoleID,
                RoleKey = m.RoleKey,
                RoleName = m.RoleName
            }).ToList();

            var stationids = stationRoles.Where(m => m.MyStationIsEnable == true).Select(m => m.StationId.Value).Distinct().ToList();
            //stationids.AddRange(stationRoles.Select(m => m.StationId.Value).Distinct());
            stationids = stationids.Distinct().ToList();
            var stations = _db.CHIS_Code_WorkStation.AsNoTracking().Where(m => stationids.Contains(m.StationID)).ToList();

            var model = new Models.ViewModel.DoctorInfo
            {
                Doctor = doc,
                Customer = cus
            };
            model.InitialStationInfo(dep, stationRoles, roles, stations);
            return PartialView("DrugStoreNurseRegister_pvDoctorInfo", model);
        }

        public IActionResult RxDrugSave(long? id)
        {
            ViewBag.FuncId = 19;

            var user = GetUserSelf();

            var customers = _rxSvr.GetNeedAddCustomers(user.StationId, user.DoctorId);

            var customer = new RxUserViewModel();
            if (id != null)
            {
                customer = _rxSvr.GetRxUser(id.Value, user.DoctorId);
            }
            if (customer.SendDrugMan.IsEmpty()) customer.SendDrugMan = user.LoginExtName;
            if (customer.SendTime == new DateTime()) customer.SendTime = DateTime.Now;
            if (customer.CheckTime == new DateTime()) customer.CheckTime = DateTime.Now;

            return View(new RxDrugSaveViewModel { RxUserList = customers, NewRxUser = customer });
        }

        [HttpPost]
        public async Task<IActionResult> RxDrugSave(RxUserViewModel model)
        {

            var info = Ass.Data.Utils.GetIdCardInfo(model.CustomerIdCode);

            bool isNewUser = false;

            #region 输入检验

            if (string.IsNullOrEmpty(model.CheckDrugMan))
            {
                ModelState.AddModelError("CheckDrugMan", "审核人不能为空");
            }

            if (model.CheckTime.Date < model.SendTime.Date)
            {
                ModelState.AddModelError("CheckTime", "审核日期不能早于发药日期");
            }

            var u = GetUserSelf();

            if (model.RxSaveId == 0)
            {
                isNewUser = true;
                //判断是否为已注册用户
                if (model.CustomerId == 0)
                {
                    var userCreated = _cusSvr.GetCustomersBy(model.CustomerIdCode).SingleOrDefault();

                    var newUser = await _cusSvr.CreateCustomerAsync(
                         model.CustomerName,
                         model.CustomerMobile,
                         model.CustomerIdCode,
                         sysSources.处方药记录快录,
                         u.CustomerId, u.LoginExtName);

                    model.CustomerId = newUser.CustomerID;
                }

                var newRx = new CHIS_DrugStore_RxSave(
                    u.StationId,
                    u.DoctorId,
                    model.CustomerId,
                    model.CustomerName,
                    model.CustomerIdCode,
                    model.CustomerMobile,
                    Ass.Data.Utils.GetIdCardInfo(model.CustomerIdCode).GenderString,
                    model.RxPicUrl1,
                    model.RxPicUrl2,
                    model.RxPicUrl3);

                newRx.SendDrugMan = u.LoginExtName;

                newRx.CheckDrugMan = model.CheckDrugMan;

                newRx.SendTime = model.SendTime;

                newRx.CheckTime = model.CheckTime;

                newRx.IsCompleted = true;

                model.RxSaveId = _rxSvr.SaveCustomer(newRx);

                _rxSvr.UpdateDrugsInfo(model.RxSaveId, model.RxSaveDrugsId);

            }

            var rxUser = _rxSvr.GetRxUser(model.RxSaveId, u.DoctorId);
            if (rxUser.DrugList == null || !rxUser.DrugList.Any())
            {
                ModelState.AddModelError("DrugList", "处方药记录不能为空");
            }

            if (!ModelState.IsValid)
            {
                var customers = _rxSvr.GetNeedAddCustomers(u.StationId, u.DoctorId);
                var customer = _rxSvr.GetRxUser(model.RxSaveId, u.DoctorId);
                if (customer != null)
                {
                    return View(new RxDrugSaveViewModel { RxUserList = customers, NewRxUser = customer });
                }
                return View(new RxDrugSaveViewModel { RxUserList = customers, NewRxUser = model });
            }

            #endregion


            //非新用户直接update,使用ifelse有ef上下文问题
            if (!isNewUser)
            {
                var rxModel = new CHIS_DrugStore_RxSave(
                    u.StationId,
                    u.DoctorId,
                    model.CustomerId,
                    model.CustomerName,
                    model.CustomerIdCode,
                    model.CustomerMobile,
                    u.GenderText,
                    model.RxPicUrl1,
                    model.RxPicUrl2,
                    model.RxPicUrl3);
                rxModel.SendDrugMan = u.LoginExtName;
                rxModel.CheckTime = model.CheckTime;
                rxModel.SendTime = model.SendTime;
                rxModel.CheckDrugMan = model.CheckDrugMan;
                rxModel.RxSaveId = model.RxSaveId;
                var res = _rxSvr.UpdateCustomerInfos(rxModel);

                if (res)
                {
                    return Redirect("~/Nurse/RxDrugSave");
                }
                return View("Error", new Exception("权限不足"));
            }

            return Redirect("~/Nurse/RxDrugSave");
        }

        /// <summary>
        /// 载入待添加信息人员
        /// </summary>
        /// <returns></returns>
        public IActionResult GetRxUserList()
        {
            var u = GetUserSelf();
            var customers = _rxSvr.GetNeedAddCustomers(u.StationId, u.DoctorId);
            return PartialView("_pvRxUserList", customers);
        }

        /// <summary>
        /// 获取已完成的Rx记录
        /// </summary>
        /// <returns></returns>
        public IActionResult GetCompletedRxList(string searchText, string TimeRange = null, int pageIndex = 1, int pageSize = 20)
        {
            var u = GetUserSelf();

            DateTime? start = null;
            DateTime? end = null;

            base.initialData_TimeRange(ref start, ref end, 1, timeRange: TimeRange);

            var rxList = _rxSvr.GetRxSaveItems(
                u.StationId,
                u.DoctorId,
                start: start,
                end: end,
                index: pageIndex,
                pageSize: pageSize,
                searchString: searchText);

            return PartialView("_pvCompletedRxList", rxList);
        }

        /// <summary>
        /// 已完成Rx记录的详情
        /// </summary>
        /// <param name="rxSaveId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetCompletedRxUser(long rxSaveId)
        {
            var u = GetUserSelf();

            var rxUser = _rxSvr.GetRxUser(rxSaveId, u.DoctorId, true);
            var customer = await _cusSvr.GetCustomerById(rxUser.CustomerId);

            rxUser.PhotoUrlDef = customer.PhotoUrlDef;

            return PartialView("_pvCompletedRxUser", rxUser);
        }



        /// <summary>
        /// 手机端填写用户信息 s StationId,d - doctorId
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult RxCusM(int s, int d)
        {
            var station = _staSvr.Find(s);
            var doctor = _docSvr.Find(d);
            return View("RxCustomerInputMobile", new RxMobileInputModel
            {
                Doctor = doctor,
                Station = station

            });
        }

        /// <summary>
        /// 手机端提交用户信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RxCusM(RxMobileInputModel model)
        {
            if (model.IsAgreement == false)
            {
                ModelState.AddModelError("IsAgreement", "同意后才能录入信息");
                return View("RxCustomerInputMobile", model);
            }

            if (model.CustomerId == 0)
            {
                var cus = await _cusSvr.CreateCustomerAsync(
                    model.CustomerName,
                    model.CustomerMobile,
                    model.CustomerIdCode,
                    sysSources.处方药记录快录,
                    0, "");

                model.CustomerId = cus.CustomerID;
            }

            var cusGenderStr = Ass.Data.Utils.GetIdCardInfo(model.CustomerIdCode).GenderString;

            var rxModel = new CHIS_DrugStore_RxSave(
                model.Station.StationID,
                model.Doctor.DoctorId,
                model.CustomerId,
                model.CustomerName,
                model.CustomerIdCode,
                model.CustomerMobile,
                cusGenderStr,
                model.RxPicUrl1,
                model.RxPicUrl2,
                model.RxPicUrl3);

            _rxSvr.SaveCustomer(rxModel);

            return RedirectToAction("CustomerInfoCompleted");

        }

        /// <summary>
        /// 手机端录入成功页
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult CustomerInfoCompleted()
        {
            return View();
        }


        public IActionResult DrugStoreMgr()
        {
            var u = UserSelf;
            if (!u.IsExtFuncAllowed("EditLoginExtUser")) return View("NoAccess");
            ViewBag.FuncId = 21;
            ViewBag.MyStaffs = _loginSvr.GetLoginExtsOfThis(u.LoginId);
            return View();
        }

        #region  LoginExtEdit
        [HttpGet]
        public IActionResult EditLoginExt(long loginExitId)
        {
            var model = new CHISSysLoginExt();
            if (loginExitId > 0) model = _loginSvr.GetLoginExt(loginExitId).CopyPropTo<CHISSysLoginExt>();
            if (model == null) throw new Exception("没有找到该登录账户信息。");
            return PartialView("DrugStoreMgr_pvEditStaff", model);
        }

        [HttpPost]
        public IActionResult EditLoginExt(CHISSysLoginExt model)
        {
            var rlt = MyDynamicResult(true, "");
            if (ModelState.IsValid)
            {
                var am = model.CopyPropTo<CHIS_Sys_LoginExt>();
                if (am.LoginExtId > 0) _loginSvr.ModifyLoginExt(am);
                else _loginSvr.AddLoginExt(am);
            }
            else
            {
                var err = this.GetErrorOfModelState(ModelState);
                rlt = MyDynamicResult(false, err);
            }
            return Json(rlt);
        }

        public IActionResult SeeRoleDetail(string roleKey)
        {
            return Json(_loginSvr.GetRoleFuncs(roleKey));
        }

        public IActionResult SetEnabledStaff(long loginExtId, bool bEnable)
        {
            bool b = _loginSvr.SetLoginExtEnable(loginExtId, bEnable);
            var model = _loginSvr.GetLoginExtsOfThis(UserSelf.LoginId);
            return PartialView("DrugStoreMgr_pvMyStaff", model);
        }
        public IActionResult LoadPvMyStaff()
        {
            var model = _loginSvr.GetLoginExtsOfThis(UserSelf.LoginId);
            return PartialView("DrugStoreMgr_pvMyStaff", model);
        }

        #endregion

        /// <summary>
        /// 新增处方记录-添加一个处方药
        /// </summary>
        /// <returns></returns>
        public IActionResult RxDrugSaveAddDrug(long rxSaveId, RxDrugViewModel model)
        {
            var rxSaveDrugsId = _rxSvr.SaveDrug(rxSaveId, model);

            //if (rxSaveId <= 0)
            //{
            //    return PartialView("partialError",new Exception("录入药品不能为空或重复！"));
            //}
            model.RxSaveDrugsId = rxSaveDrugsId;

            return PartialView("_pvRxDrugSaveBase_Drug", model);
        }

        /// <summary>
        /// 删除处方药
        /// </summary>
        /// <param name="rxSaveDrugId"></param>
        /// <returns></returns>
        public IActionResult RxDrugSaveDeleteDrug(long rxSaveDrugId)
        {
            var u = GetUserSelf();

            var res = _rxSvr.DeleteRxDrug(rxSaveDrugId, u.DoctorId);

            try
            {
                if (res)
                {
                    var rlt = MyDynamicResult(res, "删除成功");
                    return Ok(rlt);
                }
                else
                {
                    var rlt = MyDynamicResult(res, "删除失败");
                    return BadRequest(rlt);
                }
            }
            catch (Exception e)
            {
                var rlt = MyDynamicResult(false, e.Message);
                return BadRequest(rlt);
            }
        }


        /// <summary>
        /// 编辑处方记录
        /// </summary>
        /// <param name="rxSaveId"></param>
        /// <returns></returns>
        public IActionResult EditRxSave(long rxSaveId)
        {
            var u = GetUserSelf();
            RxUserViewModel model = new RxUserViewModel
            {
                SendDrugMan = u.LoginExtName,
                SendTime = DateTime.Now,
                CheckTime = DateTime.Now
            };
            if (rxSaveId == 0) return PartialView("_pvRxDrugSaveBase", model);

            return PartialView("_pvRxDrugSaveBase", model);
        }

        public IActionResult DeleteRxUser(long rxSaveId)
        {
            var u = GetUserSelf();


            var status = _rxSvr.DeleteRxOrder(rxSaveId, u.DoctorId);
            if (status)
            {
                var rlt = MyDynamicResult(true, "删除成功！");

                return Ok(rlt);
            }
            else
            {
                var rlt = MyDynamicResult(false, "删除失败！");

                return BadRequest(rlt);
            }
        }

    }
}

