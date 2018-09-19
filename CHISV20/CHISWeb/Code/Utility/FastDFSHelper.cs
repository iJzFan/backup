using FastDFS.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CHIS.Code.Utility
{
    public class FastDFSHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileBytes">二进制文件</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        /// 
        private static string ip = Global.Config.GetSection("FastDFS:IPAdress").Value;
        private static string prot = Global.Config.GetSection("FastDFS:Port").Value;
        private static string groupName = Global.Config.GetSection("FastDFS:GroupName").Value;
        private static string domainName = Global.Config.GetSection("FastDFS:DomainName").Value;

        public static async Task<string> UploadAsync(byte[] fileBytes, string fileExt)
        {


            List<IPEndPoint> pEndPoints = new List<IPEndPoint>()
            {
                new IPEndPoint(IPAddress.Parse(ip), Convert.ToInt32(prot))
            };
            ConnectionManager.Initialize(pEndPoints);

            StorageNode storageNode = await FastDFSClient.GetStorageNodeAsync(groupName);

            string str="";

            try
            {
                 str = domainName + '/' + groupName + '/' + await FastDFSClient.UploadFileAsync(storageNode, fileBytes, fileExt);
            }catch(Exception e)
            {
                //失败则两秒后重试
                await Task.Delay(2000);

                try
                {
                    str = domainName + '/' + groupName + '/' + await FastDFSClient.UploadFileAsync(storageNode, fileBytes, fileExt);
                }
                catch (Exception ex)
                {
                    //失败则两秒后重试
                    await Task.Delay(2000);

                    str = domainName + '/' + groupName + '/' + await FastDFSClient.UploadFileAsync(storageNode, fileBytes, fileExt);
                }

            }

            return str;
        }

        public static async Task<bool> RemoveAsync(string path)
        {
            var arr = path.Split("/");

            var groupName = arr[0];

            path = path.Remove(arr[0].Length);

            try
            {
                await FastDFSClient.RemoveFileAsync(groupName, path);

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
