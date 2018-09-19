using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ass;
using CHIS.Models;
using Microsoft.AspNetCore.Http;

namespace CHIS
{
    /// <summary>
    /// 通用数据处理类
    /// </summary>
    public class DtUtil
    {
        internal object orig = null;
        internal string origStr
        {
            get
            {
                if (orig is string) return (string)orig;
                else return $"{orig}";
            }
        }
        internal DateTime origDateTime
        {
            get
            {
                if (orig is DateTime) return (DateTime)orig;
                else
                {
                    return Ass.P.PDateTimeV(orig, new DateTime()).Value;
                }
            }
        }
        public DtUtil(object s)
        {
            orig = s;
        }


        /// <summary>
        /// 获取工作站图片
        /// </summary>
        /// <param name="defType">图片类别</param>
        /// <returns></returns>
        public string GetStationImg(imgSizeTypes imgSizeType = imgSizeTypes.HorizNormal)
        {
            var defimg = "";
            switch (imgSizeType)
            {
                case imgSizeTypes.VerticalNormal: defimg = "_station_v.jpg"; break;
                default: defimg = "_station_h.jpg"; break;
            }
            defimg = Global.ConfigSettings.StationImagePathRoot + defimg;
            return origStr.GetUrlPath(Global.ConfigSettings.StationImagePathRoot, defimg);
        }

        /// <summary>
        /// 获取医生图片
        /// </summary>
        /// <param name="defType">图片类别</param>
        /// <returns></returns>
        public string GetDoctorImg(int? gender, imgSizeTypes imgSizeType = imgSizeTypes.HorizNormal)
        {
            string gd = gender == 0 ? "0" : (gender == 1 ? "1" : "");
            var defimg = Global.ConfigSettings.DoctorImagePathRoot + $"_d256_{gd}.png";
            return origStr.GetUrlPath(Global.ConfigSettings.DoctorImagePathRoot, defimg);
        }


        /// <summary>
        /// 获取用户图片的网络地址
        /// </summary>
        /// <param name="gender">性别</param>
        /// <returns></returns>
        public string GetCustomerImg(int? gender)
        {
            string gd = gender == 0 ? "0" : (gender == 1 ? "1" : "");
            var defimg = Global.ConfigSettings.CustomerImagePathRoot + $"_u256_{gd}.png";
            string rlt = origStr.GetUrlPath(Global.ConfigSettings.CustomerImagePathRoot, defimg);
            return rlt;
        }


        /// <summary>
        /// 获取药品图片
        /// </summary>
        /// <param name="mkc">类别</param>
        /// <param name="isThumbnail">缩略图</param>
        /// <returns></returns>
        public string GetDrugImg(string mkc = null, bool isThumbnail = true)
        {
            string imgname = origStr;
            string root = Global.ConfigSettings.DrugImagePathRoot;
            if (imgname.IsNotEmpty())
            {
                if (imgname.ToLower().IndexOf("http") >= 0)
                {
                    if (isThumbnail) return imgname;
                    else return imgname.Replace("image/sl", "image/");
                }

                int p = imgname.LastIndexOf('.'); string n = imgname.Substring(0, p);
                if (isThumbnail) return root + n + "_72" + imgname.Substring(p);

                return root + imgname;

            }
            //默认药品图片
            switch (mkc)
            {
                case "ZYM": //中草药
                    return root + "herbdef.jpg";
                case "ZYC"://中成药
                    return root + "zycdef.jpg";
                case "CZ"://处置
                    return root + "czdef.jpg";
                default:
                    return root + "drugdef.jpg";
            }

        }



        #region 日期时间转换

        /// <summary>
        /// 转换成简单日期19880830
        /// </summary>
        /// <returns></returns>
        public string ToShortDate()
        {
            if (origDateTime == new DateTime()) return "";
            return origDateTime.ToString("yyyyMMdd");
        }
        /// <summary>
        /// 转换成简单日期时间 19880830T125930
        /// </summary>
        /// <returns></returns>
        public string ToShortDateTime()
        {
            if (origDateTime == new DateTime()) return "";
            return origDateTime.ToString("yyyyMMddTHHmmss");
        }

        /// <summary>
        /// 缩写日期转日期
        /// </summary>
        /// <param name="shortDateString"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public DateTime? ParseDateOfShortString(string shortDateString, DateTime? def = null)
        {
            try
            {
                if (shortDateString.IsEmpty()) return def;
                else
                {
                    return DateTime.ParseExact(shortDateString, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None).Date;
                }
            }
            catch (Exception) { return def; }
        }

        /// <summary>
        /// 创建新的模型
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public dynamic NewModel<T>(Func<T, dynamic> p)
        {
            T[] a = { (T)orig };
            return a.Select(p).FirstOrDefault();
        }
        public Ass.Mvc.PageListInfo<dynamic> NewPagedModel<T>(Func<T, dynamic> p) where T : class
        {
            Ass.Mvc.PageListInfo<T> m = (Ass.Mvc.PageListInfo<T>)orig;
            return new Ass.Mvc.PageListInfo<dynamic>
            {
                DataList = m.DataList.Select(p),
                PageIndex = m.PageIndex,
                PageSize = m.PageSize,
                RecordTotal = m.RecordTotal
            };
        }
  

        /// <summary>
        /// 缩写日期时间字符串转日期时间
        /// </summary>
        /// <param name="shortDateString"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public DateTime? ParseDateTimeOfShortString(string shortDateString, DateTime? def = null)
        {
            try
            {
                if (shortDateString.IsEmpty()) return def;
                else
                {
                    return DateTime.ParseExact(shortDateString, "yyyyMMddTHHmmss", null, System.Globalization.DateTimeStyles.None);
                }
            }
            catch (Exception) { return def; }
        }


        /// <summary>
        /// 获取时间段字符串的时间范围
        /// </summary>
        /// <param name="timeRange"></param>
        /// <param name="defBeforeNowDays"></param>
        /// <param name="bEndDayInclude"></param>
        /// <returns></returns>
        public (DateTime, DateTime) TimeRange(string timeRange = null, int defBeforeNowDays = 7, bool bEndDayInclude = true)
        {
            var tstart = DateTime.Now.AddDays(0 - defBeforeNowDays);
            var tend = DateTime.Now;
            if (timeRange == null) timeRange = origStr;
            if (timeRange.IsNotEmpty())
            {
                int y = DateTime.Now.Year, m = DateTime.Now.Month;
                tend = DateTime.Now;
                DateTime today = DateTime.Today;
                var wStart = (int)today.DayOfWeek; wStart = wStart == 0 ? 7 : wStart;
                switch (timeRange.ToUpper())
                {
                    case "NEXT3DAYS": tstart = today.AddDays(1); tend = today.AddDays(4); break;
                    case "NEXT7DAYS": tstart = today.AddDays(1); tend = today.AddDays(8); break;
                    case "LAST3DAYS": tstart = today.AddDays(-2); tend = today; break;
                    case "LAST7DAYS": tstart = today.AddDays(-6); tend = today; break;
                    case "YESTERDAY": tstart = today.AddDays(-1); tend = tstart; break;
                    case "TODAY": tstart = DateTime.Today; break;
                    case "THISWEEK": tstart = today.AddDays(0 - wStart + 1); break;
                    case "THISMONTH": tstart = new DateTime(y, m, 1); break;
                    case "THISQUARTER": tstart = (new DateTime(y, m, 1)).AddMonths(0 - ((today.Month - 1) % 3)); break;
                    case "THISYEAR": tstart = new DateTime(y, 1, 1); break;
                    case "TOMORROW": tstart = tend = today.AddDays(1); break;
                    case "ALL": tstart = new DateTime(); tend = new DateTime(); break;
                    default:
                        //格式分解 dt0=2017-09-01;dt1=2017-09-08;
                        var dt0 = timeRange.GetValue<DateTime?>("dt0");
                        var dt1 = timeRange.GetValue<DateTime?>("dt1");
                        if (dt0 != null) tstart = dt0.Value;
                        if (dt1 != null) tend = dt1.Value;
                        break;
                }
            }
            if (bEndDayInclude) tend = tend.Date.AddDays(1);
            if (tend < tstart) throw new Exception($"结束时间{tend}比开始时间{tstart}要早");
            return (tstart, tend);
        }



        #endregion


        /// <summary>
        /// 添加网址，有编码
        /// </summary>
        /// <param name="querys">参数</param>
        /// <returns></returns>
        public string AddUrlQueryString(dynamic querys)
        {
            List<string> args = new List<string>();
            Type t = querys.GetType();
            System.Reflection.PropertyInfo[] pInfo = t.GetProperties();
            // 遍历公共属性
            foreach (System.Reflection.PropertyInfo pio in pInfo)
            {
                string fieldName = pio.Name;        // 公共属性的Name
                Type pioType = pio.PropertyType;    // 公共属性的类型         
                var val = t.GetProperty(fieldName).GetValue(querys, null);
                args.Add($"{fieldName}=" + System.Web.HttpUtility.UrlEncode($"{val}"));
            }
            return origStr + "?" + string.Join("&", args);
        }

        /// <summary>
        /// 初始化模板替换数据
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public string Format(dynamic datas)
        {
            return Ass.Utils.Format(origStr, datas);
        }








        #region staticFunc


        //地球半径，单位米
        private const double EARTH_RADIUS = 6378137;
        /// <summary>
        /// 计算两点位置的距离，返回两点的距离，单位 米
        /// 该公式为GOOGLE提供，误差小于0.2米
        /// </summary>
        /// <param name="lat1">第一点纬度</param>
        /// <param name="lng1">第一点经度</param>
        /// <param name="lat2">第二点纬度</param>
        /// <param name="lng2">第二点经度</param>
        /// <returns></returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = Rad(lat1);
            double radLng1 = Rad(lng1);
            double radLat2 = Rad(lat2);
            double radLng2 = Rad(lng2);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;
            double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
            return result;
        }

        /// <summary>
        /// 经纬度转化成弧度
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static double Rad(double d)
        {
            return (double)d * Math.PI / 180d;
        }



        public class UserSelfUtil
        {
            HttpContext _httpContext;
            public UserSelfUtil(HttpContext httpContext)
            {
                _httpContext = httpContext;
            }

            /// <summary>
            /// 获取用户的登录信息
            /// </summary>
            /// <param name="bThrowException"></param>
            /// <returns></returns>
            public UserSelf GetUserSelf(bool bThrowException = true)
            {
                Models.UserSelf rlt = new UserSelf
                {
                    OpId = 0,
                    OpMan = "UNKNOW"
                };
                try
                {
                    if (_httpContext.User != null && _httpContext.User.Identity.IsAuthenticated)
                        return new Models.UserSelf
                        {
                            LoginId = uvInt("LoginId"),
                            OpId = uvInt("OpId"),
                            DoctorId = uvInt("DoctorId"),
                            OpMan = _httpContext.User.Identity.Name,
                            StationId = uvInt("StationId"),
                            DrugStoreStationId = uvInt("DrugStoreStationId"),
                            StationName = uvStr("StationName"),
                            StationTypeId = uvInt("StationTypeId"),
                            SelectedDepartmentId = uvIntN("SelectedDepartmentId"),
                            SelectedDepartmentName = uvStr("SelectedDepartmentName"),
                            LoginTime = uvDateTime("LoginTime"),
                            Gender = uvInt("Gender"),
                            Birthday = uvDateTime("Birthday"),
                            IsCanTreat = uvBool("IsCanTreat"),
                            IsManageUnit = uvBool("IsManageUnit"),
                            MyRoleIds = uvIntC("MyRoleIds"),
                            MyRoleNames = uvStrC("MyRoleNames"),
                            PostTitleName = uvStr("PostTitleName"),
                            PhotoUrlDef = uvStr("PhotoUrlDef"),

                            MySonStations = uvIntC("MySonStations"),
                            MyAllowStationIds = uvIntC("MyAllowStationIds"),

                            DoctorAppId = uvStr("DoctorAppId"),

                            //辅助登录
                            LoginExtId = uvInt("LoginExtId"),
                            LoginExtMobile = uvStr("LoginExtMobile"),
                            LoginExtName = uvStr("LoginExtName"),
                            LoginExtFuncKeys = uvStr("LoginExtFuncKeys")

                        };
                    else throw new Exception("没有通过权限验证");
                }
                catch (Exception ex)
                {
                    if (bThrowException) throw new ComException(ExceptionTypes.Error_Unauthorized, "没有获取到权限数据", ex);
                }
                return rlt;
            }

            private int uvInt(string key, int defVal = 0) { return Ass.P.PIntV(_httpContext.User.FindFirst(key).Value, defVal); }
            private int? uvIntN(string key, int defVal = 0) { return Ass.P.PIntN(_httpContext.User.FindFirst(key).Value); }
            private int[] uvIntC(string key) { return Ass.P.PStr(_httpContext.User.FindFirst(key).Value).ToList<int>().ToArray(); }
            private string uvStr(string key) { return Ass.P.PStr(_httpContext.User.FindFirst(key).Value); }
            private string[] uvStrC(string key) { return Ass.P.PStr(_httpContext.User.FindFirst(key).Value).Split(','); }
            private DateTime uvDateTime(string key) { return new DateTime(long.Parse(_httpContext.User.FindFirst(key).Value)); }
            private bool uvBool(string key, bool defv = false) { return Ass.P.PBool(_httpContext.User.FindFirst(key).Value, defv); }

        }


        /// <summary>
        /// 获取班段信息
        /// </summary>
        /// <returns></returns>
        public string ToSlotStr()
        {
            if (orig == null) return "";
            int slot = Ass.P.PInt(orig);
            switch (slot)
            {
                case 1: return "上午";
                case 2: return "下午";
                case 3: return "深夜";
            }
            return "";
        }


        #endregion



    }
    public enum imgSizeTypes
    {
        HorizNormal,
        HorizThumb,
        VerticalNormal,
        VerticalThumb
    }
}
