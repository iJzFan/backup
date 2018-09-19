using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public class PayStationWebSocket:BaseWebSocket
    {
 
        /// <summary>
        /// 工作站Id
        /// </summary>
        public int StationId { get; set; }
        /// <summary>
        /// 收费者医生Id
        /// </summary>
        public int DoctorId { get; set; }
        /// <summary>
        /// 当前正在支付的订单号
        /// </summary>
        public string PayOrderId { get; set; }
    }
}
