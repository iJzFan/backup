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
    public class ReservationService : BaseService
    {
        NotifyService _notifySvr;
        public ReservationService(CHISEntitiesSqlServer db
            , NotifyService notifySvr
            ) : base(db)
        {
            this._notifySvr = notifySvr;
        }



        /// <summary>
        /// 添加一个挂号
        /// </summary>
        public async Task<Models.CHIS_Register> AddOneRegister(int custId, int departId, int doctorId, int stationId,
            DateTime reservationDate, int reservationSlot,
            RegistFrom regFrom,
            RegisterTreatType regTreatType = RegisterTreatType.Normal,
            string remark = "", bool useSysUser = false,
            int? opId = 0, string opMan = "",
            int? rxDoctorId = null
            )
        {
            //var u = controller.GetUserLoginData(true);
            bool haveRegisted = false;
            var find = _db.CHIS_Register.FirstOrDefault(m => m.EmployeeID == doctorId && m.RegisterSlot == reservationSlot
                                  && m.CustomerID == custId &&
                                  (reservationDate - m.RegisterDate.Value).Days == 0);

            //如果是药店则可以多次预约            
            var station = _db.CHIS_Code_WorkStation.FirstOrDefault(m => m.StationID == stationId);
            if (station == null) throw new Exception("没有工作站");
            if (station.StationTypeId == DictValues.StationType.k_StationType_DrugStore ||
                station.StationTypeId == DictValues.StationType.k_StationType_drugstoreS ||
                station.StationTypeId == DictValues.StationType.k_StationType_drugstore2)
            {
                //3分钟内不可重复预约
                if (find != null)
                {
                    if ((_db.GetDBTime() - find.OpTime.Value).TotalMinutes < 3)
                    {
                        throw new Exception("3分钟内不可重复预约！");
                    }
                }
            }
            else
            {
                if (find != null) { haveRegisted = true; return find; }
            }

            var et = new Models.CHIS_Register
            {
                CustomerID = custId,
                Department = departId,
                EmployeeID = doctorId,
                RxDoctorId = rxDoctorId,
                StationID = stationId,
                RegisterDate = reservationDate.Date,
                RegisterSlot = reservationSlot,
                RegisterTreatType = (int)regTreatType,
                RegisterFrom = (int)regFrom,
                OpID = opId,
                OpMan = opMan,
                IsEnable = true,
                OpTime = DateTime.Now,
                IsAppointment = (reservationDate.Date - DateTime.Today).Days >= 1,
                TreatStatus = "waiting",
                RegisterSeq = (_db.CHIS_Register.AsNoTracking().Where(m => m.StationID == stationId && m.EmployeeID == doctorId && m.RegisterDate == reservationDate.Date).Max(m => m.RegisterSeq) ?? 0) + 1
            };
            var addEntry = _db.CHIS_Register.Add(et);
            await _db.SaveChangesAsync();
            var rlt = addEntry.Entity;           
            //异步执行
            _notifySvr.NotifyDoctorReservatedAsync(doctorId, custId, rlt.RegisterID);
            return rlt;
        }


        /// <summary>
        /// 获取医生的接诊清单
        /// </summary>   
        public IQueryable<vwCHIS_Register> GetDoctorRegistList(int doctorId, DateTime dt0, DateTime dt1, int pageIndex, int pageSize)
        {
            return _db.vwCHIS_Register.AsNoTracking().Where(m => m.EmployeeID == doctorId && m.RegisterDate >= dt0 && m.RegisterDate < dt1)
                .OrderByDescending(m => m.RegisterID).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

        /// <summary>
        /// 获取比当前接诊更新的接诊数据
        /// </summary>
        /// <param name="lastRegistId"></param>
        public IQueryable<vwCHIS_Register> GetDoctorNewerRegistList(long lastRegistId)
        {
            var r = _db.CHIS_Register.Find(lastRegistId);
            return _db.vwCHIS_Register.AsNoTracking().Where(m => m.EmployeeID == r.EmployeeID && m.RegisterID > r.RegisterID);
        }

    }
}

