using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.DataModel
{
    public class CustomerClaimData
    {
        public long LoginId { get; set; }
        public int CustomerId { get; set; }
        public int? DoctorId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }
        public int? Gender { get; set; }
        public string IDcard { get; set; }
        public string CustomerPWD { get; set; }
        public DateTime CreatTime { get; set; }
 
    }

    public class AHMSLoginClaimData
    {
        public long LoginId { get; set; }
        public int CustomerId { get; set; }
        public int? DoctorId { get; set; }

        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }
        public string IDcard { get; set; }
        public int? Gender { get; set; }
        public DateTime CreatTime { get; set; }

    }

}
