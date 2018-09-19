using ah.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ah.Code
{
    public static class ControllerExtension
    {
        /// <summary>
        /// 返回jsonp格式,用于跨域
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="data"></param>
        /// <param name="serializerSettings"></param>
        /// <returns></returns>
        public static JsonpResult Jsonp(this Controller controller, object data, JsonSerializerSettings serializerSettings = null)
        {
            return serializerSettings == null ? new JsonpResult(data) : new JsonpResult(data, serializerSettings);
        }
    }

    public class JsonpResult : JsonResult
    {
        public static readonly string JsonpCallbackName = "callback";
        public static readonly string CallbackApplicationType = "application/json";

        public JsonpResult(object value) : base(value) { }
        public JsonpResult(object value, JsonSerializerSettings serializerSettings) : base(value, serializerSettings) { }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            return base.ExecuteResultAsync(context);
        }

        public override void ExecuteResult(ActionContext context)
        {
            base.ExecuteResult(context);
        }
    }


}
