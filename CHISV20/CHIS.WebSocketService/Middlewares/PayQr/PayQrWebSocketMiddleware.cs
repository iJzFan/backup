using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public class PayQrWebSocketMiddleware
    {


        /// <summary>
        /// 管道代理对象
        /// </summary>
        private readonly RequestDelegate _next;
        private PayQrWebSocketConnectionMgr _wsconnMgr;
        private PayQrWebSocketHandler _wsHandler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public PayQrWebSocketMiddleware(RequestDelegate next, PayQrWebSocketConnectionMgr wsconnMgr, PayQrWebSocketHandler wsHandler)
        {
            _next = next;
            _wsconnMgr = wsconnMgr;
            _wsHandler = wsHandler;
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
                var cancellationToken = context.RequestAborted;
                var currentWebSocket = await context.WebSockets.AcceptWebSocketAsync();
                var socket = new PayQrWebSocket { WebSocket = currentWebSocket };
                _SetStationInfo(socket, context);//设置客户端的数据
                await _wsconnMgr.AddSocket(socket);
                while (socket.WebSocket.State == WebSocketState.Open)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    var status= _wsHandler.GetPayQrStatus(socket);
                    switch (status)
                    {
                        case PayQrStatuses.INITIAL://初始化
                            await _wsHandler.GetPayQrInfo(socket);//发送获取到的二维码信息给客户端，客户端进行握手处理
                            _wsHandler.StartScaningDetect(socket);
                            break;
                        case PayQrStatuses.MONITORSCANING://开始监控
                            await _wsHandler.ScanningTimeoutDetect(socket,cancellationToken);//如果超时则会退出
                            Thread.Sleep(300);
                            continue;
                        case PayQrStatuses.ERROR:
                            break;
                    }       
                    
                    Thread.Sleep(500);
                }
                await _wsconnMgr.RemoveSocket(socket);
            }
            await this._next(context);
        }

        //获取传入的基础信息
        private void _SetStationInfo(PayQrWebSocket socket, HttpContext context)
        {
            var qs = context.Request.QueryString;
            var stationId = Convert.ToInt32(context.Request.Query["stationId"]);
            var doctorId = Convert.ToInt32(context.Request.Query["doctorId"]);
            var stationId2 = Convert.ToInt32(context.User.FindFirst("stationId"));
            var doctorId2 = Convert.ToInt32(context.User.FindFirst("doctorId"));
            socket.StationId = stationId;
            socket.DoctorId = doctorId;
            socket.PayOrderId = context.Request.Query["payOrderId"].ToString();            
        }


    }
}
