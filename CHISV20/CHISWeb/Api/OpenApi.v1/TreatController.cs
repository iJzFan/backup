using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ass;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 接诊业务
    /// </summary>
    public class TreatController : OpenApiBaseController
    {
        Services.TreatService _treatSvr;
        Services.ReservationService _resvSvr;
        Services.DoctorService _docSvr;
        public TreatController(DbContext.CHISEntitiesSqlServer db
            , Services.TreatService treatSvr
            , Services.ReservationService resvSvr
            , Services.DoctorService docSvr
            ) : base(db)
        {
            this._treatSvr = treatSvr;
            _resvSvr = resvSvr;
            _docSvr = docSvr;
        }

        //=========================================================================================

        #region 诊断部分的操作

        /// <summary>GetDiagnosis
        /// 获取诊断
        /// </summary>
        /// <param name="searchText">搜索文字</param>
        [HttpGet]
        public dynamic JetDiagnosis(string searchText, int? pageIndex = 1, int? pageSize = 30)
        {
            try
            {
                var items = _treatSvr.GetDiagnosis(searchText, pageIndex.Value, pageSize.Value).Select(m => new
                {
                    label = m.DiagnoisisName,
                    value = m.DiagnoisisId,
                    itemId = m.DiagnoisisId,
                    itemValue = m.PyCode.Trim(),
                    itemTypeCode = m.DiagTypeCode
                });
                var dd = MyDynamicResult(true, "");
                dd.items = items;
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }

        /// <summary>
        /// 根据诊断Id获取诊断信息
        /// </summary>
        /// <param name="zdid">诊断Id</param>
        /// <returns>诊断信息</returns>
        [HttpGet]
        public IActionResult GetDiagnosisById(int zdid)
        {
            return Json(_treatSvr.GetDiagnosisById(zdid));
        }


        /// <summary>
        /// 添加一个诊断
        /// </summary>
        /// <param name="diagName">诊断名称</param>
        /// <param name="diagTypeCode">STANDARD标准 USERDEFINED用户自定义</param>
        /// <returns>是否成功</returns>
        [HttpGet]
        public async Task<dynamic> AddDiagnosis(string diagName, string diagTypeCode = "USERDEFINED", string diagVal = "")
        {
            try
            {
                var entity = new Models.DataModel.DiagnosisModel
                {
                    DiagnoisisName = Ass.P.PStr(diagName).Trim(),
                    TypeCode = diagTypeCode
                };
                if (diagVal.IsNotEmpty()) entity.DiagnoisisValue = diagVal;
                var rlt = await _treatSvr.AddDiagnosisAsync(entity);
                var dd = MyDynamicResult(true, "");
                dd.item = rlt;
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }

        /// <summary>
        /// 删除一个诊断,非自定义诊断不可删除
        /// </summary>
        /// <param name="diagId">诊断Id</param>
        /// <returns></returns>
        [HttpGet]
        public dynamic DeleteDiagnosis(int diagId)
        {

            try
            {
                var rlt = _treatSvr.DeleteUserDiagnosis(diagId);
                var dd = MyDynamicResult(true, "");
                return dd;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }


        #endregion


        #region 医生的约诊信息

        /// <summary>
        /// 获取医生的接诊清单，默认当日
        /// </summary>
        /// <param name="doctorId">医生Id</param>
        /// <param name="dt0">开始时间 默认当日</param>
        /// <param name="dt1">结束时间 不包含</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param> 
        [HttpGet]
        public dynamic GetDoctorRegistList(int doctorId, DateTime? dt0, DateTime? dt1, int? pageIndex = 1, int? pageSize = 20)
        {
            if (!dt0.HasValue) dt0 = DateTime.Today;
            if (!dt1.HasValue) dt1 = DateTime.Today.AddDays(30);
            return _resvSvr.GetDoctorRegistList(doctorId, dt0.Value, dt1.Value, pageIndex.Value, pageSize.Value).Select(m => new
            {
                RegisterId = m.RegisterID,
                m.CustomerName,
                CustomerGender = (m.Gender ?? 2).ToGenderString(),
                BirthdayStr = m.Birthday.HasValue ? m.Birthday.Value.ToAgeString() : "未知",
                m.DoctorName,
                m.DepartmentName,
                m.StationName,
                m.RegisterDate,
                RegisterSlot = DictValues.WorkSlot_V.Ins().GetName(Ass.P.PStr(m.RegisterSlot, "", true)),
                m.RegisterSeq,
                m.RegisterTreatTypeName,
                m.TreatStatus,
                m.TreatTime,
                m.RegisterFromName,
                CusotmerPic = m.PhotoUrlDef.ahDtUtil().GetCustomerImg(m.Gender)
            });
        }

        /// <summary>
        /// 获取医生的接诊清单，默认当日
        /// </summary>
        /// <param name="doctorAppId">医生AppId</param>
        /// <param name="dt0">开始时间 默认当日</param>
        /// <param name="dt1">结束时间 不包含</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param> 
        [HttpGet]
        public dynamic GetDoctorRegistListByDoctorAppId(string doctorAppId, DateTime? dt0, DateTime? dt1, int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                var doctor = _docSvr.FindByAppId(doctorAppId);
                if (doctor == null) throw new Exception("没有该医生信息");
                var rlt = this.GetDoctorRegistList(doctor.DoctorId, dt0, dt1, pageIndex, pageSize);
                var rtn = MyDynamicResult(true, "");
                rtn.data = rlt;
                rtn.pageIndex = pageIndex;
                rtn.pageSize = pageSize;
                return rtn;
            }
            catch (Exception ex)
            {
                var rtn = MyDynamicResult(ex);
                rtn.data = ex.Message;
                return rtn;
            }
        }

        /// <summary>
        /// 获取医生更新的接诊数据
        /// </summary>
        /// <param name="lastRegistId">最后的接诊Id</param>
        /// <returns></returns>
        [HttpGet]
        public dynamic GetDoctorNewerRegistList(long lastRegistId)
        {
            try
            {
                var rlt = _resvSvr.GetDoctorNewerRegistList(lastRegistId).Select(m => new
                {
                    RegisterId = m.RegisterID,
                    m.CustomerName,
                    CustomerGender = (m.Gender ?? 2).ToGenderString(),
                    BirthdayStr = m.Birthday.HasValue ? m.Birthday.Value.ToAgeString() : "未知",
                    m.DoctorName,
                    m.DepartmentName,
                    m.StationName,
                    m.RegisterDate,
                    RegisterSlot = DictValues.WorkSlot_V.Ins().GetName(Ass.P.PStr(m.RegisterSlot, "", true)),
                    m.RegisterSeq,
                    m.RegisterTreatTypeName,
                    m.TreatStatus,
                    m.TreatTime,
                    m.RegisterFromName,
                    CusotmerPic = m.PhotoUrlDef.ahDtUtil().GetCustomerImg(m.Gender)
                });
                var rtn = MyDynamicResult(true, "");
                rtn.data = rlt;
                return rtn;
            }
            catch (Exception ex)
            {
                var rtn = MyDynamicResult(ex);
                rtn.data = ex.Message;
                return rtn;
            }
        }

        #endregion

    }
}
