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

namespace CHIS.Services
{
    /// <summary>
    /// 处方类服务
    /// </summary>
    public class PrescriptionService : BaseService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db"></param>
        public PrescriptionService(CHISEntitiesSqlServer db) : base(db)
        {
            
        }




        /// <summary>
        /// 获取用户的处方信息
        /// </summary>
        /// <param name="searchText">输入条件 手机、邮箱、身份证、登录号、处方号、接诊号</param>
        /// <param name="dateTimeFrom">选填 开始时间</param>
        /// <param name="dateTimeEnd">选填 结束时间 间隔不超过3个月</param>
        /// <returns></returns>  
        public IEnumerable<PrescriptionSearchItem> GetPrescriptionOfCustomer(string searchText, DateTime? dateTimeFrom, DateTime? dateTimeEnd)
        {
            if (searchText.IsEmpty()) throw new Exception("必须输入搜索项");
            var t = searchText.GetStringType();
            if (t.IsGuid)
            {
                PrescriptionSearchItem rlt = null;
                long treatId = 0;
                var pno = Guid.Parse(t.String);
                var formed = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().FirstOrDefault(m => m.PrescriptionNo == pno);
                if (formed == null)
                {
                    var herb = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().FirstOrDefault(m => m.PrescriptionNo == pno);
                    if (herb == null) throw new Exception("查无此处方号");
                    rlt = new PrescriptionSearchItem();
                    rlt.PrescriptionTypeName = "中草药";
                    rlt.PrescriptionId = pno;
                    treatId = herb.TreatId;
                }
                else
                {
                    rlt = new PrescriptionSearchItem();
                    rlt.PrescriptionTypeName = "成药";
                    rlt.PrescriptionId = pno;
                    treatId = formed.TreatId;
                }
                var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
                rlt.TreatId = treat.TreatId;
                rlt.TreatTime = treat.FirstTreatTime.Value;
                rlt.CustomerName = treat.CustomerName;
                rlt.CustomerAge = treat.TreatCustomerAge?.ToString("#0.#");
                rlt.CustomerGender = treat.Gender?.ToGenderString();
                return new List<PrescriptionSearchItem>() { rlt };
            }
            else
            {
                var find = _db.CHIS_DoctorTreat.AsNoTracking().Join(_db.CHIS_Code_Customer, a => a.CustomerId, g => g.CustomerID, (a, g) =>
                new
                {
                    g.CustomerName,
                    a.CustomerId,
                    g.IDcard,
                    g.Email,
                    g.LoginName,
                    g.CustomerMobile,
                    a.TreatId,
                    a.TreatCustomerAge,
                    g.Gender,
                    a.FirstTreatTime
                });
                if (t.IsMobile) find = find.Where(m => m.CustomerMobile == t.String);
                else if (t.IsEmail) find = find.Where(m => m.Email == t.String);
                else if (t.IsIdCardNumber) find = find.Where(m => m.IDcard == t.String);
                else if (t.IsLoginNameLegal) find = find.Where(m => m.LoginName == t.String);
                else
                {
                    if (long.TryParse(t.String, out long treatId)) find = find.Where(m => m.TreatId == treatId);
                    else throw new Exception("非法输入");
                }

                if (!dateTimeEnd.HasValue) dateTimeEnd = DateTime.Today.AddDays(1);
                if (!dateTimeFrom.HasValue) dateTimeFrom = dateTimeEnd.Value.AddMonths(-3);
                if ((dateTimeEnd.Value - dateTimeFrom.Value).TotalDays > 93) throw new Exception("时间范围超出3个月");
                //限定时间范围
                find = find.Where(m => m.FirstTreatTime >= dateTimeFrom && m.FirstTreatTime < dateTimeEnd);

                //union 合并处方信息
                var pres = (from item in _db.CHIS_DoctorAdvice_Formed select new { item.PrescriptionNo, PrescriptionTypeName = "成药", item.TreatId })
                    .Union(from item in _db.CHIS_DoctorAdvice_Herbs select new { item.PrescriptionNo, PrescriptionTypeName = "中草药", item.TreatId });


                var mm = from item in find
                         join pre in pres on item.TreatId equals pre.TreatId
                         orderby item.FirstTreatTime descending
                         select new PrescriptionSearchItem
                         {
                             CustomerAge = (item.TreatCustomerAge ?? 0).ToString("#0.#"),
                             CustomerGender = (item.Gender ?? 2).ToGenderString(),
                             CustomerName = item.CustomerName,
                             TreatId = item.TreatId,
                             TreatTime = item.FirstTreatTime.Value,
                             PrescriptionId = pre.PrescriptionNo,
                             PrescriptionTypeName = pre.PrescriptionTypeName
                         };
                return mm;
            }


        }

        

    }
}
