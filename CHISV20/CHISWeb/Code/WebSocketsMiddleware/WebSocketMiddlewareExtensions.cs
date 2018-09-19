using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.WebSocketsMiddleware
{ 

    /// <summary>
    /// websocket通知中间件扩展
    /// </summary>
    public static class WebSocketStationPayMonitorMiddlewareExtensions
    {
        /// <summary>
        /// 使用websocket工作站支付监控
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseWebSocketStationPayMonitor(
          this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<StationPayMonitor>();
        }

        /// <summary>
        /// 使用websocket具体的支付监控
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseWebSocketPayMonitor(
          this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PayMonitor>();
        }
    }
}
