using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Codes.Utility
{
    /// <summary>
    /// 支付接口
    /// </summary>
    public interface IxPay<T>
    {

        /// <summary>
        /// 获取支付二维码
        /// </summary>
        /// <param name="nonce_str">本次支付的随机字符串 注意，如果是同一次支付，则随机字符串固定32位</param>
        /// <param name="body">药品描述</param>
        /// <param name="out_trade_no">商户系统内部的订单号 需要保持唯一性</param>
        /// <param name="total_fee">订单总金额，单位为分</param>
        /// <param name="notify_url">接收微信支付异步通知回调地址，通知url必须为直接可访问的url，不能携带参数</param>
        /// 
        /// <param name="detail">药品详情</param>
        /// <param name="fee_type">符合ISO 4217标准的三位字母代码，默认人民币：CNY</param>
        /// <returns></returns>
        Task<dynamic> Get2DCodeAsync(string nonce_str, string body,
            string out_trade_no, int total_fee,
            string notify_url,
            IEnumerable<T> detail = null,
            string fee_type = "CNY");


        /// <summary>
        ///   查询支付订单支付情况
        /// </summary>
        /// <param name="out_trade_no">订单号</param>
        /// <param name="payType">类别ALIQR,WXQR,WXPUB,WXH5</param>
        /// <returns></returns>
        Task<dynamic> QueryPayStatus(string out_trade_no,string payType);  
    }
}
