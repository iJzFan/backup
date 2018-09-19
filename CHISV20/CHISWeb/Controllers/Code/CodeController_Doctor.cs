using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Ass;
using CHIS.Codes.Utility;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    public partial class CodeController
    {
   

        #region 医生档案
        public IActionResult CHIS_Code_Doctor()
        {
            ViewBag.StationID = User.FindFirst("StationId").Value;
            return View();
        }


        public IActionResult CHIS_Code_Doctor2()
        {
            ViewBag.StationID = User.FindFirst("StationId").Value;
            return View();
        }

        public async Task<IActionResult> LoadDoctorsOfMyStation(string searchText, int? stationId, string timeRange = "All", bool? isVed = null, int pageIndex = 1, int pageSize = 20)
        {
            initialData_Page(ref pageIndex, ref pageSize);
            DateTime? dt0 = null, dt1 = null;
            initialData_TimeRange(ref dt0, ref dt1, timeRange);
            if (!stationId.HasValue) stationId = UserSelf.StationId;

            //bool btimeRange = timeRange != "All";
            //var ds = await new CHIS.DAL.Doctor().LoadDoctorListAsync(searchText, stationId, dt0.Value, dt1.Value, isVed,btimeRange, pageIndex, pageSize);
            //return PartialView("_pvDoctorsOfMyStationList2", new Ass.Mvc.PageListInfoDs(ds));

            var find = new DoctorCBL(this).queryDoctorsOfMyStation(stationId.Value);
            if (timeRange != "All") find = find.Where(m => m.DoctorCreateTime >= dt0 && m.DoctorCreateTime < dt1);
            if (isVed.HasValue) find = find.Where(m => m.DoctorIsAuthenticated == isVed.Value);
            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) find = find.Where(m => m.CustomerMobile == t.String);
                else if (t.IsEmail) find = find.Where(m => m.Email == t.String);
                else if (t.IsIdCardNumber) find = find.Where(m => m.IDcard == t.String);
                else if (t.IsLoginNameLegal) find = find.Where(m => m.LoginName == t.String);
                else find = find.Where(m => m.DoctorName == t.String);
            }
            var total = find.Count();
            find = find.OrderBy(m => m.DoctorId).Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var list = await find.ToListAsync();
            var model = new Ass.Mvc.PageListInfo<Models.vwCHIS_Code_Doctor>
            {
                DataList = list,
                RecordTotal = total,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return PartialView("_pvDoctorsOfMyStationList", model);
        }

        public IActionResult LoadDocotrRowDetail(int doctorId)
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
            return PartialView("_pvDoctorsOfMyStationRowDetail", model);
        }

        #endregion


        //员工档案记录查询
        public IActionResult SearchJson_CHIS_Code_Doctor(int? stationId = 0, string keyword = null, bool isOnlyThisStation = true, bool ignoreStation = false)
        {
            //筛选数据
            if (ignoreStation)
            {
                var findList = _db.vwCHIS_Code_Doctor.AsNoTracking().Select(tt => new
                {
                    DoctorId = tt.DoctorId,
                    DoctorName = tt.DoctorName,
                    Gender = tt.Gender,
                    Birthday = tt.Birthday,
                    IDcard = tt.IDcard,
                    Telephone = tt.Telephone,
                    Mobile = tt.CustomerMobile,
                    Email = tt.Email,
                    CreatDate = Convert.ToDateTime(tt.DoctorCreateTime).ToString("yyyy-MM-dd")
                });
                if (!string.IsNullOrEmpty(keyword)) findList = findList.Where(m => m.Telephone == keyword || m.IDcard == keyword || m.Email == keyword || m.DoctorName == keyword);
                return FindPagedData_jqgrid(findList.OrderBy(m => m.DoctorId));
            }
            else
            {
                var stations = isOnlyThisStation ? null : UserMgr.GetStations(stationId.Value);
                var findList = (from item in _db.CHIS_Sys_Rel_DoctorStations
                                join doctor in _db.vwCHIS_Code_Doctor on item.DoctorId equals doctor.DoctorId into temp
                                from tt in temp.DefaultIfEmpty()
                                where (stations == null) ? (item.StationId == stationId) : (stations.Contains(item.StationId)) && tt != null
                                select new
                                {
                                    DoctorId = tt.DoctorId,
                                    DoctorName = tt.DoctorName,
                                    Gender = tt.Gender,
                                    Birthday = tt.Birthday,
                                    IDcard = tt.IDcard,
                                    Telephone = tt.Telephone,
                                    Mobile = tt.CustomerMobile,
                                    Email = tt.Email,
                                    CreatDate = Convert.ToDateTime(tt.DoctorCreateTime).ToString("yyyy-MM-dd")
                                }).Distinct();
                if (!string.IsNullOrEmpty(keyword)) findList = findList.Where(m => m.Telephone == keyword || m.IDcard == keyword || m.Email == keyword || m.DoctorName == keyword);
                return FindPagedData_jqgrid(findList.OrderByDescending(m => m.CreatDate)); //分页返回
            }


        }




        //编辑的页面操作 op=NEWF/NEW/MODIFYF/MODIFY/DELETE 
        public IActionResult CHIS_Code_Doctor_Edit(string op, Models.DataModel.Doctor model, int recId)
        {
            //todo
            try
            {

                string editViewName = nameof(CHIS_Code_Doctor_Edit);
                //   var sysUser = base.GetCurrentUserInfo();
                ViewBag.OP = op;// 初始化操作类别             
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面
                        var modelnew = new Models.DataModel.Doctor() { };
                        ViewBag.OP = "NEW";
                        return View(editViewName, modelnew);
                    case "NEW": // 更新新增的数据
                        _db.BeginTransaction();
                        try
                        {
                            if (model.BaseInfo.CustomerID <= 0) throw new Exception("该用户不存在，不能添加医生信息");
                            var docList = _db.CHIS_Code_Doctor.Where(m => m.CustomerId == model.BaseInfo.CustomerID);
                            if (docList.Count() > 0) throw new Exception("该医生已存在，不能重复添加");
                            model.DoctorInfo.CustomerId = model.BaseInfo.CustomerID;
                            _db.CHIS_Code_Doctor.Add(model.DoctorInfo);
                            _db.SaveChanges();
                            _db.CommitTran();
                            return Json(new { state = "success" });
                        }
                        catch (Exception ex)
                        {
                            _db.RollbackTran();
                            return Json(new { state = "error", msg = "添加失败" + ex.Message });
                        }

                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        var d = _db.CHIS_Code_Doctor.Find(recId);
                        var c = _db.CHIS_Code_Customer.First(m => m.CustomerID == d.CustomerId);
                        var modelmodify = new Models.DataModel.Doctor();
                        modelmodify.BaseInfo = c;
                        modelmodify.DoctorInfo = d;
                        ViewBag.OP = "MODIFY";
                        return View(editViewName, modelmodify);
                    case "MODIFY": //修改后的数据
                        _db.BeginTransaction();

                        try
                        {
                            var a = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == model.BaseInfo.CustomerID);
                            if (a == null) throw new Exception("没有医生的基本信息");
                            a.CustomerName = model.BaseInfo.CustomerName;
                            a.Gender = model.BaseInfo.Gender;
                            a.Email = model.BaseInfo.Email;
                            a.Birthday = model.BaseInfo.Birthday;
                            a.IDcard = model.BaseInfo.IDcard;
                            a.EduLevel = model.BaseInfo.EduLevel;
                            a.Telephone = model.BaseInfo.Telephone;
                            a.Address = model.BaseInfo.Address;

                            var b = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == model.DoctorInfo.DoctorId);
                            b.CustomerId = a.CustomerID;
                            b.EmployeeCode = model.DoctorInfo.EmployeeCode;
                            b.DutyState = model.DoctorInfo.DutyState;
                            b.IsEnable = model.DoctorInfo.IsEnable;
                            b.OnDutyDate = model.DoctorInfo.OnDutyDate;
                            b.StopDate = model.DoctorInfo.StopDate;
                            b.Remark = model.DoctorInfo.Remark;


                            _db.SaveChanges();
                            _db.CommitTran();
                            return Json(new { state = "success" });
                        }
                        catch (Exception ex) { _db.RollbackTran(); return Json(new { state = "error", msg = "修改失败" + ex.Message }); }

                    case "DELETE": //删除，返回json 
                        //var del = MainDbContext.CHIS..Find(recId);
                        var dc = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == recId);
                        _db.Remove(dc);
                        var rlt = _db.SaveChanges() > 0;
                        return Json(rlt);
                    case "VIEW":
                        var d2 = _db.CHIS_Code_Doctor.Find(recId);
                        var c2 = _db.CHIS_Code_Customer.First(m => m.CustomerID == d2.CustomerId);
                        if (c2.CustomerID <= 0) throw new Exception("不存在该医生基本信息，不能进行修改！");
                        var viewDoc = new Models.DataModel.Doctor();
                        viewDoc.BaseInfo = c2;
                        viewDoc.DoctorInfo = d2;
                        return View(editViewName, viewDoc);
                    default:
                        throw new Exception("错误的命令");
                }
            }
            catch (Exception ex)
            {
                // Loger.WriteError("Code", "CHIS_Code_EmployeeMsg_Edit", ex);
                return View("Error", ex);
            }
        }



        #region 设置医生信息
        //第1步：用户选择 入口
        public IActionResult DoctorSets_SelectCustomer(int doctorId, int? customerId, string op = "NEWF")
        {
            var doc = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            ViewBag.op = op;
            ViewBag.doctorId = doctorId;
            ViewBag.customerId = customerId ?? (doc?.CustomerId);
            return View(doc);
        }
        //第2步：医生基础信息
        public IActionResult DoctorSets_DoctorBase(int? doctorId, int? customerId, string op)
        {
            if (!(doctorId > 0 || customerId > 0)) throw new Exception("没有传入医生Id或者用户Id");
            var model = new Models.DataModel.Doctor();
            if (doctorId > 0)
            {
                var d = _db.CHIS_Code_Doctor.Find(doctorId);
                var c = _db.CHIS_Code_Customer.First(m => m.CustomerID == d.CustomerId);
                model.BaseInfo = c;
                model.DoctorInfo = d;
            }
            else
            {
                model.BaseInfo = _db.CHIS_Code_Customer.First(m => m.CustomerID == customerId.Value);
                model.DoctorInfo = new Models.CHIS_Code_Doctor();
                model.DoctorInfo.TreatFee = 10.00m;
            }
            ViewBag.op = op;
            ViewBag.doctorId = doctorId;
            ViewBag.customerId = customerId;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DoctorSets_DoctorBase_Save(int? doctorId, int? customerId, string op, CHIS.Models.DataModel.Doctor model)
        {
            if (!(customerId > 0)) throw new Exception("必须传入用户Id");
            ViewBag.op = op;
            ViewBag.doctorId = doctorId;
            ViewBag.customerId = customerId;

            if (ModelState.IsValid)
            {
                model.DoctorInfo.CustomerId = customerId;
                Models.vwCHIS_Code_Doctor doc = null;
                if (doctorId > 0) //修改
                {
                    doc = await new DoctorCBL(this).ModifyDoctorAsync(model.DoctorInfo);
                }
                else
                {
                    doc = await new DoctorCBL(this).CreateDoctorAsync(model.DoctorInfo);
                }
                return RedirectToAction("DoctorSets_StationRoleDepart", new { doctorId = doc.DoctorId, customerId = customerId, op = op });
            }
            else return View(model);
        }



        //第3步：诊所角色科室设置
        #region 诊所角色科室设置
        public IActionResult DoctorSets_StationRoleDepart(int? doctorId, int? customerId, string op)
        {
            if (!(doctorId > 0)) throw new Exception("传入的医生Id不正确");
            var model = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            if (model == null) throw new Exception("没有找到该医生的信息");
            ViewBag.op = op;
            ViewBag.doctorId = doctorId;
            ViewBag.customerId = customerId;
            var s = GetDoctorStationInfo(doctorId.Value, UserSelf.StationId);
            ViewBag.DoctorStationInfo = s;
            return View(model);
        }
        public async Task<IActionResult> DoctorSet_AddRole(CHIS.Models.ViewModel.StationRolesDepartsItem item, int doctorId)
        {
            await new DoctorCBL(this).UpsertStationRolesAndDepartsAsync(item, doctorId);//添加
            var model = GetDoctorStationInfo(doctorId, UserSelf.StationId);
            return getDoctorStationRoleDepartsPartialView(doctorId);
        }

        public async Task<IActionResult> DoctorSet_DeleteStation(int doctorId, int stationId)
        {
            await new DoctorCBL(this).DeleteStationOfDoctorAsync(doctorId, stationId);
            var model = GetDoctorStationInfo(doctorId, UserSelf.StationId);
            return getDoctorStationRoleDepartsPartialView(doctorId);
        }
        public async Task<IActionResult> DoctorSet_DeleteDepart(int doctorId, int stationId, int departId)
        {
            await new DoctorCBL(this).DeleteDepartmentOfDoctorAsync(doctorId, departId);
            var model = GetDoctorStationInfo(doctorId, UserSelf.StationId);
            return getDoctorStationRoleDepartsPartialView(doctorId);
        }
        public async Task<IActionResult> DoctorSet_DeleteRole(int doctorId, int stationId, int roleId)
        {
            await new DoctorCBL(this).DeleteRoleOfDoctorAsync(doctorId, stationId, roleId);
            return getDoctorStationRoleDepartsPartialView(doctorId);
        }
        private PartialViewResult getDoctorStationRoleDepartsPartialView(int doctorId)
        {
            var model = GetDoctorStationInfo(doctorId, UserSelf.StationId);
            return PartialView("_pvDoctorStationRoleDeparts", model);
        }

        private IEnumerable<Models.ViewModel.DoctorStationInfo> GetDoctorStationInfo(int doctorId, int rootStationId)
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

            var allowStations = UserMgr.GetAllowedStationsAndSubStationsQuery(_db, doctorId, rootStationId).ToList();
            var stationids = stationRoles.Where(m => m.MyStationIsEnable == true).Select(m => m.StationId.Value).Distinct().ToList();
            //  stationids.AddRange(stationRoles.Select(m => m.StationId.Value).Distinct());
            stationids = (from item in stationids.Distinct()
                          where allowStations.Contains(item)
                          select item).ToList();
            var stations = _db.CHIS_Code_WorkStation.AsNoTracking().Where(m => stationids.Contains(m.StationID)).ToList();

            var model = new Models.ViewModel.DoctorInfo
            {
                Doctor = doc,
                Customer = cus
            };
            model.InitialStationInfo(dep, stationRoles, roles, stations);
            return model.StationInfos;
        }

        public IActionResult DoctorSets_StationRoleDepart_Save(int? doctorId, int? customerId, string op)
        {
            if (!(doctorId > 0)) throw new Exception("传入的医生Id不正确");
            //var model = MainDbContext.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            //if (model == null) throw new Exception("没有找到该医生的信息");             
            ViewBag.op = op;
            ViewBag.doctorId = doctorId;
            ViewBag.customerId = customerId;
            return RedirectToAction("DoctorSets_Login", new { doctorId = doctorId, customerId = customerId, op = op });

        }

        #endregion

        //第4步：登录设置
        public async Task<IActionResult> DoctorSets_Login(int? doctorId, int? customerId, string op)
        {
            if (!(doctorId > 0)) throw new Exception("必须传入医生Id");
            if (!(customerId > 0)) throw new Exception("必须传入用户Id");

            var doc = await _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefaultAsync(m => m.DoctorId == doctorId);
            var login = await _db.CHIS_Sys_Login.AsNoTracking().FirstOrDefaultAsync(m => m.CustomerId == customerId);

            ViewBag.op = op;
            ViewBag.doctorId = doctorId;
            ViewBag.customerId = customerId;
            return View(new Models.DataModel.DoctorLogin
            {
                Doctor = doc,
                Login = login
            });
        }
        public async Task<IActionResult> DoctorSets_Login_Save(int? doctorId, int? customerId, string op, CHIS.Models.DataModel.DoctorLogin model)
        {
            if (!(doctorId > 0)) throw new Exception("必须传入医生Id");
            if (!(customerId > 0)) throw new Exception("必须传入用户Id");

            try
            {
                model.Login.DoctorId = model.IsLoginToDoctor ? doctorId : null;
                bool rlt = await new DoctorCBL(this).SaveDoctorLoginAsync(model.Login);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("DoctorSets_Login", model);
            }
            return RedirectToAction("DoctorSets_ForCheck", new { doctorId = doctorId, customerId = customerId, op = op });
        }
        //第4步：完成 提交审核
        public async Task<IActionResult> DoctorSets_ForCheck(int? doctorId, int? customerId, string op)
        {
            if (!(doctorId > 0)) throw new Exception("必须传入医生Id");
            var doc = await _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefaultAsync(m => m.DoctorId == doctorId);
            ViewBag.op = op;
            ViewBag.doctorId = doctorId;
            ViewBag.customerId = customerId;
            return View(doc);
        }


        //删除一个医生用户
        public async Task<IActionResult> DoctorSets_Delete(int doctorId)
        {
            //需要判断权限
            return await TryCatchFuncAsync(async () =>
           {
               await _docrSvr.DeleteAsync(doctorId);               
               return null;
           });


        }

        #endregion








        /*

      //根据StationId获取部门的列表Json
      public IActionResult GetStationDepartments(int? sid)
      {
          if (!sid.HasValue) return null;
          var rlt = MainDbContext.CHIS_Code_Department.Where(m => m.StationID == sid);
          var rtn = from item in rlt
                    orderby item.ShowOrder
                    select new
                    {
                        Text = item.DepartmentName,
                        Value = item.DepartmentID
                    };
          return Json(rtn);
      }

      /// <summary>
      /// 校验身份证是否输入正确
      /// </summary>
      /// <param name="IDCard">身份证号</param>
      /// <param name="employeeID">员工ID</param>
      /// <returns></returns>
      public IActionResult CheckIDCardValidate(string idCard, int employeeID)
      {
          bool validate = false; string msg = "";
          string birthday = "", provinceNo = "";
          int sex = 0, province = 0;

          try
          {
              if (string.IsNullOrEmpty(idCard))
                  msg = "请输入身份证号！";
              else if (idCard.Length != 18)
                  msg = "身份证位数错误！";
              else
              {
                  int check = int.Parse(MainDbContext.MySqlFunction("dbo.fn_CheckIDCardValidate",
                      new SqlParameter("@IDCard", idCard)).ToString());
                  if (check != 1)
                      msg = "身份证输入错误！";
                  else
                  {
                      //判断是否重复
                      var result = MainDbContext.CHIS_Code_EmployeeMsg.Where(m => m.IDcard == idCard && m.EmployeeID != employeeID);
                      if (result.Count() >= 1)
                          msg = "此身份证人员已存在！";
                      else
                      {
                          validate = true;
                          birthday = idCard.Substring(6, 4) + "-" + idCard.Substring(10, 2) + "-" + idCard.Substring(12, 2);
                          sex = Ass.P.PInt(idCard.Substring(16, 1)) % 2;
                          provinceNo = idCard.Substring(0, 2);
                          province = CHIS.Static.GetDictDetailIDByKey("Province", provinceNo);
                      }
                  }
              }
          }
          catch (Exception ex)
          {
              msg = ex.Message; validate = false;
              Loger.WriteError("Code", "CheckIDCardValidate", ex);
          }
          return Json(new
          {
              validate = validate,  //是否校验正确
              birthday = birthday,  //生日 
              province = province,  //省份
              sex = sex,            //性别
              msg = msg             //错误说明 
          });
      }




      */
        //        /// <summary>
        //        /// 员工档案记录导出
        //        /// </summary>
        //        /// <returns></returns>
        //        public IActionResult Export_EmployeeMsg()
        //        {
        //            //jqgrid公共取值
        //            var user = base.GetCurrentUserInfo();
        //            int pageIndex = 1, pageSize = 0; string sort = "";
        //            base.getJqGridInfo(out pageIndex, out pageSize, out sort, 1, user.TableRecordsPerPage);

        //            //查询条件
        //            //查询条件
        //            string s_Station = getRequestParms("s_Station");
        //            string s_EmployeeCode = getRequestParms("s_EmployeeCode");
        //            string s_EmployeeName = getRequestParms("s_EmployeeName");
        //            string s_Department = getRequestParms("s_Department");
        //            string s_Sex = getRequestParms("s_Sex");
        //            string s_FromDate = getRequestParms("s_Date_From"); DateTime fromDate = P.PDateTimeVDate(s_FromDate, -100, 0);
        //            string s_ToDate = getRequestParms("s_Date_To"); DateTime toDate = P.PDateTimeVDate(s_ToDate, 100, 0);
        //            if (string.IsNullOrWhiteSpace(sort)) sort = getForm("sort");
        //            try
        //            {
        //                //只显示具有对应工作站权限的人员记录
        //                var find = from a in MainDbContext.vwCHIS_Code_Employee
        //                           join
        //b in MainDbContext.CHIS_SYS_UserStationRight on a.StationID equals b.StationID
        //                           where b.UserID == user.OpID
        //                           select a;

        //                var findList = from item in find.Distinct()
        //                               where (s_Sex.Equals("") ? true : item.Sex.ToString() == s_Sex) &&
        //                                     (s_Station.Equals("") ? true : item.StationID.ToString() == s_Station) &&
        //                                     (s_EmployeeCode.Equals("") ? true : item.EmployeeCode.Contains(s_EmployeeCode)) &&
        //                                     (s_EmployeeName.Equals("") ? true : item.EmployeeName.Contains(s_EmployeeName)) &&
        //                                     (s_Department.Equals("") ? true : item.Department.ToString() == s_Department)
        //                               select item;
        //                var dataList = findList.OrderBy(m => m.EmployeeCode, sort);

        //                List<CHIS.Codes.Utility.ExcelField> map = new List<Codes.Utility.ExcelField>()
        //                {
        //                    new ExcelField("EmployeeID","员工ID") ,
        //                    new ExcelField("EmployeeCode","工号"),
        //                    new ExcelField("EmployeeName","姓名"),
        //                    new ExcelField("Sex","性别","[男,1][女,0]"),   //选择列转换列表
        //                    new ExcelField("Birthday","出生日期"),
        //                    new ExcelField("IDcard","身份证号"),
        //                    new ExcelField("DepartmentName","科室名称"),
        //                    new ExcelField("PrincipalshipName","职务"),
        //                    new ExcelField("EduLevelName","学历"),
        //                    new ExcelField("Address","地址"),
        //                    new ExcelField("Telephone","电话"),
        //                    new ExcelField("ContactMan","紧急联系人"),
        //                    new ExcelField("ContactPhone","紧急联系电话"),
        //                    new ExcelField("OnDutyDate","入职日期"),
        //                    new ExcelField("OnDutyDate","转正日期"),
        //                    new ExcelField("OutDutyDate","离职日期"),
        //                    new ExcelField("IsEanble","可用状态","[可用,True][停用,False]"),
        //                    new ExcelField("StopDate","停用日期"),
        //                    new ExcelField("Telephone","工作站名称"),
        //                    new ExcelField("OpMan","操作人"),
        //                    new ExcelField("OpTime","操作时间"),
        //                    new ExcelField("Remark","备注")
        //                };

        //                Loger.WriteInfo("Code", "Export_EmployeeMsg", "导出员工档案");
        //                System.IO.Stream sm = CHIS.Codes.Utility.Excel.CreateExcelStream(dataList, "员工档案", map);
        //                DateTime dt = DateTime.Now;
        //                string dateTime = dt.ToString("yyMMddHHmmssfff");
        //                string filename = "Export" + dateTime + ".xls";
        //                return File(sm, "application/vnd.ms-excel", filename);
        //            }
        //            catch (Exception ex)
        //            {
        //                Loger.WriteError("CODE", "Export_EmployeeMsg", ex);
        //                return View("ErrorBlank", ex);
        //            }
        //        }


        //设置人员的权限
        [FuncAccessFilter(FuncAccessKey = "IsAllowedToSetDoctorAccess")]
        public IActionResult setDoctorAccess(int doctorId)
        {
            ViewBag.Doctor = _db.vwCHIS_Code_Doctor.Find(doctorId);
            ViewBag.MyRoles = new CHIS.Api.syshis(_db).StationsRolesOfDoctor(doctorId).ToList();
            return View();
        }
        public IActionResult UpsertDoctorStationRoles(int doctorId, int stationId, List<int> roles)
        {
            try
            {
                var rlt = new CHIS.Api.syshis(_db).UpsertDoctorStationRoles(doctorId, stationId, roles);
                return Json(new { rlt = rlt, msg = "更新角色成功" });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }




    }
}
