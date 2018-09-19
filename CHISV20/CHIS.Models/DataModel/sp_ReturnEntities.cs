using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.DataModel
{


    /// <summary>
    /// 获取角色的详细权限设置值
    /// </summary>
   public class sp_Sys_GetRoleFuncDetailsByRoleId
    {
        public string FunctionKey { get; set; }
        public int FuncDetailId { get; set; }
        public string FunctionName { get; set; }
        public string GroupKey { get; set; }
        public string FuncDetailName { get; set; }
        /// <summary>
        /// 功能说明
        /// </summary>
        public string FuncDetailRmk { get; set; }
        public string KeyName { get; set; }
        public string TypeName { get; set; }
        public string DefValue { get; set; }
        public string ValueSetted { get; set; }
        public int BelongFunctionId { get; set; }
        public int RoleId { get; set; }

        /// <summary>
        /// 多值平准法
        /// </summary>
        public string MultiMethod { get; set; }

    }
    public class typevalue
    {
        public string type { get; set; }
        public object value { get; set; }
    }
   
}
