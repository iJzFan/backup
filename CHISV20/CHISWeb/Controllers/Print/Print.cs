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
    public partial class Print : BaseController
    {
        Services.DrugService _drugSvr;
        Services.TreatService _treatSvr;
        public Print(DbContext.CHISEntitiesSqlServer db,
            Services.DrugService drugSvr,
            Services.TreatService treatSvr
            ) : base(db) { _drugSvr = drugSvr;
            _treatSvr = treatSvr;
        }
        /// <summary>
        /// 打印成药处方单
        /// </summary> 
        public IActionResult PrintFormedPrescription(Guid? prescriptNo)
        {
            if (!prescriptNo.HasValue|| prescriptNo == new Guid()) throw new Exception("请保存后打印");
            var model = _treatSvr.GetPrescriptionDetail(prescriptNo);
            return View(model);
        }

        /// <summary>
        /// 打印中药处方单
        /// </summary> 
        public IActionResult PrintHerbPrescription(Guid prescriptNo)
        {
            if (prescriptNo == new Guid()) throw new Exception("请保存后打印");
            var main = _db.vwCHIS_DoctorAdvice_Herbs.AsNoTracking().FirstOrDefault(m => m.PrescriptionNo == prescriptNo);
            var detail = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.PrescriptionNo == prescriptNo);
            var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == main.TreatId);
            return View(new CHIS.Models.ViewModel.PrintHerbModel
            {
                Treat = treat,
                Detail = detail,
                Main = main
            });
        }


        public IActionResult PrintSickNote(string sickNoteId)
        {
            var a = _db.CHIS_Doctor_SickNote.Find(sickNoteId);
            var t = _db.vwCHIS_DoctorTreat.Find(a.TreatId);
            var xy = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == a.TreatId);
            var zy = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.TreatId == a.TreatId);
            return View(new PrintSickNoteModel
            {
                SickNote = a,
                Treat = t,
                Formed = xy,
                Herbs = zy
            });
        }

    }
}
