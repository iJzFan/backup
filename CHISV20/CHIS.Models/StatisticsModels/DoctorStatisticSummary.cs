using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.StatisticsModels
{

    /// <summary>
    /// 接诊统计信息
    /// </summary>
    public class DoctorStatisticSummary
    {
        public int DoctorId { get; set; }
        public int StationId { get; set; }
        public int TreatCountOfToday { get; set; }
        public int TreatCountOfWeek { get; set; }

        public decimal FeeAmountOfToday { get; set; }
        public decimal FeeAmountOfWeek { get; set; }

        public int DrugQtyAmountOfToday { get; set; }
        public int DrugQtyAmountOfWeek { get; set; }

        public int DrugTypeCountOfToday { get; set; }
        public int DrugTypeCountOfWeek { get; set; }

    }
}
