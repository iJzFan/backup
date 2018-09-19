using Ass;
using CHIS.Models;
using CHIS.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace CHIS.Controllers.Charge
{
    public partial class ChargeController
    {

        /// <summary>
        /// 获取用户需要支付的清单
        /// </summary>
        /// <param name="customerId">用户Id</param>
        /// <param name="dt0">时间起始限定</param>
        /// <param name="dt1">时间结束限定</param> 
        [AllowAnonymous]
        [EnableCors("jk213Origin")]//允许跨域
        public IActionResult GetCustomerNeedPayList(int customerId, DateTime? dt0, DateTime? dt1)
        {
           // HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*"; //允许跨域访问    
            return TryCatchFunc(dd =>
            {
                base.initialData_TimeRange(ref dt0, ref dt1);
                dd.items = _paySvr.GetChargeListNeedPayModelByCustomerId(customerId, dt0.Value, dt1.Value);

                return null;
            });

        }

        /// <summary>
        /// 待支付明细
        /// </summary>
        [AllowAnonymous]
        [EnableCors("jk213Origin")]//允许跨域
        public async Task<IActionResult> GetTreatNeedPayInfo(long treatId)
        {    
            return await TryCatchFuncAsync(async(dd) =>
            {
                List<WXPayDetailItem> rtn = new List<WXPayDetailItem>();
                var model = await _paySvr.GetNeedPayInfoAsync(treatId);

                //整理成简单明细
                //成药
                foreach (var key in model.FormedPrescriptions.Keys)
                {
                    foreach (var item in model.FormedPrescriptions[key])
                    {
                        rtn.Add(new WXPayDetailItem
                        {
                            Amount = item.Amount,
                            Content = $"{item.DrugName}({item.DrugModel}) ({item.UnitName}) X {item.Qty}"
                        });
                    }
                }
                //中药
                foreach (var key in model.HerbPrescriptions.Keys)
                {
                    foreach (var item in model.HerbPrescriptions[key])
                    {
                        rtn.Add(new WXPayDetailItem
                        {
                            Amount = item.Amount,
                            Content = $"{item.DrugName} ({item.UnitName}) X {item.Qty}"
                        });
                    }
                }
                //附加费
                foreach (var item in model.ExtraFees)
                {
                    rtn.Add(new WXPayDetailItem
                    {
                        Amount = item.Amount,
                        Content = $"{item.FeeName} (项) X {item.Qty}"
                    });
                }
                dd.items = rtn;
                return null;
            });
        }


        /// <summary>
        /// 生成微信支付预支付订单信息
        /// </summary>
        /// <param name="treatId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [EnableCors("jk213Origin")]//允许跨域
        public IActionResult CreatePrePayInfoOfWX(long treatId, decimal amount)
        { 
            var r = CreateWXPubPayInfo(treatId, "微信公众号调用", amount);
            return Json(r);
        }

        [AllowAnonymous]
        [EnableCors("jk213Origin")]//允许跨域
        public async Task<IActionResult> UpdatepayPreOfWXPub(string payOrderId)
        { 
            return await TryCatchFuncAsync(async () =>
            {
               if (payOrderId.IsEmpty()) throw new Exception("请传入订单号");
               await _paySvr.UpdatePayedAsync(payOrderId,FeeTypes.WeChat_Pub,null,false,0,"SYSTEM");
               return null;
            });

        }



        /// <summary>
        /// 获取客户已经支付的信息 默认7天内
        /// </summary>  
        [AllowAnonymous]
        [EnableCors("jk213Origin")]//允许跨域
        public IActionResult GetCustomerPayedList(int customerId, DateTime? dt0, DateTime? dt1,int pageIndex=1,int pageSize=20)
        {   
            return TryCatchFunc(dd =>
            {
                base.initialData_TimeRange(ref dt0, ref dt1);
                if ((dt1.Value - dt0.Value).TotalDays > 366) throw new Exception("时间范围不能大于一年");
                dd.items = _paySvr.GetChargeListPayedModelByCustomerId(customerId, dt0.Value, dt1.Value,pageIndex,pageSize);
                dd.pageIndex = pageIndex;
                dd.pageSize = pageSize;
                return null;
            });
        }

        /// <summary>
        /// 获取客户已经支付的信息 默认7天内
        /// </summary>  
        [AllowAnonymous]
        [EnableCors("jk213Origin")]//允许跨域
        public IActionResult GetCustomerPayedDetail(long payId)
        {  
            return TryCatchFunc(dd =>
            {
                var model = _paySvr.GetPayedInfo(payId);
                List<WXPayDetailItem> rtn = new List<WXPayDetailItem>();
                //整理成简单明细
                //成药
                foreach (var key in model.FormedPrescriptions.Keys)
                {
                    foreach (var item in model.FormedPrescriptions[key])
                    {
                        rtn.Add(new WXPayDetailItem
                        {
                            Amount = item.Amount,
                            Content = $"{item.DrugName}({item.DrugModel}) ({item.UnitName}) X {item.Qty}"
                        });
                    }
                }
                //中药
                foreach (var key in model.HerbPrescriptions.Keys)
                {
                    foreach (var item in model.HerbPrescriptions[key])
                    {
                        rtn.Add(new WXPayDetailItem
                        {
                            Amount = item.Amount,
                            Content = $"{item.DrugName} ({item.UnitName}) X {item.Qty}"
                        });
                    }
                }
                //附加费
                foreach (var item in model.ExtraFees)
                {
                    rtn.Add(new WXPayDetailItem
                    {
                        Amount = item.Amount.Value,
                        Content = $"{item.FeeName} (项) X {item.Qty}"
                    });
                }
                dd.items = rtn;
                return null;
            });
        }


    }
}
