using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.DataModels
{
    public class PayPreMainDataModel
    {
        public decimal TotalAmount { get; set; }
        public string PayOrderId { get; set; }
        public DateTime CreateTime { get; set; }
        public string FeeTypeCode { get; set; }
    }
    
}
