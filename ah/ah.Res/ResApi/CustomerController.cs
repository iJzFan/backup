using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ah.Res.ResApi
{
    public class CustomerController : Controller
    {

        public IActionResult Index()
        {
            return Content("获取相关信息");
        }

        // GET: /<controller>/
        public IActionResult GetPhoto(string fn)
        {
            if (string.IsNullOrWhiteSpace(fn))
            {
                var img = "R0lGODlhAQABAIAAAP///////yH5BAAHAP8ALAAAAAABAAEAAAICRAEAOw==";
                return File(Convert.FromBase64String(img), "image/gif");
            }
            else
            {
                var file = Global.ConfigSettings.CustomerImagePathRoot + fn;
                return File(file, "image/png");
            }
        }

        private IActionResult GetPhotoPath(string fn)
        {
            var fnn = string.IsNullOrWhiteSpace(fn) ? "data:image/gif;base64,R0lGODlhAQABAIAAAP///////yH5BAAHAP8ALAAAAAABAAEAAAICRAEAOw==" :
                "http://localhost:61440/resapi/customer/GetPhoto?name=060c4ba34f764cb5a567934af2dfaa59.jpg";
            return Content(fnn);
        }

    }
}
