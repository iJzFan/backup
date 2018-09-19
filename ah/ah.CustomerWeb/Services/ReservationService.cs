using System;
using System.Reflection;
using System.Threading.Tasks;
using ah.Models.ViewModel;

namespace ah.Services
{
    public class ReservationService : BaseService
    {
#if DEBUG
        private readonly string _postResvUrl = "http://192.168.99.138:61450" + "/openapi/Healthor/ReservateDoctor";
        private readonly string _getDctrUrl = "http://192.168.99.138:61450" + "/openapi/Common/GetDoctorOfStation?stationId={0}&doctorId={1}";

#else
        private readonly string _postResvUrl = Global.CHIS_HOST + "/openapi/Healthor/ReservateDoctor";   
        private readonly string _getDctrUrl = Global.CHIS_HOST +  "/openapi/Common/GetDoctorOfStation?stationId={0}&doctorId={1}";
#endif

        public async Task<ResvationReturn> ReservateDoctorAsync(ReservationInfo resvInfo)
        {
            if (resvInfo.CustomerId == 0) throw new Exception("没有传入客户Id");
            if (resvInfo.DoctorId == 0) throw new Exception("没有传入医生Id");
            if (resvInfo.StationId == 0) throw new Exception("没有传入工作站Id");
            if (resvInfo.DepartmentId == 0)
            { 
                var dctr = await GetDataAsync<DoctorSEntityV01>(string.Format(_getDctrUrl, resvInfo.StationId, resvInfo.DoctorId));
                resvInfo.DepartmentId = dctr.DefDepartmentId.Value;
            }
            var model = await PostDataAsync<ResvationReturn>(_postResvUrl, resvInfo);
            return model;
        }


    }
}