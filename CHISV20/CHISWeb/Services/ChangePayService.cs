using Ass;
using CHIS.Models;
using CHIS.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CHIS.DbContext;
using CHIS;
using CHIS.Models.ViewModels;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CHIS.Services
{
    public class ChangePayService : BaseService
    {

        public static Random Rand = new Random();
        DispensingService _dispensingSvr;
        Code.Managers.IMyLogger _logger;
        IHostingEnvironment _env;
        CustomerService _cusSvr;
        PointsDetailService _pointsService;
        TreatService _treatSvr;
        AccessService _accSvr;
        Codes.Utility.XPay.AliPay _aliPay;
        WeChatService _weChatSvr;
        IHttpContextAccessor _httpContextAccessor;
        public ChangePayService(CHISEntitiesSqlServer db,
            DispensingService dispensingSvr,
            IHostingEnvironment env
            , CustomerService cusSvr
            , PointsDetailService pointsService
            , TreatService treatSvr
            , AccessService accSvr
            , DispensingService dispSvr
            , Codes.Utility.XPay.AliPay aliPay
             , WeChatService weChatSvr
            , Code.Managers.IMyLogger logger
            , IHttpContextAccessor httpContextAccessor
            ) : base(db)
        {
            _dispensingSvr = dispensingSvr;
            _env = env;
            _cusSvr = cusSvr;
            _pointsService = pointsService;
            _treatSvr = treatSvr;
            _accSvr = accSvr;
            this._logger = logger;
            _aliPay = aliPay;
            _weChatSvr = weChatSvr;
            _httpContextAccessor = httpContextAccessor;
        }


        #region 支付列表

        /// <summary>
        /// 已经支付了
        /// </summary>
        /// <param name="dt0"></param>
        /// <param name="dt1"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Ass.Mvc.PageListInfo<PayedItem> GetChargeListPayedModel(string searchText, bool bAllClinic, DateTime dt0, DateTime dt1, int stationId, int? doctorId = null, int? registOpId = null, int pageIndex = 1, int pageSize = 20)
        {
            var f = _db.vwCHIS_Charge_Pay.AsNoTracking().Join(
                _db.vwCHIS_DoctorTreat, a => a.TreatId, g => g.TreatId, (a, g) => new
                {
                    a.DoctorId,
                    a.StationId,
                    a.TreatTime,
                    g.RegistOpId,
                    a.CustomerId,
                    a.CustomerName,
                    a.CustomerMobile,
                    a.Gender,
                    a.PayedTime,
                    a.PayOrderId,
                    a.PayId,
                    a.TotalAmount,
                    a.TreatId,
                    a.FeeTypeCode,
                    a.PayRemark
                });

            f = f.Where(m => m.StationId == stationId && m.TreatTime >= dt0 && m.TreatTime < dt1);

            if (registOpId.HasValue) f = f.Where(m => m.RegistOpId == registOpId);
            else if (doctorId > 0 && !bAllClinic) f = f.Where(m => m.DoctorId == doctorId);

            if (searchText.IsNotEmpty())
            {
                var tp = searchText.GetStringType();
                if (tp.IsMobile) f = f.Where(m => m.CustomerMobile == tp.String);
                else
                    f = f.Where(m => m.CustomerName.Contains(searchText));
            }
            var find = f.Select(m => new PayedItem
            {
                CustomerId = m.CustomerId.Value,
                CustomerName = m.CustomerName,
                Gender = m.Gender,
                TreatTime = m.TreatTime,
                TreatId = m.TreatId,
                PayedTime = m.PayedTime,
                PayOrderId = m.PayOrderId,
                PayId = m.PayId,
                TotalAmount = m.TotalAmount,
                PayRemark = m.PayRemark,
                FeeTypeCode = m.FeeTypeCode,
                FeeTypeCodeName = FeeTypes.ToName(m.FeeTypeCode)
            });

            var total = find.Count();
            var list = find.OrderByDescending(m => m.PayedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            //获取图片
            var pics = _cusSvr.GetCustomersImage(list.Select(m => m.CustomerId).ToList());
            foreach (var item in list)
            {
                item.CustomerPic = pics.FirstOrDefault(m => m.CustomerId == item.CustomerId)?.PicUrl;
            }

            return new Ass.Mvc.PageListInfo<PayedItem>()
            {
                PageSize = pageSize,
                RecordTotal = total,
                PageIndex = pageIndex,
                DataList = list
            };

        }

        /// <summary>
        /// 待支付
        /// </summary>
        /// <param name="dt0"></param>
        /// <param name="dt1"></param>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Ass.Mvc.PageListInfo<ChargeCustomerItem> GetChargeListNeedPayModel(string searchText, bool bAllClinic, DateTime dt0, DateTime dt1, int stationId, int? doctorId = null, int? registOpId = null, int pageIndex = 1, int pageSize = 20)
        {
            var lst = _db.vwCHIS_Charge_DoctorTreated_NeedPay.AsNoTracking().Where(m => m.TreatTime >= dt0 && m.TreatTime < dt1 && m.StationId == stationId);

            if (registOpId.HasValue) lst = lst.Where(m => m.RegistOpId == registOpId.Value);
            else if (doctorId > 0 && !bAllClinic) lst = lst.Where(m => m.DoctorId == doctorId);

            if (searchText.IsNotEmpty())
            {
                var tp = searchText.GetStringType();
                if (tp.IsMobile) lst = lst.Where(m => m.CustomerMobile == tp.String);
                else
                    lst = lst.Where(m => m.CustomerName.Contains(searchText));
            }

            var find = lst.Select(m => new ChargeCustomerItem
            {
                CustomerId = m.CustomerId,
                CustomerName = m.CustomerName,
                Gender = m.Gender,
                Birthday = m.Birthday,
                TreatTime = m.TreatTime,
                TreatId = m.TreatId,
                TreatStationName = m.StationName,
                NeedPayAmount = m.NeedPayAmount,
                TreatAmount = m.TreatAmount
            });

            var total = find.Count();
            var list = find.OrderBy(m => m.TreatTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var pics = _cusSvr.GetCustomersImage(list.Select(m => m.CustomerId).ToList());
            foreach (var item in list) item.CustomerPic = pics.FirstOrDefault(m => m.CustomerId == item.CustomerId)?.PicUrl;
            return new Ass.Mvc.PageListInfo<ChargeCustomerItem>()
            {
                PageSize = pageSize,
                RecordTotal = total,
                PageIndex = pageIndex,
                DataList = list
            };

        }


        /// <summary>
        /// 获取所有需要支付的订单
        /// </summary>
        /// <param name="customerId">用户Id</param>
        /// <returns></returns>
        public IEnumerable<vwCHIS_Charge_DoctorTreated_NeedPay> GetChargeListNeedPayModelByCustomerId(int customerId, DateTime dt0, DateTime dt1)
        {
            if (customerId == 0) throw new Exception("请传入用户Id");
            var lst = _db.vwCHIS_Charge_DoctorTreated_NeedPay.AsNoTracking().Where(m => m.TreatTime >= dt0 && m.TreatTime < dt1);
            lst = lst.Where(m => m.CustomerId == customerId);
            return lst;
        }

        /// <summary>
        /// 获取已经支付的列表
        /// </summary> 
        public IEnumerable<vwCHIS_Charge_Pay> GetChargeListPayedModelByCustomerId(int customerId, DateTime dt0, DateTime dt1, int pageIndex, int pageSize)
        {
            var lst = _db.vwCHIS_Charge_Pay.AsNoTracking().Where(m => m.CustomerId == customerId && m.PayedTime >= dt0 && m.PayedTime < dt1).OrderByDescending(m => m.PayId);
            return lst.OrderByDescending(m => m.PayedTime).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }



        //支付后的信息
        public PayedModel GetPayedInfo(long payedId)
        {
            var pay = _db.vwCHIS_Charge_Pay.AsNoTracking().FirstOrDefault(m => m.PayId == payedId);
            if (pay == null) throw new Exception("查找支付清单错误");
            var treatId = pay.TreatId;
            var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);//&& m.TreatStatus == 2
            var cus = _db.vwCHIS_Code_Customer.AsNoTracking().FirstOrDefault(m => m.CustomerID == treat.CustomerId);
            var doctor = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == treat.DoctorId);
            var defaddrss = _dispensingSvr.GetUserDefaultMailAddress(cus.CustomerID);//默认地址
            var seladdress = _db.vwCHIS_Code_Customer_AddressInfos.FirstOrDefault(m => m.AddressId == treat.TransferAddressId);//记录的邮寄地址

            var pays = _db.CHIS_Charge_Pay.AsNoTracking().Where(m => m.TreatId == treatId);
            var paylist = pays.Select(m => m.PayId);
            var extras = _db.vwCHIS_Charge_Pay_Detail_ExtraFee.AsNoTracking().Where(m => paylist.Contains(m.PayId));


            //成药
            var formedm = _db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            var formeds = _db.vwCHIS_Charge_Pay_Detail_Formed.Where(m => paylist.Contains(m.PayId)).ToList();
            var formed = new Dictionary<CHIS_DoctorAdvice_Formed, IEnumerable<vwCHIS_Charge_Pay_Detail_Formed>>();
            var formedlist = formeds.Select(m => m.PrescriptionNo).Distinct();
            foreach (var pno in formedlist)
            {
                var key = formedm.FirstOrDefault(m => m.PrescriptionNo == pno);
                formed.Add(key, formeds.Where(m => m.PrescriptionNo == pno));
            }

            //中草药
            var herbm = _db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            var herbs = _db.vwCHIS_Charge_Pay_Detail_Herb.AsNoTracking().Where(m => paylist.Contains(m.PayId)).ToList();
            var herbmIds = herbs.Select(m => m.PrescriptionNo).Distinct();
            var herb = new Dictionary<CHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_Charge_Pay_Detail_Herb>>();
            foreach (var item in herbm.Where(m => m.ChargeStatus == 1))
            { herb.Add(item, herbs.Where(m => m.PrescriptionNo == item.PrescriptionNo)); }



            return new PayedModel
            {
                Pay = pay,
                TreatInfo = treat,
                Customer = cus,
                TreatDoctor = doctor,
                SelectAddress = seladdress,
                ExtraFees = extras,
                HerbPrescriptions = herb,
                FormedPrescriptions = formed
            };
        }


        /// <summary>
        /// 设置现金支付
        /// </summary>
        /// <param name="cashPay"></param>
        /// <returns></returns>
        public async Task<bool> SetCashPay(CashPay cashPay, int opId, string opMan)
        {
            var n = _db.CHIS_Charge_Pay.Where(m => m.PayOrderId == cashPay.PayOrderId).Count();
            if (cashPay.GetCashAmount - cashPay.ReturnCashAmount != cashPay.PayAmount) throw new Exception("支付金额错误！收入找零与支出不对");
            if (n == 0)
            {
                await UpdatePayedAsync(cashPay.PayOrderId, FeeTypes.Cash, $"收:{cashPay.GetCashAmount};零:{cashPay.ReturnCashAmount}", true, opId, opMan);
                await _logger.WriteInfoAsync("CHARGE", "ChargePaymentSuccess", $"更新现金支付{cashPay.PayOrderId}");
            }
            return true;
        }

        /// <summary>
        /// 载入需要收费的信息
        /// </summary>
        /// <param name="treatId"></param>
        /// <returns></returns>
        public async Task<NeedPayModel> GetNeedPayInfoAsync(long treatId)
        {
            var db = _db;
            var treat = db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId && m.TreatStatus == 2);
            if (treat == null) throw new Exception("没有发现接诊信息");
            var cus = db.vwCHIS_Code_Customer.AsNoTracking().FirstOrDefault(m => m.CustomerID == treat.CustomerId);
            if (cus == null) throw new Exception("没有发现用户信息");
            var doctor = db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == treat.DoctorId);
            if (doctor == null) throw new Exception("没有发现医生信息");
            var defaddrss = _dispensingSvr.GetUserDefaultMailAddress(cus.CustomerID);//默认地址

            var seladdress = db.vwCHIS_Code_Customer_AddressInfos.FirstOrDefault(m => m.AddressId == treat.TransferAddressId);//记录的邮寄地址

            if (seladdress == null || !seladdress.IsLegalAddress)
            {
                //设置一个默认地址
                seladdress = defaddrss;
                db.CHIS_DoctorTreat.Find(treat.TreatId).TransferAddressId = defaddrss?.AddressId;
                await db.SaveChangesAsync();
            }



            //成药
            var formedm = db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            var formeds = db.vwCHIS_DoctorAdvice_Formed_Detail.Where(m => m.TreatId == treatId).ToList();
            var formed = new Dictionary<CHIS_DoctorAdvice_Formed, IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail>>();
            foreach (var item in formedm.Where(m => m.ChargeStatus == 0))
            {
                if (item.ChargeStatus == 0)
                {
                    formed.Add(item, formeds.Where(m => m.PrescriptionNo == item.PrescriptionNo));
                }
            }


            //中草药
            var herbm = db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId).ToList();
            var herbmIds = herbm.Select(m => m.PrescriptionNo);
            var herbs = db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => herbmIds.Contains(m.PrescriptionNo)).ToList();
            var herb = new Dictionary<CHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>>();
            foreach (var item in herbm.Where(m => m.ChargeStatus == 0))
            {
                if (item.ChargeStatus == 0)
                {
                    herb.Add(item, herbs.Where(m => m.PrescriptionNo == item.PrescriptionNo));
                }
            }


            //附加费中邮费的重新整理                   
            IEnumerable<vwCHIS_Doctor_ExtraFee> extrafees = await AdjustTransFeeAsync(treatId, formeds, herbs, defaddrss);
            treat = db.vwCHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId && m.TreatStatus == 2);

            return new NeedPayModel
            {
                TreatInfo = treat,
                Customer = cus,
                TreatDoctor = doctor,
                SelectAddress = seladdress,
                FormedPrescriptions = formed,
                HerbPrescriptions = herb,
                ExtraFees = extrafees
            };

        }


        /// <summary>
        /// 重新调整快递费
        /// </summary>
        private async Task<IEnumerable<vwCHIS_Doctor_ExtraFee>> AdjustTransFeeAsync(long treatId, List<vwCHIS_DoctorAdvice_Formed_Detail> formeds, List<vwCHIS_DoctorAdvice_Herbs_Detail> herbs, vwCHIS_Code_Customer_AddressInfos defaddress)
        {
            //计算剑客的快递费
            await CalcJKTransFeeAsync(treatId, formeds, herbs, defaddress);
            //重新计算总价摘要
            CalcPaySumary(treatId);
            return _db.vwCHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 0);

        }

        //计算总价摘要
        public void CalcPaySumary(long treatId)
        {
            _db.FeeSumary.FromSql($"exec sp_DoctorAdvice_Sumary {treatId}").FirstOrDefaultAsync();
        }

        private async Task CalcJKTransFeeAsync(long treatId, List<vwCHIS_DoctorAdvice_Formed_Detail> formeds, List<vwCHIS_DoctorAdvice_Herbs_Detail> herbs, vwCHIS_Code_Customer_AddressInfos defaddress)
        {
            //快递费信息
            var extrafees = _db.CHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treatId && m.TreatFeeTypeId == MPS.Fee_快递).ToList();
            var toAddressId = _db.CHIS_DoctorTreat.Find(treatId)?.TransferAddressId;
            if (toAddressId == null) toAddressId = defaddress?.AddressId;
            if (toAddressId.HasValue)
            {
                //计算是否有剑客的药品
                var bjk = formeds.Any(m => m.SourceFrom == (int)DrugSourceFrom.WebNet && m.SupplierId == MPS.SupplierId_JK) ||
                       herbs.Any(m => m.SourceFrom == (int)DrugSourceFrom.WebNet && m.SupplierId == MPS.SupplierId_JK);
                if (!bjk) return;

                //计算总价是否超过58
                int toAreaId; decimal feeOrig;
                var fee = _dispensingSvr.GetTransFee(treatId, toAddressId.Value, MPS.CenterAreaId_JK, out toAreaId, out feeOrig);

                var jkentity = extrafees.FirstOrDefault(m => m.MailForSupplierId == MPS.SupplierId_JK);
                if (extrafees == null || jkentity == null)
                {
                    await _db.CHIS_Doctor_ExtraFee.AddAsync(new CHIS_Doctor_ExtraFee
                    {
                        TreatFeeTypeId = MPS.Fee_快递,
                        Qty = 1,
                        FeeRemark = "健客快递费",
                        TreatFeeOriginalPrice = feeOrig,
                        TreatFeePrice = fee,
                        Amount = fee,
                        IsConfirmed = true,
                        TreatId = treatId,
                        MailForSupplierId = MPS.SupplierId_JK,
                        MailFromAreaId = MPS.CenterAreaId_JK,
                        MailToAreaId = toAreaId,
                        MailAddressInfoId = toAddressId,
                        ChargeStatus = 0
                    });
                }
                else
                {
                    if (jkentity.ChargeStatus == 0)
                    {
                        jkentity.TreatFeeOriginalPrice = feeOrig;
                        jkentity.TreatFeePrice = fee;
                        jkentity.Amount = fee;
                        jkentity.IsConfirmed = true;
                        jkentity.MailFromAreaId = MPS.CenterAreaId_JK;
                        jkentity.MailToAreaId = toAreaId;
                        jkentity.MailAddressInfoId = toAddressId;
                        _db.Update(jkentity);
                    }
                }
                await _db.SaveChangesAsync();
            }

        }


        /// <summary>
        /// 缴费首页信息
        /// </summary>
        /// <returns></returns>
        public PayIndexModel GetPayIndexInfo()
        {
            return new PayIndexModel();
        }

        #endregion




        public bool CheckIsPayed(string payOrderId, bool bThrowException = true)
        {
            if (_db.CHIS_Charge_Pay.Where(m => m.PayOrderId == payOrderId).Count() > 0)
            {
                if (bThrowException) throw new SuccessedException("已经历史支付成功！");
                return true;
            }
            else return false;
        }


        public CHIS_Charge_PayPre FindPayPre(long payPreId, bool bContext = false)
        {
            if (bContext) return _db.CHIS_Charge_PayPre.Find(payPreId);
            return _db.CHIS_Charge_PayPre.AsNoTracking().Single(m => m.Id == payPreId);
        }
        public CHIS_Charge_PayPre FindPayPre(string payOrderId)
        {
            return _db.CHIS_Charge_PayPre.AsNoTracking().Single(m => m.PayOrderId == payOrderId);
        }
        public PayPreInfo FindPayPreInfo(string payOrderId)
        {
            var prepay = _db.CHIS_Charge_PayPre.AsNoTracking().Single(m => m.PayOrderId == payOrderId);
            var treat = _db.vwCHIS_DoctorTreat.AsNoTracking().Single(m => m.TreatId == prepay.treatId);
            return new PayPreInfo
            {
                CHIS_Charge_PayPre = prepay,
                CustomerId = treat.CustomerId,
                CustomerName = treat.CustomerName
            };
        }


        #region 支付过程操作
        /// <summary>
        /// 发起并生成一个预支付的清表
        /// </summary>
        /// <param name="feeTypeCode">支付类型，请使用FeeTypes选择 </param>
        /// <returns>返回预支付的总情况</returns>
        public async Task<CHIS_Charge_PayPre> CreatePayPreAsync(long treatId, string payRemark, int opId, string opMan, bool bForceNew = false)
        {
            var payOrderId = "";
            var db = _db;
            if (treatId == 0) throw new Exception("没有接诊号");
            bool bNeedNew = false;
            //根据TreatID,StationID等条件，生成一个加密Code
            var treat = db.CHIS_DoctorTreat.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId);
            var formed = db.CHIS_DoctorAdvice_Formed.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 0).ToList(); //成药
            var herb = db.CHIS_DoctorAdvice_Herbs.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 0).ToList();//中药
            var extra = db.CHIS_Doctor_ExtraFee.AsNoTracking().Where(m => m.TreatId == treatId && m.ChargeStatus == 0).ToList();//附加费

            decimal totalAmount = formed.Sum(m => m.Amount) + herb.Sum(m => m.Amount) + extra.Sum(m => m.Amount);//总金额
            if (totalAmount != treat.NeedPayAmount) throw new Exception("总金额与需要支付金额核对不一致");

            var formedIds = formed.Select(m => m.PrescriptionNo).ToList();
            var herbIds = herb.Select(m => m.PrescriptionNo).ToList();
            var extraIds = extra.Select(m => m.ExtraFeeId).ToList();
            string encodestring = _MD5Encode(treatId, treat.StationId,
                                             formedIds, herbIds, extraIds,
                                             Ass.P.PInt(totalAmount * 100));
            int stationId = treat.StationId;
            //查找是否已经记录了该部分的数据，如果找到，则返回
            CHIS_Charge_PayPre rtn = null;
            var find = await db.CHIS_Charge_PayPre.AsNoTracking().Where(m => m.treatId == treatId && m.StationId == treat.StationId
               && m.CheckMD5Code == encodestring && m.TotalAmount > 0).FirstOrDefaultAsync();
            if (find != null && bForceNew) { db.Remove(find); await db.SaveChangesAsync(); find = null; }
            System.Threading.Thread.Sleep(10);
            if (find == null) bNeedNew = true;
            else if (find.PayStatus == 1) throw new SuccessedException();
            else if (find.PayStatus > 1)
            {
                if (bForceNew) //强制删除必须是有错误的时候
                {
                    db.CHIS_Charge_PayPre.Remove(find);
                    await db.SaveChangesAsync();
                }
                throw new Exception("该订单支付错误！" + find.PayErrorMsg);
            }
            else
            {
                var payed = db.CHIS_Charge_Pay.AsNoTracking().FirstOrDefault(m => m.TreatId == treatId && m.StationId == treat.StationId && m.PayOrderId == find.PayOrderId);
                if (payed != null)
                {
                    find.PayStatus = 1;
                    find.PayRemark += "[追加付款成功]";
                    find.PayedSuccessTime = payed.PayedTime;
                    db.Update(find);
                    await db.SaveChangesAsync();
                    throw new SuccessedException();
                }
                if (find.PayRemark == payRemark) return find;
                else
                {
                    find.PayRemark = payRemark;
                    db.Update(find);
                    await db.SaveChangesAsync();
                    return find;
                }
            }

            bool isNotAllowedCashPay = false;
            if (bNeedNew)
            {
                db.BeginTransaction();
                try
                {
                    //生成一个临时付款单号
                    DateTime now = DateTime.Now;
                    rtn = (await db.CHIS_Charge_PayPre.AddAsync(new CHIS_Charge_PayPre
                    {
                        CreateTime = now,
                        StationId = treat.StationId,
                        treatId = treatId,
                        PayRemark = payRemark,
                        PayStatus = 0,
                        PayOrderId = string.Format("{0:yyyyMMddHHmm}-{1}-{2}-{3:000}", now, stationId, treat.DoctorId, Rand.Next(1, 999))
                    })).Entity;
                    await db.SaveChangesAsync();
                    payOrderId = rtn.PayOrderId;
                    //查找具体项目，并写入到预支付表
                    decimal checkTotalAmount = 0m;
                    //-------------- 成药 ------------------------               }
                    var formedItems = db.CHIS_DoctorAdvice_Formed_Detail.AsNoTracking().Where(m => formedIds.Contains(m.PrescriptionNo.Value));
                    foreach (var m in formedItems)
                    {
                        checkTotalAmount += m.Amount;
                        await db.CHIS_Charge_PayPre_Detail_Formed.AddAsync(new CHIS_Charge_PayPre_Detail_Formed
                        {

                            TreatId = m.TreatId,
                            PayPreID = rtn.Id,
                            Amount = m.Amount,
                            PrescriptionNo = m.PrescriptionNo.Value,
                            Price = m.Price,
                            Quantity = m.Qty,
                            UnitID = m.UnitId,
                            DoctorAdviceId = m.AdviceFormedId
                        });
                    }


                    //---------------- 中药 ------------------------------
                    var herbItems = db.vwCHIS_DoctorAdvice_Herbs_Detail.AsNoTracking().Where(m => herbIds.Contains(m.PrescriptionNo)).ToList();
                    foreach (var item in herb)
                    {
                        var num = item.Qty;
                        foreach (var m in herbItems.Where(m => m.PrescriptionNo == item.PrescriptionNo))
                        {
                            checkTotalAmount += (m.Amount) * num; //包含付数
                            await db.CHIS_Charge_PayPre_Detail_Herb.AddAsync(new CHIS_Charge_PayPre_Detail_Herb
                            {

                                TreatId = m.TreatId,
                                PayPreID = rtn.Id,
                                Amount = m.Amount * num,//包含付数
                                PrescriptionNo = m.PrescriptionNo,
                                Price = m.Price,
                                Quantity = (int)m.Qty * num,//包含付数
                                UnitID = m.UnitId,
                                DoctorAdviceId = m.Id
                            });
                        }
                    }
                    var herbdrugids = herbItems.Select(m => m.CnHerbId).ToList();
                    var formeddrugids = formedItems.Select(m => m.DrugId).ToList();
                    formeddrugids.AddRange(herbdrugids);
                    var formeddrugs = db.CHIS_Code_Drug_Main.AsNoTracking().Where(m => formeddrugids.Contains(m.DrugId));
                    isNotAllowedCashPay = isNotAllowedCashPay || formeddrugs.Any(m => m.SourceFrom != (int)DrugSourceFrom.Local);

                    // ----------------- 附加费 --------------------------------
                    var items = db.CHIS_Doctor_ExtraFee.AsNoTracking().Where(m => extraIds.Contains(m.ExtraFeeId));
                    foreach (var m in items)
                    {
                        checkTotalAmount += m.Amount;
                        await db.CHIS_Charge_PayPre_Detail_ExtraFee.AddAsync(new CHIS_Charge_PayPre_Detail_ExtraFee
                        {
                            PayPreId = rtn.Id,
                            ExtraFeeAmount = m.Amount,
                            ExtraFeeTypeId = m.TreatFeeTypeId,
                            DoctorExtraFeeId = m.ExtraFeeId
                        });
                    }
                    await db.SaveChangesAsync();
                    //重新设置总信息
                    rtn.PayOrderId = payOrderId;//订单号
                    rtn.TotalAmount = totalAmount;//总金额
                    rtn.CheckMD5Code = encodestring;//编码
                    rtn.IsAllowedCashPay = !isNotAllowedCashPay;//是否允许现金支付
                    await db.SaveChangesAsync();

                    //采用舍值法，舍去最后金额
                    var checkTotalVal = (int)(checkTotalAmount * 100);
                    var totalAmountVal = (int)(totalAmount * 100);
                    if (checkTotalVal != totalAmountVal)
                    { throw new Exception($"总金额核对错误！[核对值{checkTotalVal}]"); }
                    db.CommitTran();
                }
                catch (Exception ex)
                {
                    db.RollbackTran();
                    if (ex is SuccessedException)
                    {
                        await this.UpdatePayedAsync(rtn.PayOrderId, FeeTypes.Cash, "追加成功", true, opId, opMan);
                    }
                    rtn = null; throw ex;
                }
            }
            return rtn;
        }

        /// <summary>
        /// 更新预支付数据
        /// </summary>
        /// <param name="payOrderId"></param>
        /// <param name="payStatus"></param>
        /// <param name="payTime"></param>
        /// <returns></returns>
        private async Task<CHIS_Charge_PayPre> UpdatePayPreAsync(string payOrderId, string feeTypeCode, string payRmk = null, bool isCash = false, int payStatus = 1, DateTime? payTime = null, CHISEntitiesSqlServer db = null)
        {
            db = db ?? _db;
            if (payOrderId.IsEmpty()) throw new Exception("更新付款信息没有传入付款单号。");
            if (!FeeTypes.IsValidFeeType(feeTypeCode)) throw new Exception("传入费用类型错误");
            var finds = db.CHIS_Charge_PayPre.AsNoTracking().Where(u => u.PayOrderId == payOrderId);
            var count = finds.Count();
            if (count == 0) throw new Exception("没有找到预付款信息数据");
            if (count > 1) throw new Exception("发现多个数据");
            if (payTime == null) payTime = DateTime.Now;
            var m = finds.FirstOrDefault();
            if (m == null) throw new Exception($"没有找到单号({payOrderId})的预付款信息。");
            if (isCash) { if (!m.IsAllowedCashPay) throw new Exception("该订单不允许现金支付！"); }
            m.FeeTypeCode = feeTypeCode;
            m.PayStatus = payStatus;
            m.PayedSuccessTime = payTime;
            m.PayRemark = m.PayRemark + payRmk;
            db.Update(m);
            await db.SaveChangesAsync();
            return m;
        }



        //更新到收款表
        private async Task<bool> UpdatePayAsync(CHIS_Charge_PayPre payPre, int opId, string opMan)
        {
            if (payPre.PayStatus == 0) throw new Exception("该笔支付订单没有支付成功，请确认。");
            var db = _db;
            db.BeginTransaction();
            try
            {
                var formed = db.CHIS_Charge_PayPre_Detail_Formed.AsNoTracking().Where(m => m.PayPreID == payPre.Id).ToList();//成药
                var herb = db.CHIS_Charge_PayPre_Detail_Herb.AsNoTracking().Where(m => m.PayPreID == payPre.Id).ToList();//中药
                var extras = db.CHIS_Charge_PayPre_Detail_ExtraFee.AsNoTracking().Where(m => m.PayPreId == payPre.Id).ToList();//附加费

                var find = db.CHIS_Charge_Pay.AsNoTracking().Where(m => m.PayOrderId == payPre.PayOrderId);
                if (find.Count() > 0) throw new PayException(PayExceptionType.PayedFinishAndSuccess, "该支付订单已经存在,已经支付，无需重复。");
                //添加主表
                #region 添加主表

                var debuglog = _env.IsDevelopment() ? "[测试]" : "";

                var pay = (await db.CHIS_Charge_Pay.AddAsync(new CHIS_Charge_Pay
                {
                    TotalAmount = payPre.TotalAmount,
                    TreatId = payPre.treatId,
                    FeeTypeCode = payPre.FeeTypeCode,
                    PayedTime = payPre.PayedSuccessTime ?? DateTime.Now,
                    PayOrderId = payPre.PayOrderId,
                    StationId = payPre.StationId,
                    PayRemark = payPre.PayRemark + debuglog,
                    sysOpId = opId,
                    sysOpMan = opMan,
                    sysOpTime = DateTime.Now
                })).Entity;
                await db.SaveChangesAsync();
                #endregion


                //添加从表
                #region 添加一般从表
                //------------- 成药 ------------------------------
                foreach (var mm in formed)
                {
                    await db.CHIS_Charge_Pay_Detail_Formed.AddAsync(new CHIS_Charge_Pay_Detail_Formed
                    {
                        Amount = mm.Amount,
                        PrescriptionNo = mm.PrescriptionNo,
                        TreatId = mm.TreatId,
                        UnitId = mm.UnitID,
                        Quantity = mm.Quantity,
                        Price = mm.Price,
                        PayId = pay.PayId,
                        DoctorAdviceId = mm.DoctorAdviceId
                    });

                    //更新医嘱缴费状态
                    var ma = db.CHIS_DoctorAdvice_Formed.FirstOrDefault(m => m.PrescriptionNo == mm.PrescriptionNo);
                    if (ma == null) throw new Exception("没有发现医嘱信息");
                    ma.ChargeStatus = (short)ChargeStatus.Payed;//设置为已经缴费

                }

                // ------------ 中药 -------------------------------
                foreach (var mm in herb)
                {
                    await db.CHIS_Charge_Pay_Detail_Herb.AddAsync(new CHIS_Charge_Pay_Detail_Herb
                    {
                        Amount = mm.Amount,
                        PrescriptionNo = mm.PrescriptionNo,
                        TreatId = mm.TreatId,
                        UnitID = mm.UnitID,
                        Quantity = mm.Quantity,
                        Price = mm.Price,
                        PayId = pay.PayId,
                        PayOrderId = pay.PayOrderId,
                        DoctorAdviceId = mm.DoctorAdviceId
                    });
                    //更新医嘱缴费状态
                    var ma = db.CHIS_DoctorAdvice_Herbs.FirstOrDefault(m => m.PrescriptionNo == mm.PrescriptionNo);
                    if (ma == null) throw new Exception("没有发现医嘱信息");
                    ma.ChargeStatus = (short)ChargeStatus.Payed;//设置为已经缴费             
                }

                #endregion

                #region 添加附加费从表
                foreach (var mm in extras)
                {
                    await db.CHIS_Charge_Pay_Detail_ExtraFee.AddAsync(new CHIS_Charge_Pay_Detail_ExtraFee
                    {
                        DoctorExtraFeeId = mm.DoctorExtraFeeId,
                        ExtraFeeAmount = mm.ExtraFeeAmount,
                        ExtraFeeTypeId = mm.ExtraFeeTypeId,
                        PayId = pay.PayId
                    });
                }
                await db.SaveChangesAsync();
                #endregion

                var treat = _treatSvr.FindTreat(pay.TreatId);
                treat.NeedPayAmount = 0;
                await db.SaveChangesAsync();
                db.CommitTran();

                //操作成功则赠送积分信息
                if (payPre.FeeTypeCode == FeeTypes.AliPay_QR ||
                    payPre.FeeTypeCode == FeeTypes.WeChat_H5 ||
                    payPre.FeeTypeCode == FeeTypes.WeChat_Pub ||
                    payPre.FeeTypeCode == FeeTypes.WeChat_QR)
                {
                    _pointsService.ChangePoints(new Models.InputModel.PointsDetailInputModel
                    {
                        CustomerId = treat.CustomerId,
                        Description = FeeTypes.ToName(payPre.FeeTypeCode) + $"支付赠送(消费{pay.TotalAmount})",
                        Points = (int)(payPre.TotalAmount * 100)
                    });
                }

                _logger.WriteSUCCESS($"订单支付更新数据成功:{payPre.PayOrderId}", opId, opMan);
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                _logger.WriteError(ex, opId, opMan);
                throw ex;
            }
            return true;
        }




        /// <summary>
        /// 更新Paypere和更新Pay 并写入日志
        /// </summary>
        /// <param name="payOrderId"></param>
        public async Task UpdatePayedAsync(string payOrderId, string feeTypeCode, string payRmk, bool isCash, int opId, string opMan)
        {
            _db.BeginTransaction();
            try
            {
                if (!FeeTypes.IsValidFeeType(feeTypeCode)) throw new Exception("传入费用类型错误");
                if (_db.CHIS_Charge_Pay.Where(m => m.PayOrderId == payOrderId).Count() > 0) throw new SuccessedException("该订单已经支付过了");
                var paypre = await UpdatePayPreAsync(payOrderId, feeTypeCode, payRmk, isCash, db: _db);
                if (paypre.PayStatus == 1)
                {
                    await UpdatePayAsync(paypre, opId, opMan);
                    await _logger.WriteSUCCESSAsync("ChargePayService", "UpdatePayed", "支付成功:" + payOrderId, opId, opMan, payOrderId);
                }
                _db.CommitTran();
            }
            catch (Exception ex)
            {
                _db.RollbackTran();

                //如果是历史成功了，则直接抛出历史成功错误
                if (ex is SuccessedException) throw ex;
                //否则记录错误
                await _logger.WriteErrorAsync("ChargePayService", "UpdatePayed", ex, opId, opMan);
                throw ex;
            }
        }


        /// <summary>
        /// 设置预支付失败信息
        /// </summary>
        /// <param name="payOrderId"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public async Task SetPayPreFailedAsync(string payOrderId, Exception ex)
        {
            if (ex is SuccessedException) return;
            var find = _db.CHIS_Charge_PayPre.AsNoTracking().FirstOrDefault(m => m.PayOrderId == payOrderId);
            if (find.PayStatus == 1) return;
            find.PayStatus = 2;
            find.PayErrorMsg = ex.Message;
            _db.Update(find);
            await _db.SaveChangesAsync();
        }




        #endregion

        #region 私有方法


        private string _MD5Encode(long treatID, int stationID,
            List<Guid> formeds,
            List<Guid> herbs,
            List<long> extras,
            int amount)
        {
            if (formeds == null) formeds = new List<Guid>(); formeds.Sort();
            if (herbs == null) herbs = new List<Guid>(); herbs.Sort();
            if (extras == null) extras = new List<long>(); extras.Sort();

            string a = string.Format("{0}|{1}|{2}|{3}|{4}|{5}",
                    treatID, stationID,
                    string.Join(",", formeds),
                    string.Join(",", herbs),
                    string.Join(",", extras), amount);
            return Ass.Data.Secret.MD5(a);
        }
        private string _MD5EncodeRefund(long treatID, string feeTypeCode, List<long> adviceIds, List<long> checkIds, List<long> testIds)
        {
            if (adviceIds == null) adviceIds = new List<long>(); adviceIds.Sort();
            if (checkIds == null) checkIds = new List<long>(); checkIds.Sort();
            if (testIds == null) testIds = new List<long>(); testIds.Sort();

            string a = string.Format("{0}|{1}|{2}|{3}|{4}",
                    treatID, feeTypeCode,
                    string.Join(",", adviceIds),
                    string.Join(",", checkIds),
                    string.Join(",", testIds));
            return Ass.Data.Secret.MD5(a);
        }

        #endregion



        #region 生成支付订单

        #region 现金 收款 数据生成

        /// <summary>
        /// 创建现金收单
        /// </summary>
        /// <param name="treatId"></param>
        /// <param name="payRemark"></param>
        /// <param name="isReorder"></param>
        /// <param name="opId"></param>
        /// <param name="opMan"></param>
        /// <returns></returns>
        public async Task<dynamic> CreateCashOrder(long treatId, string payRemark, bool isReorder, int opId, string opMan)
        {
            CHIS_Charge_PayPre prepay = null;

            var cus = _db.vwCHIS_DoctorTreat.FirstOrDefault(m => m.TreatId == treatId);
#if DEBUG
            payRemark += "[测试]";
#endif
            prepay = await CreatePayPreAsync(treatId, payRemark, opId, opMan, isReorder);
            _logger.WriteInfo($"生成现金支付{prepay.PayOrderId}【￥{prepay.TotalAmount}】");
            return new
            {
                rlt = true,
                msg = "",
                payOrderId = prepay.PayOrderId,
                total_fee = prepay.TotalAmount * 100,
                isAllowedCashPay = prepay.IsAllowedCashPay,
                treatId = prepay.treatId,
                payPreId = prepay.Id,
                prepay = prepay
            };
        }






        #endregion

        /// <summary>
        /// 创建支付宝收单
        /// </summary>

        public async Task<dynamic> CreateAli2DCode(long treatId, string payRemark,
            CHIS_Charge_PayPre prepay = null, int opId = 0, string opMan = "")
        {
            bool hasPrepay = prepay != null;
            try
            {
#if DEBUG
                payRemark += "[测试]";
#endif
                var cus = _db.vwCHIS_DoctorTreat.FirstOrDefault(m => m.TreatId == treatId);
                if (!hasPrepay) prepay = await CreatePayPreAsync(treatId, payRemark, opId, opMan);
                if (prepay.PayStatus == 1) throw new Exception("该笔支付已经支付成功了。");
                //根据微信操作类获取微信的二维码        
                var rlt = await _aliPay.Get2DCodeAsync(
                    Guid.NewGuid().ToString("N"),
                    $"支付宝支付-会员[{cus.CustomerId}]{cus.CustomerName}",
                    prepay.PayOrderId,
                    (int)(prepay.TotalAmount * 100),
                    UrlRoot + "/Charge/AliQrPaySuccessCallBack");
                if (rlt.rlt == true)
                {
                    prepay.ali2DCodeUrl = rlt.code_url;//设置微信的二维码                    
                    _db.CHIS_Charge_PayPre.Update(prepay);
                    await _db.SaveChangesAsync();
                }
                _logger.WriteInfo($"发起支付宝二维码支付{prepay.PayOrderId}【￥{prepay.TotalAmount}】");
                return rlt;
            }
            catch (Exception ex)
            {
                if (ex is SuccessedException)
                {
                    await UpdatePayedAsync(prepay.PayOrderId, FeeTypes.Cash, "追加成功", false, opId, opMan);
                    throw ex;//如果是成功则传出
                }
                return new { rlt = false, msg = ex.Message };
            }
        }



        /// <summary>
        /// 创建微信收单
        /// </summary>
        public async Task<dynamic> CreateWX2DCode(long treatId, string payRemark,
            CHIS_Charge_PayPre prepay = null, int opId = 0, string opMan = "")
        {

            bool hasPrepay = prepay != null;
            try
            {
#if DEBUG
                payRemark += "[测试]";
#endif
                var cus = _db.vwCHIS_DoctorTreat.FirstOrDefault(m => m.TreatId == treatId);
                if (!hasPrepay) prepay = await CreatePayPreAsync(treatId, payRemark, opId, opMan);

                if (prepay.PayStatus == 1) throw new Exception("该笔支付已经支付成功了。");
                //根据微信操作类获取微信的二维码  此处会抛错出来      
                var rlt = await new Codes.Utility.XPay.WXPay().Get2DCodeAsync(
                    Guid.NewGuid().ToString("N"),
                    $"微信支付-会员[{cus.CustomerId}]{cus.CustomerName}",
                    prepay.PayOrderId,
                    (int)(prepay.TotalAmount * 100),
                    UrlRoot + "/Charge/WXQRPaySuccessCallBack");
                if (rlt.rlt == true)
                {
                    prepay.wx2DCodeUrl = rlt.code_url;//设置微信的二维码
                    if (!hasPrepay)
                    {
                        _db.CHIS_Charge_PayPre.Update(prepay);
                        await _db.SaveChangesAsync();
                    }
                }
                _logger.WriteInfo($"发起微信二维码支付{prepay.PayOrderId}【￥{prepay.TotalAmount}】");
                return rlt;
            }
            catch (Exception ex)
            {
                if (ex is SuccessedException)
                {
                    await UpdatePayedAsync(prepay.PayOrderId, FeeTypes.Cash, "追加成功", false, opId, opMan);
                    throw ex;//如果是成功则传出
                }
                if (ex is PayOrderSameException)
                {
                    _db.CHIS_Charge_PayPre.Remove(prepay);
                    await _db.SaveChangesAsync();
                    return await CreateWX2DCode(treatId, payRemark);//继续创建
                }
                return new { rlt = false, msg = ex.Message };
            }
        }





        #endregion

        #region UrlRoot
        public static string _urlRoot = null;
        internal string UrlRoot
        {
            get
            {
                if (_urlRoot.IsEmpty())
                {
                    var url = _httpContextAccessor.HttpContext.Request.Scheme + "://" +
                             _httpContextAccessor.HttpContext.Request.Host;
                    if (url.ContainsIgnoreCase("localhost"))
                    {
                        url = Global.Config.GetSection("RdSettings:UrlRoot").Value;
                        if (url.ContainsIgnoreCase("localhost"))
                        {
                            var addrs = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
                            var iplocal = addrs.FirstOrDefault(m => !m.IsIPv6LinkLocal).ToString();
                            url = url.Replace("localhost", iplocal);
                        }
                    }
                    _urlRoot = url;
                }
                return _urlRoot;
            }
        }
        #endregion

    }
}

