using System;
using System.Collections.Generic;
using System.Text;
using Ass;

namespace CHIS.Models.ViewModel
{
    public class DrugSelectItem
    {
        public string StockFromId { get; set; }
        public string DefDrugImg { get; set; }
        public int DrugId { get; set; }
        public String DrugName { get; set; }
        public String DrugModel { get; set; }
        public String DrugPinYin { get; set; }
        public String Alias { get; set; }
        public String DrugOrigMf { get; set; }
        public String StockUnitName { get; set; }
        public String CanUseUnitNames { get; set; }
        public decimal StockSalePrice { get; set; }
        /// <summary>
        /// 大类
        /// </summary>
        public string MedKindCode { get; set; }

        //拼音缩写码
        public string PyCode { get; set; }


        /// <summary>
        /// [A-Z]以及下划线_
        /// </summary>
        public char FirstLetter
        {
            get
            {
                char rtn = '_';
                if (DrugPinYin.IsNotEmpty()) rtn = DrugPinYin[0].ToString().ToUpper()[0];
                if (!new System.Text.RegularExpressions.Regex("[A-Z]").IsMatch(rtn.ToString())) return '_';
                return rtn;
            }
        }
    }

    public class SelectDrugsViewModel
    {
        public List<int> SelectedDrugs { get; set; }        
    }
    public class DrugStockIndexItem
    {
        /// <summary>
        /// 药品Id
        /// </summary>
        public int DrugId { get; set; }
        /// <summary>
        /// 库存来源
        /// </summary>
        public string StockFromId { get; set; }
    }
    
}
