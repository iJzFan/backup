using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ah.WeChatService.Utilities
{
    public static class Server
    {
        private static string _appDomainAppPath;
        public static string AppDomainAppPath
        {
            private get { return _appDomainAppPath ?? (_appDomainAppPath = AppContext.BaseDirectory); }
            set
            {
                _appDomainAppPath = value;

                if (!_appDomainAppPath.EndsWith("/"))
                {
                    _appDomainAppPath += "/";
                }

            }
        }

        private static string _webRootPath;
        /// <summary>
        /// wwwroot文件夹目录（专供ASP.NET Core MVC使用）
        /// </summary>
        public static string WebRootPath
        {
            get => _webRootPath ?? (_webRootPath = AppDomainAppPath + "wwwroot/");
            set => _webRootPath = value;
        }

        public static string GetMapPath(string virtualPath)
        {
            if (virtualPath == null)
            {
                return "";
            }

            return virtualPath.StartsWith("~/") ? virtualPath.Replace("~/", AppDomainAppPath) : Path.Combine(AppDomainAppPath, virtualPath);
        }

        public static HttpContext HttpContext
        {
            get
            {
                HttpContext context = new DefaultHttpContext();
                return context;
            }
        }
    }
}
