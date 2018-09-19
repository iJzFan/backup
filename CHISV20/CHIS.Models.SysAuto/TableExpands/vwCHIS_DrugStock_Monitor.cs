using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHIS.Models
{
    public partial class vwCHIS_DrugStock_Monitor
    {

        /// <summary>
        /// 是否多单位
        /// </summary>
        [NotMapped]        
        public bool IsMultyUnit
        {
            get
            {
                return UnitBigId != UnitSmallId  && StockUnitId == UnitSmallId;               
            }
        }

        /// <summary>
        /// 多单位大单位数量
        /// </summary>
        [NotMapped]
        public decimal BigStockNumber
        {
            get
            {
                return (DrugStockNum * 1.0m) / (OutpatientConvertRate.Value * 1.0m);
            }
        }
        /// <summary>
        /// 多单位大单位数量是否有多余
        /// </summary>
        [NotMapped]
        public bool HasBigStockNumberMore
        {
            get
            {
                return BigStockNumber > (int)BigStockNumber;
            }
        }
    }
}
