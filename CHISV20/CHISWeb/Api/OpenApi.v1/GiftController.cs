using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHIS.Code.Filter;
using CHIS.DbContext;
using CHIS.Models.InputModel;
using CHIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CHIS.Api.OpenApi.v1
{
    /// <summary>
    /// 积分兑换
    /// </summary>
    public class GiftController : OpenApiBaseController
    {
        private GiftService _giftService;

        private GiftOrderService _giftOrderService;

        private PointsDetailService _pointsService;

        private ILogger<GiftController> _logger;

        public GiftController(GiftService giftService,
            GiftOrderService giftOrderService,
            PointsDetailService pointsService,
            ILogger<GiftController> logger,
            CHISEntitiesSqlServer db) : base(db)
        {
            _giftService = giftService;
            _giftOrderService = giftOrderService;
            _pointsService = pointsService;
            _logger = logger;
        }

        /// <summary>
        /// 获取兑换商品
        /// </summary>
        /// <param name="giftId">商品Id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetById(int giftId)
        {

            var model = _giftService.GetGift(giftId);

            return Ok(model);

        }

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name="index">页码</param>
        /// <param name="pageSize">页容</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(int index = 1, int pageSize = 10)
        {

            var model = _giftService.GetGiftList(index, pageSize);

            return Ok(model);

        }

        [HttpPost]
        [TypeFilter(typeof(CHISTokenAuth))]
        [ValidationFilter]
        public async Task<IActionResult> CreateOrder([FromBody]GiftOrderInputModel model)
        {

            try
            {
                await _giftOrderService.CreateGiftOrderAsync(model);

                return Ok(new { rlt = true, state = "success" });
            }
            catch (Exception e)
            {
                if (!(e is ApplicationException))
                    _logger.LogError(e, e.Message);

                return Ok(new { rlt = false, state = "fail", msg = e.Message });
            }

        }

        [HttpGet]
        [TypeFilter(typeof(CHISTokenAuth))]
        [ValidationFilter]
        public IActionResult UpdateOrderCode(string openId, string cardId, string cardCode)
        {

            _giftOrderService.UpdateCardCode(openId, cardId, cardCode);

            return Ok();

        }


        [HttpGet]
        [TypeFilter(typeof(CHISTokenAuth))]
        public IActionResult GiftOrderList(int customerId, int index = 1, int pageSize = 10)
        {
            var model = _giftOrderService.GetGiftOrderList(customerId, index, pageSize);

            return Ok(model);

        }

        [HttpGet]
        [TypeFilter(typeof(CHISTokenAuth))]
        public IActionResult GiftOrderDetail(int customerId, long giftOrderId)
        {
            var model = _giftOrderService.GetGiftOrder(giftOrderId, customerId);

            return Ok(model);

        }


        [HttpGet]
        [TypeFilter(typeof(CHISTokenAuth))]
        public IActionResult PointsDetail(int customerId, int index = 1, int pageSize = 10)
        {
            var model = _pointsService.GetPointsDetailList(customerId, index, pageSize);

            return Ok(model);

        }

        [HttpGet]
        [TypeFilter(typeof(CHISTokenAuth))]
        public IActionResult CurrentPoints(int customerId)
        {
            var model = _pointsService.CurrentPoints(customerId);

            return Ok(model);

        }

        [HttpGet]
        [Authorize("ThirdPartAuth")]//第三方接口
        public string UseGift(long giftOrderId, int customerId)
        {
            try
            {
                _giftOrderService.UseGift(giftOrderId, customerId, User.Identity.Name);

                var model = _giftOrderService.GetGiftOrder(giftOrderId, customerId);

                return $"{model.GiftName}使用成功";
            }
            catch (Exception e)
            {
                return $"使用失败,{e.Message}";
            }

        }

        [HttpGet]
        [TypeFilter(typeof(CHISTokenAuth))]//通过微信回调将卡券设置为已使用
        public IActionResult UseGiftByWeChat(string cardCode, string staffOpenId, string spendingPlace)
        {
            try
            {
                _giftOrderService.UseGiftByWeChat(cardCode, staffOpenId, spendingPlace);

                return Ok();
            }
            catch (ApplicationException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [TypeFilter(typeof(CHISTokenAuth))]
        public IActionResult ChangePoints(PointsDetailInputModel model)
        {
            try
            {
                _pointsService.ChangePoints(model);

                return Ok(new { rlt = true, state = "success" });
            }
            catch (Exception e)
            {
                if (!(e is ApplicationException))
                    _logger.LogError(e, e.Message);

                return Ok(new { rlt = false, state = "fail", msg = e.Message });
            }

        }

        [HttpPost]
        [TypeFilter(typeof(CHISTokenAuth))]
        public IActionResult ChangePointsByPointsRule(int customerId, int pointsRuleId, decimal? consumerMoney)
        {
            try
            {
                _pointsService.ChangePoints(customerId, pointsRuleId, consumerMoney);

                return Ok(new { rlt = true, state = "success" });
            }
            catch (Exception e)
            {
                if (!(e is ApplicationException))
                    _logger.LogError(e, e.Message);

                return Ok(new { rlt = false, state = "fail", msg = e.Message });
            }

        }

    }
}
