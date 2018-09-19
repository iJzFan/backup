using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ass;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHIS.Models
{
 
    public partial class CHIS_Register
    {
 
        [NotMapped]
        public bool HaveRegisted { get; set; }
    }
     

}
