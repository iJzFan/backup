using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ass.Data
{

    /// <summary>
    /// 自身项目的高级数据库工具
    /// </summary>
    public interface IDbUtils
    {
        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <returns></returns>
        object ExecuteStorageProgress(string spName, params object[] args);
        T ExecuteStorageProgress<T>(string spName, params object[] args);
         
    }


    public class SqlServerDbUtils : IDbUtils
    {
        public object ExecuteStorageProgress(string spName, params object[] args)
        {
            throw new NotImplementedException();
        }

        public T ExecuteStorageProgress<T>(string spName, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
    public class MySqlDbUtils : IDbUtils
    {
        public object ExecuteStorageProgress(string spName, params object[] args)
        {
            throw new NotImplementedException();
        }

        public T ExecuteStorageProgress<T>(string spName, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
