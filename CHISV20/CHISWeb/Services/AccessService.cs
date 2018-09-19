using CHIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ass;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using CHIS.Models.ViewModel;
using CHIS.DbContext;
using CHIS.Models.StatisticsModels;
using CHIS.Models.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace CHIS.Services
{
    /// <summary>
    /// 权限服务
    /// </summary>
    public class AccessService : BaseService
    {
        IHttpContextAccessor _httpContextAccessor;
        IMemoryCache _memoryCache;
        /// <summary>
        /// 构造函数
        /// </summary>
        public AccessService(DbContext.CHISEntitiesSqlServer db
              , IMemoryCache memoryCache
            , IHttpContextAccessor httpContextAccessor) : base(db)
        {
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
        }

        //===============================================================================



        #region 人员与权限信息
        Code.Managers.UserFrameManager _userMgr; Models.UserSelf _logindata = null;
        /// <summary>
        /// 用户框架管理器
        /// </summary>
        public Code.Managers.UserFrameManager UserMgr
        {

            get
            {
                return _userMgr ?? (_userMgr = new Code.Managers.UserFrameManager(this.UserSelf));
            }
        }

        /// <summary>
        /// 登录的用户信息
        /// </summary>
        public Models.UserSelf UserSelf
        {
            get
            {
                return _logindata ?? (_logindata = new DtUtil.UserSelfUtil(_httpContextAccessor.HttpContext).GetUserSelf());
            }
        }

        #endregion


        /// <summary>
        /// 获取操作Id和操作人
        /// </summary>
        /// <param name="bException"></param>
        /// <returns></returns>
        public (int, string) GetOpIdAndOpMan(bool bException = false)
        {
            string opman = "System"; int opid = 0;
            try
            {
                opman = UserSelf.OpManFullMsg; opid = UserSelf.OpId;
            }
            catch (Exception ex)
            {
                if (bException) throw ex;
            }
            return (opid, opman);
        }



        private ObjReturn GetFuncDetailSettedValue(string functionKey, string key, int doctorId, int stationId)
        {
            //获取缓存数据
            var cacheKey = "MEM_FuncDetails";
            var cacheDictKey = $"FUNCDETAIL_{functionKey}_{key}_{doctorId}_{stationId}";

            if (!_memoryCache.TryGetValue(cacheKey, out Dictionary<string, CHIS.Models.DataModel.typevalue> result))
            {
                result = new Dictionary<string, CHIS.Models.DataModel.typevalue>();
                _memoryCache.Set(cacheKey, result);
                //设置相对过期时间60分钟
                _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(260)));
            }
            CHIS.Models.DataModel.typevalue rtn = null;
            if (!result.ContainsKey(cacheDictKey))
            {
                string sql = string.Format("exec sp_Sys_GetFuncDetailAccess '{0}','{1}',{2},{3}", functionKey, key, doctorId, stationId);
                rtn = _db.SqlQuery<CHIS.Models.DataModel.typevalue>(sql).First();
                result.Add(cacheDictKey, rtn);
            }
            if (rtn == null) rtn = result[cacheDictKey];
            return new Ass.ObjReturn(rtn.value, rtn.type);
        }

        /// <summary>
        /// 获取我的功能配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public ObjReturn GetFuncConfig(string configName, int? doctorId = null, int? stationId = null)
        {
            ObjReturn rlt = null;
            var typeName = "string";
            if (!doctorId.HasValue) doctorId = UserSelf.DoctorId;
            if (!stationId.HasValue) stationId = UserSelf.StationId;
            string[] fn = configName.Split('|');
            if (fn[0] == "FUNCDETAIL")
            {
                typeName = fn[1];
                var functionKey = fn[2];
                var key = fn[4];
                rlt = GetFuncDetailSettedValue(functionKey, key, UserSelf.DoctorId, UserSelf.StationId);
            }
            return rlt;
        }
        /// <summary>
        /// 获取我的配置
        /// </summary>
        /// <param name="myConfigSectionKey"></param>
        /// <param name="doctorId"></param>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public string GetMyConfig(string myConfigSectionKey, int? doctorId = null, int? stationId = null,string nullDefVal="")
        {
            if (!doctorId.HasValue) doctorId = UserSelf.DoctorId;
            if (!stationId.HasValue) stationId = UserSelf.StationId;
            var rlt= _db.CHIS_Sys_MyConfig.AsNoTracking().Where(m => m.DoctorId == doctorId.Value && m.StationId == stationId.Value)
                 .Where(m => m.SectionKey == myConfigSectionKey).FirstOrDefault()?.SectionValue;

            return rlt.IsEmpty() ? nullDefVal : rlt;  
        }




    }
}
