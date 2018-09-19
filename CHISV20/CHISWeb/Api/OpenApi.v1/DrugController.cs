using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ass;
using CHIS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Api.OpenApi.v1
{

    /// <summary>
    /// 药品处理业务
    /// </summary>
    public class DrugController : OpenApiBaseController
    {
        Services.JKWebNetService _jkSvr;
        Services.DictService _dictSvr;
        public DrugController(DbContext.CHISEntitiesSqlServer db
             , Services.JKWebNetService jkSvr
            , Services.DictService dictSvr
            ) : base(db)
        {
            _jkSvr = jkSvr;
            _dictSvr = dictSvr;
        }

        //=========================================================================================



        /// <summary>
        /// 更新药品信息
        /// </summary>
        [HttpGet]
        public dynamic UpdateDrugInfo(int drugid)
        {
            try
            {
                var drug = _db.CHIS_Code_Drug_Main.AsNoTracking().FirstOrDefault(m => m.DrugId == drugid);
                bool jkupdate = false;
                bool timechk = (drug.ThreePartDrugRefreshTime == null || (DateTime.Now - drug.ThreePartDrugRefreshTime.Value).TotalDays > 180);
                if (drug.DrugCode.StartsWith("AHJK") && timechk) jkupdate = true;
                if (drug.SourceFrom == (int)DrugSourceFrom.WebNet && drug.SupplierId == MPS.SupplierId_JK) jkupdate = true;
                if (jkupdate)
                {
                    var threePartDrugId = 0;
                    if (drug.SupplierId == MPS.SupplierId_JK) threePartDrugId = drug.ThreePartDrugId.Value;
                    else threePartDrugId = Ass.P.PIntV(drug.DrugCode.Replace("AHJK", ""));

                    var dd = _jkSvr.QueryDrugInfo(threePartDrugId);
                    if (drug.DrugPicUrl != Global.ConfigSettings.JKImageRoot + dd.thumbnailUrl)
                    {
                        drug.DrugPicUrl = Global.ConfigSettings.JKImageRoot + dd.thumbnailUrl;
                        //todo: 抓取大图和缩略图更新到本地
                    }

                    if (dd.prescriptionType == "2") drug.DrugRxType = "OTC_R";
                    if (dd.prescriptionType == "3") drug.DrugRxType = "OTC_G";
                    if (dd.prescriptionType == "4" || dd.prescriptionType == "5") drug.DrugRxType = "RX";
                    if (dd.prescriptionType == "9") drug.MedialMainKindCode = MPS.MedicalMainKindCode.MT;
                    if (dd.introduction.IsNotEmpty()) drug.UseRemark = dd.introduction;
                    drug.ThreePartDrugRefreshTime = DateTime.Now;
                    _db.Update(drug);
                    _db.SaveChanges();
                }
                var d = MyDynamicResult(true, "");
                d.drugPicUrl = drug.DrugPicUrl.ahDtUtil().GetDrugImg(drug.MedialMainKindCode); 
                return d;
            }
            catch (Exception ex) { return MyDynamicResult(ex); }
        }



        /// <summary>
        /// 获取药品信息
        /// </summary>
        /// <param name="searchText">搜索内容空格分割[品名 商标/厂商]</param>
        /// <param name="sourceFrom">0 - 本地；1 - 网络；2 - 三方</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容</param>
        /// <returns></returns>
        [HttpGet]
        public dynamic GetDrugsBasic(string searchText, int? sourceFrom = 0, int? pageIndex = 1, int? pageSize = 20)
        {
            var ss = Ass.P.PStr(searchText).ToLower().Split(' ');
            searchText = ss[0];
            var searchText1 = (ss.Length > 1 ? ss[1] : "").Trim();//厂商

            var finds = _db.CHIS_Code_Drug_Main.AsNoTracking().Where(m => m.SourceFrom == sourceFrom && m.IsEnable);

            long drugId = 0;
            if (long.TryParse(searchText, out drugId))
            {
                if (searchText.Length > 10) finds = finds.Where(m => m.BarCode == searchText);
                else
                {
                    if (searchText == "999") finds.Where(m => m.CodeDock.Contains(searchText) || m.DrugId == drugId);
                    finds = finds.Where(m => m.DrugId == drugId);
                }
            }
            else
            {
                finds = finds.Where(m => m.CodeDock.Contains(searchText));
            }

            var findrlt = finds.Join(_db.CHIS_Code_Drug_Outpatient, a => a.DrugId, g => g.DrugId, (a, g) => new
            {
                DrugId = a.DrugId,
                DrugName = a.DrugName,
                DrugModel = a.DrugModel,
                DrugImgUrl = a.DrugPicUrl.ahDtUtil().GetDrugImg(a.MedialMainKindCode,true),
                ManufacturerOrigin = a.ManufacturerOrigin,
                a.OriginPlace,
                g.UnitBigId,
                g.UnitSmallId,
                g.DosageUnitId,
                g.OutpatientConvertRate,
                g.DosageContent,
                UnitBigName = _dictSvr.GetDictById(g.UnitBigId).ItemName,
                UnitSmallName = _dictSvr.GetDictById(g.UnitSmallId).ItemName,
                DosageUnitName = _dictSvr.GetDictById(g.DosageUnitId).ItemName,
                a.ManufacturerOriginPinYin
            });
            if (searchText1.IsNotEmpty())
            {
                findrlt = findrlt.Where(m => m.ManufacturerOriginPinYin.Contains(searchText1));
            }
            return findrlt.OrderBy(m => m.DrugId).Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

    }
}
