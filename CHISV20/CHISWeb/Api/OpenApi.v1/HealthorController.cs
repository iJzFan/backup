using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ass;
using CHIS.Models.DataModel;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 康健人员操作
    /// </summary>
    public class HealthorController : OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        public HealthorController(DbContext.CHISEntitiesSqlServer db,
            Services.ReservationService resSvr,
            Services.WorkStationService stationSvr,
            Services.DoctorService docrSvr,
            Services.CustomerService cusSvr            
            ) : base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
        }

        //=========================================================================================


        #region 查询工作站

        /// <summary>
        /// 搜索接诊工作站
        /// </summary>
        /// <param name="searchText">搜索内容</param>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        [HttpGet]
        public dynamic SearchTreatStation(string searchText, double? lat, double? lng, int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                var finds = _stationSvr.SearchTreatStation(searchText, lat, lng, pageIndex.Value, pageSize.Value);
                var rlt = new Ass.Mvc.PageListInfo
                {
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    RecordTotal = finds.Item1
                };
                var d = MyDynamicResult(true, "");
                d.totalPages = rlt.PageTotal;
                d.items = finds.Item2;
                d.pageIndex = pageIndex;
                d.pageSize = pageSize;
                return d;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }


        #endregion



        #region 预约  
        /// <summary>
        /// 预约医生
        /// </summary>
        /// <param name="model">预约信息</param> 
        [HttpPost]
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
                    , opId: model.OpId, opMan: model.OpMan,rxDoctorId:model.RxDoctorId
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
                d.customer = cust;
                d.stationName = depart.StationName;
                d.departmentName = depart.DepartmentName;
                d.employee = doctor;                
                d.reservationDate = model.ReservationDate.ToString("yyyy-MM-dd");
                d.timeInfo = timestr;
                return d;

            }
            catch (Exception ex) { return MyDynamicResult(ex); }

        }




        #endregion



    }
}
