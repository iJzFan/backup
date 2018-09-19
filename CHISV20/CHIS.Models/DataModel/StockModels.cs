/*******************************************************************************
 * Copyright © 2017 ah 版权所有
 * Author: Rex
 * Description: 操作Model 本类存放简单数据类型
*********************************************************************************/
using CHIS.Models;
using System;
using Ass;

namespace CHIS.Models.DataModel
{
   public class MyDrugIncomePrice
    {
        public int DrugId { get; set; }
        public decimal? IncomePrice { get; set; }
        public int InUnitId { get; set; }
    }
}
