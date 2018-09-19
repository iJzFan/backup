using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.Utility
{
    public class DataBaseHelper
    {


        /// <summary>
        /// 获取主数据库
        /// </summary>
        /// <returns></returns>
        public CHIS.DbContext.CHISEntitiesSqlServer GetMainDbContext()
        {
            return DbContextSqlServer;
        }
        public CHIS.DbContext.CHISLogEntitiesSqlServer GetLogDbContext()
        {
            return LogDbContextSqlServer;
        }


        public string ConnectionString
        {
            get
            {
                string connStr = "";
                if (Global.Config == null) connStr = "Data Source=dg.tsjk365.com;Initial Catalog=CHISV20;User ID=chisadmin;Password=123.abc;";
                else connStr = Global.Config.GetSection("ConnectionStrings:SqlConnection").Value;// "Data Source=192.168.99.251;Initial Catalog=CHIS;User ID=chisadmin;Password=123.abc;";
                return connStr;
            }
        }
        public string LogConnectionString
        {
            get
            {
                string connStr = "";
                if (Global.Config == null) connStr = "Data Source=dg.tsjk365.com;Initial Catalog=CHISV20Logs;User ID=chisadmin;Password=123.abc;";
                else connStr = Global.Config.GetSection("ConnectionStrings:LogSqlConnection").Value;// "Data Source=192.168.99.251;Initial Catalog=CHIS;User ID=chisadmin;Password=123.abc;";
                return connStr;
            }
        }

        #region DbContext 数据库     

        CHIS.DbContext.CHISEntitiesSqlServer _dbContextSqlServer = null;
        CHIS.DbContext.CHISLogEntitiesSqlServer _dbLogContextSqlServer = null;
        CHIS.DbContext.CHISEntitiesMySql _dbContextMySql = null;
        private CHIS.DbContext.CHISEntitiesSqlServer DbContextSqlServer
        {
            get
            {           
                return _dbContextSqlServer ?? (_dbContextSqlServer = new DbContext.CHISEntitiesSqlServer(ConnectionString));
            }
        }
        private CHIS.DbContext.CHISLogEntitiesSqlServer LogDbContextSqlServer
        {
            get
            {
                return _dbLogContextSqlServer ?? (_dbLogContextSqlServer = new DbContext.CHISLogEntitiesSqlServer(LogConnectionString));
            }
        }

        private CHIS.DbContext.CHISEntitiesMySql DbContextMySql
        {
            get
            {
                return _dbContextMySql ?? (_dbContextMySql = new DbContext.CHISEntitiesMySql(Global.Config.GetSection("ConnectionStrings:MySqlConn").Value));

            }
        }



        #endregion      

    }
}
