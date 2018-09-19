using System;

namespace CHIS.Models
{
    /// <summary>
    /// TreeGrid的数据
    /// </summary>
    public class TreeGridModel
    {
        public string id { get; set; }
        public string parentId { get; set; }
        public string text { get; set; }
        public bool isLeaf { get; set; }
        public bool expanded { get; set; }
        public bool loaded { get; set; }
        public string entityJson { get; set; }
    }

    /// <summary>
    /// TreeSelect的数据
    /// </summary>
    public class TreeSelectModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public string parentId { get; set; }
        public object data { get; set; }
    }

    public class TreeItem<T>
    {
        public T id { get; set; }
        public T pId { get; set; }
        public string name { get; set; }
    }
}
