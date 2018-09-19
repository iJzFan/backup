using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class PaginatedItemsViewModel<TEntity> where TEntity : class
    {

        /// <summary>
        /// 第几页
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        //public int PageSize { get; private set; }

        /// <summary>
        /// 共几页
        /// </summary>
        public int Total { get; private set; }

        /// <summary>
        /// 数据总条数
        /// </summary>
        public int FindTotal { get; private set; }
        /// <summary>
        /// 数据
        /// </summary>
        public IEnumerable<TEntity> Rows { get; private set; }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">数据每页</param>
        /// <param name="count">数据条数</param>
        /// <param name="data">数据</param>
        public PaginatedItemsViewModel(int pageIndex, int pageSize, int count, IEnumerable<TEntity> data)
        {
            this.Page = pageIndex;
            //this.PageSize = pageSize;
            this.FindTotal = count;
            this.Total = (int)Math.Ceiling(count / (decimal)pageSize); ;
            this.Rows = data;
        }
    }
}
