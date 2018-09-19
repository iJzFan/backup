using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ah
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
        public static object MySqlFunction(this ah.DbContext.AHMSEntitiesSqlServer db, string funcName,
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
        public static IList<T> SqlQuery<T>(this ah.DbContext.AHMSEntitiesSqlServer db, string sql, params object[] parameters)
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
                            model = new T(); foreach (var l in propts)
                            {
                                val = reader[l.Name]; if (val == DBNull.Value)
                                {
                                    l.SetValue(model, null);
                                }
                                else
                                {
                                    l.SetValue(model, val);
                                }
                            }
                            rtnList.Add(model);
                        }
                    }
                    return rtnList;
                }
            }
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








        #endregion
        #region
        /// <summary>
        /// 简单验证是否是手机号
        /// </summary>
        public static bool IsMobileNumber(this string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            return s.Length == 11;
        }
        #endregion

    }
}
