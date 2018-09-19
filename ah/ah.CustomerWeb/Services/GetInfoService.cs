using ah.Code.Utility;
using ah.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ah.Services
{
    public class GetInfoService:BaseService
    {
        private readonly static string _getDoctorsUrl = Global.CHIS_HOST + "/openapi/Doctor/JetDoctorByIds?doctorIds=";

        private readonly static string _getStationUrl = Global.CHIS_HOST + "/openapi/Station/JetTreatStationsByIds?stationIds=";

        private readonly static string _searchDoctorsUrl = Global.CHIS_HOST + "/openapi/Doctor/SearchTreatDoctors";

        private readonly static string _searchStationUrl = Global.CHIS_HOST + "/openapi/Healthor/SearchTreatStation";

        public GetInfoService()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// 获取医生详情
        /// </summary>
        /// <param name="doctorIds"></param>
        /// <returns></returns>
        public async Task<JsonListViewModel<DoctorSimpleInfo>> GetDoctorsInfosAsync(IEnumerable<int> doctorIds)
        {
            var doctorIdsStr = "";

            foreach (var doctorId in doctorIds)
            {
                doctorIdsStr = doctorIdsStr + doctorId.ToString() + ",";
            }

            var doctorRes = await _client.GetAsync(_getDoctorsUrl + doctorIdsStr);

            var doctorModel = await ToClassAsync<JsonListViewModel<DoctorSimpleInfo>>(doctorRes);

            return doctorModel;
        }

        /// <summary>
        /// 获取工作站详情
        /// </summary>
        /// <param name="stationIds"></param>
        /// <returns></returns>
        public async Task<JsonListViewModel<StationSimpleInfo>> GetStationsInfosAsync(IEnumerable<int> stationIds)
        {
            var stationIdsStr = "";

            foreach (var stationId in stationIds)
            {
                stationIdsStr = stationIdsStr + stationId.ToString() + ",";
            }

            var stationRes = await _client.GetAsync(_getStationUrl + stationIdsStr);

            var stationModel = await ToClassAsync<JsonListViewModel<StationSimpleInfo>>(stationRes);

            return stationModel;
        }

        /// <summary>
        /// 搜索医生信息
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonListViewModel<DoctorSimpleInfo>> SearchDoctorsInfosAsync(string searchText = null, int pageIndex = 1, int pageSize = 20)
        {
            var searchStr = "?searchText=" +
                HttpUtility.UrlEncode(string.IsNullOrEmpty(searchText) ? "" : searchText) +
                "&pageIndex=" +
                pageIndex.ToString() +
                "&pageSize" +
                pageSize.ToString();

            var doctorRes = await _client.GetAsync(_searchDoctorsUrl + searchStr);

            var doctorModel = await ToClassAsync<JsonListViewModel<DoctorSimpleInfo>>(doctorRes);

            return doctorModel;
        }

        /// <summary>
        /// 搜索附近的工作站
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonListViewModel<StationInfo>> SearchNearStationAsync(string searchText, double? lat, double? lng, int? pageIndex = 1, int? pageSize = 20)
        {
            var searchStr = "?searchText=" +
                HttpUtility.UrlEncode(string.IsNullOrEmpty(searchText) ? "" : searchText) +
                "&pageIndex=" +
                pageIndex +
                "&pageSize=" +
                pageSize;

            if (lat.HasValue)
            {
                searchStr = searchStr + "&lat=" + lat.ToString();
            }

            if (lng.HasValue)
            {
                searchStr = searchStr + "&lng=" + lng.ToString();
            }

            var stationRes = await _client.GetAsync(_searchStationUrl + searchStr);

            var stationModel = await ToClassAsync<JsonListViewModel<StationInfo>>(stationRes);

            return stationModel;
        }
    }
}
