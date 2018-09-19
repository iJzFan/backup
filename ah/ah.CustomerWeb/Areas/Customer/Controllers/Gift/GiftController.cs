using ah.Models.ViewModel;
using ah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ah.Areas.Customer.Controllers.Gift
{

    public class GiftController : QueryController
    {

        private GiftService _giftService;

        public GiftController(GiftService giftService)
        {
            _giftService = giftService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {


            var points = await _giftService.GetCurrentPoints(GetCurrentLoginUser.CustomerId);

            var pointsDetail = await _giftService.GetPointsDetailAsync(GetCurrentLoginUser.CustomerId);

            return View("GiftMain", new GiftMainViewModel
            {
                CustomerId = GetCurrentLoginUser.CustomerId,
                CustomerName = GetCurrentLoginUser.CustomerName,
                Points = points,
                PointsDetail = pointsDetail
            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> PointsDetail(int index)
        {

            var pointsDetail = await _giftService.GetPointsDetailAsync(GetCurrentLoginUser.CustomerId, index);

            return PartialView("_pvPointsDetail", pointsDetail);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> List(int index = 1)
        {
            var model = await _giftService.GetListAsync(index);

            return View("_pvGiftList", model);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetById(int giftId)
        {

            var model = await _giftService.GetByIdAsync(giftId);

            return View("_pvGift", model);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ExchangeGift(int giftId, int count, string customerName, string phoneNumber, string address)
        {
            if (!Request.Cookies.ContainsKey("WXInfo"))
            {
                return Ok(new { rlt = false, state = "fail", msg = "请使用微信端进行兑换" });//通过微信端才能获得微信卡券推送
            }

            var model = await _giftService.ExchangeGiftAsync(GetCurrentLoginUser.CustomerId, giftId, count, customerName, phoneNumber, address);

            return Ok(model);
        }
        //暂时加了页面跳转
        [HttpGet("[action]")]
        public async Task<IActionResult> OrderInfo(long giftOrderId)
        {

            var model = await _giftService.GetGiftOrderAsync(GetCurrentLoginUser.CustomerId, giftOrderId);

            return View(model);
        }

        //暂时加了页面跳转
        [HttpGet("[action]")]
        public async Task<IActionResult> OrderList(int? index = 1, int? pageSize = 10)
        {

            var model = await _giftService.GetGiftOrderListAsync(GetCurrentLoginUser.CustomerId, index.Value, pageSize.Value);

            return View(model);
        }

        //暂时加了页面跳转
        [HttpGet("[action]")]
        public async Task<IActionResult> UseGift(long giftOrderId, int customerId)
        {

            var model = await _giftService.GetGiftOrderAsync(customerId, giftOrderId);
            return View(model);
        }

    }
}
