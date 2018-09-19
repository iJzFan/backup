using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Ass;
using Microsoft.AspNetCore.Authorization;

namespace CHISWeb.Controllers.Tools
{
    public partial class ToolsController : CHIS.Controllers.BaseController
    {
        IHostingEnvironment _env;
        public ToolsController(IHostingEnvironment env,CHIS.DbContext.CHISEntitiesSqlServer db):base(db)
        {
            this._env = env;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="posType"></param>
        /// <param name="sourceId"></param>
        /// <param name="picType">留空，则是头像图片/cert证件图片</param>
        /// <returns></returns>
        public IActionResult UploadPic(string fileName, string posType, string sourceId, string picType = "icon", string size = "256x256")
        {
            ViewBag.fileName = Ass.P.PStr(fileName).Replace("undefined", "");
            ViewBag.posType = posType;
            ViewBag.SourceId = sourceId;
            ViewBag.Size = new System.Drawing.Size().Parse(size);
            if (picType == "icon") return View("UploadPic");
            if (picType == "cert") return View("UploadCertPic");
            if (picType == "drug") return View("UploadPic");
            if (picType == "gift") return View("UploadPic");
            else return null;
        }


        /*
        public JsonResult SendBase64Image(string image, string posType, string fileName)
        {
            // return Json(new { rlt = true });

            int start = image.IndexOf(',');
            if (start > 0) image = image.Substring(start + 1);
            var root = "";
            switch (posType)
            {
                case "customer": root = CHIS.Global.ConfigSettings.CustomerImagePathRoot; break;
                case "doctor": root = CHIS.Global.ConfigSettings.DoctorImagePathRoot; break;
                case "station": root = CHIS.Global.ConfigSettings.StationImagePathRoot; break;
                case "cert":root= CHIS.Global.ConfigSettings.CertificateImagePathRoot; break;
            }
            string name = Guid.NewGuid().ToString("N") + ".jpg";
            var img = Ass.Data.Images.Base64StringToImage(image, size: new System.Drawing.Size(800, 800));
            if (root.Contains("http")) root =new Uri(root).AbsolutePath;
            string abroot=( this._env.WebRootPath + root).Replace('/', System.IO.Path.DirectorySeparatorChar);            
            img.Save(abroot+name); 
            if (!string.IsNullOrEmpty(fileName) && System.IO.File.Exists(abroot + fileName)) System.IO.File.Delete(abroot + fileName);
            return Json(new { rlt = true, Name = name });
        }
        */
        /// <summary>
        /// 用于展示前端布局和功能组件说明
        /// 只是用于开发人员展示
        /// </summary>
        public IActionResult ToolIndex()
        {
            return View();
        }
        /// <summary>
        /// 通用树弹出框
        /// </summary>
        public IActionResult UniversalTree(string turl)
        {
            ViewBag.turl = turl;
            return View();
        }


        #region 二维码扫描入口
        /// <summary>
        /// 二维码扫描
        /// </summary>
        /// <param name="n">名</param>
        /// <param name="v">值</param>
        /// <param name="t">类别</param>
        /// <param name="tid">选填 接诊Id</param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult QrScan(string n,string v,string t,long? tid=null)
        {
            var tu = t.ToUpper();
            switch (tu)
            {
                case "SICKNOTE":
                    return RedirectToAction("SickNoteCheck", "Search", new { sickNoteId =v});
                case "FORMED":
                    return RedirectToAction("PrescriptionCheck", "Search",
                        new { prescriptionNo = v});
            }
            return View();
        }

 
        public IActionResult InQrScan()
        {
            return View();
        }

        #endregion
    }
}