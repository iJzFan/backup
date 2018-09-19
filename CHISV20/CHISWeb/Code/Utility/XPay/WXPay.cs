using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ass;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CHIS.Models;
using System.Net.Http;
using System.Security.Authentication;

namespace CHIS.Codes.Utility.XPay
{

    /// <summary>
    /// 微信支付方式
    /// https://pay.weixin.qq.com/wiki/doc/api/wap.php?chapter=9_4
    /// </summary>
    public class WXPay : IxPay<WXPayParam_detail>
    {
        /// <summary>
        /// 微信设置数据
        /// </summary>
        WXParams _wxParams;
        public WXPay() : this(WXParams.LoadDefault()) { }
        public WXPay(WXParams wxParams)
        {
            this._wxParams = wxParams;
        }

        /// <summary>
        /// 获取微信支付二维码
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
        public async Task<dynamic> Get2DCodeAsync(string nonce_str, string body,
            string out_trade_no, int total_fee,
            string notify_url,
            IEnumerable<WXPayParam_detail> detail = null,
            string fee_type = "CNY")
        {
            string wxurl = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            string xml = CreateWXXml(new Dictionary<string, string> {
                                        {"body",                body                    },
                                        {"attach",              "微信支付"              },
                                        {"nonce_str",           nonce_str               },
                                        {"out_trade_no",        out_trade_no+"_WXQR"    },
                                        {"trade_type",          "NATIVE"                },
                                        {"total_fee",           total_fee.ToString()    },
                                        {"spbill_create_ip",    getIP()                 },
                                        {"notify_url",          notify_url              }
                                    });

            //发送信息到微信后台获取xml
            byte[] data = Encoding.UTF8.GetBytes(xml);
            var httpReq = (HttpWebRequest)WebRequest.Create(wxurl);
            httpReq.Method = "POST";
            httpReq.ContentType = "text/xml";
            Stream requeststream = await httpReq.GetRequestStreamAsync();//获取写入的数据流                   
            requeststream.Write(data, 0, data.Length);//将数据写入到当前流中  

            var resp = await httpReq.GetResponseAsync();



            //获取返回的xml
            XDocument rxml = XDocument.Load(resp.GetResponseStream());

            var rtncode = getXDocNodeValue("return_code", rxml);
            if (rtncode.IsExists && rtncode.Value == "FAIL")
            {
                throw new Exception(getXDocNodeValue("return_msg", rxml).Value);
            }
            else
            {
                string rltcode = getXDocNodeValue("result_code", rxml).Value;
                if (rtncode.Value == "SUCCESS" && rltcode == "SUCCESS")
                {
                    return new
                    {
                        rlt = true,
                        msg = "",
                        code_url = getXDocNodeValue("code_url", rxml).Value,
                        prepay_id = getXDocNodeValue("prepay_id", rxml).Value,
                        trade_type = getXDocNodeValue("trade_type", rxml).Value,
                        payOrderId = out_trade_no,
                        total_fee = total_fee
                    };
                }
                else
                {
                    var err_code = getXDocNodeValue("err_code", rxml).Value;
                    var err_code_des = getXDocNodeValue("err_code_des", rxml).Value;

                    if (err_code == "ORDERPAID")
                        throw new SuccessedException("该订单曾经微信支付成功！");
                    if (err_code == "INVALID_REQUEST" && Ass.P.PStr(err_code_des).IndexOf("201") >= 0)
                        throw new PayOrderSameException(err_code, err_code_des);//201就是订单号重复                  
                    else throw new CodeException(err_code, err_code_des);
                }
            }


        }


        public async Task<dynamic> GetH5PayInfoAsync(string nonce_str, string body,
            string out_trade_no, int total_fee,
            string notify_url,string ipAddress,
            IEnumerable<WXPayParam_detail> detail = null,
            string fee_type = "CNY")
        {
            string wxurl = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            string xml = CreateWXXml(new Dictionary<string, string> {
                                        {"body",                body                    },
                                        {"attach",              "微信支付"              },
                                        {"nonce_str",           nonce_str               },
                                        {"out_trade_no",        out_trade_no+"_WXH5"    },
                                        {"trade_type",          "MWEB"                  },
                                        {"total_fee",           total_fee.ToString()    },
                                        {"spbill_create_ip",    ipAddress                 },
                                        {"notify_url",          notify_url              }
                                    });

            //发送信息到微信后台获取xml
            byte[] data = Encoding.UTF8.GetBytes(xml);
            var httpReq = (HttpWebRequest)WebRequest.Create(wxurl);
            httpReq.Method = "POST";
            httpReq.ContentType = "text/xml";
            Stream requeststream = await httpReq.GetRequestStreamAsync();//获取写入的数据流                   
            requeststream.Write(data, 0, data.Length);//将数据写入到当前流中  

            var resp = await httpReq.GetResponseAsync();



            //获取返回的xml
            XDocument rxml = XDocument.Load(resp.GetResponseStream());

            var rtncode = getXDocNodeValue("return_code", rxml);
            if (rtncode.IsExists && rtncode.Value == "FAIL")
            {
                throw new Exception(getXDocNodeValue("return_msg", rxml).Value);
            }
            else
            {
                string rltcode = getXDocNodeValue("result_code", rxml).Value;
                if (rtncode.Value == "SUCCESS" && rltcode == "SUCCESS")
                {
                    return new
                    {
                        rlt = true,
                        msg = "",
                        // code_url = getXDocNodeValue("code_url", rxml).Value,
                        prepay_id = getXDocNodeValue("prepay_id", rxml).Value,
                        trade_type = getXDocNodeValue("trade_type", rxml).Value,
                        payOrderId = out_trade_no,
                        //h5页面跳转地址
                        mweb_url = getXDocNodeValue("mweb_url", rxml).Value + "&redirect_url=" + System.Web.HttpUtility.UrlEncode(notify_url),
                        total_fee = total_fee
                    };
                }
                else
                {
                    var err_code = getXDocNodeValue("err_code", rxml).Value;
                    var err_code_des = getXDocNodeValue("err_code_des", rxml).Value;

                    if (err_code == "ORDERPAID")
                        throw new SuccessedException("该订单曾经微信支付成功！");
                    if (err_code == "INVALID_REQUEST" && Ass.P.PStr(err_code_des).IndexOf("201") >= 0)
                        throw new PayOrderSameException(err_code, err_code_des);//201就是订单号重复                  
                    else throw new CodeException(err_code, err_code_des);
                }
            }


        }




        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
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
                string wxurl = "https://api.mch.weixin.qq.com/pay/orderquery";
                string nonce_str = Ass.Data.Secret.MD5(Guid.NewGuid().ToString());
                string xml = CreateWXXml(new Dictionary<string, string> {
                                        {"nonce_str",           nonce_str                        },
                                        {"out_trade_no",        $"{out_trade_no}_{payType}"      }
                                    });
                //发送信息到微信后台获取xml
                byte[] data = Encoding.UTF8.GetBytes(xml);
                var httpReq = (HttpWebRequest)WebRequest.Create(wxurl);
                httpReq.Method = "POST";
                httpReq.ContentType = "text/xml";
                Stream requeststream = await httpReq.GetRequestStreamAsync();//获取写入的数据流                   
                requeststream.Write(data, 0, data.Length);//将数据写入到当前流中  

                var resp = await httpReq.GetResponseAsync();
                //获取返回的xml
                XDocument rxml = XDocument.Load(resp.GetResponseStream());

                var rtncode = getXDocNodeValue("return_code", rxml);
                if (rtncode.IsExists && rtncode.Value == "FAIL") { throw new Exception(getXDocNodeValue("return_msg", rxml).Value); }
                else
                {
                    string rltcode = getXDocNodeValue("result_code", rxml).Value;
                    if (rtncode.Value == "SUCCESS" && rltcode == "SUCCESS")
                    {
                        var state = getXDocNodeValue("trade_state", rxml).Value;
                        if (state == "SUCCESS") return new { rlt = true, msg = "" };
                        else
                        {
                            string msg = "";
                            switch (state)
                            {
                                case "REFUND": msg = "REFUND—转入退款"; break;
                                case "NOTPAY": msg = "NOTPAY—未支付"; break;
                                case "CLOSED": msg = "CLOSED—已关闭"; break;
                                case "REVOKED": msg = "REVOKED—已撤销（刷卡支付）"; break;
                                case "USERPAYING": msg = "USERPAYING--用户支付中"; break;
                                case "PAYERROR": msg = "PAYERROR--支付失败(其他原因，如银行返回失败)"; break;
                            }
                            throw new Exception(msg);
                        }
                    }
                    else
                    {
                        var err_code = getXDocNodeValue("err_code", rxml).Value;
                        var err_code_des = getXDocNodeValue("err_code_des", rxml).Value;
                        throw new Exception(err_code + ":" + err_code_des);
                    }
                }
            }
            catch (Exception ex)
            {
                return new { rlt = false, msg = ex.Message };
            }

        }




        //public static string Post(string xml, string url, bool isUseCert, int timeout)
        //{
        //    System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接

        //    string result = "";//返回结果

        //    HttpWebRequest request = null;
        //    HttpWebResponse response = null;
        //    Stream reqStream = null;

        //    try
        //    {
        //        //设置最大连接数
        //        //ServicePointManager.DefaultConnectionLimit = 200;
        //        //设置https验证方式
        //        if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        //        {                    
        //            ServicePointManager.ServerCertificateValidationCallback =
        //                    new RemoteCertificateValidationCallback(CheckValidationResult);
        //        }

        //        /***************************************************************
        //        * 下面设置HttpWebRequest的相关属性
        //        * ************************************************************/
        //        request = (HttpWebRequest)WebRequest.Create(url);

        //        request.Method = "POST";
        //        request.ContinueTimeout = timeout * 1000;

        //        //设置代理服务器
        //        //WebProxy proxy = new WebProxy();                          //定义一个网关对象
        //        //proxy.Address = new Uri(WxPayConfig.PROXY_URL);              //网关服务器端口:端口
        //        //request.Proxy = proxy;

        //        //设置POST的数据类型和长度
        //        request.ContentType = "text/xml";
        //        byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
        //        request.ContentLength = data.Length;

        //        //是否使用证书
        //        if (isUseCert)
        //        {
        //            //string path = HttpContext.Current.Request.PhysicalApplicationPath;
        //            //X509Certificate2 cert = new X509Certificate2(path + WxPayConfig.SSLCERT_PATH, WxPayConfig.SSLCERT_PASSWORD);
        //            //request.ClientCertificates.Add(cert);
        //        }

        //        //往服务器写入数据
        //        reqStream = request.GetRequestStream();
        //        reqStream.Write(data, 0, data.Length);
        //        reqStream.Close();

        //        //获取服务端返回
        //        response = (HttpWebResponse)request.GetResponse();

        //        //获取服务端返回数据
        //        StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
        //        result = sr.ReadToEnd().Trim();
        //        sr.Close();
        //    }
        //    catch (System.Threading.ThreadAbortException e)
        //    {
        //        System.Threading.Thread.ResetAbort();
        //    }
        //    catch (WebException e)
        //    {

        //        if (e.Status == WebExceptionStatus.ProtocolError)
        //        {

        //        }
        //        throw new Exception(e.ToString());
        //    }
        //    catch (Exception e)
        //    {

        //        throw new Exception(e.ToString());
        //    }
        //    finally
        //    {
        //        //关闭连接和流
        //        if (response != null)
        //        {
        //            response.Close();
        //        }
        //        if (request != null)
        //        {
        //            request.Abort();
        //        }
        //    }
        //    return result;
        //}


        /*

    #region 退款操作
    /// <summary>
    /// 退款申请
    /// </summary>
    /// <param name="nonce_str"></param>
    /// <param name="out_trade_no"></param>
    /// <param name="out_refund_no"></param>
    /// <param name="total_fee"></param>
    /// <param name="refund_fee"></param>
    /// <returns></returns>
    public async Task<Exception> WXRefundApplyAsync(string refundUid, string nonce_str, string out_trade_no, string out_refund_no, int total_fee, int refund_fee)
    {
        try
        {
            string apiurl = "https://api.mch.weixin.qq.com/secapi/pay/refund";
            string xml = CreateWXXml(new Dictionary<string, string> {
                          {"nonce_str", nonce_str},
                          {"out_trade_no", out_trade_no},
                          {"total_fee", total_fee.ToString()},
                          {"refund_fee", refund_fee.ToString()},
                          {"op_user_id", _wxParams.MchId },
                          {"out_refund_no",refundUid }
                        });
            //发送信息到微信后台获取xml 需要证书      

            //var handler = new HttpClientHandler();
            //handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            //handler.SslProtocols = SslProtocols.Tls12;
            //handler.ClientCertificates.Add(new X509Certificate2("cert.crt"));
            //var client = new HttpClient(handler);
            //var result = client.GetAsync(apiurl).GetAwaiter().GetResult();


            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            X509Certificate cer = new X509Certificate(_wxParams.CertPath, _wxParams.MchId, X509KeyStorageFlags.MachineKeySet);
            byte[] data = Encoding.UTF8.GetBytes(xml);
            var httpReq = (HttpWebRequest)WebRequest.Create(apiurl);
            httpReq.ClientCertificates.Add(cer);
            httpReq.Method = "POST";
            httpReq.ContentLength = data.Length;
            httpReq.ContentType = "text/xml";
            using (var stream = await httpReq.GetRequestStreamAsync()) { stream.Write(data, 0, data.Length); }
            var resp = await httpReq.GetResponseAsync();
            //获取返回的xml
            XDocument rxml = XDocument.Load(resp.GetResponseStream());

            var rtncode = getXDocNodeValue("return_code", rxml);
            if (rtncode.IsExists && rtncode.Value == "FAIL")
            {
                throw new Exception(getXDocNodeValue("return_msg", rxml).Value);
            }
            else
            {
                string rltcode = getXDocNodeValue("result_code", rxml).Value;
                if (rtncode.Value == "SUCCESS" && rltcode == "SUCCESS")
                {
                    return null;
                }
                else
                {
                    var err_code = getXDocNodeValue("err_code", rxml).Value;
                    var err_code_des = getXDocNodeValue("err_code_des", rxml).Value;
                    throw new RefundException(err_code, err_code_des);
                }
            }

        }
        catch (Exception ex)
        {
            return ex;
        }
    }



    /// <summary>
    /// 获取申请退款的结果
    /// </summary>
    /// <param name="nonce_str"></param>
    /// <param name="out_trade_no"></param>
    /// <returns></returns>
    public async Task<Exception> GetRefundApplyResultAsync(string nonce_str, string out_trade_no, Guid refundUId)
    {

        string apiurl = "https://api.mch.weixin.qq.com/pay/refundquery";
        try
        {
            string xml = CreateWXXml(new Dictionary<string, string> {
                                    {"nonce_str",nonce_str },
                                    { "out_trade_no", out_trade_no}
                                   });
            //发送信息到微信后台获取xml
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            X509Certificate cer = new X509Certificate(_wxParams.CertPath, _wxParams.MchId, X509KeyStorageFlags.MachineKeySet);
            byte[] data = Encoding.UTF8.GetBytes(xml);
            var httpReq = (HttpWebRequest)WebRequest.Create(apiurl);
            httpReq.Method = "POST";
            httpReq.ContentLength = data.Length;
            httpReq.ContentType = "text/xml";
            using (var stream = httpReq.GetRequestStream()) { stream.Write(data, 0, data.Length); }
            var resp = await httpReq.GetResponseAsync();
            //获取返回的xml
            XmlResult xmlrlt = GetXmlResult(resp.GetResponseStream());
            if (xmlrlt.Get("return_code") == "FAIL")
            {
                throw new Exception(xmlrlt.Get("return_msg"));
            }
            else
            {
                if (xmlrlt.Get("return_code") == "SUCCESS" && xmlrlt.Get("result_code") == "SUCCESS")
                {
                    //查找具体的内容
                    var n = Ass.P.PIntV(xmlrlt.Get("refund_count"), 0);
                    for (int i = 0; i < n; i++)
                    {
                        if (xmlrlt.Get("out_refund_no_" + i) == refundUId.ToString("N"))
                        {
                            var s = xmlrlt.Get("refund_status_" + i);
                            if (s == "SUCCESS") return null;
                            else throw new RefundException(s, "退款失败" + s);
                        }
                    }
                    throw new RefundException("NOTFIND_UID", "没有找到退款UID");
                }
                else
                {
                    throw new RefundException(xmlrlt.Get("err_code"), xmlrlt.Get("err_code_des"));
                }
            }
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    #endregion

    */

        #region 私有函数
        private string CreateWXXml(Dictionary<string, string> baseDict)
        {
            Dictionary<string, string> dict = baseDict;
            dict.Add("appid", _wxParams.AppId);
            dict.Add("mch_id", _wxParams.MchId);

            //整理数据    空数据过滤 a-z排序        
            var a = (from item in dict
                     where !(string.IsNullOrWhiteSpace(item.Value))
                     orderby item.Key
                     select item).ToDictionary(o => o.Key, p => p.Value);
            dict = a;


            StringBuilder b = new StringBuilder();
            b.Append("<xml>");
            foreach (var item in dict)
            {
                b.AppendFormat("<{0}>{1}</{0}>", item.Key, item.Value);
            }
            b.AppendFormat("<sign>{0}</sign>", getSign(dict));
            b.Append("</xml>");
            return b.ToString();
        }

        private dynamic getXDocNodeValue(string name, XDocument xdoc, bool isDeclaration = false)
        {
            try
            {
                var xnode = xdoc.Root.Elements(name).FirstOrDefault();
                if (xnode == null)
                {
                    return new { IsExists = false };
                }
                else
                {
                    if (isDeclaration)
                    {
                        var dt = xnode.DescendantNodes().ToList()[0] as XCData;
                        return new { IsExists = true, Name = name, Value = dt.Value };
                    }
                    else return new { IsExists = true, Name = name, Value = xnode.Value };
                }
            }
            catch (Exception ex)
            {
                var e = ex;
                return new { IsExists = false };
            }
        }



        /// <summary>
        /// 生成签名
        /// </summary>
        private object getSign(Dictionary<string, string> ps)
        {
            var s = from item in ps
                    orderby item.Key
                    select string.Format("{0}={1}", item.Key, item.Value);
            string stringA = string.Join("&", s);
            string stringB = stringA + "&key=" + _wxParams.Key;
            string sign = Ass.Data.Secret.MD5(stringB).ToUpper();
            return sign;
        }


        private string getIP()
        {
            return "127.0.0.1";
            
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



        #endregion
    }

    public class XmlResult
    {
        Dictionary<string, string> kvs = new Dictionary<string, string>();

        public string Get(string key)
        {
            if (kvs.ContainsKey(key))
            {
                var v = kvs[key];
                if (key == "out_trade_no")
                {
                    v = v.Replace("_WXQR", "");
                    v = v.Replace("_WXPUB", "");
                    v = v.Replace("_WXH5", "");
                }
                return v;
            }
            else return "";
        }
        public void Set(string key, object val)
        {
            string k = key.ToLower();
            string v = string.Format("{0}", val);
            if (kvs.ContainsKey(k)) kvs[k] = v;
            else kvs.Add(k, v);
        }
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (var key in kvs.Keys)
            {
                b.AppendFormat("{0}={1};", key, kvs[key]);
            }
            return b.ToString();
        }
    }

    public class WXPayParam_detail
    {

        private string _goods_id;
        private string _goods_name;
        private string _goods_category;
        private string _body;

        /// <summary>
        /// 药品的编号
        /// </summary>
        public string goods_id { get { return _goods_id.GetSubString(32); } set { _goods_id = value; } }

        /// <summary>
        /// 必填 256 药品名称
        /// </summary>
        public string goods_name { get { return _goods_name.GetSubString(256); } set { _goods_name = value; } }

        /// <summary>
        /// 药品数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 药品单价，单位为分
        /// </summary>
        public int price { get; set; }

        /// <summary>
        /// 药品类目ID
        /// </summary>
        public string goods_category { get { return _goods_category.GetSubString(32); } set { _goods_category = value; } }

        /// <summary>
        /// 可选 1000 药品描述信息
        /// </summary>
        public string body { get { return _body.GetSubString(1000); } set { _body = value; } }
    }

    /// <summary>
    /// 微信配置
    /// </summary>
    public class WXParams
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string MchId { get; set; }
        public string Key { get; set; }


        /// <summary>
        /// 证书地址
        /// </summary>
        public string CertPath { get; set; }


        #region 获取微信配置参数
        public static WXParams LoadDefault()
        {
            return getWXParams("WXParamsJK813");
        }

        private static WXParams getWXParams(string secName)
        {
            return new WXParams
            {
                AppId = Global.Config.GetSection($"{secName}:AppId").Value,
                AppSecret = Global.Config.GetSection($"{secName}:AppSecret").Value,
                CertPath = Ass.IO.WinPathToUnionPath(Global.Config.GetSection($"{secName}:CertPath").Value),
                Key = Global.Config.GetSection($"{secName}:Key").Value,
                MchId = Global.Config.GetSection($"{secName}:MchId").Value
            };
        }

        public static WXParams LoadWXParams(string type)
        {
            WXParams rtn = new WXParams();
            switch (type)
            {
                case "jk813": return getWXParams("WXParamsJK813");
                default: rtn = LoadDefault(); break;
            }
            return rtn;
        }


        #endregion
    }
}
