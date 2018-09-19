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

namespace CHIS.Controllers
{
    public partial class PharmacyController : BaseController
    {
        Services.PharmacyService _pharSvr;
        Services.AccessService _accSvr;
        public PharmacyController(Services.PharmacyService pharSvr
            ,Services.AccessService accSvr
            ,DbContext.CHISEntitiesSqlServer db):base(db)
        {
            _pharSvr = pharSvr;
            _accSvr = accSvr;
        }
    }
}
