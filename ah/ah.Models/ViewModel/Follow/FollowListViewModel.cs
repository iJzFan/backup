using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class FollowListViewModel
    {
        public int CustomerId { get; set; }

        public int RecentStationCount { get; set; }

        public IEnumerable<int> RecentStationIds { get; set; }

        public int RecentDoctorCount { get; set; }

        public IEnumerable<int> RecentDoctorIds { get; set; }

        public int FollowStationCount { get; set; }

        public IEnumerable<int> FollowStationIds { get; set; }

        public int FollowDoctorCount { get; set; }

        public IEnumerable<int> FollowDoctorIds { get; set; }
    }
}
