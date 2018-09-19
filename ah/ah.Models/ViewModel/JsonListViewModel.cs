using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    public class JsonListViewModel<T>
    {
        public JsonListViewModel()
        {
            Items = new List<T>();
            PageIndex = 0;
            PageSize = 0;
            TotalPages = 0;
        }

        public bool Rlt { get; set; }

        public string Code { get; set; }

        public string Msg { get; set; }

        public string State { get; set; }

        public string Message { get; set; }

        public int PageIndex { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; }

        public IEnumerable<T> Items { get; set; }
    }
}
