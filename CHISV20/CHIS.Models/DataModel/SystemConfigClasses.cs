using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CHIS.Models
{
    /// <summary>
    /// 角色项目
    /// </summary>
    public class RoleItem
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 角色键名
        /// </summary>
        public string RoleKey { get; set; }
        /// <summary>
        /// 角色名
        /// </summary>
        public string RoleName { get; set; }
    }



    /// <summary>
    /// 地址区域
    /// </summary>
    public class AddrArea
    {
        public int AreaId { get; set; }
        public int ParentAreaId { get; set; }
        public string AreaName { get; set; }
        public IEnumerable<AddrArea> Children { get; set; }
        public string PinYin { get; set; }
        public string MergerName { get; set; }
    }


 

}
