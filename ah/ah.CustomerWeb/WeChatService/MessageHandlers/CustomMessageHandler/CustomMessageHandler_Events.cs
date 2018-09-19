using Senparc.Weixin.MP.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ah.WeChatService.MessageHandlers.CustomMessageHandler
{
    public partial class CustomMessageHandler
    {


        /// <summary>
        /// 通过二维码扫描关注扫描事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnEvent_ScanRequestAsync(RequestMessageEvent_Scan requestMessage)
        {
            var array = requestMessage.EventKey.Split(',');

            if (array[0] == "RX")
            {
                //通过扫描关注
                var responseMessage = CreateResponseMessage<ResponseMessageNews>();

                responseMessage.Articles.Add(new Article
                {
                    Description = "约处方",
                    Title = "快速约处方",
                    Url = $"http://my.jk213.com/customer/rx/{array[1]}-{array[2]}-{array[3]}",
                    PicUrl = "http://r.jk213.com/reslib/img/doctor/_d256_1.png"
                });

                return responseMessage;

            }
            else
            {
                var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);

                responseMessage.Content = GetWelcomeInfo();

                return responseMessage;
            }
        }

        /// <summary>
        /// 订阅（关注）事件
        /// </summary>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnEvent_SubscribeRequestAsync(RequestMessageEvent_Subscribe requestMessage)
        {

            //responseMessage.Content = GetWelcomeInfo();
            var array = requestMessage.EventKey.Split(',');

            if (array[0] == "qrscene_RX")
            {
                //通过扫描关注
                var responseMessage = CreateResponseMessage<ResponseMessageNews>();

                responseMessage.Articles.Add(new Article
                {
                    Description = "约处方",
                    Title = "快速约处方",
                    Url = $"http://my.jk213.com/customer/rx/{array[1]}-{array[2]}-{array[3]}",
                    PicUrl = "http://r.jk213.com/reslib/img/doctor/_d256_1.png"
                });

                return responseMessage;

            }
            else
            {
                var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);

                responseMessage.Content = GetWelcomeInfo();

                return responseMessage;
            }
        }

        /// <summary>
        /// 用户领取卡券
        /// </summary>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnEvent_User_Get_CardRequestAsync(RequestMessageEvent_User_Get_Card requestMessage)
        {

            var openId = requestMessage.FromUserName ?? "";

            var cardId = requestMessage.CardId ?? "";

            var cardCode = requestMessage.UserCardCode ?? "";

            try
            {
                await _service.GetDataAsync(Global.CHIS_HOST + "/openapi/gift/UpdateOrderCode" + "?openId=" + openId + "&cardId=" + cardId + "&cardCode=" + cardCode);
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                   await _service.GetDataAsync(Global.CHIS_HOST + "/openapi/gift/UpdateOrderCode" + "?openId=" + openId + "&cardId=" + cardId + "&cardCode=" + cardCode);
                }
                else
                {
                    throw;
                }
            }

            return null;
        }

        /// <summary>
        /// 用户消费卡券
        /// </summary>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnEvent_User_Consume_CardRequestAsync(RequestMessageEvent_User_Consume_Card requestMessage)
        {

            var staffOpenId = requestMessage.StaffOpenId ?? "";

            var spendingPlace = requestMessage.LocationName ?? "";

            var cardCode = requestMessage.UserCardCode ?? "";

            try
            {
                await _service.GetDataAsync(Global.CHIS_HOST + "/openapi/gift/usegiftbywechat" + "?staffOpenId=" + staffOpenId + "&spendingPlace=" + spendingPlace + "&cardCode=" + cardCode);
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                   await _service.GetDataAsync(Global.CHIS_HOST + "/openapi/gift/usegiftbywechat" + "?openId=" + staffOpenId + "&spendingPlace=" + spendingPlace + "&cardCode=" + cardCode);
                }
                else
                {
                    throw;
                }
            }

            return null;
        }

        /// <summary>
        /// 用户自行删除卡券
        /// </summary>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnEvent_User_Del_CardRequestAsync(RequestMessageEvent_User_Del_Card requestMessage)
        {

            var staffOpenId = "";

            var spendingPlace = "自行删除";

            var cardCode = requestMessage.UserCardCode ?? "";

            try
            {
                await _service.GetDataAsync(Global.CHIS_HOST + "/openapi/gift/usegiftbywechat" + "?staffOpenId=" + staffOpenId + "&spendingPlace=" + spendingPlace + "&cardCode=" + cardCode);
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    await _service.GetDataAsync(Global.CHIS_HOST + "/openapi/gift/usegiftbywechat" + "?openId=" + staffOpenId + "&spendingPlace=" + spendingPlace + "&cardCode=" + cardCode);
                }
                else
                {
                    throw;
                }
            }

            return null;
        }

        private string GetWelcomeInfo()
        {
            return "欢迎关注【健康813微信公众平台】";
        }

    }
}
