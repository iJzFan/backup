using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ah.Code.Utility;
using ah.DbContext;
using ah.Models.ViewModel;
using ah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ah.Areas.Customer.Controllers
{

    public class FollowController : QueryController
    {
        private FollowListService _followListService;

        private GetInfoService _getInfoService;

        public FollowController(FollowListService followListService, GetInfoService getInfoService)
        {
            _followListService = followListService;
            _getInfoService = getInfoService;
        }

        /// <summary>
        /// 获取关注列表
        /// </summary>
        /// <returns>PartialView</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var followModel = await _followListService.GetListAsync(GetCurrentLoginUser.CustomerId);

            var stationModel = new JsonListViewModel<StationSimpleInfo>();

            var doctorModel = new JsonListViewModel<DoctorSimpleInfo>();

            if (followModel.FollowStationCount != 0)
            {
                stationModel = await _getInfoService.GetStationsInfosAsync(followModel.FollowStationIds);
                foreach(var item in stationModel.Items)
                {
                    item.IsFollow = true;
                }
            }

            if (followModel.FollowDoctorCount != 0)
            {
                doctorModel = await _getInfoService.GetDoctorsInfosAsync(followModel.FollowDoctorIds);
                foreach (var item in doctorModel.Items)
                {
                    item.IsFollow = true;
                }
            }

            return PartialView("_pvFollowList", new MyFollowsViewModel
            {
                Doctors = doctorModel.Items,
                Stations = stationModel.Items
            });
        }

        /// <summary>
        /// 获取历史记录
        /// </summary>
        /// <returns>PartialView</returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> Recent()
        {
           var recentModel = await _followListService.GetListAsync(GetCurrentLoginUser.CustomerId);

            var stationModel = new JsonListViewModel<StationSimpleInfo>();

            var doctorModel = new JsonListViewModel<DoctorSimpleInfo>();

            if (recentModel.RecentStationCount != 0)
            {
                stationModel = await _getInfoService.GetStationsInfosAsync(recentModel.RecentStationIds);
            }

            if (recentModel.RecentDoctorCount != 0)
            {
                doctorModel = await _getInfoService.GetDoctorsInfosAsync(recentModel.RecentDoctorIds);
            }

            return PartialView("_pvRecentList", new MyFollowsViewModel
            {
                Doctors = doctorModel.Items,
                Stations = stationModel.Items
            });
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        /// <returns>PartialView</returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> DeleteRecent()
        {
            await _followListService.DeleteRecentListAsync(GetCurrentLoginUser.CustomerId, RecentType.All);

            return PartialView("_pvRecentList", new MyFollowsViewModel());
        }

        /// <summary>
        /// 关注取关诊所或者医生
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <param name="isFollow">关注true,取关false</param>
        /// <returns>{rlt:true}/{rlt:false msg:"error"}</returns>
        [HttpPost]
        [ProducesResponseType(typeof(JsonReturn),200)]
        public async Task<IActionResult> Post(int? stationId, int? doctorId, bool isFollow)
        {
            try
            {
                await _followListService.UpdateListAsync(GetCurrentLoginUser.CustomerId, stationId, doctorId, isFollow);

                return Ok(new { rlt = true });
            }
            catch (ApplicationException e)
            {
                return Ok(new { rlt = false, msg = e.Message });
            }
            catch (HttpRequestException e)
            {
                return Ok(new { rlt = false, msg = e.Message });
            }

        }
    }
}
