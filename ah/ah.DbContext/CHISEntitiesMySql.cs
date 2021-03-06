﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ass;
using MySQL.Data.EntityFrameworkCore.Extensions;
 

namespace ah.DbContext
{
    public class CHISEntitiesMySql : Microsoft.EntityFrameworkCore.DbContext
    {
        public Ass.Data.IDbUtils DbUtils
        {
            get { return new Ass.Data.MySqlDbUtils(); }
        }


        private string _connstr = null;
        public CHISEntitiesMySql(string connstring) {
            this._connstr = connstring;
        }

        public DbSet<ah.Models.CHIS_Code_Customer> CHIS_Code_Customer { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseMySQL(_connstr);
    }
}
