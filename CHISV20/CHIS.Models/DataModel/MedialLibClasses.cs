using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.DataModel
{
    public class MyMedicalHistoryRecordsModel
    {
        public vwCHIS_Code_Customer Customer { get; set; }
        public Ass.Mvc.PageListInfo<MyMedicalHistoryRecordItemModel> List1st { get; set; }

    }
    public class MyMedicalHistoryRecordDetailModel
    {
        public vwCHIS_DoctorTreat Treat { get; set; }
        /// <summary>
        /// 特殊接诊
        /// </summary>
        public ViewModels.SpecialTreat SpecialTreat { get; set; }
        public Dictionary<CHIS_DoctorAdvice_Formed, IEnumerable<vwCHIS_DoctorAdvice_Formed_Detail>> FormDic { get; set; }
        public Dictionary<vwCHIS_DoctorAdvice_Herbs, IEnumerable<vwCHIS_DoctorAdvice_Herbs_Detail>> HerbDic { get; set; }


    }
    public class MyMedicalHistoryRecordItemModel
    {
        public long TreatId { get; set; }
      public DateTime FirstTreatTime { get; set; }
      public DateTime  TreatTime { get; set; }
       public int DoctorId { get; set; }
       public string DoctorName { get; set; }
       public string Diagnosis1Name { get; set; }
       public string Diagnosis2Name { get; set; }
    }
}
