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
using CHIS.DbContext;
using CHIS;
using Microsoft.AspNetCore.Hosting;
using CHIS.Models.ViewModel;

namespace CHIS.Services
{
    public class DispensingService : BaseService
    {
        IHostingEnvironment _env;
        Services.JKWebNetService _jkSvr;
        public DispensingService(CHISEntitiesSqlServer db
            , Services.JKWebNetService jkSvr
            , IHostingEnvironment env
            ) : base(db)
        {
            _env = env;
            _jkSvr = jkSvr;
        }

        //public IQueryable<OrderDetail> GetPayOrderDetails(DrugSourceFrom drugSourceFrom, string payOrderId)
        //{
        //    return MainDbContext.vwCHIS_Charge_Pay_Detail.Where(m => m.SourceFrom == (int)drugSourceFrom && m.PayOrderId == payOrderId).Select(m => new OrderDetail
        //    {
        //        OrderNo = m.PayOrderId,
        //        OrderTime = m.PayedTime.ToString(),
        //        PrescriptionNo = m.PrescriptionNo,
        //        DrugName = m.DrugName,
        //        DrugCode = m.DrugCode,
        //        DrugType = m.DrugModel,
        //        Number = m.Quantity,
        //        UnitId = m.UnitId.Value,
        //        UnitName = m.UnitName,
        //        Price = m.Price,
        //        Amout = m.Amount,
        //        totalPrce = m.TotalAmount,
        //        SourceFrom = m.SourceFrom
        //    });
        //}







        #region 用户地址

        /// <summary>
        /// 设置接诊的药品发货地址
        /// 如果是第一次，则设置为默认地址
        /// 否则则设置为给定的地址
        /// </summary>
        public bool SetTreatMailAddress(long treatId, long? addressId = null, bool bThrowException = true)
        {
            try
            {
                if (treatId == 0) throw new Exception("没有传入接诊Id");
                var treat = _db.CHIS_DoctorTreat.Find(treatId);
                if (treat == null) throw new Exception("没有接诊数据");


                int toAreaId = 0; decimal feeOriginal = 0m;
                var fromAreaId = MPS.CenterAreaId_JK;//广州 荔湾

                var model = _db.CHIS_Doctor_ExtraFee.FirstOrDefault(m => m.TreatId == treatId && m.TreatFeeTypeId == (int)ExtraFeeTypes.TransFee);
                if (model != null)
                {
                    if (addressId > 0)
                    {
                        //调整地址
                        if (model.ChargeStatus == 0) { model.MailAddressInfoId = addressId; }
                        else throw new Exception("费用已收，不能调整地址");
                    }
                    //更新费用
                    var fee = GetTransFee(treatId, model.MailAddressInfoId.Value, fromAreaId, out toAreaId, out feeOriginal);
                    model.TreatFeeOriginalPrice = feeOriginal;
                    model.TreatFeePrice = fee;
                    model.Amount = fee;

                }
                else
                {
                    if (addressId == null)
                    {
                        var defaddr = GetUserDefaultMailAddress(treat.CustomerId);
                        if (defaddr != null) addressId = defaddr.AddressId;
                    }

                    if (addressId.HasValue) //如果有地址
                    {
                        var fee = GetTransFee(treatId, addressId.Value, fromAreaId, out toAreaId, out feeOriginal);
                        _db.CHIS_Doctor_ExtraFee.Add(new CHIS_Doctor_ExtraFee
                        {
                            MailAddressInfoId = addressId,
                            TreatId = treatId,
                            TreatFeeTypeId = (int)ExtraFeeTypes.TransFee,
                            Qty = 1,
                            IsConfirmed = false,
                            TreatFeeOriginalPrice = feeOriginal,
                            TreatFeePrice = fee,
                            Amount = fee,
                            ChargeStatus = 0,
                            MailFromAreaId = fromAreaId,
                            MailToAreaId = toAreaId
                        });
                    }
                }
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                if (bThrowException) throw ex;
                else return false;
            }
        }


        /// <summary>
        /// 获取接诊的邮寄地址
        /// </summary>
        /// <param name="treatId"></param>
        /// <returns></returns>
        public vwCHIS_Doctor_ExtraFee GetTreatMailAddress(long treatId)
        {
            var finds = _db.vwCHIS_Doctor_ExtraFee.Where(m => m.TreatId == treatId);
            if (finds.Count() == 1) return finds.First();
            return null;
        }



        /// <summary>
        /// 计算邮费信息
        /// </summary>
        /// <param name="treatId">接诊号</param>
        /// <param name="addressId">邮寄地址Id</param>
        /// <param name="fromAreaId">出发地</param>
        /// <param name="toAreaId">目的地</param>
        /// <param name="feeOriginal">原价</param>
        /// <returns>实际邮费</returns>
        public decimal GetTransFee(long treatId, long addressId, int fromAreaId, out int toAreaId, out decimal feeOriginal)
        {
            decimal? rtn = null;
            var sum = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == treatId && m.SourceFrom == (int)DrugSourceFrom.WebNet).Sum(m => m.Amount);
            if (sum >= 59) rtn = 0; //满59 免邮费

            toAreaId = 0;
            feeOriginal = 15m;

            var addrfind = _db.CHIS_Code_Customer_AddressInfos.Find(addressId);
            if (addrfind.IsLegalAddress)
            {
                toAreaId = addrfind.AreaId.Value;
                //判断是否在广东省内
                var a3 = _db.SYS_ChinaArea.Find(toAreaId);
                var a2 = _db.SYS_ChinaArea.Find(a3.ParentAreaId);
                var a1 = _db.SYS_ChinaArea.Find(a2.ParentAreaId);
                if (a1 == null) throw new Exception("地址出现错误");
                var b3 = _db.SYS_ChinaArea.Find(fromAreaId);
                var b2 = _db.SYS_ChinaArea.Find(b3.ParentAreaId);
                var b1 = _db.SYS_ChinaArea.Find(b2.ParentAreaId);

                if (a1.AreaId == b1.AreaId) { feeOriginal = 10m; if (rtn == null) rtn = 10m; }
                else { feeOriginal = 15m; if (rtn == null) rtn = 15m; }
                return rtn.Value;
            }
            return 0;
        }

        /// <summary>
        /// 获取用户默认地址
        /// </summary>
        /// <param name="customerId">用户Id</param>
        /// <param name="throwex">是否抛出错误</param>
        /// <returns>null 或者地址实体信息</returns>
        public vwCHIS_Code_Customer_AddressInfos GetUserDefaultMailAddress(int customerId, bool throwex = false, bool bAllAddr = true)
        {
            vwCHIS_Code_Customer_AddressInfos rtn = null;
            try
            {
                var find = _db.vwCHIS_Code_Customer_AddressInfos.FirstOrDefault(m => m.CustomerId == customerId && m.IsDefault == true);
                if (find != null) rtn = find;
                else
                {
                    if (throwex && !bAllAddr) throw new Exception("没有发现默认地址");
                    var finds = _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == customerId);
                    if (finds.Count() == 1) rtn = finds.First();
                    else throw new Exception("没有发现任何地址信息");
                }
                if (rtn != null && rtn.IsLegalAddress) return rtn;
                else { throw new Exception("找到地址信息不完整"); }
            }
            catch (Exception ex)
            {
                if (throwex) throw ex;
            }
            return null;
        }

        /// <summary>
        /// 客户地址统一设置默认为空
        /// </summary>
        /// <param name="custId">客户Id</param>
        public void ClearCustomerAddressInfoAsNotDefault(int custId)
        {
            //批量更新数据库,首先重置默认地址，全部设置为false(0),ture(1)    
            SqlParameter[] pms = new SqlParameter[] { new SqlParameter("@CustomerId", SqlDbType.Int) { Value = custId } };
            _db.Database.ExecuteSqlCommand("Update CHIS_Code_Customer_AddressInfos set IsDefault=0 where CustomerId=@CustomerId", pms);

        }

        /// <summary>
        /// 添加一个用户地址
        /// </summary>
        public vwCHIS_Code_Customer_AddressInfos AddUserMailAddress(int customerId, bool isDefault, string contactName, string contactMobile, int areaId, string detailaddr, string rmk = null)
        {
            var rltEntry = _db.CHIS_Code_Customer_AddressInfos.Add(new CHIS_Code_Customer_AddressInfos
            {
                CustomerId = customerId,
                IsDefault = isDefault,
                ContactName = contactName,
                Mobile = contactMobile,
                AreaId = areaId,
                AddressDetail = detailaddr,
                Remark = rmk
            });
            _db.SaveChanges();
            return _db.vwCHIS_Code_Customer_AddressInfos.Find(rltEntry.Entity.AddressId);
        }


        /// <summary>
        /// 修改一个用户地址
        /// </summary>
        public vwCHIS_Code_Customer_AddressInfos ModifyUserMailAddress(CHIS_Code_Customer_AddressInfos model)
        {
            _db.Update(model);
            _db.SaveChanges();
            return _db.vwCHIS_Code_Customer_AddressInfos.Find(model.AddressId);
        }

        /// <summary>
        /// 获取用户地址信息
        /// </summary>
        /// <param name="customerId">用户Id</param>
        public IEnumerable<vwCHIS_Code_Customer_AddressInfos> GetUserAddressInfos(int customerId)
        {
            return _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().Where(m => m.CustomerId == customerId);
        }
        public IEnumerable<vwCHIS_Code_Customer_AddressInfos> GetUserAddresInfosByTreatId(long treatId)
        {
            var customerId = 0;
            var treat = _db.CHIS_DoctorTreat.Find(treatId);
            if (treat != null) customerId = treat.CustomerId;
            return GetUserAddressInfos(customerId);
        }

        #endregion



        /// <summary>
        /// 增加一个出库项
        /// </summary>
        /// <param name="drugId">药品Id</param>
        /// <param name="qty">数量</param>
        /// <param name="unitId">单位Id</param>
        /// <param name="treatId">接诊Id</param>
        /// <param name="stationId">工作站Id</param>
        /// <param name="rmk">20个字符内</param>
        public async Task AddDispensingDrugOutItemAsync(long doctorAdviceId,string doctorAdviceType, int drugId, int qty, int unitId, long? treatId, int stationId, int drugStoreStationId, string rmk)
        {
            if (!drugId.IsLegalDbId()) throw new Exception("药品Id非法");
            if (qty <= 0) throw new Exception("不符合要求的数量");
            if (stationId <= 0) throw new Exception("不符合要求的工作站Id");
            if (unitId <= 0) throw new Exception("不符合要求的单位Id");

            var a = (from item in _db.CHIS_DrugStock_Monitor.AsNoTracking()
                     where item.DrugId == drugId && item.StockUnitId == unitId && (item.StationId == drugStoreStationId || item.StationId == -1) && item.StockDrugIsEnable == true
                             && item.DrugStockNum >= qty
                     select item).AsNoTracking().FirstOrDefault();

            if (a == null) throw new Exception("没有该药品或该药品库存不足。");

            var newDrugStockNum = a.DrugStockNum - qty;
            //新增出库记录
            _db.CHIS_DrugStock_Out.Add(new CHIS_DrugStock_Out
            {
                DrugId = drugId,
                DrugOutId = Guid.NewGuid(),
                Qty = qty,
                UnitId = unitId,
                TreatId = treatId.Value,
                StationId = stationId,
                DrugStoreStationId = drugStoreStationId,
                OutTime = DateTime.Now,
                StockNumPre = a.DrugStockNum,
                StockNumAft = newDrugStockNum,
                Rmk = rmk,
                StockMonitorId = a.DrugStockMonitorId,
                DoctorAdviceId = doctorAdviceId,
                DoctorAdviceType=doctorAdviceType
            });

            //更新库存                     
            a.DrugStockNum = newDrugStockNum;
            _db.CHIS_DrugStock_Monitor.Update(a);
            // _db.Database.ExecuteSqlCommand($"Update CHIS_DrugStock_Monitor set DrugStockNum={newDrugStockNum} where DrugStockMonitorId='{a.DrugStockMonitorId}'");
            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// 发送各种药品
        /// </summary>
        /// <param name="payOrderId">支付订单号</param>
        /// <returns></returns>
        public async Task<bool> DispenseAllDrugsByPayOrderId(string payOrderId)
        {
            var find = _db.CHIS_Charge_Pay.SingleOrDefault(m => m.PayOrderId == payOrderId);
            if (find == null)
            {
                throw new Exception("订单没有支付,不能发药");
            }

            long treatId = find.TreatId;
            return await DispenseAllDrugsByTreatId(treatId);             
        }
        public async Task<bool> DispenseAllDrugsByTreatId(long treatId)
        {            
            //获取接诊的工作站和默认药库
            var stationId = _db.CHIS_DoctorTreat.Find(treatId).StationId;
            var drugStationId = (_db.CHIS_Code_WorkStation.Find(stationId).DrugStoreStationId) ?? stationId;


            var fms = (from fmd in _db.CHIS_DoctorAdvice_Formed_Detail.AsNoTracking()
                       join f in _db.CHIS_DoctorAdvice_Formed.AsNoTracking() on fmd.PrescriptionNo equals f.PrescriptionNo into t_fm
                       from fm in t_fm.DefaultIfEmpty()
                       join dg in _db.CHIS_Code_Drug_Main.AsNoTracking() on fmd.DrugId equals dg.DrugId into t_dg
                       from drug in t_dg.DefaultIfEmpty()
                       where fmd.TreatId == treatId && fmd.DispensingStatus == 0 && fm.TreatId == treatId && fm.ChargeStatus == 1
                             && drug.SourceFrom == (int)DrugSourceFrom.Local
                       select new { fmd.DrugId, fmd.PrescriptionNo }).ToList();
            var fmsPres = fms.Select(m => m.PrescriptionNo).Distinct().ToList();
            var hbs = (from fmd in _db.CHIS_DoctorAdvice_Herbs_Detail.AsNoTracking()
                       join f in _db.CHIS_DoctorAdvice_Herbs.AsNoTracking() on fmd.PrescriptionNo equals f.PrescriptionNo into t_fm
                       from fm in t_fm.DefaultIfEmpty()
                       join dg in _db.CHIS_Code_Drug_Main.AsNoTracking() on fmd.CnHerbId equals dg.DrugId into t_dg
                       from drug in t_dg.DefaultIfEmpty()
                       where fmd.TreatId == treatId && fmd.DispensingStatus == 0 && fm.TreatId == treatId && fm.ChargeStatus == 1
                             && drug.SourceFrom == (int)DrugSourceFrom.Local
                       select new { DrugId = fmd.CnHerbId, fmd.PrescriptionNo }).ToList();
            var hbsPres = hbs.Select(m => m.PrescriptionNo).Distinct().ToList();

            //todo 此处有bug，就是多个相同药品发药的时候，会引发不能成功发送的问题。
            //发送本地成药
            var bNoPayed = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 0).Count() > 0;
            if (bNoPayed) throw new ComException(ExceptionTypes.Error_BeThorw, "该接诊有订单没有支付");

            foreach (var pre in fmsPres)
            {
                await SendDrug_LocalFormed(fms.Where(m => m.PrescriptionNo == pre).Select(m => m.DrugId)
                    , treatId, pre, stationId, drugStationId);
            }
            //发送本地中药
            foreach (var pre in hbsPres)
            {
                await SendDrug_LocalHerb(hbs.Where(m => m.PrescriptionNo == pre).Select(m => m.DrugId)
                    , treatId, pre, stationId, drugStationId);
            }

            //发送网络三方药品
            try
            {
                SendDrug_SendOrderToNetJK(treatId, "默认自动发药");
            }
            catch (Exception ex) { if (!(ex is NotProcessException)) throw ex; }
            return true;
        }

        #region 发药


        /// <summary>
        /// 发送本地药品
        /// </summary>
        /// <param name="drugs">核查药品</param>
        /// <param name="treatId">接诊号</param>
        /// <param name="prescriptionNo">处方单号</param>
        /// <param name="stationId">发药工作站</param>
        /// <param name="drugStoreStationId">药品库所在工作站</param>
        /// <returns></returns>
        public async Task<bool> SendDrug_LocalFormed(IEnumerable<int> drugs, long treatId, Guid? prescriptionNo
           , int stationId, int drugStoreStationId
            )
        {
            if (treatId == 0) throw new Exception("没有传入接诊号");
            if (drugs.Count() == 0) throw new Exception("没有医药品传入");
            var drugs1 = drugs.ToList();
            drugs1.Sort();
            var form = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId && m.PrescriptionNo == prescriptionNo);
            if (form.ChargeStatus != 1) throw new Exception("该处方单付款状态非 已支付");
            var finds = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.PrescriptionNo == form.PrescriptionNo);
            finds = finds.Where(m => m.DispensingStatus == 0 && m.SourceFrom == (int)DrugSourceFrom.Local);//本地药品
            var findslist = finds.ToList();
            var drugs2 = finds.Select(m => m.DrugId).ToList(); drugs2.Sort();
            if (string.Join(",", drugs1) != string.Join(",", drugs2)) throw new Exception("发药的药品清单校验错误");
            //可以发药了,采用事务来发药                


            //----- 写状态 ------
            string nowDrug = "";
            var ids = findslist.Select(m => m.AdviceFormedId);
            var modfinds = _db.CHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => ids.Contains(m.AdviceFormedId)).ToList();

            //-----发送每一笔药品，并写出库记录， 写日志 -------        
            foreach (var item in findslist)
            {
                try
                {
                    _db.BeginTransaction();
                    nowDrug = $"{item.DrugName}({item.DrugId})";

                    //1.修改药品发药状态
                    var mitem = modfinds.FirstOrDefault(m => m.AdviceFormedId == item.AdviceFormedId);
                    mitem.DispensingStatus = 1;
                    _db.CHIS_DoctorAdvice_Formed_Detail.Update(mitem);

                    //单位要转换成为出库小单位
                    int sqty = item.Qty;
                    int sunit = item.UnitId;
                    if (item.UnitId != item.UnitSmallId && item.UnitId == item.UnitBigId)
                    {
                        sunit = item.UnitSmallId.Value;
                        sqty = (int)(item.Qty * item.OutpatientConvertRate);
                    }
                    //2.扣减库存项目                     
                    await AddDispensingDrugOutItemAsync(item.AdviceFormedId,"FORMED", item.DrugId, sqty, sunit, item.TreatId, stationId, drugStoreStationId, "本地药");
                    //3.写入发药日志
                    await _db.CHIS_Shipping_DispensingLog.AddAsync(new CHIS_Shipping_DispensingLog
                    {
                        TreatId = form.TreatId,
                        DrugId = item.DrugId,
                        Qty = item.Qty,
                        Price = item.Price,
                        AdivceDetailId = item.AdviceFormedId,//所在记录位置
                        PrescriptionNo = item.PrescriptionNo,
                        DispensingTime = DateTime.Now
                    });
                    await _db.SaveChangesAsync();
                    _db.CommitTran();
                }
                catch (Exception ex)
                { _db.RollbackTran(ex); throw new Exception(nowDrug + ex.Message); }
            }
            return true;
        }


        /// <summary>
        /// 发送本地中药
        /// </summary>
        /// <param name="drugs">核查药品</param>
        /// <param name="treatId">接诊号</param>
        /// <param name="prescriptionNo">处方单号</param>
        /// <param name="stationId">发药工作站</param>
        /// <param name="drugStoreStationId">药品库所在工作站</param>
        /// <returns></returns>
        public async Task<bool> SendDrug_LocalHerb(IEnumerable<int> drugs, long treatId, Guid? prescriptionNo
            , int stationId, int drugStoreStationId
             )
        {

            if (treatId == 0) throw new Exception("没有传入接诊号");
            if (drugs.Count() == 0) throw new Exception("没有中药传入");
            var drugs1 = drugs.ToList();
            drugs1.Sort();
            var herb = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId && m.PrescriptionNo == prescriptionNo);
            if (herb.ChargeStatus != 1) throw new Exception("该处方单付款状态非 已支付");
            var finds = _db.CHIS_DoctorAdvice_Herbs_Detail.Where(m => m.PrescriptionNo == herb.PrescriptionNo);
            finds = finds.Where(m => m.DispensingStatus == 0);
            var findslist = finds.ToList();
            var drugs2 = finds.Select(m => m.CnHerbId).ToList(); drugs2.Sort();
            if (string.Join(",", drugs1) != string.Join(",", drugs2)) throw new Exception("发药的药品清单校验错误");
            //可以发药了   
            //-----写出库记录， 写日志 -------                  
            foreach (var item in findslist)
            {
                try
                {
                    _db.BeginTransaction();
                    //1.更新发药状态
                    item.DispensingStatus = 1;
                    _db.CHIS_DoctorAdvice_Herbs_Detail.Update(item);

                    //写入发药出库记录
                    await AddDispensingDrugOutItemAsync(item.Id,"HERB", item.CnHerbId, item.Qty, item.UnitId, item.TreatId, stationId, drugStoreStationId, "本地药");

                    //写入发药日志
                    await _db.CHIS_Shipping_DispensingLog.AddAsync(new CHIS_Shipping_DispensingLog
                    {
                        TreatId = herb.TreatId,
                        DrugId = item.CnHerbId,
                        Qty = item.Qty,
                        Price = item.Price,
                        AdivceDetailId = item.Id,
                        PrescriptionNo = item.PrescriptionNo,
                        DispensingTime = DateTime.Now
                    });
                    await _db.SaveChangesAsync();
                    _db.CommitTran();
                }
                catch (Exception ex) { _db.RollbackTran(ex); throw ex; }
            }
            return true;
        }





        /// <summary>
        /// 给第三方推送订单
        /// </summary>
        /// <remarks>方法:Json_SendOrderToNet(int treatId)</remarks>
        /// <param name="treatId">接诊ID</param>
        /// <returns></returns>
        public bool SendDrug_SendOrderToNetJK(long treatId, string sendRmk)
        {
            var vwTreat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
            var vwExtraFee = _db.vwCHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treatId && m.TreatFeeTypeId == (int)ExtraFeeTypes.TransFee && m.MailForSupplierId == MPS.SupplierId_JK && m.ChargeStatus == ChargeStatus.Payed).ToList();
            int custId = vwTreat.CustomerId;//客户Id 
                                            /*
                                             *获取第三方药品的总金额
                                             */
            var formed = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => m.TreatId == treatId &&
                                        m.SourceFrom == (int)DrugSourceFrom.WebNet &&
                                        m.SupplierId == MPS.SupplierId_JK &&
                                        m.ChargeStatus == ChargeStatus.Payed &&
                                        m.DispensingStatus == (int)DispensingStatus.NeedSend).ToList();
            var herbs = _db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => m.TreatId == treatId &&
                                        m.SourceFrom == (int)DrugSourceFrom.WebNet &&
                                        m.SupplierId == MPS.SupplierId_JK &&
                                        m.ChargeStatus == ChargeStatus.Payed &&
                                        m.DispensingStatus == (int)DispensingStatus.NeedSend).ToList();

            if (formed.Count + herbs.Count == 0) throw new NotProcessException(); //抛出不需要处理的错误

            long selectedAddressId = (long)vwTreat.TransferAddressId;//选择邮寄地址Id
            var moblie = vwTreat.CustomerMobile;//客户手机号
            var orderId = Guid.NewGuid().ToString();//给第三方的订单Id

            var formedAmout = formed.Sum(e => e.Amount);
            var herbsAmout = herbs.Sum(e => e.Amount);
            var ext = vwExtraFee.Sum(m => m.Amount);
            decimal totalamount = formedAmout + ext;
            var sendAddr = _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().FirstOrDefault(m => m.AddressId == selectedAddressId);

            if (custId == 0 && orderId == null) throw new Exception("用户的ID和订单编号不能为空");

            var FormedDoctorAdvices = new List<vwCHIS_DoctorAdvice_Formed_Detail>();//发送成药的信息记录
                                                                                    //查询是否已经存在该订单号    
            var netOrderItem = _db.CHIS_Shipping_NetOrder.AsNoTracking().FirstOrDefault(m => m.SendOrderId == orderId);
            //IsSendedSuccess 0:默认是0.表示未发送订单，1表示发送成功订单，2表示发送失败
            if (netOrderItem != null && netOrderItem.SendedStatus == (int)SendState.SendSucces) throw new Exception("该订单已经发送了");
            else if (netOrderItem == null)
            {
                //4.将查询到的数据存入netOrderEntity对象中
                netOrderItem = new Models.CHIS_Shipping_NetOrder
                {
                    StationId = vwTreat.StationId,
                    TreatId = treatId,
                    CreatTime = DateTime.Now,
                    SendTime = DateTime.Now,
                    TreatTime = vwTreat.TreatTime,
                    SendOrderId = orderId,
                    CustomerId = custId,
                    SendAddressId = selectedAddressId,
                    TotalAmount = totalamount,
                    ContainTransFee = ext,
                    IsRdOrder = _env.IsDevelopment(),//是否是测试数据
                    SupplierId = MPS.SupplierId_JK
                };

                netOrderItem = _db.Add(netOrderItem).Entity;
                _db.SaveChanges();
                var netdrugs = _db.vwCHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(
                    m => m.TreatId == treatId && m.SourceFrom == (int)DrugSourceFrom.WebNet && m.SupplierId == MPS.SupplierId_JK && m.DispensingStatus == (int)DispensingStatus.NeedSend).ToList();
                foreach (var item in netdrugs)
                {
                    var detailEnty = new Models.CHIS_Shipping_NetOrder_Formed_Detail
                    {
                        DrugId = item.DrugId,
                        PrescriptionNo = item.PrescriptionNo.ToString(),
                        Qty = item.Qty,
                        Price = item.Price,
                        Amount = item.Amount,
                        UnitName = item.UnitName,
                        NetOrderId = netOrderItem.NetOrderId,
                        DoctorAdviceId = item.AdviceFormedId //医生接诊开药的Id
                    };
                    _db.Add(detailEnty);
                    FormedDoctorAdvices.Add(item);//方便发送成功后写入日志
                }
                _db.SaveChanges();
            }
            if (!sendAddr.Mobile.IsMobileNumber()) throw new Exception("手机号码格式错误！");

            //发送给健客信息                    
            string FromJKSavedOrderNo = "";
            var bStatus = _jkSvr.SendJKWebNetOrder(totalamount, orderId, selectedAddressId, sendRmk, out FromJKSavedOrderNo);
            if (!bStatus) throw new Exception("发送给三方健客信息没有成功");


            var enty = _db.CHIS_Shipping_NetOrder.FirstOrDefault(m => m.SendOrderId == orderId);
            //更新数据库
            if (bStatus && FromJKSavedOrderNo.IsNotEmpty())
            {
                enty.SendedStatus = (int)SendState.SendSucces;
                enty.NetOrderNO = FromJKSavedOrderNo;
                enty.SendTime = DateTime.Now;
                _db.SaveChanges();

                //回写状态和日志
                foreach (var item in FormedDoctorAdvices)
                {
                    //修改发药的状态
                    // MainDbContext.CHIS_DoctorAdvice_Formed_Detail.Find(item.AdviceFormedId).DispensingStatus = 1;//发药
                    var afd = _db.CHIS_DoctorAdvice_Formed_Detail.FirstOrDefault(m => m.AdviceFormedId == item.AdviceFormedId);
                    afd.DispensingStatus = 1;
                    _db.SaveChanges();
                    //记录一份发药日志
                    AddSendDrugLog(item.AdviceFormedId, item.DrugId, item.PrescriptionNo.Value, item.Price, item.Qty, item.TreatId);
                }



                //给该用户发送短信通知           
                Codes.Utility.SMS sms = new Codes.Utility.SMS();
                string getormsg = $"尊敬的{sendAddr.ContactName}您好,你的药品已发货，请保持电话畅通，祝您身体健康。【天使健康】"; //收件人的
                sms.PostSmsInfoAsync(sendAddr.Mobile, getormsg).ToString();
                //if (sendAddr.Mobile != payOrderInfor.Telephone)
                //{ //账号所有者
                //    string ownermsg = $"尊敬的{payOrderInfor.CustomerName}您好，您于{payOrderInfor.PayedTime.ToString("yyyy年MM月dd日")}在{payOrderInfor.StationName}就诊药品已发货，请注意查收，发货地址:{sendAddr.FullAddress},收件人：{sendAddr.ContactName},联系电话:{sendAddr.Mobile}，请保持电话畅通，祝您身体健康。【天使健康】";
                //    sms.PostSmsInfoAsync(payOrderInfor.Telephone, ownermsg).ToString();
                //}
            }
            else
            {
                enty.SendedStatus = (int)SendState.SendFail;
                enty.SendTime = DateTime.Now;
                _db.SaveChanges();
                throw new Exception("发送数据给健客失败！");
            }

            return true;
        }




        /// <summary>
        /// 添加发送药品的Log
        /// </summary>
        /// <param name="doctorAdviceId">开药的具体的医嘱Id</param>
        /// <param name="drugId">药品号</param>
        /// <param name="prescriptionNo">处方单号</param> 
        public void AddSendDrugLog(long doctorAdviceId, int drugId, Guid prescriptionNo, decimal price, int qty, long treatId)
        {
            //记录一份发药日志
            _db.CHIS_Shipping_DispensingLog.Add(new CHIS_Shipping_DispensingLog
            {
                AdivceDetailId = doctorAdviceId,
                DispensingTime = DateTime.Now,
                DrugId = drugId,
                PrescriptionNo = prescriptionNo,
                Price = price,
                Qty = qty,
                TreatId = treatId
            });
            _db.SaveChanges();
        }

        #endregion


        #region 获取列表
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="pageIndex"></param>
        /// <param name="dispensingStatus">发送状态 0 未发 1 已发 2 其他</param>
        /// <param name="tstart"></param>
        /// <param name="tend"></param>
        /// <returns></returns>
        public Ass.Mvc.PageListInfo<Models.ViewModel.DispensingItemViewModel> GetDispenseList(            
            string searchText,int stationId, DateTime dt0, DateTime dt1, int dispensingStatus = 0, 
            int pageIndex = 1, int pageSize = 20,int? opId=null)
        {
 

            var finds = _db.SqlQuery<DispensingItemViewModel>(string.Format("exec sp_Dispensing_TreatList {0},{1},'{2:yyyy-MM-dd}','{3:yyyy-MM-dd}'", dispensingStatus, stationId, dt0, dt1));
                       
       
            if (opId.HasValue) finds = finds.Where(m => m.RegistOpId == opId).ToList();
            else if (searchText.IsNotEmpty()) finds = finds.Where(m => m.CustomerName.Contains(searchText) || m.CustomerMobile == searchText).ToList();
            //分页
            var list = finds.OrderBy(m => m.TreatTime).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var model = new Ass.Mvc.PageListInfo<Models.ViewModel.DispensingItemViewModel>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = finds.Count(),
                DataList = list
            };
            return model;      
        }

        #region 发药详细页面

        /// <summary>
        /// 获取发药详情
        /// </summary>
        /// <param name="treatId">接诊号</param>  
        public DispensingDetailViewModel DispensingDetail(long treatId)
        {
            if (treatId == 0) throw new Exception("接诊号错误");
            var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
            var station = _db.vwCHIS_Code_WorkStation.AsNoTracking().FirstOrDefault(m => m.StationID == treat.StationId);
            var customer = _db.vwCHIS_Code_Customer.AsNoTracking().FirstOrDefault(m => m.CustomerID == treat.CustomerId);
            var formedPre = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId);
            var herbPre = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId);

            var _du = GetDrugDatas(treatId);//载入药品数据

            //获取药店列表
            var lst0 = _du.herbs.Select(m => new { SourceFrom = m.SourceFrom, SupplierId = m.SupplierId }).ToList();
            var lst = _du.formeds.Select(m => new { SourceFrom = m.SourceFrom, SupplierId = m.SupplierId }).ToList();
            lst.AddRange(lst0);
            lst = lst.OrderBy(m => m.SourceFrom).ThenBy(m => m.SupplierId).ToList();
            var supplierids = lst.Select(m => m.SupplierId).Distinct();
            var suppliers = _db.CHIS_Code_Supplier.AsNoTracking().Where(m => supplierids.Contains(m.SupplierID)).ToList();
            var drugStore = new Dictionary<DrugStoreItem, DispensingDetailOfStoreViewModel>(); //按药店分类，存放所有药品内容
            foreach (var item in lst)
            {
                if (drugStore.Keys.Any(m => (m.DrupSourceFrom == item.SourceFrom && (m.Supplier == null || m.Supplier.SupplierID == item.SupplierId)))) continue;
                var key = new DrugStoreItem
                {
                    DrupSourceFrom = item.SourceFrom.Value,
                    Supplier = suppliers.FirstOrDefault(m => m.SupplierID == item.SupplierId)
                };
                var val = GetDetailOfStore(key, treat, station, _du.formeds, _du.herb, _du.herbs);
                drugStore.Add(key, val);
            }



            var model = new DispensingDetailViewModel
            {
                Treat = treat,
                Customer = customer,
                SelectedAddress = _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().FirstOrDefault(m => m.AddressId == treat.TransferAddressId),
                DispensingDetailSumary = new Models.ViewModel.DispensingDetailSumary
                {
                    FormedPrescription = formedPre,
                    HerbPrescription = herbPre,
                    Formed = _du.formeds,
                    Herb = _du.sumary_herb
                },
                DrugStoreDetails = drugStore
            };
            return model;
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

        #endregion

    }
}
