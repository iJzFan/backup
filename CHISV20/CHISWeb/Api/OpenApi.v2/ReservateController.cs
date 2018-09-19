using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ass;
using CHIS.Models.DataModel;
using Microsoft.AspNetCore.Authorization;

namespace CHIS.Api.OpenApi.v2
{

    /// <summary>
    /// 康健人员操作
    /// </summary>
    public class ReservateController : CHIS.Api.v2.OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        Services.WeChatService _weChatSvr;
        public ReservateController(Services.ReservationService resSvr
            , Services.WorkStationService stationSvr
            , Services.DoctorService docrSvr
            , Services.CustomerService cusSvr
            , Services.WeChatService wechatSvr
            ) //: base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
            _weChatSvr = wechatSvr;
        }

        //=========================================================================================



        /// <summary>
        /// 获取约诊的清单
        /// </summary>
        /// <param name="searchText">搜索 手机/身份证</param>
        /// <param name="timeRange">时间范围特定字符 (可选)默认Today今天,可选值:All,Today,Tomorrow,ThisYear,ThisQuarter,ThisMonth,ThisWeek,Yesterday,Last7Days,Last3Days,Next7Days,Next3Days,dt0=yyyy-MM-dd;dt1=yyyy-MM-dd </param>
        /// <param name="stationId">搜索工作站 (可选)默认本工作站 </param>
        /// <param name="departId">搜索科室 (可选)默认 </param>
        /// <param name="doctorId">搜索医生 (可选)默认</param>
        /// <param name="pageIndex">页码 (可选)默认1</param>
        /// <param name="pageSize">页容 (可选)默认20</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public dynamic SearchRegistedList(string searchText, string timeRange = "Today", int? stationId = null, int? departId = null, int? doctorId = null, int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                var dts = timeRange.ahDtUtil().TimeRange();
                int? registOpId = null;
                if (stationId == null) stationId = UserSelf.StationId;
                var model = new BllCaller.TreatRegistBllCaller().SearchRegistList(searchText, dts.Item1, dts.Item2, stationId, departId, doctorId, registOpId, pageIndex.Value, pageSize.Value);
                var doctors = _docrSvr.FindList(model.DataList.Select(m => m.EmployeeID.Value).ToList()).ToList();
                var rxdoctors = _docrSvr.FindList(model.DataList.Select(m => m.RxDoctorId.Value).ToList()).ToList();
                var rlt = MyDynamicResult("");
                rlt.data = model.DataList.Select(m => new
                {
                    RegisterId = m.RegisterID,
                    RegisterDate = m.RegisterDate?.ToString("yyyy-MM-dd"),
                    m.RegisterFromName,
                    m.RegisterSeq,
                    m.RegisterSlot,
                    StationId = m.StationID,
                    m.StationName,
                    Customer = new
                    {
                        CustomerId = m.CustomerID,
                        m.CustomerName,
                        CustomerGender = m.Gender,
                        m.CustomerMobile,
                        CustomerEmail = m.Email,
                        CustomerPicUrl = m.CustomerPhoto.ahDtUtil().GetCustomerImg(m.Gender)
                    },
                    Doctor = doctors.Select(a => new
                    {
                        a.DoctorId,
                        a.DoctorName,
                        DoctorGender = a.Gender,
                        a.PostTitleName,
                        DoctorPicUrl = a.DoctorPhotoUrl.ahDtUtil().GetDoctorImg(a.Gender)
                    }).FirstOrDefault(b => b.DoctorId == m.EmployeeID),
                    RxDoctor = rxdoctors.Select(a => new
                    {
                        a.DoctorId,
                        a.DoctorName,
                        DoctorGender = a.Gender,
                        a.PostTitleName,
                        DoctorPicUrl = a.DoctorPhotoUrl.ahDtUtil().GetDoctorImg(a.Gender)
                    }).FirstOrDefault(b => b.DoctorId == m.EmployeeID)
                });
                rlt.pageIndex = pageIndex;
                rlt.pageSize = pageSize;
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }


        /// <summary>
        /// 预约医生  
        /// </summary>
        /// <param name="model">预约信息</param> 
        //[ApiExplorerSettings(GroupName ="v2")]
        [HttpPost]
        [Authorize("ThirdPartAuth")]
        public async Task<dynamic> ReservateDoctor([FromBody] ReservateDoctorModel model)
        {
            try
            {
                if (!ModelState.IsValid) throw new ModelStateException(ModelState);

                if (!model.OpId.HasValue)
                {
                    model.OpId = TryGet<int?>(() => UserSelf.OpId);
                    model.OpMan = TryGet<string>(() => UserSelf.OpManFullMsg);
                }

                //姓名
                var cust = await _cusSvr.GetCustomerById(model.CustomerId);

                //科室             
                var depart = _stationSvr.GetDepartmentById(model.DepartmentId);
                //医生
                var doctor = _docrSvr.Find(model.DoctorId);
                var workInfo = _docrSvr.GetDoctorOnDutyInfo(model.DoctorId,model.DepartmentId, model.ReservationDate, model.ReservationSlot);
                var timestr = $"{workInfo.FromTime?.ToString(@"hh\:mm")}—{workInfo.ToTime?.ToString(@"hh\:mm")}";

                var add = await _resSvr.AddOneRegister(cust.CustomerID, depart.DepartmentID, model.DoctorId, depart.StationID.Value, model.ReservationDate, model.ReservationSlot, RegistFrom.V20Web
                    , opId: model.OpId, opMan: model.OpMan, rxDoctorId: model.RxDoctorId
                    );
                //给用户发送短信
                StringBuilder b = new StringBuilder();
                b.Append($"尊敬的{cust.CustomerName},请于{model.ReservationDate.ToString("yyyy年MM月dd日")} {timestr} 至{depart.DepartmentName}({doctor.DoctorName})医生处就诊，感谢您的预约。【天使健康】");
                Codes.Utility.SMS sms = new Codes.Utility.SMS();
                //await new SendVCodeCBL(this).SendVCode(cust.Telephone, b.ToString());
                var rlt = await sms.PostSmsInfoAsync(cust.CustomerMobile, b.ToString());
                var d = MyDynamicResult(true, "");
                d.rltCode = add.HaveRegisted ? "REPET_REGIST" : "";
                d.registStatus = add.HaveRegisted ? "请注意,您已经预约了,不能重复预约。先前预约信息如下。" : "";
                d.registerId = add.RegisterID;
                d.registerSeq = add.RegisterSeq;
                d.customerId = cust.CustomerID;
                d.customerName = cust.CustomerName;
                d.customerGender = cust.Gender;
                d.customerBirthday = cust.Birthday;
                d.customerPicUrl = cust.PhotoUrlDef.ahDtUtil().GetCustomerImg(cust.Gender);
                d.stationName = depart.StationName;
                d.departmentName = depart.DepartmentName;
                d.doctorName = doctor.DoctorName;
                d.doctorGender = doctor.Gender;
                d.doctorTitle = doctor.PostTitleName;
                d.doctorId = doctor.DoctorId;
                d.doctorPicUrl = doctor.DoctorPhotoUrl.ahDtUtil().GetDoctorImg(doctor.Gender, imgSizeTypes.HorizThumb);
                d.reservationDate = model.ReservationDate.ToString("yyyy-MM-dd");
                d.timeInfo = timestr;
                return d;

            }
            catch (Exception ex) { return MyDynamicResult(ex); }

        }



        /// <summary>
        /// 获取快速约诊的扫描二维码
        /// </summary>
        /// <param name="stationId">工作站Id (可选)默认登录工作站</param>
        /// <param name="doctorId">医生Id (可选)默认登录医生</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public async Task<IActionResult> GetReservate2DCode(int? stationId,int? doctorId)
        {
            try
            {
                if (UserSelf == null && (stationId == null || doctorId == null)) throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录调用必须填入StationId,DoctorId参数");
                if (stationId == null) stationId = UserSelf.StationId;
                if (doctorId == null) doctorId = UserSelf.DoctorId;
                var rxDoctor = _docrSvr.GetMyDefRxDoctor(stationId.Value);
                var rlt = (rxDoctor == null) ? "" : (await _weChatSvr.CreateQRCodeUrl(stationId.Value, doctorId.Value, rxDoctor.DoctorId));
                var jm = MyDynamicResult(rlt);                
                return Ok(jm);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }


        /// <summary>
        /// 获取工作站的处方医生 RxDoctor
        /// </summary>
        /// <param name="stationId">工作站Id (可选) 默认登录工作站</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetRxDoctorsOfStation(int? stationId)
        {
            try
            {
                if (UserSelf == null && (stationId == null))
                    throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录调用必须填入StationId参数");
                if (stationId == null) stationId = UserSelf.StationId;
                var mm = _docrSvr.GetMyRxDoctors(UserSelf.StationId);
                var jm = MyDynamicResult(mm);
                return Ok(jm);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }


    }
}
