using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Ass;
using Microsoft.Extensions.Configuration;
using CHIS.Models.DataModel;
using System.Collections.Generic;
using CHIS.Models;

namespace CHIS.DAL
{
    public class DrugsDAL : BaseDal
    {

        /// <summary>
        /// 获取最后入库的价格
        /// </summary>
        public async Task<DataTable> GetMyLastIncomePrice(IList<int> drugids, int stationId)
        {
            var sql = @"
 select m.DrugId,m.IncomePrice,m.InUnitId from (
 select
 row_number() over(partition by a.DrugId order by a.InTime desc ) as Num
 ,a.DrugId,a.IncomePrice,a.InUnitId
 from CHIS_DurgStock_Income a
 where a.StationId={0} and a.IncomePrice>0 and a.DrugId in({1})
 ) m where m.Num<3
";
            sql = string.Format(sql, stationId, string.Join(",", drugids));
            return (await base.QuerySqlAsync(sql)).Tables[0];
        }


    }
}
