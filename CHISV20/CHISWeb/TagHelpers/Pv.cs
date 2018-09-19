using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Html;

namespace CHIS.TagHelpers
{
    /// <summary>
    /// 用于进行partialview载入的工具
    /// </summary>
    [HtmlTargetElement("pv")]
    public class PvTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
    {

        /// <summary>
        /// Additional parameters for the route.
        /// 额外参数
        /// </summary>
        [HtmlAttributeName("url-pms-", DictionaryAttributePrefix = "url-pms-")]
        public IDictionary<string, string> UrlParms { get; set; } =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 连接后台地址
        /// </summary>
        [HtmlAttributeName("url")]
        public string Url { get; set; }


        /// <summary>
        /// 允许刷新
        /// </summary>
        [HtmlAttributeName("refresh-allowed")]
        public bool RefreshAllowed { get; set; }





        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string tagid = Guid.NewGuid().ToString("N");
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            HtmlContentBuilder builder = new HtmlContentBuilder();
            HtmlContentBuilder js = new HtmlContentBuilder();
            if(RefreshAllowed) builder.AppendHtml($"<a class='refresh' tagid='refresh_{tagid}' title='刷新'><i class='fa fa-refresh'></i>&nbsp;刷新</a>");
            builder.AppendHtml($"<div tagid='{tagid}'></div>");

            string jn = "";
            foreach (var k in UrlParms.Keys) { jn += $"{k}:'{UrlParms[k]}',"; }
            js.AppendHtml("<script>$(function(){");//js开始
            js.AppendHtml("var o={{$c:$('[tagid={0}]'),load:function(){{o.$c.load('{1}',{{{2}}});}}}};o.load();", tagid,Url,jn);
            //如果需要刷新按钮
            if (RefreshAllowed)
            { 
                js.AppendHtml("var $b=$('[tagid=refresh_{0}]');",tagid);
                js.AppendHtml("$b.on('click',function(){o.load();});");
            }
            js.AppendHtml("});</script>");//js结束

            //合成代码
            builder.AppendHtml(js);
            output.Content.AppendHtml(builder);

            base.Process(context, output);
        }
    }
}
