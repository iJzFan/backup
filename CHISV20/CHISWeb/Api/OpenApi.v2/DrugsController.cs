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
    /// 药品
    /// </summary>
    public class DrugsController : CHIS.Api.v2.OpenApiBaseController
    {

        Services.ReservationService _resSvr;
        Services.WorkStationService _stationSvr;
        Services.DoctorService _docrSvr;
        Services.CustomerService _cusSvr;
        Services.DrugService _drugSvr;
        Services.DictService _dicSvr;
        public DrugsController(Services.ReservationService resSvr,
            Services.WorkStationService stationSvr,
            Services.DoctorService docrSvr,
            Services.CustomerService cusSvr,
            Services.DrugService drugSvr,
            Services.DictService dicSvr
            ) //: base(db)
        {
            _resSvr = resSvr;
            _stationSvr = stationSvr;
            _cusSvr = cusSvr;
            _docrSvr = docrSvr;
            _drugSvr = drugSvr;
            _dicSvr = dicSvr;
        }

        //=========================================================================================


        /// <summary>
        /// 获取诊所的药品
        /// </summary>
        /// <param name="drugStoreStationId">注意这里是药房的Id,要通过我的药房Id获取到，而不一定是本诊所的StationId 默认：获取登录数据内的DrugStoreId</param>
        ///<param name="pageIndex">页码</param>
        ///<param name="pageSize">页容</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetDrugListOfClinic(int? drugStoreStationId, int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                if (drugStoreStationId == null) drugStoreStationId = UserSelf.DrugStoreStationId;
                var drugs = _drugSvr.GetAllDrugsOfClinic(drugStoreStationId.Value).Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
                var rlt = MyDynamicResult(drugs);
                rlt.pageIndex = pageIndex;
                rlt.pageSize = pageSize;
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }


        /// <summary>
        /// 搜索药品
        /// </summary>
        /// <param name="searchText">搜索文本</param>
        /// <param name="drugStoreStationId">注意这里是药房的Id,要通过我的药房Id获取到，而不一定是本诊所的StationId</param>
        ///<param name="pageIndex">页码</param>
        ///<param name="pageSize">页容</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult SearchDrugsOfClinic(string searchText, int? drugStoreStationId, int? pageIndex = 1, int? pageSize = 20)
        {
            try
            {
                searchText = Ass.P.PStr(searchText);
                if (searchText.Length <= 2) throw new UnvalidComException("起码输入2个搜索字符");
                if (drugStoreStationId == null)
                {
                    if (UserSelf == null) throw new UnvalidComException("未登录的用户，必须输入药房Id");
                    drugStoreStationId = UserSelf.DrugStoreStationId;
                }
                IEnumerable<string> fm = null;
                var drugs = _drugSvr.QueryStockDrugInfos(searchText, drugStoreStationId.Value, ref fm).Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).Select(m => new
                {
                    m.StationId,
                    DrugInfo = new
                    {
                        m.DrugId,
                        m.DrugCode,
                        m.BarCode,
                        m.DrugName,
                        m.Alias,
                        DrugPicUrl = m.DrugPicUrl.ahDtUtil().GetDrugImg(m.MedialMainKindCode, true),
                        m.ManufacturerOrigin,
                        m.Trademark,
                        m.OriginPlace,
                        m.MedialMainKindCode,
                        DrugModel = new
                        {
                            m.DrugModel,
                            m.OutUnitBigName,
                            m.OutpatientConvertRate,
                            m.OutUnitSmallName,
                            m.DosageContent,
                            m.DosageUnitName,
                            m.UnitBigId,
                            m.UnitSmallId,
                            m.DosageUnitId,
                            m.IsMultyUnit,
                        },
                        DefaultUsage = new
                        {
                            m.DefDrugGivenTakeTypeId,
                            DefDrugGivenTakeTypeName = _dicSvr.GetDictById(m.DefDrugGivenTakeTypeId).ItemName,
                            m.DefDrugGivenTimeTypeId,
                            DefDrugGivenTimeTypeName = _dicSvr.GetDictById(m.DefDrugGivenTimeTypeId).ItemName,
                            m.DefDrugGivenWhereTypeId,
                            DefDrugGivenWhereTypeName = _dicSvr.GetDictById(m.DefDrugGivenWhereTypeId).ItemName,
                            m.DefDosage,
                        },
                    },
                    StockInfo = new
                    {
                        m.DrugStockMonitorId,
                        m.DrugStockNum,
                        m.StockUnitName,
                        DrugStockRemark = m.DrugStockNum + m.StockUnitName + "(" + m.BigStockNumber + m.OutUnitBigName + ")",
                        m.StockUnitId,
                        m.StockBuyPrice,
                        m.StockSalePrice,
                        m.StockDrugIsEnable,
                        m.BigStockNumber,
                        m.HasBigStockNumberMore
                    },
                    m.ThreePartDrugId,
                    m.SupplierCoName,
                    m.SupplierId,
                    m.SourceFrom,
                    m.ThreePartDrugRefreshTime,
                    m.DrugStockTypeId,
                    m.ValidDays,
                    m.DrugCompleteScore,
                    m.DrugRxType,
                    m.IsMultyUnit,
                });
                var rlt = MyDynamicResult(drugs);
                rlt.pageIndex = pageIndex;
                rlt.pageSize = pageSize;
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }

        /// <summary>
        /// 获取药品信息
        /// </summary>
        /// <param name="drugId">药品Id</param> 
        /// <returns></returns>
        [HttpGet]
        [Authorize("ThirdPartAuth")]
        public IActionResult GetDrugInfo(int drugId)
        {
            try
            {
                var drug = _drugSvr.FindView(drugId);
                var rlt = MyDynamicResult(drug);                
                return Ok(rlt);
            }
            catch (Exception ex) { return Ok(MyDynamicResult(ex)); }

        }


    }
}
