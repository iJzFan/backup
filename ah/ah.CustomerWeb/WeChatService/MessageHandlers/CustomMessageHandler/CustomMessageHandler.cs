using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ah.Services;
using ah.WeChatService.Utilities;
using System.Threading.Tasks;

namespace ah.WeChatService.MessageHandlers.CustomMessageHandler
{
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        private BaseService _service;

        public CustomMessageHandler(Stream inputStream, PostModel postModel, BaseService service, int maxRecordCount = 0)
    : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalWeixinContext.ExpireMinutes = 3。
            //WeixinContext.ExpireMinutes = 3;

            //if (!string.IsNullOrEmpty(postModel.AppId))
            //{
            //    appId = postModel.AppId;//通过第三方开放平台发送过来的请求
            //}

            _service = service;

            //在指定条件下，不使用消息去重
            base.OmitRepeatedMessageFunc = requestMessage =>
            {
                var textRequestMessage = requestMessage as RequestMessageText;
                if (textRequestMessage != null && textRequestMessage.Content == "容错")
                {
                    return false;
                }
                return true;
            };
        }

        public CustomMessageHandler(RequestMessageBase requestMessage)
            : base(requestMessage)
        {
        }

        public override void OnExecuting()
        {
            //测试MessageContext.StorageData
            if (CurrentMessageContext.StorageData == null)
            {
                CurrentMessageContext.StorageData = 0;
            }
            base.OnExecuting();
        }

        public override void OnExecuted()
        {
            base.OnExecuted();
            CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData) + 1;
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            return null;
        }

        public override async Task<IResponseMessageBase> DefaultResponseMessageAsync(IRequestMessageBase requestMessage)
        {
            /* 所有没有被处理的消息会默认返回这里的结果，
            * 因此，如果想把整个微信请求委托出去（例如需要使用分布式或从其他服务器获取请求），
            * 只需要在这里统一发出委托请求，如：
            * var responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
            * return responseMessage;
            */

            return null;
        }

    }
}
