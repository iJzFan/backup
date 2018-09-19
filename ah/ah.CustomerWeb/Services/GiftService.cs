using System;
using System.Reflection;
using System.Threading.Tasks;
using ah.Models.ViewModel;

namespace ah.Services
{
    public class GiftService : BaseService
    {
        private readonly string _getbyIdUrl = Global.CHIS_HOST + "/openapi/gift/getbyid?giftId=";

        private readonly string _getListUrl = Global.CHIS_HOST + "/openapi/gift/index";

        private readonly string _getCurrentPointsUrl = Global.CHIS_HOST + "/openapi/gift/currentpoints?customerid=";

        private readonly string _getPointsDetailUrl = Global.CHIS_HOST + "/openapi/Gift/PointsDetail?customerId=";

        private readonly string _postGiftOrderUrl = Global.CHIS_HOST + "/openapi/Gift/CreateOrder";
       
        private readonly string _getGiftOrderUrl = Global.CHIS_HOST + "/openapi/Gift/GiftOrderDetail";

        private readonly string _getGiftOrderListUrl = Global.CHIS_HOST + "/openapi/Gift/GiftOrderList";

        public async Task<GiftViewModel> GetByIdAsync(int giftId)
        {
            var model = await GetDataAsync<GiftViewModel>(_getbyIdUrl + giftId.ToString());

            return model;
        }

        public async Task<PaginatedItemsViewModel<GiftViewModel>> GetListAsync(int index = 1, int pageSize = 10)
        {
            var model = await GetDataAsync<PaginatedItemsViewModel<GiftViewModel>>(_getListUrl + "?index=" + index.ToString() + "&pageSize=" + pageSize.ToString());

            return model;
        }

        public async Task<long> GetCurrentPoints(int customerId)
        {
            var points = await GetDataAsync<long>(_getCurrentPointsUrl + customerId.ToString());

            return points;
        }

        public async Task<PaginatedItemsViewModel<PointsDetailViewModel>> GetPointsDetailAsync(int customerId, int index = 1, int pageSize = 10)
        {
            var model = await GetDataAsync<PaginatedItemsViewModel<PointsDetailViewModel>>(_getPointsDetailUrl +
                customerId.ToString() +
                "&index=" +
                index.ToString() +
                "&pageSize=" +
                pageSize.ToString());

            return model;
        }

        public async Task<Object> ExchangeGiftAsync(int customerId, int giftId, int count, string customerName, string phoneNumber, string address)
        {
            var model = await PostDataAsync<Object>(_postGiftOrderUrl, new
            {
                CustomerId = customerId,
                GiftId = giftId,
                Count = count,
                CustomerName = customerName,
                PhoneNumber = phoneNumber,
                Address = address
            });

            return model;
        }

        public async Task<GiftOrderViewModel> GetGiftOrderAsync(int customerId, long giftOrderId)
        {
            var model = await GetDataAsync<GiftOrderViewModel>(_getGiftOrderUrl + "?customerId=" + customerId.ToString() + "&giftOrderId=" + giftOrderId.ToString());

            model.CheckOrderState();

            return model;
        }

        public async Task<PaginatedItemsViewModel<GiftOrderViewModel>> GetGiftOrderListAsync(int customerId, int index, int pageSize)
        {
            var model = await GetDataAsync<PaginatedItemsViewModel<GiftOrderViewModel>>(_getGiftOrderListUrl + "?customerId=" + customerId.ToString() + "&index=" + index.ToString() + "&pageSize=" + pageSize.ToString());

            foreach(var item in model.Rows)
            {
                item.CheckOrderState();
            }

            return model;
        }


    }
}