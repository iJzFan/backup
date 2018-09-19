using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.DataModels
{
    public class OneMachine
    {
        public string folk { get; set; } 
        public string agency { get; set; }
        public string valid { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string idcard { get; set; }
        public string age { get; set; }
        public string sex { get; set; }
        public string address { get; set; }
        public string measureTime { get; set; }
        public string birthday { get; set; }//格式为YYYY-MM-DD，可选。如果空使用身份证号计算生日
        public string barcode { get; set; }
        public string userIcon { get; set; }// String,采用BASE64编码

    }
}
