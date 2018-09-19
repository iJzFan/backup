using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass.Models;
using Ass;
using System.Collections.Generic;
using CHIS.Models.ViewModel;
using CHIS.Models;
using CHIS.Services;
using Microsoft.AspNetCore.Hosting;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{



    public partial class DispensingController : BaseController
    {
        DispensingService _cbl;
        Services.JKWebNetService _jkSvr;
        Services.DispensingService _dispSvr;
        IHostingEnvironment _env;
        private AutoMapper.IMapper _mapper;
        public DispensingController(DispensingService cbl, DbContext.CHISEntitiesSqlServer db
            , Services.JKWebNetService jkSvr
            , Services.DispensingService dispSvr
            , AutoMapper.IMapper mapper
            , IHostingEnvironment env) : base(db)
        {
            _jkSvr = jkSvr;
            _env = env;
            _dispSvr = dispSvr;
            _mapper = mapper;
            this._cbl = cbl;
        }

        #region 发药列表页
        // GET: /<controller>/
        //分配发药
        public IActionResult Index()
        {
            return View("Dispensing");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="pageIndex"></param>
        /// <param name="dispensingStatus">发送状态 0 未发 1 已发 2 其他</param>
        /// <param name="tstart"></param>
        /// <param name="tend"></param>
        /// <returns></returns>
        public IActionResult GetSendList(string searchText, string TimeRange = "Today", int dispensingStatus = 0, int pageIndex = 1, int pageSize = 20)
        {
            int days = 1;
#if DEBUG
            days = 100;
#endif
            DateTime? dt0 = null; DateTime? dt1 = null;

            initialData_TimeRange(ref dt0, ref dt1, days, timeRange: TimeRange);


            var finds = _db.SqlQuery<DispensingItemViewModel>(string.Format("exec sp_Dispensing_TreatList {0},{1},'{2:yyyy-MM-dd}','{3:yyyy-MM-dd}'", dispensingStatus, UserSelf.StationId, dt0, dt1));

            //如果角色是药店护士
            int? registOpId = null;
            if (UserSelf.MyRoleNames.Contains("drugstore_nurse"))
            {
                registOpId = UserSelf.OpId;
            }
            if (registOpId.HasValue) finds = finds.Where(m => m.RegistOpId == registOpId).ToList();
            else if (searchText.IsNotEmpty()) finds = finds.Where(m => m.CustomerName.Contains(searchText) || m.CustomerMobile == searchText).ToList();
            //分页
            var list = finds.OrderBy(m => m.TreatTime).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var model = new Ass.Mvc.PageListInfo<DispensingItemViewModel>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = finds.Count(),
                DataList = list
            };

            ViewBag.SendStatus = dispensingStatus;
            return PartialView("_pvNeedSendList", model);
        }



        #endregion

        #region 发药详细页面

        public IActionResult DispensingDetail(long treatId)
        { 
            var model = _dispSvr.DispensingDetail(treatId); 
            return PartialView(model);
        }


        public IActionResult LoadDispensingDetail(long treatId)
        {           
            var model = _dispSvr.DispensingDetail(treatId); 
            return PartialView("_pvDispensingDetail", model);
        }





        //总览摘要
        public IActionResult LoadDispensingDetailSumary(long treatId)
        {
            var _du = GetDrugDatas(treatId);
            return PartialView("_pvDispensingDetailSumaryList", new DispensingDetailSumary
            {
                FormedPrescription = _du.formedPre,
                HerbPrescription = _du.herbPre,
                Formed = _du.formeds,
                Herb = _du.sumary_herb
            });
        }

        //载入每一个详细的药店的发药信息
        public IActionResult LoadDispensingDetailsOfStore(int sourceFrom, long treatId, int supplierId = 0)
        {
            var pvName = "";
            switch (sourceFrom)
            {
                case 0: pvName = "_pvDispensingLocal"; break;
                case 1: pvName = "_pvDispensingWeb"; break;
                case 2: pvName = "_pvDispensingThreePart"; break;
            }
            var model = GetDetailOfStore(sourceFrom, treatId, supplierId);
            return PartialView(pvName, model);
        }






        #region 载入数据
        private DispensingUtils GetDrugDatas(long treatId)
        {
            var formeds = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus > 0).OrderBy(m => m.PrescriptionNo).ToList();
            var herb = _db.vwCHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus > 0).ToList();//付款的中药主表
            var herbs = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus > 0).ToList();
            var formedPre = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId);
            var herbPre = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId);

            var sumary_herb = new Dictionary<vwCHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>>();
            foreach (var item in herb)
            {
                sumary_herb.Add(item, herbs.Where(m => m.PrescriptionNo == item.PrescriptionNo));
            }

            return new DispensingUtils
            {
                formeds = formeds,
                herb = herb,
                herbs = herbs,
                sumary_herb = sumary_herb,
                formedPre = formedPre,
                herbPre = herbPre
            };
        }

        private DispensingDetailOfStoreViewModel GetDetailOfStore(DrugStoreItem storekey,
            vwCHIS_DoctorTreat treat,
            vwCHIS_Code_WorkStation station,
            IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail> formeds,
            IEnumerable<vwCHIS_DoctorAdvice_Herbs> herb,
            IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail> herbs)
        {

            var supplierId = storekey.Supplier == null ? 0 : storekey.Supplier.SupplierID;
            var formed = formeds.Where(m => m.SourceFrom == storekey.DrupSourceFrom && m.SupplierId == supplierId);
            herbs = herbs.Where(m => m.SourceFrom == storekey.DrupSourceFrom && m.SupplierId == supplierId);
            var herbm = herbs.Select(m => m.PrescriptionNo).Distinct();
            var herbdic = new Dictionary<vwCHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>>();
            foreach (var item in herbm)
            {
                var key = herb.FirstOrDefault(m => m.PrescriptionNo == item);
                var val = herbs.Where(m => m.PrescriptionNo == item);
                herbdic.Add(key, val);
            }
            return new DispensingDetailOfStoreViewModel
            {
                Station = station,
                WebSupplier = storekey.Supplier,
                Formed = formed,
                Herb = herbdic
            };

        }

        private DispensingDetailOfStoreViewModel GetDetailOfStore(int sourceFrom, long treatId, int supplierId)
        {
            var supplier = _db.CHIS_Code_Supplier.AsNoTracking().FirstOrDefault(m => m.SupplierID == supplierId);
            //载入本药房所需要发送的中药和西药品
            DrugStoreItem storekey = new DrugStoreItem
            {
                DrupSourceFrom = sourceFrom,
                Supplier = supplier
            };
            var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
            var station = _db.vwCHIS_Code_WorkStation.AsNoTracking().FirstOrDefault(m => m.StationID == treat.StationId);
            var formeds = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus > 0).OrderBy(m => m.PrescriptionNo).ToList();
            var herb = _db.vwCHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus > 0).ToList();//付款的中药主表
            var herbs = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus > 0).ToList();

            return GetDetailOfStore(storekey, treat, station, formeds, herb, herbs);
        }

        #endregion






        #endregion

        #region 给第三方发送订单
        /// <summary>
        /// 给第三方推送订单
        /// </summary>
        /// <remarks>方法:Json_SendOrderToNet(int treatId)</remarks>
        /// <param name="treatId">接诊ID</param>
        /// <returns></returns>
        public IActionResult Json_SendOrderToNetJK(long treatId, string sendRmk)
        {
            try
            {
                _dispSvr.SendDrug_SendOrderToNetJK(treatId, sendRmk);
                return Json(new { rlt = true, msg = "" });
            }
            catch (Exception ex)
            {                
                return Json(new { rlt = false, msg = ex.Message });
            }
        }
                 
        #endregion



        #region 标记发药状态

        /// <summary>
        /// 设置未成功发药的状态
        /// </summary>
        /// <param name="drugType">FORMED、HERB</param>
        /// <param name="dbid">数据库内的Id </param>
        /// <param name="drugId">药品Id</param>
        /// <param name="Rmk">备注</param>
        /// <returns>Json</returns>
        public IActionResult SetDispensingStatus(string drugType, int dbid, int drugId, string Rmk, int status = 2)
        {
            return TryCatchFunc(() =>
            {
                if (Ass.P.PStr(Rmk).Length > 15) throw new Exception("备注不超过15个汉字");
                if (drugType == "FORMED")
                {
                    if (status == 2)
                    {
                        var find = _db.CHIS_DoctorAdvice_Formed_Detail.FirstOrDefault(m => m.AdviceFormedId == dbid && m.DrugId == drugId);
                        find.DispensingStatus = (int)DispensingStatus.NeedReturn;
                        find.DispensingRmk = Rmk;
                        _db.SaveChanges();
                    }
                    if (status == 0)
                    {
                        var find = _db.CHIS_DoctorAdvice_Formed_Detail.FirstOrDefault(m => m.AdviceFormedId == dbid && m.DrugId == drugId);
                        find.DispensingStatus = (int)DispensingStatus.NeedSend;
                        find.DispensingRmk = null;
                        _db.SaveChanges();
                    }
                }
                if (drugType == "HERB")
                {
                    if (status == 2)
                    {
                        var find = _db.CHIS_DoctorAdvice_Herbs_Detail.FirstOrDefault(m => m.Id == dbid && m.CnHerbId == drugId);
                        find.DispensingStatus = (int)DispensingStatus.NeedReturn;
                        find.DispensingRmk = Rmk;
                        _db.SaveChanges();
                    }
                    if (status == 0)
                    {
                        var find = _db.CHIS_DoctorAdvice_Herbs_Detail.FirstOrDefault(m => m.Id == dbid && m.CnHerbId == drugId);
                        find.DispensingStatus = (int)DispensingStatus.NeedSend;
                        find.DispensingRmk = null;
                        _db.SaveChanges();
                    }
                }
                return null;
            });
        }

        #endregion


        #region 发药清单

        //发药 本地中药 单个处方发药
        public async Task<IActionResult> SendDrug_LocalHerb(IEnumerable<int> drugs, long treatId, int drugSource, int supplierId, Guid? prescriptionNo)
        {
            return await TryCatchFuncAsync(async () =>
           {

               var b = await _dispSvr.SendDrug_LocalHerb(drugs, treatId, prescriptionNo, UserSelf.StationId, UserSelf.DrugStoreStationId);

               return null;
           });
        }

        public async Task<IActionResult> SendDrug_LocalFormed(IEnumerable<int> drugs, long treatId, int drugSource, int supplierId, Guid? prescriptionNo)
        {
            return await TryCatchFuncAsync(async () =>
           {
               var b = await _dispSvr.SendDrug_LocalFormed(drugs, treatId, prescriptionNo, UserSelf.StationId, UserSelf.DrugStoreStationId);
               return null;
           });

        }

        public IActionResult SendDrug_Web(IEnumerable<int> drugs, long treatId, int drugSource, int supplierId, string sendRmk = null)
        {
            return TryCatchFunc(() =>
            {
                if (supplierId == MPS.SupplierId_JK)
                {
                    return Json_SendOrderToNetJK(treatId, sendRmk);
                }
                else throw new Exception("没有该供应商信息，或者该部分没有完善");
                //return null;
            });
        }


        #endregion


        //未发药备注弹出框
        public IActionResult DispensingRmk(string drugType, int dbid, int drugId, string Rmk)
        {
            ViewBag.Rmk = Rmk;
            ViewBag.drugType = drugType;
            ViewBag.dbid = dbid;
            ViewBag.drugId = drugId;
            return View(nameof(DispensingRmk));
        }









        //修改地址 type:由于收费和发药都可以修改地址  根据类型不同 回调的地址也不同
        public IActionResult ChangeAddress(int customerId, long selectedAddressId, long treatId, string type)
        {
            if (customerId == 0) throw new Exception("必须传入customerId");
            var model = _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == customerId);
            ViewBag.SelectedAddressId = selectedAddressId;
            ViewBag.CustomerId = customerId;
            ViewBag.TreatId = treatId;
            ViewBag.Type = type;
            return View(nameof(ChangeAddress), model);
        }

        [HttpPost]
        public IActionResult ChangeAddress(Models.CHIS_Code_Customer_AddressInfos model, int customerId, long selectedAddressId, long treatId, string type)
        {
            if (customerId == 0) throw new Exception("必须传入customerId");
            if (!(model.AreaId > 0)) throw new Exception("必须传入区域AreaId");
            _cbl.AddUserMailAddress(customerId, model.IsDefault, model.ContactName, model.Mobile, model.AreaId.Value, model.AddressDetail, model.Remark);
            return ChangeAddress(customerId, selectedAddressId, treatId, type);
        }

    }
}