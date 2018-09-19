using CHIS.Codes.Utility.XPay;
using Microsoft.Extensions.Options;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHIS.Models;
using Senparc.Weixin.MP.AdvancedAPIs.Card;

namespace CHIS.Services
{
    //微信相关方法
    public class WeChatService
    {
        private WXParams _wxParams;

        public WeChatService(IOptionsSnapshot<WXParams> wxParams)
        {
            _wxParams = wxParams.Value;
        }

        /// <summary>
        /// 申请带参二维码(临时)
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <param name="rxDoctorId"></param>
        /// <returns></returns>
        public async Task<string> CreateQRCodeUrl(int stationId, int doctorId, int rxDoctorId)
        {

            var ticks = DateTime.Now.Ticks.ToString();
            var sceneId = int.Parse(ticks.Substring(ticks.Length - 7, 7));

            var qrResult = await QrCodeApi.CreateAsync(_wxParams.AppId, 60 * 60 * 24, sceneId, QrCode_ActionName.QR_STR_SCENE, $"RX,{stationId},{doctorId},{rxDoctorId}");
            return qrResult.url;
        }


        /// <summary>
        /// 创建微信卡券
        /// </summary>
        /// <param name="gift"></param>
        /// <returns></returns>
        public async Task<string> CreateWeChatCardAsync(GiftInputModel gift)
        {
            if (gift.Type == 0) return null;//实物不创建卡券

            var infoBase = new Card_BaseInfoBase
            {
                brand_name = "天使健康积分平台",

                can_give_friend = false,

                can_share = false,

                color = "Color010",

                code_type = Card_CodeType.CODE_TYPE_QRCODE,

                description = gift.Description,

                logo_url = "http://www.lgstatic.com/thumbnail_300x300/image1/M00/44/07/Cgo8PFXT8p2AIiAvAAAnzQSZgWQ814.png",

                notice = "请出示二维码核销卡券",

                sku = new Card_BaseInfo_Sku
                {
                    quantity = 1000000,

                    total_quantity = 1000000
                }
            };

            var timeStampBegin = new DateTime(1970, 1, 1);

            if (gift.ExpiryDate.HasValue)
            {
                var beginTimestamp = (DateTime.Now.Ticks - timeStampBegin.Ticks) / 10000000;

                var endTimestamp = (DateTime.MaxValue.Ticks - timeStampBegin.Ticks) / 10000000;

                infoBase.date_info = new Card_BaseInfo_DateInfo { begin_timestamp = beginTimestamp, end_timestamp = endTimestamp, type = "1" };
            }
            if (gift.AvailableDays.HasValue)
            {
                infoBase.date_info = new Card_BaseInfo_DateInfo { fixed_begin_term = 0, fixed_term = gift.AvailableDays.Value, type = "2" };
            }

            if (gift.Type == 1)//满减类
            {


                infoBase.title = $"￥{gift.DiscountPrice} 代金券";

                var cardInfo = new Card_CashData
                {
                    least_cost = (int)(gift.OrderLimit.Value * 100),

                    reduce_cost = (int)(gift.DiscountPrice.Value * 100),

                    base_info = infoBase
                };

                var result = await CardApi.CreateCardAsync(_wxParams.AppId, cardInfo);

                return result.card_id;
            }

            if (gift.Type == 2)//打折类
            {
                infoBase.title = $"{(long)gift.DiscountPrice.Value * 100}折 折扣券";

                if (gift.OrderLimit.HasValue)
                {
                    infoBase.title = $"满￥{(long)gift.OrderLimit}打{(long)gift.DiscountPrice.Value * 100}折";
                }
                else
                {
                    infoBase.title = $"{(long)gift.DiscountPrice.Value * 100}折优惠券";
                }

                var discount = 100L - (long)gift.DiscountRate.Value * 100;

                var cardInfo = new Card_DisCountData { discount = discount };

                var result = await CardApi.CreateCardAsync(_wxParams.AppId, cardInfo);

                return result.card_id;
            }

            return null;
        }

        //投放卡券
        public async Task DeliveryCardAsync(string openId,string cardId)
        {
             var result = await CustomApi.SendCardAsync(_wxParams.AppId, openId, cardId, null);
        }
    }
}

