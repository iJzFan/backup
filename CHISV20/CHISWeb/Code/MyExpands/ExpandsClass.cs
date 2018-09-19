using CHIS.Controllers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CHIS
{
    public static class ExpandsClass
    {

        #region 调用Sql自定义函数
        /// <summary> 
        /// 运行自定义函数
        /// 引用下列的命名空间
        ///  1. using System.Data.SqlClient;
        ///  2. using Ass;
        /// </summary>      
        /// <param name="funcName">函数名</param>
        /// <param name="sqlParms">参数</param>
        /// <returns></returns>
        public static object MySqlFunction(this CHIS.DbContext.CHISEntitiesSqlServer db, string funcName,
            params System.Data.SqlClient.SqlParameter[] sqlParms)
        {

            string connectionString = new Code.Utility.DataBaseHelper().ConnectionString;
            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                string strSql = funcName; //自定SQL函数
                var cmd = new System.Data.SqlClient.SqlCommand(strSql, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (sqlParms != null)
                {
                    foreach (var p in sqlParms)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
                System.Data.SqlClient.SqlParameter rtnp = new System.Data.SqlClient.SqlParameter();
                rtnp.ParameterName = "@Return";
                rtnp.Direction = System.Data.ParameterDirection.ReturnValue;
                cmd.Parameters.Add(rtnp);

                conn.Open();
                object o = cmd.ExecuteScalar();

                return rtnp.Value;

            }

        }

        /// <summary>
        /// 执行存储过程返回表单
        /// </summary>  
        public static IList<T> SqlQuery<T>(this CHIS.DbContext.CHISEntitiesSqlServer db, string sql, params object[] parameters)
            where T : new()
        {
            //注意：不要对GetDbConnection获取到的conn进行using或者调用Dispose，否则DbContext后续不能再进行使用了，会抛异常 
            string connectionString = new Code.Utility.DataBaseHelper().ConnectionString;
            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddRange(parameters);
                    var propts = typeof(T).GetProperties();
                    var rtnList = new List<T>();
                    T model; object val;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model = new T();
                            foreach (var l in propts)
                            {
                                try
                                {
                                    val = reader[l.Name];
                                    if (val == DBNull.Value)
                                    {
                                        l.SetValue(model, null);
                                    }
                                    else
                                    {
                                        l.SetValue(model, val);
                                    }
                                }
                                catch { }
                            }
                            rtnList.Add(model);
                        }
                    }
                    return rtnList;
                }
            }
        }


        /// <summary>
        /// 添加并保存一个实体
        /// </summary>
        public static T AddSave<T>(this CHIS.DbContext.CHISEntitiesSqlServer db, T entity) where T : class
        {
            var add = db.Add<T>(entity);
            db.SaveChanges();
            return add.Entity;
        }

        /// <summary>
        /// 添加或者更新一个实体
        /// </summary>
        public static T Upsert<T>(this CHIS.DbContext.CHISEntitiesSqlServer db, T entity) where T : class
        {
            try { var r = db.Update(entity).Entity; db.SaveChanges(); return r; }
            catch { return db.AddSave(entity); }
        }
 



        ///// <summary>
        ///// Ah自写扩展方法， 删除一个实体 考虑到后面删除的统一性
        ///// </summary> 
        //public static Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> RemoveAh<T>(this Microsoft.EntityFrameworkCore.DbContext db, T entity) where T : class
        //{ 
        //    return db.Remove<T>(entity);
        //}
        //public static void RemoveRangeAh<T>(this Microsoft.EntityFrameworkCore.DbContext db, params T[] entities) where T:class
        //{            
        //    foreach (var item in entities.ToArray())
        //    {
        //        db.Remove<T>(item);
        //    }
        //    // db.RemoveRange(entities);
        //}
        #region 分页查询
        /// <summary>
        /// 获取分页后的集合
        /// </summary> 
        /// <param name="findtotal">输出查询的总数</param>
        /// <param name="totalPage">输出总页数</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        /// <param name="sort">排序</param>
        /// <param name="wherePredicate">条件Lambda</param>
        /// <param name="orderPredicate">排序Lambda</param>
        /// <returns></returns>
        public static IList<T> GetPagedList<T, Torder>(this CHIS.DbContext.CHISEntitiesSqlServer db, out int findtotal, out int totalPage,
            int pageIndex, int pageSize,
            System.Linq.Expressions.Expression<Func<T, bool>> wherePredicate,
            System.Linq.Expressions.Expression<Func<T, Torder>> orderPredicate) where T : class
        {
            var findlist = db.Set<T>().Where(wherePredicate);
            findtotal = findlist.Count();
            totalPage = (int)Math.Ceiling(findtotal * 1.0f / pageSize);
            //排序获取当前页的数据  
            var dataList = findlist.OrderBy(orderPredicate).
                      Skip(pageSize * (pageIndex - 1)).
                      Take(pageSize).AsQueryable().ToList();
            return dataList;
        }

        #endregion

        /// <summary>
        /// 在工作站权限内
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> InStation<TEntity>([NotNullAttribute] this IQueryable<TEntity> source, int rootStationId, string fieldName = "StationId", string tableName = "") where TEntity : class
        {
            if (string.IsNullOrWhiteSpace(tableName)) tableName = typeof(TEntity).Name;
            return source.FromSql<TEntity>(string.Format("select * from {0} where dbo.fn_InStation({1},{2})=1", tableName, rootStationId, fieldName));
        }




        public static decimal ToAge(this DateTime dt)
        {
            return  (decimal)((DateTime.Now - dt).TotalDays) / 365.25m;
        }



        public static bool ContainsIgnoreCase(this string str, string s)
        {
            return str.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string Trimy(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            return str.Trim();
        }

        public static string GetSubString(this string str, int length)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            str = str.Trim();
            if (str.Length <= length) return str;
            else return str.Substring(0, length);
        }


        #endregion
        #region
        /// <summary>
        /// 简单验证是否是手机号
        /// </summary>
        public static bool IsMobileNumber(this string s)
        {
            s = Ass.P.PStr(s);
            if (string.IsNullOrEmpty(s)) return false;
            return s.Length == 11;
        }
        #endregion


        #region 前端页面生成控制 TagHelper
        public static Microsoft.AspNetCore.Html.IHtmlContentBuilder AppendHtml(this Microsoft.AspNetCore.Html.IHtmlContentBuilder builder, string format, params object[] args)
        {
            return builder.AppendHtml(string.Format(format, args));
        }


        #endregion

        #region RazorPage
        /// <summary>
        /// 获取登录后的用户数据，不用启动数据库操作
        /// </summary>
        public static Models.UserSelf GetUserSelf(this Microsoft.AspNetCore.Mvc.Razor.RazorPageBase razorPage)
        {
            if (razorPage.ViewBag.UserSelf != null) return razorPage.ViewBag.UserSelf as Models.UserSelf;
            else return null;
        }

        /// <summary>
        /// 获取用户管理器，可能会启动数据库的操作
        /// </summary>
        public static Code.Managers.UserFrameManager GetUserMgr(this Microsoft.AspNetCore.Mvc.Razor.RazorPageBase razorPage)
        {
            if (razorPage.ViewBag.UserSelf != null)
            {
                var userSelf = razorPage.ViewBag.UserSelf as Models.UserSelf;
                return new Code.Managers.UserFrameManager(userSelf);
            }
            else return null;
        }

        #endregion

        #region WebViewPage

        public static Boolean DEBUG(this Microsoft.AspNetCore.Mvc.Razor.RazorPageBase page)
        {            
            var value = false;
#if DEBUG
            value = true;
#endif
            return value;
        }

        #endregion

    }
}
