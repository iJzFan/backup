using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models
{

    /// <summary>
    /// 剑客数据导入
    /// </summary>
    public partial class TEMP_Excel_JKImport
    {
        /// <summary>
        /// 检查数据 错误则抛错
        /// </summary> 
        public bool CheckData()
        {
            List<string> exps = new List<string>();
            if (this.ThreePartDrugId == 0) exps.Add("三方药品Id不能为空");
            if (string.IsNullOrWhiteSpace(this.UnitName)) exps.Add("单位不能为空");
            if (this.UnitId == 0) exps.Add($"单位Id不能为0,可能是转换单位【{UnitName}】错误");
            if (JKPrice == 0) exps.Add("必须要有药品价格");
            if (exps.Count == 0) return true;
            throw new Exception(string.Join(";", exps));
        }
    }
}
