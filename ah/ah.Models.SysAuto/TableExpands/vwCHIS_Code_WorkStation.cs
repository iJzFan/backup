using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ah.Models
{

    public partial class vwCHIS_Code_WorkStationEx
    {

        /// <summary>
        /// 该节点禁止使用
        /// </summary>
        public bool IsForbiddenNode
        {
            get { return sysIsOriginalNode != 1; }
        }

        /// <summary>
        /// 默认工作站图片
        /// </summary>
        public string DefStationPic
        {
            get
            {
                if (string.IsNullOrEmpty(StationPic))
                {
                    if (IsManageUnit) return "_stationmgr.jpg";
                    else return "_station.jpg";
                }
                else return StationPic;
            }
        }
    }
    public partial class vwCHIS_Code_WorkStation
    {



        /// <summary>
        /// 默认工作站图片
        /// </summary>
        public string DefStationPic
        {
            get
            {
                if (string.IsNullOrEmpty(StationPic))
                {
                    if (IsManageUnit) return "_stationmgr.jpg";
                    else return "_station.jpg";
                }
                else return StationPic;
            }
        }


        /// <summary>
        /// 默认工作站图片（横图)
        /// </summary>
        public string DefStationPicH
        {
            get
            {
                if (string.IsNullOrEmpty(StationPic))
                {
                    if (IsManageUnit) return "_stationmgr_h.jpg";
                    else return "_station_h.jpg";
                }
                else return StationPic;
            }
        }
    }
}
