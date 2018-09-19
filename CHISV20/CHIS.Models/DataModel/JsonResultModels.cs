/*******************************************************************************
 * Copyright © 2017 ah 版权所有
 * Author: Rex
 * Description: 操作Model 本类存放简单数据类型
*********************************************************************************/
using CHIS.Models;
using System;
using Ass;

namespace CHIS.Models
{
    public class BaseJsonRlt
    {
        public bool rlt { get; set; }
        public string msg { get; set; }
        public string status { get; set; }
    }
    public class PayQrInfo : BaseJsonRlt
    {
        public string payOrderId { get; set; }
        public string wx2DCodeUrl { get; set; }
        public string ali2DCodeUrl { get; set; }
        public string union2DCodeUrl { get; set; }
        public int totalAmount { get; set; }
        public bool isAllowedCashPay { get; set; }
        public string customerName { get; set; }
    }

    public class PayWxH5Info : BaseJsonRlt
    {
        public string wxH5Url { get; set; }
        public string payOrderId { get; set; }
        public int totalAmount { get; set; }
    }
}
