using CHIS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CHIS.Code.MyExpands
{
    public static class Json
    {
        public static object ToJson(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject(Json);
        }
        public static string ToJson(this object obj)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        public static string ToJson(this object obj, string datetimeformats)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = datetimeformats };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        public static T ToObject<T>(this string Json)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json);
        }
        public static List<T> ToList<T>(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<List<T>>(Json);
        }
        public static DataTable ToTable(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<DataTable>(Json);
        }
        public static JObject ToJObject(this string Json)
        {
            return Json == null ? JObject.Parse("{}") : JObject.Parse(Json.Replace("&nbsp;", ""));
        }



        public static string TreeGridJson(this List<TreeGridModel> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"rows\": [");
            sb.Append(TreeGridJson(data, -1, "0"));
            sb.Append("]}");
            return sb.ToString();
        }
        private static string TreeGridJson(List<TreeGridModel> data, int index, string parentId)
        {
            StringBuilder sb = new StringBuilder();
            var ChildNodeList = data.FindAll(t => t.parentId == parentId);
            if (ChildNodeList.Count > 0) { index++; }
            foreach (TreeGridModel entity in ChildNodeList)
            {
                string strJson = entity.entityJson;
                strJson = strJson.Insert(1, "\"loaded\":" + (entity.loaded == true ? false : true).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"expanded\":" + (entity.expanded).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"isLeaf\":" + (entity.isLeaf == true ? false : true).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"parent\":\"" + parentId + "\",");
                strJson = strJson.Insert(1, "\"level\":" + index + ",");
                sb.Append(strJson);
                sb.Append(TreeGridJson(data, index, entity.id));
            }
            return sb.ToString().Replace("}{", "},{");
        }




        public static string TreeSelectJson(this List<TreeSelectModel> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(TreeSelectJson(data, "0", ""));
            sb.Append("]");
            return sb.ToString();
        }
        private static string TreeSelectJson(List<TreeSelectModel> data, string parentId, string blank)
        {
            StringBuilder sb = new StringBuilder();
            var ChildNodeList = data.FindAll(t => t.parentId == parentId);
            var tabline = "";
            if (parentId != "0")
            {
                tabline = "　　";
            }
            if (ChildNodeList.Count > 0)
            {
                tabline = tabline + blank;
            }
            foreach (TreeSelectModel entity in ChildNodeList)
            {
                entity.text = tabline + entity.text;
                string strJson = entity.ToJson();
                sb.Append(strJson);
                sb.Append(TreeSelectJson(data, entity.id, tabline));
            }
            return sb.ToString().Replace("}{", "},{");
        }




    }



    public static class Expands_Json
    {
    /*
        public static dynamic GetValue(this Newtonsoft.Json.Linq.JObject jObj, params string[] keys)
        {
            try
            {
                return Ass.Data.Utils.GetJsonValue(jObj, keys);
            }
            catch { return ""; }
        }
        public static dynamic GetValueString(this Newtonsoft.Json.Linq.JObject jObj, params string[] keys)
        {
            try
            {
                return Ass.P.PStr(Ass.Data.Utils.GetJsonValue(jObj, keys));
            }
            catch { return ""; }
        }
        public static dynamic GetValue(this Newtonsoft.Json.Linq.JToken jObj, params string[] keys)
        {
            try
            {
                return Ass.Data.Utils.GetJsonValue(jObj, keys);
            }
            catch { return ""; }
        }

        public static dynamic GetValueString(this Newtonsoft.Json.Linq.JToken jObj, params string[] keys)
        {
            try
            {
                return Ass.P.PStr(Ass.Data.Utils.GetJsonValue(jObj, keys));
            }
            catch { return ""; }
        }
    */
        /// <summary>
        /// Object转Json字符串
        /// </summary>
        public static string JsonString(this object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }


    }


}
