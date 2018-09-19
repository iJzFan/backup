using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ah.Models;
using ah.Models.ViewModel;

namespace ahWeb.Api
{

    /// <summary>
    /// 有关病历的api
    /// </summary>
    public class Prescription : BaseDBController
    {

        //获取接诊病历信息
        //public DoctorPrintsViewModel GetPrescriptionByTreatId(long treatId)
        //{
        //    return GetPrintsModelByTreatId(treatId, null, null, null);
        //}

        //public DoctorPrintsViewModel GetPrintsModelByTreatId(long treatId, IEnumerable<long> adviceIds, IEnumerable<long> checkIds, IEnumerable<long> testIds)
        //{


        //    var doctorTreat = MainDbContext.vwCHIS_DoctorTreat.Where(m => m.TreatId == treatId).AsNoTracking().FirstOrDefault();
        //    var costomer = MainDbContext.vwCHIS_Code_Customer.Where(m => m.CustomerID == doctorTreat.CustomerId).AsNoTracking().FirstOrDefault();
        //    var employee = MainDbContext.vwCHIS_Code_Doctor.Where(m => m.DoctorId == doctorTreat.DoctorId).AsNoTracking().FirstOrDefault();
        //    var registor = MainDbContext.vwCHIS_Register.Where(m => m.RegisterID == doctorTreat.RegisterID).AsNoTracking().FirstOrDefault();

        //    bool ball = (adviceIds == null || adviceIds.Count() == 0) &&
        //                (checkIds == null || checkIds.Count() == 0) &&
        //                (testIds == null || testIds.Count() == 0);
        //    // =========================== 检查项目 ==========================================
        //    var checkItems = MainDbContext.vwCHIS_Doctor_CheckItem.Where(m => m.TreatID == doctorTreat.TreatId && m.PrescriptionNo != null).AsNoTracking();
        //    if (!ball) checkItems = checkItems.Where(m => checkIds.Contains(m.CheckID));
        //    List<Print_CheckItems> print_checkItems = new List<Print_CheckItems>();
        //    foreach (var checkitem in checkItems)
        //    {
        //        var finds = print_checkItems.FirstOrDefault(m => m.PrescriptionNumber == checkitem.PrescriptionNo);
        //        if (finds == null)
        //        {
        //            var addmodel = new Print_CheckItems() { PrescriptionNumber = checkitem.PrescriptionNo };
        //            addmodel.CheckItems.Add(checkitem);
        //            print_checkItems.Add(addmodel);
        //        }
        //        else
        //        {
        //            finds.CheckItems.Add(checkitem);
        //        }
        //    }
        //    // =========================== 检验项目 ==========================================
        //    var testItems = MainDbContext.vwCHIS_Doctor_TestItem.Where(m => m.TreatID == doctorTreat.TreatId && m.PrescriptionNo != null).AsNoTracking();
        //    if (!ball) testItems = testItems.Where(m => testIds.Contains(m.TestID));
        //    List<Print_TestItems> print_testItems = new List<Print_TestItems>();
        //    foreach (var testitem in testItems)
        //    {
        //        var finds = print_testItems.FirstOrDefault(m => m.PrescriptionNumber == testitem.PrescriptionNo);
        //        if (finds == null)
        //        {
        //            var addmodel = new Print_TestItems() { PrescriptionNumber = testitem.PrescriptionNo };
        //            addmodel.TestItems.Add(testitem);
        //            print_testItems.Add(addmodel);
        //        }
        //        else
        //        {
        //            finds.TestItems.Add(testitem);
        //        }
        //    }


        //    // =========================== 中成药项目 ==========================================
        //    var zyc_Advices = MainDbContext.vwCHIS_Doctor_AdviceItem.Where(m => m.TreatID == doctorTreat.TreatId && m.MedialMainKindCode == "ZYC" && m.PrescriptionNo != null).AsNoTracking();
        //    if (!ball) zyc_Advices = zyc_Advices.Where(m => adviceIds.Contains(m.AdviceID));
        //    List<Print_ZYC_Advices> print_zycAdvices = new List<Print_ZYC_Advices>();
        //    foreach (var zycitem in zyc_Advices)
        //    {
        //        var finds = print_zycAdvices.FirstOrDefault(m => m.PrescriptionNumber == zycitem.PrescriptionNo);
        //        if (finds == null)
        //        {
        //            var addmodel = new Print_ZYC_Advices() { PrescriptionNumber = zycitem.PrescriptionNo };
        //            addmodel.ZYC_Advices.Add(zycitem);
        //            print_zycAdvices.Add(addmodel);
        //        }
        //        else
        //        {
        //            finds.ZYC_Advices.Add(zycitem);
        //        }
        //    }



        //    // =========================== 西药项目 ==========================================
        //    var xy_Advices = MainDbContext.vwCHIS_Doctor_AdviceItem.Where(m => m.TreatID == doctorTreat.TreatId && m.MedialMainKindCode == "XY" && m.PrescriptionNo != null).AsNoTracking();
        //    if (!ball) xy_Advices = xy_Advices.Where(m => adviceIds.Contains(m.AdviceID));
        //    List<Print_XY_Advices> print_xyAdvices = new List<Print_XY_Advices>();
        //    foreach (var xyitem in xy_Advices)
        //    {
        //        var finds = print_xyAdvices.FirstOrDefault(m => m.PrescriptionNumber == xyitem.PrescriptionNo);
        //        if (finds == null)
        //        {
        //            var addmodel = new Print_XY_Advices() { PrescriptionNumber = xyitem.PrescriptionNo };
        //            addmodel.XY_Advices.Add(xyitem);
        //            print_xyAdvices.Add(addmodel);
        //        }
        //        else
        //        {
        //            finds.XY_Advices.Add(xyitem);
        //        }
        //    }


        //    // =========================== 综合项目 ==========================================
        //    var zh_Advices = MainDbContext.vwCHIS_Doctor_AdviceItem.Where(m => m.TreatID == doctorTreat.TreatId && m.MedialMainKindCode == "ZHL" && m.PrescriptionNo != null).AsNoTracking();
        //    if (!ball) zh_Advices = zh_Advices.Where(m => adviceIds.Contains(m.AdviceID));
        //    List<Print_ZH_Advices> print_zhAdvices = new List<Print_ZH_Advices>();
        //    foreach (var zhitem in zh_Advices)
        //    {
        //        var finds = print_zhAdvices.FirstOrDefault(m => m.PrescriptionNumber == zhitem.PrescriptionNo);
        //        if (finds == null)
        //        {
        //            var addmodel = new Print_ZH_Advices() { PrescriptionNumber = zhitem.PrescriptionNo };
        //            addmodel.ZH_Advices.Add(zhitem);
        //            print_zhAdvices.Add(addmodel);
        //        }
        //        else
        //        {
        //            finds.ZH_Advices.Add(zhitem);
        //        }
        //    }


        //    // =========================== 中药药方项目 ========================================== 
        //    var zyf_mains = MainDbContext.CHIS_DoctorAdvice.Where(m => m.TreatID == doctorTreat.TreatId && m.MedialMainKindCode == "ZYF" && m.PrescriptionNo != null).AsNoTracking();
        //    if (!ball) zyf_mains = zyf_mains.Where(m => adviceIds.Contains(m.AdviceID));
        //    List<Print_CnHerbs> zyf_advices = new List<Print_CnHerbs>();
        //    foreach (var zyf in zyf_mains)
        //    {
        //        var herbsDetals = MainDbContext.CHIS_DoctorAdvice_ZH_Detail.Where(m => m.ParentAdviceID == zyf.AdviceID).AsNoTracking().ToList();
        //        zyf_advices.Add(new Print_CnHerbs
        //        {
        //            MainAdvice = zyf,
        //            HerbsDetails = herbsDetals
        //        });
        //    }


        //    var model = new ah.Models.ViewModel.DoctorPrintsViewModel
        //    {
        //        DoctorTreat = doctorTreat,
        //        Customer = costomer,
        //        Doctor = employee,
        //        Registor = registor,
        //        Print_CheckItems = print_checkItems,
        //        Print_TestItems = print_testItems,
        //        Print_ZYC_Advices = print_zycAdvices,
        //        Print_XY_Advices = print_xyAdvices,
        //        Print_ZH_Advices = print_zhAdvices,
        //        Print_CnHerbs = zyf_advices
        //    };

        //    return model;
        //}



    }
}