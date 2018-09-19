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
    public partial class MedicalLib 
    { 
        //药品详细信息
        public IActionResult DrugDetail(int drugId)
        {
            var drug = _db.vwCHIS_Code_Drug_Main.Find(drugId);
            return View(drug);
        }

    }
}
