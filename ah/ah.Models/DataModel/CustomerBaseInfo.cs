using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models
{
    public class CustomerBaseInfo
    {
        public long LoginId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? Gender { get; set; }
        public string Password { get; set; }
        public string Mobile { get; set; }
        
        public string Email { get; set; }

        public string IdCard { get; set; }

        public string OrigPwd { get; set; }
        public string NewPwd { get; set; }
        
        public string NewPwdConfirm { get; set; }
 
    }
}
