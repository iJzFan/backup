using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass.Models;
using Ass;
using System.Collections.Generic;
using CHIS.Models;

namespace CHIS.Controllers
{
    public partial class DoctorController : BaseController
    {

        //医生审核
        public IActionResult DoctorPending()
        {
            return View("DoctorPending");
        }

        #region 诊断
        //打开诊断搜索
        public IActionResult GetDiagnosis(string actionID)
        {
            ViewBag.actionID = actionID;
            return View(GetViewPath("Diagnosis", "_pvDiagnosis"));
        }
        //打开新增诊断
        public IActionResult AddDiagnosis(string actionID)
        {
            ViewBag.actionID = actionID;
            return View(GetViewPath("Diagnosis", "_pvAddDiagnosis"));
        }

        #endregion


        public IActionResult DoctorPendingList(string doctorName)
        {

            var finds = _db.vwCHIS_Code_Doctor_Authenticate.AsNoTracking();
            if (doctorName.IsNotEmpty())
            {
                if (doctorName.GetStringType().IsMobile) finds = finds.Where(m => m.Mobile == doctorName);
                else finds = finds.Where(m => m.DoctorName == doctorName);
            }
            //只能搜索所在工作站及其下属的医生

            var stations = UserMgr.GetStations(UserSelf.StationId);

            finds = (from item in _db.CHIS_Sys_Rel_DoctorStations
                     join doctor in finds on item.DoctorId equals doctor.DoctorId into temp
                     from tt in temp.DefaultIfEmpty()
                     where (stations == null) ? (item.StationId == UserSelf.StationId) : (stations.Contains(item.StationId)) && tt != null
                     select tt).Distinct();


            return FindPagedData_jqgrid(finds.OrderBy(m => m.DoctorId));
        }

        public IActionResult GetDoctorPendingList(string searchText, string timeRange = "Today", bool? isNeedCheck = null, int? rootStationId = null, int pageIndex = 1, int pageSize = 20)
        {
            DateTime dt = DateTime.Now;
            DateTime? dt0 = null, dt1 = null;
            base.initialData_TimeRange(ref dt0, ref dt1, timeRange);
            rootStationId = rootStationId ?? UserSelf.StationId;
            var finds = new BllCaller.DoctorMgrBllCaller().GetDoctorPendingList(searchText, dt0, dt1, isNeedCheck, rootStationId);
            var total = finds.Count();
            finds = finds.OrderBy(m => m.DoctorId).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var model = new Ass.Mvc.PageListInfo<vwCHIS_Code_Doctor_Authenticate>
            {
                DataList = finds,
                RecordTotal = total,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            base._setDebugText(dt);
            return PartialView("_pvDoctorsPendingList", model);
        }



        public IActionResult DoctorPendingDetail(int doctorId)
        {
            var doctor = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            var login = _db.vwCHIS_Sys_Login.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            if (login == null) throw new ChkException("LOGIN_UNSET", "没有设定医生的登录关联");
            ViewBag.Doctor = doctor;
            ViewBag.Login = login;
            ViewBag.MyDeparts = _db.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId).ToList().OrderBy(m => m.StationID);
            ViewBag.MyCerts = _db.vwCHIS_Code_DoctorCertbook.AsNoTracking().Where(m => m.DoctorId == doctorId).ToList().OrderBy(m => m.CertTypeId);
            ViewBag.StationAccess = _db.vwCHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationIsEnable == true).ToList();
            return View();
        }

        //设置客户认证
        public IActionResult SetCustomerAuthenticated(int customerId, bool isVed)
        {
            return TryCatchFunc(() =>
            {
                var cus = _db.CHIS_Code_Customer.Find(customerId);

                var login = _db.CHIS_Sys_Login.FirstOrDefault(m => m.CustomerId == cus.CustomerID);
                if (login.IdCardNumberIsAuthenticated != true) throw new Exception("身份证认证没有通过，不能设置客户为加验状态。");
                if (isVed)
                {
                    cus.CustomerIsAuthenticated = isVed;
                    cus.CustomerAuthenticatedTime = DateTime.Now;
                }
                else
                {
                    cus.CustomerIsAuthenticated = null;
                    cus.CustomerAuthenticatedTime = null;
                }
                _db.SaveChanges();
                Logger.WriteInfoAsync("DoctorPending", "SetAuthenticated", $"设置会员({cus.CustomerID},{cus.CustomerName})验证状态为{isVed}");
                return Json(MyDynamicResult(true, "设置人员加验成功！"));
            });
        }
        //设置客户身份证验证
        public IActionResult SetCustomerIdCardAuthenticated(int customerId, bool isVed)
        {
            return TryCatchFunc(() =>
            {
                var cus = _db.CHIS_Code_Customer.Find(customerId);
                var login = _db.CHIS_Sys_Login.FirstOrDefault(m => m.CustomerId == cus.CustomerID);
                if (login.IdCardNumber != cus.IDcard) throw new Exception("检测到客户与登录信息的身份证号不一致");


                if (isVed)
                {
                    login.IdCardNumberIsAuthenticated = isVed;
                    login.IdCardNumberAuthenticatedTime = DateTime.Now;
                }
                else
                {
                    login.IdCardNumberIsAuthenticated = null;
                    login.IdCardNumberAuthenticatedTime = null;
                }
                _db.SaveChanges();
                Logger.WriteInfoAsync("DoctorPending", "SetAuthenticated", $"设置会员({cus.CustomerID},{cus.CustomerName})(loginId={login.LoginId})身份证验证状态为{isVed}");
                return Json(MyDynamicResult(true, "设置人员的身份证认证成功！"));
            });
        }

        //设置医生认证
        public IActionResult SetDoctorAuthenticated(int doctorId, bool isVed)
        {
            return TryCatchFunc(() =>
            {
                var cus = _db.CHIS_Code_Doctor.Find(doctorId);
                if (isVed)
                {
                    cus.DoctorIsAuthenticated = isVed;
                    cus.DoctorAuthenticatedTime = DateTime.Now;
                    //如果医生都设置过了，那么客户认证也是过了的
                    var lg = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == cus.CustomerId);
                    if (lg.CustomerIsAuthenticated != true)
                    {
                        lg.CustomerIsAuthenticated = true;
                        lg.CustomerAuthenticatedTime = DateTime.Now;
                    }
                }
                else
                {
                    cus.DoctorIsAuthenticated = null;
                    cus.DoctorAuthenticatedTime = null;
                }
                _db.SaveChanges();

                var doctor = _db.vwCHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                Logger.WriteInfoAsync("DoctorPending", "SetAuthenticated", $"设置医生({doctor.DoctorId},{doctor.DoctorName}) 证验证状态为{isVed}");

                return Json(MyDynamicResult(true, "设置人员认证成功！"));
            });
        }

        //设置是否是测试医生
        public IActionResult SetDoctorIsForTest(int doctorId, bool isTest)
        {
            return TryCatchFunc(() =>
            {
                var doctor = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                doctor.IsForTest = isTest;
                _db.SaveChanges();
                return Json(MyDynamicResult(true, isTest ? "设置医生为测试人员！" : "取消人员测试资格"));
            });
        }
        //设置是否是测试医生
        public IActionResult SetIsDoctor(int doctorId, bool isDoctor)
        {
            return TryCatchFunc(() =>
            {
                var doctor = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                doctor.IsDoctor = isDoctor;
                _db.SaveChanges();
                return Json(MyDynamicResult(true, "设置为医生！"));
            });
        }
        //设置是否是测试医生
        public IActionResult SetIsDoctorOfPerson(int doctorId, bool isDoctor)
        {
            return TryCatchFunc(() =>
            {
                var doctor = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                doctor.IsDoctorOfPerson = isDoctor;
                _db.SaveChanges();
                return Json(MyDynamicResult(true, "设置为个人医生！"));
            });
        }



        //设置医生诊金
        public IActionResult SetDoctorTreatFee(int doctorId, decimal fee)
        {
            return TryCatchFunc(() =>
            {
                if (fee < 0 || fee > 10000) throw new Exception("诊金不得小于0或者大于1万");
                var doctor = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                doctor.TreatFee = fee;
                _db.SaveChanges();
                return Json(MyDynamicResult(true, "设置医生的接诊费用成功！"));
            });
        }

        //设置证书是否审核
        public IActionResult SetCertIsVerified(long certbookId, bool isVed = false)
        {
            return TryCatchFunc(() =>
            {
                var find = _db.CHIS_Code_DoctorCertbook.Find(certbookId);
                if (isVed)
                {
                    find.IsVerified = true; find.VerifiedTime = DateTime.Now;
                }
                else
                {
                    find.IsVerified = false; find.VerifiedTime = null;
                }
                _db.SaveChanges();
                return null;
            });
        }

        //设置医生的部门是否通过审核
        public IActionResult SetDoctorDepartIsVerifed(int departId, int doctorId, bool isVed = false)
        {
            return TryCatchFunc(() =>
            {
                var finds = _db.CHIS_Code_Rel_DoctorDeparts.Where(m => m.DepartId == departId && m.DoctorId == doctorId);
                foreach (var find in finds)
                {
                    if (isVed)
                    {
                        find.IsVerified = true; find.VerifiedTime = DateTime.Now;
                    }
                    else
                    {
                        find.IsVerified = false; find.VerifiedTime = null;
                    }
                }
                _db.SaveChanges();
                return null;
            });
        }
        public IActionResult SetDoctorDepartIsVerifedByKey(long dbid, bool isVed = false)
        {
            return TryCatchFunc(() =>
            {
                var find = _db.CHIS_Code_Rel_DoctorDeparts.Find(dbid);
                if (isVed)
                {
                    find.IsVerified = true; find.VerifiedTime = DateTime.Now;
                }
                else
                {
                    find.IsVerified = false; find.VerifiedTime = null;
                }

                _db.SaveChanges();
                return null;
            });
        }


        /// <summary>
        /// 设置医生某工作站的角色是否可用
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SetDoctorStationRole(int doctorId, int stationId, int roleId, bool isEnable)
        {
            return await TryCatchFuncAsync(async () =>
           {

               var finds = _db.CHIS_Sys_Rel_DoctorStationRoles.Where(m => m.DoctorId == doctorId);
               var f0 = finds.Where(m => m.StationId == stationId);
               var f1 = f0.Where(m => m.RoleId == roleId);

               //设置工作站权限
               if (f1.Count() == 0)
               {                    //新增
                   if (isEnable) await _db.AddAsync(new CHIS_Sys_Rel_DoctorStationRoles { DoctorId = doctorId, StationId = stationId, RoleId = roleId, MyRoleIsEnable = true, MyStationIsEnable = true });
               }
               else
               {                    //修改
                   var m0 = f1.AsNoTracking().FirstOrDefault();
                   if (isEnable)
                   {
                       m0.MyRoleIsEnable = true; m0.MyStationIsEnable = true;
                   }
                   else { m0.MyRoleIsEnable = false; }
                   _db.Update(m0);
               }
               await _db.SaveChangesAsync();

               //调整医生和授权的工作站
               var fs = _db.CHIS_Sys_Rel_DoctorStationRoles.Where(m => m.DoctorId == doctorId).ToList();// 找到新的数据
               var sEnabled = fs.Where(m => m.MyStationIsEnable == true && stationId > 0).Select(m => m.StationId.Value).ToList().Distinct();//找出允许的工作站
               var sDisabled = fs.Where(m => m.MyStationIsEnable == false && stationId > 0).Select(m => m.StationId.Value).ToList().Distinct();//找不禁止的工作站
                                                                                                                                               //如果禁止的工作站不在允许列表，则为真正禁止的工作站
               var sRealDisabled = sDisabled.Where(m => !sEnabled.Contains(m)).ToList();

               var ff0 = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.DoctorId == doctorId).ToList();
               var dblist = ff0.Select(m => m.StationId).Distinct().ToList();
               foreach (var item in ff0)
               {
                   //如果删除列表里包含这个Id，则删除[CHIS_Sys_Rel_DoctorStations]
                   if (sRealDisabled.Contains(item.StationId))
                   {
                       _db.Remove(item);
                   }
               }
               foreach (int s in sEnabled)
               {
                   //如果数据库里面不包含 工作站Id 则需要新增
                   if (!dblist.Contains(s))
                   {
                       await _db.AddAsync(new CHIS_Sys_Rel_DoctorStations
                       {
                           DoctorId = doctorId,
                           StationId = s,
                           StationIsEnable = true
                       });
                   }
               }
               await _db.SaveChangesAsync();
               return null;
           });
        }

        public IActionResult SetDoctorCheckingFinished(int doctorId)
        {
            return TryCatchFunc(() =>
            {
                var find = _db.CHIS_Code_Doctor.Find(doctorId);
                find.IsChecking = false;
                _db.SaveChanges();
                return null;
            });
        }

    }
}
