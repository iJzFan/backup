using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
 
    public partial class MyStationSettings : BaseController
    {
        public MyStationSettings(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        public IActionResult Index()
        {
            return View();
        }
    }

}