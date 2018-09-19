using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS
{
    public static class AssExpands
    {

        /// <summary>
        /// 判断Id是否为需要新增的空Id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool IsIdEmpty(this Guid guid)
        {
            if (guid == new Guid()) return true;
            else return false;
        }
        public static bool IsIdEmpty(this Guid? guid)
        {
            if (!guid.HasValue) return true;
            if (guid.HasValue && guid == new Guid()) return true;
            return false;
        }
        public static bool IsIdEmpty(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return true;
            str = (str + "").Trim();
            if (str == "0") return true;    
            if (str == new Guid().ToString()) return true;
            return false;
        }

        /// <summary>
        /// 获取项目数据工具
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DtUtil ahDtUtil(this object obj)
        {
            return new DtUtil(obj);
        }



    }

}
