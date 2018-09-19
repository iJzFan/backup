using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass.Models;
using Ass;
using System.Collections.Generic;
using CHIS.Models.DataModel;
using CHIS.Models;

namespace CHIS.Controllers
{
    public partial class MedicalLib : BaseController
    {
        public MedicalLib(DbContext.CHISEntitiesSqlServer db) : base(db) { }

        //我的病历记录
        [AllowAnonymous]
        public IActionResult MyMedicalRecords(int customerId)
        {
            var cus = _db.vwCHIS_Code_Customer.Find(customerId);
            var model = new Models.DataModel.MyMedicalHistoryRecordsModel
            {
                Customer = cus,
                List1st = GetListOfMyMedicalRecord(customerId)
            };
            return View(model);
        }
        [AllowAnonymous]
        public IActionResult MyMedicalRecordList(int customerId, int pageIndex = 1, int pageSize = 10)
        {
            return PartialView("_pvMyMedicalRecordList", GetListOfMyMedicalRecord(customerId, pageIndex, pageSize));
        }

        public Ass.Mvc.PageListInfo<MyMedicalHistoryRecordItemModel> GetListOfMyMedicalRecord(int customerId, int pageIndex = 1, int pageSize = 10)
        {
            var finds = _db.vwCHIS_DoctorTreat.AsNoTracking().Where(m => m.CustomerId == customerId && m.TreatStatus == 2).Select(m => new MyMedicalHistoryRecordItemModel
            {
                TreatId = m.TreatId,
                FirstTreatTime = m.FirstTreatTime.Value,
                TreatTime = m.TreatTime,
                DoctorId = m.DoctorId,
                DoctorName = m.DoctorName,
                Diagnosis1Name = m.Diagnosis1,
                Diagnosis2Name = m.Diagnosis2
            });
            var count = finds.Count();
            var items = finds.OrderByDescending(m => m.FirstTreatTime).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
            var model = new Ass.Mvc.PageListInfo<MyMedicalHistoryRecordItemModel>
            {
                DataList = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = count
            };
            return model;
        }

        [AllowAnonymous]
        public IActionResult MyMedicalRecordDetails(long treatId)
        {
            var treat = _db.vwCHIS_DoctorTreat.Find(treatId);
            var herb = _db.vwCHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 1).ToList();
            var herbs = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 1).ToList();
            var form = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 1).ToList();
            var forms = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 1).ToList();

            Dictionary<CHIS_DoctorAdvice_Formed, IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail>> formdic = new Dictionary<CHIS_DoctorAdvice_Formed, IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail>>();
            foreach (var item in form) formdic.Add(item, forms.Where(m => m.PrescriptionNo == item.PrescriptionNo));


            Dictionary<vwCHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>> herbDic = new Dictionary<vwCHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>>();
            foreach (var item in herb) herbDic.Add(item, herbs.Where(m => m.PrescriptionNo == item.PrescriptionNo));
             
            var model = new MyMedicalHistoryRecordDetailModel
            {
                Treat = treat,
                FormDic = formdic,
                HerbDic = herbDic,
                SpecialTreat = GetSpecialTreatData(treatId)//载入特殊接诊
            };
            return PartialView("_pvMyMedicalRecordDetails", model);
        }

        /// <summary>
        /// 载入特殊接诊的数据
        /// </summary>
        private Models.ViewModels.SpecialTreat GetSpecialTreatData(long treatId)
        {
            //载入特殊接诊数据
            var specialTreat = _db.CHIS_Doctor_TreatExt.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
            Models.ViewModels.SpecialTreat sTreat = null;
            if (specialTreat != null)
            {
                var reg = _db.vwCHIS_Register.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
                if (reg.SpetialDepartTypeVal == MPS.SpetialDepartType_PSYCH)
                {
                    //如果是心理接诊
                    sTreat = new Models.ViewModels.SpecialTreat_Psych
                    {
                        DoctorTreatExt = specialTreat,
                        QsData = _db.vwCHIS_Data_PsychPretreatQs.AsNoTracking().FirstOrDefault(m => m.PsychPretreatQsId.ToString() == reg.PropValue),
                        SpetialDepartTypeVal = reg.SpetialDepartTypeVal
                    };
                }
                else sTreat = new Models.ViewModels.SpecialTreat
                {
                    //载入通用的数据
                    DoctorTreatExt = specialTreat,
                    SpetialDepartTypeVal = reg.SpetialDepartTypeVal
                };
            }
            return sTreat;
        }
    }
}
