using Ass;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ah.Code.Managers
{

    /// <summary>
    /// 我的Razer前端工具
    /// </summary>
    public interface IMyRazor
    {
        /// <summary>
        /// 获取列表清单
        /// </summary>
        /// <param name="tvstr">[Text,Value]比如 "[北京,BeiJing][天津,TianJin]" 或者"[Yes][No][Cancel]"则显示和值都是一致的</param>
        IEnumerable<SelectListItem> GetSelectListItems(string tvstr, string selvalue = null);


        /// <summary>
        /// 根据字典类别码来获取字典内的信息
        /// </summary>
        /// <param name="dictKey">字典类别码</param>
        /// <param name="selectedvalues">选择的值</param>
        /// <param name="isUseValue">是否使用值，否则会用keyId</param>
        /// <param name="onlyEnabled">是否只选Enable可用的项目</param>
        /// <param name="showHidden">是否显示隐藏的项目</param>
        /// <param name="tagGroup">目标组，可以用逗号分割</param>
        IEnumerable<SelectListItem> GetSelectListItemsByDictKey(string dictKey, string selectedvalues = null, bool isUseValue = false, bool onlyEnabled = true, bool showHidden = false, string tagGroup = null);


        /// <summary>
        /// 获取详细功能的设定值
        /// </summary>
        /// <param name="functionKey">所属功能板块</param>
        /// <param name="key">设定键名</param>
        /// <returns>各种值类型</returns>
        Ass.ObjReturn GetFuncDetailSettedValue(string functionKey, string key, int doctorId, int stationId);

        /// <summary>
        /// 获取人员头像
        /// </summary>
        /// <param name="dburl">数据库内的Url</param>
        /// <param name="gender">性别</param>
        string GetCustomerDefImagePath(string dburl, int? gender);

    }
    public class MyRazor : BaseInject, IMyRazor
    {
        public ObjReturn GetFuncDetailSettedValue(string functionKey, string key, int doctorId, int stationId)
        {
            string sql = string.Format("exec sp_Sys_GetFuncDetailAccess '{0}','{1}',{2},{3}", functionKey, key, doctorId, stationId);
            typevalue rtn = MainDbContext.SqlQuery<typevalue>(sql).First();
            return new Ass.ObjReturn(rtn.value, rtn.type);
        }

        public IEnumerable<SelectListItem> GetSelectListItems(string tvstr, string selvalue = null)
        {
            List<SelectListItem> rtn = new List<SelectListItem>();
            if (!string.IsNullOrWhiteSpace(tvstr))
            {
                var dds = tvstr.Trim().Split(']');
                foreach (string s in dds)
                {
                    string[] tmps = s.Trim('[', ']').Split(',');
                    string text = "", value = "";
                    if (tmps.Length == 1) text = value = tmps[0];
                    else if (tmps.Length == 2) { text = tmps[0]; value = tmps[1]; }
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (string.IsNullOrWhiteSpace(selvalue))
                            rtn.Add(new SelectListItem
                            {
                                Text = text,
                                Value = value
                            });
                        else
                            rtn.Add(new SelectListItem
                            {
                                Text = text,
                                Value = value,
                                Selected = selvalue.Trim().Equals(value, StringComparison.CurrentCultureIgnoreCase)
                            });
                    }
                }
            }
            return rtn;
        }


        public IEnumerable<SelectListItem> GetSelectListItemsByDictKey(string dictKey, string selectedvalues = null, bool isUseValue = false, bool onlyEnabled = true, bool showHidden = false, string tagGroup = null)
        {
            if (Global.DictDetail == null) Global.Initial();
            var rlt = Global.DictDetail.Where(m => m.DictKey == dictKey);
            if (onlyEnabled) rlt = rlt.Where(m => m.IsEnable != false);
            if (!showHidden) rlt = rlt.Where(m => m.IsDefaultHidden != true);

            IEnumerable<Models.vwCHIS_Code_DictDetail> finds = new List<Models.vwCHIS_Code_DictDetail>();//申明一个新的存放结果
            if (!string.IsNullOrEmpty(tagGroup))
            {
                string[] tags = tagGroup.Split(',');
                foreach (string tag in tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        finds = finds.Union(rlt = rlt.Where(m => m.GroupTag.ContainsKeyId(tag)));
                    }
                }
            }
            else finds = rlt;


            if (finds == null || finds.Count() == 0) return new List<SelectListItem>();

            if (string.IsNullOrWhiteSpace(selectedvalues))
            {
                return from item in finds
                       select new SelectListItem
                       {
                           Text = item.ItemName, //字典字段名称
                           Value = isUseValue ? item.ItemValue : item.DetailID.ToString(),
                           Selected = item.IsDefault == true
                       };
            }
            else 
            {
                return from item in finds
                       select new SelectListItem
                       {
                           Text = item.ItemName, //字典字段名称
                           Value = isUseValue ? item.ItemValue : item.DetailID.ToString(),
                           Selected = selectedvalues.ContainsKeyId(item.DetailID.ToString())
                       };
            }
        }




        /// <summary>
        /// 获取人员头像
        /// </summary>
        /// <param name="dburl">数据库内的Url</param>
        /// <param name="gender">性别</param>
        /// <returns></returns>
        public string GetCustomerDefImagePath(string dburl, int? gender)
        {
            if (dburl.IsNotEmpty() && dburl.Contains("http")) return dburl;
            return Global.ConfigSettings.CustomerImagePathRoot + Models.PhotoUtils.CustomerPhotoUrlDef(dburl, gender);
        }


    }
}
