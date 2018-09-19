using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class PaginatedItemsViewModel<TEntity> where TEntity : class
    {

        /// <summary>
        /// 第几页
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        //public int PageSize { get; private set; }

        /// <summary>
        /// 共几页
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 数据总条数
        /// </summary>
        public int FindTotal { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public IEnumerable<TEntity> Rows { get; set; }
    }
}
