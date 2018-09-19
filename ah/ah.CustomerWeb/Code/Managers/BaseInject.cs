using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ah.Code.Managers
{
    public class BaseInject
    {

        #region DbContext 数据库   

        public ah.DbContext.AHMSEntitiesSqlServer MainDbContext
        {
            get
            {
                return DbContextSqlServer;
            }
        }

        ah.DbContext.AHMSEntitiesSqlServer _dbContextSqlServer = null;
        ah.DbContext.CHISEntitiesMySql _dbContextMySql = null;
        public ah.DbContext.AHMSEntitiesSqlServer DbContextSqlServer
        {
            get
            {

                string connStr = Global.Config.GetSection("ConnectionStrings:SqlConnection").Value;// "Data Source=192.168.99.251;Initial Catalog=CHIS;User ID=chisadmin;Password=123.abc;";
                return _dbContextSqlServer ?? (_dbContextSqlServer = new DbContext.AHMSEntitiesSqlServer(connStr));
            }
        }

        public ah.DbContext.CHISEntitiesMySql DbContextMySql
        {
            get
            {
                return _dbContextMySql ?? (_dbContextMySql = new DbContext.CHISEntitiesMySql(Global.Config.GetSection("ConnectionStrings:MySqlConn").Value));

            }
        }

        #endregion


    }
}
