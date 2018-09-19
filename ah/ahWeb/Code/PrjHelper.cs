using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ah.Code
{
    public static class PrjHelper
    {

        /// <summary>
        /// 班次信息
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string TransSlot(int? i)
        {
            switch (i)
            {
                case 1:return "上午";
                case 2:return "下午";
                case 3:return "夜班";
                case 4:return "深夜";
            }
            return "";
        }

        internal static object TransTreatStatus(string status)
        {
            switch (status.ToLower())
            {
                case "waiting":return "待诊";
                case "treating":return "接诊中";
                case "treated": return "已诊";                
                case "outtime": return "过期";
            }
            return "";
        }
    }

    

} 
