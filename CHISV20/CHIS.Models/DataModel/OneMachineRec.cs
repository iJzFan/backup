using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.DataModels
{
    public class OneMachineRec
    {
        public long OneMachineRecId { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public int Sex { get; set; }
        public Int16 Age { get; set; }
        public string IDcard { get; set; }
        public string Telephone { get; set; }    
        public DateTime MeasureTime { get; set; }
    }

    public class OneMachineInputData
    {
        public long Id { get; set; }
        public string MachineId { get; set; }
        public DateTime? MeasureTime { get; set; }
        public string MemName { get; set; }
        public string MemMobile { get; set; }
        public string MemIDCode { get; set; }
        public string UnitNo { get; set; }
        public string UnitName { get; set; }
        public string MacAddr { get; set; }
        public string RecordNo { get; set; }
        public string LoginType { get; set; }
        public string DeviceType { get; set; }
        public string MemGender { get; set; }
        public decimal? MemAge { get; set; }
        public DateTime? MemBirthday { get; set; }
        public string MemNation { get; set; }
        public DateTime? MemIdStartDate { get; set; }
        public DateTime? MemIdEndDate { get; set; }
        public string MemIdDepart { get; set; }
        public string MemBarCode { get; set; }
        public string MemICCode { get; set; }
        public string MemSocialCode { get; set; }
        public string MemUserId { get; set; }
        public string TreatStatus { get; set; }





        static Dictionary<string, object> _tr = null;

        private static void InitTestResults()
        {
            AddTestResultItems("height", new Dictionary<string, string>() {
                { "-1", "消瘦"},
                { "0", "正常"},
                { "1", "超重"},
                { "2", "肥胖"}}
            );
            AddTestResultItems("Fat", new Dictionary<string, string>() {
                { "1", "稍瘦"},
                { "2", "标准"},
                { "3", "超重"},
                { "4", "肥胖"}}
           );
            AddTestResultItems("MinFat", new Dictionary<string, string>() {
                { "-1", "消瘦"},
                { "0", "正常"},
                { "1", "超重"},
                { "2", "肥胖"}}
            );
            AddTestResultItems("MinFat", new Dictionary<string, string>() {
                { "1", "偏低"},
                { "2", "标准"},
                { "3", "偏高"},
                { "4", "高"}}
            , "Physique");
            AddTestResultItems("MinFat", new Dictionary<string, string>() {
                { "1", "消瘦"},
                { "2", "标准"},
                { "3", "隐藏性肥胖"},
                { "4", "肌肉型肥胖/健壮"},
                { "5", "肥胖"}}
           , "Shape");

            AddTestResultItems("BloodPressure", new Dictionary<string, string>() {
                { "-1", "低血压"},
                { "0", "正常血压"},
                { "1", "正常高血压"},
                { "2", "轻度高血压"},
                { "3", "中度高血压"},
                { "4","重度高血压"} }
            );
            AddTestResultItems("BO", new Dictionary<string, string>() {
                { "-1", "低血氧"},
                { "0", "正常血氧"}  }
            );
            AddTestResultItems("Temperature", new Dictionary<string, string>() {
                { "-1", "低温"},
                { "0", "正常"},
                { "1", "高温" } }
           );
            AddTestResultItems("Whr", new Dictionary<string, string>() {
                { "0", "正常"},
                { "1", "上身肥胖"},
                { "2", "下身肥胖" } }
           );
            //血糖
            AddTestResultItems("BloodSugar", new Dictionary<string, string>() {
                { "-1", "低血糖"},
                { "0", "正常"},
                { "1", "偏高"},
                { "2", "高" } }
            );
            AddTestResultItems("BloodSugar", new Dictionary<string, string>() {
                { "1", "餐前血糖"},
                { "2", "餐后血糖"},
                { "3", "随机血糖" } }, "BloodsugarType"
           );
            //血尿酸
            AddTestResultItems("Ua", new Dictionary<string, string>() {
                { "-1", "低"},
                { "0", "正常"},
                { "1", "高" } }
            );

            //总胆固醇
            AddTestResultItems("Chol", new Dictionary<string, string>() {
                { "-1", "低"},
                { "0", "正常"},
                { "1", "高" }, { "2", "过高" } }
            );
            //血脂
            AddTestResultItems("BloodFat", new Dictionary<string, string>() {
                { "-1", "低"},
                { "0", "正常"},
                { "1", "高" } }
            );
            //血红蛋白
            AddTestResultItems("Hb", new Dictionary<string, string>() {
                { "-1", "血红蛋白偏低"},
                { "0", "正常"},
                { "1", "血红蛋白偏高"}  }
            );
            //酒精浓度
            AddTestResultItems("Alcohol", new Dictionary<string, string>() {
                { "0", "正常"},
                { "1", "饮酒"},
                { "2", "醉酒" } }
            );
            //心电图分析
            AddTestResultItems("Ecg", new Dictionary<string, string>() {
{"5025","节律无异常                            "},
{"5026","疑似心跳稍快，请注意休息              "},
{"5027","疑似心跳过快，请注意休息              "},
{"5028","疑似阵发性心跳过快                    "},
{"5029","疑似心跳稍缓，请注意休息              "},
{"5030","疑似心跳过缓，请注意休息              "},
{"5031","疑似心跳间期缩短                      "},
{"5032","疑似心跳间期不规则                    "},
{"5033","疑似心跳稍快伴有心跳间期缩短          "},
{"5034","疑似心跳稍缓伴有心跳间期缩短          "},
{"5035","疑似心跳稍缓伴有心跳间期不规则        "},
{"5036","波形有漂移                            "},
{"5037","疑似心跳过快伴有波形漂移              "},
{"5038","疑似心跳过缓伴有波形漂移              "},
{"5039","疑似心跳间期缩短伴有波形漂移          "},
{"5040","疑似心跳间期不规则伴有波形漂移        "},
{"5041","信号较差，请重新测量                  "}}
            , "Analysis");






        }
        private static void AddTestResultItems(string secName, Dictionary<string, string> pms, string key = "Result")
        {
            foreach (var item in pms) AddTestResultItem(secName, key, item.Key, item.Value);
        }

        private static void AddTestResultItem(string secName, string key, string val, object valrmk)
        {
            try
            {
                if (_tr == null) _tr = new Dictionary<string, object>();
                string seckey = $"{secName}_{key}_{val}";
                _tr.Add(seckey.ToLower(), valrmk);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                throw ex;
            }
        }

        /// <summary>
        /// 获取结论的描述
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string GetResultString(string sectionName, object valobj, string key = "Result")
        {
            try
            {
                if (_tr == null) InitTestResults();
                string valstr = string.Format("{0}", valobj).Trim();
                if (valstr == "") return "";
                string seckey = $"{sectionName}_{key}_{valobj}".ToLower();
                string val = Ass.P.PStr(valobj);
                return Ass.P.PStr(_tr[seckey]);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }



    }


}
