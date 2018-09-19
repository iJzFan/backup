using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{

    /// <summary>
    /// 乳腺癌管理实体集
    /// </summary>
    public class BreastMgrSumery
    {

        /// <summary>
        /// 乳腺癌管理总控主文档
        /// </summary>
        public ah.Models.vwAHMS_HMGR_BreastMgr BreastMgrMain { get; set; }


        /// <summary>
        /// 乳腺癌管理详情
        /// </summary>
        public IEnumerable<ah.Models.vwAHMS_HMGR_BreastMgr_Detail> BreastMgrDetails { get; set; }

        /// <summary>
        /// 我的乳腺癌管理等级 空则未评定
        /// </summary>
        public int? MyBreastLevel
        {
            get { return BreastMgrMain?.BreastMgrLevel; }
        }

        /// <summary>
        /// 当前乳腺癌问卷的Id
        /// </summary>
        public Guid? MyBreastUseQAId
        {
            get
            {
                return BreastMgrMain?.BreastQuestionId;
            }
        }
    }
}
