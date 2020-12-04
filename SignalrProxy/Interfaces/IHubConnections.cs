using System;
using System.Threading.Tasks;

namespace SignalrProxy.Interfaces
{
    public interface IHubConnections<THub>
    {
        public Task Add(string connectionId, Guid clientId, string channel);
        public Task Remove(string connectionId);

        Task Push(string eventName, Guid clientId, object payload);
    }
}