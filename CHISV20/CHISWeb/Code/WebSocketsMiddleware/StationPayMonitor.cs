using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CHIS.Code.WebSocketsMiddleware
{
 

    /// <summary>
    /// 工作站支付的监控器
    /// </summary>
    public class StationPayMonitor
    {

        /// <summary>
        /// 管道代理对象
        /// </summary>
        private readonly RequestDelegate _next;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public StationPayMonitor(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 中间件调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            //判断是否为websocket请求
            if (context.WebSockets.IsWebSocketRequest)
            {
                //接收客户端
                var webSocket = context.WebSockets.AcceptWebSocketAsync().Result;
                //启用一个线程处理接收客户端数据
                new Thread(Accept).Start(webSocket);
                while (webSocket.State == WebSocketState.Open)
                {
                    webSocket.SendAsync(SendClientMsg($"当前时间:{DateTime.Now}"), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                    Thread.Sleep(1000);
                }
            }
            return this._next(context);




        }
        private ArraySegment<byte> SendClientMsg(string msg)
        {
            return new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(msg));
        }

        void Accept(object obj)
        {
            var webSocket = obj as WebSocket;
            while (true)
            {
                var acceptArr = new byte[1024];

                var result = webSocket.ReceiveAsync(new ArraySegment<byte>(acceptArr), CancellationToken.None).Result;

                var acceptStr = System.Text.Encoding.UTF8.GetString(acceptArr).Trim(char.MinValue);
                Console.WriteLine("收到信息：" + acceptStr);
            }

        }

    }

}
