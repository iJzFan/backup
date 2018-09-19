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
using Microsoft.Extensions.Caching.Memory;

namespace CHIS.Services
{
    /// <summary>
    /// 数据字典服务类
    /// </summary>
    public class DictService : BaseService
    {
        private IEnumerable<vwCHIS_Code_DictDetail> dictLib { get; set; }
        private IEnumerable<SYS_ChinaArea> areaLib { get; set; }
        IMemoryCache _memoryCache;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db"></param>
        public DictService(CHISEntitiesSqlServer db
            , IMemoryCache memoryCache) : base(db)
        {
            _memoryCache = memoryCache;
            dictLib = InitalDictLib();
            areaLib = InitalAreaLib();
        }

        private IEnumerable<SYS_ChinaArea> InitalAreaLib()
        {
            var cacheKey = "ChinaAreaDetails";
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<SYS_ChinaArea> result))
            {
                result = _db.SYS_ChinaArea.AsNoTracking().ToList();
                _memoryCache.Set(cacheKey, result);
                //设置相对过期时间260分钟
                _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(260)));
            }
            return result;
        }

        private IEnumerable<vwCHIS_Code_DictDetail> InitalDictLib()
        {
            //获取缓存数据
            var cacheKey = "DictDetails";
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<vwCHIS_Code_DictDetail> result))
            {
                result = _db.vwCHIS_Code_DictDetail.AsNoTracking().ToList();
                _memoryCache.Set(cacheKey, result);
                //设置相对过期时间260分钟
                _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(260)));
            }
            return result;
        }




        /// <summary>
        /// 查找字典的含义
        /// </summary>
        /// <param name="detailId"></param>
        /// <returns></returns>
        public string FindName(int? detailId)
        {
            try
            {
                return dictLib.Single(m => m.DetailID == detailId).ItemName;
            }
            catch { return ""; }
        }

        /// <summary>
        /// 根据值来查找字典数据，需要框定查询主类范围
        /// </summary>
        /// <param name="mainKey">主类key</param>
        /// <param name="val">值</param>
        /// <returns></returns>
        public string FindNameByValue(string mainKey, object val)
        {
            string v = $"{val}".Trim();
            return dictLib.Where(m => m.DictKey == mainKey).FirstOrDefault(m => m.ItemValue == v)?.ItemName;
        }

        /// <summary>
        /// 获取字典信息
        /// </summary>
        /// <param name="detailId"></param>
        /// <returns></returns>
        public vwCHIS_Code_DictDetail GetDictById(int? detailId)
        {
            var rtn = new vwCHIS_Code_DictDetail();
            try
            {
                if (detailId == null) throw new Exception("没有传入主Id");
                return dictLib.Single(m => m.DetailID == detailId);
            }
            catch (Exception ex) { return rtn; }
        }

        public IQueryable<CHIS_Code_Dict_Main> GetDictMain(int pageIndex = 1, int pageSize = 20)
        {
            return _db.CHIS_Code_Dict_Main.AsNoTracking().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IEnumerable<vwCHIS_Code_DictDetail> GetDictByMainKey(string mainKey, bool isEnable = true)
        {
            if (mainKey.IsEmpty()) mainKey = "NOTHISKEY";
            return dictLib.Where(m => m.DictKey == mainKey && m.IsEnable == isEnable).OrderBy(m => m.ShowOrder);
        }


        /// <summary>
        /// 获取地址信息
        /// </summary>
        /// <param name="areaId">地址Id</param>
        /// <returns></returns>
        public SYS_ChinaArea GetAreaById(int areaId)
        {
            return areaLib.FirstOrDefault(m => m.AreaId == areaId);
        }
        /// <summary>
        /// 获取子地址
        /// </summary>  
        public IEnumerable<SYS_ChinaArea> GetSubArea(int parentAreaId)
        {
            return areaLib.Where(m => m.ParentAreaId == parentAreaId);
        }

    }
}
