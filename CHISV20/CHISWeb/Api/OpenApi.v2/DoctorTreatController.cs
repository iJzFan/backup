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
using System.ComponentModel.DataAnnotations;

namespace CHIS.Api.OpenApi.v2
{

    /// <summary>
    /// 药品
    /// </summary>
    public class DoctorTreatController : CHIS.Api.v2.OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        Services.DrugService _drugSvr;
        Services.DictService _dicSvr;
        Services.TreatService _treatSvr;
        public DoctorTreatController(Services.ReservationService resSvr,
            Services.WorkStationService stationSvr,
            Services.DoctorService docrSvr,
            Services.CustomerService cusSvr,
            Services.DrugService drugSvr,
            Services.DictService dicSvr,
            Services.TreatService treatSvr
            ) //: base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
            _drugSvr = drugSvr;
            _dicSvr = dicSvr;
            _treatSvr = treatSvr;
        }

        //=========================================================================================

        /// <summary>
        /// 获取接诊清单
        /// </summary>
        /// <param name="searchText">搜索患者的姓名手机</param>
        /// <param name="treatStatus">接诊状态 默认ALL所有,TREATING在诊,TREATED已诊</param>
        /// <param name="stationId">接诊站Id 默认登录工作站</param> 
        /// <param name="doctorId">接诊医生 默认登录工作站</param>        
        /// <param name="dctrBuzType">业务类型 默认ALL所有,MYTREAT自己接诊，RXSIGN需要签名</param>
        /// <param name="TimeRange">接诊时间范围</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetTreatList(string searchText,
            string treatStatus = "ALL",
            int? stationId = null, int? doctorId = null,
            string dctrBuzType = "ALL", string TimeRange = "Today", int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                if (UserSelf == null && stationId == null)
                    throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录的用户，需要输入参数:stationId");

                stationId = stationId ?? UserSelf.StationId;
                doctorId = doctorId ?? UserSelf.DoctorId;
                var dtt = TimeRange.ahDtUtil().TimeRange();
                var pdd = _treatSvr.GetTreatList(searchText, dtt.Item1, dtt.Item2, stationId.Value, doctorId.Value, treatStatus, "THIS", dctrBuzType, pageIndex.Value, pageSize.Value);
                var mdd = pdd.DataList.Select(m => new
                {
                    m.TreatId,
                    m.RegisterDate,
                    m.StationName,
                    m.CustomerName,
                    CustomerGender = m.Gender?.ToGenderString(),
                    m.TreatCustomerAge,
                    m.CustomerMobile,
                    m.Diagnosis1,
                    m.FirstTreatTime,
                    LastTreatTime = m.TreatTime,
                    m.DoctorName,
                    m.RxDoctorName,
                    TreatStatus = _dicSvr.FindNameByValue("TreatStatus", m.TreatStatus),
                    m.IsNeedRxSign,//是否需要处方签名
                    IsRxDoctorSigned = m.IsNeedRxDoctorSign != true,
                    IdInfos = new
                    {
                        m.DoctorId,
                        m.StationId,
                        RegisterId = m.RegisterID,
                        m.RxDoctorId,
                        m.CustomerId,
                    },
                    ValueInfos = new
                    {
                        m.TreatStatus,
                        CustomerGender = m.Gender
                    }
                });
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }


        /// <summary>
        /// 获取接诊详细信息
        /// </summary>
        /// <param name="treatId">接诊Id</param> 
        /// <param name="fullVer">是否获取所有信息 默认false</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetTreatDetail(long treatId, bool? fullVer = false)
        {
            try
            {
                var mdd = _treatSvr.GetTreatDetail(treatId).ahDtUtil()
                    .NewModel<PatientDetailViewModel>(m => new
                    {
                        m.DoctorTreat.TreatId,
                        m.DoctorTreat.DoctorName,
                        m.DoctorTreat.StationName,
                        m.DoctorTreat.DepartmentName,
                        FeeSumary = new
                        {
                            m.FeeSumary.NeedPayAmount,
                            m.FeeSumary.TreatAmount,
                            m.FeeSumary.TreatFee,
                            m.FeeSumary.TransFee,
                        },
                        Customer = new
                        {
                            m.DoctorTreat.CustomerName,
                            m.DoctorTreat.TreatCustomerAge,
                            CustomerGender = m.DoctorTreat.Gender,
                            CustomerPicUrl = m.CHIS_Code_Customer.PhotoUrlDef.ahDtUtil().GetCustomerImg(m.CHIS_Code_Customer.Gender)
                        },
                        #region Treat
                        Treat = new
                        {
                            m.DoctorTreat.Height,
                            m.DoctorTreat.Weight,
                            m.DoctorTreat.Temperature,
                            m.DoctorTreat.TempSource,
                            m.DoctorTreat.BloodPress_H,
                            m.DoctorTreat.BloodPress_L,
                            m.DoctorTreat.AliveChildrenNum,
                            m.DoctorTreat.BirthChildrenNum,
                            m.DoctorTreat.Complain,
                            m.DoctorTreat.Examination,
                            m.DoctorTreat.Gestation,
                            m.DoctorTreat.IsMenstruation,
                            m.DoctorTreat.MenstruationCircleDay,
                            m.DoctorTreat.MenstruationDay,
                            m.DoctorTreat.MenstruationDays,
                            m.DoctorTreat.MenstruationEndOldYear,
                            m.DoctorTreat.MenstruationStartOldYear,
                            m.DoctorTreat.PresentIllness,
                            m.DoctorTreat.PregnancyNum,
                            m.DoctorTreat.Pulse,
                            m.DoctorTreat.RespiratoryPerMinute,

                            m.DoctorTreat.TreatAdvice,
                            m.DoctorTreat.Diagnosis1,
                            IsDiagnosis1Unsure = m.DoctorTreat.FstIsDiag,
                            m.DoctorTreat.FirstTreatTime,
                            LastTreatTime = m.DoctorTreat.TreatTime,

                            m.DoctorTreat.Diagnosis2,
                            m.DoctorTreat.Diagnosis3,
                            IsDiagnosis2Unsure = m.DoctorTreat.SecIsDiag,
                            IsDiagnosis3Unsure = m.DoctorTreat.ThrIsDiag,
                        },
                        #endregion
                        FormedPrescriptionNos = m.FormedList.Select(a => new { a.Main.PrescriptionNo, a.Main.ChargeStatus }),
                        HerbPrescriptionNos = m.HerbList.Select(a => new { a.Main.PrescriptionNo, a.Main.ChargeStatus }),
                        Prescriptions = fullVer == false ? null : new
                        {
                            #region 成药
                            Formeds = m.FormedList.Select(a => new
                            {
                                Main = new
                                {
                                    a.Main.PrescriptionNo,
                                    a.Main.Amount,
                                    a.Main.ChargeStatus,
                                    a.Main.CreateTime,
                                    a.Main.DoctorSignUrl,
                                    a.Main.RxDoctorSignUrl
                                },
                                Items = a.Details.Select(b => new
                                {
                                    b.DrugName,
                                    b.DrugModel,
                                    b.ManufacturerOrigin,
                                    b.Qty,
                                    b.UnitName,
                                    b.Price,
                                    b.Amount,
                                    b.OriginPlace,
                                    b.UnitId,
                                    b.DispensingStatus,
                                    b.DispensingRmk,
                                    b.DrugId,
                                    AdviceItemId = b.AdviceFormedId,
                                }),
                                a.IsNeedRxSign,
                                a.IsRxSigned
                            }),
                            #endregion
                            #region 中药
                            Herbs = m.HerbList.Select(a => new
                            {
                                Main = new
                                {
                                    a.Main.PrescriptionNo,
                                    a.Main.Amount,
                                    a.Main.ChargeStatus,
                                    a.Main.HerbTitle,
                                    a.Main.Qty,
                                    GivenTakeTypeName = _dicSvr.FindName(a.Main.GivenTakeTypeId),
                                    a.Main.GivenRemark,
                                    a.Main.GivenTakeTypeId,
                                    a.Main.DoctorSignUrl,
                                    a.Main.RxDoctorSignUrl
                                },
                                Items = a.Details.Select(b => new
                                {
                                    b.DrugName,
                                    b.HerbUseTypeName,
                                    b.OriginPlace,
                                    b.Qty,
                                    b.UnitName,
                                    b.Price,
                                    b.Amount,
                                    b.DispensingStatus,
                                    b.DispensingRmk,
                                    AdviceItemId = b.Id,
                                    DrugId = b.CnHerbId,
                                    b.ManufacturerOrigin,
                                    b.UnitId,
                                }),
                                a.IsNeedRxSign,
                                a.IsRxSigned
                            }),
                            #endregion
                        },
                        #region 附加费
                        ExtraFees = m.TreatExtraFees.Select(a => new
                        {
                            a.FeeName,
                            a.TreatFeePrice,
                            a.TreatFeeOriginalPrice,
                            a.Qty,
                            a.Amount,
                            a.ChargeStatus,
                            a.FeeRemark,
                        }),
                        #endregion

                        #region IdInfos and Additions
                        IdInfos = new
                        {
                            m.DoctorTreat.CustomerId,
                            TreatDoctorId = m.DoctorTreat.DoctorId,
                            m.CHIS_Register.RxDoctorId,
                            m.DoctorTreat.StationId,
                        },
                        Additions = new
                        {
                            RegisterId = m.CHIS_Register.RegisterID,
                            m.CHIS_Register.RegisterDate,
                            m.RegistDepartment,
                        }
                        #endregion
                    });
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }

        /// <summary>
        /// 获取处方详细信息
        /// </summary>
        /// <param name="prescriptionGuid">处方号</param> 

        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetPrescriptionDetail(Guid prescriptionGuid)
        {
            try
            {
                var dd = _treatSvr.GetPrescriptionDetail(prescriptionGuid);
                if (dd is CHIS.Models.ViewModel.PrintFormedModel)
                {
                    var rlt0 = (CHIS.Models.ViewModel.PrintFormedModel)dd;
                    var mmd = rlt0.ahDtUtil().NewModel<Models.ViewModel.PrintFormedModel>(m => new
                    {
                        m.PrescriptionType,
                        m.Main.PrescriptionNo,
                        m.Main.Amount,
                        m.Main.ChargeStatus,
                        m.Treat.DoctorName,
                        m.RxDoctorName,
                        IsNeedRxSign = m.Regist.RxDoctorId > 0,
                        IsRxDoctorSigned = !string.IsNullOrWhiteSpace(m.Main.RxDoctorSignUrl),
                        Main = new
                        {
                            m.Treat.FirstTreatTime,
                            m.Treat.CustomerName,
                            CustomerGender = m.Treat.Gender,
                            m.Treat.TreatCustomerAge,
                            m.Treat.StationName,
                            m.Treat.DepartmentName,
                            m.Treat.Diagnosis1,
                            m.Treat.Diagnosis2,
                            IsDiagnosis1Unsure = m.Treat.FstIsDiag,
                            IsDiagnosis2Unsure = m.Treat.SecIsDiag,
                        },
                        Items = m.Detail.Select(a => new
                        {
                            a.DrugId,
                            a.DrugName,
                            a.DrugModel,
                            a.Qty,
                            a.UnitName,
                            a.GivenTakeTypeName,
                            a.GivenDays,
                            a.GivenDosage,
                            a.GivenNum,
                            a.GivenRemark,
                            a.GivenWhereTypeName,
                            a.GroupNum,
                            a.ManufacturerOrigin,
                            a.OriginPlace
                        }),
                        Additions = new
                        {
                            m.RxDoctorId,
                            m.Main.TreatId,
                            m.Main.RxDoctorSignUrl,
                            m.Main.DoctorSignUrl
                        }
                    });
                    var rlt = MyDynamicResult(mmd);
                    return Ok(rlt);
                }
                if (dd is CHIS.Models.ViewModel.PrintHerbModel)
                {
                    var rlt0 = (CHIS.Models.ViewModel.PrintHerbModel)dd;
                    var mmd = rlt0.ahDtUtil().NewModel<Models.ViewModel.PrintHerbModel>(m => new
                    {
                        m.PrescriptionType,
                        m.Main.PrescriptionNo,
                        m.Main.Amount,
                        m.Main.ChargeStatus,
                        m.Treat.DoctorName,
                        m.RxDoctorName,
                        IsNeedRxSign = m.Regist.RxDoctorId > 0,
                        IsRxDoctorSigned = !string.IsNullOrWhiteSpace(m.Main.RxDoctorSignUrl),
                        Main = new
                        {
                            m.Treat.FirstTreatTime,
                            m.Treat.CustomerName,
                            CustomerGender = m.Treat.Gender,
                            m.Treat.TreatCustomerAge,
                            m.Treat.StationName,
                            m.Treat.DepartmentName,
                            m.Treat.Diagnosis1,
                            m.Treat.Diagnosis2,
                            IsDiagnosis1Unsure = m.Treat.FstIsDiag,
                            IsDiagnosis2Unsure = m.Treat.SecIsDiag,

                            m.Main.HerbTitle,
                            m.Main.Qty,
                            m.Main.DoctorAdvice,
                            m.Main.GivenRemark,
                            m.Main.GivenTakeTypeName,
                        },
                        Items = m.Detail.Select(a => new
                        {
                            DrugId = a.CnHerbId,
                            a.DrugName,
                            a.Qty,
                            a.UnitName,
                            a.HerbUseTypeName,
                            a.HerbPackageNum,
                            a.HerbUseTypeId,
                            a.Trademark,
                            a.ManufacturerOrigin,
                            a.OriginPlace
                        }),
                        Additions = new
                        {
                            m.RxDoctorId,
                            m.Main.TreatId,
                            m.Main.RxDoctorSignUrl,
                            m.Main.DoctorSignUrl
                        }
                    });
                    var rlt = MyDynamicResult(mmd);
                    return Ok(rlt);
                }
                throw new BeThrowComException("没有发现正确数据");
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }



        #region 处方编辑

        /// <summary>
        /// 保存一个接诊及其下面的处方药品
        /// </summary>
        /// <param name="prescriptionNo">处方号，如果没带，则删除其他接诊后新增</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public async Task<IActionResult> SaveTreatDrugs(
           [FromBody] DataTreatForDrugStore treatData)
        {
            try
            {
                //规整默认数据
                if (treatData == null) throw new UnvalidComException("没有传入数据，或者数据格式错误");
                treatData.DoctorTreatData.DoctorId = UserSelf.DoctorId;
                if (treatData.DoctorTreatData.StationId == 0) treatData.DoctorTreatData.StationId = UserSelf.StationId;
                List<DataFormedV0Input> formeds = new List<DataFormedV0Input>();
                formeds.Add(new DataFormedV0Input
                {
                    Main = new FormedMainV0Input { PrescriptionNo = treatData.FormedPrescriptionNo ?? Guid.NewGuid() },
                    Detail = treatData.FormedDrugAdvices
                });
              //  var mdd = "";
                var mdd = await _treatSvr.SaveTreatAndPrescriptionsAsync(treatData.DoctorTreatData,
                    formeds, null,
                    treatData.CustomerMailAddressId,
                    UserSelf.DrugStoreStationId, UserSelf.OpId, UserSelf.OpMan);
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }


        // /// <summary>
        // /// 保存一个接诊及其下面的处方
        // /// </summary>
        // /// <returns></returns>
        //// [HttpPost]
        // [Authorize("ThirdPartAuth")]
        // [Authorize("LoginUserAuth")]
        // private async Task<IActionResult> SaveTreatAndPrescriptions(DoctorTreatV0Input treat, IEnumerable<DataFormedV0Input> formeds, IEnumerable<DataHerbV0Input> herbs)
        // {
        //     try
        //     {
        //         //规整默认数据
        //         treat.DoctorId = UserSelf.DoctorId;
        //         var mdd = await _treatSvr.SaveTreatAndPrescriptionsAsync(treat, formeds, herbs, UserSelf.DrugStoreStationId, UserSelf.OpId, UserSelf.OpMan);
        //         var rlt = MyDynamicResult(mdd);
        //         return Ok(rlt);
        //     }
        //     catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        // }

        #endregion

    }
}
