using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.ViewModels
{
    public class PatientDetailViewModel
    {

        public Models.vwCHIS_Code_Customer CHIS_Code_Customer { get; set; }

        public Models.CHIS_Register CHIS_Register { get; set; }

        public vwCHIS_Code_Department RegistDepartment { get; set; }

        public Models.vwCHIS_DoctorTreat DoctorTreat { get; set; }

 


        public Models.CHIS_Code_Customer_HealthInfo CustomerHealthInfo { get; set; }

        public IEnumerable<Models.vwCHIS_Doctor_ExtraFee> TreatExtraFees { get; set; }

        public CHIS_Doctor_SickNote SickNote { get; set; }

        /// <summary>
        /// 中药处方的集合
        /// </summary>
        public IEnumerable<CnHerbsMainViewModel> HerbList { get; set; }

        /// <summary>
        /// 成药处方的集合
        /// </summary>
        public IEnumerable<FormedMainViewModel> FormedList { get; set; }

        /// <summary>
        /// 费用摘要
        /// </summary>
        public FeeSumaryViewModel FeeSumary { get; set; }


        /// <summary>
        /// 特殊接诊数据
        /// </summary>
        public SpecialTreat SpecialTreat { get; set; }
    }

    /// <summary>
    /// 特殊接诊数据
    /// </summary>
    public class SpecialTreat
    {
        /// <summary>
        /// 特殊科室的值信息 是标志性字段
        /// </summary>
        public string SpetialDepartTypeVal { get; set; }

        /// <summary>
        /// 接诊扩展
        /// </summary>
        public CHIS_Doctor_TreatExt DoctorTreatExt { get; set; }
    }

    /// <summary>
    /// 特殊接诊 心理学
    /// </summary>
    public class SpecialTreat_Psych : SpecialTreat
    {        
        /// <summary>
        /// 问卷的数据
        /// </summary>
        public vwCHIS_Data_PsychPretreatQs QsData { get; set; }
    }

}
