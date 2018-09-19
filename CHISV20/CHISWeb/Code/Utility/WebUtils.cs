using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Codes.Utility
{
    ///<summary>
    ///短信接口
    ///</summary>
    public class SMS
    {
        public string uid = "114";
        public string username = "tsjk";
        public string password = "3D9188577CC9BFE9291AC66B5CC872B7";
        /// <summary> 发送短信，得到返回值</summary>
        /// string strRet = null;
        /// <summary> 发送短信，得到返回值</summary>
        public async Task<string> PostSmsInfoAsync(string mobile, string txt)
        {
            var pms = $"uid={uid}&username={username}&password={password}&mobiles={mobile}&content={txt}";
            return await WebHelper.PostMoth("http://v.369sms.com/SMS/SendSingleSMS", pms, Encoding.UTF8);
        }
    }
    //邮件接口

    public class Email
    {

    }
    /// <summary>
    /// 网络药店数据接口，
    /// method: post
    /// </summary>

    public class NETDrugStore
    {


        string url = "http://218.107.9.125:8070/openapi/openApis/main";
        //正式环境 
        // string url = "https://open-api.jianke.com/openapi/openApis/main";
        /// <summary>
        /// 通过POST方法把数据发送给网络药店
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="order_no"></param>
        /// <returns>SUCCESS/FAIL</returns>

        public string HttpsPostDrugOrderInfo(string data)
        {
            //var parameter = $"sign={signValue}";
            var jsonData = data;
            //string resultCode = Ass.Web.WebHelper.PostMethod("http",url,per, ASCIIEncoding.UTF8);
            string resultCode = WebHelper.PostTest(url, jsonData).Result;
            // string resultCode = Ass.Web.WebHelper.GetResponseData(url, jsonData);
            return resultCode;
        }
        /// <summary>
        /// 健客网络接口
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string HttpsPostDrugOrderInfo(string data, string url)
        {

            var jsonData = data;
            string resultCode = WebHelper.PostTest(url, jsonData).Result;
            return resultCode;
        }
    }
}

