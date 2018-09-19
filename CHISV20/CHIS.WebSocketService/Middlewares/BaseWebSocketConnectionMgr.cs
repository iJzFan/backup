using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CHIS.WebSocketService.Middlewares
{
    public abstract class BaseWebSocketConnectionMgr<T>
    {

        internal static ConcurrentDictionary<string, T> _socketConcurrentDictionary = new ConcurrentDictionary<string, T>();


        /// <summary>
        /// 监控器内的监控数量
        /// </summary>
        public int MonitorSocketCount
        {
            get { return _socketConcurrentDictionary.Count; }
        }

        public async Task AddSocket(T socket)
        {
            var key = GetId(socket);
            if (_socketConcurrentDictionary.ContainsKey(key))
            {
                var oldSocket = GetItem(key);
                await RemoveSocket(oldSocket);
                T removeItem = default(T);
                _socketConcurrentDictionary.Remove(key, out removeItem);
                oldSocket = default(T);//设置为空
                removeItem = default(T);
            }
            //添加新的socket
            _socketConcurrentDictionary.TryAdd(key, socket);
        }

        /// <summary>
        /// 默认为Guid，N格式
        /// </summary>
        internal virtual string GetId(T socket)
        {
            return Guid.NewGuid().ToString("N");
        }

        public T GetItem(string key)
        {
            return _socketConcurrentDictionary[key];
        }

        /// <summary>
        /// 移除WebSocket，并注意关闭
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public async Task RemoveSocket(T socket)
        {
            _socketConcurrentDictionary.TryRemove(GetSocketId(socket), out T aSocket);
            if (aSocket is BaseWebSocket)
            {
                var bSocket = aSocket as BaseWebSocket;
                await bSocket.WebSocket.CloseAsync(
                    closeStatus: WebSocketCloseStatus.NormalClosure,
                    statusDescription: "连接关闭",
                    cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
        }


        public string GetSocketId(T socket)
        {
            return _socketConcurrentDictionary.FirstOrDefault(k => k.Value.Equals(socket)).Key;
        }


        public ConcurrentDictionary<string, T> GetAll()
        {
            return _socketConcurrentDictionary;
        }

        public bool ContainsKey(string key)
        {
            return _socketConcurrentDictionary.ContainsKey(key);
        }
    }

}
