using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class FollowListViewModel
    {
        public int CustomerId { get; set; }

        public string RecentStationIds { get; set; }

        public string RecentDoctorIds { get; set; }

        public string FollowStationIds { get; set; }

        public string FollowDoctorIds { get; set; }
    }
}
