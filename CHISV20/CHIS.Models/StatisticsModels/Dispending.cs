using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.StatisticsModels
{
   public class Dispending
    {
        public int StationId { get; set; }
        public string StationName { get; set; }
        public int OrderNum { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ToJKJson
    {
       
        public string method { get; set; }
        public string sign { get; set; }
        public string signMethod { get; set; }
        public string cid { get; set; }
        public string randomStr { get; set; }
        public string externalOrderNo { get; set; }
        public string deliveryType { get; set; }
        public string deliveryZipCode { get; set; }
        public int money { get; set; }
        public string orderTime { get; set; }
        public string consigness { get; set; }
        public string telephone { get; set; }
        public string mobilephone { get; set;}
        public string province { get; set; }
        public string city { get; set; }
        public string isRxDrug { get; set; }
        public string invoice { get; set; }
        public string accountId { get; set; }
        public string district { get; set; }
        public string town { get; set; }
        public string address { get; set; }
        public string paymentType { get; set; }
        public string transportCosts { get; set; }
        public dynamic orderProductList { get; set; }

    }

    public class OrderProduct {
        public string productCode { get; set; }
        public string productName { get; set; }
        public int amount { get; set; }//数量
        public int actualPrice { get; set; }//单价
        public string packing { get; set; }


    }
    //订单编号
    public class OrderNoList
    {
        public string orderNo { get; set; }
    }


}
