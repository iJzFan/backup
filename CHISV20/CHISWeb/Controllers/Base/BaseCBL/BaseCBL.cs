using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Controllers
{
    public class BaseCBL
    {
        public BaseController controller = null;
        public DbContext.CHISEntitiesSqlServer _db { get; set; }       
         
        public Models.UserSelf CurrentOper = null;
        public Models.UserSelf CurrentMachineOper = null;

        public BaseCBL(BaseController c)
        {
            this.controller = c;
            this._db = c._db;

        }

        public BaseCBL(DbContext.CHISEntitiesSqlServer dbcontext, Models.UserSelf currentOper = null)
        {
            this._db = dbcontext;
            this.CurrentOper = currentOper;
        }
    }
}
