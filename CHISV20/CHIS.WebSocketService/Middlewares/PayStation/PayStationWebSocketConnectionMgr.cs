using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{

    public class PayStationWebSocketConnectionMgr : BaseWebSocketConnectionMgr<PayStationWebSocket>
    {
        internal override string GetId(PayStationWebSocket socket)
        {
            return GetId(socket.StationId, socket.DoctorId);
        }
        public string GetId(int stationId, int doctorId)
        {
            return $"SPWS_{stationId}_{doctorId}";
        }
    }

}
