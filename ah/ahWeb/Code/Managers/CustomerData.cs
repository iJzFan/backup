using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ah.Models;
using Microsoft.EntityFrameworkCore;

namespace ah.Code.Managers
{


    public interface IUserFrameMgr
    {
        /// <summary>
        /// 获取用户的信息
        /// </summary>
        Models.CHIS_Code_Customer GetCustomerData(int customerId);

        /// <summary>
        /// 获取用户的健康信息
        /// </summary>
        Models.CHIS_Code_Customer_HealthInfo GetCustomerHealthInfo(int customerId);

        Models.vwCHIS_Sys_Login GetLoginInfo(int customerId);
    }

    public class CustomerFrameManager : BaseInject, IUserFrameMgr
    {
        public Models.CHIS_Code_Customer GetCustomerData(int customerId)
        {
           
            var customer = MainDbContext.CHIS_Code_Customer.Find((int)customerId);
            return customer;
            
        }

        public CHIS_Code_Customer_HealthInfo GetCustomerHealthInfo(int customerId)
        {
            return MainDbContext.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == customerId);
        }

        public vwCHIS_Sys_Login GetLoginInfo(int customerId)
        {
            var find = MainDbContext.vwCHIS_Sys_Login.AsNoTracking().FirstOrDefault(m => m.CustomerId == customerId);
            return find;
        }
    }

}
