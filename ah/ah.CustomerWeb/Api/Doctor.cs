using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass;
using ah.Areas.Customer.Controllers.Base;
using ah.Models;
using ah.Services;

namespace ahWeb.Api
{
    [AllowAnonymous]
    public class Doctor : BaseDBController
    {
        private GetInfoService _getInfoService;

        public Doctor(ah.DbContext.AHMSEntitiesSqlServer db, GetInfoService getInfoService) : base(db)
        {
            _getInfoService = getInfoService;
        }

        /// <summary>
        /// ͨ������ID��ѯҽ����Ϣ
        /// <returns>�ɹ��򷵻��û���Ϣ</returns>
        /// </summary>
        public IActionResult Json_GetDoctorOfDepartment(int departmentId)
        {
            try
            {
                var doctors = MainDbContext.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DepartId == departmentId && m.IsVerified == true).ToList().Select(m => m.DoctorId);
                var find = MainDbContext.vwCHIS_Code_Doctor.AsNoTracking().Where(m => doctors.Contains(m.DoctorId));

                var departs = MainDbContext.CHIS_Code_Department.Where(m => m.StationID == 10).Select(m => m.DepartmentID);

                find = find.Where(m => m.IsDoctor == true);
                if (!departs.Contains(departmentId))
                    find = find.Where(m => m.IsForTest == false);

                var docList = find.Select(m => new
                {
                    DoctorId = m.DoctorId,
                    DoctorName = m.DoctorName,
                    DoctorSkillRmk = m.DoctorSkillRmk,
                    DoctorPhotoUrl = m.DoctorPhotoUrl,
                    PostTitleName = m.PostTitleName,
                    PhotoUrlDef = m.PhotoUrlDef
                });
                docList = docList.OrderBy(m => m.DoctorName);
                return Json(new { rlt = true, msg = "", docList = docList });
            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }

        /// <summary>
        /// ͨ������վID��ѯҽ����Ϣ
        /// <returns>�ɹ��򷵻�ҽ����Ϣ</returns>
        /// </summary>
       // [ResponseCache(Duration = 100, VaryByQueryKeys = new string[] { "stationId" })]
        public IActionResult Json_GetDoctorOfStation(int stationId)
        {
            return TryCatchFunc((dd) =>
            {
                var doctors = MainDbContext.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.StationId == stationId && m.StationIsEnable).Select(m => m.DoctorId);
                var doctors2 = MainDbContext.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.StationID == stationId && m.IsEnable == true).Select(m => m.DoctorId.Value);
                var docs = doctors.Intersect(doctors2);
                var find = MainDbContext.vwCHIS_Code_Doctor.AsNoTracking().Where(m => docs.Contains(m.DoctorId));

                find = find.Where(m => m.IsDoctor == true);
                if (stationId != 10)
                    find = find.Where(m => m.IsForTest == false);

                var docList = find.Select(m => new
                {
                    DoctorId = m.DoctorId,
                    DoctorName = m.DoctorName,
                    DoctorSkillRmk = m.DoctorSkillRmk,
                    DoctorPhotoUrl = m.DoctorPhotoUrl,
                    PostTitleName = m.PostTitleName,
                    PhotoUrlDef = m.PhotoUrlDef
                }).OrderBy(m => m.DoctorName);
                dd.docList = docList;
                return null;
            });
        }
        #region ��ȡ����ҽ����Ϣ
        //[ResponseCache(Duration = 100, VaryByQueryKeys = new string[] { "searchText", "pageIndex" })]
        public IActionResult json_GetAllDoctors(string searchText, int pageIndex = 1, int pageSize = 20)
        {
            return TryCatchFunc((dd) =>
            {
                dd.pageListInfo = queryAllDoctors(searchText, pageIndex, pageSize);
                return null;
            });
        }
        public IActionResult AllDoctorsLists()
        {
            //�˴���Ҫ�ر�ָ��ҳ���ļ���λ��
            return View("~/Areas/Customer/Views/Appointment/AllDoctorsLists.cshtml");
        }
        //[ResponseCache(Duration = 100, VaryByQueryKeys = new string[] { "searchText", "pageIndex" })]
        public async Task<IActionResult> pv_GetAllDoctors(string searchText, int pageIndex = 1, int pageSize = 20)
        {
            var model = await _getInfoService.SearchDoctorsInfosAsync(searchText, pageIndex, pageSize);
            ViewBag.searchText = searchText;
            return PartialView("~/Areas/Customer/Views/Appointment/_pvDoctorList.cshtml", model);
        }

        private Ass.Mvc.PageListInfo<vwCHIS_Code_Doctor> queryAllDoctors(string searchText, int pageIndex = 1, int pageSize = 20)
        {
            var finds = MainDbContext.vwCHIS_Code_Doctor.AsNoTracking().Where(m => m.IsForTest != true);
            if (searchText.IsNotEmpty())
            {
                var strType = searchText.GetStringType();
                if (strType.IsMobile) finds = finds.Where(m => m.CustomerMobile == searchText);
                else finds = finds.Where(m => m.DoctorName == searchText);
            }

            var count = finds.Count();
            finds = finds.OrderBy(m => m.DoctorId).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return new Ass.Mvc.PageListInfo<vwCHIS_Code_Doctor>
            {
                DataList = finds,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = count
            };
        }


        #endregion

        /// <summary>
        /// ��ȡҽ��������Ϣ
        /// </summary>
        public IActionResult Json_GetDoctorWorkInfos(int doctorId, int departId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {

                dateFrom = (dateFrom.Date <= DateTime.Now.Date ? DateTime.Now.AddDays(0).Date : dateFrom.Date);
                if (dateTo <= dateFrom ||
                    ((dateTo - DateTime.Now).TotalDays > 16))
                    dateTo = DateTime.Now.AddDays(16).Date;

                var finds = InitalDoctorOnDutyInfo(doctorId, departId, dateFrom, dateTo).Select(
                    m => new ah.Models.ViewModel.DoctorWorkInfo
                    {
                        DoctorId = doctorId,
                        DoctorName = m.DoctorName,
                        WorkDate = m.ScheduleDate,
                        Work_Vacation = m.IsWorkSlot == false ? "VACATION" : "WORK",
                        SlotIndex = m.Slot.Value,
                        SlotTimeStart = m.FromTime.Value,
                        SlotTimeEnd = m.ToTime.Value,
                        AllowRegNum = m.MaxCount,
                        ReservateLimitNum = m.ReservateLimitNum,
                        ReservatedNum = m.ReservatedNum
                    }).ToList().OrderBy(m => m.WorkDate).OrderBy(m => m.SlotIndex);



                //���п����ʱ��
                var tagDays = new List<DateTime>();
                for (int i = 0; i <= (dateTo - dateFrom).Days; i++)
                {
                    tagDays.Add(dateFrom.AddDays(i).Date);
                }

                //��ȡ���ϰ��յ�ʱ���
                var workDayList = (from item in finds
                                   where item.IsWork
                                   orderby item.WorkDate
                                   select item.WorkDate.Date).Distinct();

                //��������ʱ��
                var notWorkDays = from item in tagDays where !workDayList.Contains(item) select item.ToString("yyyy-MM-dd");


                return Json(new
                {
                    rlt = true,
                    msg = "ҽ�����ϰ����",
                    items = finds,
                    workDays = workDayList,
                    notWorkDays = notWorkDays
                });

            }
            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
        }


        //��ȡҽ�����հ��ʣ��ԤԼ��������Ϣ
        public IActionResult Json_GetDoctorReservationInfo(int doctorId, int departId, DateTime date, int slotNum)
        {

            return TryCatchFunc((dd) =>
            {
                //todo ������
                InitalDoctorOnDutyInfo(doctorId, departId, date, date);

                var findDutys = GetDoctorOnDutyInfo(doctorId, departId, date, slotNum);
                int restReservatedNum = findDutys.ReservateLimitNum - findDutys.ReservatedNum;//ʣ��ԤԼ����     
                if (date.Date == DateTime.Today)//����ǵ���
                {
                    restReservatedNum = findDutys.MaxCount + findDutys.AddCount - findDutys.TodayRegistedNum;
                }
                bool isFull = (restReservatedNum <= 0);
                dd.isWorkSlot = findDutys.IsWorkSlot.Value;
                dd.isFull = isFull;
                dd.restReservatedNum = restReservatedNum;
                return null;
            });
        }




        #region ��ʼ��ҽ���ϰ�����



        /// <summary>
        /// Ĭ���ϰ��������������
        /// </summary>
        public List<vwCHIS_Doctor_OnOffDutyData> InitalDoctorOnDutyInfo(int doctorId, int departId, DateTime dateFrom, DateTime dateTo)
        {
            var rtn = new List<vwCHIS_Doctor_OnOffDutyData>();
            int[] slots = new int[] { 1, 2 };
            int days = (dateTo - dateFrom).Days;
            for (int i = 0; i <= days; i++)
            {
                DateTime date = dateFrom.AddDays(i).Date;
                foreach (var slotNum in slots)
                {
                    var slotInfo = InitalDoctorOnDutyInfo(doctorId, departId, date, slotNum);
                    if (date.Date == DateTime.Today)
                    {
                        if (DateTime.Now.TimeOfDay.Add(new TimeSpan(0, 10, 0)) > slotInfo.ToTime) continue;
                    }
                    rtn.Add(slotInfo);
                }
            }
            return rtn;
        }

        /// <summary>
        /// ��ȡ��Ա��ҽ����ֵ�������Ϣ
        /// </summary>
        private vwCHIS_Doctor_OnOffDutyData InitalDoctorOnDutyInfo(int doctorId, int departId, DateTime date, int slotNum)
        {

            var depart = MainDbContext.CHIS_Code_Department.FirstOrDefault(m => m.DepartmentID == departId);
            if (depart == null) throw new Exception("û�иÿ�����Ϣ");

            var item = MainDbContext.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.DoctorId == doctorId && m.DepartmentId == departId &&
                                           (m.ScheduleDate - date).Days == 0 && m.Slot == slotNum);
            if (item == null)
            {
                //����������ڼ�����                
                var doctor = MainDbContext.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                if (doctor == null) throw new Exception("û���ҵ�ԤԼҽ����Ϣ");
                var defSlot = MainDbContext.CHIS_Code_DoctorWorkInfo.FirstOrDefault(m => m.DoctorId == doctorId && m.DepartmentId == departId && m.Slot == slotNum);
                if (defSlot == null) //����ҽ��Ĭ�ϵ�Slot
                {
                    TimeSpan fromTime = slotNum == 1 ? new TimeSpan(8, 0, 0) : new TimeSpan(14, 0, 0);
                    TimeSpan toTime = slotNum == 1 ? new TimeSpan(12, 0, 0) : new TimeSpan(18, 0, 0);
                    defSlot = MainDbContext.Add(new CHIS_Code_DoctorWorkInfo
                    {
                        DoctorId = doctor.DoctorId,
                        DepartmentId = departId,
                        Slot = slotNum,
                        DefSlotAllowNum = 100,
                        DefSlotAllowReservateNum = 20,
                        FromTime = fromTime,
                        ToTime = toTime
                    }).Entity;
                    MainDbContext.SaveChanges();
                }
                var addItem = MainDbContext.Add(new CHIS_Doctor_OnOffDutyData
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
                MainDbContext.SaveChanges();
                return MainDbContext.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.OnOffDutyId == addItem.OnOffDutyId);
            }
            return item;
        }


        public vwCHIS_Doctor_OnOffDutyData GetDoctorOnDutyInfo(int doctorId, int departId, DateTime date, int slotNum)
        {
            return MainDbContext.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.DoctorId == doctorId && m.DepartmentId == departId &&
                                         (m.ScheduleDate - date).Days == 0 && m.Slot == slotNum);
        }


        #endregion

    }
}