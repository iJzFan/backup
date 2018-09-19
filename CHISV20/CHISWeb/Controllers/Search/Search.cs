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
        Services.DoctorService _docSvr;
        public Search(CHIS.DbContext.CHISEntitiesSqlServer db
            ,Services.DoctorService docSvr):base(db)
        {
            _docSvr = docSvr;
        }

        
        public ActionResult SearchDoctors()
        {
            return View();
        }
        public ActionResult FindDoctors(string searchText)
        {
            if (searchText.IsEmpty()) return Content("请输入医生信息");
            var model = _docSvr.SearchTreatDoctors(searchText).Item2.ToList();
            return PartialView("SearchDoctors_ul", model);
        }
    }
}
