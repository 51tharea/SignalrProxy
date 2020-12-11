using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleApplication.Hubs;
using SignalrProxy.Interfaces;

namespace SampleApplication.Services
{
    public interface IUserService
    {
        Task AddUser(string userName, Guid clientId);
        Task Remove(Guid clientId);
        Task<Dictionary<Guid, string>> GetUsers(Guid clientId);
        Task addChannel(string channelName, Guid clientId);
    }

    public class UserService : IUserService
    {
        private readonly IHubConnections<SampleHub> Connections;
        private readonly SortedDictionary<Guid, string> ConnectionList = new SortedDictionary<Guid, string>();
        private readonly SortedDictionary<Guid, string> Channels = new SortedDictionary<Guid, string>();

        public UserService(IHubConnections<SampleHub> connections)
        {
            Connections = connections;
        }

        public Task AddUser(string userName, Guid clientId)
        {
            if (!ConnectionList.ContainsKey(clientId)) ConnectionList.Add(clientId, userName);


            return Task.CompletedTask;
        }

        public Task Remove(Guid clientId)
        {
            ConnectionList.Remove(clientId);
            Channels.Remove(clientId);

            return Task.CompletedTask;
        }

        public Task<Dictionary<Guid, string>> GetUsers(Guid clientId)
        {
            var result = ConnectionList.Where(p => p.Key != clientId)
                .Select(s => new KeyValuePair<Guid, string>(s.Key, s.Value))
                .ToDictionary(x => x.Key, x => x.Value);

            return Task.FromResult(result);
        }

        public Task addChannel(string channelName, Guid clientId)
        {
            if (!Channels.ContainsKey(clientId)) ConnectionList.Add(clientId, channelName);

            return Task.CompletedTask;
        }
    }
}