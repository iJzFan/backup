using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CHIS.DbContext;
using CHIS.Models;
using CHIS.Models.InputModel;
using CHIS.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Services
{
    public class GiftOrderService : BaseService
    {
        private IMapper _mapper;

        private WeChatService _weChatService;

        public GiftOrderService(CHISEntitiesSqlServer db, IMapper mapper, WeChatService weChatService) : base(db)
        {
            _mapper = mapper;
            _weChatService = weChatService;
        }

        public GiftOrderViewModel GetGiftOrder(long giftOrderId, int customerId)
        {
            var entity = _db.CHIS_Gift_Order.Where(x => x.GiftOrderId == giftOrderId && x.CustomerId == customerId).Include(x => x.Gift).SingleOrDefault();

            var model = _mapper.Map<CHIS_Gift_Order, GiftOrderViewModel>(entity);

            return model;
        }

        public GiftOrderViewModel GetGiftOrder(long giftOrderId)
        {
            var entity = _db.CHIS_Gift_Order.Where(x => x.GiftOrderId == giftOrderId).Include(x => x.Gift).SingleOrDefault();

            var model = _mapper.Map<CHIS_Gift_Order, GiftOrderViewModel>(entity);

            return model;
        }


        public PaginatedItemsViewModel<GiftOrderViewModel> GetGiftOrderList(int customerId, int index = 1, int pageSize = 10)
        {
            if (index < 0 || pageSize < 0)
            {
                index = 1;
                pageSize = 10;
            }

            var root = _db.CHIS_Gift_Order.Include(x => x.Gift).Where(x => x.CustomerId == customerId);

            var count = root.Count();

            var entities = root.OrderByDescending(x => x.GiftOrderId).Skip(index - 1).Take(pageSize).ToList();

            var models = _mapper.Map<List<CHIS_Gift_Order>, List<GiftOrderViewModel>>(entities);

            return new PaginatedItemsViewModel<GiftOrderViewModel>(index, pageSize, count, models);
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="model"></param>
        public async Task CreateGiftOrderAsync(GiftOrderInputModel model)
        {
            var entity = _mapper.Map<GiftOrderInputModel, CHIS_Gift_Order>(model);

            entity.Create();

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var gift = _db.CHIS_Code_Gift.SingleOrDefault(x => x.GiftId == entity.GiftId);

                    if (gift.Type > 0)
                    {
                        var customer = _db.CHIS_Code_Customer.SingleOrDefault(x => x.CustomerID == entity.CustomerId);

                        if (entity.Count > 1)
                        {

                            //虚拟券类拆分订单

                            var list = new List<CHIS_Gift_Order>();

                            //var count = entity.Count;

                            //entity.Count = 1;

                            for (int i = 0; i < entity.Count; i++)
                            {
                                list.Add(new CHIS_Gift_Order { CreatedTime = entity.CreatedTime, Count = 1, CustomerId = entity.CustomerId, GiftId = entity.GiftId });
                            }
                            foreach (var item in list)
                            {
                                CommitOrder(item);
                            }

                            transaction.Commit();

                            for (int i = 0; i < entity.Count; i++)
                            {//投放卡券
                                await _weChatService.DeliveryCardAsync(customer.WXOpenId, gift.WeChatCardId);
                            }

                        }
                        else
                        {
                            CommitOrder(entity);

                            transaction.Commit();

                            await _weChatService.DeliveryCardAsync(customer.WXOpenId, gift.WeChatCardId);
                        }
                    }
                    else
                    {
                        CommitOrder(entity);

                        transaction.Commit();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    throw;
                }
            }
        }

        /// <summary>
        /// 使用礼品券
        /// </summary>
        /// <param name="giftOrderId"></param>
        /// <param name="customerId"></param>
        /// <param name="spendingPlace">消费地点</param>
        public void UseGift(long giftOrderId, int customerId, string spendingPlace)
        {
            var entity = _db.CHIS_Gift_Order.Where(x => x.GiftOrderId == giftOrderId && x.CustomerId == customerId).Include(x => x.Gift).SingleOrDefault();

            if (entity == null)
            {
                throw new ApplicationException("礼品券信息不存在");
            }

            entity.UseGift(spendingPlace);

            _db.SaveChanges();
        }

        /// <summary>
        /// 使用礼品券
        /// </summary>
        /// <param name="cardCode"></param>
        /// <param name="staffOpenId"></param>
        /// <param name="spendingPlace">消费地点</param>
        public void UseGiftByWeChat(string cardCode, string staffOpenId, string spendingPlace)
        {
            var entity = _db.CHIS_Gift_Order.Where(x => x.WeChatCardCode == cardCode && x.IsCompleted != true).Include(x => x.Gift).SingleOrDefault();

            if (entity == null)
            {
                throw new ApplicationException("礼品券信息不存在");
            }

            if (string.IsNullOrWhiteSpace(spendingPlace))
            {
                spendingPlace = "第三方门店"; //TODO: 通过staffOpenId获取门店信息
            }

            entity.UseGift(spendingPlace);

            _db.SaveChanges();
        }

        /// <summary>
        /// 更新礼品券的CardCode
        /// </summary>
        public void UpdateCardCode(string openId, string cardId, string cardCode)
        {
            var entity = _db.CHIS_Gift_Order
                .Include(x => x.Customer)
                .Include(x => x.Gift)
                .OrderByDescending(x => x.GiftId)
                .FirstOrDefault(x => x.Customer.WXOpenId == openId &&
                                     x.Gift.WeChatCardId == cardId &&
                                     x.IsCompleted != true &&
                                     x.WeChatCardCode == null &&
                                     (!x.DeadLine.HasValue || x.DeadLine.Value > DateTime.Now)); //卡券Code由腾讯自动生成,之能通过这种方法绑定

            if (entity == null)
            {
                throw new ApplicationException("礼品券信息不存在");
            }

            entity.WeChatCardCode = cardCode;//TODO:不应该对属性直接赋值

            _db.SaveChanges();
        }

        /// <summary>
        /// 礼品发货
        /// </summary>
        /// <param name="giftOrderId"></param>
        /// <param name="shipper">快递公司</param>
        /// <param name="logisticCode">快递单号</param>
        public void Ship(long giftOrderId, string shipper, string logisticCode)
        {
            var entity = _db.CHIS_Gift_Order.SingleOrDefault(x => x.GiftOrderId == giftOrderId);

            entity.Ship(shipper, logisticCode);

            _db.SaveChanges();
        }


        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="searchText">收货人或者orderId</param>
        /// <param name="orderStatus">Shipped:已发货,UnShipped:未发货,Virtual:虚拟商品</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedItemsViewModel<GiftOrderViewModel> GetGiftOrderList(string searchText, string orderStatus = "", int pageIndex = 1, int pageSize = 20)
        {
            if (pageIndex < 0 || pageSize < 0)
            {
                pageIndex = 1;
                pageSize = 20;
            }

            var root = _db.CHIS_Gift_Order.Include(x => x.Gift).AsQueryable();

            switch (orderStatus.ToLower())
            {
                case "shipped":
                    root = root.Where(x => x.IsCompleted.Value && x.Address != null);
                    break;
                case "unshipped":
                    root = root.Where(x => x.LogisticCode == null && !string.IsNullOrEmpty(x.Address));
                    break;
                case "virtual":
                    root = root.Where(x => x.Gift.Type > 0);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                root = long.TryParse(searchText, out var orderId) ? root.Where(x => x.GiftOrderId == orderId) : root.Where(x => x.CustomerName.Contains(searchText));
            }

            var count = root.Count();

            var entities = root.OrderByDescending(x => x.GiftOrderId).Skip(pageIndex - 1).Take(pageSize).ToList();

            var models = _mapper.Map<List<CHIS_Gift_Order>, List<GiftOrderViewModel>>(entities);

            return new PaginatedItemsViewModel<GiftOrderViewModel>(pageIndex, pageSize, count, models);
        }

        /// <summary>
        /// 编辑订单
        /// </summary>
        /// <param name="model"></param>
        public void EditOrder(GiftOrderViewModel model)
        {
            var entity = _mapper.Map<GiftOrderViewModel, CHIS_Gift_Order>(model);

            _db.CHIS_Gift_Order.Update(entity);
        }

        private void CommitOrder(CHIS_Gift_Order entity)
        {
            var entityInDb = _db.CHIS_Gift_Order.Add(entity);

            _db.SaveChanges();

            var order = _db.CHIS_Gift_Order.Include(x => x.Gift).Include(x => x.Customer).SingleOrDefault(x => x.GiftOrderId == entityInDb.Entity.GiftOrderId);

            order.ExchangeGift($"兑换礼品 {order.Gift.GiftName} {entity.Count}份", entityInDb.Entity.GiftOrderId);

            _db.SaveChanges();
        }
    }
}
