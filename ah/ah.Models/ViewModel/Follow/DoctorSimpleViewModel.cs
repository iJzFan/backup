using System;
using System.Collections.Generic;
using System.Text;

namespace ah.Models.ViewModel
{
    //public class DoctorSimpleViewModel
    //{
    //    public DoctorSimpleViewModel()
    //    {
    //        Items = new List<DoctorSimpleInfo>();
    //        PageIndxe = 0;
    //        PageSize = 0;
    //    }

    //    public bool Rlt { get; set; }

    //    public string Code { get; set; }

    //    public string Msg { get; set; }

    //    public string State { get; set; }

    //    public string Message { get; set; }

    //    public int PageIndxe { get; set; }

    //    public int PageSize { get; set; }

    //    public IEnumerable<DoctorSimpleInfo> Items { get; set; }
    //}

    public class DoctorSimpleInfo
    {
        public int DoctorOpId { get { return CustomerId; } }
        public int CustomerId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorGender { get; set; }
        public string PostTitleName { get; set; }
        public string DoctorSkillRmk { get; set; }
        public string DoctorPhotoUrl { get; set; }
        public bool? IsFollow { get; set; }
        /// <summary>
        /// 医生App端的Id
        /// </summary>
        public string DoctorAppId { get; set; }
    }
}
