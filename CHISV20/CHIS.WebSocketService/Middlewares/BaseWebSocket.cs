using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public class BaseWebSocket
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();
        /// <summary>
        /// WebSocket
        /// </summary>
        public WebSocket WebSocket { get; set; }  
        /// <summary>
        /// 该链接创建的时间
        /// </summary>
        public DateTime CreatTime { get; private set; } = DateTime.Now;
    }
}
