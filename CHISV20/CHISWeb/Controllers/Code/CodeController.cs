using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ass;
using CHIS.Code;
using CHIS.Codes.Utility;

namespace CHIS.Controllers
{
    [Authorize]
    public partial class CodeController : BaseController
    {
        Services.DrugService _drugSvr;
        Services.CustomerService _cusSvr;
        Services.DoctorService _docrSvr;
        public CodeController(DbContext.CHISEntitiesSqlServer db
            , Services.DrugService drugsvr
            , Services.CustomerService cusSvr
            , Services.DoctorService docrSvr
            ) : base(db)
        {
            _drugSvr = drugsvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
        }
    }
}
