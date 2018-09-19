using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class GiftMainViewModel
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public long Points { get; set; }

        public PaginatedItemsViewModel<PointsDetailViewModel> PointsDetail { get; set; }
    }
}
