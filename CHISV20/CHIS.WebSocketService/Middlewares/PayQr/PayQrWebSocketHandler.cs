using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public class PayQrWebSocketHandler
    {
        /*
                        "GETPAYINFO"://获取到二维码相关数据
                        "PAYEDSUCCESS"://已经支付成功了
                        "THISPAYOK"://当次支付成功
                        "ERROR"://出现失败
         */
        protected PayQrWebSocketConnectionMgr _wsconnMgr;
        protected IConfiguration _configuration;
        IHostingEnvironment _hostEnvi;
        public PayQrWebSocketHandler(PayQrWebSocketConnectionMgr wsconnMgr, IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this._wsconnMgr = wsconnMgr;
            _configuration = configuration;
            TimeOutSeconds = Convert.ToInt32(configuration.GetSection("QRPay:Timeout").Value);
            _hostEnvi = hostingEnvironment;
        }

        public int TimeOutSeconds = 60 * 10;//设置10分钟超时

        /// <summary>
        /// 获取支付状态
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public PayQrStatuses GetPayQrStatus(PayQrWebSocket socket)
        {
            return socket.PayQrStatus;
        }

        /// <summary>
        /// 获取支付信息
        /// </summary>
        /// <param name="socket"></param>
        public async Task GetPayQrInfo(PayQrWebSocket socket)
        {
            CHIS.Models.PayQrInfo QrInfo = null;
            try
            {
                string h = await Ass.Net.WebHelper.WebPost(GetUrl("QRPay:HostUrl", "QRPay:GetPayInfo") + $"?payOrderId={socket.PayOrderId}");
                QrInfo = GetFromJsonString<CHIS.Models.PayQrInfo>(h);
                if (QrInfo.status == "ERROR") throw new Exception();
                if (QrInfo.status == "PAYEDSUCCESS")
                {
                    await NotifyToClient(socket, QrInfo);
                    socket.PayQrStatus = PayQrStatuses.PAYEDSUCCESS;
                    await _wsconnMgr.RemoveSocket(socket);//关闭连接
                }
                else
                {
                    QrInfo.union2DCodeUrl = GetUnion2DCodeUrl(QrInfo);//获取统一二维码
                    socket.WxQrUrl = QrInfo.wx2DCodeUrl;
                    socket.AliQrUrl = QrInfo.ali2DCodeUrl;
                    socket.PayAmount = QrInfo.totalAmount;
                    await NotifyToClient(socket, QrInfo);
                    socket.PayQrStatus = PayQrStatuses.GETPAYINFO;
                }

            }
            catch (Exception ex)
            {
                if (QrInfo == null) QrInfo = new CHIS.Models.PayQrInfo() { rlt = false, msg = ex.Message, status = "ERROR" };
                await NotifyToClient(socket, QrInfo);
                await _wsconnMgr.RemoveSocket(socket);//关闭连接
            }
        }

        public async Task<CHIS.Models.PayWxH5Info> GetWxPayH5Info(PayQrWebSocket socket,string ipAddress)
        {
            CHIS.Models.PayWxH5Info QrInfo = null;
            try
            {
                string h = await Ass.Net.WebHelper.WebPost(GetUrl("QRPay:HostUrl", "QRPay:GetWxH5PayInfo") + $"?payOrderId={socket.PayOrderId}&ipAddress={ipAddress}");
                QrInfo = GetFromJsonString<CHIS.Models.PayWxH5Info>(h);
                if (QrInfo.status == "ERROR") throw new Exception();
                if (QrInfo.status == "PAYEDSUCCESS")
                {
                    await NotifyToClient(socket, QrInfo);
                    socket.PayQrStatus = PayQrStatuses.PAYEDSUCCESS;
                    await _wsconnMgr.RemoveSocket(socket);//关闭连接
                    return new CHIS.Models.PayWxH5Info() { rlt = false, msg = "已经支付过了", status = "PAYEDSUCCESS" };
                }
                else
                {
                    return QrInfo;             
                }

            }
            catch (Exception ex)
            {
                if (QrInfo == null) QrInfo = new CHIS.Models.PayWxH5Info() { rlt = false, msg = ex.Message, status = "ERROR" };
                //await NotifyToClient(socket, QrInfo);
                //await _wsconnMgr.RemoveSocket(socket);//关闭连接
                return QrInfo;
            }
        }



        /// <summary>
        /// 开始侦测
        /// </summary>
        /// <param name="socket"></param>
        public void StartScaningDetect(PayQrWebSocket socket)
        {
            socket.PayQrStatus = PayQrStatuses.MONITORSCANING;
            socket.StartScanTime = DateTime.Now;

        }
        /// <summary>
        /// 侦测超时
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public async Task ScanningTimeoutDetect(PayQrWebSocket socket, CancellationToken cancellationToken)
        {
            if ((DateTime.Now - socket.StartScanTime).TotalSeconds > this.TimeOutSeconds)
            {
                socket.PayQrStatus = PayQrStatuses.TIMEOUT;
                await NotifyToClient(socket, new { status = PayQrStatuses.TIMEOUT.ToString(), msg = "操作超时,请重新启动付款" }, cancellationToken);
                await _wsconnMgr.RemoveSocket(socket);
            }
        }

        /// <summary>
        /// 设置订单支付状态，成功或者失败
        /// </summary>
        /// <param name="payOrderId">订单</param>
        /// <param name="bSuccess"></param>
        /// <param name="msg">失败的消息</param> 
        public async Task SetOrderPayedStatus(string payOrderId, bool bSuccess, string msg)
        {
            var ws = _wsconnMgr.GetItem(_wsconnMgr.GetId(payOrderId));
            if (bSuccess)
            {
                ws.PayQrStatus = PayQrStatuses.THISPAYOK;
                await NotifyToClient(ws, new { status = PayQrStatuses.THISPAYOK.ToString(), msg = "支付成功！" });
                await _wsconnMgr.RemoveSocket(ws);//关闭连接
            }
            else
            {
                ws.PayQrStatus = PayQrStatuses.ERROR;
                await NotifyToClient(ws, new { status = PayQrStatuses.ERROR.ToString(), msg = msg });
                await _wsconnMgr.RemoveSocket(ws);
            }
        }


        private T GetFromJsonString<T>(string jsonString)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            return (T)serializer.ReadObject(mStream);
        }

        //获取连接地址
        private string GetUrl(string host, string reqpath)
        {
            var hoststr = _configuration.GetSection(host).Value;
            return hoststr + _configuration.GetSection(reqpath).Value;
        }


        private async Task NotifyToClient(
            BaseWebSocket socket, object data,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            var segment = new ArraySegment<byte>(buffer);
            await socket.WebSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }

        /// <summary>
        /// 获取统一二维码
        /// </summary> 
        private string GetUnion2DCodeUrl(CHIS.Models.PayQrInfo model)
        {
            string md5 = Ass.Data.Secret.MD5($"{model.wx2DCodeUrl}|{model.ali2DCodeUrl}|{model.totalAmount}|{model.payOrderId}");
            string s = $"{md5}|{model.totalAmount}|{model.payOrderId}";
            string ens = System.Web.HttpUtility.UrlEncode(Ass.Data.Secret.Encript(s, Global.SYS_ENCRIPT_PWD));
            var host = Global.Localhost2Ip(_configuration.GetSection("SysEnvi:SelfUrlRoot").Value);
            var path = _configuration.GetSection("QRPay:QRUnionScanUrl").Value;
            return $"{host}{path}?code={ens}";
        }



    }
}
