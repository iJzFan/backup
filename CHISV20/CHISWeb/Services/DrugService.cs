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
using System.Text;

namespace CHIS.Services
{
    public class DrugService : BaseService
    {
        public DrugService(CHISEntitiesSqlServer db) : base(db)
        {

        }

        /// <summary>
        /// 获取我的药房诊所的所有药品
        /// </summary>
        /// <param name="stationId">诊所Id</param>
        public IQueryable<DrugSelectItem> GetAllDrugsOfClinic(int stationId)
        {
            var mydrugs = _db.vwCHIS_DrugStock_Monitor.Where(m => m.StationId == stationId && m.DrugStockNum > 0 && m.StockDrugIsEnable && m.MedialMainKindCode != MPS.MedicalMainKindCode.ZYM)
                .OrderBy(m => m.QPCode)
                .Select(m => new DrugSelectItem
                {
                    DefDrugImg = m.DrugPicUrl.ahDtUtil().GetDrugImg(m.MedialMainKindCode, true),
                    StockFromId = m.DrugStockMonitorId,
                    DrugId = m.DrugId,
                    DrugName = m.DrugName,
                    DrugModel = m.DrugModel,
                    DrugPinYin = m.QPCode,
                    PyCode = m.PyCode,
                    Alias = m.Alias,
                    DrugOrigMf = $"{m.OriginPlace}{m.ManufacturerOrigin}",
                    StockUnitName = m.StockUnitName,
                    CanUseUnitNames = m.StockUnitName,//可使用的单位
                    StockSalePrice = m.StockSalePrice,
                    MedKindCode = m.MedialMainKindCode
                });
            return mydrugs;
        }

        /// <summary>
        /// 获取药品
        /// </summary>
        /// <param name="drugs"></param>
        /// <returns></returns>
        public IQueryable<CHIS_Code_Drug_Main> GetDrugs(IList<int> drugs)
        {
            return _db.CHIS_Code_Drug_Main.AsNoTracking().Where(m => drugs.Contains(m.DrugId));
        }

        /// <summary>
        /// 获取药品
        /// </summary>
        /// <param name="drugs"></param>
        /// <returns></returns>
        public IQueryable<vwCHIS_DrugStock_Monitor> GetStockDrugsInfo(IList<DrugStockIndexItem> drugFromIds)
        {
            var a = drugFromIds.Select(m => m.StockFromId);
            return _db.vwCHIS_DrugStock_Monitor.Where(m => a.Contains(m.DrugStockMonitorId) && m.StockDrugIsEnable);

        }





        /// <summary>
        /// 获取一个空白新Model
        /// </summary>
        /// <param name="parentid">父级Id</param>
        public Models.ViewModels.Code_DrugViewModel GetNewModel(int opId, string opMan)
        {

            var modelNew = new Models.ViewModels.Code_DrugViewModel()
            {
                CHIS_Code_Drug_Main = new CHIS_Code_Drug_Main(),
                CHIS_Code_Drug_Storage = new CHIS_Code_Drug_Storage(),
                CHIS_Code_Drug_Outpatient = new CHIS_Code_Drug_Outpatient(),
                DrugApply = new CHIS_Code_Drug_Main_Apply()
            };

            modelNew.CHIS_Code_Drug_Main.IsEnable = true;
            modelNew.CHIS_Code_Drug_Main.IsSkinTest = false;
            modelNew.CHIS_Code_Drug_Main.sysOpId = opId;
            modelNew.CHIS_Code_Drug_Main.sysOpMan = opMan;
            modelNew.CHIS_Code_Drug_Main.sysOpTime = DateTime.Now;
            return modelNew;
        }


        public void SaveNewModel(Models.ViewModels.Code_DrugViewModel model, Models.CHIS_Code_Drug_Main_Apply applyModel, int opId, string opMan)
        {

            _db.BeginTransaction();

            try
            {
                model.CHIS_Code_Drug_Main.sysOpId = opId;
                model.CHIS_Code_Drug_Main.sysOpMan = opMan;
                model.CHIS_Code_Drug_Main.sysOpTime = DateTime.Now;
                if (applyModel != null)
                {
                    model.CHIS_Code_Drug_Main.SourceFrom = (int)DrugSourceFrom.Local;
                    model.CHIS_Code_Drug_Main.SupplierId = 0;
                    model.CHIS_Code_Drug_Main.ThreePartDrugId = null;
                    model.CHIS_Code_Drug_Main.IsEnable = false;
                }
                if (string.IsNullOrWhiteSpace(model.CHIS_Code_Drug_Main.PyCode)) model.CHIS_Code_Drug_Main.PyCode = Ass.Utils.GetPinYinCode(model.CHIS_Code_Drug_Main.DrugName);
                if (string.IsNullOrWhiteSpace(model.CHIS_Code_Drug_Main.WBCode)) model.CHIS_Code_Drug_Main.WBCode = Ass.Utils.Get5BiCode(model.CHIS_Code_Drug_Main.DrugName);
                if (string.IsNullOrWhiteSpace(model.CHIS_Code_Drug_Main.QPCode)) model.CHIS_Code_Drug_Main.QPCode = Ass.Utils.GetQuanPinCode(model.CHIS_Code_Drug_Main.DrugName);
                if (model.CHIS_Code_Drug_Main.ManufacturerOrigin.IsNotEmpty()) model.CHIS_Code_Drug_Main.ManufacturerOriginPinYin = getManufacturerOriginPinYin(model.CHIS_Code_Drug_Main.ManufacturerOrigin, model.CHIS_Code_Drug_Main.Trademark);

                model.CHIS_Code_Drug_Main.DrugCompleteScore = getDrugCompleteScore(model);

                var rlt0 = _db.Add<Models.CHIS_Code_Drug_Main>(model.CHIS_Code_Drug_Main).Entity;
                _db.SaveChanges();
                model.CHIS_Code_Drug_Storage.DrugId = rlt0.DrugId;
                _db.Add<Models.CHIS_Code_Drug_Storage>(model.CHIS_Code_Drug_Storage);
                model.CHIS_Code_Drug_Outpatient.DrugId = rlt0.DrugId;
                _db.Add<Models.CHIS_Code_Drug_Outpatient>(model.CHIS_Code_Drug_Outpatient);
                _db.SaveChanges();

                //如果是申请
                if (applyModel != null)
                {
                    applyModel.DrugId = rlt0.DrugId;
                    _db.CHIS_Code_Drug_Main_Apply.Add(applyModel);
                    _db.SaveChanges();
                }

                _db.CommitTran();
            }
            catch (Exception ex) { _db.RollbackTran(); throw ex; }

        }

        private int getDrugCompleteScore(Code_DrugViewModel model)
        {
            //完善度评分
            var m = model.CHIS_Code_Drug_Main;
            var a = model.CHIS_Code_Drug_Outpatient;
            var rlt = 0;
            if (a.UnitBigId != a.UnitSmallId && a.DosageUnitId == a.UnitSmallId) rlt += 20;
            if (a.UnitBigId != a.UnitSmallId && a.UnitSmallId != a.DosageUnitId) rlt += 40;
            rlt += m.BarCode.IsNotEmpty() ? 20 : 0;
            rlt += m.ApprovalCode.IsNotEmpty() ? 6 : 0;

            rlt += m.DrugModel.IsNotEmpty() ? 5 : 0;
            rlt += m.OriginPlace.IsNotEmpty() || m.ManufacturerOrigin.IsNotEmpty() ? 5 : 0;
            rlt += m.DrugPicUrl.IsNotEmpty() ? 5 : 0;
            rlt += m.DrugRxType.IsNotEmpty() ? 5 : 0;

            rlt += m.Indicate.IsNotEmpty() ? 2 : 0;
            rlt += m.UseRemark.IsNotEmpty() ? 2 : 0;
            rlt += m.Notice.IsNotEmpty() ? 2 : 0;
            rlt += m.MainIngredients.IsNotEmpty() ? 2 : 0;
            rlt += m.Taboo.IsNotEmpty() ? 2 : 0;
            rlt += m.UntowardEffect.IsNotEmpty() ? 2 : 0;

            rlt += m.DefDrugGivenTakeTypeId > 0 ? 1 : 0;
            rlt += m.DefDrugGivenTimeTypeId > 0 ? 1 : 0;
            rlt += m.DefDrugGivenWhereTypeId > 0 ? 1 : 0;
            rlt += a.DosageContent > 0 ? 1 : 0;

            rlt += m.FormTypeId > 0 ? 1 : 0;
            rlt += m.MediTypeId > 0 ? 1 : 0;

            rlt += m.ValidDays > 0 ? 1 : 0;

            return rlt > 100 ? 100 : rlt;
        }

        public Models.ViewModels.Code_DrugViewModel ModifyInitialModel(string recid)
        {
            if (!int.TryParse(recid, out int drugid)) throw new Exception("不是正确的编号Id格式");
            var drugmain = _db.CHIS_Code_Drug_Main.AsNoTracking().FirstOrDefault(m => m.DrugId == drugid);
            if (drugmain == null) throw new ChkException(ExceptionCodes.DrugNotFound, "没有找到该药品数据");

            var modelmodify = new CHIS.Models.ViewModels.Code_DrugViewModel()
            {
                CHIS_Code_Drug_Main = drugmain,
                CHIS_Code_Drug_Storage = _db.CHIS_Code_Drug_Storage.AsNoTracking().FirstOrDefault(m => m.DrugId == drugid) ?? new CHIS_Code_Drug_Storage(),
                CHIS_Code_Drug_Outpatient = _db.CHIS_Code_Drug_Outpatient.AsNoTracking().FirstOrDefault(m => m.DrugId == drugid),
                DrugApply = _db.CHIS_Code_Drug_Main_Apply.AsNoTracking().FirstOrDefault(m => m.DrugId == drugid)
            };
            if (modelmodify.CHIS_Code_Drug_Main == null) throw new Exception("没有找到该数据");
            return modelmodify;
        }



        public void SaveModifyModel(Models.ViewModels.Code_DrugViewModel model, int opId, string opMan)
        {

            //采用事务提交
            _db.BeginTransaction();

            try
            {
                model.CHIS_Code_Drug_Main.sysOpId = opId;
                model.CHIS_Code_Drug_Main.sysOpMan = opMan;
                model.CHIS_Code_Drug_Main.sysOpTime = DateTime.Now;

                if (model.CHIS_Code_Drug_Main.PyCode.IsEmpty()) model.CHIS_Code_Drug_Main.PyCode = Ass.Utils.GetPinYinCode(model.CHIS_Code_Drug_Main.DrugName);
                if (model.CHIS_Code_Drug_Main.WBCode.IsEmpty()) model.CHIS_Code_Drug_Main.WBCode = Ass.Utils.Get5BiCode(model.CHIS_Code_Drug_Main.DrugName);
                if (model.CHIS_Code_Drug_Main.QPCode.IsEmpty()) model.CHIS_Code_Drug_Main.QPCode = Ass.Utils.GetQuanPinCode(model.CHIS_Code_Drug_Main.DrugName);
                if (model.CHIS_Code_Drug_Main.ManufacturerOrigin.IsNotEmpty()) model.CHIS_Code_Drug_Main.ManufacturerOriginPinYin = getManufacturerOriginPinYin(model.CHIS_Code_Drug_Main.ManufacturerOrigin, model.CHIS_Code_Drug_Main.Trademark);


                model.CHIS_Code_Drug_Main.DrugCompleteScore = getDrugCompleteScore(model);

                var rlt0 = _db.Update<Models.CHIS_Code_Drug_Main>(model.CHIS_Code_Drug_Main).Entity;
                _db.SaveChanges();

                model.CHIS_Code_Drug_Storage.DrugId = rlt0.DrugId;
                if (_db.CHIS_Code_Drug_Storage.AsNoTracking().FirstOrDefault(m => m.DrugId == rlt0.DrugId) == null)
                    _db.Add(model.CHIS_Code_Drug_Storage);
                else _db.Update<Models.CHIS_Code_Drug_Storage>(model.CHIS_Code_Drug_Storage);

                model.CHIS_Code_Drug_Outpatient.DrugId = model.CHIS_Code_Drug_Main.DrugId;
                if (_db.CHIS_Code_Drug_Outpatient.AsNoTracking().FirstOrDefault(m => m.DrugId == rlt0.DrugId) == null)
                    _db.Add(model.CHIS_Code_Drug_Outpatient);
                else _db.Update<Models.CHIS_Code_Drug_Outpatient>(model.CHIS_Code_Drug_Outpatient);

                _db.SaveChanges();
                _db.CommitTran();
            }
            catch (Exception ex)
            {
                _db.RollbackTran();
                if (ex.InnerException != null) throw ex.InnerException;
                throw ex;
            }

        }

        private string getManufacturerOriginPinYin(string manufacturerOrigin, string treadmark)
        {
            return string.Format("{0}|{1}|{2}|{3}",
                Ass.Data.Chinese2Spell.GetFstLettersLower(treadmark),
                Ass.Data.Chinese2Spell.ConvertLower(treadmark),
                Ass.Data.Chinese2Spell.GetFstLettersLower(manufacturerOrigin),
                Ass.Data.Chinese2Spell.ConvertLower(manufacturerOrigin)
                );
        }



        /// <summary>
        /// 药品一般编辑调整
        /// </summary>
        /// <param name="drugOutpatient"></param>
        /// <returns></returns>
        public CHIS_Code_Drug_Main DrugNormalEdit(vwCHIS_Code_Drug_Main drugOutpatient)
        {
            var main = _db.CHIS_Code_Drug_Main.Find(drugOutpatient.DrugId);
            var model = _db.CHIS_Code_Drug_Outpatient.FirstOrDefault(m => m.DrugId == drugOutpatient.DrugId);

            main.Trademark = drugOutpatient.Trademark;
            main.ManufacturerOrigin = drugOutpatient.ManufacturerOrigin;
            main.OriginPlace = drugOutpatient.OriginPlace;
            //别名
            if (main.Alias != drugOutpatient.Alias)
            {
                main.Alias = drugOutpatient.Alias;
                main.AliasSCode = main.AliasPyCode = Ass.Data.Chinese2Spell.GetFstLettersLower(drugOutpatient.Alias);
                main.AliasQPCode = Ass.Data.Chinese2Spell.ConvertLower(drugOutpatient.Alias);
            }

            main.DefDrugGivenTakeTypeId = drugOutpatient.DefDrugGivenTakeTypeId;
            main.DefDrugGivenTimeTypeId = drugOutpatient.DefDrugGivenTimeTypeId;
            main.DefDrugGivenWhereTypeId = drugOutpatient.DefDrugGivenWhereTypeId;
            //获取供应商拼音
            main.ManufacturerOriginPinYin = getManufacturerOriginPinYin(main.ManufacturerOrigin, main.Trademark);



            bool bmod = (model.UnitBigId != drugOutpatient.UnitSmallId) ||
                (model.UnitSmallId != drugOutpatient.UnitSmallId) ||
                (model.OutpatientConvertRate != drugOutpatient.OutpatientConvertRate) ||
                (model.DosageContent != drugOutpatient.DosageContent);

            model.UnitSmallId = drugOutpatient.UnitSmallId;
            model.UnitBigId = drugOutpatient.UnitBigId;
            model.DosageUnitId = drugOutpatient.DosageUnitId;
            model.DosageContent = drugOutpatient.DosageContent;
            model.OutpatientConvertRate = drugOutpatient.OutpatientConvertRate.Value;
            model.DefDosage = drugOutpatient.DefDosage;

            _db.SaveChanges();

            //删除库存
            if (bmod)
            {
                var finds = _db.CHIS_DrugStock_Monitor.Where(m => m.DrugId == drugOutpatient.DrugId);
                _db.CHIS_DrugStock_Monitor.RemoveRange(finds);
                _db.SaveChanges();
            }
            return main;
        }





        public async Task DeleteAsync(string recids)
        {

            _db.BeginTransaction();
            try
            {
                var ids = recids.ToList<int>();
                if (ids.Count == 0) throw new Exception("传入的删除Id格式错误,或者没有传入删除的Id。");
                var finds0 = _db.CHIS_Code_Drug_Storage.Where(m => ids.Contains(m.DrugId));
                var finds1 = _db.CHIS_Code_Drug_Outpatient.Where(m => ids.Contains(m.DrugId));
                var finds2 = _db.CHIS_Code_Drug_Main.Where(m => ids.Contains(m.DrugId));
                var finds3 = _db.CHIS_Code_Drug_Main_Apply.Where(m => ids.Contains(m.DrugId.Value));

                _db.RemoveRange(finds0);
                _db.RemoveRange(finds1);
                _db.RemoveRange(finds2);
                _db.RemoveRange(finds3);

                await _db.SaveChangesAsync();
                _db.CommitTran();
            }
            catch (Exception ex) { _db.RollbackTran(); throw ex; }

        }

        /// <summary>
        /// 查找药品
        /// </summary>
        /// <param name="drugId"></param>
        /// <returns></returns>
        public CHIS_Code_Drug_Main Find(int drugId)
        {
            return _db.CHIS_Code_Drug_Main.Find(drugId);
        }

        public vwCHIS_Code_Drug_Main FindView(int drugId)
        {
            return _db.vwCHIS_Code_Drug_Main.Find(drugId);
        }

        /// <summary>
        /// 获取药品供应商
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        public CHIS_Code_Supplier GetDrugSupplier(int supplierId)
        {
            return _db.CHIS_Code_Supplier.Find(supplierId);
        }
        /// <summary>
        /// 获取药品供应商
        /// </summary>
        /// <param name="supplierIds"></param>
        /// <returns></returns>
        public IQueryable<CHIS_Code_Supplier> GetDrugSuppliers(IEnumerable<int> supplierIds)
        {
            return _db.CHIS_Code_Supplier.AsNoTracking().Where(m => supplierIds.Contains(m.SupplierID));
        }


        #region 开药品
        public IQueryable<Models.vwCHIS_DrugStock_Monitor> QueryStockDrugInfos(string term,int drugStoreStationId, ref IEnumerable<string> drugFrom, string drugType = "")
        {
            term = Ass.P.PStr(term).ToLower();          
            //首先筛选本工作站门诊房 或者 剑客网上药品
            if (drugFrom == null) drugFrom = new List<string> { "0-0" };//默认本地
            List<int?> f0 = new List<int?>(), f1 = new List<int?>(), f2 = new List<int?>();
            foreach (var item in drugFrom)
            {
                try
                {
                    var ss = item.Split('-');
                    var a = int.Parse(ss[0]); var bb = int.Parse(ss[1]);
                    if (a == 0) f0.Add(bb);
                    if (a == 1) f1.Add(bb);
                    if (a == 2) f2.Add(bb);
                }
                catch { }
            }

            //组织sql语句实现快速搜索
            StringBuilder b = new StringBuilder(), ba = new StringBuilder();
            b.AppendFormat("select * from vwCHIS_DrugStock_Monitor where (StationId={0} {1}) and StockDrugIsEnable=1", drugStoreStationId, f1.Count() > 0 ? " or StationId=-1" : "");
            if (drugType.IsNotEmpty()) b.AppendFormat(" and MedialMainKindCode='{0}'", drugType);

            //搜索范围
            if (f0.Count > 0) ba.AppendFormat("or (SourceFrom={0} and SupplierId in({1}))", 0, string.Join(",", f0));
            if (f1.Count > 0) ba.AppendFormat("or (SourceFrom={0} and SupplierId in({1}))", 1, string.Join(",", f1));
            if (f2.Count > 0) ba.AppendFormat("or (SourceFrom={0} and SupplierId in({1}))", 2, string.Join(",", f2));
            if (ba.Length > 10) b.AppendFormat(" and ({0})", ba.Remove(0, 2));

            //搜索数量大于零
            b.Append(" and DrugStockNum>0 ");

            //筛选内容

            long drugId = 0;
            if (long.TryParse(term, out drugId))
            {
                if (term.Length > 8)
                {
                    b.AppendFormat(" and BarCode={0}", term);
                }
                else
                {
                    var b0 = b.ToString() + $" and DrugId={drugId}";
                    var b1 = b.AppendFormat(" and charindex('{0}',codedock)>0 ", term).ToString();
                    b = new StringBuilder();
                    b.Append(b0); b.Append(" union "); b.Append(b1);
                }
            }
            else
            {
                b.AppendFormat(" and charindex('{0}',codedock)>0 ", term);
            }

            return _db.vwCHIS_DrugStock_Monitor.FromSql(b.ToString());
        }
        public Models.vwCHIS_DrugStock_Monitor QueryStorckDrug(int drugId)
        {        
            return _db.vwCHIS_DrugStock_Monitor.FirstOrDefault(m => m.DrugId == drugId);
        }




        #endregion






    }
}
