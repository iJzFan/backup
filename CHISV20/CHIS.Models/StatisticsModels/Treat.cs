using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.StatisticsModels
{

    /// <summary>
    /// 接诊统计信息
    /// </summary>
    public class TreatBasicSummary
    {
        public string name { get; set; }
        public int waiting { get; set; }
        public int treating { get; set; }
        public int treated { get; set; }
        public int seq { get; set; }
    }
}
