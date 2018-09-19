using Ass;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ah
{
    public class Global
    {
        public readonly static string PROJECTNAME = "CustomerWeb";

        public readonly static string AUTHENTICATION_SCHEME = "AppCookieAuthenticationScheme" + PROJECTNAME;
        public readonly static string AUTHENTICATION_CLAIMS_IDENTITY = "AccountClaimsIdentity" + PROJECTNAME;
        public readonly static string AUTHENTICATION_ISSUER = "https://www.jk213.com/anglehealth" + PROJECTNAME;
        public readonly static string AUTHENTICATION_COOKIE_NAME = ".CookieAccountName" + PROJECTNAME;

        public readonly static string ROUTE_FORBIDDEN = "/Home/Forbidden";
        public readonly static string ROUTE_LOGIN = "/Customer/Home/CustomerLogin";
        /// <summary>
        /// 登出路径
        /// </summary>
        public readonly static string ROUTE_LOGOUT = "/Customer/Home/CustomerLoginOut";
        public readonly static string ROUTE_ERROR = "/Customer/Home/Error";
        /// <summary>
        /// 登录Schme
        /// </summary>
        public readonly static string SIGN_IN_SCHEME = "http://my.jk213.com/signin#" + PROJECTNAME;

        public readonly static string CHIS_HOST = "http://chis.jk213.com";

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
                CustomerImagePathRoot = getConfigUri("SystemSettings:CustomerImgPathRoot");
                DoctorImagePathRoot = getConfigUri("SystemSettings:DoctorImgPathRoot");
                StationImagePathRoot = getConfigUri("SystemSettings:StationImgPathRoot");
                CertificateImagePathRoot = getConfigUri("SystemSettings:CertificateImagePathRoot");
            }
            private static string getConfigUri(string secName, bool isUrl = true)
            {
                var rlt = Config.GetSection(secName).Value;
                if (!string.IsNullOrEmpty(WebHost)) rlt = rlt.Replace("localhost", WebHost);
                if (isUrl) return rlt;
                else return Ass.IO.WinPathToUnionPath(rlt);//转换成统一地址
            }

            /// <summary>
            /// 用户图像根目录
            /// </summary>
            public static string CustomerImagePathRoot = getConfigUri("SystemSettings:CustomerImgPathRoot");
            /// <summary>
            /// 医生头像根目录
            /// </summary>
            public static string DoctorImagePathRoot = getConfigUri("SystemSettings:DoctorImgPathRoot");
            /// <summary>
            /// 工作站图像根路径
            /// </summary>
            public static string StationImagePathRoot = getConfigUri("SystemSettings:StationImgPathRoot");

            /// <summary>
            /// 证件图像的根路径
            /// </summary>
            public static string CertificateImagePathRoot = getConfigUri("SystemSettings:CertificateImagePathRoot");

            /// <summary>
            /// 日志路径
            /// </summary>
            public static string LogPath = getConfigUri("RdSettings:LogPath");


        }

        /// <summary>
        /// 可关注诊所上限
        /// </summary>
        public static int MaxStationFollow = 20;

        /// <summary>
        /// 可关注医生上限
        /// </summary>
        public static int MaxDoctorFollow = 20;

        /// <summary>
        /// 最近预约诊所记录上限
        /// </summary>
        public static int MaxStationRecent = 20;

        /// <summary>
        /// 最近预约医生记录上限
        /// </summary>
        public static int MaxDoctorRecent = 20;



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



        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="level">   "LogLevel": "Info,Debug,Warning,Error,Excep"</param>
        public static void WriteLog(object obj, string key = "", bool isToJson = false, string level = "Info")
        {
            try
            {
                var isLog = Ass.P.PBool(ah.Global.Config.GetSection("RdSettings:IsLog").Value, false);
                if (isLog)
                {

                    var lv = "," + Ass.P.PStr(ah.Global.Config.GetSection("RdSettings:LogLevel").Value).ToLower() + ",";
                    if (obj is Exception) { level = "Excep"; }
                    var ba = lv.Contains("," + level.ToLower() + ",");//是否允许保存本级别的日志                

                    if (ba)
                    {
                        //获取调用数据位置 .netcore 2.0才能支持
                    


                        string c = $"{obj}";
                        if (obj is Exception) c = ((Exception)obj).Message;
                        if (isToJson) c = Newtonsoft.Json.JsonConvert.SerializeObject(obj);//转为json

                        string p = ah.Global.ConfigSettings.LogPath;
                        var f = System.IO.Path.Combine(p, $"ah_{ah.Global.PROJECTNAME}_{DateTime.Now.ToString("yyyyMMdd")}.log");
                        if (!System.IO.Directory.Exists(p)) System.IO.Directory.CreateDirectory(p);

                        var keys = string.IsNullOrWhiteSpace(key) ? "" : key + "\t: ";
                        System.IO.File.AppendAllText(f, $"[{DateTime.Now.ToString("yyyyMMddHHmmss.fff")}]\t{keys}{c}\r\n");
                    }
                }
            }
            catch  { }
        }


    }
}
