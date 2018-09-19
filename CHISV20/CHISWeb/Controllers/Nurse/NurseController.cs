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

namespace CHIS.Controllers
{
    public partial class NurseController : BaseController
    {
        Services.CustomerService _cusSvr;
        Services.WorkStationService _staSvr;
        Services.DoctorService _docSvr;
        Services.LoginService _loginSvr;
        Services.RxService _rxSvr;
        public NurseController(DbContext.CHISEntitiesSqlServer db
            ,Services.CustomerService cusSvr
            ,Services.WorkStationService staSvr
            ,Services.DoctorService docSvr
            ,Services.LoginService loginSvr
            ,Services.RxService rxSvr
            ) : base(db) {
            _cusSvr = cusSvr;
            _staSvr = staSvr;
            _docSvr = docSvr;
            _loginSvr = loginSvr;
            _rxSvr = rxSvr;
        }
        public IActionResult Register()
        {
             var treatDeparts = _staSvr.GetTreatDepartmentOfStation(UserSelf.StationId).Select(m => new Ass.KeyValueItem { Value = m.DepartmentID, KeyItem = m.DepartmentName }).ToList();
            var doctors=  _staSvr.GetDoctorsOfStation(UserSelf.StationId,null,1,100).Select(m => new Ass.KeyValueItem { Value = m.DoctorId, KeyItem = m.DoctorName }).ToList();
            ViewBag.Departs = treatDeparts;
            ViewBag.Doctors = doctors;
            return View();
        }

        public IActionResult LoadPvNewRegist(int stationId, long customerId)
        {
            var cus = _db.vwCHIS_Code_Customer.AsNoTracking().FirstOrDefault(m => m.CustomerID == customerId);
            return PartialView("_pvAddRegister", new NewRegistViewModel
            {
                StationId = stationId,
                CustomerId = customerId,
                DoctorId = null,
                ReservationSlot = null,
                Customer = cus,
                Allergic = cus.Allergic,
                PastMedicalHistory = cus.PastMedicalHistory,
                ReservationDate = DateTime.Today
            });
        }
        public IActionResult LoadPvAddCustomer(string customerKeyText)
        {
            var s = Ass.P.PStr(customerKeyText).GetStringType();
            var model = new RegistNewCustomerViewModel
            {
                CustomerKeyText = customerKeyText,
                Customer = new Models.vwCHIS_Code_Customer
                {
                    IDcard = s.IsIdCardNumber ? s.String : "",
                    CustomerMobile = s.IsMobile ? s.String : "",
                    Email = s.IsEmail ? s.String : ""
                }
            };
            return PartialView("_pvRegisterAddCustomer", model);
        }
        public async Task<IActionResult> AddCustomer(RegistNewCustomerViewModel model)
        {
        
            return await TryCatchFuncAsync(async (dd) =>
           {
               if (!ModelState.IsValid) throw new Exception(base.GetErrorOfModelState(ModelState));
               model.Customer.sysSource = sysSources.CHIS约号快增.ToString();
               var cus = await _cusSvr.CreateCustomerAsync(model.Customer,UserSelf.OpId,UserSelf.OpMan);
               dd.stationId = UserSelf.StationId;
               dd.customerId = cus.CustomerID;
               return null;
           });
        }

        public IActionResult SearchRegistList(string searchText, string timeRange = "Today", int? stationId = null, int? departId = null, int? doctorId = null, int pageIndex = 1, int pageSize = 20)
        {
            DateTime dt0 = DateTime.Today, dt1 = DateTime.Now;
            base.initialData_TimeRange(ref dt0, ref dt1, timeRange);
            base.initialData_Page(ref pageIndex, ref pageSize);
            if (stationId == null) stationId = UserSelf.StationId;

            //如果角色是药店护士
            int? registOpId = null;
            if (UserSelf.MyRoleNames.Contains("drugstore_nurse"))
            {
                registOpId = UserSelf.OpId;
            }
            var model = new BllCaller.TreatRegistBllCaller().SearchRegistList(searchText, dt0, dt1, stationId, departId, doctorId,registOpId, pageIndex, pageSize);
            return PartialView("_pvRegistList", model);
        }



    }
}
