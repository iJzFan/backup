using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using Ass;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using CHIS.Models.ViewModel;

namespace CHIS.Controllers
{
    public partial class Search : BaseController
    {
        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> SickNoteCheck(string sickNoteId)
        {
            CHIS.Models.ViewModel.SickNoteCheckModel model = null;
            if (sickNoteId.IsNotEmpty())
            {
                var sn = _db.CHIS_Doctor_SickNote.AsNoTracking().FirstOrDefault(m => m.SickNoteId == sickNoteId);
                var treat = await _db.vwCHIS_DoctorTreat.AsNoTracking().SingleOrDefaultAsync(m => m.TreatId == sn.TreatId);
                model = new SickNoteCheckModel
                {
                    SickNote = sn,
                    Treat = treat
                };
            }
            return View(model);
        }
        public async Task<IActionResult> GetSickNoteInfo(string sickNoteId)
        {
            if (sickNoteId.IsEmpty()) throw new Exception("没有找到Id");
            var sn = _db.CHIS_Doctor_SickNote.Find(sickNoteId);
            var treat = await _db.vwCHIS_DoctorTreat.AsNoTracking().SingleOrDefaultAsync(m => m.TreatId == sn.TreatId);
            return View("SickNoteCheck_pv_SearchResult");
        }


        #region 处方查询系统

        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> PrescriptionCheck(Guid? prescriptionNo)
        {
            CHIS.Models.ViewModel.PrescriptionCheckModel model = await GetPrescriptionCheckModel(prescriptionNo); 
            return View(model);
        }
        [AllowAnonymous]
        public async Task<IActionResult> LoadPrescriptionCheck(Guid? prescriptionNo)
        {
            CHIS.Models.ViewModel.PrescriptionCheckModel model = await GetPrescriptionCheckModel(prescriptionNo);
            return PartialView("PrescriptionCheck_pv_SearchResult",model);
        }


        private async Task<CHIS.Models.ViewModel.PrescriptionCheckModel> GetPrescriptionCheckModel(Guid? prescriptionNo)
        {
            CHIS.Models.ViewModel.PrescriptionCheckModel model = new PrescriptionCheckModel();
            if (prescriptionNo != null && prescriptionNo != new Guid())
            {
                long treatId = 0;
                CHIS_DoctorAdvice_Herbs herb = null;
                CHIS_DoctorAdvice_Formed formed = null;
                model.PrescriptionNo = prescriptionNo.Value;

                formed = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().FirstOrDefault(m => m.PrescriptionNo == prescriptionNo);
                if (formed == null)
                {
                    herb = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().FirstOrDefault(m => m.PrescriptionNo == prescriptionNo);
                    treatId = herb.TreatId;
                    model.herb = herb;
                }
                else
                {
                    model.formed = formed;
                    treatId = formed.TreatId;
                }

                var treat = await _db.vwCHIS_DoctorTreat.AsNoTracking().SingleOrDefaultAsync(m => m.TreatId == treatId);
                model.Treat = treat;
            }
            return model;
        }

        #endregion


    }
}
