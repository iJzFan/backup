using System;
using System.Collections.Generic;
using System.Text;

namespace ah
{
    public class ClientEnvi
    {
        /// <summary>
        /// 客户端类型 wx/ie/firefox/chorme
        /// </summary>
        public string ClientType { get; set; }

        /// <summary>
        /// 客户端公网IP
        /// </summary>
        public string ClientIP { get; set; }
        /// <summary>
        /// 客户端的经度
        /// </summary>
        public double ClientLng { get; set; }
        /// <summary>
        /// 客户端的纬度
        /// </summary>
        public double ClientLat { get; set; }
    }
}
