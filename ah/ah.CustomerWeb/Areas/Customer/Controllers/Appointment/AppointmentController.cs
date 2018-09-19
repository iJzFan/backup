
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ah.Models;
using Ass;
using System.Threading.Tasks;
using ah.Code;
using Microsoft.AspNetCore.Cors;
using System.Net.Http;
using Newtonsoft.Json;
using ah.Models.ViewModel;
using ah.Services;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Areas.Customer.Controllers
{
    public partial class AppointmentController : BaseController
    {
        private FollowListService _followService;

        private GetInfoService _getInfoService;

        public AppointmentController(ah.DbContext.AHMSEntitiesSqlServer db, FollowListService followService, GetInfoService getInfoService) : base(db)
        {
            _followService = followService;

            _getInfoService = getInfoService;
        }

        /// <summary>
        /// 预约首页
        /// </summary>
        /// <param name="stationId">工作站Id</param>
        /// <param name="doctorId">医生Id</param>
        [AllowAnonymous]
        [ResponseCache(Duration = 1000)]
        public IActionResult Index(int? stationId = null, int? departId = null, int? doctorId = null)
        {
            var model = new ah.Models.ViewModel.AppointmentViewModel();
            //检测是否可以预约
            if (stationId.IsLegalDbId())
            {
                var fd = MainDbContext.CHIS_Code_WorkStation.AsNoTracking().FirstOrDefault(m => m.StationID == stationId && m.IsEnable == true && m.IsCanTreat == true);
                if (fd == null) throw new ChkException("STATION_IS_ILLEGAL", "该工作站不能被接诊预约，没有发现此工作站，或者禁用，或者非接诊的工作站");
                model.StationName = fd.StationName;
            }
            if (departId.IsLegalDbId())
            {
                if (MainDbContext.CHIS_Code_Department.Where(m => m.DepartmentID == departId && m.IsEnable == true && m.IsNotTreatDept != true).Count() == 0)
                    throw new ChkException("DEPART_IS_ILLEGAL", "该科室部门不能被接诊预约，没有发现此科室，或者已经禁用，或者非接诊的科室");
            }
            if (doctorId.IsLegalDbId())
            {
                if (MainDbContext.CHIS_Code_Rel_DoctorDeparts.Where(m => m.DepartId == departId && m.DoctorId == doctorId && m.IsVerified).Count() == 0)
                    throw new ChkException("NO_DOCTOR_IN_DEPART", "该部门下没有医生的信息");
            }
            model.StationId = stationId;
            model.DepartId = departId;
            model.DoctorId = doctorId;
            model.Date = DateTime.Now.ToDateString();
            model.Slot = DateTime.Now > DateTime.Today.AddHours(12) ? 2 : 1;
            return View("Index", model);
        }
        //预约填写基本信息
        [AllowAnonymous]
        [ResponseCache(Duration = 1000)]
        public IActionResult IndexStep2(string stationName, int stationId, int? departId = 0, int? doctorId = null)
        {
            ViewBag.stationName = stationName;
            ViewBag.stationId = stationId;
            ViewBag.departId = departId;
            ViewBag.doctorId = doctorId;
            return View("Index_Step2");
        }
        //预约填写基本信息
        //[AllowAnonymous]
        [ResponseCache(Duration = 1000)]
        public async Task<IActionResult> SelectDoctorInfo(int doctorId)
        {
            var modelList = await _getInfoService.GetDoctorsInfosAsync(new int[] { doctorId });

            if (!modelList.Rlt)
            {
                return View("Error");
            }

            var followList = await _followService.GetListAsync(GetCurrentLoginUser.CustomerId);

            var model = modelList.Items.FirstOrDefault()??new DoctorSimpleInfo();

            if (followList.FollowDoctorIds.Contains(model.DoctorId))
            {
                model.IsFollow = true;
            }

            return View("selectDoctorInfo", model);
        }
        //获取预约用户的信息
        public IActionResult CustomerApppintenmt()
        {
            return null;
        }
        //填写心理问卷
        [AllowAnonymous]
        public IActionResult PsyQuestionnaire(int customerId, int doctorId)
        {
            //医生(暂时先命名为model)
            var doctor = MainDbContext.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            var cus = MainDbContext.vwCHIS_Code_Customer.AsNoTracking().FirstOrDefault(m => m.CustomerID == customerId);
            var model = new ah.Models.vwCHIS_Data_PsychPretreatQs
            {
                DoctorId = doctorId,
                DoctorName = doctor.DoctorName,

                CustomerId = customerId,
                CustomerName = cus.CustomerName,
                CustomerMobile = cus.CustomerMobile,
                Address = cus.Address,
                AddressAreaId = cus.AddressAreaId,
                AliveChildNum = cus.AliveChildNum,
                BirthChildrenNum = cus.BirthChildrenNum,
                PregnancyNum = cus.PregnancyNum,
                Gender = cus.Gender,
                Birthday = cus.Birthday,
                Occupation = cus.Occupation,
                EduLevel = cus.EduLevel,
                TopSchool = cus.TopSchool,
                Religion = cus.Religion,
                MarriageId = cus.Marriage
            };
            return View(nameof(PsyQuestionnaire), model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult PsyQuestionnaire(vwCHIS_Data_PsychPretreatQs model)
        {
            return TryCatchFunc((dd) =>
            {

                if (model.CustomerId == null || model.CustomerId <= 0) throw new Exception("请传入用户Id");
                if (model.DoctorId == null || model.DoctorId <= 0) throw new Exception("请传入医生Id");

                //更新同步患者数据
                var cus0 = MainDbContext.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == model.CustomerId);
                var cus1 = MainDbContext.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == model.CustomerId);

                if (model.EduLevel > 0) cus0.EduLevel = model.EduLevel.Value;//学历
                if (model.TopSchool.IsNotEmpty()) cus0.TopSchool = model.TopSchool;//学校
                if (model.Occupation.IsNotEmpty()) cus0.Occupation = model.Occupation;//职业
                if (model.Religion.IsNotEmpty()) cus0.Religion = model.Religion;//宗教
                if (model.MarriageId > 0) cus0.Marriage = model.MarriageId;//婚姻
                if (model.PregnancyNum > 0) cus1.PregnancyNum = model.PregnancyNum;
                if (model.BirthChildrenNum > 0) cus1.BirthChildrenNum = model.BirthChildrenNum;
                if (model.AliveChildNum > 0) cus1.AliveChildNum = model.AliveChildNum;

                if (model.AddressAreaId > 0) cus0.AddressAreaId = model.AddressAreaId;
                if (model.Address.IsNotEmpty()) cus0.Address = model.Address;
                MainDbContext.SaveChanges();

                //写入问卷数据
                var mm = model.CopyPropTo<CHIS_Data_PsychPretreatQs>();
                mm.CreateTime = DateTime.Now;
                var entity = MainDbContext.Add(mm).Entity;
                MainDbContext.SaveChanges();

                dd.PropName = "PsychPretreatQsId";
                dd.PropValue = entity.PsychPretreatQsId;
                return null;

            });
        }



        /// <summary>
        /// 手机预约信息，并返回给用户        
        /// <returns>成功则返回用户信息</returns>
        /// </summary>
        /// <param name="reservationDate">预约时间</param>
        /// <param name="reservationSlot">预约班段</param>
        /// <param name="propName">额外增加的属性名称和值</param>
        /// <param name="returnType">默认html;json,jsonp</param>
        [AllowAnonymous]
        [HttpPost]
        [EnableCors("chisOrigin")]//允许跨域
        public async Task<IActionResult> GetReservationInfo(int customerId, int departmentId, int doctorId,
            DateTime reservationDate, int reservationSlot, string returnType = "html", string propName = null, string propValue = null, string allergic = "", string pastMedicalHistory = "", int opId = 26, string opMan = "SYSTEM")
        {
            try
            {

                // var u = GetCurrentLoginUser;
                //姓名
                var cust = MainDbContext.vwCHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == customerId);
                //科室
                var depart = MainDbContext.vwCHIS_Code_Department.FirstOrDefault(m => m.DepartmentID == departmentId);
                //医生
                var doctor = MainDbContext.vwCHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                //获取医生的值班信息
                var workInfo = new ahWeb.Api.Doctor(_db, _getInfoService).GetDoctorOnDutyInfo(doctorId, departmentId, reservationDate, reservationSlot);
                if (workInfo == null)
                {
                    new ahWeb.Api.Doctor(_db, _getInfoService).InitalDoctorOnDutyInfo(doctorId, departmentId, reservationDate, reservationDate.AddDays(7));
                    workInfo = new ahWeb.Api.Doctor(_db, _getInfoService).GetDoctorOnDutyInfo(doctorId, departmentId, reservationDate, reservationSlot);
                }
                var timestr = $"{workInfo.FromTime?.ToString(@"hh\:mm")}—{workInfo.ToTime?.ToString(@"hh\:mm")}";


                bool haveRegisted = false;

                var cusH = MainDbContext.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == cust.CustomerID);
                if (allergic.IsNotEmpty()) cusH.Allergic = allergic;
                if (pastMedicalHistory.IsNotEmpty()) cusH.PastMedicalHistory = pastMedicalHistory;
                MainDbContext.SaveChanges();

#if DEBUG
                var rlt = new Models.ViewModel.ReservationRlt
                {
                    rlt = true,
                    msg = "",
                    registStatus = haveRegisted ? "请注意,您已经预约了,不要重复预约。先前预约信息如下。" : "",
                    rltCode = "REPET_REGIST",
                    registerId = 111,// add.RegisterID,
                    registerSeq = 21,// add.RegisterSeq,
                    customer = cust,
                    stationName = depart.StationName,
                    departmentName = depart.DepartmentName,
                    doctor = doctor,
                    reservationDate = reservationDate,
                    timeInfo = timestr
                };
#else


               
                  //添加一条预约数据
                  var add = new ahWeb.Api.Business(_db).AddOneRegister(cust.CustomerID, depart.DepartmentID, doctorId, depart.StationID.Value
                      , reservationDate, reservationSlot, RegistFrom.V20Web, out haveRegisted,
                      opId: opId,opMan: opMan, 
                      propName:propName,propValue:propValue);


                  //给用户发送短信
                  StringBuilder custStr = new StringBuilder();
                  custStr.Append($"尊敬的会员{cust.CustomerName},已经为您预约好{depart.StationName}{depart.DepartmentName}{doctor.DoctorName}医生，请于{reservationDate.ToString("yyyy年MM月dd日")}{timestr} 按时就诊。健康热线:0769-86325818【天使健康】");
                  StringBuilder docStr = new StringBuilder();
                  docStr.Append($"尊敬的{doctor.DoctorName}医生您好,天使健康会员{cust.CustomerName}预约您于{reservationDate.ToString("yyyy年MM月dd日")}{timestr}在{depart.StationName}就诊，请安排好时间，谢谢！客服热线：0769-86325818【天使健康】");
                  Base.SMS sms = new Base.SMS();
                  string c = await sms.PostSmsInfo(cust.Telephone, custStr.ToString());
                  if (c.Equals("true"))
                  {
                      string d = await sms.PostSmsInfo(doctor.Telephone, docStr.ToString());
                  }

                  var rlt = new Models.ViewModel.ReservationRlt
                  {
                      rlt = true,
                      msg = "",
                      registStatus = haveRegisted ? "请注意,您已经预约了,不要重复预约。先前预约信息如下。" : "",
                      rltCode="REPET_REGIST",
                      registerId = add.RegisterID,
                      registerSeq = add.RegisterSeq,
                      customer = cust,
                      stationName = depart.StationName,
                      departmentName = depart.DepartmentName,
                      doctor = doctor,
                      reservationDate = reservationDate,
                      timeInfo = timestr
                  };
#endif
                await _followService.UpdateRecentListAsync(customerId, depart.StationID, doctorId);
                if (returnType == "json") return Json(rlt);
                if (returnType == "jsonp") return this.Jsonp(rlt);
                else return PartialView("_pvRegistSuccess", rlt);
            }
            catch (Exception ex)
            {
                if (returnType == "json") return Json(new { rlt = false, msg = ex.Message });
                if (returnType == "jsonp") return this.Jsonp(new { rlt = false, msg = ex.Message });
                else return PartialView("_pvRegistSuccess", new Models.ViewModel.ReservationRlt { ex = ex });
            }

        }

        /// <summary>
        /// 生成手机4位验证码并发送给该手机
        /// <returns></returns>
        /// </summary>
        //public IActionResult Json_SendMobCode(string mob)
        //{
        //    try
        //    {
        //        string contents = "";
        //        Random rad = new Random();//实例化随机数产生器rad；
        //        int code = rad.Next(1000, 10000);//用rad生成大于等于1000，小于等于9999的随机数；
        //                                         //插入数据库 
        //        var m = MainDbContext.InsertEntity<CHIS_DataTemp_SMS>(new CHIS_DataTemp_SMS
        //        {
        //            PhoneCode = mob,
        //            VCode = code.ToString(),
        //            CreatTime = DateTime.Now
        //        });
        //        Codes.Utility.SMS sms = new Codes.Utility.SMS();
        //        mob = mob.Replace(" ", "");
        //        contents = $" {m.VCode}为您本次注册天使健康会员验证码，有效时间为1分钟【天使健康】";
        //        string rlt = sms.PostSmsInfo(mob, contents);
        //        if (rlt != "true") new Exception(rlt);
        //        return Json(new { rlt = true, msg = "" });
        //    }
        //    catch (Exception e)
        //    { return Json(new { rlt = false, msg = e.Message }); }
        //}

        [AllowAnonymous]
        public async Task<IActionResult> StationInfos(string searchText, double? lat, double? lng, int? pageIndex = 1, int? pageSize = 20)
        {
            var model = await _getInfoService.SearchNearStationAsync(searchText, lat, lng, pageIndex, pageSize);

            if (GetCurrentLoginUser != null)
            {
                var followList = await _followService.GetListAsync(GetCurrentLoginUser.CustomerId);

                foreach (var item in model.Items)
                {
                    if (followList.FollowStationIds.Contains(item.StationId))
                    {
                        item.IsFollow = true;
                    }
                }
            }

            ViewBag.searchText = searchText;

            return PartialView("_pvStationInfos", model);

        }
    }
}
