using CHIS.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS
{

    /// <summary>
    /// 字典类的基础类
    /// </summary>
    public class BaseDictValue
    {

        /// <summary>
        /// 大类的Id
        /// </summary>
		public int DictId { get; set; }
        /// <summary>
        /// 大类的Key
        /// </summary>
        public string DictKey { get; set; }
        /// <summary>
        /// 大类的Name
        /// </summary>
        public string DictName { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsEnable { get; set; }
        /// <summary>
        /// 是否是值类型
        /// </summary>
        public bool IsValueCode { get; set; }

        public BaseDictValue(int dictId,string dictKey,string dictName,bool isEnable,bool isValueCode)
        {
            this.DictId = dictId; this.DictKey = dictKey; this.DictName =dictName;
            this.IsEnable = isEnable; this.IsValueCode = isValueCode;
        }

        /// <summary>
        /// 获取详细名称
        /// </summary>
        public string GetName(int? detailId,string defVal="") {
            try
            {
                if (detailId.HasValue) return Items.Find(m => m.DetailID == detailId)?.ItemName;
                return defVal;
            }
            catch(Exception ex) { return defVal; }
        }
        /// <summary>
        /// 获取详细名称
        /// </summary>
        public string GetName(string value) { return Items.Find(m => m.ItemValue == value)?.ItemName; }
        /// <summary>
        /// 获取详细Id
        /// </summary>
        public int? GetDetailId(string name) { return Items.Find(m => m.ItemName == name)?.DetailID; }
        /// <summary>
        /// 获取详细值
        /// </summary>
        public string GetDetailValue(string name) { return Items.Find(m => m.ItemName == name)?.ItemValue; }
        /// <summary>
        /// 值集合
        /// </summary>
        public List<CHIS_Code_Dict_Detail> Items = new List<CHIS_Code_Dict_Detail>();
    }
}
