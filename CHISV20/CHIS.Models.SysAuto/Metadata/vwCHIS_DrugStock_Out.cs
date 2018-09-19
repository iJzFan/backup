using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ass;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHIS.Models
{
 
    public partial class vwCHIS_DrugStock_Out
    {

        public vwCHIS_DrugStock_Out()
        {

        }
        [NotMapped]
        public string PayOrderId { get; set; }
    }
     

}
