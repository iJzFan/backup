using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ah.Models.ViewModel
{
    public class AppointmentViewModel
    {
        public int? StationId { get; set; }
        public string StationName { get; set; }
        public int? DepartId { get; set; }
        public int? DoctorId { get; set; }
        public string Date { get; set; }
        public int Slot { get; set; }
    }

}
