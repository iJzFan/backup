using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class FollowListDataModel
    {
        public int CustomerId { get; set; }

        public string RecentStationIds { get; set; }

        public string RecentDoctorIds { get; set; }

        public string FollowStationIds { get; set; }

        public string FollowDoctorIds { get; set; }
    }
}
