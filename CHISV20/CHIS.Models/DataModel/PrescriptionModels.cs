using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.DataModel
{
    public class PrescriptionSearchItem
    {
        public long TreatId { get; set; }
        public Guid PrescriptionId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerGender { get; set; }
        public string CustomerAge { get; set; }

        public DateTime TreatTime { get; set; }
        /// <summary>
        /// 处方类型
        /// </summary>
        public string PrescriptionTypeName { get; set; }
    }
}
