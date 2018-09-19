using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public class PayQrWebSocketConnectionMgr : BaseWebSocketConnectionMgr<PayQrWebSocket>
    { 
        internal override string GetId(PayQrWebSocket socket)
        {
            return GetId(socket.PayOrderId);
        }
        public string GetId(string payOrderId)
        {
            return $"POWS_{payOrderId}";
        }
         
    }
}
