using CHIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ass;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using CHIS.Models.ViewModel;
using CHIS.DbContext;
using CHIS.Models.StatisticsModels;
using CHIS.Models.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace CHIS.Services
{
    /// <summary>
    /// 通知管理服务
    /// </summary>
    public class NotifyService : BaseService
    {
        Services.DrugService _drugSvr;
        IConfiguration _config;
        Code.Managers.IMyLogger _logger;
        AccessService _accSvr;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public NotifyService(CHISEntitiesSqlServer db
            , IConfiguration config
            , Services.DrugService drugSvr
            , Code.Managers.IMyLogger logger
            , AccessService accSvr
            ) : base(db)
        {
            _drugSvr = drugSvr;
            _config = config;
            _logger = logger;
            _accSvr = accSvr;
        }

        //===============================================================================

        #region
        /// <summary>
        /// 回调通知支付状态
        /// </summary>
        public async Task SWSNofityPayedStatusAsync(string payOrderId, bool bSuccess, string errMsg = "")
        {
            try
            {

                string url = _config.GetSection("WebSocketService:NotifyPayQrResult").Value;
                errMsg = System.Web.HttpUtility.UrlEncode(errMsg);//编码
                url = url += $"?payOrderId={payOrderId}&bSuccess={bSuccess}&errMsg={errMsg}";
                url = Global.Localhost2Ip(url);
                await Ass.Net.WebHelper.WebPost(url);
            }
            catch (Exception ex)
            {
                var rlt = _accSvr.GetOpIdAndOpMan();
                _logger.WriteError(ex, rlt.Item1, rlt.Item2);
            }
        }
        public async Task SWSNotifyPayChangeAsync(string payOrderId, int? stationId = null, int? doctorId = null)
        {
            try
            {
                if (!stationId.HasValue) stationId = _accSvr.UserSelf.StationId;
                if (!doctorId.HasValue) doctorId = _accSvr.UserSelf.DoctorId;

                string url = _config.GetSection("WebSocketService:NotifyPayChanged").Value;
                var code = Ass.Data.Secret.Encript($"{stationId}|{doctorId}|{payOrderId}", Global.SYS_ENCRIPT_PWD);
                code = System.Net.WebUtility.UrlEncode(code);
                url = url += $"?code={code}";
                url = Global.Localhost2Ip(url);
                await Ass.Net.WebHelper.WebPost(url);
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex.Message);
            }
        }




        #endregion


        /// <summary>
        /// 通知医生被预约了
        /// </summary>
        public void NotifyDoctorReservatedAsync(int doctorId, int customerId, long registId)
        {
            try
            {
                var doctor = _db.vwCHIS_Code_Doctor.Single(m => m.DoctorId == doctorId);
                var regist = _db.vwCHIS_Register.FirstOrDefault(m => m.RegisterID == registId);
                if (doctor.DoctorAppId.IsNotEmpty())
                {
                    //发送消息给App端  
                    //？appId=2126&registTime=2018-08-08&stationName=五铢1号&customerName=李四
#if DEBUG
                    doctor.DoctorAppId = "2132";
#endif
                    string url = _config.GetSection("AppInterface:DoctorRegistedNotify").Value
                        .Trim()
                        .ahDtUtil().AddUrlQueryString(new
                        {
                            appId = doctor.DoctorAppId,
                            registTime = regist.RegisterDate.ahDtUtil().ToShortDate(),
                            stationName = regist.StationName,
                            customerName = regist.CustomerName,
                            registId = regist.RegisterID
                        });
                    Ass.Net.WebHelper.WebPost(url);
                }

                //发送短信
                if (doctor.CustomerMobile.GetStringType().IsMobile)
                {
                    // SMSHelper.SmsSend(doctor.CustomerMobile, SMS_Constants.SMSTemplate.FindPassWord, "", "");
                    SendSMS(doctor.CustomerMobile
                         , "{doctorName}医生,会员{customerName}预约您{datetime}于{stationName}就诊,请注意接诊。"
                         , new
                         {
                             doctorName = doctor.DoctorName,
                             customerName = regist.CustomerName,
                             datetime = regist.RegisterDate?.ToString("yyyy年M月d日"),
                             stationName = regist.StationName
                         });
                }
            }
            catch (Exception ex)
            {
                var op = _accSvr.GetOpIdAndOpMan();
                _logger.WriteError(ex, op.Item1, op.Item2);
            }
        }




        /// <summary>
        /// 发送短信信息
        /// </summary> 
        private void SendSMS(string mobile, string template, dynamic data)
        {
            var a = template.ahDtUtil().Format(data) + "【天使健康】";
            new Codes.Utility.SMS().PostSmsInfoAsync(mobile, a);
        }












    }
}
