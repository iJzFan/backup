using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Ass;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Collections.Generic;
using System.Reflection;

namespace CHIS.DAL
{
    public class BaseDal
    {

        public static IConfiguration Configuration { get; set; }
        static BaseDal()
        {
            //ReloadOnChange = true 当appsettings.json被修改时重新加载            
            Configuration = new ConfigurationBuilder()
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }



        internal string _connstr = null;
        public BaseDal(string connstr)
        {
            this._connstr = connstr;
        }
        public BaseDal()
        {
            this._connstr = BaseDal.Configuration.GetConnectionString("SqlConnection");//获取默认的连接数据库字符串
        }


        public async Task<DataSet> QueryPageSql(string sql,string sqlOrder, int pageIndex, int pageSize, string sql2 = "")
        {            
            DataSet ds = new DataSet();
            using (var conn = new SqlConnection(_connstr))
            {
                await conn.OpenAsync();
                var s = string.Format("{0} {1}  OFFSET {2} ROWS FETCH NEXT {3} ROWS ONLY ;", sql,sqlOrder,(pageIndex-1)*pageSize,pageSize);               
                s += string.Format("select {0} PageIndex,{1} PageSize,count(1) RecordTotal from ({2}) countTable;", pageIndex,pageSize,sql);
                s += sql2;
                SqlDataAdapter da = new SqlDataAdapter(s, conn);
                da.Fill(ds);
            }
            return ds;
        }
        
        public async Task<DataSet> QuerySqlAsync(string sql)
        {
            DataSet ds = null;
            using (var conn = new SqlConnection(_connstr))
            {
                await conn.OpenAsync();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                ds = new DataSet();
                da.Fill(ds);             
            }
            return ds;
        }
        public async Task<object> ExecuteScalarAsync(string sql)
        {
            object rtn = null;
            using (var conn = new SqlConnection(_connstr))
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(sql, conn);
                rtn = await cmd.ExecuteScalarAsync();
        
                

            }
            return rtn;
        }






        public string pDateTime(DateTime dt)
        {
            return dt.ToStdString();
        }
        public string pDate(DateTime dt)
        {
            return dt.ToDateString();
        }


        /// <summary>
        /// 转换成List
        /// 利用反射和泛型
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<T> ConvertToList<T>(DataTable dt) where T: class,new()
        {

            // 定义集合
            List<T> ts = new List<T>();

            // 获得此模型的类型
            Type type = typeof(T);
            //定义一个临时变量
            string tempName = string.Empty;
            //遍历DataTable中所有的数据行
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性
                PropertyInfo[] propertys = t.GetType().GetProperties();
                //遍历该对象的所有属性
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;//将属性名称赋值给临时变量
                    //检查DataTable是否包含此列（列名==对象的属性名）  
                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter
                        if (!pi.CanWrite) continue;//该属性不可写，直接跳出
                        //取值
                        object value = dr[tempName];
                        //如果非空，则赋给对象的属性
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                //对象添加到泛型集合中
                ts.Add(t);
            }

            return ts;

        }

    }


}
