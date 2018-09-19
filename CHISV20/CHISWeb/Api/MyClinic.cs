using CHIS.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Api
{
    public class MyClinic : BaseDBController
    {
        DrugService _drugSrv;
        public MyClinic(DrugService drugSrv,DbContext.CHISEntitiesSqlServer db):base(db)
        {
            _drugSrv = drugSrv;
        }

        public IActionResult GetMyClinicDrugs()
        {
            var rlt = _drugSrv.GetAllDrugsOfClinic(UserSelf.StationId);
            return Json(rlt);
        }

    }
}
