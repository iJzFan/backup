using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ah.Models;

namespace ahWeb.Api
{
    public class BreastApi : BaseDBController
    {
        //问卷分析
        public dynamic QuestionAnalist(Guid qid)
        {
            try
            {
                bool isNeedMgr = CheckNeedBreastMgr(qid);
                var qm = MainDbContext.AHMS_QAFlow_Main.Find(qid);
                qm.QARlt_IsNeedMgr = isNeedMgr;
                MainDbContext.SaveChanges();
                return new { rlt = true, msg = "判定成功", isNeedMgr = isNeedMgr };
            }
            catch (Exception ex) { return new { rlt = false, msg = ex.Message }; }
        }

        //判定是否需要乳腺癌管理
        private bool CheckNeedBreastMgr(Guid qid)
        {
            var qm = MainDbContext.AHMS_QAFlow_Main.FirstOrDefault(m => m.QAFlowMainId == qid);//主表
            var qd = MainDbContext.AHMS_QAFlow_Main_detail.AsNoTracking().Where(m => m.QAFlowMainId == qid);//详细表
            switch (qm.QAVer)
            {
                case "1.0": return BreastQA_1_0(qm, qd);
            }
            throw new Exception("没有进入预定判定程序");
        }

        //是否需要管理
        private bool BreastQA_1_0(AHMS_QAFlow_Main qm, IQueryable<AHMS_QAFlow_Main_detail> qd)
        {
            Dictionary<string, string> qv = new Dictionary<string, string>();
            foreach (var item in qd) qv.Add(item.QAItemNameId, item.QAItemValue);

            //确定性判定 包含1项则判定管理
            if (_bn(qv["RXC_Q2"]) != false) return true; // 现病史有乳腺问题
            if (_bn(qv["RXC_Q3"]) != false) return true; // 既往史有乳腺问题
            if (_bn(qv["RXC_Q72"]) == true) return true;
            if (_bn(qv["RXC_Q83"]) == true) return true;
            if (_bn(qv["RXC_Q51"]) == true) return true;
            if (_bn(qv["RXC_Q52"]) != false) return true;
            if (_bn(qv["RXC_Q53"]) != false) return true;
         
            //组合性判定
            Dictionary<string, bool> rlt = new Dictionary<string, bool>();
            if (((DateTime.Now - qm.Birthday.Value).TotalDays / 365.20f) >= 40) rlt.Add("Age", true);
            if (qv["RXC_Q41"].Trim() == "0-12岁") rlt.Add("RXC_Q41", true);
            if (qv["RXC_Q42"].Trim() == "55岁及以上") rlt.Add("RXC_Q42", true);
            if (qv["RXC_Q43"].Trim() == "30岁及以上") rlt.Add("RXC_Q43", true);

            if (_bn(qv["RXC_Q61"]) == true) rlt.Add("RXC_Q61", true);
            if (_bn(qv["RXC_Q62"]) == true) rlt.Add("RXC_Q62", true);
            if (_bn(qv["RXC_Q81"]) == true) rlt.Add("RXC_Q81", true);
            if (_bn(qv["RXC_Q82"]) == true) rlt.Add("RXC_Q82", true);
            if (_bn(qv["RXC_Q84"]) == true) rlt.Add("RXC_Q84", true);
            if (_hv(qv["RXC_Q2"], "感染HPV")) rlt.Add("RXC_Q2_hpv", true);
            if (_hv(qv["RXC_Q2"], "EBV")) rlt.Add("RXC_Q2_ebv", true);
            if (_hv(qv["RXC_Q3"], "感染HPV")) rlt.Add("RXC_Q3_hpv", true);
            if (_hv(qv["RXC_Q3"], "EBV")) rlt.Add("RXC_Q3_ebv", true);

            if (rlt.Count > 3) return true;//超过三项，则判定为需要管理

            return false;
        }




        /// <summary>
        /// 所在答案列表中是否已val开头的答案（某些答案包含多余文字)
        /// </summary> 
        private bool _hv(string obj,string val)
        {
            string[] vv = string.Format("{0}", obj).Split('|');
            foreach(var v in vv)
            {
                if (v.StartsWith(val)) return true;
            }
            return false;
        }

        /// <summary>
        /// 问卷二值判定
        /// </summary>
        private bool? _bn(object obj)
        {
            string a = string.Format("{0}", obj);
            if (a == "有" || a == "是") return true;
            if (a == "无" || a == "否") return false;
            return null;
        }
    }
}
