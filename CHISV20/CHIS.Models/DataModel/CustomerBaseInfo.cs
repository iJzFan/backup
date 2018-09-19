using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models
{
    public class CustomerBaseInfo
    {
        public int CustomerId { get; set; }
        public string  CustomerName {get;set;}
        public string Password { get; set; }
        public string Mobile{ get; set; }
        public string LoginName { get; set; }

    }
}
