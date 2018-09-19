using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CHIS.WebSocketService
{
    public class Global
    {
        /// <summary>
        /// 全局加密密码
        /// </summary>
        public static readonly string SYS_ENCRIPT_PWD = "tsjk@2018";

        /// <summary>
        /// localhost转Ip地址
        /// </summary>
        /// <returns></returns>
        internal static string Localhost2Ip(string url)
        {
            var addrs = Dns.GetHostAddresses(Dns.GetHostName());
            var iplocal = addrs.FirstOrDefault(m => !m.IsIPv6LinkLocal).ToString();
            if (url.Contains("localhost"))
            {
                return url.Replace("localhost", iplocal);
            }
            return url;
        }
    }
}
