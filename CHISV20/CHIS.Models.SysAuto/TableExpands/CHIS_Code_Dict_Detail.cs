using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models
{
    public partial class CHIS_Code_Dict_Detail
    {
        public CHIS_Code_Dict_Detail() { }
        public CHIS_Code_Dict_Detail(int detailID,string itemKey ,string itemName,string itemValue,bool isEnable ,bool isDefault)
        {
            this.DetailID = detailID;
            this.ItemKey = itemKey;
            this.ItemName = itemName;
            this.ItemValue = itemValue;
            this.IsEnable = isEnable;
            this.IsDefault = isDefault;
        }
    }
}
