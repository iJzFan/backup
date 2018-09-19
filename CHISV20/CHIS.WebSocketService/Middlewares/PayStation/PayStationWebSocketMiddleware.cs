using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public class PayStationWebSocketMiddleware
    {


        /// <summary>
        /// 管道代理对象
        /// </summary>
        private readonly RequestDelegate _next;
        private PayStationWebSocketConnectionMgr _wsconnMgr;
        private PayStationWebSocketHandler _wsHandler;
        IConfiguration _configuration;
        int socketStationTimeoutSeconds = 60 * 60 * 24;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public PayStationWebSocketMiddleware(RequestDelegate next
            , PayStationWebSocketConnectionMgr wsconnMgr
            , PayStationWebSocketHandler wsHandler
            , IConfiguration configuration)
        {
            _next = next;
            _wsconnMgr = wsconnMgr;
            _wsHandler = wsHandler;
            socketStationTimeoutSeconds = Convert.ToInt32(configuration.GetSection("QRPay:socketStationTimeoutSeconds").Value);
        }



        /// <summary>
        /// 中间件调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            //判断是否为websocket请求
            if (context.WebSockets.IsWebSocketRequest)
            {
                //接收客户端
                // var cancellationToken = context.RequestAborted;
                var currentWebSocket = await context.WebSockets.AcceptWebSocketAsync();
                var socket = new PayStationWebSocket { WebSocket = currentWebSocket };
                _SetStationInfo(socket, context);//设置客户端的数据
                await _wsconnMgr.AddSocket(socket);
                while (socket.WebSocket.State == WebSocketState.Open &&
                            (DateTime.Now - socket.CreatTime).TotalSeconds < this.socketStationTimeoutSeconds)
                {
                    // if (cancellationToken.IsCancellationRequested) break;
                    //处理数据
                    if (_wsHandler.HasNewOrder(socket))//如果有新的订单，则通知前端新的订单
                    {
                        await _wsHandler.NotifyStationNewPayOrder(socket);
                        _wsHandler.ClearPayOrderId(socket);
                    }
                    Thread.Sleep(500);
                }
                await _wsconnMgr.RemoveSocket(socket);
            }
            await this._next(context);
        }

        //获取传入的基础信息
        private void _SetStationInfo(PayStationWebSocket socket, HttpContext context)
        {
            var qs = context.Request.QueryString;
            var stationId = Convert.ToInt32(context.Request.Query["stationId"]);
            var doctorId = Convert.ToInt32(context.Request.Query["doctorId"]);
            var stationId2 = Convert.ToInt32(context.User.FindFirst("stationId"));
            var doctorId2 = Convert.ToInt32(context.User.FindFirst("doctorId"));
            socket.StationId = stationId;
            socket.DoctorId = doctorId;
        }


    }
}
