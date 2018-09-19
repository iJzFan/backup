using ah.Areas.Customer.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ah.Areas.Customer.Controllers.Base
{
    public class CustomerCLB:BaseCBL
    {
        public CustomerCLB(BaseController c) : base(c)
        {

        }
        /// <summary>
        /// 客户地址统一设置默认为空
        /// </summary>
        /// <param name="custId">客户Id</param>
        public void ClearCustomerAddressInfoAsNotDefault(int?custId)
        {
            //批量更新数据库,首先重置默认地址，全部设置为false(0),ture(1)    
            SqlParameter[] pms = new SqlParameter[] { new SqlParameter("@CustomerId", SqlDbType.Int) { Value = custId } };
            MainDbContext.Database.ExecuteSqlCommand("Update CHIS_Code_Customer_AddressInfos set IsDefault=0 where CustomerId=@CustomerId", pms);

        }
    }
}
