using Alipay.AopSdk.AspnetCore;
using Alipay.AopSdk.F2FPay.AspnetCore;
using Alipay.AopSdk.F2FPay.Business;
using Alipay.AopSdk.F2FPay.Domain;
using Alipay.AopSdk.F2FPay.Model;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CHIS.Codes.Utility.XPay
{
    /// <summary>
    /// 支付宝支付
    /// </summary>
    public class AliPay : IxPay<GoodsInfo>
    {

        private readonly IAlipayF2FService _alipayF2FService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private IAlipayService _alipayService;
        public AliPay(IHostingEnvironment hostingEnvironment, IAlipayF2FService alipayF2FService, IAlipayService alipayService)
        {
            _hostingEnvironment = hostingEnvironment;
            _alipayF2FService = alipayF2FService;
            _alipayService = alipayService;
        }

        //获取支付宝账号的基本信息
        /// <summary>
        /// 获取支付宝支付二维码
        /// </summary>
        /// <param name="nonce_str">本次支付的随机字符串 注意，如果是同一次支付，则随机字符串固定32位</param>
        /// <param name="body">药品描述</param>
        /// <param name="out_trade_no">商户系统内部的订单号 需要保持唯一性</param>
        /// <param name="total_fee">订单总金额，单位为分</param>
        /// <param name="notify_url">接收支付宝支付异步通知回调地址，通知url必须为直接可访问的url，不能携带参数</param>
        /// 
        /// <param name="detail">药品详情</param>
        /// <param name="fee_type">符合ISO 4217标准的三位字母代码，默认人民币：CNY</param>
        /// <returns></returns>
        public async Task<dynamic> Get2DCodeAsync(string nonce_str, string body,
            string out_trade_no, int total_fee,
            string notify_url,
            IEnumerable<GoodsInfo> detail = null,
            string fee_type = "CNY")
        {
            try
            {

                AlipayTradePrecreateContentBuilder builder = new AlipayTradePrecreateContentBuilder();
                //收款账号
                builder.seller_id = _alipayService.Options.Uid;
                //订单编号
                builder.out_trade_no = out_trade_no+"_ALIQR";
                //订单总金额
                builder.total_amount = (total_fee/100m).ToString("#0.00");//注意支付宝的总金额是元为基准的，腾讯是分为基准的
                //参与优惠计算的金额
                //builder.discountable_amount = "";
                //不参与优惠计算的金额
                //builder.undiscountable_amount = "";
                //订单名称
                builder.subject = "东莞天使健康平台账单";
                //自定义超时时间
                builder.timeout_express = "5m";
                //订单描述
                builder.body = body;
                //门店编号，很重要的参数，可以用作之后的营销
                builder.store_id = "tsjkit";
                //操作员编号，很重要的参数，可以用作之后的营销
                builder.operator_id = "admin";

                //传入商品信息详情
                if (detail != null && detail.Count() > 0)
                {
                    builder.goods_detail = detail.ToList();
                }

              
                AlipayF2FPrecreateResult precreateResult = _alipayF2FService.TradePrecreate(builder, notify_url);
                await Task.Delay(10);

                switch (precreateResult.Status)
                {
                    case ResultEnum.SUCCESS:
                        return new
                        {
                            rlt = true,
                            msg = "",
                            code_url = precreateResult.response.QrCode,
                            //prepay_id = precreateResult.response.sub,
                            //trade_type = precreateResult.response.,
                            payOrderId = precreateResult.response.OutTradeNo.Replace("_ALIQR",""),
                            total_fee = total_fee
                        };
                    case ResultEnum.FAILED:
                        throw new Exception(precreateResult.response.Msg);                                    
                    case ResultEnum.UNKNOWN:
                        throw new Exception("生成二维码失败：" + (precreateResult.response == null ? "配置或网络异常，请检查后重试" : "系统异常，请更新外部订单后重新发起请求"));
                    default:throw new Exception("未知返回状态");
                }                 
            }
            catch (Exception ex)
            {
                if (ex is PayException)
                {
                    var exx = ex as PayException;
                    return new { rlt = false, msg = ex.Message, errId = exx.ExceptionId, payOrderId = out_trade_no };
                }
                return new { rlt = false, msg = ex.Message };
            }
        }






        /// <summary>
        ///   查询支付订单支付情况
        /// </summary>
        /// <param name="out_trade_no">订单号</param>
        /// <param name="payType">类别ALIQR,WXQR,WXPUB,WXH5</param>
        /// <returns></returns>
        public async Task<dynamic> QueryPayStatus(string out_trade_no,string payType)
        {
            try
            {
                AlipayF2FQueryResult queryResult = new AlipayF2FQueryResult();                  
                queryResult = _alipayF2FService.TradeQuery($"{out_trade_no}_{payType}");
                await Task.Delay(10);
                if (queryResult?.Status == ResultEnum.SUCCESS)
                {
                    return new { rlt = true, msg = "" };                    
                }
                throw new CodeException(queryResult?.Status.ToString(), queryResult?.response.Msg);
            }
            catch (Exception ex)
            {
                return new { rlt = false, msg = ex.Message };
            }

        }

        /// <summary>
        /// 获取Xml数据
        /// </summary>
        /// <returns></returns>
        internal XmlResult GetXmlResult(Stream body)
        {
            XmlResult rtn = new XmlResult();
            try
            {
                if (body == null) throw new Exception("没有传入xml数据流。");
                XDocument xdoc = XDocument.Load(body);
                foreach (var node in xdoc.Root.Elements())
                {
                    if (node.Value.Contains("CDATA"))
                    {
                        var dt = node.DescendantNodes().ToList()[0] as XCData;
                        rtn.Set(node.Name.LocalName, dt.Value);
                    }
                    else rtn.Set(node.Name.LocalName, node.Value);
                }
            }
            catch (Exception ex) { rtn.Set("err_msg", ex.Message); }
            return rtn;
        }


        //        public static string GetMD5(string s, string _input_charset)
        //        {
        //            / < summary >
        //            / 与ASP兼容的MD5加密算法
        //            / </ summary >

        //            byte[] t = ComputeHash(Encoding.GetEncoding(_input_charset).GetBytes(s));
        //            StringBuilder sb = new StringBuilder(32);
        //            for (int i = 0; i < t.Length; i++)
        //            {
        //                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
        //            }
        //            return sb.ToString();
        //        }

        //        public static string[] BubbleSort(string[] r)
        //        {
        //            / < summary >
        //            / 冒泡排序法
        //            / </ summary >

        //            int i, j; //交换标志   
        //            string temp;

        //            bool exchange;

        //            for (i = 0; i < r.Length; i++) //最多做R.Length-1趟排序   
        //            {
        //                exchange = false; //本趟排序开始前，交换标志应为假  

        //                for (j = r.Length - 2; j >= i; j--)
        //                {
        //                    if (System.String.CompareOrdinal(r[j + 1], r[j]) < 0)　//交换条件  
        //                    {
        //                        temp = r[j + 1];
        //                        r[j + 1] = r[j];
        //                        r[j] = temp;

        //                        exchange = true; //发生了交换，故将交换标志置为真   
        //                    }
        //                }

        //                if (!exchange) //本趟排序未发生交换，提前终止算法   
        //                {
        //                    break;
        //                }

        //            }
        //            return r;
        //        }
        //        public static String Get_Http(String a_strUrl, int timeout)
        //        {
        //            string strResult;
        //            try
        //            {

        //                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(a_strUrl);
        //                myReq.Timeout = timeout;
        //                HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
        //                Stream myStream = HttpWResp.GetResponseStream();
        //                StreamReader sr = new StreamReader(myStream, Encoding.Default);
        //                StringBuilder strBuilder = new StringBuilder();
        //                while (-1 != sr.Peek())
        //                {
        //                    strBuilder.Append(sr.ReadLine());
        //                }

        //                strResult = strBuilder.ToString();
        //            }
        //            catch (Exception exp)
        //            {

        //                strResult = "错误：" + exp.Message;
        //            }

        //            return strResult;
        //        }




        //        public string CreatUrl(
        //            string gateway,
        //            string service,
        //            string partner,
        //            string sign_type,
        //            string out_trade_no,
        //            string subject,
        //            string body,
        //            string payment_type,
        //            string total_fee,
        //            string show_url,
        //            string seller_email,
        //            string key,
        //            string return_url,
        //            string _input_charset,
        //            string notify_url,
        //            string extra_common_param = ""
        //            )
        //        {
        //            return CreatUrl(
        //            gateway,
        //            service,
        //            partner,
        //            sign_type,
        //           out_trade_no,
        //            subject,
        //            body,
        //            payment_type,
        //           total_fee,
        //           show_url,
        //           seller_email,
        //           key,
        //           return_url,
        //           _input_charset,
        //           notify_url,
        //           "",
        //           extra_common_param);
        //        }



        //        public string CreatUrl(
        //            string gateway,
        //            string service,
        //            string partner,
        //            string sign_type,
        //            string out_trade_no,
        //            string subject,
        //            string body,
        //            string payment_type,
        //            string total_fee,
        //            string show_url,
        //            string seller_email,
        //            string key,
        //            string return_url,
        //            string _input_charset,
        //            string notify_url,
        //            string token,
        //            string extra_common_param = ""
        //            )
        //        {
        //            / < summary >
        //            / 2015年11月17日14: 08:00
        //              / </ summary >
        //              int i;
        //            string[] Oristr;
        //            构造数组；  
        //            if (!string.IsNullOrEmpty(token))
        //            {
        //                if (extra_common_param == "COD2MOTOPAY")
        //                {
        //                    Oristr = new[]{
        //                    "service="+service,
        //                    "partner=" + partner,
        //                    "subject=" + subject,
        //                    "body=" + body,
        //                    "out_trade_no=" + out_trade_no,
        //                    "total_fee=" + total_fee,
        //                    "show_url=" + show_url,
        //                    "payment_type=" + payment_type,
        //                    "seller_email=" + seller_email,
        //                    "notify_url=" + notify_url,
        //                    "_input_charset="+_input_charset,
        //                    "return_url=" + return_url,
        //                    "token="+token,
        //                    "extra_common_param="+extra_common_param
        //                    };
        //                }
        //                else
        //                {
        //                    Oristr = new[]{
        //                    "service="+service,
        //                    "partner=" + partner,
        //                    "subject=" + subject,
        //                    "body=" + body,
        //                    "out_trade_no=" + out_trade_no,
        //                    "total_fee=" +total_fee,
        //                    "show_url=" + show_url,
        //                    "payment_type=" + payment_type,
        //                    "seller_email=" + seller_email,
        //                    "notify_url=" + notify_url,
        //                    "_input_charset="+_input_charset,
        //                    "return_url=" + return_url,
        //                    "token="+token
        //                    };
        //                }

        //            }
        //            else
        //            {
        //                if (extra_common_param == "COD2MOTOPAY")
        //                {
        //                    Oristr = new[]{
        //                    "service="+service,
        //                    "partner=" + partner,
        //                    "subject=" + subject,
        //                    "body=" + body,
        //                    "out_trade_no=" + out_trade_no,
        //                    "total_fee="+total_fee,
        //                    "show_url=" + show_url,
        //                    "payment_type=" + payment_type,
        //                    "seller_email=" + seller_email,
        //                    "notify_url=" + notify_url,
        //                    "_input_charset="+_input_charset,
        //                    "return_url=" + return_url,
        //                    "extra_common_param="+extra_common_param
        //                    };
        //                }
        //                else
        //                {
        //                    Oristr = new[]{
        //                    "service="+service,
        //                    "partner=" + partner,
        //                    "subject=" + subject,
        //                    "body=" + body,
        //                    "out_trade_no=" + out_trade_no,
        //                    "total_fee=" + total_fee,
        //                    "show_url=" + show_url,
        //                    "payment_type=" + payment_type,
        //                    "seller_email=" + seller_email,
        //                    "notify_url=" + notify_url,
        //                    "_input_charset="+_input_charset,
        //                    "return_url=" + return_url
        //                    };
        //                }


        //            }
        //            进行排序；  
        //            string[] Sortedstr = BubbleSort(Oristr);


        //            构造待md5摘要字符串 ；  

        //            StringBuilder prestr = new StringBuilder();

        //            for (i = 0; i < Sortedstr.Length; i++)
        //            {
        //                if (i == Sortedstr.Length - 1)
        //                {
        //                    prestr.Append(Sortedstr[i]);

        //                }
        //                else
        //                {

        //                    prestr.Append(Sortedstr[i] + "&");
        //                }

        //            }

        //            prestr.Append(key);

        //            生成Md5摘要；  
        //            string sign = GetMD5(prestr.ToString(), _input_charset);

        //            构造支付Url；  
        //            char[] delimiterChars = { '=' };
        //            StringBuilder parameter = new StringBuilder();
        //            parameter.Append(gateway);
        //            for (i = 0; i < Sortedstr.Length; i++)
        //            {
        //                parameter.Append(Sortedstr[i].Split(delimiterChars)[0] + "=" + WebUtility.UrlEncode(Sortedstr[i].Split(delimiterChars)[1]) + "&");
        //            }

        //            parameter.Append("sign=" + sign + "&sign_type=" + sign_type);


        //            返回支付Url；  
        //            return parameter.ToString();

        //        }

        //        public string CreatUrl_MissPayments(
        //                                string gateway,
        //                                string service,
        //                                string partner,
        //                                string sign_type,
        //                                string out_trade_no,
        //                                string key,
        //                                string _input_charset

        //         )
        //        {
        //            / < summary >
        //            / created by sunzhizhi 2006.5.21,sunzhizhi @msn.com。  
        //            / </ summary >
        //            int i;

        //            构造数组；  
        //            string[] Oristr ={
        //                "service="+service,
        //                "partner=" + partner,
        //                "out_trade_no=" + out_trade_no,
        //                "_input_charset="+_input_charset
        //                };

        //            进行排序；  
        //            string[] Sortedstr = BubbleSort(Oristr);


        //            构造待md5摘要字符串 ；  

        //            StringBuilder prestr = new StringBuilder();

        //            for (i = 0; i < Sortedstr.Length; i++)
        //            {
        //                if (i == Sortedstr.Length - 1)
        //                {
        //                    prestr.Append(Sortedstr[i]);
        //                }
        //                else
        //                {
        //                    prestr.Append(Sortedstr[i] + "&");
        //                }

        //            }

        //            prestr.Append(key);

        //            生成Md5摘要；  
        //            string sign = GetMD5(prestr.ToString(), _input_charset);

        //            构造支付Url；  
        //            char[] delimiterChars = { '=' };
        //            StringBuilder parameter = new StringBuilder();
        //            parameter.Append(gateway);
        //            for (i = 0; i < Sortedstr.Length; i++)
        //            {
        //                parameter.Append(Sortedstr[i].Split(delimiterChars)[0] + "=" + WebUtility.UrlEncode(Sortedstr[i].Split(delimiterChars)[1]) + "&");
        //            }

        //            parameter.Append("sign=" + sign + "&sign_type=" + sign_type);


        //            返回支付Url；  
        //            return parameter.ToString();

        //        }


        //        public static string GetTranSign(string TranData)
        //        {
        //            if (string.IsNullOrWhiteSpace(TranData))
        //                return null;
        //            string sign = GetMD5(TranData + "alskdjfaow;fjel;asdjf", "utf-8");
        //            return sign;
        //        }

        //支付宝
        public class AliPayParams
        {

            public string AppId { get; set; }
            public string Uid { get; set; }
            public string Gatewayurl { get; set; }
            public string PrivateKey { get; set; }
            public string AlipayPublicKey { get; set; }
            public string SignType { get; set; }
            public string CharSet { get; set; }
            public static AliPayParams GetAliPayParams(string secName)
            {
                return new AliPayParams
                {
                    AppId = Global.Config.GetSection($"{secName}:AppId").Value,
                    Uid = Global.Config.GetSection($"{secName}:Uid").Value,
                    Gatewayurl = Global.Config.GetSection($"{secName}:Gatewayurl").Value,
                    PrivateKey = Global.Config.GetSection($"{secName}:PrivateKey").Value,
                    AlipayPublicKey = Global.Config.GetSection($"{secName}:AlipayPublicKey").Value,
                    SignType = Global.Config.GetSection($"{secName}:SignType").Value,
                    CharSet = Global.Config.GetSection($"{secName}:CharSet").Value
                };
            }
        }
    }
}
