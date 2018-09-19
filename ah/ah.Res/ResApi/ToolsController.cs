using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Ass;

namespace ah.Res.ResApi
{
    public partial class ToolsController : Controller
    {
        IHostingEnvironment _env;
        public ToolsController(IHostingEnvironment env)
        {
            this._env = env;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="posType"></param>
        /// <param name="sourceId"></param>
        /// <param name="picType">¡Ùø’£¨‘Ú «Õ∑œÒÕº∆¨/cert÷§º˛Õº∆¨</param>
        /// <returns></returns>
        public IActionResult UploadPic(string fileName, string posType, string sourceId, string picType = "icon", string size = "256x256")
        {
            ViewBag.fileName = fileName;
            ViewBag.posType = posType;
            ViewBag.SourceId = sourceId;
            ViewBag.Size = new System.Drawing.Size().Parse(size);
            if (picType == "icon") return View("UploadPic");
            if (picType == "cert") return View("UploadCertPic");
            else return null;
        }

        public JsonResult SendBase64Image(string image, string posType, string fileName)
        {
            HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*"; //‘ –ÌøÁ”Ú∑√Œ     
            int start = image.IndexOf(',');
            if (start > 0) image = image.Substring(start + 1);
            var root = "";
            switch (posType)
            {
                case "customer": root = ah.Global.ConfigSettings.CustomerImagePathRoot; break;
                case "doctor": root = ah.Global.ConfigSettings.DoctorImagePathRoot; break;
                case "station": root = ah.Global.ConfigSettings.StationImagePathRoot; break;
                case "cert": root = ah.Global.ConfigSettings.CertificateImagePathRoot; break;
                case "drug": root = ah.Global.ConfigSettings.DrugImagePathRoot; break;
            }

            string _name = Guid.NewGuid().ToString("N");
            string name = _name + ".jpg";
            var img = Ass.Data.Images.Base64StringToImage(image, size: new System.Drawing.Size(800, 800));
            if (root.Contains("http")) root = new Uri(root).AbsolutePath;
            string abroot = (this._env.WebRootPath + root).Replace('/', System.IO.Path.DirectorySeparatorChar);
            img.Save(abroot + name);
            //Àı¬‘Õº72x72
            var imgmin = Ass.Data.Images.Base64StringToImage(image, size: new System.Drawing.Size(72, 72));
            imgmin.Save(abroot + _name + "_72.jpg");

            if (!string.IsNullOrEmpty(fileName) && System.IO.File.Exists(abroot + fileName)) System.IO.File.Delete(abroot + fileName);
            return Json(new { rlt = true, Name = name });
        }
    }
}