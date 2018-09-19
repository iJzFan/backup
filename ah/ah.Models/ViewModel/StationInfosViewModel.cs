using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    //public class StationInfosViewModel
    //{
    //    public int TotalPages { get; set; }

    //    public int PageIndex { get; set; }

    //    public int PageSize { get; set; }

    //    public bool Rlt { get; set; }

    //    public string Msg { get; set; }

    //    public string State { get; set; }

    //    public string Message { get; set; }

    //    public IEnumerable<StationInfo> Items { get; set; }

    //}

    public class StationInfo
    {
        public int StationId { get; set; }
        /// <summary>
        /// 工作站名
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public double? Lng { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string StationAddress { get; set; }

        /// <summary>
        /// 工作站横图
        /// </summary>
        public string StationPicHUrl { get; set; }
        /// <summary>
        /// 工作站纵向图
        /// </summary>
        public string StationPicVUrl { get; set; }
        /// <summary>
        /// 工作站介绍
        /// </summary>
        public string StationRmk { get; set; }

        public bool? IsFollow { get; set; }


        /// <summary>
        /// 距离多少米
        /// </summary>
        public string DiffOfMe{ get; set; }

    }
}
