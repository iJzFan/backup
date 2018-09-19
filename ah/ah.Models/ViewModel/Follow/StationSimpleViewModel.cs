using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    //public class StationSimpleViewModel
    //{
    //    public StationSimpleViewModel()
    //    {
    //        Items = new List<StationSimpleInfo>();
    //    }

    //    public bool Rlt { get; set; }

    //    public string Code { get; set; }

    //    public string Msg { get; set; }

    //    public string State { get; set; }

    //    public string Message { get; set; }

    //    public IEnumerable<StationSimpleInfo> Items { get; set; }

    //}

    public class StationSimpleInfo
    {
        public int StationId { get; set; }

        public string StationName { get; set; }

        public string StationImgUrl { get; set; }

        public bool? IsFollow { get; set; }
    }
}
