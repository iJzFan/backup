using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHIS.DbContext;
using CHIS.Models.ViewModel;

namespace CHIS.Services
{
    public class FollowListService:BaseService
    {
        public FollowListService(CHISEntitiesSqlServer db) : base(db)
        {
        }

        public FollowListViewModel Get(int customerId)
        {
            var model = _db.CHIS_Customer_FollowList.SingleOrDefault(x=>x.CustomerId==customerId);

            if (model != null)
            {
                return new FollowListViewModel
                {
                    CustomerId = model.CustomerId,
                    RecentDoctorIds = model.RecentDoctorIds,
                    RecentStationIds = model.RecentStationIds,
                    FollowDoctorIds = model.FollowDoctorIds,
                    FollowStationIds = model.FollowStaionIds
                };
            }

            return new FollowListViewModel();
        }

        public void Update(FollowListViewModel followlist)
        {
            var model = _db.CHIS_Customer_FollowList.SingleOrDefault(x => x.CustomerId == followlist.CustomerId);

            if (model == null)
            {
                _db.CHIS_Customer_FollowList.Add(new Models.CHIS_Customer_FollowList
                {
                    CustomerId = followlist.CustomerId,
                    FollowDoctorIds = followlist.FollowDoctorIds,
                    FollowStaionIds = followlist.FollowStationIds,
                    RecentDoctorIds = followlist.RecentDoctorIds,
                    RecentStationIds = followlist.RecentStationIds
                });

                _db.SaveChanges();

                return;
            }

            model.FollowDoctorIds = followlist.FollowDoctorIds;
            model.FollowStaionIds = followlist.FollowStationIds;
            model.RecentStationIds = followlist.RecentStationIds;
            model.RecentDoctorIds = followlist.RecentDoctorIds;

            _db.SaveChanges();
        }
    }
}
