using Ass;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CHIS
{
    public class Global
    {
        #region 全局变量
        /// <summary>
        /// 程序名
        /// </summary>
        public readonly static string PROJECTNAME = "CHISWeb";

        /// <summary>
        /// 验证Scheme
        /// </summary>
        public readonly static string AUTHENTICATION_SCHEME = "AppCookieAuthenticationScheme" + PROJECTNAME;
        public readonly static string AUTHENTICATION_CLAIMS_IDENTITY = "AccountClaimsIdentity" + PROJECTNAME;
        public readonly static string AUTHENTICATION_ISSUER = "https://www.jk213.com/anglehealth" + PROJECTNAME;
        public readonly static string AUTHENTICATION_COOKIE_NAME = ".CookieAccountName" + PROJECTNAME;
        /// <summary>
        /// 登录Schme
        /// </summary>
        public readonly static string SIGN_IN_SCHEME = "http://chis.jk213.com/signin#" + PROJECTNAME;

        /// <summary>
        /// 禁止路径
        /// </summary>
        public readonly static string ROUTE_FORBIDDEN = "/Home/Forbidden";
        /// <summary>
        /// 登录路径
        /// </summary>
        public readonly static string ROUTE_LOGIN = "/Home/Login";
        /// <summary>
        /// 登出路径
        /// </summary>
        public readonly static string ROUTE_LOGOUT = "/Home/OutLogin";
        /// <summary>
        /// 默认错误页路径
        /// </summary>
        public readonly static string ROUTE_ERROR = "/Home/Error";

        public readonly static int WAIT_MSEC = 10000;
        #endregion


        #region 验证常量
        /// <summary>
        /// 登录用户验证
        /// </summary>       
        public readonly static string JWT_LOGIN_USER_CLAIMS_IDENTITY = "JwtUserAuth" + PROJECTNAME;
     
        #endregion


        /// <summary>
        /// 全局加密密码
        /// </summary>
        public static readonly string SYS_ENCRIPT_PWD="tsjk@2018";



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

        /// <summary>
        /// 配置器
        /// </summary>
        public static Microsoft.Extensions.Configuration.IConfiguration Config { get; set; }

        /// <summary>
        /// 是否调试
        /// </summary>
        public static bool IsDebug { get { return Ass.P.PBool(Config.GetSection("RdSettings:IsDebug").Value); } }

        #region 缓存字典数据
        /// <summary>
        /// 清空
        /// </summary>
        public static bool ClearTempData()
        { 
            _db_ChinaArea = null;
            _db_dictDetail = null;
            _db_FuncAccess = null;
            return true;
        }

        /// <summary>
        /// 字典数据静态缓存
        /// </summary>
        public static IEnumerable<Models.vwCHIS_Code_DictDetail> db_DictDetail
        {
            get
            {
                if (_db_dictDetail == null) _Initial_Dict().Wait(WAIT_MSEC);
                return _db_dictDetail;
            }
        }
        public static IEnumerable<Models.SYS_ChinaArea> db_ChinaArea
        {
            get
            {
                if (_db_ChinaArea == null) _Initial_ChinaArea().Wait(WAIT_MSEC);
                return _db_ChinaArea;
            }
        }
        public static IEnumerable<Models.CHIS_Sys_FuncAccess> db_FuncAccess
        {
            get
            {
                if (_db_FuncAccess == null) _Initial_FuncAccess().Wait(WAIT_MSEC);
                return _db_FuncAccess;
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public static async Task Initial()
        {
            await _Initial_Dict();  
            await _Initial_ChinaArea();
            await _Initial_FuncAccess();
            //初始化文档
            if (!System.IO.Directory.Exists(ConfigSettings.ImportFilePath))
                System.IO.Directory.CreateDirectory(ConfigSettings.ImportFilePath);
            if (!System.IO.Directory.Exists(ConfigSettings.ExportFilePath))
                System.IO.Directory.CreateDirectory(ConfigSettings.ExportFilePath);
        }
        public static async Task _Initial_Dict()
        {
            var db = new Code.Utility.DataBaseHelper().GetMainDbContext();
            _db_dictDetail = null;
            _db_dictDetail = await db.vwCHIS_Code_DictDetail.AsNoTracking().ToListAsync();
        }
        public static async Task _Initial_ChinaArea()
        {
            var db = new Code.Utility.DataBaseHelper().GetMainDbContext();
            _db_ChinaArea = null;
            _db_ChinaArea = await db.SYS_ChinaArea.AsNoTracking().ToListAsync();
        }
        public static async Task _Initial_FuncAccess()
        {
            var db = new Code.Utility.DataBaseHelper().GetMainDbContext();
            _db_FuncAccess = null;
            _db_FuncAccess = await db.CHIS_Sys_FuncAccess.AsNoTracking().ToListAsync();
        }


        private static IEnumerable<Models.vwCHIS_Code_DictDetail> _db_dictDetail = null;
        static IEnumerable<Models.SYS_ChinaArea> _db_ChinaArea = null;
        static IEnumerable<Models.CHIS_Sys_FuncAccess> _db_FuncAccess = null;

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
            var mdd = _db_dictDetail.FirstOrDefault(m => m.DictKey == dictKey);
            if (mdd == null) return null;

            IEnumerable<Models.vwCHIS_Code_DictDetail> rlt = null;

            if (!string.IsNullOrWhiteSpace(pywb))
            {
                if (pywb.HasChinese())
                {//如果包含汉字 则采用汉字搜索
                    rlt = _db_dictDetail.Where(m =>
                             m.DictID == mdd.DictID && (m.ItemName != null && m.ItemName.Contains(pywb)));
                }
                else
                {
                    pywb = pywb.Trim();
                    if (useime == "WUBI") rlt = _db_dictDetail.Where(m =>
                             m.DictID == mdd.DictID && (m.WbCode != null && m.WbCode.StartsWith(pywb, StringComparison.CurrentCultureIgnoreCase)));

                    if (useime == "PINYIN")
                    {
                        //首先全拼 然后简拼
                        var qplist = _db_dictDetail.Where(m => m.DictID == mdd.DictID && m.QPcode.Contains(pywb.ToLower())).ToList();
                        rlt = _db_dictDetail.Where(m =>
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

            rlt = _db_dictDetail.Where(m => m.DictID == mdd.DictID);
            if (groupTag.IsNotEmpty()) rlt = rlt.Where(m => m.GroupTag.IsNotEmpty() && m.GroupTag.ContainsKeyId(groupTag));
            rlt = rlt.OrderBy(m => m.ShowOrder ?? 10000);  //默认返回
            if (maxRows.HasValue) return rlt.Take(maxRows.Value);
            return rlt;
        }


        /// <summary>
        /// 单位换算成Id
        /// </summary>
        /// <param name="unitName"></param>
        /// <returns></returns>
        internal static int UnitName2Id(string unitName)
        {
            try
            {
                var finds = GetDictDetailListByDictKey("GoodsUnit");
                var find = finds.FirstOrDefault(m => m.ItemName == unitName);
                return find.DetailID;
            }
            catch { return 0; }
        }
        /// <summary>
        /// Id转换成单位名
        /// </summary> 
        internal static string UnitId2Name(int unitId)
        {
            try
            {
                return _db_dictDetail.FirstOrDefault(m => m.DetailID == unitId).ItemName;
            }
            catch { return ""; }
        }


        #endregion



        /// <summary>
        /// 配置设置
        /// </summary>
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
                QrScanUrl = getConfigUri("SystemSettings:QrScanUrl");
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
            /// 药品图像的根路径
            /// </summary>
            public static string DrugImagePathRoot = getConfigUri("SystemSettings:DrugImagePathRoot");

            /// <summary>
            /// 上载图片API
            /// </summary>
            public static string UploadImageAPI = getConfigUri("SystemSettings:UploadImageAPI");

            /// <summary>
            /// 健客图片基地址
            /// </summary>
            public static string JKImageRoot = getConfigUri("SystemSettings:JKImageRoot");

            /// <summary>
            /// 二维码扫描地址
            /// </summary>
            public static string QrScanUrl = getConfigUri("SystemSettings:QrScanUrl");
            /// <summary>
            /// WebSocket的服务器根地址
            /// </summary>
            public static string WSServerRoot =Global.Localhost2Ip(Config.GetSection("WebSocketService:ServerRoot").Value);

            


            /// <summary>
            /// 导出文件暂存
            /// </summary>
            public static string ExportFilePath = getConfigUri("SystemSettings:ExportFilePath", false);
            /// <summary>
            /// 导入文件暂存
            /// </summary>
            public static string ImportFilePath = getConfigUri("SystemSettings:ImportFilePath", false);
        }


        /// <summary>
        /// App的接口信息
        /// </summary>
        public static class AppInterface
        {
            /// <summary>
            /// App医生的在线状态
            /// </summary>
            public static string DoctorAppOnlineStatusUrl { get; set; } = Global.Config.GetSection("AppInterface:DoctorAppOnlineStatusUrl").Value;
        }

    }
}
