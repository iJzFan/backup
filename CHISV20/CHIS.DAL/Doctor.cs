using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Ass;
using Microsoft.Extensions.Configuration;

namespace CHIS.DAL
{
    public class Doctor : BaseDal
    {


        public async Task<DataSet> LoadDoctorListAsync(string searchText, int? stationId, DateTime dt0, DateTime dt1, bool? isVed = null, bool bTimeRange = true, int pageIndex = 1, int pageSize = 20)
        {
            var tbname = nameof(CHIS.Models.vwCHIS_Code_Doctor);
            var et = new Models.vwCHIS_Code_Doctor();

            StringBuilder sb = new StringBuilder(), w = new StringBuilder(), r = new StringBuilder();
            sb.AppendFormat("select m.* from {0} m Where 1=1", tbname);
            if (isVed.HasValue)
                w.AppendFormat(" AND m.{0}={1} ", nameof(et.DoctorIsAuthenticated), isVed == true ? 1 : 0);
            if (bTimeRange)
                w.AppendFormat(" AND (m.{0}>='{1}' AND m.{0}<'{2}')", nameof(et.DoctorCreateTime), pDateTime(dt0), pDateTime(dt1));

            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) w.AppendFormat(" AND (m.{0}='{1}') ", nameof(et.CustomerMobile), t.String);
                else if (t.IsEmail) w.AppendFormat(" AND (m.{0}='{1}') ", nameof(et.Email), t.String);
                else if (t.IsIdCardNumber) w.AppendFormat(" AND (m.{0}='{1}') ", nameof(et.IDcard), t.String);
                else if (t.IsLoginNameLegal) w.AppendFormat(" AND (m.{0}='{1}') ", nameof(et.LoginName), t.String);
                else w.AppendFormat(" AND (m.{0}='{1}') ", nameof(et.DoctorName), t.String);
            }

            w.AppendFormat(" AND exists(select 1 from  CHIS_Sys_Rel_DoctorStations where StationId={0} and DoctorId=m.DoctorId)",stationId);


            r.AppendFormat(" ORDER BY m0.{0} DESC", nameof(et.DoctorId));
            var sql0 = sb.ToString() + w.ToString();
            var sql = string.Format("select m0.*,l.IsLock,l.LockTime,l.WhyLock from ({0}) m0 left join CHIS_Sys_Login l on l.DoctorId=m0.DoctorId", sql0);
            
          
            return await QueryPageSql(sql, r.ToString(), pageIndex, pageSize);
        }


    }
}
