using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Ass;
using Microsoft.Extensions.Configuration;

namespace CHIS.DAL
{
    public class Staticstics_Finatial : BaseDal
    {

        /// <summary>
        /// 工作站的财务报表 明细
        /// </summary>
        /// <param name="searchText">医生姓名或者手机</param>
        /// <param name="stationId"></param>
        /// <param name="sTime"></param>
        /// <param name="eTime"></param>
        /// <returns></returns>
        public async Task<DataSet> FinanceStatementAsync(string searchText, int stationId, DateTime sTime, DateTime eTime, int pageIndex, int pageSize)
        {
            string starTime = sTime.ToString("yyyy-MM-dd");
            string endTime = eTime.ToString("yyyy-MM-dd");
            string sql = @"select  a.FeeTypeCode, a.CustomerName, a.Gender,a.DoctorName,a.TotalAmount,a.PayedTime,a.StationName,a.sysOpMan 
            from [CHISV20].[dbo].[vwCHIS_Charge_Pay] as a  
            where a.PayedTime>='{1}' and a.PayedTime<'{2}' and dbo.fn_InStation({0},a.StationId)=1 ";
            sql = string.Format(sql, stationId, starTime, endTime);
            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) sql += $" and a.DoctorMobile='{t.String}'";
                else sql += $" and a.DoctorName='{t.String}' ";
            }
            // return await QuerySqlAsync(sql);
            var order = " Order By a.PayedTime DESC ";
            if (pageIndex == 0 || pageSize == 0) return await QuerySqlAsync(sql + order);
            return await QueryPageSql(sql, order, pageIndex, pageSize);      
        }

        public async Task<decimal> PayTotalAmountAsync(string searchText, int stationId, DateTime sTime, DateTime eTime)
        {
            string starTime = sTime.ToString("yyyy-MM-dd");
            string endTime = eTime.ToString("yyyy-MM-dd");
            string sql = "select sum(a.TotalAmount) as'totalAmount' from [CHISV20].[dbo].[vwCHIS_Charge_Pay] as a  where a.PayedTime>='{1}' and a.PayedTime<'{2}' and dbo.fn_InStation({0},a.StationId)=1";
            sql = string.Format(sql, stationId, starTime, endTime);
            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) sql += $" and a.DoctorMobile='{t.String}'";
                else sql += $" and a.DoctorName='{t.String}' ";
            }
            return Ass.P.PDecimalV(await ExecuteScalarAsync(sql));
        }




        public async Task<DataSet> NetWebOfFinanceStatementAsync(string searchText, int stationId, int suplierId, DateTime sTime, DateTime eTime, int pageIndex = 1, int pageSize = 20)
        {
            string starTime = sTime.ToString("yyyy-MM-dd");
            string endTime = eTime.ToString("yyyy-MM-dd");
            int sendedStatus = 1;
            string sql = @"
    select b.totalAmount,b.StationId,b.SendTime,b.SupplierId,d.DoctorName,d.CustomerMobile as DoctorMobile
	,c.CustomerName,c.Gender ,c.CustomerMobile,w.StationName,b.NetOrderNO,b.SendOrderId
	from [CHIS_Shipping_NetOrder] b
	left join CHIS_DoctorTreat t on t.TreatId=b.TreatId
	left join CHIS_Code_WorkStation w on w.StationID=b.StationId
	left join vwCHIS_Code_Doctor d on d.DoctorId=t.DoctorId
	left join CHIS_Code_Customer c on c.CustomerID=b.CustomerId
	where b.SendedStatus=1 and b.SupplierId={1} and b.SendTime>='{2}' and b.SendTime<'{3}' and b.SendedStatus={4}  and dbo.fn_InStation({0},b.StationId)=1
";
            sql = string.Format(sql, stationId, suplierId, starTime, endTime,sendedStatus);

            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) sql += $" and b.DoctorMobile='{t.String}'";
                else sql += $" and b.DoctorName='{t.String}' ";
            }
            // return await QuerySqlAsync(sql);
            var order = " Order By b.SendTime DESC ";
            if (pageIndex == 0||pageSize==0) return await QuerySqlAsync(sql + order);
            return await QueryPageSql(sql, order, pageIndex, pageSize);
        }

        public async Task<decimal> NetWebOfPayTotalAmountAsync(string searchText, int stationId, int suplierId, DateTime sTime, DateTime eTime)
        {
            string starTime = sTime.ToString("yyyy-MM-dd");
            string endTime = eTime.ToString("yyyy-MM-dd");
            string sql = @"
	select sum(b.TotalAmount) as'totalAmount'
	from [CHIS_Shipping_NetOrder] b
	left join CHIS_DoctorTreat t on t.TreatId=b.TreatId
	left join vwCHIS_Code_Doctor d on d.DoctorId=t.DoctorId
	left join CHIS_Code_Customer c on c.CustomerID=b.CustomerId
	where b.SendedStatus=1 and b.SupplierId={1} and b.SendTime>='{2}' and b.SendTime<'{3}' and dbo.fn_InStation({0},b.StationId)=1
";
            sql = string.Format(sql, stationId, suplierId, starTime, endTime);
            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) sql += $" and b.DoctorMobile='{t.String}'";
                else sql += $" and b.DoctorName='{t.String}' ";
            }
            return Ass.P.PDecimalV(await ExecuteScalarAsync(sql));
        }




    }
}
