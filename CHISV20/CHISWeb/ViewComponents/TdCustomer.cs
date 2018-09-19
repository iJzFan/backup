using CHIS.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.ViewComponents
{
    public class TdCustomerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(TdCustomerModel model)
        { 
            return View(model);
        }
    }
}
