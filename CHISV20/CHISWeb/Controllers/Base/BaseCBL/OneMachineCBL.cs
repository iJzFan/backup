using Ass;
using Ass.Mvc;
using System;
using System.Collections.Generic;
 
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Controllers
{
    public class OneMachineCBL : BaseCBL
    {
        public OneMachineCBL(BaseController c) : base(c)
        {

        }

 
        /// <summary>
        /// 获取一体机的清单表
        /// </summary>
        /// <param name="fromdt">开始时间</param>
        /// <param name="todt">结束时间</param>
        public PageListInfo<CHIS.Models.DataModels.OneMachineRec> OneMachineList(DateTime fromdt, DateTime todt, int pageSize, int pageIndex = 1,bool onlyCount=false)
        {
            var u = controller.UserSelf;
            SqlParameter[] sqlp = new SqlParameter[3];
            sqlp[0] = new SqlParameter("@stationId", u.StationId);
            sqlp[1] = new SqlParameter("@datetimeStart",fromdt);
            sqlp[2] = new SqlParameter("@datetimeEnd", todt);
            var findList = (from item in _db.SqlQuery<CHIS.Models.DataModels.OneMachineRec>("exec sp_DataInput_TreatListFromOneMachine @stationId,@datetimeStart,@datetimeEnd", sqlp)
                            select item).ToList();
          

            int findTotal = findList.Count();
            if (onlyCount) return new PageListInfo<Models.DataModels.OneMachineRec> { RecordTotal = findTotal };

            int totalPage = (int)Math.Ceiling(findTotal * 1.0f / pageSize);

            //排序获取当前页的数据  
            var dataList = findList.OrderBy(m => m.MeasureTime).
                    Skip(pageSize * (pageIndex - 1)).
                    Take(pageSize).AsQueryable().ToList();

            return new PageListInfo<Models.DataModels.OneMachineRec>()
            {
                PageSize = pageSize,
                PageIndex = pageIndex,
                RecordTotal = findTotal,
                DataList = dataList
            };
        }
 

    }
}
