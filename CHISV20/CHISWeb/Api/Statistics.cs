using CHIS.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Api
{
    public class Statistics : BaseDBController
    {
        public  Statistics(DbContext.CHISEntitiesSqlServer db) : base(db) { }
        /// <summary>
        /// 获取医生的基本接诊统计信息
        /// sp_Statistics_DoctorTreat_BasicSummary
        /// </summary>
        /// <param name="stationId">工作站 -1 表示所有工作站</param>
        /// <param name="doctorId">医生 -1 表示所有医生</param>
        /// <param name="hisDaysAgo">历史追索天数</param>
        /// <returns></returns>
        public IActionResult Statistics_DoctorTreat_BasicSummary(int stationId, int doctorId, int hisDaysAgo = 60)
        {
            return TryCatchFunc((dd) =>
            {
                if (stationId == 0 || stationId < -1) throw new Exception("错误的工作站Id");
                if (doctorId == 0 || doctorId < -1) throw new Exception("错误的医生Id");

                var finds = _db.SqlQuery<Models.StatisticsModels.TreatBasicSummary>(string.Format("exec sp_Statistics_DoctorTreat_BasicSummary {0},{1},{2}", stationId, doctorId, hisDaysAgo));
                dd.item = finds;
                dd.NeedTreat = string.Format("<span><s>{0}</s><i>{1}</i></span>", finds.First(m => m.name == "ThisToday").waiting, finds.First(m => m.name == "ThatToday").waiting);
                return null;
            });
        }

        public IActionResult Statistics_DoctorTreat_BasicSummaryPv(int stationId, int doctorId, int hisDaysAgo = 60)
        {
            if (stationId == 0 || stationId < -1) throw new Exception("错误的工作站Id");
            if (doctorId == 0 || doctorId < -1) throw new Exception("错误的医生Id");
            var finds = _db.SqlQuery<Models.StatisticsModels.TreatBasicSummary>(string.Format("exec sp_Statistics_DoctorTreat_BasicSummary {0},{1},{2}", stationId, doctorId, hisDaysAgo));
            return ApiPartialView("_pvDoctorTreatBasicSummary", finds);
        }
        public IActionResult Statistics_DoctorTreat_BasicSummaryOfMePv(int hisDaysAgo = 60)
        {
            var finds = _db.SqlQuery<Models.StatisticsModels.TreatBasicSummary>(string.Format("exec sp_Statistics_DoctorTreat_BasicSummary {0},{1},{2}", UserSelf.StationId, UserSelf.DoctorId, hisDaysAgo)).ToList();
 
            foreach(var item in finds)
            {
                if (item.name == "ThisToday") item.seq = 0;
                if (item.name == "ThisPastDay") item.seq = 1;
                if (item.name == "ThisTomorow") item.seq = 2;
                if (item.name == "ThisFuture") item.seq = 3;
                if (item.name == "ThatToday") item.seq = 4;
                if (item.name == "ThatPastDay") item.seq = 5;
                if (item.name == "ThatTomorow") item.seq = 6;
                if (item.name == "ThatFuture") item.seq = 7;
            }           
            return ApiPartialView("_pvDoctorTreatBasicSummary", finds.OrderBy(m => m.seq));
        }


        /// <summary>
        /// 需要接诊 Html格式返回数据
        /// </summary> 
        public IActionResult Statistics_DoctorTreat_BasicSummary_NeedTreatHtml(int stationId, int doctorId, int hisDaysAgo = 60)
        {
            try
            {
                if (stationId == 0 || stationId < -1) throw new Exception("错误的工作站Id");
                if (doctorId == 0 || doctorId < -1) throw new Exception("错误的医生Id");
                var finds = _db.SqlQuery<Models.StatisticsModels.TreatBasicSummary>(string.Format("exec sp_Statistics_DoctorTreat_BasicSummary {0},{1},{2}", stationId, doctorId, hisDaysAgo));
                return Content(string.Format("<span><s>{0}</s><i>{1}</i></span>",
                                    finds.First(m => m.name == "ThisToday").waiting,
                                    finds.First(m => m.name == "ThatToday").waiting),
                              "text/html");
            }
            catch { return Content(""); }
        }
        /// <summary>
        /// 需要自己的待接诊数量 Html格式返回数据
        /// </summary> 
        public IActionResult Statistics_DoctorTreat_BasicSummary_NeedTreatHtmlOfMe(int hisDaysAgo = 60)
        {
            try
            {
                var finds = _db.SqlQuery<Models.StatisticsModels.TreatBasicSummary>(string.Format("exec sp_Statistics_DoctorTreat_BasicSummary {0},{1},{2}", UserSelf.StationId, UserSelf.DoctorId, hisDaysAgo));
                return Content(string.Format("<span title='本工作站待诊:{0},其他工作站待诊:{1}'><s>{0}</s><i>{1}</i></span>",
                                    finds.First(m => m.name == "ThisToday").waiting,
                                    finds.First(m => m.name == "ThatToday").waiting),
                              "text/html");
            }
            catch { return Content(""); }
        }

        /// <summary>
        /// 需要自己的待接诊数量 Html格式返回数据
        /// </summary> 
        public IActionResult Statistics_DoctorTreat_BasicSummary_NeedTreatHtml_ThisStationOfMe(int hisDaysAgo = 60)
        {
            try
            {
                
                var finds = _db.SqlQuery<Models.StatisticsModels.TreatBasicSummary>(string.Format("exec sp_Statistics_DoctorTreat_BasicSummary {0},{1},{2}", UserSelf.StationId, UserSelf.DoctorId, hisDaysAgo));
                var rlt = finds.First(m => m.name == "ThisToday").waiting;
                if (rlt > 0)
                    return Content(string.Format("<span title='本工作站{0}人次待诊'><s>{0}</s></span>",
                                      rlt), "text/html");
                else return Content("");
            }
            catch { return Content(""); }
        }
        /// <summary>
        /// 需要自己的待接诊数量 Html格式返回数据
        /// </summary> 
        public IActionResult Statistics_DoctorTreat_BasicSummary_NeedTreatHtml_ThatStationOfMe(int hisDaysAgo = 60)
        {
            try
            {
                var finds = _db.SqlQuery<Models.StatisticsModels.TreatBasicSummary>(string.Format("exec sp_Statistics_DoctorTreat_BasicSummary {0},{1},{2}", UserSelf.StationId, UserSelf.DoctorId, hisDaysAgo));
                var rlt = finds.First(m => m.name == "ThatToday").waiting;
                if (rlt > 0)
                {
                    return Content(string.Format("<span title='其他工作站还有{0}人次待诊'><s>{0}</s></span>", rlt),
                                  "text/html");
                }
                else return Content("");
            }
            catch { return Content(""); }
        }

    }
}
