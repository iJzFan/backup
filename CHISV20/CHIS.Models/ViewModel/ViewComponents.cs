using System;
using System.Collections.Generic;
using System.Text;
using Ass;

namespace CHIS.Models.ViewModel
{
    public class TdCustomerModel
    {
        public string CustomerName { get; set; }
        public int? Gender { get; set; }
        public DateTime? BirthDay { get; set; }

        public string GenderText { get { return Gender?.ToGenderString(); } }
        public string BirthDayText { get { return BirthDay?.ToAgeString(); } }
    }
}
