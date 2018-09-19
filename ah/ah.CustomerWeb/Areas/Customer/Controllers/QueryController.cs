using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ah.Areas.Customer.Controllers
{
    [Route("Customer/[controller]")]
    [Area("Customer")]
    [Authorize]
    public class QueryController : Controller
    {
        protected class JsonReturn
        {
            public bool Rlt { get; set; }
            public string Message { get; set; }
        }

        protected Models.DataModel.CustomerClaimData userData = null;

        protected Models.DataModel.CustomerClaimData GetCurrentLoginUser
        {
            get
            {
                if (this.User == null) return null;
                if (User.Identity.IsAuthenticated)
                {
                    if (userData != null) return userData;
                    userData = new Models.DataModel.CustomerClaimData
                    {
                        CustomerName = this.User.Identity.Name


                    };
                    //其它属性
                    ClaimsPrincipal user = this.User;
                    IEnumerable<Claim> claims = user.Claims;
                    foreach (Claim cm in claims)
                    {
                        switch (cm.Type)
                        {

                            case "CustomerId": userData.CustomerId = int.Parse(cm.Value); break;
                            case "LoginId": userData.LoginId = int.Parse(cm.Value); break;
                            case "CustomerName": userData.CustomerName = cm.Value; break;
                            case "CustomerMobile": userData.CustomerMobile = cm.Value; break;
                            case "CustomerEmail": userData.CustomerEmail = cm.Value; break;
                            case "Gender": userData.Gender = int.Parse(cm.Value); break;
                            case "CustomerPWD": userData.CustomerPWD = cm.Value; break;
                            case "IDCard": userData.IDcard = cm.Value; break;
                        }
                    }
                    return userData;
                }



                return null;
            }
        }
    }
}
