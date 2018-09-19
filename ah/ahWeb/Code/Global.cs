using Ass;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ah
{
    public class Global
    {

        public readonly static string PROJECTNAME = "AHMSWeb";

        public readonly static string AUTHENTICATION_SCHEME = "AppCookieAuthenticationScheme" + PROJECTNAME;
        public readonly static string AUTHENTICATION_CLAIMS_IDENTITY = "AccountClaimsIdentity" + PROJECTNAME;
        public readonly static string AUTHENTICATION_ISSUER = "https://www.jk213.com/anglehealth" + PROJECTNAME;
        public readonly static string AUTHENTICATION_COOKIE_NAME = ".CookieAccountName" + PROJECTNAME;

        public readonly static string ROUTE_FORBIDDEN = "/HealthMgr/BackPanel/Forbidden";
        public readonly static string ROUTE_LOGIN = "/HealthMgr/BackPanel/Login";
        public readonly static string ROUTE_ERROR = "/HealthMgr/BackPanel/Error";
        /// <summary>
        /// 登出路径
        /// </summary>
        public readonly static string ROUTE_LOGOUT = "/HealthMgr/BackPanel/BackLoginOut";
        /// <summary>
        /// 登录Schme
        /// </summary>
        public readonly static string SIGN_IN_SCHEME = "http://hm.jk213.com/signin#" + PROJECTNAME;




        /// <summary>
        /// 配置器
        /// </summary>
        public static Microsoft.Extensions.Configuration.IConfigurationRoot Config { get; set; }
        public static class ConfigSettings
        {
            static string webHost = null;
            /// <summary>
            /// 网址根地址
            /// </summary>
            public static string WebHost { get => webHost; set { webHost = value; reloadConfig(); } }

            private static void reloadConfig()
            {
                Func<string, string> getConfigUri = (string secName) =>
                   {
                       var rlt = Config.GetSection(secName).Value;
                       if (!string.IsNullOrEmpty(WebHost)) return rlt.Replace("localhost", WebHost);
                       else return rlt;
                   };

                CustomerImagePathRoot = getConfigUri("SystemSettings:CustomerImgPathRoot");
                DoctorImagePathRoot = getConfigUri("SystemSettings:DoctorImgPathRoot");
                StationImagePathRoot = getConfigUri("SystemSettings:StationImgPathRoot");
                CertificateImagePathRoot = getConfigUri("SystemSettings:CertificateImagePathRoot");
                CustomerLoginPath = getConfigUri("Webfig:CustomerLogin");
                CustomerActionPath = getConfigUri("Webfig:CustomerAction");
            }




            /// <summary>
            /// 用户图像根目录
            /// </summary>
            public static string CustomerImagePathRoot;
            /// <summary>
            /// 医生头像根目录
            /// </summary>
            public static string DoctorImagePathRoot;
            /// <summary>
            /// 工作站图像根路径
            /// </summary>
            public static string StationImagePathRoot;
            /// <summary>
            /// 证件图像的根路径
            /// </summary>
            public static string CertificateImagePathRoot;

            public static string CustomerLoginPath;

            /// <summary>
            /// 登录或者注册的统一操作接口
            /// </summary>
            public static string CustomerActionPath;


        }




        #region 缓存字典数据
        /// <summary>
        /// 字典数据静态缓存
        /// </summary>
        public static IEnumerable<Models.vwCHIS_Code_DictDetail> DictDetail { get { return dictDetail; } }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public static void Initial()
        {
            var db = new Code.Utility.DataBaseHelper().GetMainDbContext();
            dictDetail = null;
            dictDetail = db.vwCHIS_Code_DictDetail.AsNoTracking().ToList();
        }


        private static IEnumerable<Models.vwCHIS_Code_DictDetail> dictDetail = null;


        /// <summary>
        /// 根据键类码和拼音或五笔搜索
        /// </summary> 
        /// <param name="dictKey">字典类别主码</param>
        /// <param name="pywb">拼音或五笔</param>
        /// <returns></returns>
        public static IEnumerable<Models.vwCHIS_Code_DictDetail> GetDictDetailListByDictKey(string dictKey, string pywb = "", string useime = "PINYIN", int? maxRows = null, string groupTag = null)
        {
            pywb = Ass.P.PStr(pywb).ToUpper().Trim();
            if (string.IsNullOrWhiteSpace(dictKey)) return null;
            if (useime.IsEmpty()) useime = "PINYIN";
            var mdd = dictDetail.FirstOrDefault(m => m.DictKey == dictKey);
            if (mdd == null) return null;

            IEnumerable<Models.vwCHIS_Code_DictDetail> rlt = null;

            if (!string.IsNullOrWhiteSpace(pywb))
            {
                if (pywb.HasChinese())
                {//如果包含汉字 则采用汉字搜索
                    rlt = dictDetail.Where(m =>
                             m.DictID == mdd.DictID && (m.ItemName != null && m.ItemName.Contains(pywb)));
                }
                else
                {
                    pywb = pywb.Trim();
                    if (useime == "WUBI") rlt = dictDetail.Where(m =>
                             m.DictID == mdd.DictID && (m.WbCode != null && m.WbCode.StartsWith(pywb, StringComparison.CurrentCultureIgnoreCase)));

                    if (useime == "PINYIN")
                    {
                        //首先全拼 然后简拼
                        var qplist = dictDetail.Where(m => m.DictID == mdd.DictID && m.QPcode.Contains(pywb.ToLower())).ToList();
                        rlt = dictDetail.Where(m =>
       m.DictID == mdd.DictID && (m.PyCode != null && m.PyCode.StartsWith(pywb, StringComparison.CurrentCultureIgnoreCase)));
                        qplist.AddRange(rlt);
                        rlt = qplist;
                    }
                }

                if (groupTag.IsNotEmpty()) rlt = rlt.Where(m => m.GroupTag.IsNotEmpty() && m.GroupTag.ContainsKeyId(groupTag));
                rlt = rlt.Where(m => m.ItemName != null);
                rlt = rlt.OrderBy(m => m.ShowOrder ?? 10000).OrderBy(m => m.ItemName.Length);  //按排列顺序显示               
                if (maxRows.HasValue) return rlt.Take(maxRows.Value);
                return rlt;
            }

            rlt = dictDetail.Where(m => m.DictID == mdd.DictID);
            if (groupTag.IsNotEmpty()) rlt = rlt.Where(m => m.GroupTag.IsNotEmpty() && m.GroupTag.ContainsKeyId(groupTag));
            rlt = rlt.OrderBy(m => m.ShowOrder ?? 10000);  //默认返回
            if (maxRows.HasValue) return rlt.Take(maxRows.Value);
            return rlt;
        }







        #endregion




    }
}
