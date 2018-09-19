using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class CustomerInfo
    {
        public ah.Models.CHIS_Code_Customer Customer { get; set; }
        public ah.Models.CHIS_Code_Customer_HealthInfo CustomerHealthInf { get; set; }
    }


    /// <summary>
    /// 存放到公共cookie里面的customerInfo
    /// </summary>
    public class CUSTOMER_INFO
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int CustomerId { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public int? Gender { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime? Birthday { get; set; }
        public int? MariageStatusId { get; set; }
        /// <summary>
        /// 婚况
        /// </summary>
        public string MariageStatusName { get; set; }






        public string LoginType { get; set; }
    }


}
