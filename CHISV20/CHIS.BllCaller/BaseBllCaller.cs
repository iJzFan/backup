using CHIS.DbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;

namespace CHIS.BllCaller
{
    /// <summary>
    /// 基础Bll调用
    /// </summary>
    public class BaseBllCaller
    {

        public static Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }
        static BaseBllCaller()
        {
            //ReloadOnChange = true 当appsettings.json被修改时重新加载            
            Configuration = new ConfigurationBuilder()
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }
        internal string _connstr = null;
        public BaseBllCaller(string connstr)
        {
            this._connstr = connstr;
        }
        public BaseBllCaller()
        {
            this._connstr = BaseBllCaller.Configuration.GetConnectionString("SqlConnection");//获取默认的连接数据库字符串
        }






        CHISEntitiesSqlServer _dbcontext = null;
        /// <summary>
        /// 主数据上下文
        /// </summary>
        public CHISEntitiesSqlServer CHISDbContext
        {
            get
            {
                return _dbcontext??(_dbcontext= new CHISEntitiesSqlServer(_connstr));
            }
        }
    }
}
