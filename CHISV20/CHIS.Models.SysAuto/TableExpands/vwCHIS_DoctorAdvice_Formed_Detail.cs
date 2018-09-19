using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHIS.Models
{
    public partial class vwCHIS_DoctorAdvice_Formed_Detail
    {

        public vwCHIS_DoctorAdvice_Formed_Detail()
        {
            OutpatientConvertRate = 1;
        }

        [NotMapped]
        public decimal PriceBigUnit { get; set; }

        /// <summary>
        /// 是否多单位
        /// </summary>
        [NotMapped]
        public bool IsMultyUnit
        {
            get
            {
                return UnitBigId != UnitSmallId;
            }

        }
        //
        [NotMapped]
        public decimal BigStockNumber
        {
            get
            {
                return (Qty * 1.0m) / (OutpatientConvertRate.Value * 1.0m);
            }
        }
        //取模
        [NotMapped]
        public decimal QtyModel
        {
            get
            {
                return (Qty * 1.0m) % (OutpatientConvertRate.Value * 1.0m);
            }
        }

        /// <summary>
        /// 合计大包小包
        /// </summary>
        public string PackageShow
        {
            get
            {
                var bigNum = (int)BigStockNumber;
                var minNum = QtyModel;
                var a = "";
                if (bigNum > 0) a = bigNum + this.OutUnitBigName;
                if (minNum > 0) a += minNum.ToString("#0.##") + this.OutUnitSmallName;
                return a;
            }
        }
    }

}
