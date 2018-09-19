using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CHIS.Code.Managers;
using CHIS.Code.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Api.OpenApi.v1
{

    public class PictureController : OpenApiBaseController
    {


        public PictureController(DbContext.CHISEntitiesSqlServer db) : base(db)
        {
        }

        /// <summary>
        /// 上传base64图片
        /// </summary>
        /// <param name="data"></param>
        /// <returns>返回url</returns>
        [HttpPost]
        public async Task<IActionResult> UploadBase64([FromBody]JObject data)
        {
            var imageBase64 = data["image_base64"].ToObject<string>();
            var fileName = data["file_name"].ToObject<string>();

            if (!string.IsNullOrWhiteSpace(imageBase64))
            {
                var reg = new Regex("data:image/(.*);base64,");

                imageBase64 = reg.Replace(imageBase64, "");

                byte[] imageByte = Convert.FromBase64String(imageBase64);

                var fileExt = fileName.Split('.').Reverse().FirstOrDefault();

                try
                {
                    var url = await FastDFSHelper.UploadAsync(imageByte, fileExt);
                    var rlt= MyDynamicResult(true, "");
                    rlt.url = url;

                    var img = BytesToImage(imageByte);

                    //var imgSM = SizeImageWithOldPercent(img, 100, 100);

                    //var byteSM = ImageToBytes(imgSM, fileExt);

                    //var urlSM = await FastDFSHelper.UploadAsync(byteSM, fileExt);

                    //rlt.urlSM = urlSM;

                    return Ok(rlt);

                } catch (Exception e)
                {
                    var rlt = MyDynamicResult(false, "网络错误，请重试");

                    return BadRequest(rlt);
                }
            }
            else
            {
                var rlt = MyDynamicResult(false, "图片不能为空");

                return BadRequest(rlt);
            }
        }

        #region 裁图相关私有方法

        /// <summary> 
        /// 按照指定大小缩放图片，但是为了保证图片宽高比自动截取 
        /// </summary> 
        /// <param name="srcImage"></param> 
        /// <param name="iWidth"></param> 
        /// <param name="iHeight"></param> 
        /// <returns></returns> 
        private static Bitmap SizeImageWithOldPercent(Image srcImage, int iWidth, int iHeight)
        {
            try
            {
                // 要截取图片的宽度（临时图片） 
                int newW = srcImage.Width;
                // 要截取图片的高度（临时图片） 
                int newH = srcImage.Height;
                // 截取开始横坐标（临时图片） 
                int newX = 0;
                // 截取开始纵坐标（临时图片） 
                int newY = 0;
                // 截取比例（临时图片） 
                double whPercent = 1;
                whPercent = ((double)iWidth / (double)iHeight) * ((double)srcImage.Height / (double)srcImage.Width);
                if (whPercent > 1)
                {
                    // 当前图片宽度对于要截取比例过大时 
                    newW = int.Parse(Math.Round(srcImage.Width / whPercent).ToString());
                }
                else if (whPercent < 1)
                {
                    // 当前图片高度对于要截取比例过大时 
                    newH = int.Parse(Math.Round(srcImage.Height * whPercent).ToString());
                }
                if (newW != srcImage.Width)
                {
                    // 宽度有变化时，调整开始截取的横坐标 
                    newX = Math.Abs(int.Parse(Math.Round(((double)srcImage.Width - newW) / 2).ToString()));
                }
                else if (newH == srcImage.Height)
                {
                    // 高度有变化时，调整开始截取的纵坐标 
                    newY = Math.Abs(int.Parse(Math.Round(((double)srcImage.Height - (double)newH) / 2).ToString()));
                }
                // 取得符合比例的临时文件 
                Bitmap cutedImage = CutImage(srcImage, newX, newY, newW, newH);
                // 保存到的文件 
                Bitmap b = new Bitmap(iWidth, iHeight);
                Graphics g = Graphics.FromImage(b);
                // 插值算法的质量 
                g.InterpolationMode = InterpolationMode.Default;
                g.DrawImage(cutedImage, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(0, 0, cutedImage.Width, cutedImage.Height), GraphicsUnit.Pixel);
                g.Dispose();
                return b;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary> 
        /// 剪裁 -- 用GDI+ 
        /// </summary> 
        /// <param name="b">原始Bitmap</param> 
        /// <param name="StartX">开始坐标X</param> 
        /// <param name="StartY">开始坐标Y</param> 
        /// <param name="iWidth">宽度</param> 
        /// <param name="iHeight">高度</param> 
        /// <returns>剪裁后的Bitmap</returns> 
        private static Bitmap CutImage(Image b, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (b == null)
            {
                return null;
            }
            int w = b.Width;
            int h = b.Height;
            if (StartX >= w || StartY >= h)
            {
                // 开始截取坐标过大时，结束处理 
                return null;
            }
            if (StartX + iWidth > w)
            {
                // 宽度过大时只截取到最大大小 
                iWidth = w - StartX;
            }
            if (StartY + iHeight > h)
            {
                // 高度过大时只截取到最大大小 
                iHeight = h - StartY;
            }
            try
            {
                Bitmap bmpOut = new Bitmap(iWidth, iHeight);
                Graphics g = Graphics.FromImage(bmpOut);
                g.DrawImage(b, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(StartX, StartY, iWidth, iHeight), GraphicsUnit.Pixel);
                g.Dispose();
                return bmpOut;
            }
            catch
            {
                return null;
            }
        }

        private static Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = Image.FromStream(ms);
            return image;
        }

        private static byte[] ImageToBytes(Image image,string fileExt)
        {
            
            using (MemoryStream ms = new MemoryStream())
            {
                if (fileExt.ToLower() == "jpg")
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                if (fileExt.ToLower() == "gif")
                {
                    image.Save(ms, ImageFormat.Gif);
                }
                if (fileExt.ToLower() == "png")
                {
                    image.Save(ms, ImageFormat.Png);
                }
                if (fileExt.ToLower() == "bmp")
                {
                    image.Save(ms, ImageFormat.Bmp);
                }
                if (fileExt.ToLower() == "ico")
                {
                    image.Save(ms, ImageFormat.Icon);
                }

                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        #endregion
    }
}
