using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ah
{
    public class Global
    {
        public readonly static string AUTHENTICATION_SCHEME = "AppCookieAuthenticationScheme";
        public readonly static string AUTHENTICATION_CLAIMS_IDENTITY = "AccountClaimsIdentity";
        //  public readonly static string AUTHENTICATION_CLAIMS_IDENTITY_AHMS = "AccountClaimsIdentityAHMS";

        public readonly static string AUTHENTICATION_ISSUER = "https://www.jk213.com/anglehealth";
        public readonly static string AUTHENTICATION_COOKIE_NAME = ".CookieAccountName";

        public readonly static string ROUTE_FORBIDDEN = "/Home/Forbidden";
        public readonly static string ROUTE_LOGIN = "/Customer/Home/CustomerLogin";
        public readonly static string ROUTE_ERROR = "/Customer/Home/Error";




        /// <summary>
        /// 配置器
        /// </summary>
        public static Microsoft.Extensions.Configuration.IConfigurationRoot Config { get; set; }

        public static class ConfigSettings
        {
            /// <summary>
            /// 用户图像根目录
            /// </summary>
            public static string CustomerImagePathRoot = Config.GetSection("SystemSettings:CustomerImgPathRoot").Value;
            /// <summary>
            /// 医生头像根目录
            /// </summary>
            public static string DoctorImagePathRoot = Config.GetSection("SystemSettings:DoctorImgPathRoot").Value;
            /// <summary>
            /// 工作站图像根路径
            /// </summary>
            public static string StationImagePathRoot = Config.GetSection("SystemSettings:StationImgPathRoot").Value;

            /// <summary>
            /// 证件图像的根路径
            /// </summary>
            public static string CertificateImagePathRoot = Config.GetSection("SystemSettings:CertificateImagePathRoot").Value;

            /// <summary>
            /// 药品图像的根路径
            /// </summary>
            public static string DrugImagePathRoot = Config.GetSection("SystemSettings:DrugImagePathRoot").Value;


        }
    }
}
