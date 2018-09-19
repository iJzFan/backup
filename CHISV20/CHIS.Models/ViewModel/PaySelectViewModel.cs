using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.ViewModels
{

    public enum PayTypes
    {
       Cashe, //现金
       WX, //微信
       AliPay //支付宝
    }

   public class PaySelectViewModel
    {
        public vwCHIS_Code_Customer Customer { get; set; }
        public decimal PayAmount { get; set; }
        public PayTypes PayType { get; set; }
        public string PayRemark { get; set; }
        /// <summary>
        /// 实收现金
        /// </summary>
        public decimal CusPayMoney { get; set; }

        /// <summary>
        /// 找零金额
        /// </summary>
        public decimal ReturnMoney { get; set; }
 
     
    }
}
