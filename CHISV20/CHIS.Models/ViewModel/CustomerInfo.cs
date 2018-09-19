using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class CustomerInfo
    {
        public CHIS_Code_Customer Customer { get; set; }
        public CHIS_Code_Customer_HealthInfo CustomerHealthInf { get; set; }
    }
    public class CustomerPic
    {
        public int CustomerId { get; set; }
        public string PicUrl { get; set; }
    }
}
