using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CHIS.Models.DataModel
{
    public class MyConfig
    {

        public MyConfig(List<CHIS_Sys_MyConfig> svData, IEnumerable<DoctorSEntityV02> rxDoctors)
        {
            DefDrugTakeShow = getVal(svData, "DefDrugTakeShow", "EnCh");
            SelectDrugStores = getVal(svData, "SelectDrugStores", "Local").Split(",");
            DefDrugUnit = getVal(svData, "DefDrugUnit", "SMALL");
            DefIncludeStation = getVal(svData, "DefIncludeStation", "False") == "True";
            DefAutoSendDrugs= getVal(svData, "DefAutoSendDrugs", "False") == "True";
            MyRxDoctors = rxDoctors;
        }


        /// <summary>
        /// 默认开药用药信息显示
        /// </summary>
        [DisplayName("药品默认用法显示")]
        public string DefDrugTakeShow { get; set; } = "EnCh";

        /// <summary>
        /// 药房来源的显示
        /// </summary>
        [DisplayName("显示药房")]
        public string[] SelectDrugStores { get; set; } = "Local".Split(",");

        /// <summary>
        /// 默认出药单位
        /// </summary>
        [DisplayName("默认出药单位")]
        public string DefDrugUnit { get; set; } = "SMALL";


        /// <summary>
        /// 收费发药包含整个工作站
        /// </summary>
        [DisplayName("收费发药包含整个工作站")]
        public bool DefIncludeStation { get; set; } = false;

        /// <summary>
        /// 收费后自动发药
        /// </summary>
        [DisplayName("收费后自动发药")]
        public bool DefAutoSendDrugs { get; set; } = false;


        /// <summary>
        /// 我的处方医生
        /// </summary>
        public IEnumerable<DoctorSEntityV02> MyRxDoctors { get; set; }

        /// <summary>
        /// 默认的处方医生
        /// </summary>
        public DoctorSEntityV02 MyDefRxDoctor
        {
            get
            {
                return MyRxDoctors.FirstOrDefault(m => m.IsRxDefault);
            }
        }


        #region tool functions

        private string getVal(List<CHIS_Sys_MyConfig> svData, string secKey, string defVal = "")
        {
            var fd = svData.FirstOrDefault(m => m.SectionKey == secKey);
            if (fd == null) return defVal;
            else return fd.SectionValue;
        }
        /// <summary>
        /// 是否选中该Checkbox
        /// </summary>
        /// <param name="values"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public string CheckBoxChecked(IEnumerable<string> values, string val)
        {
            return values.Contains(val) ? "checked" : "";
        }
        /// <summary>
        /// 是否选中该Radio
        /// </summary>
        /// <param name="values"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public string RadioChecked(string basevalue, string val)
        {
            return basevalue == val ? "checked" : "";
        }

        #endregion
    }



}
