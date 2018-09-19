using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public class PayQrWebSocket : BaseWebSocket
    {
        /// <summary>
        /// 当前正在支付的订单号
        /// </summary>
        public string PayOrderId { get; set; }

        /// <summary>
        /// 工作站Id
        /// </summary>
        public int StationId { get; set; }
        /// <summary>
        /// 收费者医生Id
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        public PayQrStatuses PayQrStatus { get; set; } = PayQrStatuses.INITIAL;
        /// <summary>
        /// 开始扫描时间
        /// </summary>
        public DateTime StartScanTime { get; set; }

        /// <summary>
        /// 微信的二维码地址
        /// </summary>
        public string WxQrUrl { get; set; }
        /// <summary>
        /// 支付宝的二维码地址
        /// </summary>
        public string AliQrUrl { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        public int PayAmount { get; set; }
    }
    public enum PayQrStatuses
    {
        INITIAL = 0,
        GETPAYINFO = 1,//获取到二维码相关数据
        MONITORSCANING = 2, //检测扫码中
        PAYEDSUCCESS = 3,//已经支付成功了
        THISPAYOK = 4,//当次支付成功
        ERROR = 5, //出现失败
        TIMEOUT=6
    }

}
