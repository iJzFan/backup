using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alipay.AopSdk.F2FPay.AspnetCore;
using Alipay.AopSdk.F2FPay.Business;
using Alipay.AopSdk.F2FPay.Domain;
using Alipay.AopSdk.F2FPay.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using CHIS.Codes.Utility.XPay;
using static CHIS.Codes.Utility.XPay.AliPay;
using Alipay.AopSdk.AspnetCore;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers.Charge
{
    public partial class ChargeController
    {
        

        #region 扫码支付
        //二维码支付测试页面
        public IActionResult Scan()
        {
            return View(nameof(Scan));
        }
        /// <summary>
        /// 生成支付二维码
        /// </summary>
        /// <param name="orderName">订单名称</param>
        /// <param name="orderAmount">订单金额</param>
        /// <param name="outTradeNo">订单号</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ScanCodeGen(string orderName, string orderAmount, string outTradeNo)
        {

            AlipayTradePrecreateContentBuilder builder = BuildPrecreateContent(orderName, orderAmount, outTradeNo);

            //如果需要接收扫码支付异步通知，那么请把下面两行注释代替本行。
            //推荐使用轮询撤销机制，不推荐使用异步通知,避免单边账问题发生。
            AlipayF2FPrecreateResult precreateResult = _alipayF2FService.TradePrecreate(builder);
            //string notify_url = "http://10.5.21.14/Pay/Notify";  //商户接收异步通知的地址
            //AlipayF2FPrecreateResult precreateResult = serviceClient.tradePrecreate(builder, notify_url);

            //以下返回结果的处理供参考。
            //payResponse.QrCode即二维码对于的链接
            //将链接用二维码工具生成二维码打印出来，顾客可以用支付宝钱包扫码支付。
            var bitmap = new Bitmap(Path.Combine(_hostingEnvironment.WebRootPath, "images/error.png"));
            switch (precreateResult.Status)
            {
                case ResultEnum.SUCCESS:
                    bitmap.Dispose();
                    bitmap = RenderQrCode(precreateResult.response.QrCode);
                    //轮询订单结果
                    //根据业务需要，选择是否新起线程进行轮询
                    ParameterizedThreadStart parStart = new ParameterizedThreadStart(LoopQuery);
                    Thread myThread = new Thread(parStart);
                    object o = precreateResult.response.OutTradeNo;
                    Console.WriteLine("支付成功结果：" + o);
                    myThread.Start(o);
                    break;
                case ResultEnum.FAILED:
                    Console.WriteLine("生成二维码失败：" + precreateResult.response.Body);
                    break;

                case ResultEnum.UNKNOWN:
                    Console.WriteLine("生成二维码失败：" + (precreateResult.response == null ? "配置或网络异常，请检查后重试" : "系统异常，请更新外部订单后重新发起请求"));
                    break;
            }
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            byte[] bytes = ms.GetBuffer();
            return File(bytes, "image/png");
        }


        /// <summary>
        /// 构造支付请求数据
        /// </summary>
        /// <param name="orderName">订单名称</param>
        /// <param name="orderAmount">订单金额</param>
        /// <param name="outTradeNo">订单编号</param>
        /// <returns>请求结果集</returns>
        private AlipayTradePrecreateContentBuilder BuildPrecreateContent(string orderName, string orderAmount, string outTradeNo)
        {
            //线上联调时，请输入真实的外部订单号。
            if (string.IsNullOrEmpty(outTradeNo))
            {
                outTradeNo = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "0000" + (new Random()).Next(1, 10000).ToString();
            }

            AlipayTradePrecreateContentBuilder builder = new AlipayTradePrecreateContentBuilder();
            //收款账号
            builder.seller_id = _alipayService.Options.Uid;
            //订单编号
            builder.out_trade_no = outTradeNo;
            //订单总金额
            builder.total_amount = orderAmount;
            //参与优惠计算的金额
            //builder.discountable_amount = "";
            //不参与优惠计算的金额
            //builder.undiscountable_amount = "";
            //订单名称
            builder.subject = orderName;
            //自定义超时时间
            builder.timeout_express = "5m";
            //订单描述
            builder.body = "";
            //门店编号，很重要的参数，可以用作之后的营销
            builder.store_id = "tsjkit";
            //操作员编号，很重要的参数，可以用作之后的营销
            builder.operator_id = "admin";

            //传入商品信息详情
            List<GoodsInfo> gList = new List<GoodsInfo>();
            GoodsInfo goods = new GoodsInfo();
            goods.goods_id = "goods id";
            goods.goods_name = "goods name";
            goods.price = "0.01";
            goods.quantity = "1";
            gList.Add(goods);
            builder.goods_detail = gList;

            //系统商接入可以填此参数用作返佣
            //ExtendParams exParam = new ExtendParams();
            //exParam.sysServiceProviderId = "20880000000000";
            //builder.extendParams = exParam;
            return builder;
        }

        /// <summary>
        /// 渲染二维码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private Bitmap RenderQrCode(string str)
        {
            QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.L;
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(str, eccLevel))
                {
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {

                        Bitmap bp = qrCode.GetGraphic(20, Color.Black, Color.White,
                            new Bitmap(Path.Combine(_hostingEnvironment.WebRootPath, "images/alipay.png")), 15);
                        return bp;
                    }
                }
            }

        }

        /// <summary>
        /// 轮询支付结果
        /// </summary>
        /// <param name="o">订单号</param>
        public void LoopQuery(object o)
        {
            AlipayF2FQueryResult queryResult = new AlipayF2FQueryResult();
            int count = 100;
            int interval = 10000;
            string outTradeNo = o.ToString();

            for (int i = 1; i <= count; i++)
            {
                Thread.Sleep(interval);
                queryResult = _alipayF2FService.TradeQuery(outTradeNo);
                if (queryResult?.Status == ResultEnum.SUCCESS)
                {
                    DoSuccessProcess(queryResult);
                    return;
                }
            }
            DoFailedProcess(queryResult);
        }

        /// <summary>
        /// 请添加支付成功后的处理
        /// </summary>
        private void DoSuccessProcess(AlipayF2FQueryResult queryResult)
        {
            //支付成功，请更新相应单据
            Console.WriteLine("扫码支付成功：商户订单号 " + queryResult.response.OutTradeNo);

        }

        /// <summary>
        /// 请添加支付失败后的处理
        /// </summary>
        private void DoFailedProcess(AlipayF2FQueryResult queryResult)
        {
            //支付失败，请更新相应单据
            Console.WriteLine("扫码支付失败：商户订单号 " + queryResult.response.OutTradeNo);
        }
        #endregion
        #region 支付异步回调通知

        /// <summary>
        /// 支付异步回调通知 需配置域名 因为是支付宝主动post请求这个action 所以要通过域名访问或者公网ip
        /// </summary>
        public async void Notify()
        {
            /* 实际验证过程建议商户添加以下校验。
			1、商户需要验证该通知数据中的out_trade_no是否为商户系统中创建的订单号，
			2、判断total_amount是否确实为该订单的实际金额（即商户订单创建时的金额），
			3、校验通知中的seller_id（或者seller_email) 是否为out_trade_no这笔单据的对应的操作方（有的时候，一个商户可能有多个seller_id/seller_email）
			4、验证app_id是否为该商户本身。
			*/
            Dictionary<string, string> sArray = GetRequestPost();
            if (sArray.Count != 0)
            {
                bool flag = _alipayService.RSACheckV1(sArray);
                if (flag)
                {
                    //交易状态
                    //判断该笔订单是否在商户网站中已经做过处理
                    //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                    //请务必判断请求时的total_amount与通知时获取的total_fee为一致的
                    //如果有做过处理，不执行商户的业务程序

                    //注意：
                    //退款日期超过可退款期限后（如三个月可退款），支付宝系统发送该交易状态通知
                    Console.WriteLine(Request.Form["trade_status"]);

                    await Response.WriteAsync("success");
                }
                else
                {
                    await Response.WriteAsync("fail");
                }
            }
        }
        #endregion
        #region 支付同步回调

        /// <summary>
        /// 支付同步回调
        /// </summary>
        [HttpGet]
        public IActionResult Callback()
        {
            /* 实际验证过程建议商户添加以下校验。
			1、商户需要验证该通知数据中的out_trade_no是否为商户系统中创建的订单号，
			2、判断total_amount是否确实为该订单的实际金额（即商户订单创建时的金额），
			3、校验通知中的seller_id（或者seller_email) 是否为out_trade_no这笔单据的对应的操作方（有的时候，一个商户可能有多个seller_id/seller_email）
			4、验证app_id是否为该商户本身。
			*/
            Dictionary<string, string> sArray = GetRequestGet();
            if (sArray.Count != 0)
            {
                bool flag = _alipayService.RSACheckV1(sArray);
                if (flag)
                {
                    Console.WriteLine($"同步验证通过，订单号：{sArray["out_trade_no"]}");
                    ViewData["PayResult"] = "同步验证通过";
                }
                else
                {
                    Console.WriteLine($"同步验证失败，订单号：{sArray["out_trade_no"]}");
                    ViewData["PayResult"] = "同步验证失败";
                }
            }
            return View();
        }

        #endregion
        #region 解析请求参数

        private Dictionary<string, string> GetRequestGet()
        {
            Dictionary<string, string> sArray = new Dictionary<string, string>();

            ICollection<string> requestItem = Request.Query.Keys;
            foreach (var item in requestItem)
            {
                sArray.Add(item, Request.Query[item]);

            }
            return sArray;

        }

        private Dictionary<string, string> GetRequestPost()
        {
            Dictionary<string, string> sArray = new Dictionary<string, string>();

            ICollection<string> requestItem = Request.Form.Keys;
            foreach (var item in requestItem)
            {
                sArray.Add(item, Request.Form[item]);

            }
            return sArray;

        }

        #endregion
    }
}
