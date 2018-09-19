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

namespace CHIS.Services
{
    /// <summary>
    /// 处方管理类服务
    /// </summary>
    public class RxService : BaseService
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db"></param>
        public RxService(CHISEntitiesSqlServer db

            ) : base(db)
        {

        }

        //===============================================================================


        /// <summary>
        /// 获取处方记录清单
        /// </summary>
        /// <param name="stationId">加盟药店Id</param>
        /// <param name="index">页数</param>
        /// <param name="doctorId">分店Id</param>
        /// <param name="loginExtId">登录具体人员的Id</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchString"></param>
        /// <returns>数据总数,处方记录列表数据</returns>
        public PaginatedItemsViewModel<RxUserViewModel> GetRxSaveItems(
            int stationId,
            int? doctorId = null,
            int? loginExtId = null,
            DateTime? start = null,
            DateTime? end = null,
            int index = 1,
            int pageSize = 10,
            string searchString = null
            )
        {

            var root = _db.CHIS_DrugStore_RxSave.AsNoTracking().Where(x =>
            x.StationId == stationId &&
            x.IsCompleted == true &&
            x.IsDelete != true);

            #region 根据条件选择数据
            if (doctorId.HasValue)
            {
                root = root.Where(x => x.DoctorId == doctorId);
            }

            if (loginExtId.HasValue)
            {
                root = root.Where(x => x.CustomerId == loginExtId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                root = root.Where(x => x.CustomerName == searchString || x.CustomerMobile == searchString);
            }

            if (start.HasValue && end.HasValue)
            {
                root = root.Where(x => x.SendTime.Date >= start.Value.Date && x.SendTime.Date <= end.Value.Date);
            }

            var count = root.Count();

            if (index >= 1)
            {
                root = root.OrderByDescending(x => x.sysCreateTime).Skip((index - 1) * pageSize).Take(pageSize);
            }
            #endregion


            var models = root./*Include(x => x.DrugList).*/ToList();

            var rxList = new List<RxUserViewModel>();

            #region Mapping
            foreach (var model in models)
            {
                //var drugList = new List<RxDrugViewModel>();

                //if (model.DrugList.Count() > 0)
                //{
                //    foreach (var drug in model.DrugList)
                //    {
                //        drugList.Add(new RxDrugViewModel
                //        {
                //            DrugDeadTime = drug.DrugDeadTime,
                //            DrugId = drug.DrugId,
                //            DrugManufacture = drug.DrugManufacture,
                //            DrugModel = drug.DrugModel,
                //            DrugName = drug.DrugName,
                //            DrugPiNo = drug.DrugPiNo,
                //            DrugQty = drug.DrugQty,
                //            DrugUnitName = drug.DrugUnitName,
                //            RxSaveDrugsId = drug.RxSaveDrugsId,
                //        });
                //    }
                //}
                rxList.Add(new RxUserViewModel
                {
                    CheckDrugMan = model.CheckDrugMan,
                    SendDrugMan = model.SendDrugMan,
                    CheckTime = model.CheckTime,
                    SendTime = model.SendTime,
                    CustomerGenderStr = model.CustomerGenderStr,
                    CustomerId = model.CustomerId,
                    CustomerIdCode = model.CustomerIdCode,
                    CustomerMobile = model.CustomerMobile,
                    CustomerName = model.CustomerName,
                    RxPicUrl1 = model.RxPicUrl1,
                    RxPicUrl2 = model.RxPicUrl2,
                    RxPicUrl3 = model.RxPicUrl3,
                    RxSaveId = model.RxSaveId,
                    //DrugList = drugList
                });

            }
            #endregion

            return new PaginatedItemsViewModel<RxUserViewModel>(index, pageSize, count, rxList);
        }

        /// <summary>
        /// 获取当天本店待添加处方药品的人员信息
        /// </summary>
        /// <param name="stationId">加盟药店Id</param>
        /// <param name="doctorId">分店Id</param>
        /// <returns></returns>
        public IEnumerable<RxUserViewModel> GetNeedAddCustomers(int stationId, int doctorId)
        {
            var models = _db.CHIS_DrugStore_RxSave
                .AsNoTracking().Where(x =>
                x.StationId == stationId &&
                x.DoctorId == doctorId &&
                x.IsCompleted != true &&
                x.IsDelete != true &&
                x.sysCreateTime.Date > DateTime.Now.AddDays(-1)
            ).ToList();

            var rxUserList = new List<RxUserViewModel>();

            #region Mapping

            foreach (var model in models)
            {
                rxUserList.Add(new RxUserViewModel
                {
                    CustomerGenderStr = model.CustomerGenderStr,
                    CustomerId = model.CustomerId,
                    CustomerIdCode = model.CustomerIdCode,
                    CustomerMobile = model.CustomerMobile,
                    CustomerName = model.CustomerName,
                    RxPicUrl1 = model.RxPicUrl1,
                    RxPicUrl2 = model.RxPicUrl2,
                    RxPicUrl3 = model.RxPicUrl3,
                    RxSaveId = model.RxSaveId
                });
            }

            #endregion

            return rxUserList;
        }

        /// <summary>
        /// 获取Rx订单
        /// </summary>
        /// <param name="rxSaveId"></param>
        /// <param name="doctorId"></param>
        /// <param name="isCompleted"></param>
        /// <returns></returns>
        public RxUserViewModel GetRxUser(long rxSaveId, int doctorId, bool isCompleted = false)
        {
            var model = _db.CHIS_DrugStore_RxSave.AsNoTracking().Include(x => x.DrugList)
                .SingleOrDefault(x => x.RxSaveId == rxSaveId && x.DoctorId == doctorId && x.IsDelete != true && x.IsCompleted == isCompleted);

            if (model == null)
            {
                return new RxUserViewModel();
            }

            var drugList = new List<RxDrugViewModel>();

            #region Mapping

            foreach (var drug in model.DrugList)
            {
                drugList.Add(new RxDrugViewModel
                {
                    DrugDeadTime = drug.DrugDeadTime,
                    DrugId = drug.DrugId,
                    DrugManufacture = drug.DrugManufacture,
                    DrugModel = drug.DrugModel,
                    DrugName = drug.DrugName,
                    DrugPiNo = drug.DrugPiNo,
                    DrugQty = drug.DrugQty,
                    DrugUnitName = drug.DrugUnitName,
                    RxSaveDrugsId = drug.RxSaveDrugsId
                });
            }

            #endregion

            return new RxUserViewModel
            {
                CustomerGenderStr = model.CustomerGenderStr,
                CustomerId = model.CustomerId,
                CustomerIdCode = model.CustomerIdCode,
                CustomerMobile = model.CustomerMobile,
                CustomerName = model.CustomerName,
                RxPicUrl1 = model.RxPicUrl1,
                RxPicUrl2 = model.RxPicUrl2,
                RxPicUrl3 = model.RxPicUrl3,
                RxSaveId = model.RxSaveId,
                DrugList = drugList
            };
        }


        /// <summary>
        /// 保存顾客信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns>RxSaveId</returns>
        public long SaveCustomer(CHIS_DrugStore_RxSave model)
        {
            var entity = _db.CHIS_DrugStore_RxSave.Add(model);

            _db.SaveChanges();

            return entity.Entity.RxSaveId;
        }

        /// <summary>
        /// 保存药品信息
        /// </summary>
        /// <param name="rxSaveId"></param>
        /// <param name="rxDrug"></param>
        /// <returns>RxSaveDrugsId,返回-1则出错</returns>
        public long SaveDrug(long rxSaveId, RxDrugViewModel rxDrug)
        {
            var model = new CHIS_DrugStore_RxSave_Drugs
            {
                DrugDeadTime = rxDrug.DrugDeadTime,
                DrugId = rxDrug.DrugId,
                DrugManufacture = rxDrug.DrugManufacture,
                DrugModel = rxDrug.DrugModel,
                DrugName = rxDrug.DrugName,
                DrugPiNo = rxDrug.DrugPiNo,
                DrugQty = rxDrug.DrugQty,
                DrugUnitName = rxDrug.DrugUnitName,
                RxSaveId = rxSaveId
            };

            var entity = _db.CHIS_DrugStore_RxSave_Drugs.Add(model);

            _db.SaveChanges();

            return entity.Entity.RxSaveDrugsId;

        }

        /// <summary>
        /// 删除药品信息,Complete后不可删除
        /// </summary>
        /// <param name="rxSaveDrugId"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public bool DeleteRxDrug(long rxSaveDrugId, int doctorId)
        {
            var entity = _db.CHIS_DrugStore_RxSave_Drugs
                .Where(x => x.RxSaveDrugsId == rxSaveDrugId)
                .Include(x => x.RxOrder)
                .Where(x => x.RxOrder.DoctorId == doctorId && x.RxOrder.IsCompleted != true)
                .SingleOrDefault();

            if (entity == null)
            {
                return false;
            }

            _db.CHIS_DrugStore_RxSave_Drugs.Remove(entity);
            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// 删除Rx订单
        /// </summary>
        /// <param name="rxSaveId"></param>
        /// <param name="doctorId"></param>
        /// <returns>失败返回false</returns>
        public bool DeleteRxOrder(long rxSaveId, int doctorId)
        {
            var entity = _db.CHIS_DrugStore_RxSave
                .SingleOrDefault(x => x.RxSaveId == rxSaveId && x.DoctorId == doctorId && x.IsDelete != true);

            if (entity == null)
            {
                return false;
            }

            entity.IsDelete = true;

            _db.SaveChanges();

            return true;

        }

        /// <summary>
        /// 更新客户信息,删除或完成后不可修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateCustomerInfos(CHIS_DrugStore_RxSave model)
        {
            //var entity = _db.CHIS_DrugStore_RxSave
            //    .Where(x =>
            //    x.RxSaveId == model.RxSaveId &&
            //    x.DoctorId == model.DoctorId &&
            //    x.IsDelete != true &&
            //    x.IsCompleted != true).SingleOrDefault();

            //if (entity == null)
            //{
            //    return false;
            //}

            //entity = model;

            model.IsCompleted = true;

            _db.CHIS_DrugStore_RxSave.Update(model);

            _db.SaveChanges();

            return true;

        }

        /// <summary>
        /// 新注册用户绑定
        /// </summary>
        /// <param name="rxSaveId"></param>
        /// <param name="rxSaveDrugsId"></param>
        public void UpdateDrugsInfo(long rxSaveId, IEnumerable<long> rxSaveDrugsId)
        {
            var drugList = _db.CHIS_DrugStore_RxSave_Drugs.AsNoTracking().Where(x => rxSaveDrugsId.Contains(x.RxSaveDrugsId)).ToList();

            foreach (var drug in drugList)
            {
                drug.RxSaveId = rxSaveId;
            }

            _db.UpdateRange(drugList);

            _db.SaveChanges();

        }

    }
}
