using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS
{

    /// <summary>
    /// 功能允许的过滤器
    /// </summary>
    public class FuncAccessFilterAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// 允许名称
        /// </summary>
        public string FuncAccessKey { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            
            bool bPass = false;
            var model = Global.db_FuncAccess.FirstOrDefault(m => m.AKeyId == FuncAccessKey);
            if (model != null)
            {
                var doctorId = int.Parse(context.HttpContext.User.FindFirst("DoctorId").Value);
                var stationId = int.Parse(context.HttpContext.User.FindFirst("StationId").Value);
                string sql = string.Format("exec sp_Sys_GetFuncDetailAccess '{0}','{1}',{2},{3}", model.ActionKey, model.FuncName, doctorId, stationId);
                var db = new Code.Utility.DataBaseHelper().GetMainDbContext();
                CHIS.Models.DataModel.typevalue rtn = db.SqlQuery<CHIS.Models.DataModel.typevalue>(sql).First();
                if (model.ResultType == "bool")
                {
                    bPass = new Ass.ObjReturn(rtn.value, rtn.type).ToBool();
                    //通过则不予理睬
                }               
            }

            //未通过返回页面
            if (!bPass) context.Result = new RedirectResult("/Home/Unaccess");//转到未被授权的页面

        }

         
    }
}
