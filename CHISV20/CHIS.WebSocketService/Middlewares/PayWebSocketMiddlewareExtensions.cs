using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
 

    /// <summary>
    /// websocket中间件扩展
    /// </summary>
    public static class PayWebSocketMiddlewareExtensions
    {
        /// <summary>
        /// 使用WebSocket 支付工作站监控。
        /// 使用路由/ws-pay-station-monitor。
        /// </summary>
        public static IApplicationBuilder UseWebSocketPayStationMonitor(
                this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PayStationWebSocketMiddleware>();
        }

        /// <summary>
        /// 使用WebSocket 二维码支付监控。
        /// 使用路由/ws-pay-qr-monitor。
        /// </summary>
        public static IApplicationBuilder UseWebSocketPayQrMonitor(
                this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PayQrWebSocketMiddleware>();
        }

    }
}
