using CHIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ass;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using CHIS.Models.ViewModel;
using CHIS.DbContext;
using CHIS.Models.StatisticsModels;
using CHIS.Models.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Caching.Memory;

namespace CHIS.Services
{
    /// <summary>
    /// 接诊业务类
    /// </summary>
    public class TreatService : BaseService
    {
        IMemoryCache _memoryCache;
        DrugService _drugSvr;
        DictService _dictSvr;
        public TreatService(CHISEntitiesSqlServer db
            , DrugService drugSvr
            , IMemoryCache memoryCache
            , DictService dictSvr
            ) : base(db)
        {
            _memoryCache = memoryCache;
            _drugSvr = drugSvr;
            _dictSvr = dictSvr;
            GetDiagnosis("", 1, 1);
        }

        /// <summary>
        /// 查找一个接诊
        /// </summary> 
        public vwCHIS_DoctorTreat FindTreat(long treatId)
        {
            return _db.vwCHIS_DoctorTreat.Find(treatId);
        }


        /// <summary>
        /// 根据接诊获取地址列表
        /// </summary>
        /// <param name="treatId">接诊号</param>
        /// <param name="selectAddressId">选择的地址表</param>
        /// <returns></returns>
        public IQueryable<vwCHIS_Code_Customer_AddressInfos> GetMyAddressInfosByTreatId(long treatId, out long? selectAddressId)
        {
            long? defId = null;
            var customerId = _db.CHIS_DoctorTreat.AsNoTracking().Single(m => m.TreatId == treatId).CustomerId;
            var exfee = _db.vwCHIS_Doctor_ExtraFee.FirstOrDefault(m => m.TreatId == treatId && m.TreatFeeTypeId == (int)(ExtraFeeTypes.TransFee));
            if (exfee == null)
            {
                defId = exfee.MailAddressInfoId;
            }
            var rlt = _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == customerId);

            var lstdef = rlt.FirstOrDefault(m => m.IsDefault == true);
            if (lstdef == null) lstdef = rlt.FirstOrDefault();
            if (lstdef != null && defId == null) defId = lstdef?.AddressId;
            selectAddressId = defId;
            return rlt;
        }

        /// <summary>
        /// 获取接诊清单
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId">如果DoctorId=0则表示所有，null默认本医生</param>
        /// <param name="TimeRange"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Ass.Mvc.PageListInfo<CHIS.Models.DataModel.DoctorTreatItem> GetTreatList(
            string searchText, DateTime dt0, DateTime dt1,
            int stationId, int doctorId,
            string treatStatus = "ALL", string stationWhich = "ALL",
            string dctrBuzType = "ALL",
            int pageIndex = 1, int pageSize = 20)
        {

            var finds = from a in _db.vwCHIS_DoctorTreat.AsNoTracking()
                        join r in _db.vwCHIS_Register.AsNoTracking() on a.RegisterID equals r.RegisterID into t_reg
                        from reg in t_reg.DefaultIfEmpty()
                        join d in _db.vwCHIS_Code_Doctor.AsNoTracking() on reg.RxDoctorId equals d.DoctorId into t_rxdoctor
                        from rxdoctor in t_rxdoctor.DefaultIfEmpty()
                        select new CHIS.Models.DataModel.DoctorTreatItem
                        {
                            TreatId = a.TreatId,
                            RegisterID = a.RegisterID,
                            StationId = a.StationId,
                            DoctorId = a.DoctorId,
                            DoctorName = a.DoctorName,
                            FirstTreatTime = a.FirstTreatTime,
                            TreatTime = a.TreatTime,
                            TreatStatus = a.TreatStatus,
                            CustomerMobile = a.CustomerMobile,
                            CustomerName = a.CustomerName,
                            RxDoctorId = reg.RxDoctorId,
                            RxDoctorName = rxdoctor.DoctorName,
                            RegisterDate = a.RegisterDate,
                            StationName = a.StationName,
                            Gender = a.Gender,
                            Birthday = a.Birthday,
                            Diagnosis1 = a.Diagnosis1,
                            CustomerId = a.CustomerId,
                            TreatCustomerAge = a.TreatCustomerAge

                        };


            if (dctrBuzType == "RXSIGN") finds = finds.Where(m => m.RxDoctorId > 0 && m.RxDoctorId == doctorId);
            else if (dctrBuzType == "MYTREAT")
            {
                finds = finds.Where(m => m.DoctorId == doctorId);
            }
            else
            {
                finds = finds.Where(m => m.RxDoctorId == doctorId || m.DoctorId == doctorId);
            }

            if (stationWhich == "THIS") finds = finds.Where(m => m.StationId == stationId);

            finds = finds.Where(m => (m.FirstTreatTime >= dt0 || m.TreatTime >= dt0) && (m.FirstTreatTime < dt1 || m.TreatTime < dt1));
            if (treatStatus == "TREATING") finds = finds.Where(m => m.TreatStatus == 1);
            else if (treatStatus == "TREATED") finds = finds.Where(m => m.TreatStatus == 2);


            if (searchText.IsNotEmpty()) finds = finds.Where(m => m.CustomerMobile == searchText || m.CustomerName == searchText);

            var items = finds.OrderByDescending(m => m.TreatId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            // 处方签名的再次数据调整            
            var treatids = items.Select(m => m.TreatId);
            var fms = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => treatids.Contains(m.TreatId)).ToList();
            var hbs = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => treatids.Contains(m.TreatId)).ToList();
            foreach (var item in items)
            {
                item.IsNeedRxDoctorSign = fms.Where(m => m.TreatId == item.TreatId).Any(m => m.RxDoctorSignUrl == null) ||
                                    hbs.Where(m => m.TreatId == item.TreatId).Any(m => m.RxDoctorSignUrl == null);
            }
            var model = new Ass.Mvc.PageListInfo<CHIS.Models.DataModel.DoctorTreatItem>
            {
                DataList = items,
                RecordTotal = finds.Count(),
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return model;
        }




        /// <summary>
        /// 获取诊断内容
        /// </summary>
        /// <param name="searchText">搜索项</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        /// <returns></returns>
        public List<CHIS_Code_Diagnosis> GetDiagnosis(string searchText, int pageIndex, int pageSize)
        {   //获取缓存数据
            var cacheKey = "Diagnosis";
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<CHIS_Code_Diagnosis> result))
            {
                result = _db.CHIS_Code_Diagnosis.AsNoTracking().ToList();
                _memoryCache.Set(cacheKey, result);
                //设置相对过期时间60分钟
                _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60)));
                /*
                 //设置绝对过期时间2分钟
                 _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                     .SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
                 //移除缓存
                 _memoryCache.Remove(cacheKey);
                 //缓存优先级 （程序压力大时，会根据优先级自动回收）
                 _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                     .SetPriority(CacheItemPriority.NeverRemove));
                 //缓存回调 10秒过期会回调
                 _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                     .SetAbsoluteExpiration(TimeSpan.FromSeconds(10))
                     .RegisterPostEvictionCallback((key, value, reason, substate) =>
                     {
                         Console.WriteLine($"键{key}值{value}改变，因为{reason}");
                     }));
                 //缓存回调 根据Token过期
                 var cts = new CancellationTokenSource();
                 _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                     .AddExpirationToken(new CancellationChangeToken(cts.Token))
                     .RegisterPostEvictionCallback((key, value, reason, substate) =>
                     {
                         Console.WriteLine($"键{key}值{value}改变，因为{reason}");
                     }));
                 cts.Cancel();
                 */
            }

            var t = Ass.P.PStr(searchText).ToLower().GetStringType();

            var f1 = result.Where(m => m.PyCode.Contains(t.String)
                                    || m.QPCode.Contains(t.String)
                                    || m.DiagnoisisName.Contains(t.String))
                                    .OrderBy(m => m.DiagnoisisName.Length)
                                    .Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return f1.ToList();
        }
        /// <summary>
        /// 根据诊断Id获取诊断信息
        /// </summary>
        /// <param name="zdid">诊断Id</param>
        /// <returns>诊断信息</returns>
        public CHIS_Code_Diagnosis GetDiagnosisById(int zdid)
        {
            //获取缓存数据
            var cacheKey = "Diagnosis";
            if (!_memoryCache.TryGetValue("Diagnosis", out IEnumerable<CHIS_Code_Diagnosis> result))
            {
                result = _db.CHIS_Code_Diagnosis.AsNoTracking().ToList();
                _memoryCache.Set(cacheKey, result);
                _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60)));     //设置相对过期时间60分钟          
            }
            return result.SingleOrDefault(m => m.DiagnoisisId == zdid);
        }


        /// <summary>
        /// 添加一个诊断
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<CHIS_Code_Diagnosis> AddDiagnosisAsync(DiagnosisModel entity)
        {
            if (entity.DiagnoisisName.IsEmpty()) throw new Exception("没有诊断名");
            CHIS_Code_Diagnosis model = new CHIS_Code_Diagnosis()
            {
                DiagnoisisName = entity.DiagnoisisName,
                DiagnoisisValue = entity.DiagnoisisValue ?? Ass.Data.Chinese2Spell.Convert(entity.DiagnoisisName),
                DiagTypeCode = entity.TypeCode,
                PyCode = Ass.Data.Chinese2Spell.GetFstLettersLower(entity.DiagnoisisName),
                QPCode = Ass.Data.Chinese2Spell.ConvertLower(entity.DiagnoisisName),
                WbCode = ""
            };


            var num = _db.CHIS_Code_Diagnosis.AsNoTracking().Where(m => m.DiagnoisisName == entity.DiagnoisisName).Count();
            if (num > 0) throw new Exception("该诊断已经存在！");
            var id = _db.CHIS_Code_Diagnosis.Max(m => m.DiagnoisisId) + 1;
            model.DiagnoisisId = id;
            var rlt = _db.CHIS_Code_Diagnosis.Add(model).Entity;
            await _db.SaveChangesAsync();
            _memoryCache.Remove("Diagnosis");//移除缓存
            return rlt;
        }

        /// <summary>
        /// 删除一个诊断
        /// </summary>
        /// <param name="diagId"></param>
        /// <returns></returns>
        public bool DeleteUserDiagnosis(int diagId)
        {
            //判断是否使用了
            if (diagId <= 0) throw new Exception("传入诊断Id错误");
            var used = _db.CHIS_DoctorTreat.Where(m => m.FstDiagnosis == diagId || m.SecDiagnosis == diagId).Count() > 0;
            if (used) throw new Exception("该诊断已经使用，不能删除！");
            var rlt = _db.CHIS_Code_Diagnosis.Find(diagId);
            if (rlt == null) throw new Exception("没有找到该诊断");
            if (rlt.DiagTypeCode == "STANDARD") throw new Exception("标准的诊断，不可删除");
            if (rlt != null) _db.CHIS_Code_Diagnosis.Remove(rlt);
            _db.SaveChanges();
            _memoryCache.Remove("Diagnosis");//移除缓存
            return true;
        }

        /// <summary>
        /// 获取我的历史诊断
        /// </summary>
        /// <param name="searchText">搜索内容</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        /// <returns></returns>
        public List<CHIS_Follow_DoctorSelectDiagnosis> GetMyHistoryDiagnosis(string searchText, int doctorId, int stationId, int pageIndex, int pageSize)
        {
            var finds = _db.CHIS_Follow_DoctorSelectDiagnosis.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId);
            finds = finds.OrderByDescending(m => m.LatestTime);
            finds = finds.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return finds.ToList();
        }
        /// <summary>
        /// 添加一个诊断历史
        /// </summary>
        /// <param name="doctorId"></param>
        /// <param name="stationId"></param>
        /// <param name="diagnosisId"></param>
        /// <returns></returns>
        public void AddHistoryDiagnosis(int doctorId, int stationId, int diagnosisId)
        {
            var id = $"{doctorId}_{stationId}_{diagnosisId}";
            var find = _db.CHIS_Follow_DoctorSelectDiagnosis.Find(id);
            if (find == null) _db.Add(new CHIS_Follow_DoctorSelectDiagnosis
            {
                SelDiagnosisId = id,
                DiagnosisId = diagnosisId,
                LatestTime = DateTime.Now,
                SelectNum = 1,
                DoctorId = doctorId,
                StationId = stationId
            });
            else
            {
                find.SelectNum += 1;
                find.LatestTime = DateTime.Now;
            }
            _db.SaveChanges();
        }


        /// <summary>
        /// 载入需要签名的清单
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId">如果DoctorId=0则表示所有，null默认本医生</param>
        /// <param name="TimeRange"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Ass.Mvc.PageListInfo<CHIS.Models.DataModel.PrescriptionItem> QueryMyRxSignList(string searchText, int doctorId, DateTime? dt0, DateTime? dt1, string signStatus = "ALL", int pageIndex = 1, int pageSize = 20)
        {
            //合并处方
            var qy = (from a in _db.CHIS_DoctorAdvice_Formed.AsNoTracking()
                      select new
                      {
                          PresTypeName = "成药处方",
                          a.TreatId,
                          a.PrescriptionNo,
                          a.RxDoctorSignUrl,
                          a.Amount,
                          a.ChargeStatus,
                          a.DoctorSignUrl
                      })
                      .Union(from b in _db.CHIS_DoctorAdvice_Herbs.AsNoTracking()
                             select new
                             {
                                 PresTypeName = "中药处方",
                                 b.TreatId,
                                 b.PrescriptionNo,
                                 b.RxDoctorSignUrl,
                                 b.Amount,
                                 b.ChargeStatus,
                                 b.DoctorSignUrl
                             }).OrderByDescending(a => a.TreatId);
            //筛选我的处方
            var finds = from a in qy
                        join t in _db.CHIS_DoctorTreat.AsNoTracking() on a.TreatId equals t.TreatId into t_treat
                        from treat in t_treat.DefaultIfEmpty()
                        join r in _db.CHIS_Register.AsNoTracking() on treat.RegisterID equals r.RegisterID into t_reg
                        from reg in t_reg.DefaultIfEmpty()
                        join c in _db.CHIS_Code_Customer.AsNoTracking() on treat.CustomerId equals c.CustomerID into t_cus
                        from cus in t_cus.DefaultIfEmpty()
                        join s in _db.CHIS_Code_WorkStation.AsNoTracking() on treat.StationId equals s.StationID into t_station
                        from station in t_station.DefaultIfEmpty()
                        join dp in _db.CHIS_Code_Department.AsNoTracking() on treat.Department equals dp.DepartmentID into t_dp
                        from depart in t_dp.DefaultIfEmpty()
                        join dr in _db.vwCHIS_Code_Doctor.AsNoTracking() on treat.DoctorId equals dr.DoctorId into t_dr
                        from doctor in t_dr.DefaultIfEmpty()
                        join dr2 in _db.vwCHIS_Code_Doctor.AsNoTracking() on reg.RxDoctorId equals dr2.DoctorId into t_dr2
                        from rxdoctor in t_dr2.DefaultIfEmpty()
                        where reg.RxDoctorId == doctorId && treat.TreatStatus == 2
                        select new CHIS.Models.DataModel.PrescriptionItem
                        {
                            PresTypeName = a.PresTypeName,
                            TreatId = a.TreatId,
                            DoctorSignUrl = a.DoctorSignUrl,
                            RxDoctorSignUrl = a.RxDoctorSignUrl,
                            PrescriptionNo = a.PrescriptionNo,
                            RegisterId = reg.RegisterID,
                            RegisterDate = reg.RegisterDate,
                            RxDoctorId = reg.RxDoctorId,
                            FirstTreatTime = treat.FirstTreatTime,
                            TreatTime = treat.TreatTime,
                            TreatStatus = treat.TreatStatus,
                            CustomerId = treat.CustomerId,
                            CustomerMobile = cus.CustomerMobile,
                            CustomerName = cus.CustomerName,
                            CustomerBirthday = cus.Birthday,
                            CustomerGender = cus.Gender,
                            StationName = station.StationName,
                            DepartmentName = depart.DepartmentName,
                            FstDiagnosis = treat.FstDiagnosis ?? 0,
                            DoctorName = doctor.DoctorName,
                            RxDoctorName = rxdoctor.DoctorName
                        };

            finds = finds.Where(m => (m.FirstTreatTime >= dt0 || m.TreatTime >= dt0) && (m.FirstTreatTime < dt1 || m.TreatTime < dt1));
            if (signStatus == "UNSIGN") finds = finds.Where(m => m.IsRxSigned == false);
            else if (signStatus == "SIGNED") finds = finds.Where(m => m.IsRxSigned == true);
            if (searchText.IsNotEmpty()) finds = finds.Where(m => m.CustomerMobile == searchText || m.CustomerName == searchText);

            var items = finds.OrderByDescending(m => m.TreatId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            //进一步完善数据
            foreach (var item in items)
            {
                item.Diagnosis1 = GetDiagnosisById(item.FstDiagnosis).DiagnoisisName;
            }

            var model = new Ass.Mvc.PageListInfo<CHIS.Models.DataModel.PrescriptionItem>
            {
                DataList = items,
                RecordTotal = finds.Count(),
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return model;
        }




        /// <summary>
        /// 保存接诊和处方
        /// </summary>
        /// <param name="dataTreat"></param>
        /// <returns></returns>
        public async Task<TreatSaveBackData> SaveTreatAndPrescriptionsAsync(
            DoctorTreatV0Input treatMain, IEnumerable<DataFormedV0Input> formeds, IEnumerable<DataHerbV0Input> herbs,
            long mailAddressId,
            int drugStoreStationId,
            int opId, string opMan)
        {
            var mdt = await TreatDataConvertAsync(new DataTreatV0Input { DoctorTreatData = treatMain, FormedPrescriptions = formeds, HerbPrescriptions = herbs }, mailAddressId, drugStoreStationId);
            mdt.DoctorTreatData.TreatStatus = 2;//标记为已诊
            mdt.DoctorTreatData.TreatTime = DateTime.Now;//接诊时间   
            mdt.DoctorTreatData.OpID = opId;
            mdt.DoctorTreatData.OpMan = opMan;
            mdt.DoctorTreatData.OpTime = DateTime.Now;
            //  return true;
            return await SaveTreatAndPrescriptionsAsync(mdt, drugStoreStationId);
        }

        public async Task<DataTreat> TreatDataConvertAsync(DataTreatV0Input dataTreat, long mailAddressId, int drugStoreStationId)
        {
            var rtn = new DataTreat();
            var regist = _db.CHIS_Register.SingleOrDefault(m => m.RegisterID == dataTreat.DoctorTreatData.RegisterId);
            bool needNewTreat = false;
            var hasTreat = _db.CHIS_DoctorTreat.Any(m => m.TreatId == regist.TreatId);
            if (!hasTreat) { needNewTreat = true; }
            else { dataTreat.DoctorTreatData.TreatId = regist.TreatId.Value; }

            var treat = _db.CHIS_DoctorTreat.Find(dataTreat.DoctorTreatData.TreatId);
            var dtreat = dataTreat.DoctorTreatData;
            var cus = _db.CHIS_Code_Customer.SingleOrDefault(m => m.CustomerID == dtreat.CustomerId);


            #region 接诊数据
            if (treat == null)
            {
                treat = new CHIS_DoctorTreat();
                treat.TreatId = dtreat.TreatId;
                treat.TreatStatus = 0;
                treat.Department = regist.Department;
                treat.CustomerId = dtreat.CustomerId;
                treat.RegisterID = dtreat.RegisterId;
                treat.FirstTreatTime = DateTime.Now;
                treat.TreatCustomerAge = (cus.Birthday ?? DateTime.Today).ToAge();
            }
            treat.DoctorId = dtreat.DoctorId;
            treat.Complain = dtreat.Complain;
            treat.PresentIllness = dtreat.PresentIllness;
            treat.FstDiagnosis = dtreat.Diagnosis1Id;
            treat.Examination = dtreat.Examination;
            treat.PresentIllness = dtreat.PresentIllness;
            treat.StationId = dtreat.StationId;
            rtn.DoctorTreatData = treat;
            #endregion

            //数据准备
            long treatId = treat.TreatId;
            var dbformedMain = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            var dbherbMain = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            var dbformedDetails = _db.CHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            var dbherbDetails = _db.CHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            #region 转换成药数据
            //获取主类
            Func<DataFormedV0Input, CHIS_DoctorAdvice_Formed> getFormedMain = (mm) =>
            {
                var maindb = dbformedMain.SingleOrDefault(m => m.PrescriptionNo == mm.Main.PrescriptionNo);
                if (maindb == null) maindb = new CHIS_DoctorAdvice_Formed()
                {
                    TreatId = treat.TreatId,
                    ChargeStatus = 0,
                    CreateTime = DateTime.Now,
                    PrescriptionNo = mm.Main.PrescriptionNo,
                    Amount = 0
                };
                return maindb;
            };
            //获取详细信息
            Func<DataFormedV0Input, IEnumerable<CHIS_DoctorAdvice_Formed_Detail>> getFormedDetails = (mm) =>
            {
                var main = getFormedMain(mm);
                var items = dbformedDetails.Where(m => m.PrescriptionNo == mm.Main.PrescriptionNo).ToList();
                foreach (var item in mm.Detail)
                {
                    var dbitem = items.SingleOrDefault(m => m.AdviceFormedId == item.AdviceFormedId && m.AdviceFormedId > 0);
                    if (dbitem == null)
                    {
                        //==============添加数据 
                        items.Add(new CHIS_DoctorAdvice_Formed_Detail
                        {
                            AdviceFormedId = 0,
                            PrescriptionNo = mm.Main.PrescriptionNo,
                            DrugId = item.DrugId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            GivenRemark = item.GivenRemark,
                            GroupNum = item.GroupNum,
                            StockFromId = item.StockFromId
                        });
                    }
                    else
                    {
                        //==============调整数据
                        if (dbitem.DispensingStatus == 0 && main.ChargeStatus == 0)
                        {
                            dbitem.Qty = item.Qty;
                            dbitem.UnitId = item.UnitId;
                            dbitem.GivenRemark = item.GivenRemark;
                            dbitem.GroupNum = item.GroupNum;
                            dbitem.StockFromId = item.StockFromId;
                        }
                    }
                }
                //============== 删除多余数据
                if (main.ChargeStatus == 0)
                {
                    var ids = mm.Detail.Select(m => m.AdviceFormedId).ToList();
                    items.RemoveAll(m => m.AdviceFormedId > 0 && !ids.Contains(m.AdviceFormedId));
                }
                return items;
            };
            var formedPrescriptions = new List<DataFormed>();
            if (dataTreat.FormedPrescriptions != null)
            {
                foreach (var item in dataTreat.FormedPrescriptions)
                {
                    formedPrescriptions.Add(new DataFormed
                    {
                        Main = getFormedMain(item),
                        Detail = getFormedDetails(item)
                    });
                }
            }
            rtn.FormedPrescriptions = formedPrescriptions;
            #endregion
            #region 转换中药数据
            Func<DataHerbV0Input, CHIS_DoctorAdvice_Herbs> getHerbMain = (mm) =>
            {
                var maindb = dbherbMain.SingleOrDefault(m => m.PrescriptionNo == mm.Main.PrescriptionNo);
                if (maindb == null) maindb = new CHIS_DoctorAdvice_Herbs()
                {
                    TreatId = treat.TreatId,
                    ChargeStatus = 0,
                    PrescriptionNo = mm.Main.PrescriptionNo,
                    Amount = 0
                };
                if (maindb.ChargeStatus == 0)
                {
                    maindb.DoctorAdvice = mm.Main.DoctorAdvice;
                    maindb.GivenRemark = mm.Main.GivenRemark;
                    maindb.GivenTakeTypeId = mm.Main.GivenTakeTypeId;
                    maindb.HerbTitle = mm.Main.HerbTitle;
                    maindb.Qty = mm.Main.Qty;
                }
                return maindb;
            };
            //获取详细信息
            Func<DataHerbV0Input, IEnumerable<CHIS_DoctorAdvice_Herbs_Detail>> getHerbDetails = (mm) =>
            {
                var main = getHerbMain(mm);
                var items = dbherbDetails.Where(m => m.PrescriptionNo == mm.Main.PrescriptionNo).ToList();
                foreach (var item in mm.Detail)
                {
                    var dbitem = items.SingleOrDefault(m => m.Id == item.HerbAdviceId && item.HerbAdviceId > 0);
                    if (dbitem == null)
                    {
                        //==============添加数据 
                        items.Add(new CHIS_DoctorAdvice_Herbs_Detail
                        {
                            PrescriptionNo = mm.Main.PrescriptionNo,
                            CnHerbId = item.DrugId,
                            UnitId = item.UnitId,
                            Qty = item.Qty,
                            HerbUseTypeId = item.HerbUseTypeId,
                            StockFromId = item.StockFromId,
                            TreatId = treat.TreatId
                        });
                    }
                    else
                    {
                        //==============调整数据
                        if (dbitem.DispensingStatus == 0 && main.ChargeStatus == 0)
                        {
                            dbitem.Qty = item.Qty;
                            dbitem.UnitId = item.UnitId;
                            dbitem.StockFromId = item.StockFromId;
                            dbitem.HerbUseTypeId = item.HerbUseTypeId;
                        }
                    }
                }
                //============== 删除多余数据
                if (main.ChargeStatus == 0)
                {
                    var ids = mm.Detail.Select(m => m.HerbAdviceId).ToList();
                    items.RemoveAll(m => m.Id > 0 && !ids.Contains(m.Id));
                }
                return items;
            };

            var herbPrescriptions = new List<DataHerb>();
            if (dataTreat.HerbPrescriptions != null)
            {
                foreach (var item in dataTreat.HerbPrescriptions)
                {
                    herbPrescriptions.Add(new DataHerb
                    {
                        Main = getHerbMain(item),
                        Detail = getHerbDetails(item)
                    });

                }
            }
            rtn.HerbPrescriptions = herbPrescriptions;
            #endregion

            #region 其他费用
            var extraFees = _db.CHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            AddMailInfo(ref extraFees, mailAddressId, drugStoreStationId, treatId);//添加邮费
            AddTreatFee(ref extraFees, treat.DoctorId, treat.StationId, treatId);//添加诊金
            rtn.ExtraFees = extraFees;
            #endregion
            return rtn;
        }

        private CHIS_Doctor_ExtraFee AddTreatFee(ref List<CHIS_Doctor_ExtraFee> extraFees,
            int doctorId, int stationId, long treatId)
        {
            CHIS_Doctor_ExtraFee rtn = null;
            int feeTypeId = DictValues.ExtraFeeType.k_ExtraFeeType_ZJ;
            var dbextra = extraFees.FirstOrDefault(m => m.TreatFeeTypeId == feeTypeId);
            bool brefresh = false;
            if (dbextra == null)
            {
                rtn = new CHIS_Doctor_ExtraFee
                {
                    TreatId = treatId,
                    Qty = 1,
                    ChargeStatus = 0,
                    TreatFeeTypeId = feeTypeId
                };
                extraFees.Add(rtn);
                brefresh = true;
            }
            else if (dbextra != null)
            {
                //更新
                if (dbextra.ChargeStatus == 0)
                {
                    brefresh = true;
                }
            }
            else
            {
                //不需要邮费
            }
            //更新
            if (brefresh)
            {
                rtn = extraFees.FirstOrDefault(m => m.TreatFeeTypeId == feeTypeId);
                rtn.TreatFeeOriginalPrice = GetTreatFeeOrigPrice(doctorId, stationId, out decimal newPrice);
                rtn.TreatFeePrice = newPrice;
                rtn.Amount = newPrice * rtn.Qty;
                rtn.FeeRemark = "诊金";
            }

            return rtn;
        }

        /// <summary>
        /// 获取医生的诊金 原价格
        /// </summary>
        /// <param name="doctorId"></param>
        /// <param name="stationId"></param>
        /// <param name="newPrice">实际价格</param>
        /// <returns>原价格</returns>
        public decimal GetTreatFeeOrigPrice(int doctorId, int stationId, out decimal newPrice)
        {
            decimal origPrice = 0;
            var fee = _db.CHIS_Code_Doctor.Find(doctorId).TreatFee;
            origPrice = fee;
            //todo: 医生诊金没有考虑到工作站和打折情况
            newPrice = fee;
            return origPrice;
            
        }

        public CHIS_Doctor_ExtraFee AddMailInfo(ref List<CHIS_Doctor_ExtraFee> extraFees, long mailAddressId, int drugStoreStationId, long treatId)
        {
            CHIS_Doctor_ExtraFee mailExtraFee = null;
            var address = _db.CHIS_Code_Customer_AddressInfos.AsNoTracking().FirstOrDefault(m => m.AddressId == mailAddressId);
            var mailExtra = extraFees.SingleOrDefault(m => m.TreatFeeTypeId == CHIS.DictValues.ExtraFeeType.k_ExtraFeeType_YF);
            bool brefresh = false;
            if (mailExtra == null && address != null)
            {
                mailExtraFee = new CHIS_Doctor_ExtraFee
                {
                    TreatId = treatId,
                    Qty = 1,
                    ChargeStatus = 0,
                    TreatFeeTypeId = DictValues.ExtraFeeType.k_ExtraFeeType_YF
                };
                extraFees.Add(mailExtraFee);
                brefresh = true;
            }
            else if (mailExtra != null)
            {
                //更新
                if (mailExtra.ChargeStatus == 0)
                {
                    brefresh = true;
                }
            }
            else
            {
                //不需要邮费
            }
            //更新
            if (brefresh)
            {
                mailExtraFee = extraFees.FirstOrDefault(m => m.TreatFeeTypeId == CHIS.DictValues.ExtraFeeType.k_ExtraFeeType_YF);
                mailExtraFee.MailAddressInfoId = mailAddressId;
                mailExtraFee.MailFromAreaId = getMailFromAreaId(drugStoreStationId, out decimal origPrice, out decimal newPrice);
                mailExtraFee.MailToAreaId = address.AreaId;
                mailExtraFee.TreatFeeOriginalPrice = origPrice;
                mailExtraFee.TreatFeePrice = newPrice;
                mailExtraFee.Amount = newPrice * mailExtraFee.Qty;
                mailExtraFee.FeeRemark = "运费";
            }

            return mailExtraFee;
        }

        //获取邮寄起始地址
        private int? getMailFromAreaId(int drugStoreStationId, out decimal origPrice, out decimal newPrice)
        {   //todo:获取邮寄地址
            origPrice = 0;
            newPrice = 0;
            return 1965;
        }

        /// <summary>
        /// 保存接诊和处方
        /// </summary>
        /// <param name="dataTreat"></param>
        /// <returns></returns>
        public async Task<TreatSaveBackData> SaveTreatAndPrescriptionsAsync(DataTreat dataTreat, int drugStoreStationId)
        {
            _db.BeginTransaction();
            try
            {
                //保存数据

                //1.保存接诊
                #region
                long treatId = 0;
                dataTreat.DoctorTreatData.TreatTime = DateTime.Now;
                dataTreat.DoctorTreatData.TreatStatus = 2;

                //搜寻到约诊里的接诊数据
                var reg = _db.CHIS_Register.Find(dataTreat.DoctorTreatData.RegisterID);
                bool needNewTreat = false;
                var hasTreat = _db.CHIS_DoctorTreat.Any(m => m.TreatId == reg.TreatId);
                if (!hasTreat) { needNewTreat = true; }
                else { treatId = dataTreat.DoctorTreatData.TreatId = reg.TreatId.Value; }
                //新增或者更新接诊
                if (needNewTreat)
                {
                    var treat = await _db.CHIS_DoctorTreat.AddAsync(dataTreat.DoctorTreatData);
                    treatId = treat.Entity.TreatId;
                    await _db.SaveChangesAsync();
                    //调整接诊数据                  
                    //reg.TreatId = treatId; //由于数据库有触发，这里不用写，写了反而错误
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _db.CHIS_DoctorTreat.Update(dataTreat.DoctorTreatData);
                    await _db.SaveChangesAsync();
                }
                #endregion


                //获取药库的信息
                var stockDrugList = getStockDrugList(dataTreat.FormedPrescriptions, dataTreat.HerbPrescriptions, drugStoreStationId);

                //2.保存成药
                #region   
                var existFormeds = new List<Guid>();
                if (dataTreat.FormedPrescriptions != null)
                {
                    foreach (var item in dataTreat.FormedPrescriptions)
                    {
                        //只有未支付状态订单，才能保存                        
                        if (item.Main.ChargeStatus == 0)
                        {
                            //配置主键和基础联系数据
                            var preNo = item.Main.PrescriptionNo;
                            item.Main.TreatId = treatId;
                            if (preNo == new Guid())
                            {
                                preNo = Guid.NewGuid();
                                item.Main.PrescriptionNo = preNo;
                            }

                            //整理价格和数据 从具体表到总表
                            var details = _db.CHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.PrescriptionNo == preNo).ToList();
                            var addList = new List<CHIS_DoctorAdvice_Formed_Detail>();
                            var updateList = new List<CHIS_DoctorAdvice_Formed_Detail>();
                            var newList = item.Detail.Select(m => m.AdviceFormedId).ToList();
                            var delList = details.Where(m => !newList.Contains(m.AdviceFormedId));
                            foreach (var mm in item.Detail)
                            {
                                mm.TreatId = treatId;
                                mm.PrescriptionNo = preNo;
                                mm.DispensingStatus = 0;
                                mm.Price = getDrugPrice(stockDrugList, mm.StockFromId, mm.DrugId, mm.UnitId, out string selectStockFromId);
                                mm.Amount = mm.Price * mm.Qty;
                                mm.StockFromId = selectStockFromId;
                                if (details.Any(m => m.AdviceFormedId == mm.AdviceFormedId)) updateList.Add(mm);
                                else addList.Add(mm);
                            }

                            //更新主表数据
                            item.Main.Amount = item.Detail.Sum(m => m.Amount);
                            item.Main.TreatId = treatId;

                            //更新数据库主表
                            if (_db.CHIS_DoctorAdvice_Formed.Any(m => m.PrescriptionNo == preNo))
                                _db.CHIS_DoctorAdvice_Formed.Update(item.Main);
                            else _db.CHIS_DoctorAdvice_Formed.Add(item.Main);
                            existFormeds.Add(preNo);//调整存在的数据

                            //更新数据库从表                          
                            await _db.CHIS_DoctorAdvice_Formed_Detail.AddRangeAsync(addList);
                            _db.CHIS_DoctorAdvice_Formed_Detail.UpdateRange(updateList);
                            //删除多余数据
                            _db.CHIS_DoctorAdvice_Formed_Detail.RemoveRange(delList);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
                //删除多余的主表项目
                var delfmlist = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 0 && (!existFormeds.Contains(m.PrescriptionNo))).ToList();
                _db.CHIS_DoctorAdvice_Formed.RemoveRange(delfmlist);
                var preList = delfmlist.Select(m => m.PrescriptionNo).ToList();
                var delfmds = _db.CHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => preList.Contains(m.PrescriptionNo.Value)).ToList();
                _db.CHIS_DoctorAdvice_Formed_Detail.RemoveRange(delfmds);
                await _db.SaveChangesAsync();
                #endregion
                //3.保存中药
                #region
                var existHerbs = new List<Guid>();
                if (dataTreat.HerbPrescriptions != null)
                {
                    foreach (var item in dataTreat.HerbPrescriptions)
                    {
                        //只有未支付状态订单，才能保存                        
                        if (item.Main.ChargeStatus == 0)
                        {
                            //配置主键和基础联系数据
                            var preNo = item.Main.PrescriptionNo;
                            item.Main.TreatId = treatId;
                            if (preNo == new Guid()) preNo = Guid.NewGuid();

                            //更新从表
                            var details = _db.CHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.PrescriptionNo == preNo).ToList();
                            var addList = new List<CHIS_DoctorAdvice_Herbs_Detail>();
                            var updateList = new List<CHIS_DoctorAdvice_Herbs_Detail>();
                            var newList = item.Detail.Select(m => m.Id).ToList();
                            var delList = details.Where(m => !newList.Contains(m.Id));
                            foreach (var mm in item.Detail)
                            {
                                mm.TreatId = treatId;
                                mm.PrescriptionNo = preNo;
                                mm.DispensingStatus = 0;
                                mm.Price = getDrugPrice(stockDrugList, mm.StockFromId, mm.CnHerbId, mm.UnitId, out string selectStockFromId);
                                mm.Amount = mm.Qty * mm.Price;
                                mm.StockFromId = selectStockFromId;
                                if (details.Any(m => m.Id == mm.Id)) updateList.Add(mm);
                                else addList.Add(mm);
                            }
                            //更新主表
                            item.Main.Amount = item.Detail.Sum(m => m.Amount);
                            item.Main.TreatId = treatId;
                            //更新主表数据库
                            if (_db.CHIS_DoctorAdvice_Herbs.Any(m => m.PrescriptionNo == preNo))
                                _db.CHIS_DoctorAdvice_Herbs.Update(item.Main);
                            else _db.CHIS_DoctorAdvice_Herbs.Add(item.Main);
                            existHerbs.Add(preNo);

                            //更新从表数据库
                            await _db.CHIS_DoctorAdvice_Herbs_Detail.AddRangeAsync(addList);
                            _db.CHIS_DoctorAdvice_Herbs_Detail.UpdateRange(updateList);
                            //删除多余数据
                            _db.CHIS_DoctorAdvice_Herbs_Detail.RemoveRange(delList);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
                //删除多余的主表项目
                var delhblist = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 0 && (!existFormeds.Contains(m.PrescriptionNo))).ToList();
                _db.CHIS_DoctorAdvice_Herbs.RemoveRange(delhblist);
                var prehbList = delhblist.Select(m => m.PrescriptionNo).ToList();
                var delhbs = _db.CHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => prehbList.Contains(m.PrescriptionNo)).ToList();
                _db.CHIS_DoctorAdvice_Herbs_Detail.RemoveRange(delhbs);
                await _db.SaveChangesAsync();
                #endregion

                //4.添加其他费用 邮费诊金
                #region                
                UpdateDbList<CHIS_Doctor_ExtraFee>(_db.CHIS_Doctor_ExtraFee, a => a.TreatId == treatId,
                    dataTreat.ExtraFees, "ExtraFeeId");
                await _db.SaveChangesAsync();
                #endregion
                _db.CommitTran();
                //5.修正数据 主要是总的接诊费用
                #region       
                GetTreatFeeSumary(treatId);//获取总的费用 
                #endregion 
                return new TreatSaveBackData
                {
                    TreatId = treatId,
                    RegistId = reg.RegisterID,
                    FormedPrescriptionKeyIds = dataTreat.FormedPrescriptions.Select(m => m.Main.PrescriptionNo).ToList(),
                    HerbPrescriptionKeyIds = dataTreat.HerbPrescriptions.Select(m => m.Main.PrescriptionNo).ToList()
                };
            }
            catch (Exception ex)
            {
                _db.RollbackTran();
                throw ex;
            }
        }

        //更新数据库数据
        private void UpdateDbList<T>(DbSet<T> dbSet, Func<T, bool> p, IEnumerable<T> newList, string keyName) where T : class
        {
            var dblist = dbSet.AsNoTracking<T>().Where(p).ToList();//数据库现在有的数据
            var tAdd = new List<T>();
            var tUpdate = new List<T>();
            var newListKeys = new List<string>();
            foreach (T item in newList)
            {
                var keyStr = item.GetValue(keyName).ToString();
                if (dblist.Any(m => m.GetValue(keyName).ToString() == keyStr && (!keyStr.IsIdEmpty())))
                    tUpdate.Add(item);
                else tAdd.Add(item);
                newListKeys.Add(keyStr);
            }
            var tDelete = dblist.Where(m => !newListKeys.Contains(m.GetValue(keyName).ToString())).ToList();

            dbSet.RemoveRange(tDelete);
            dbSet.AddRange(tAdd);
            dbSet.UpdateRange(tUpdate);
        }

        //获取药品价格
        private decimal getDrugPrice(List<CHIS_DrugStock_Monitor> stockDrugList, string stockFromId, int drugId, int unitId, out string selectStockFromId)
        {
            CHIS_DrugStock_Monitor src = null;
            var asrc = stockDrugList.FirstOrDefault(m => m.DrugId == drugId);
            var bsrc = stockDrugList.FirstOrDefault(m => m.DrugStockMonitorId == stockFromId);
            src = bsrc != null ? bsrc : asrc;
            if (src == null) throw new BeThrowComException("没有发现该药品");
            selectStockFromId = src.DrugStockMonitorId;
            decimal price = src.StockSalePrice;
            if (unitId == src.StockUnitId) return price;
            else
            {
                //根据药品进行单位转换
                var drug = _db.CHIS_Code_Drug_Outpatient.AsNoTracking().FirstOrDefault(m => m.DrugId == src.DrugId);
                if (drug.UnitBigId == unitId) return price * drug.OutpatientConvertRate;
                else if (drug.UnitSmallId == unitId) return price / drug.OutpatientConvertRate;
                else throw new BeThrowComException($"药品信息没有该大小拆零单位({_dictSvr.FindName(unitId)})");
            }
        }

        //获取库存药品单
        private List<CHIS_DrugStock_Monitor> getStockDrugList(IEnumerable<DataFormed> formedPrescriptions, IEnumerable<DataHerb> herbPrescriptions, int drugStoreStationId)
        {
            var stockId = new List<string>();
            var drugIds = new List<int>();
            foreach (var item in formedPrescriptions)
            {
                stockId.AddRange(item.Detail.Select(m => m.StockFromId));
                drugIds.AddRange(item.Detail.Where(m => string.IsNullOrEmpty(m.StockFromId)).Select(m => m.DrugId));
            }
            foreach (var item in herbPrescriptions)
            {
                stockId.AddRange(item.Detail.Select(m => m.StockFromId));
                drugIds.AddRange(item.Detail.Where(m => string.IsNullOrEmpty(m.StockFromId)).Select(m => m.CnHerbId));
            }

            var stockDrugs = _db.CHIS_DrugStock_Monitor.AsNoTracking().Where(m => stockId.Contains(m.DrugStockMonitorId) ||
                                                        (m.StationId == drugStoreStationId && drugIds.Contains(m.DrugId) && m.StockDrugIsEnable == true)).ToList();
            return stockDrugs;
        }

        /// <summary>
        /// 接诊信息
        /// </summary>
        /// <param name="treatId">接诊号</param>
        public TreatSummary GetTreatSummary(long treatId)
        {
            var f = from item in _db.CHIS_DoctorTreat.AsNoTracking()
                    join r in _db.CHIS_Register.AsNoTracking() on item.RegisterID equals r.RegisterID into t_reg
                    from reg in t_reg.DefaultIfEmpty()
                    join d in _db.vwCHIS_Code_Doctor.AsNoTracking() on reg.RxDoctorId equals d.DoctorId into t_dr
                    from rxdctr in t_dr.DefaultIfEmpty()
                    where item.TreatId == treatId
                    select new TreatSummary
                    {
                        TreatId = item.TreatId,
                        RxDoctorId = reg.RxDoctorId,
                        RxDoctorName = rxdctr.DoctorName
                    };
            return f.AsNoTracking().FirstOrDefault();
        }


        /// <summary>
        /// 获取接诊详情
        /// </summary>
        /// <param name="treatId"></param>
        /// <returns></returns>
        public PatientDetailViewModel GetTreatDetail(long treatId)
        {

            var reg = _db.CHIS_Register.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
            if (reg == null) throw new UnexistedComException("约诊信息不存在");
            var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
            if (treat == null) throw new UnexistedComException("接诊信息不存在");
            var dlst = new int[] { treat.DoctorId, reg.RxDoctorId ?? 0 };
            var doctors = _db.vwCHIS_Code_Doctor.AsNoTracking().Where(m => dlst.Contains(m.DoctorId)).ToList();
            var rxDoctorName = doctors.FirstOrDefault(a => a.DoctorId == reg.RxDoctorId)?.DoctorName;
            var healthInfo = _db.CHIS_Code_Customer_HealthInfo.AsNoTracking().FirstOrDefault(m => m.CustomerId == reg.CustomerID);
            if (healthInfo == null)
            {
                var addEntry = _db.CHIS_Code_Customer_HealthInfo.Add(new Models.CHIS_Code_Customer_HealthInfo { CustomerId = reg.CustomerID.Value });
                _db.SaveChanges();
                healthInfo = addEntry.Entity;
            }


            //其他附加费
            var fees = _db.vwCHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treat.TreatId).ToList();

            //成药数据

            var formedmains = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treat.TreatId).ToList();
            var formedList = new List<CHIS.Models.ViewModels.FormedMainViewModel>();
            foreach (var item in formedmains)
            {
                formedList.Add(new Models.ViewModels.FormedMainViewModel
                {
                    Main = item,
                    Details = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.PrescriptionNo == item.PrescriptionNo).OrderBy(m => m.GroupNum).ToList(),
                    TreatSummary = new TreatSummary
                    {
                        TreatId = treat.TreatId,
                        RxDoctorId = reg.RxDoctorId,
                        RxDoctorName = rxDoctorName
                    }
                });
            }


            //中药数据
            var herbmains = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treat.TreatId).ToList();
            var herbList = new List<Models.ViewModels.CnHerbsMainViewModel>();
            foreach (var item in herbmains)
            {
                herbList.Add(new Models.ViewModels.CnHerbsMainViewModel
                {
                    Main = item,
                    Details = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.PrescriptionNo == item.PrescriptionNo).ToList(),
                    TreatSummary = new TreatSummary
                    {
                        TreatId = treat.TreatId,
                        RxDoctorId = reg.RxDoctorId,
                        RxDoctorName = rxDoctorName
                    }

                });
            }


            var model = new Models.ViewModels.PatientDetailViewModel()
            {
                DoctorTreat = treat,
                CHIS_Code_Customer = _db.vwCHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == reg.CustomerID),
                CHIS_Register = reg,
                CustomerHealthInfo = healthInfo,
                TreatExtraFees = fees,
                FormedList = formedList,// 成药处方
                HerbList = herbList,//中药处方
                FeeSumary = GetTreatFeeSumary(treat.TreatId) //获取费用
            };
            return model;
        }
        public Models.ViewModels.FeeSumaryViewModel GetTreatFeeSumary(long treatId)
        {
            //todo 对运费的综合处理
            return _db.SqlQuery<Models.ViewModels.FeeSumaryViewModel>($"exec sp_DoctorAdvice_Sumary {treatId}").FirstOrDefault();
        }

        /// <summary>
        /// 获取处方详情
        /// </summary>
        /// <param name="prescriptionGuid"></param>
        /// <returns></returns>
        public dynamic GetPrescriptionDetail(Guid? prescriptionGuid)
        {
            if (!prescriptionGuid.HasValue) throw new UnvalidComException("没有传入处方号，请确认是否保存了处方。");
            //判断该处方是中药还是成药处方
            string PRESTYPE = ""; CHIS_DoctorAdvice_Formed mainForm = null; vwCHIS_DoctorAdvice_Herbs mainHerb = null;
            mainForm = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().FirstOrDefault(m => m.PrescriptionNo == prescriptionGuid);
            if (mainForm != null) PRESTYPE = "FORMED";
            else
            {
                mainHerb = _db.vwCHIS_DoctorAdvice_Herbs.AsNoTracking().FirstOrDefault(m => m.PrescriptionNo == prescriptionGuid);
                if (mainHerb != null) PRESTYPE = "HERB";
            }
            if (PRESTYPE.IsEmpty()) throw new UnvalidComException("没有找到该处方信息");

            if (PRESTYPE == "FORMED")
            {
                var detail = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.PrescriptionNo == prescriptionGuid).OrderBy(m => m.GroupNum).ThenBy(m => m.AdviceFormedId).ToList();
                var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == mainForm.TreatId);
                var regist = _db.vwCHIS_Register.AsNoTracking().SingleOrDefault(m => m.RegisterID == treat.RegisterID);
                var rxdoctor = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == regist.RxDoctorId);

                var drugAttr = _drugSvr.GetDrugs(detail.Select(m => m.DrugId).ToList()).Join(
                    _db.CHIS_Code_Supplier, a => a.SupplierId, g => g.SupplierID, (a, g) => new DrugAttrItem
                    {
                        DrugId = a.DrugId,
                        SupplierId = a.SupplierId,
                        SupplierCompanyShortName = g.CompanyShortName,
                        DrugSourceFrom = a.SourceFrom
                    }
                    ).ToList();
                return new CHIS.Models.ViewModel.PrintFormedModel
                {
                    Treat = treat,
                    Detail = detail,
                    Main = mainForm,
                    Regist = regist,
                    RxDoctorName = rxdoctor?.DoctorName,
                    RxDoctorId = rxdoctor?.DoctorId,
                    DrugAttrList = drugAttr
                };
            }
            if (PRESTYPE == "HERB")
            {
                var detail = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.PrescriptionNo == prescriptionGuid);
                var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == mainHerb.TreatId);
                var regist = _db.vwCHIS_Register.AsNoTracking().SingleOrDefault(m => m.RegisterID == treat.RegisterID);
                var rxdoctor = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == regist.RxDoctorId);

                var drugAttr = _drugSvr.GetDrugs(detail.Select(m => m.CnHerbId).ToList()).Join(
                    _db.CHIS_Code_Supplier, a => a.SupplierId, g => g.SupplierID, (a, g) => new DrugAttrItem
                    {
                        DrugId = a.DrugId,
                        SupplierId = a.SupplierId,
                        SupplierCompanyShortName = g.CompanyShortName,
                        DrugSourceFrom = a.SourceFrom
                    }
                    ).ToList();
                return new CHIS.Models.ViewModel.PrintHerbModel
                {
                    Treat = treat,
                    Detail = detail,
                    Main = mainHerb,
                    Regist = regist,
                    RxDoctorName = rxdoctor?.DoctorName,
                    RxDoctorId = rxdoctor?.DoctorId,
                    DrugAttrList = drugAttr
                };
            }
            throw new BeThrowComException("非定义类型的处方");
        }








    }
}
