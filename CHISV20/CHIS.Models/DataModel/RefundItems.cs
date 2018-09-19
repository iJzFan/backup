using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.DataModels
{
    public class RefundItems
    {
        public RefundItems()
        {
            AdviceIds = new List<long>();
            CheckIds = new List<long>();
            TestIds = new List<long>();
        }

        public string PayOrderId { get; set; }
        public long TreatId { get; set; }
        public string FeeTypeCode { get; set; }
        public string RefundRemark { get; set; }
        /// <summary>
        /// 预计可退款总金额
        /// </summary>
        public decimal RefundAmount { get; set; }

        public List<long> AdviceIds { get; set; }
        public List<long> CheckIds { get; set; }
        public List<long> TestIds { get; set; }


        //    /// <summary>
        //    /// 退款的RefundKey用于标识该退款的唯一性 32位
        //    /// </summary>
        //    public string RefundKey
        //    {
        //        get
        //        {
        //            AdviceIds.Sort();
        //            CheckIds.Sort();
        //            TestIds.Sort();
        //            string a = string.Join(",", AdviceIds);
        //            string b = string.Join(",", CheckIds);
        //            string c = string.Join(",", TestIds);
        //            string str = string.Format("{0}|{1}|{2}|{3}", PayOrderId, a, b, c);
        //            return Ass.Encrypt.EncryptTool.Md5Hash(str);
        //        }
        //    }
        //}

        public class RefundInfo
        {
            public List<RefundItems> CashList { get; set; }
            public decimal CashTotalAmount
            {
                get
                {
                    return CashList.Sum(m => m.RefundAmount);
                }
            }
            public List<RefundItems> AliPayList { get; set; }
            public decimal AliPayTotalAmount
            {
                get
                {
                    return AliPayList.Sum(m => m.RefundAmount);
                }
            }
            public List<RefundItems> WeChatList { get; set; }
            public decimal WeChatTotalAmount
            {
                get
                {
                    return WeChatList.Sum(m => m.RefundAmount);
                }
            }

            public decimal TotalAmount
            {
                get { return CashTotalAmount + AliPayTotalAmount + WeChatTotalAmount; }
            }

            public RefundInfo(IEnumerable<RefundItems> items)
            {
                CashList = new List<RefundItems>();
                AliPayList = new List<RefundItems>();
                WeChatList = new List<RefundItems>();
                foreach (var item in items)
                {
                    if (item.FeeTypeCode == FeeTypes.Cash)
                    {
                        CashList.Add(item);
                    }
                    if (item.FeeTypeCode == FeeTypes.WeChat_Pub || item.FeeTypeCode == FeeTypes.WeChat_QR)
                    {
                        WeChatList.Add(item);
                    }
                    if (item.FeeTypeCode == FeeTypes.AliPay_QR)
                    {
                        AliPayList.Add(item);
                    }
                }
                CashList.Sort((RefundItems m1, RefundItems m2) =>
                {
                    return m1.PayOrderId.CompareTo(m2.PayOrderId);
                });
                WeChatList.Sort((RefundItems m1, RefundItems m2) =>
                {
                    return m1.PayOrderId.CompareTo(m2.PayOrderId);
                });
                AliPayList.Sort((RefundItems m1, RefundItems m2) =>
                {
                    return m1.PayOrderId.CompareTo(m2.PayOrderId);
                });
            }

        }

        public class RefundItemsHelper
        {
            private List<RefundItems> lst = new List<RefundItems>();

            public decimal TotalRefundAmount
            {
                get
                {
                    return lst.Sum(m => m.RefundAmount);
                }
            }

            public List<RefundItems> ToList()
            {
                return lst;
            }

            //public void AddItem(vwCHIS_Charge_Pay_Detail payDetail)
            //{
            //    var find = lst.Where(m => m.PayOrderId == payDetail.PayOrderId).FirstOrDefault();
            //    if (find == null)
            //    {
            //        lst.Add(new RefundItems
            //        {
            //            PayOrderId = payDetail.PayOrderId,
            //            FeeTypeCode = payDetail.FeeTypeCode,
            //            TreatId = payDetail.TreatId
            //        });
            //        find = lst.Where(m => m.PayOrderId == payDetail.PayOrderId).FirstOrDefault();
            //    }



            //    bool badd = false;
            //    if (payDetail.AdviceId > 0)
            //    {
            //        if (!find.AdviceIds.Contains(payDetail.AdviceId.Value))
            //        {
            //            find.AdviceIds.Add(payDetail.AdviceId.Value);
            //            badd = true;
            //        }
            //    }
            //    else if (payDetail.CheckId > 0)
            //    {
            //        if (!find.CheckIds.Contains(payDetail.CheckId.Value))
            //        {
            //            find.CheckIds.Add(payDetail.CheckId.Value);
            //            badd = true;
            //        }
            //    }
            //    else if (payDetail.TestId > 0)
            //    {
            //        if (!find.TestIds.Contains(payDetail.TestId.Value))
            //        {
            //            find.TestIds.Add(payDetail.TestId.Value);
            //            badd = true;
            //        }
            //    }

            //    //如果需要添加
            //    if (badd) find.RefundAmount += payDetail.Amount;
            //}
        }
    }
}
