using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public class PayStationWebSocketHandler
    {
        protected PayStationWebSocketConnectionMgr _wsconnMgr;
        public PayStationWebSocketHandler(PayStationWebSocketConnectionMgr wsconnMgr)
        {
            this._wsconnMgr = wsconnMgr;
        }

 


        /// <summary>
        /// 设置新的订单Id
        /// </summary>
        /// <param name="code"></param>
        public void SetNewPayOrderId(string code)
        {
            var a = Ass.Data.Secret.Decript(code, Global.SYS_ENCRIPT_PWD).Split('|');
            var stationId = Ass.P.PInt(a[0]);
            var doctorId = Ass.P.PInt(a[1]);
            var payOrderId = a[2];
            if (!string.IsNullOrEmpty(payOrderId))
            {
                var key = _wsconnMgr.GetId(stationId, doctorId);
                if(_wsconnMgr.ContainsKey(key))
                _wsconnMgr.GetItem(key).PayOrderId = payOrderId;
            }
        }

        /// <summary>
        /// 判断是否有新的订单
        /// </summary> 
        public bool HasNewOrder(PayStationWebSocket socket)
        {
            if (socket.PayOrderId == null) return false;
            else return true;
        }
        /// <summary>
        /// 清空支付单号
        /// </summary>
        /// <param name="socket"></param>
        public void ClearPayOrderId(PayStationWebSocket socket)
        {
            socket.PayOrderId = null;
        }

        /// <summary>
        /// 通知工作站新订单情况 ,以方便前端启动新订单支付监控        
        /// </summary>
        /// <returns>返回json格式的字符串</returns>
        public async Task NotifyStationNewPayOrder(
            PayStationWebSocket socket,
            CancellationToken cancellationToken = default(CancellationToken))
        {         
            var rtn = new
            {
                rlt=true,
                msg="",
                payOrderId = socket.PayOrderId,
                notifyTime = DateTime.Now,
                stationId = socket.StationId,
                doctorId = socket.DoctorId
            };
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(rtn));
            var segment = new ArraySegment<byte>(buffer);
            await socket.WebSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }



    }
}
