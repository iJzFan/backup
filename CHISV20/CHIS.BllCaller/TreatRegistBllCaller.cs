using Ass;
using CHIS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHIS.BllCaller
{
    public class TreatRegistBllCaller : BaseBllCaller
    {
        public Ass.Mvc.PageListInfo<vwCHIS_Register> SearchRegistList(string searchText, DateTime dt0, DateTime dt1, int? stationId, int? departId, int? doctorId,int? registOpId, int pageIndex, int pageSize)
        {

            var db = CHISDbContext;
            var find = db.vwCHIS_Register.AsNoTracking().Where(m => m.RegisterDate >= dt0 && m.RegisterDate < dt1 && m.StationID == stationId);

            if (departId.HasValue) find = find.Where(m => m.Department == departId);
            if (registOpId.HasValue) find = find.Where(m => m.OpID == registOpId);
            if (doctorId.HasValue) find = find.Where(m => m.EmployeeID == doctorId);


            if (searchText.IsNotEmpty())
            {
                var t = searchText.GetStringType();
                if (t.IsMobile) find = find.Where(m => m.CustomerMobile == searchText);
                else if (t.IsEmail) find = find.Where(m => m.Email == searchText);
                else if (t.IsIdCardNumber) find = find.Where(m => m.IDcard == searchText);
                else find = find.Where(m => m.CustomerName == searchText);
            }
            var total = find.Count();
            var items = find.OrderByDescending(m => m.OpTime).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
            return new Ass.Mvc.PageListInfo<vwCHIS_Register>
            {
                DataList = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = total
            };
        }






    }
}
