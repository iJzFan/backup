using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ah.Models;
using ah;
using ah.DbContext;

namespace ahWeb.Api
{


    //业务单元的Api 
    public partial class Business : BaseDBController
    {
        public Business(AHMSEntitiesSqlServer db) : base(db) { }

        /// <summary>
        /// 添加一个挂号
        /// </summary>
        public vwCHIS_Register AddOneRegister(int custId, int departId, int employeeId, int stationId,
            DateTime reservationDate, int reservationSlot,
            RegistFrom regFrom, out bool haveRegisted,
            RegisterTreatType regTreatType = RegisterTreatType.Normal,
            string remark = "", int? opId = null, string opMan = null,string propName=null,string propValue=null
            )
        {
            if (opId == null)
            {
                opId = DbData.SystemMachineId;
                opMan = "SYSTEM";
            }

            haveRegisted = false;
            var find = MainDbContext.CHIS_Register.FirstOrDefault(m => m.EmployeeID == employeeId && m.RegisterSlot == reservationSlot
                                  && m.CustomerID == custId &&
                                  (reservationDate - m.RegisterDate.Value).Days == 0);

            long regid = 0;
            if (find == null)
            {
                var add = MainDbContext.Add(new CHIS_Register
                {
                    CustomerID = custId,
                    Department = departId,
                    EmployeeID = employeeId,
                    StationID = stationId,
                    RegisterDate = reservationDate.Date,
                    RegisterSlot = reservationSlot,
                    RegisterTreatType = (int)regTreatType,
                    RegisterFrom = (int)regFrom,
                    PropName=propName,
                    PropValue=propValue,
                    OpID = opId,
                    OpMan = opMan
                }).Entity;
                MainDbContext.SaveChanges();
                regid = add.RegisterID;
            }
            if (find != null) { haveRegisted = true; regid = find.RegisterID; }

            return MainDbContext.vwCHIS_Register.AsNoTracking().FirstOrDefault(m => m.RegisterID == regid);


        }



    }
}