using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alipay.AopSdk.Core.Request;
using CHIS.Code.Filter;
using CHIS.DbContext;
using CHIS.Models;
using CHIS.Models.ViewModel;
using CHIS.Services;
using Microsoft.AspNetCore.Mvc;

namespace CHIS.Controllers.Gift
{
    [Route("[controller]")]
    public class GiftController : BaseController
    {
        #region Constructor

        private GiftService _giftService;

        private GiftOrderService _orderService;

        private WeChatService _weChatService;

        public GiftController(GiftOrderService orderService, GiftService giftService,WeChatService weChatService, CHISEntitiesSqlServer db) : base(db)
        {
            _giftService = giftService;

            _orderService = orderService;

            _weChatService = weChatService;
        }

        #endregion

        /// <summary>
        /// 礼品详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id = 1)
        {
            var model = _giftService.GetGift(id);

            return View("GiftDetail", model);
        }

        /// <summary>
        /// 编辑礼品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("[action]/{id}")]
        public IActionResult Edit(int id = 1)
        {
            var model = _giftService.GetGift(id);

            return View(model);
        }

        [HttpPost("[action]")]
        public IActionResult Edit(GiftViewModel model)
        {
            try
            {
                _giftService.UpdateGift(model);

                return Ok(new {rlt = true});
            }
            catch (Exception e)
            {
                return Ok(new {rlt=false,msg=e.Message });
            }
        }

        /// <summary>
        /// 礼品首页
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 礼品列表
        /// </summary>
        /// <param name="searchText">礼品名字搜索</param>
        /// <param name="giftStatus">OutOfStock:库存不足;Expire:活动结束,已过期;</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public IActionResult GetGiftList(string searchText, string giftStatus = "", int pageIndex = 1, int pageSize = 20)
        {
            var model = _giftService.SearchGifts(searchText, giftStatus, pageIndex, pageSize);
            return PartialView("_pvGiftList", model);
        }
        /// <summary>
        /// 发布新礼品
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("[action]")]
        [ValidationFilter]
        public async Task<IActionResult> Create(GiftInputModel model)
        {

            if (model.WeChatCardId == null)// 没有通过公众号制作卡券则自动生成
            {
                model.WeChatCardId = await _weChatService.CreateWeChatCardAsync(model);
            }
            
            var giftId = _giftService.PublishGitf(model);

            return Redirect($"/Gift/{giftId}");
        }

        /// <summary>
        /// 删除礼品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("[action]/{id}")]
        public IActionResult Delete(int id)
        {
            _giftService.DeleteGift(id);

            return Redirect("/gift/Index");
        }
        ///<summary>
        /// 订单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public IActionResult OrderIndex()
        {
            return View();
        }


        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="searchText">收货人或者orderId</param>
        /// <param name="orderStatus">Shipped:已发货,UnShipped:未发货,Virtual:虚拟商品</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// <returns></returns>
        [HttpPost("[action]")]
        public IActionResult GiftOrderList(string searchText, string orderStatus = "UnShipped", int pageIndex = 1, int pageSize = 20)
        {
            var model = _orderService.GetGiftOrderList(searchText, orderStatus, pageIndex, pageSize);
            ViewBag.orderStatus = orderStatus;
            return PartialView("_pvGiftOrderList", model);
        }

        /// <summary>
        /// 发货
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]/{giftOrderId}")]
        public IActionResult ShipView(long giftOrderId)
        {
            var model = new CHIS.Models.CHIS_Gift_Order() {
                GiftOrderId = giftOrderId
            };
            return PartialView("_pvShipView",model);
        }

        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="giftOrderId"></param>
        /// <param name="shipper">快递公司</param>
        /// <param name="logisticCode">快递单号</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public IActionResult Ship(long giftOrderId,string shipper,string logisticCode)
        {
            try
            {
                _orderService.Ship(giftOrderId, shipper, logisticCode);

                return Ok(new { rlt = "success" });
            }
            catch(Exception e)
            {
                return BadRequest(new { rlt = "fail", msg = e.Message });
            }
        }

        /// <summary>
        /// 编辑订单
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]/{giftOrderId}")]
        public IActionResult EditOrder(long giftOrderId)
        {
            var model = _orderService.GetGiftOrder(giftOrderId);

            return PartialView("_pvEditOrder", model);
        }

        /// <summary>
        /// 编辑订单
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public IActionResult EditOrder(GiftOrderViewModel model)
        {
             _orderService.EditOrder(model);

            return Redirect(nameof(OrderIndex));
        }

    }
}
