using ah.Code.MyExpands;
using ah.Code.Utility;
using ah.Models.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ah.Services
{
    public class FollowListService:BaseService
    {

        private readonly string _getUrl = Global.CHIS_HOST + "/customer/followlist?customerId=";

        private readonly string _postUrl = Global.CHIS_HOST + "/customer/followlist";

        public FollowListService()
        {
        }

        //private async Task PostDataAsync(FollowListDataModel data)
        //{
        //    var json = JObject.FromObject(data);

        //    var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

        //    await _client.PostAsync(_postUrl, content);
        //}

        //private async Task<FollowListDataModel> GetDataAsync(int customerId)
        //{
        //    var res = await _client.GetAsync(_getUrl + customerId.ToString());

        //    if (res.StatusCode != HttpStatusCode.OK)
        //    {
        //        throw new ApplicationException("网络错误请重试！");
        //    }

        //    var data = await ToClassAsync<FollowListDataModel>(res);

        //    return data;
        //}

        /// <summary>
        /// 获取用户的关注和历史列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<FollowListViewModel> GetListAsync(int customerId)
        {
            var data = await GetDataAsync<FollowListDataModel>(_getUrl+customerId.ToString());

            var model = new FollowListViewModel();

            model.CustomerId = data.CustomerId;

            var followDoctorIds = data.FollowDoctorIds.ToNotNullList<int>();
            model.FollowDoctorIds = followDoctorIds;
            model.FollowDoctorCount = followDoctorIds.Count;

            var followStationIds = data.FollowStationIds.ToNotNullList<int>();
            model.FollowStationIds = followStationIds;
            model.FollowStationCount = followStationIds.Count;

            var recentDoctorIds = data.RecentDoctorIds.ToNotNullList<int>();
            recentDoctorIds.Reverse();
            model.RecentDoctorIds = recentDoctorIds;
            model.RecentDoctorCount = recentDoctorIds.Count;

            var recentStationIds = data.RecentStationIds.ToNotNullList<int>();
            recentStationIds.Reverse();
            model.RecentStationIds = recentStationIds;
            model.RecentStationCount = recentStationIds.Count;

            return model;
        }

        /// <summary>
        /// 更新用户的关注列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <param name="isFollow">关注或取关</param>
        /// <returns></returns>
        public async Task UpdateListAsync(int customerId, int? stationId, int? doctorId, bool isFollow)
        {
            var data = await GetDataAsync<FollowListDataModel>(_getUrl + customerId.ToString());

            data.CustomerId = customerId;

            if (stationId.HasValue)
            {
                var followStationIds = data.FollowStationIds.ToNotNullList<int>();

                if (isFollow)
                {
                    if (followStationIds.Contains(stationId.Value))
                    {
                        throw new ApplicationException("你已关注该诊所！");
                    }

                    if (followStationIds.Count >= Global.MaxStationFollow)
                    {
                        throw new ApplicationException("已达到关注上限！");
                    }

                    followStationIds.Add(stationId.Value);

                    data.FollowStationIds = followStationIds.ToJson();
                }
                else
                {
                    if (!followStationIds.Contains(stationId.Value))
                    {
                        throw new ApplicationException("你没有关注该诊所！");
                    }

                    followStationIds.Remove(stationId.Value);

                    data.FollowStationIds = followStationIds.ToJson();
                }
            }

            if (doctorId.HasValue)
            {
                var followDoctorIds = data.FollowDoctorIds.ToNotNullList<int>();

                if (isFollow)
                {
                    if (followDoctorIds.Contains(doctorId.Value))
                    {
                        throw new ApplicationException("你已关注该医生！");
                    }

                    if (followDoctorIds.Count >= Global.MaxDoctorFollow)
                    {
                        throw new ApplicationException("已达到关注上限！");
                    }

                    followDoctorIds.Add(doctorId.Value);

                    data.FollowDoctorIds = followDoctorIds.ToJson();
                }
                else
                {
                    if (!followDoctorIds.Contains(doctorId.Value))
                    {
                        throw new ApplicationException("你没有关注该医生！");
                    }

                    followDoctorIds.Remove(doctorId.Value);

                    data.FollowDoctorIds = followDoctorIds.ToJson();
                }
            }

            await PostDataAsync(_postUrl,data);
        }

        /// <summary>
        /// 关注置顶
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public async Task MoveFollowToHead(int customerId, int? stationId, int? doctorId)
        {
            var data = await GetDataAsync<FollowListDataModel>(_getUrl + customerId.ToString());

            if (stationId.HasValue)
            {
                var followStationIds = data.FollowStationIds.ToNotNullList<int>();

                followStationIds.Remove(stationId.Value);
                followStationIds.Add(stationId.Value);
            }

            if (doctorId.HasValue)
            {
                var followDoctorIds = data.FollowStationIds.ToNotNullList<int>();

                followDoctorIds.Remove(doctorId.Value);
                followDoctorIds.Add(doctorId.Value);
            }
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="recent">全部,诊所,医生</param>
        /// <returns></returns>
        public async Task DeleteRecentListAsync(int customerId,RecentType recent)
        {
            var data = await GetDataAsync<FollowListDataModel>(_getUrl + customerId.ToString());

            if(recent == RecentType.All)
            {
                data.RecentDoctorIds = null;
                data.RecentStationIds = null;
            }

            if (recent == RecentType.Doctor)
            {
                data.RecentDoctorIds = null;
            }

            if (recent == RecentType.Station)
            {
                data.RecentStationIds = null;
            }

            await PostDataAsync(_postUrl,data);
        }

        /// <summary>
        /// 更新用户历史记录
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public async Task UpdateRecentListAsync(int customerId,int? stationId,int? doctorId)
        {
            var data = await GetDataAsync<FollowListDataModel>(_getUrl + customerId.ToString());

            if (stationId.HasValue)
            {
                var recentStationIds = data.RecentStationIds.ToNotNullList<int>();

                if (recentStationIds.Count >= Global.MaxStationRecent)
                {
                    recentStationIds.RemoveAt(0);
                }

                if (recentStationIds.Contains(stationId.Value))
                {
                    recentStationIds.Remove(stationId.Value);
                    recentStationIds.Add(stationId.Value);
                }
                else
                {
                    recentStationIds.Add(stationId.Value);
                }

                data.RecentStationIds = recentStationIds.ToJson();
            }

            if (doctorId.HasValue)
            {
                var recentDoctorIds = data.RecentDoctorIds.ToNotNullList<int>();

                if (recentDoctorIds.Count >=Global.MaxDoctorRecent)
                {
                    recentDoctorIds.RemoveAt(0);
                }

                if (recentDoctorIds.Contains(doctorId.Value))
                {
                    recentDoctorIds.Remove(doctorId.Value);
                    recentDoctorIds.Add(doctorId.Value);
                }
                else
                {
                    recentDoctorIds.Add(doctorId.Value);
                }

                data.RecentDoctorIds = recentDoctorIds.ToJson();
            }

            await PostDataAsync(_postUrl,data);
        }
    }
}
