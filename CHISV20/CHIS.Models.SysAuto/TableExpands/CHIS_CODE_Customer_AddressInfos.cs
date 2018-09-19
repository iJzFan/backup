using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models
{
    public partial class CHIS_Code_Customer_AddressInfos 
    {

        /// <summary>
        /// 是否是合法地址
        /// </summary>
        public bool IsLegalAddress 
        {
            get
            {               

                return this.AreaId > 0 && 
                       !string.IsNullOrWhiteSpace(this.ContactName) && 
                       !string.IsNullOrWhiteSpace(this.Mobile) && 
                       this.Mobile.Length > 6 && 
                       !string.IsNullOrWhiteSpace(this.AddressDetail);
            }
        }
    }

    public partial class vwCHIS_Code_Customer_AddressInfos
    {

        /// <summary>
        /// 是否是合法地址
        /// </summary>
        public bool IsLegalAddress
        {
            get
            {
                return this.AreaId > 0 &&
                       !string.IsNullOrWhiteSpace(this.ContactName) &&
                       !string.IsNullOrWhiteSpace(this.Mobile) &&
                       this.Mobile.Length > 6 &&
                       !string.IsNullOrWhiteSpace(this.AddressDetail) && this.AreaLevel==3;
            }
        }
    }
}
