using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Ass;

namespace CHIS.DAL
{
    public class Dispensing : BaseDal
    {
        public Dispensing() { }
        public Dispensing(string connstr) : base(connstr)
        {

        }


        public async Task<DataSet> LoadNetOrderList(string searchText, DateTime dt0, DateTime dt1, string contactName, int supplierId, int sendedStatus,int stationId, int pageIndex, int pageSize)
        {
            var tbname = nameof(CHIS.Models.vwCHIS_Shipping_NetOrder);
            var et = new Models.vwCHIS_Shipping_NetOrder();

            StringBuilder sb = new StringBuilder(), w = new StringBuilder(), r = new StringBuilder();
            sb.AppendFormat("select * from {0} m Where {1}={2}", tbname,nameof(et.StationId),stationId);
            w.AppendFormat(" AND m.{0}={1} ", nameof(et.SendedStatus), sendedStatus);
            w.AppendFormat(" AND (m.{0}>='{1}' AND m.{0}<'{2}')", nameof(et.SendTime), pDateTime(dt0), pDateTime(dt1));
            w.AppendFormat(" AND (m.{0}={1}) ", nameof(et.SupplierId), supplierId);
            if (contactName.IsNotEmpty())
            {
                if (contactName.GetStringType().IsMobile) w.AppendFormat(" AND (m.{0}='{1}') ", nameof(et.Mobile), contactName);
                else w.AppendFormat(" AND (m.{0}='{1}' ) ", nameof(et.ContactName), contactName);
            }

            if (searchText.IsNotEmpty())
            {
                if (searchText.GetStringType().IsMobile) w.AppendFormat(" AND (m.{0}='{1}') ", nameof(et.CustomerMobile), searchText);
                else w.AppendFormat(" AND (m.{1}='{0}' or m.{2}='{0}') ", searchText, nameof(et.CustomerName), nameof(et.NetOrderNO));
            }

            r.AppendFormat(" ORDER BY m.{0} DESC", nameof(et.SendTime));

            var sql = sb.ToString() + w.ToString();

            return await QueryPageSql(sql, r.ToString(), pageIndex, pageSize);
        }


    }
}
