using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    [AllowAnonymous]
    public partial class ReservationController : BaseController
    {
        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        public ReservationController(DbContext.CHISEntitiesSqlServer db,
            Services.ReservationService resSvr,
            Services.WorkStationService stationSvr
            ) : base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
        }
        public IActionResult Appointment()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Mobile_Appointment()
        {
            return View();
        }
        /// <summary>
        /// 通过手机号码或证件号码查询用户
        /// <returns>成功则返回用户信息</returns>
        /// </summary>
        public IActionResult Json_GetCustomerList(string cusName, string searchNumber)
        {
            var cusList = new CustomerCBL(this).SearchCustomers(
              searchNumber.IsMobileNumber() ? null : searchNumber,
              cusName,
              searchNumber.IsMobileNumber() ? searchNumber : null).Select(m => new
              {
                  CustomerName = m.CustomerName,
                  CustomerId = m.CustomerID,
                  Gender = m.Gender,
                  MobileNumber = m.Telephone,
                  CustomerImage = m.PhotoUrlDef,// "/v20/images/user.png",
                  Birthday = m.Birthday,
                  Optime = m.OpTime,
                  IDCardNumber = m.IDcard,
                  CreateDate = m.CustomerCreateDate?.ToString("yyyy-MM-dd")
              });
            return Json(new { rlt = true, msg = "", items = cusList });
        }

        /// <summary>
        /// 查询门诊信息，并分页
        /// <returns>成功则返回用户信息</returns>
        /// </summary>
        public IActionResult Json_GetStationList(int pageIndexSearch = 1)
        {
            var pageIndex = pageIndexSearch;
            var pageSize = 3;

            var findList = _db.CHIS_Code_WorkStation.Where(m => m.IsCanTreat == true);
            int findTotal = findList.Count();
            int totalPage = (int)Math.Ceiling(findTotal * 1.0f / pageSize);
            //排序获取当前页的数据  
            var dataList = findList.OrderBy(m => m.StationName)
            .Skip(pageSize * (pageIndex - 1))
            .Take(pageSize).AsQueryable().ToList().Select(m => new
            {
                StationId = m.StationID,
                StationName = m.StationName,
                StationAddress = m.Address,
                StationPic = m.StationPic,
                StationDetail = m.StationRmk
            });
            return Json(new { rlt = true, msg = "选择门诊", items = dataList, totalPages = totalPage, pageIndex = pageIndex });
        }

        /// <summary>
        /// 通过门诊ID查询门诊信息
        /// <returns>成功则返回用户信息</returns>
        /// </summary>
        public IActionResult Json_departmemtIdByQuery(int stationId)
        {
            try
            {
                var station = _db.vwCHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == stationId);
                if (station == null) throw new Exception("没有该工作站信息");
                var departmemtsList = BasicDataHelper.GetDepartment(stationId, DepartSelectTypes.TreatmentDepartments);
                return Json(
                    new
                    {
                        rlt = true,
                        msg = "选择门诊部门",
                        stationId = station.StationID,
                        stationName = station.StationName,
                        departmemtsList = departmemtsList
                    });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        /// <summary>
        /// 通过科室ID查询医生信息
        /// <returns>成功则返回用户信息</returns>
        /// </summary>
        public IActionResult Json_GetDoctorOfDepartment(int departmentId)
        {
            try
            {
                var docList = _db.CHIS_Code_Rel_DoctorDeparts.Where(m => m.DepartId == departmentId);//////
                return Json(new { rlt = true, msg = "", docList = docList });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        /// <summary>
        /// 获取医生出勤信息
        /// </summary>
        public IActionResult Json_GetDoctorWorkInfos(int doctorId, int stationId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {

                dateFrom = (dateFrom.Date <= DateTime.Now.Date ? DateTime.Now.AddDays(0).Date : dateFrom.Date);
                if (dateTo <= dateFrom ||
                    ((dateTo - DateTime.Now).TotalDays > 16))
                    dateTo = DateTime.Now.AddDays(16).Date;

                var finds = new DoctorCBL(this).InitalEmployeeOnDutyInfo(doctorId, stationId, dateFrom, dateTo).Select(
                    m => new Models.ViewModels.EmployeeWorkInfo
                    {
                        DoctorId = doctorId,
                        DoctorName = m.DoctorName,
                        WorkDate = m.ScheduleDate,
                        Work_Vacation = m.IsWorkSlot == false ? "VACATION" : "WORK",
                        SlotIndex = m.Slot.Value,
                        SlotTimeStart = m.FromTime.Value,
                        SlotTimeEnd = m.ToTime.Value,
                        AllowRegNum = m.MaxCount,
                        ReservateLimitNum = m.ReservateLimitNum,
                        ReservatedNum = m.ReservatedNum
                    }).ToList().OrderBy(m => m.WorkDate).OrderBy(m => m.SlotIndex);



                //所有考察的时间
                var tagDays = new List<DateTime>();
                for (int i = 0; i <= (dateTo - dateFrom).Days; i++)
                {
                    tagDays.Add(dateFrom.AddDays(i).Date);
                }

                //获取有上班日的时间表
                var workDayList = (from item in finds
                                   where item.IsWork
                                   orderby item.WorkDate
                                   select item.WorkDate.Date).Distinct();

                //不工作的时间
                var notWorkDays = from item in tagDays where !workDayList.Contains(item) select item.ToString("yyyy-MM-dd");


                return Json(new
                {
                    rlt = true,
                    msg = "医生的上班情况",
                    items = finds,
                    workDays = workDayList,
                    notWorkDays = notWorkDays
                });

            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        //获取医生当日班次剩余预约数量等信息
        public IActionResult Json_GetDoctorReservationInfo(int doctorId, DateTime date, int slotNum)
        {
            try
            {

                var findDutys = new DoctorCBL(this).GetEmployeeOnDutyInfo(doctorId, date, slotNum);
                int restReservatedNum = findDutys.ReservateLimitNum - findDutys.ReservatedNum;//剩余预约数量     

                if (date.Date == DateTime.Today)//如果是当日
                {
                    restReservatedNum = findDutys.MaxCount + findDutys.AddCount - findDutys.TodayRegistedNum;
                }
                bool isFull = (restReservatedNum <= 0);

                return Json(new
                {
                    rlt = true,
                    msg = "",
                    isWorkSlot = findDutys.IsWorkSlot.Value,
                    isFull = isFull,
                    restReservatedNum = restReservatedNum
                });

            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }



        /// <summary>
        /// 手机预约信息，并返回给用户
        /// <returns>成功则返回用户信息</returns>
        /// </summary>
        public async Task<IActionResult> Json_GetReservationInfo(int customerID, int departmentID, int doctorId,
            DateTime reservationDate, int reservationSlot, int? registOpId = null, string registOpMan = "",int? rxDoctorId=null)
        {
            try
            {
                if (!registOpId.HasValue)
                {
                    try { registOpId = UserSelf.OpId; registOpMan = UserSelf.OpManFullMsg; }
                    catch { }
                }
                //var u = UserLoginData;
                //姓名
                var cust = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == customerID);
                //科室
                var depart = _db.vwCHIS_Code_Department.FirstOrDefault(m => m.DepartmentID == departmentID);
                if (depart == null) throw new Exception("科室没有找到");
                //医生
                var employee = _db.vwCHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                var workInfo = new DoctorCBL(this).GetEmployeeOnDutyInfo(doctorId, reservationDate, reservationSlot);
                if (workInfo == null) workInfo = new DoctorCBL(this).InitalEmployeeOnDutyInfo(doctorId, depart.DepartmentID, reservationDate, reservationDate.AddDays(7)).FirstOrDefault();
                var timestr = $"{workInfo.FromTime?.ToString(@"hh\:mm")}—{workInfo.ToTime?.ToString(@"hh\:mm")}";
                 
                var add = await _resSvr.AddOneRegister(cust.CustomerID, depart.DepartmentID, doctorId, depart.StationID.Value, reservationDate, reservationSlot, RegistFrom.V20Web
                    , opId: registOpId, opMan: registOpMan,rxDoctorId:rxDoctorId
                    );


                //给用户发送短信
                StringBuilder b = new StringBuilder();
                b.Append($"尊敬的{cust.CustomerName},请于{reservationDate.ToString("yyyy年MM月dd日")} {timestr} 至{depart.DepartmentName}({employee.DoctorName})医生处就诊，感谢您的预约。【天使健康】");
                Codes.Utility.SMS sms = new Codes.Utility.SMS();
                //await new SendVCodeCBL(this).SendVCode(cust.Telephone, b.ToString());
                var rlt = await sms.PostSmsInfoAsync(cust.CustomerMobile, b.ToString());
                return Json(new
                {
                    rlt = true,
                    msg = "",
                    rltCode = add.HaveRegisted ? "REPET_REGIST" : "",
                    registStatus = add.HaveRegisted ? "请注意,您已经预约了,不能重复预约。先前预约信息如下。" : "",
                    registerId = add.RegisterID,
                    registerSeq = add.RegisterSeq,
                    customer = cust,
                    stationName = depart.StationName,
                    departmentName = depart.DepartmentName,
                    employee = employee,
                    reservationDate = reservationDate.ToString("yyyy-MM-dd"),
                    timeInfo = timestr
                });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }

        }

 

    }

}