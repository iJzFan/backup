using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ass;
using CHIS.Models.DataModel;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CHIS.Models.ViewModel;

namespace CHIS.Api.OpenApi.v2
{

    /// <summary>
    /// 发药
    /// </summary>
    public class DispensingController : CHIS.Api.v2.OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        Services.DrugService _drugSvr;
        Services.DictService _dicSvr;
        Services.DispensingService _dispSvr;
        IMapper _mapper;
        public DispensingController(Services.ReservationService resSvr,
            Services.WorkStationService stationSvr,
            Services.DoctorService docrSvr,
            Services.CustomerService cusSvr,
            Services.DrugService drugSvr,
            Services.DictService dicSvr
            , Services.DispensingService dispSvr
            , IMapper mapper
            ) //: base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
            _drugSvr = drugSvr;
            _dicSvr = dicSvr;
            _dispSvr = dispSvr;
            _mapper = mapper;
        }

        //=========================================================================================

        /// <summary>
        /// 获取发药列表
        /// </summary>
        /// <param name="stationId">接诊站Id</param> 
        /// <param name="dispensingStatus">发药状态 0 未发，1已发 2待退 3已退</param>
        /// <param name="timeRange">时间</param>
        /// <param name="searchText">搜索患者手机</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetNeedDispensingList(string searchText, int stationId,
            string timeRange = "Today", int dispensingStatus = 0,
            int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                if (UserSelf == null && stationId == null)
                    throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录的用户，需要输入参数:stationId");
                var dtt = timeRange.ahDtUtil().TimeRange();
                int? registOpId = null;
                if (UserSelf.MyRoleNames.Contains("drugstore_nurse"))
                {
                    registOpId = UserSelf.OpId;
                }
                var dd = _dispSvr.GetDispenseList(searchText, stationId, dtt.Item1, dtt.Item2, dispensingStatus, pageIndex.Value, pageSize.Value, registOpId);
                var mdd = new Ass.Mvc.PageListInfo<Models.ViewModel.DispensingItem>
                {
                    DataList = _mapper.Map<IEnumerable<DispensingItemViewModel>, IEnumerable<DispensingItem>>(dd.DataList),
                    PageIndex = dd.PageIndex,
                    PageSize = dd.PageSize,
                    RecordTotal = dd.RecordTotal
                };
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }

        /// <summary>
        /// 获取发药详细 支付完成后可以正常查看
        /// </summary>
        /// <param name="treatId">接诊Id</param> 
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetDispenseDetail(int treatId)
        {
            try
            {
                var mdd = _dispSvr.DispensingDetail(treatId).ahDtUtil().NewModel<DispensingDetailViewModel>(m => new
                {

                    m.Treat.DoctorName,
                    m.Treat.Diagnosis1,
                    m.Customer.CustomerName,
                    MailInfo = new
                    {

                        m.SelectedAddress.FullAddress,
                        m.SelectedAddress.ContactName,
                        m.SelectedAddress.Mobile,
                        m.SelectedAddress.Remark,
                        m.SelectedAddress.ZipCode
                    },
                    FormedPrescriptions = m.DispensingDetailSumary.FormedPrescription.Select(a => new
                    {
                        Main = new
                        {
                            a.PrescriptionNo,
                            a.Amount,
                            a.ChargeStatus,
                            IsNeedDispense = m.DispensingDetailSumary.IsNeedFormedDispense(a.PrescriptionNo)
                        },
                        Items = m.DispensingDetailSumary.GetFormedDetailByPreNo(a.PrescriptionNo).Select(b => new
                        {
                            b.DrugId,
                            b.DispensingStatus,
                            b.DispensingRmk,
                            b.DrugName,
                            b.DrugModel,
                            b.ManufacturerOrigin,
                            b.OriginPlace,
                            b.Qty,
                            b.UnitName,
                            b.UnitId,
                        })
                    }),
                    HerbPrescriptions = m.DispensingDetailSumary.HerbPrescription.Select(a => new
                    {
                        Main = new
                        {
                            a.HerbTitle,
                            a.Qty,
                            a.DoctorAdvice,
                            a.GivenRemark,
                            GivenTakeTypeName = _dicSvr.FindName(a.GivenTakeTypeId),
                            a.Amount,
                            a.ChargeStatus,
                            IsNeedDispense = m.DispensingDetailSumary.IsNeedHerbDispense(a.PrescriptionNo)
                        },
                        Items = m.DispensingDetailSumary.GetHerbDetailByPreNo(a.PrescriptionNo).Select(b => new
                        {
                            DrugId = b.CnHerbId,
                            b.DispensingStatus,
                            b.DispensingRmk,
                            b.DrugName,
                            b.Qty,
                            b.UnitName,
                            b.ManufacturerOrigin,
                            b.HerbUseTypeName
                        })
                    }),
                    IdInfos = new
                    {
                        m.Customer.CustomerID,
                        m.SelectedAddress.AddressId,
                        m.Treat.TreatId
                    }
                });
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 根据TreatId发药
        /// </summary>
        /// <param name="treatId">接诊Id</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public async Task<IActionResult> DispensingByTreatId(long treatId)
        {
            try
            {                
                var bmdd = await _dispSvr.DispenseAllDrugsByTreatId(treatId);
                var rlt = MyDynamicResult(true,bmdd?"发送成功":"发送失败",bmdd);                
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }


    }
}
