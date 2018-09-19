using CHIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ass;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using CHIS.DbContext;

namespace CHIS.Services
{
    public class BaseService
    {
       internal  CHISEntitiesSqlServer _db;
        public BaseService(CHISEntitiesSqlServer db) 
        {
            this._db = db;
        }
         



    }
}
