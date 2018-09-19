using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class MyFollowsViewModel
    {
        public MyFollowsViewModel()
        {
            Doctors = new List<DoctorSimpleInfo>();
            Stations = new List<StationSimpleInfo>();
        }

        public IEnumerable<DoctorSimpleInfo> Doctors { get; set; }

        public IEnumerable<StationSimpleInfo> Stations { get; set; }
    }
}
