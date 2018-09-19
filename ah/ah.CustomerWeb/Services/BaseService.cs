using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ah.Services
{
    public class BaseService
    {
        protected HttpClient _client;

        public BaseService()
        {

            var scheme = Global.Config.GetSection("CHISToken:Scheme").Value;

            var parameter = Global.Config.GetSection("CHISToken:Parameter").Value;

            _client = new HttpClient();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
        }

        public async Task<T> PostDataAsync<T>(string postUrl,Object data)
        {
            var json = JObject.FromObject(data);

            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            var message = await _client.PostAsync(postUrl, content);

            return await ToClassAsync<T>(message);
        }

        public async Task PostDataAsync(string postUrl, Object data)
        {
            var json = JObject.FromObject(data);

            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            await _client.PostAsync(postUrl, content);
        }

        public async Task<T> GetDataAsync<T>(string getUrlWithQuery)
        {
            var res = await _client.GetAsync(getUrlWithQuery);

            if (res.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("网络错误请重试！");
            }

            var data = await ToClassAsync<T>(res);

            return data;
        }

        public async Task GetDataAsync(string getUrlWithQuery)
        {
            var res = await _client.GetAsync(getUrlWithQuery);

            if (res.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("网络错误请重试！");
            }
        }

        /// <summary>
        /// Convert HttpResponseMessage to Model
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async static Task<T> ToClassAsync<T>(HttpResponseMessage msg)
        {
            var js = await msg.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<T>(js);
            return model;
        }
    }
}
