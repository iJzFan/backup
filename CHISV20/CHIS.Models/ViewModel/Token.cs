using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class Token
    {
        public string AccessToken { get; set; }

        public DateTime ExpiresTime { get; set; }
    }
}
