using Ass;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHIS.Models.DataModel
{
    public class CustomerInfo
    {
    
        public CHIS_Code_Customer Customer { get; set; }
        public CHIS_Code_Customer_HealthInfo Health { get; set; }
         
    }

    public class CustomerAndRelations
    {
        public vwCHIS_Code_Customer Customer { get; set; }
        public IEnumerable<dynamic> MyRelationships { get; set; }
    } 
 
   
}
