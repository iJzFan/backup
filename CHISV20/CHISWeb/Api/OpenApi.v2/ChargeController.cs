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

namespace CHIS.Api.OpenApi.v2
{

    /// <summary>
    /// 支付
    /// 
    /// </summary>
    public class ChargeController : CHIS.Api.v2.OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        Services.DrugService _drugSvr;
        Services.DictService _dicSvr;
        Services.ChangePayService _chargeSvr;
        public ChargeController(Services.ReservationService resSvr,
            Services.WorkStationService stationSvr,
            Services.DoctorService docrSvr,
            Services.CustomerService cusSvr,
            Services.DrugService drugSvr,
            Services.DictService dicSvr,
            Services.ChangePayService chargeSvr
            ) //: base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
            _drugSvr = drugSvr;
            _dicSvr = dicSvr;
            _chargeSvr = chargeSvr;
        }

        //=========================================================================================

        /// <summary>
        /// 获取未支付的列表
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <param name="searchText">搜索患者姓名手机</param>
        /// <param name="pageSize">页容</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="doctorId">指定搜索医生</param>
        /// <param name="bAllClinic">是否包含整个诊所</param>
        /// <param name="stationId">接诊站Id</param> 
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetUnchargeList(string searchText,
                    int? stationId,
                    int? doctorId = null, bool? bAllClinic = false,
                    string timeRange = "Today",
                    int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                if (UserSelf == null && (stationId == null)) throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录用户，需要输入stationId");
                stationId = stationId ?? UserSelf.StationId;
                doctorId = doctorId ?? UserSelf.DoctorId;
                var dtt = timeRange.ahDtUtil().TimeRange();

                var md = _chargeSvr.GetChargeListNeedPayModel(searchText, bAllClinic.Value, dtt.Item1, dtt.Item2,
                    stationId.Value, doctorId.Value, null, pageIndex.Value, pageSize.Value);

                var rlt = MyDynamicResult(md);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }

        /// <summary>
        /// 获取客户未支付的列表
        /// </summary>
        /// <param name="customerId">客户Id</param>
        /// <param name="timeRange">时间范围 默认今天Today</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetUnchargeListByCustomerId(int customerId, string timeRange = "Today")
        {
            try
            {
                var dtt = timeRange.ahDtUtil().TimeRange();
                var md = _chargeSvr.GetChargeListNeedPayModelByCustomerId(customerId, dtt.Item1, dtt.Item2).Select(m => new Models.ViewModel.ChargeCustomerItem
                {
                    CustomerId = m.CustomerId,
                    TreatAmount = m.TreatAmount,
                    Birthday = m.Birthday,
                    CustomerName = m.CustomerName,
                    Gender = m.Gender,
                    NeedPayAmount = m.NeedPayAmount,
                    TreatId = m.TreatId,
                    TreatStationName = m.StationName,
                    TreatTime = m.TreatTime
                });
                var rlt = MyDynamicResult(md);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }





        /// <summary>
        /// 获取完成支付的列表
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <param name="searchText">搜索患者姓名手机</param>
        /// <param name="pageSize">页容</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="doctorId">指定搜索医生</param>
        /// <param name="bAllClinic">是否包含整个诊所</param>
        /// <param name="stationId">接诊站Id</param> 
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetChargedList(string searchText,
                    int? stationId,
                    int? doctorId = null, bool? bAllClinic = false,
                    string timeRange = "Today",
                    int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                if (UserSelf == null && (stationId == null)) throw new ComException(ExceptionTypes.Error_Unauthorized, "未登录用户，需要输入stationId");
                stationId = stationId ?? UserSelf.StationId;
                doctorId = doctorId ?? UserSelf.DoctorId;
                var dtt = timeRange.ahDtUtil().TimeRange();
                var mdd = _chargeSvr.GetChargeListPayedModel(searchText, bAllClinic.Value, dtt.Item1, dtt.Item2, stationId.Value, doctorId.Value, null, pageIndex.Value, pageSize.Value);
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }


        /// <summary>
        /// 获取客户完成支付的列表
        /// </summary>
        /// <param name="customerId">客户Id</param>
        /// <param name="timeRange">时间范围 默认今天Today</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult GetChargedListByCustomerId(int customerId, string timeRange = "Today", int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                var dtt = timeRange.ahDtUtil().TimeRange();
                var md = _chargeSvr.GetChargeListPayedModelByCustomerId(customerId, dtt.Item1, dtt.Item2, pageIndex.Value, pageSize.Value)
                .Select(m => new Models.ViewModel.PayedItem
                {
                    CustomerId = m.CustomerId.Value,
                    CustomerName = m.CustomerName,
                    Gender = m.Gender,
                    TreatId = m.TreatId,
                    FeeTypeCode = m.FeeTypeCode,
                    FeeTypeCodeName = FeeTypes.ToName(m.FeeTypeCode),
                    PayedTime = m.PayedTime,
                    PayId = m.PayId,
                    PayOrderId = m.PayOrderId,
                    TotalAmount = m.TotalAmount,
                    PayRemark = m.PayRemark,
                    TreatTime = m.TreatTime                    
                });
                var rlt = MyDynamicResult(md);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }



        /// <summary>
        /// 生成支付订单和微信或者支付宝二维码,现金
        /// </summary>
        /// <param name="treatId">接诊Id</param>
        /// <param name="payRmk">支付说明</param>
        /// <param name="payType">支付方式:WX2D-微信二维码,ALI2D-支付宝二维码,CASH-现金</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public async Task<IActionResult> CreateChargeOrder(long treatId,string payRmk,string payType)
        {
            try
            {
                object dd = null;
                switch (payType)
                {
                    case "WX2D":
                        var d0 = await _chargeSvr.CreateWX2DCode(treatId, payRmk, null, UserSelf.OpId, UserSelf.OpMan);
                        dd = d0;
                        break;
                    case "ALI2D":
                        var d1 =await _chargeSvr.CreateAli2DCode(treatId, payRmk, null, UserSelf.OpId, UserSelf.OpMan);
                        dd = d1;
                        break;
                    case "CASH":
                        var d2 =await _chargeSvr.CreateCashOrder(treatId, payRmk, true, UserSelf.OpId, UserSelf.OpMan);
                        dd = d2;
                        break;
                    default:
                        throw new ComException(ExceptionTypes.Error_BeThorw, "没有该支付类型");
                        
                }             
                var rlt = MyDynamicResult(dd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }

        /// <summary>
        /// 检查订单是否已经成功支付了(请勿不断请求)
        /// </summary>
        /// <param name="payOrderId">支付单号</param>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public IActionResult CheckOrderIsPayed(string payOrderId)
        {
            try
            {                
                var dd= _chargeSvr.CheckIsPayed(payOrderId, true);
                var rlt = MyDynamicResult(dd?"成功":"未支付",dd);                
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }
         


        /// <summary>
        /// 设置现金支付
        /// </summary>
        /// <param name="cashPay">请求内容</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("ThirdPartAuth")]
        [Authorize("LoginUserAuth")]
        public async Task<IActionResult> SetCashCharge(Models.ViewModel.CashPay cashPay)
        {
            try
            {
                bool mdd = await _chargeSvr.SetCashPay(cashPay,UserSelf.OpId,UserSelf.OpMan);
                var rlt = MyDynamicResult(mdd);
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }
        }


    }
}
