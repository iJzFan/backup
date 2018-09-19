using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
namespace ah.Codes.Utility
{
    ///<summary>
    ///短信接口
    ///</summary>
    public class WebHelper
    {
        // <summary>  
        /// 上传的文件  
        /// </summary>  
        /// <param name="url">服务器请求地址</param>  
        /// <param name="paramDate">发送的数据</param>  
        /// <param name="encode">网站编码</param>  
        /// <returns>字符串类型的true/false</returns>  
        public static async Task<string> PostMoth(string url, string paramDate, System.Text.Encoding encode)
        {
            string str = string.Empty;
            try
            {
                byte[] byteArray = encode.GetBytes(paramDate);//将数据转换成对应的网站编码            
                System.Net.HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);//发出请求  
                                                                                                  //ServicePointManager.DefaultConnectionLimit = 100;  
                webRequest.ContentType = "application/x-www-form-urlencoded";//报表头类型  
                webRequest.Method = "POST";//请求方式Post  
                                           // webRequest.Timeout = 12000;//请求的超时时间  
                                           // webRequest.ContentLength = byteArray.Length;//获取要传输的长度  
                Stream requeststream = await webRequest.GetRequestStreamAsync();//获取写入的数据流                  
                                                                                //StreamWriter strw = new StreamWriter(requeststream);  
                requeststream.Write(byteArray, 0, byteArray.Length);//将数据写入到当前流中  
                HttpWebResponse webResponse = (HttpWebResponse)(await webRequest.GetResponseAsync());//返回来自服务器的相应   
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (webResponse.Headers["Content-Encoding"] != null && webResponse.Headers["Content-Encoding"].ToLower().Contains("gzip"))//如果http头中接受gzip的话，这里就要判断是否为有压缩，有的话，直接解压缩即可  
                    {
                        requeststream = new System.IO.Compression.GZipStream(requeststream,
                            System.IO.Compression.CompressionMode.Decompress);//解压缩基础流  
                    }
                    StreamReader strReader = new StreamReader(webResponse.GetResponseStream());//读取对应编码的文件流  
                    str = strReader.ReadToEnd();//读文件流至最后  
                    webResponse.Dispose();
                    strReader.Dispose();
                    requeststream.Dispose();//关闭文件流  
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}

