using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHIS.Models.ViewModel
{

    public class PrescriptionComModel<Tmain, Tdetail>
    {
        /// <summary>
        ///  接诊信息
        /// </summary>
        public vwCHIS_DoctorTreat Treat { get; set; }
        /// <summary>
        /// 主表
        /// </summary>
        public Tmain Main { get; set; }
        /// <summary>
        /// 详情
        /// </summary>
        public IEnumerable<Tdetail> Detail { get; set; }
        /// <summary>
        /// 约号信息
        /// </summary>
        public vwCHIS_Register Regist { get; set; }
        /// <summary>
        /// 处方医生
        /// </summary>
        public string RxDoctorName { get; set; }
        public int? RxDoctorId { get; set; }
        /// <summary>
        /// 处方类别
        /// </summary>
        public string PrescriptionType { get; set; }

        /// <summary>
        /// 存放药品详细信息
        /// </summary>
        public IEnumerable<DrugAttrItem> DrugAttrList { get; set; }
    }

    public class PrintFormedModel : PrescriptionComModel<CHIS_DoctorAdvice_Formed, vwCHIS_DoctorAdvice_Formed_Detail>
    {
        public PrintFormedModel() { PrescriptionType = "FORMED"; }
    }
    public class PrintHerbModel : PrescriptionComModel<vwCHIS_DoctorAdvice_Herbs, vwCHIS_DoctorAdvice_Herbs_Detail>
    {
        public PrintHerbModel() { PrescriptionType = "HERB"; }
    }




    public class PrintSickNoteModel
    {
        public CHIS_Doctor_SickNote SickNote { get; set; }
        public vwCHIS_DoctorTreat Treat { get; set; }

        public IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail> Formed { get; set; }
        public IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail> Herbs { get; set; }



    }
    public class DrugAttrItem
    {
        public int DrugId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierCompanyShortName { get; set; }
        public int DrugSourceFrom { get; set; }
    }
}
