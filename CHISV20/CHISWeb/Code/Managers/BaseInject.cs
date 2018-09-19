using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.Managers
{
    public class BaseInject
    {
        public CHIS.DbContext.CHISEntitiesSqlServer db = null;
        public CHIS.DbContext.CHISEntitiesSqlServer MainDbContext
        {
            get
            {
                return db ?? (db = new Code.Utility.DataBaseHelper().GetMainDbContext());
            }
        }



    }
}
