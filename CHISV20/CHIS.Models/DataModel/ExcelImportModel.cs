using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.DataModel
{
    /// <summary>
    /// Excel导入入库数据模型
    /// </summary>
    public class ExcelIncomeImportModel
    {
        public int DrugId { get; set; }
        public string DrugName { get; set; }
        /// <summary>
        /// 别名
        /// </summary>
        public string DrugAlias { get; set; }
        public string DrugModel { get; set; }
        public string ManufacturerOrigin { get; set; }
        public string DrugOrigPlace { get; set; }
        public string BatNo { get; set; }
        public DateTime ProduceTime { get; set; }
        public DateTime DeadlineTime { get; set; }
        public decimal? IncomePrice { get; set; }
        public int IncomeNum { get; set; }
        public string IncomeUnitName { get; set; }

        public int IncomeUnitId { get; set; }


        private decimal? _incomePriceSmall, _incomePriceBig, unitConvertRate;
        public decimal? incomePriceSmall
        {
            get
            {
                if (_incomePriceSmall == null && _incomePriceBig.HasValue && UnitConvertRate.HasValue)
                {
                    _incomePriceSmall = _incomePriceBig / UnitConvertRate;
                }
                return _incomePriceSmall;
            }
            set { _incomePriceSmall = value; }
        }
        public decimal? incomePriceBig
        {
            get
            {
                if (_incomePriceBig == null && _incomePriceSmall.HasValue && UnitConvertRate.HasValue)
                {
                    _incomePriceBig = _incomePriceSmall * UnitConvertRate;
                }
                return _incomePriceBig;
            }
            set { _incomePriceBig = value; }
        }

        public int? outUnitSmallId { get; set; }
        public int? outUnitBigId { get; set; }
        public string outUnitBigName { get; set; }
        public string outUnitSmallName { get; set; }

        public decimal? UnitConvertRate
        {
            get { return unitConvertRate; }
            set { unitConvertRate = value;_incomePriceBig = null;_incomePriceSmall = null; }
        }



        /// <summary>
        /// 检查数据 错误则抛错
        /// </summary> 
        public bool CheckData()
        {
            List<string> exps = new List<string>();
            if (this.DrugId == 0) exps.Add("药品Id不能为空");
            if (string.IsNullOrWhiteSpace(this.BatNo)) exps.Add("批号不能为空");
            if (ProduceTime == new DateTime()) exps.Add("生产日期不能为空");
            if (DeadlineTime == new DateTime()) exps.Add("有效截止日期不能为空");
            if (IncomeNum < 1) exps.Add("入库数量必须大于0");
            if (IncomeUnitId == 0) exps.Add($"必须有入库单位Id,可能是单位({this.IncomeUnitName})换算成Id没有找到");
            if (exps.Count == 0) return true;
            throw new Exception(string.Join(";", exps));
        }
    }





}
