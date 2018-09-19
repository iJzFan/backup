using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.ViewModels
{
    /*
    public class DispensingDetailViewModel
    {
        public Models.vwCHIS_Charge_Pay PayOrderInfo { get; set; }
        public Models.vwCHIS_Shipping_NetOrder PostPayOrderInfo { get; set; }
        public Models.vwCHIS_Code_Customer_AddressInfos CustomerAddressInfo { get; set; }
        public IQueryable<OrderDetail> OrderDetailOfLocal { get; set; }
        public IQueryable<OrderDetail> OrderDetailOfJK { get; set; }
        public IQueryable<OrderDetail> OrderDetailOfThreePart { get; set; }

        public bool HaveLocal { get { return OrderDetailOfLocal.Count() > 0; } }
        public bool HaveJKWebNet { get { return OrderDetailOfJK.Count() > 0; } }
        public bool Have3Part { get { return OrderDetailOfThreePart.Count() > 0; } }


        public decimal TotalAmountOfLocal
        {
            get
            {
                return OrderDetailOfLocal.Sum(m => m.Amout);
            }
        }
        public decimal TotalAmountOfJK
        {
            get
            {
                return OrderDetailOfJK.Sum(m => m.Amout);
            }
        }
        public decimal TotalAmountOf3Part
        {
            get
            {
                return OrderDetailOfThreePart.Sum(m => m.Amout);
            }
        }

        private Dictionary<string, List<OrderDetail>> orderDetailOfJKGrouped = null,
                                                      orderDetailOfLocalGrouped=null,
                                                      orderDetailOfThreePartGrouped=null;
        public Dictionary<string, List<OrderDetail>> OrderDetailOfJKGrouped
        {
            get
            {
                if (orderDetailOfJKGrouped == null)
                {
                    orderDetailOfJKGrouped = new Dictionary<string, List<OrderDetail>>();
                    foreach (var item in OrderDetailOfJK)
                    {
                        if (!orderDetailOfJKGrouped.ContainsKey(item.PrescriptionNo))
                            orderDetailOfJKGrouped.Add(item.PrescriptionNo, new List<OrderDetail>());
                        orderDetailOfJKGrouped[item.PrescriptionNo].Add(item);
                    }
                }
                return orderDetailOfJKGrouped;
            }          
        }

        public Dictionary<string, List<OrderDetail>> OrderDetailOfLocalGrouped
        {
            get
            {
                if (orderDetailOfLocalGrouped == null)
                {
                    orderDetailOfLocalGrouped = new Dictionary<string, List<OrderDetail>>();
                    foreach (var item in OrderDetailOfLocal)
                    {
                        if (!orderDetailOfLocalGrouped.ContainsKey(item.PrescriptionNo))
                            orderDetailOfLocalGrouped.Add(item.PrescriptionNo, new List<OrderDetail>());
                        orderDetailOfLocalGrouped[item.PrescriptionNo].Add(item);
                    }
                }
                return orderDetailOfLocalGrouped;
            }
        }
        public Dictionary<string, List<OrderDetail>> OrderDetailOfThreePartGrouped
        {
            get
            {
                if (orderDetailOfThreePartGrouped == null)
                {
                    orderDetailOfThreePartGrouped = new Dictionary<string, List<OrderDetail>>();
                    foreach (var item in OrderDetailOfThreePart)
                    {
                        if (!orderDetailOfThreePartGrouped.ContainsKey(item.PrescriptionNo))
                            orderDetailOfThreePartGrouped.Add(item.PrescriptionNo, new List<OrderDetail>());
                        orderDetailOfThreePartGrouped[item.PrescriptionNo].Add(item);
                    }
                }
                return orderDetailOfThreePartGrouped;
            }
        }
        
    }
    */
    public class OrderDetail
    {
        public string OrderNo { get; set; }
        public int CustromerId { get; set; }
        public string OrderTime { get; set; }
        public string PrescriptionNo { get; set; }
        public string DrugName { get; set; }
        public string DrugCode { get; set; }
        public string DrugType { get; set; }
        public decimal Number { get; set; }
        public string UnitName { get; set; }
        public int UnitId { get; set; }
        public decimal Price { get; set; }
        public decimal Amout { get; set; }
        public decimal? totalPrce { get; set; }
        public int? SourceFrom { get; set; }
       
    }
    public class PostSendAddress
    {
        public string mergerName { get; set; }
        public long addressId { get; set; }
        public string addressDetail { get; set; }
        public string contactName { get; set; }
        public string mobile { get; set; }
        public string zipCode { get; set; }
    }
}
