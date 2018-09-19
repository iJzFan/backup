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

namespace CHIS.Services
{
    public class DoctorService : BaseService
    {
        Services.AccessService _accSvr;
        public DoctorService(CHISEntitiesSqlServer db
            , Services.AccessService accSvr) : base(db)
        {
            _accSvr = accSvr;
        }




        /// <summary>
        /// 获取工作站的医生作业统计
        /// </summary>
        /// <param name="doctorId">医生Id</param>
        /// <param name="stationId">工作站</param>        
        public async Task<DoctorStatisticSummary> GetMyStatisticSummaryAsync(int doctorId, int stationId)
        {
            var dateTimeTodayStart = DateTime.Today;
            var dateTimeTodayEnd = DateTime.Today.AddDays(1);
            var dw = (int)DateTime.Now.DayOfWeek;
            var dateTimeWeekStart = DateTime.Today.AddDays(0 - dw + 1);
            var dateTimeWeekEnd = dateTimeWeekStart.AddDays(7);
            //sp_Statistics_Doctor_Summary
            //_db
            var tToday = (await _db.QueryTableAsync(string.Format("sp_Statistics_Doctor_Summary {0},{1},'{2:yyyy-MM-dd}','{3:yyyy-MM-dd}'", doctorId, stationId, dateTimeTodayStart, dateTimeTodayEnd))).Rows[0];
            var tWeek = (await _db.QueryTableAsync(string.Format("sp_Statistics_Doctor_Summary {0},{1},'{2:yyyy-MM-dd}','{3:yyyy-MM-dd}'", doctorId, stationId, dateTimeWeekStart, dateTimeWeekEnd))).Rows[0];


            return new DoctorStatisticSummary
            {
                TreatCountOfToday = Convert.ToInt32(tToday["TreatCount"]),
                TreatCountOfWeek = Convert.ToInt32(tWeek["TreatCount"]),
                FeeAmountOfToday = Convert.ToDecimal(tToday["TreatFeeAmount"]),
                FeeAmountOfWeek = Convert.ToDecimal(tWeek["TreatFeeAmount"]),
                DrugQtyAmountOfToday = Convert.ToInt32(tToday["DrugCount"]),
                DrugQtyAmountOfWeek = Convert.ToInt32(tWeek["DrugCount"]),
                DrugTypeCountOfToday = Convert.ToInt32(tToday["DrugTypeCount"]),
                DrugTypeCountOfWeek = Convert.ToInt32(tWeek["DrugTypeCount"]),
                DoctorId = doctorId,
                StationId = stationId
            };
        }

        /// <summary>
        /// 获取医生的值班信息
        /// </summary>
        /// <param name="doctorId"></param>
        /// <param name="date"></param>
        /// <param name="slotNum"></param>
        /// <returns></returns>
        public Models.vwCHIS_Doctor_OnOffDutyData GetDoctorOnDutyInfo(int doctorId,int departId, DateTime date, int slotNum)
        {
            var rlt= _db.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.DoctorId == doctorId &&
                                         (m.ScheduleDate - date).Days == 0 && m.Slot == slotNum);
            if (rlt == null) rlt= InitalEmployeeOnDutyInfo(doctorId, departId, date, slotNum);
            return rlt;
        }

        /// <summary>
        /// 获取雇员（医生）值班情况信息
        /// </summary>
        private Models.vwCHIS_Doctor_OnOffDutyData InitalEmployeeOnDutyInfo(int doctorId, int departId, DateTime date, int slotNum)
        {

            var depart = _db.CHIS_Code_Department.Find(departId);

            var item = _db.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.DoctorId == doctorId &&
                                           (m.ScheduleDate - date).Days == 0 && m.Slot == slotNum);
            if (item == null)
            {
                //如果是正常节假日则                
                var doctor = _db.vwCHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                if (doctor == null) throw new Exception("没有找到预约医生信息");
                var defSlot = _db.CHIS_Code_DoctorWorkInfo.FirstOrDefault(m => m.DoctorId == doctorId && m.Slot == slotNum);
                if (defSlot == null) //更新医生默认的Slot
                {
                    TimeSpan fromTime = slotNum == 1 ? new TimeSpan(8, 0, 0) : new TimeSpan(14, 0, 0);
                    TimeSpan toTime = slotNum == 1 ? new TimeSpan(12, 0, 0) : new TimeSpan(18, 0, 0);
                    defSlot = _db.Add(new Models.CHIS_Code_DoctorWorkInfo
                    {
                        DoctorId = doctor.DoctorId,
                        Slot = slotNum,
                        DefSlotAllowNum = 100,
                        DefSlotAllowReservateNum = 20,
                        FromTime = fromTime,
                        ToTime = toTime
                    }).Entity;
                    _db.SaveChanges();
                }
                var addItem = _db.Add(new Models.CHIS_Doctor_OnOffDutyData
                {
                    DoctorId = doctorId,
                    DepartmentId = departId,
                    ScheduleDate = date.Date,
                    StationId = depart.StationID,
                    Slot = slotNum,
                    ReservateLimitNum = defSlot.DefSlotAllowReservateNum.Value,
                    ReservatedNum = 0,
                    MaxCount = defSlot.DefSlotAllowNum.Value,
                    FromTime = defSlot.FromTime,
                    IsNextDayFromTime = defSlot.IsNextDayOfFromTime,
                    IsNextDayToTime = defSlot.IsNextDayOfToTime,
                    ToTime = defSlot.ToTime,
                    IsLimitNum = false,
                    IsWorkSlot = true,
                    AddCount = 0
                }).Entity;
                _db.SaveChanges();
                return _db.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.OnOffDutyId == addItem.OnOffDutyId);
            }
            return item;
        }



        /// <summary>
        /// 查找医生信息
        /// </summary>
        /// <param name="doctorId">医生Id</param>
        /// <param name="searchText">搜索的名称</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public (int, IQueryable<DoctorSEntityV00>) SearchTreatDoctors(string searchText, int? doctorId = null, int pageIndex = 1, int pageSize = 20)
        {
            int total = 0;
            var query = _db.vwCHIS_Code_Doctor.AsNoTracking();
#if DEBUG
#else
            query=query.Where(m => m.IsDoctorOfPerson);
#endif
            if (doctorId.HasValue && doctorId > 0) { query = query.Where(m => m.DoctorId == doctorId); total = 1; }
            else
            {
                if (string.IsNullOrEmpty(searchText)) query = query.Where(m => true);//返回所有
                else
                {
                    var t = searchText.Trim().GetStringType();
                    if (t.IsMobile) query = query.Where(m => m.Telephone == t.String || m.CustomerMobile == t.String);
                    else if (t.IsEmail) query = query.Where(m => m.Email == t.String);
                    else
                    {
                        if (t.IsContainChinese) query = query.Where(m => m.DoctorName == searchText);
                        else query = query.Where(m => m.NamePY.Contains(searchText));
                    }
                }
            }
            query = query.OrderBy(m => m.DoctorId).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
            var rlt = query.Select(m => new DoctorSEntityV00
            {
                DoctorId = m.DoctorId,
                DoctorName = m.DoctorName,
                DoctorGender = (m.Gender ?? 2).ToGenderString(),
                PostTitleName = m.PostTitleName,
                CustomerId = m.CustomerId.Value,
                DoctorPhotoUrl = m.DoctorPhotoUrl.ahDtUtil().GetDoctorImg(m.Gender, imgSizeTypes.HorizThumb),
                DoctorSkillRmk = m.DoctorSkillRmk,
                DoctorAppId = m.DoctorAppId
            });
            return (total, rlt);
        }

        public IEnumerable<vwCHIS_Code_Doctor> FindList(IEnumerable<int> doctorIds)
        {
            return _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctorIds.Contains(m.DoctorId));
        }

        /// <summary>
        /// 查询医生信息
        /// </summary>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public Models.vwCHIS_Code_Doctor Find(int doctorId)
        {
            return _db.vwCHIS_Code_Doctor.Find(doctorId);
        }

        /// <summary>
        /// 根据list查询医生们
        /// </summary>
        /// <param name="doctorids"></param>
        /// <returns></returns>
        public IQueryable<vwCHIS_Code_Doctor> Find(List<int> doctorids)
        {
            return _db.vwCHIS_Code_Doctor.AsNoTracking()
                .Where(m => doctorids.Contains(m.DoctorId))
                .OrderBy(m => doctorids.IndexOf(m.DoctorId));
        }


        /// <summary>
        /// 获取医生的所在工作站
        /// </summary>
        /// <param name="doctorId">医生Id</param>
        /// <returns></returns>
        public IQueryable<StationInfoMin> GetDoctorsStations(int doctorId, bool bOnlyTreat = true)
        {
            var treatRoles = "treat_doctor,treat_doctor_all,drugstore_treat_ext".Split(',');
            var roleids = _db.CHIS_SYS_Role.AsNoTracking().Where(m => treatRoles.Contains(m.RoleKey)).Select(m => m.RoleID).ToList();
            var q = _db.CHIS_Sys_Rel_DoctorStationRoles.Where(m => m.DoctorId == doctorId && m.MyStationIsEnable && m.MyRoleIsEnable);
            if (bOnlyTreat) q = q.Where(m => roleids.Contains(m.RoleId.Value));
            return q.Join(_db.CHIS_Code_WorkStation, a => a.StationId, g => g.StationID, (a, g) => new
            {
                g.StationID,
                g.StationName,
                g.IsCanTreat,
                g.IsEnable
            }).Where(m => m.IsEnable && m.IsCanTreat).Select(m => new StationInfoMin
            {
                StationName = m.StationName,
                StationId = m.StationID
            }).Distinct();
        }

        /// <summary>
        ///  查询医生
        /// </summary>
        /// <param name="doctorAppId"></param>
        /// <returns></returns>
        internal Models.vwCHIS_Code_Doctor FindByAppId(string doctorAppId)
        {
            return _db.vwCHIS_Code_Doctor.SingleOrDefault(m => m.DoctorAppId == doctorAppId);
        }


        /// <summary>
        /// 删除医生信息以及所有信息
        /// </summary>
        /// <param name="doctorId"></param>
        internal async Task<bool> DeleteAsync(int doctorId)
        {
            // var u = controller.UserSelf; 
            if (!_accSvr.GetFuncConfig(MyConfigNames.DoctorDocument_DoctorDocs_IsDel).ToBool()) throw new Exception("无删除权限");


            _db.BeginTransaction();

            try
            {
                var rlt0 = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.DoctorId == doctorId);
                _db.CHIS_Sys_Rel_DoctorStations.RemoveRange(rlt0);

                var rlt1 = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId);
                _db.CHIS_Code_Rel_DoctorDeparts.RemoveRange(rlt1);

                var rlt2 = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId);
                _db.CHIS_Sys_Rel_DoctorStationRoles.RemoveRange(rlt2);

                var doctor = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                _db.CHIS_Code_Doctor.Remove(doctor);

                var login = _db.CHIS_Sys_Login.FirstOrDefault(m => m.DoctorId == doctorId);
                if (login != null)
                {
                    login.DoctorId = null;
                    login.IsAllowedAHMS = false;
                }
                await _db.SaveChangesAsync();
                _db.CommitTran();
            }
            catch (Exception ex)
            { _db.RollbackTran(); throw ex; }

            return true;

        }

        internal IQueryable<DepartInfoMin> GetDoctorDepartmentsOfStation(int doctorId, int stationId, bool bOnlyTreat = true)
        {
            var q = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId && m.IsVerified);
            var qr = q.Join(_db.CHIS_Code_Department, a => a.DepartId, g => g.DepartmentID, (a, g) => new
            {
                a.DepartId,
                g.IsEnable,
                g.IsNotTreatDept,
                g.StationID
            }).Where(m => m.StationID == stationId && m.IsEnable);
            if (bOnlyTreat) qr = qr.Where(m => m.IsNotTreatDept != true);
            return qr.Join(_db.CHIS_Code_Department, a => a.DepartId, g => g.DepartmentID, (a, g) => new DepartInfoMin
            {
                DepartId = a.DepartId.Value,
                DepartmentName = g.DepartmentName
            });

        }


        /// <summary>
        /// 获取处方医生
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DoctorSEntityV02> GetMyRxDoctors(int stationId)
        {
            var finds = _db.CHIS_DrugStore_Doctor.AsNoTracking().Where(m => m.StationId == stationId).Join(
                 _db.vwCHIS_Code_Doctor, a => a.DoctorId, g => g.DoctorId, (a, g) => new DoctorSEntityV02
                 {
                     DoctorId = g.DoctorId,
                     DoctorName = g.DoctorName,
                     DoctorGender = (g.Gender ?? 2).ToGenderString(),
                     PostTitleName = g.PostTitleName,
                     CustomerId = g.CustomerId.Value,
                     DoctorPhotoUrl = g.DoctorPhotoUrl.ahDtUtil().GetDoctorImg(g.Gender, imgSizeTypes.HorizThumb),
                     DoctorSkillRmk = P.PStr(g.DoctorSkillRmk,null,true),
                     DoctorAppId = g.DoctorAppId.IsEmpty()?"0":g.DoctorAppId,
                     StationId = stationId,
                     IsRxDefault = a.IsDefault
                 });

            return finds.OrderByDescending(m=>m.IsRxDefault);
        }
        /// <summary>
        /// 获取默认的处方医生
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public DoctorSEntityV02 GetMyDefRxDoctor(int stationId)
        {
            return GetMyRxDoctors(stationId).FirstOrDefault(m => m.IsRxDefault);
        }

        /// <summary>
        /// 添加处方医生
        /// </summary>  
        public bool AddRxDoctor(int stationId, int doctorId)
        {
            var find = _db.CHIS_DrugStore_Doctor.FirstOrDefault(m => m.DoctorId == doctorId && m.StationId == stationId);
            if (find != null) return true;
            else _db.CHIS_DrugStore_Doctor.Add(new CHIS_DrugStore_Doctor
            {
                StationId = stationId,
                DoctorId = doctorId,
                IsDefault = false,
                DrugStoreDoctorId=$"{stationId}_{doctorId}"
            });
            _db.SaveChanges();
            return true;
        }
        /// <summary>
        /// 删除处方医生
        /// </summary> 
        public bool DeleteRxDoctor(int stationId,int doctorId)
        {
            var find = _db.CHIS_DrugStore_Doctor.FirstOrDefault(m => m.DoctorId == doctorId && m.StationId == stationId);
            if (find != null) {
                _db.Remove(find);
            }
            _db.SaveChanges();
            return true;
        }
        /// <summary>
        /// 设置默认医生
        /// </summary>  
        public bool SetRxDoctorIsDefault(int stationId,int doctorId)
        {
            var finds = _db.CHIS_DrugStore_Doctor.Where(m => m.StationId == stationId);
            foreach(var item in finds)
            {
                if (item.DoctorId == doctorId) item.IsDefault = true;
                else item.IsDefault = false;
            }
            _db.SaveChanges();
            return true;
        }
    }
}
