using ah.Areas.Customer.Controllers;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ah.Areas.Customer.Controllers.Base
{
    public class BaseCBL
    {
        public BaseController controller = null;
        public ah.DbContext.AHMSEntitiesSqlServer MainDbContext { get { return controller.MainDbContext; } }
        public BaseCBL(BaseController c)
        {
            this.controller = c;
        }

    }

    public class SMS
    {
        public string uid = "114";
        public string username = "tsjk";
        public string password = "3D9188577CC9BFE9291AC66B5CC872B7";
        /// <summary> 发送短信，得到返回值</summary>
        /// string strRet = null;
        /// <summary> 发送短信，得到返回值</summary>
        public async Task<string> PostSmsInfo(string mobile, string txt)
        {
            var pms = $"uid={uid}&username={username}&password={password}&mobiles={mobile}&content={txt}";
            var code=await Codes.Utility.WebHelper.PostMoth("http://v.369sms.com/SMS/SendSingleSMS", pms, Encoding.UTF8);
            return code;
        }
    }

    public class EmailHelper
    {
        const string FROM = "service@mydt.cn";//发送人的邮箱和用户名
        public string FromAccount = "天使健康管理有限公司";
        const string PASSWORD = "Tt212323";
        const int PORTSSH = 465;
        const int PORT = 25;
        const bool EMAIL_USE_SSL = true;
        const string HOST = "smtp.mydt.cn";
        const string path = @"E:\GitHub\TestMailClient\NetSmtpClient\.NETFoundation.png";

        /// <summary>
        /// 简单邮件的发送
        /// </summary>
        /// <param name="mailto">发送人的地址</param>
        /// <param name="message"></param>
        /// <param name="subject"></param>
        public void SendEmail(string mailto, string message, string subject)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(FromAccount, FROM));
            emailMessage.To.Add(new MailboxAddress("mail", mailto));
            emailMessage.Subject = string.Format(subject);
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(HOST, PORT, false);
                client.AuthenticationMechanisms.Remove("XALIOAUTH");//取消验证
                client.Authenticate(FROM, PASSWORD);
                client.Send(emailMessage);
                client.Disconnect(true);

            }

        }
        /// <summary>
        /// 异步发送邮件
        /// </summary>
        /// <param name="mailto"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task SendEmailAsync(string mailto, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(FromAccount, FROM));
            emailMessage.To.Add(new MailboxAddress("mail", mailto));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(HOST, PORT, MailKit.Security.SecureSocketOptions.None).ConfigureAwait(false);
                await client.AuthenticateAsync(FROM, PASSWORD);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);

            }
        }


        /// <summary>
        /// 异步发送邮件 html
        /// </summary>
        /// <param name="mailto"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task SendEmailHtmlAsync(string mailto, string subject, string htmlContent)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(FromAccount, FROM));
            emailMessage.To.Add(new MailboxAddress("mail", mailto));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = htmlContent };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(HOST, PORT, MailKit.Security.SecureSocketOptions.None).ConfigureAwait(false);
                await client.AuthenticateAsync(FROM, PASSWORD);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);

            }
        }
    }

}
