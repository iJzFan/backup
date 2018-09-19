using CHIS.Code.MyExpands;
using CHIS.Code.Utility;
using CHIS.Codes.Utility;
using CHIS.Models.StatisticsModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ass;
using Microsoft.AspNetCore.Hosting;

namespace CHIS.Services
{
    /// <summary>
    /// 健客处理接口服务
    /// </summary>
    public class JKWebNetService :BaseService
    {
        IHostingEnvironment _env;
        public JKWebNetService(CHIS.DbContext.CHISEntitiesSqlServer db
            , IHostingEnvironment env) : base(db)
        {
            _env = env;
#if DEBUG
            _tscid = "tsjk001"; _key = "49ek#739";
            // _url = "http://218.107.9.125:8070/openapi/openApis/main";
#endif
        }

        //todo 传给健客     测试    
        // public string tscid = "tsjk001";//健客提供给天使健康cid
        // public string key = "123456";//健康提供给天使健康cid对应的秘钥
        // string url = "http://218.107.9.125:8070/openapi/openApis/main";
        //正式
        public string _tscid = "tsjk001";//健客提供给天使健康cid
        public string _key = "49ek#739";//健康提供给天使健康cid对应的秘钥
        string _url = "https://open-api.jianke.com/openapi/openApis/main";
 

        List<OrderNoList> orderList = null;//订单编号
        /// <summary>  
        /// 创建剑客订单
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="parameters">随同请求POST的参数名称及参数值字典</param>  
        /// <param name="timeout">请求的超时时间</param>  
        ///  <param methods="post">post</param>  
        /// <returns></returns>  
        public bool SendJKWebNetOrder(decimal totalamount, string orderId, long selectedAddressId,string sendRmk, out string FromJKSavedOrderNo)
        {
            if (_env.IsDevelopment()) sendRmk += "[测试]";
            //汇总健康发送数据
            //通过订单编号查询网络发药信息视图表
            var netInf = _db.vwCHIS_Shipping_NetOrder.AsNoTracking().Where(m => m.SendOrderId == orderId).Select(m => new
            {
                orderTime = m.CreatTime,
                netOrderId = m.NetOrderId,

                telephone = m.Mobile,
                consignee = m.ContactName,
                transportCosts = m.ContainTransFee,
                address = m.AddressDetail,

            }).FirstOrDefault();
            var addrInf = _db.vwCHIS_Code_Customer_AddressInfos.AsNoTracking().FirstOrDefault(m => m.AddressId == selectedAddressId);

            Dictionary<string, string> kvs = new Dictionary<string, string>();
            kvs.Add("method", "jianke.order.externalcreate");
            kvs.Add("signMethod", "md5");
            kvs.Add("cid", _tscid);
            kvs.Add("randomStr", Ass.Data.Utils.GetRandomString(8));// Ass.Data.Utils.GetRandomString(8));
            kvs.Add("externalOrderNo", orderId);
            kvs.Add("accountId", "00000000-0000-0000-0000-000000000000");
            kvs.Add("invoice", "null");
            kvs.Add("status", "2");
            kvs.Add("deliveryZipCode", addrInf.ZipCode.ToString());
            kvs.Add("money", Ass.P.PInt(totalamount * 100).ToString());
            kvs.Add("orderTime", netInf.orderTime.ToString("yyyy-M-d H:m:s"));
            kvs.Add("paymentType", "2");//默认微信支付
            kvs.Add("consignee", netInf.consignee + sendRmk.ToRoundStr('['));//发货人的姓名
            kvs.Add("telephone", netInf.telephone); 
            kvs.Add("mobilephone", netInf.telephone);
            kvs.Add("province", ComTools.GetAreaName(addrInf.MergerName, 0));
            kvs.Add("city", ComTools.GetAreaName(addrInf.MergerName, 1));
            kvs.Add("district", ComTools.GetAreaName(addrInf.MergerName, 2));
            kvs.Add("town", "");
            kvs.Add("address", netInf.address);
            kvs.Add("transportCosts", Ass.P.PInt((netInf.transportCosts ?? 0) * 100).ToString());//物流费
            var drugList = _db.vwCHIS_Shipping_NetOrder_Formed_Detail.Where(m => m.NetOrderId == netInf.netOrderId).AsNoTracking().ToList().Select(m => new
            OrderProduct
            {
                productCode = m.ThreePartDrugId.ToString().Replace(" ", ""),
                productName = m.DrugName.ToString().Replace(" ", ""),
                amount = Ass.P.PInt(m.Qty),
                actualPrice = Ass.P.PInt(m.Price * 100),
                packing = m.DrugModel
            }
            ).ToList();
            var json = JKSign(kvs, drugList);
            var netJK = new NETDrugStore();
            string codeJson = netJK.HttpsPostDrugOrderInfo(json, _url);
            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(codeJson);
            var rtnCode = jobj.GetValueString("returnCode", "code");
            if (rtnCode == "SUCCESS")
            {
                FromJKSavedOrderNo = jobj.GetValueString("orderNo");
                return true;
            }
            else throw new Exception("三方返回错误信息:[" + rtnCode + "]" + jobj.GetValueString("returnMsg", "message"));
        }

        /// <summary>  
        /// 单个订单物流查询
        /// </summary>  
        /// <param name="JKOrderId">健康的订单编号</param>  
        /// <returns></returns>  
        public bool Singel_QueryLogistics(string JKOrderId, out string jkLogisticsList)
        {
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            kvs.Add("method", "jianke.logistics.get");
            kvs.Add("cid", _tscid);
            kvs.Add("signMethod", "md5");
            kvs.Add("randomStr", Ass.Data.Utils.GetRandomString(8));
            string[] jKorderIdStr = new string[1];
            jKorderIdStr[0] = JKOrderId;
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(jKorderIdStr).Replace("\"", "");
            kvs.Add("orderNo", str);
            var json = JKSign(kvs, "orderNo");
            var netJK = new NETDrugStore();
            string codeJson = netJK.HttpsPostDrugOrderInfo(json, _url);

            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(codeJson);
            var rtnCode = jobj.GetValueString("returnCode", "code");
            if (rtnCode == "SUCCESS")
            {
                jkLogisticsList = jobj.GetValue("mapList")[0].GetValueString("orderNo");
                return true;
            }
            else throw new Exception("三方返回错误信息:[" + rtnCode + "]" + jobj.GetValueString("returnMsg", "message"));
        }
        /// <summary>
        /// 通过剑客编号批量查询物流
        /// </summary>
        /// <param name="JKOrderId"></param>
        /// <param name="jkLogisticsList"></param>
        /// <returns></returns>
        public bool multi_QueryLogistics(string[] JKOrderId, out string jkLogisticsList)
        {
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            kvs.Add("method", "jianke.logistics.get");
            kvs.Add("cid", _tscid);
            kvs.Add("signMethod", "md5");
            kvs.Add("randomStr", Ass.Data.Utils.GetRandomString(8));
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(JKOrderId).Replace("\"", "");
            kvs.Add("orderNo", str);
            var json = JKSign(kvs, "orderNo");
            var netJK = new NETDrugStore();
            string codeJson = netJK.HttpsPostDrugOrderInfo(json, _url);

            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(codeJson);
            var rtnCode = jobj.GetValueString("returnCode", "code");
            if (rtnCode == "SUCCESS")
            {
                jkLogisticsList = jobj.GetValue("mapList")[0].GetValueString("orderNo");
                return true;
            }
            else throw new Exception("三方返回错误信息:[" + rtnCode + "]" + jobj.GetValueString("returnMsg", "message"));
        }
        #region
        /// <summary>
        /// 健客签名算法生成及拼接json
        /// </summary>
        /// <param name="DicData"></param>
        /// <param name="orderList"></param>
        /// <returns>json</returns>
        public string JKSign(Dictionary<string, string> dictData, string listName)
        {
            //整理List的数据为字符串
            var liststr = Newtonsoft.Json.JsonConvert.SerializeObject(orderList);
            //添加到dict里面去
            var sortItems = dictData.Where(m => !string.IsNullOrWhiteSpace(m.Value)).OrderBy(a => a.Key);
            List<string> kvstrings = new List<string>();
            foreach (var item in sortItems) kvstrings.Add($"{item.Key}={item.Value}");
            string stringA = string.Join("&", kvstrings);
            string stringSignTemp = stringA + "&key=" + _key;
            string signValue = Ass.Data.Secret.MD5(stringSignTemp).ToUpper();
            StringBuilder b = new StringBuilder();
            b.Append("{");
            foreach (var item in sortItems)
            {
                b.AppendLine($"\"{item.Key}\":\"{item.Value}\",");
                //if (item.Key == listName)
                //    b.AppendLine($"\"{item.Key}\":{item.Value},");
                //else
                //    b.AppendLine($"\"{item.Key}\":\"{item.Value}\",");
            }
            b.Append($"\"sign\":\"{signValue}\"");
            b.Append("}");
            var json = b.ToString();
            json = json.Replace("\n", "");
            return json;
        }
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="dictData"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public string JKSign(Dictionary<string, string> dictData, List<OrderProduct> items)
        {
            if (items.Count == 0) throw new Exception("没有药品清单");
            var drugJsonList = Newtonsoft.Json.JsonConvert.SerializeObject(items).ToString();
            dictData.Add("orderProductList", string.Join(",", drugJsonList));
         
            //生成stringA
            var sortItems = dictData.Where(m => !string.IsNullOrWhiteSpace(m.Value)).OrderBy(a => a.Key);
            List<string> kvstrings = new List<string>();
            foreach (var item in sortItems) kvstrings.Add($"{item.Key}={item.Value}");
            string stringA = string.Join("&", kvstrings);
            string stringSignTemp = stringA + "&key=" + _key;
            string signValue = Ass.Data.Secret.MD5(stringSignTemp).ToUpper();
            //创建json
            StringBuilder b = new StringBuilder();
            b.Append("{");
            foreach (var item in sortItems)
            {
                if (item.Key == "orderProductList")
                    b.AppendLine($"\"{item.Key}\":" + Newtonsoft.Json.JsonConvert.SerializeObject(items) + ",");
                else if (item.Key == "money")
                    b.AppendLine($"\"{item.Key}\":{item.Value},");
                else
                    b.AppendLine($"\"{item.Key}\":\"{item.Value}\",");
            }
            b.Append($"\"sign\":\"{signValue}\"");
            b.Append("}");
            var json = b.ToString();

            json = json.Replace("\n", "");
            return json;
        }
        public string JKSign(Dictionary<string, string> dictData, string listName, string listName1)
        {
            //整理List的数据为字符串
            var liststr = Newtonsoft.Json.JsonConvert.SerializeObject(orderList);
            //添加到dict里面去
            var sortItems = dictData.Where(m => !string.IsNullOrWhiteSpace(m.Value)).OrderBy(a => a.Key);
            List<string> kvstrings = new List<string>();
            foreach (var item in sortItems) kvstrings.Add($"{item.Key}={item.Value}");
            string stringA = string.Join("&", kvstrings);
            string stringSignTemp = stringA + "&key=" + _key;
            string signValue = Ass.Data.Secret.MD5(stringSignTemp).ToUpper();
            StringBuilder b = new StringBuilder();
            b.Append("{");
            foreach (var item in sortItems)
            {
                if (item.Key == listName)
                    b.AppendLine($"\"{item.Key}\":{item.Value},");
                else if (item.Key == listName1)
                    b.AppendLine($"\"{item.Key}\":{item.Value},");
                else
                    b.AppendLine($"\"{item.Key}\":\"{item.Value}\",");
            }
            b.Append($"\"sign\":\"{signValue}\"");
            b.Append("}");
            var json = b.ToString();
            json = json.Replace("\n", "");
            return json;
        }
        #endregion
        /// <summary>
        /// 处理健客返回值
        /// </summary>
        /// <param name="codeJson"></param>
        /// <returns></returns>
        public Object Json_HandleResult(string codeJson)
        {
            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(codeJson);
            var rtnCode = jobj.GetValueString("returnCode", "code");
            if (rtnCode == "SUCCESS")
            {

                return jobj;
            }
            else throw new Exception("三方返回错误信息:[" + rtnCode + "]" + jobj.GetValueString("returnMsg", "message"));
        }
        /// <summary>
        /// 检查药品是否正常可用 否则抛出错误(单个药品的库存查询，如果大于10个就可以发货)正式库存查询
        /// </summary>
        /// <param name="threePartDrugId">健客药品Id</param>
        /// <returns>true 表示可用</returns>
        public bool CheckDrugIsAvaliable(int threePartDrugId)
        {

            //todo 验证健客的药品是否可用
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            kvs.Add("method", "jianke.product.get");
            kvs.Add("signMethod", "md5");
            kvs.Add("cid", _tscid);
            kvs.Add("randomStr", Ass.Data.Utils.GetRandomString(8));         
            kvs.Add("productCode", threePartDrugId.ToString());
            var json = JKSign(kvs, "productCode");
            if (json == "") throw new Exception("没有数据可查询！");
            //药品list集合productCode
            var netJK = new NETDrugStore();
            string codeJson = netJK.HttpsPostDrugOrderInfo(json, _url);
            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(codeJson);
            var rtnCode = jobj.GetValueString("returnCode", "code");
            if (rtnCode == "SUCCESS")
            {
                string numStr = jobj.GetValue("productList")[0].GetValueString("productInventory");
                if (string.IsNullOrEmpty(numStr))
                {
                    throw new Exception("暂无该药品");
                }
                else
                {
                    int num = Convert.ToInt32(numStr);
                    if (num <= 0) throw new Exception("该药品没有库存");
                    if (num <= 6) throw new Exception("库存紧张");
                    return true;
                }
            }
            else throw new Exception("三方返回错误信息:[" + rtnCode + "]" + jobj.GetValueString("returnMsg", "message"));

        }

        /// <summary>
        /// 获取药品的基本信息
        /// </summary>
        /// <param name="threePartDrugId">第三方药品的Id</param>

        public CHIS.Models.DataModels.DrugInfo QueryDrugInfo(int threePartDrugId)
        {
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            kvs.Add("method", "jianke.product.get");
            kvs.Add("signMethod", "md5");
            kvs.Add("cid", _tscid);
            kvs.Add("randomStr", Ass.Data.Utils.GetRandomString(8));
            //string[] productIdStr = new string[1];
            //productIdStr[0] = threePartDrugId.ToString();
            //var str = Newtonsoft.Json.JsonConvert.SerializeObject(productIdStr).Replace("\"", "");
            //kvs.Add("productCode", str);
            kvs.Add("productCode", threePartDrugId.ToString());
            var json = JKSign(kvs, "productCode");
            if (json == "") throw new Exception("没有数据可查询！");
            //药品list集合productCode
            var netJK = new NETDrugStore();
            string codeJson = netJK.HttpsPostDrugOrderInfo(json, _url);
            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(codeJson);
            var rtnCode = jobj.GetValueString("returnCode", "code");
            if (rtnCode == "SUCCESS")
            {
                var item = jobj.GetValue("productList")[0];
                return new CHIS.Models.DataModels.DrugInfo
                {
                    productCode = item.GetValueString("productCode"),
                    productName = item.GetValueString("productName"),
                    mainProductCode = item.GetValueString("mainProductCode"),
                    productStatusType = Ass.P.PIntV(item.GetValueString("productStatusType"), 0),
                    marketPrice = Ass.P.PIntV(item.GetValueString("marketPrice"), 9999),
                    ourPrice = Ass.P.PIntV(item.GetValueString("ourPrice"), 9999),
                    manufacturer = item.GetValueString("manufacturer"),
                    prescriptionType = item.GetValueString("prescriptionType"),
                    productInventory = Ass.P.PIntV(item.GetValueString("productInventory"), 0),
                    productAttribute = item.GetValueString("productAttribute"),
                    thumbnailUrl = item.GetValueString("thumbnailUrl"),
                    introduction = item.GetValueString("introduction"),
                    packing = item.GetValueString("packing")
                };
            }
            else throw new Exception("三方返回错误信息:[" + rtnCode + "]" + jobj.GetValueString("returnMsg", "message"));
        }

        /// <summary>
        /// 虚拟库存查询
        /// </summary>
        /// <param name="ThreePartDrugId"></param>
        /// <param name="qyt"></param>
        /// <returns></returns>
        public bool CheckDrugIsAvaliableReal(string ThreePartDrugId/*, out int qyt*/)
        {
            var warehouse = new string[2];
            //todo 验证健客的药品是否可用
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            kvs.Add("method", "jianke.stock.getReal");
            kvs.Add("signMethod", "md5");
            kvs.Add("cid", _tscid);
            kvs.Add("randomStr", Ass.Data.Utils.GetRandomString(8));
            //仓库
            warehouse[0] = "1";
            warehouse[1] = "4";
            var warehouseCode = Newtonsoft.Json.JsonConvert.SerializeObject(warehouse).Replace("\"", "");
            kvs.Add("warehouseCode", warehouseCode);
            var productIdStr = new string[1];
            productIdStr[0] = ThreePartDrugId;
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(productIdStr).Replace("\"", "");
            kvs.Add("productCode", str);
            var json = JKSign(kvs, "productCode", "warehouseCode");
            if (json == "") throw new Exception("没有数据可查询！");
            //药品list集合productCode
            var netJK = new NETDrugStore();
            string codeJson = netJK.HttpsPostDrugOrderInfo(json, _url);
            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(codeJson);
            var rtnCode = jobj.GetValueString("returnCode", "code");
            if (rtnCode == "SUCCESS")
            {
                var numStr = jobj.GetValueString("quantity");
                int num = Convert.ToInt32(numStr);
                //qyt = num;
                if (num <= 0) throw new Exception("该药品没有库存");
                if (num <= 6) throw new Exception("库存紧张");
                return true;
            }
            else throw new Exception("三方返回错误信息:[" + rtnCode + "]" + jobj.GetValueString("returnMsg", "message"));


        }
        /// <summary>
        /// 批量产品库存查询
        /// </summary>
        /// <param name="ThreePartDrugId"></param>
        /// <returns></returns>
        public bool CheckDrugIsAvaliables(string[] ThreePartDrugIds)
        {


            //todo 验证健客的药品是否可用
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            kvs.Add("method", "jianke.stock.get");
            kvs.Add("signMethod", "md5");
            kvs.Add("cid", _tscid);
            kvs.Add("randomStr", Ass.Data.Utils.GetRandomString(8));
            var strs = Newtonsoft.Json.JsonConvert.SerializeObject(ThreePartDrugIds).Replace("\"", "");
            kvs.Add("productCode", strs);
            var json = JKSign(kvs, "productCode");
            if (json == "") throw new Exception("没有数据可查询！");
            //药品list集合productCode
            var netJK = new NETDrugStore();
            string codeJson = netJK.HttpsPostDrugOrderInfo(json, _url);
            Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(codeJson);
            var rtnCode = jobj.GetValueString("returnCode", "code");
            if (rtnCode == "SUCCESS")
            {
                var numStr = jobj.GetValueString("quantity");
                int num = Convert.ToInt32(numStr);
                if (num <= 10) throw new Exception("库存紧张");
                return true;
            }
            else throw new Exception("三方返回错误信息:[" + rtnCode + "]" + jobj.GetValueString("returnMsg", "message"));


        }

    }
}
