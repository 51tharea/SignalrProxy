using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalrProxy.Interfaces
{
    public interface IHubConnections<THub>
    {
        public Task Add(string connectionId, Guid clientId, string channel);
        public Task Remove(string connectionId);
        Task Push(string eventName, Guid clientId, object payload);
        Task PushAll(string eventName, Guid clientId, object payload);
        Task<KeyValuePair<string, Guid>> GetUserId(string connectionId);

        ConcurrentDictionary<string, Guid> GetClients { get; }
    }
}