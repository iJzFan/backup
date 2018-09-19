using Ass;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Code.Managers
{

    /// <summary>
    /// 我的Razer前端工具
    /// </summary>
    public interface IMyRazor
    {

        /// <summary>
        /// 获取字典详细名
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetDictDetailNameById(int? id);

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
        /// 获取人员头像
        /// </summary>
        /// <param name="dburl">数据库内的Url</param>
        /// <param name="gender">性别</param>
        string GetCustomerDefImagePath(string dburl, int? gender);

        /// <summary>
        /// 获取图片的Url，如果有http则直接使用
        /// </summary>
        /// <param name="baseroot">父目录</param>
        /// <param name="imgurl">图片名</param>        
        string GetImageUrl(string baseroot, string imgname);

        /// <summary>
        /// 载入导航的信息
        /// </summary>
        /// <param name="funcId"></param>
        /// <returns></returns>
        string loadPageNav(int funcId);

        Models.SYS_ChinaArea GetChinaAreaById(int? areaId);

        /// <summary>
        /// 从数据库获取菜单，避免数据库大量重复运算
        /// </summary>
        /// <returns></returns>
        Task<IHtmlContent> GetMenuFromDBAsync(int doctorId, int stationId);
        /// <summary>
        /// 将Html写入数据库
        /// </summary>
        Task<IHtmlContent> WriteMenu2DBAsync(IHtmlContent html, int doctorId, int stationId);

        /// <summary>
        /// 获取工作站的部门
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        IEnumerable<SelectListItem> GetDepartsOfStation(int? stationId, bool bAllDepart = false);

    }
    public class MyRazor : BaseInject, IMyRazor
    {
        public async Task<IHtmlContent> GetMenuFromDBAsync(int doctorId, int stationId)
        {
            var find = await MainDbContext.CHIS_Sys_UserMenu.AsNoTracking().FirstOrDefaultAsync(m => m.DoctorId == doctorId && m.StationId == stationId);
            return new HtmlContentBuilder().AppendHtml(find?.MenuContent);
        }
        public async Task<IHtmlContent> WriteMenu2DBAsync(IHtmlContent html, int doctorId, int stationId)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            StringBuilder sb = new StringBuilder();
            System.IO.TextWriter tw = new System.IO.StringWriter(sb);
            html.WriteTo(tw, System.Text.Encodings.Web.HtmlEncoder.Default);
            tw.Dispose();sw.Dispose();
            var find = await MainDbContext.CHIS_Sys_UserMenu.AsNoTracking().FirstOrDefaultAsync(m => m.DoctorId == doctorId && m.StationId == stationId);
            if (find == null)
            {
                await MainDbContext.AddAsync(new Models.CHIS_Sys_UserMenu
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctorId,
                    StationId = stationId,
                    MenuContent = sb.ToString()
                });
            }
            else
            {
                find.MenuContent = sb.ToString();
                MainDbContext.Update(find);
            }
            await MainDbContext.SaveChangesAsync();
            return html;
        }

        public Models.SYS_ChinaArea GetChinaAreaById(int? areaId)
        {
            if (areaId.HasValue && areaId > 0)
                return Global.db_ChinaArea.FirstOrDefault(m => m.AreaId == areaId);
            return null;
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


        public string GetDictDetailNameById(int? id)
        {
            if (!id.HasValue || id <= 0) return "";
            if (Global.db_DictDetail == null) Global.Initial().Wait(Global.WAIT_MSEC);
            return Global.db_DictDetail.FirstOrDefault(m => m.DetailID == id)?.ItemName;
        }

        public IEnumerable<SelectListItem> GetSelectListItemsByDictKey(string dictKey, string selectedvalues = null, bool isUseValue = false, bool onlyEnabled = true, bool showHidden = false, string tagGroup = null)
        {
            if (Global.db_DictDetail == null) Global.Initial().Wait(Global.WAIT_MSEC);
            var rlt = Global.db_DictDetail.Where(m => m.DictKey == dictKey);
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

            finds = finds.OrderBy(m => m.ShowOrder);
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

        public string GetImageUrl(string baseroot, string imgname)
        {
            var rtn = imgname;
            if (imgname.IsNotEmpty() && imgname.Contains("http")) rtn= imgname;
            rtn= baseroot + imgname;
            if (rtn[rtn.Length - 1] == '/') return string.Empty;
            return rtn;
        }


        public string loadPageNav(int funcId)
        {
            StringBuilder b = new StringBuilder();
            int functionId = funcId;
            if (functionId > 0)
            {
                b.Append("<ul class='page-nav'>");
                var finds = MainDbContext.CHIS_SYS_Function.FromSql("exec sp_Sys_GetFunctionsToRoot {0}", functionId).ToList();
                for (int i = finds.Count - 1; i >= 0; i--)
                {
                    var item = finds[i];
                    if (string.IsNullOrEmpty(item.UrlAddress)) b.AppendFormat("<li>{0}</li>", item.FunctionName);
                    else b.AppendFormat("<li><a href='{1}?pagefn={2}'>{0}</a></li>", item.FunctionName, item.UrlAddress, item.FunctionID);
                }
                b.Append("</ul>");
            }
            return b.ToString();
        }

        public IEnumerable<SelectListItem> GetDepartsOfStation(int? stationId,bool bAllDepart=false)
        {
            var find= new BllCaller.StationDepartmentBllCaller().DepartsOfStation(stationId.Value, bAllDepart);
            return find.Select(m => new SelectListItem
            {
                Text = m.DepartmentName,
                Value = m.DepartmentID.ToString()
            });
        }
    }



}
