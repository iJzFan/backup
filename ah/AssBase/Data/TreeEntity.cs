using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ass.Data
{

    /// <summary>
    /// 树形结构的数据表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeEntity<T> where T : class
    {
        List<T> _allTs = null;
        List<TreeEntity<T>> _allItems = null;
        List<TreeEntity<T>> _subItems = new List<TreeEntity<T>>();


        public TreeEntity<T> FatherEntity { get; set; }

        /// <summary>
        /// 父项目
        /// </summary>
        public T ParentItem { get; set; }
        /// <summary>
        /// 本项目
        /// </summary>
        public T ThisItem { get; set; }



        /// <summary>
        /// 子项目
        /// </summary>
        public List<TreeEntity<T>> SubItems { get { return _subItems; } }
        /// <summary>
        /// 是否有子项目
        /// </summary>
        public bool HasSubs { get { return SubItems.Count > 0; } }
        /// <summary>
        /// 是否有父项目
        /// </summary>
        public bool HasParent { get { return FatherEntity != null; } }



        //初始化树形数据
        public TreeEntity<T> Inital(List<T> list, Func<T, string> pMyFatherId, Func<T, string> pMyId)
        {
            this._allTs = list;
            this._allItems = (from item in this._allTs
                             select new TreeEntity<T>
                             {
                                 ThisItem = item,
                                 ParentItem = this._allTs.FirstOrDefault(a => pMyId.Invoke(a) == pMyFatherId.Invoke(item))
                             }).ToList();           
            for (int i = 0; i < _allItems.Count; i++)
            {
                var item = _allItems[i];
                //找到父级
                var father = _allItems.FirstOrDefault(m => m.ThisItem == item.ParentItem);
                father = (item.ParentItem == null || father == null) ? this : father;
                item.FatherEntity = father;
                father.SubItems.Add(item);
            }

            return this;

        }





    }
}
