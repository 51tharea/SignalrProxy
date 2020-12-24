using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SampleApplication.Hubs;
using SignalrProxy.Interfaces;

namespace SampleApplication.Services
{
    public interface IUserService
    {
        Task AddUser(string userName, Guid clientId);
        Task Remove(Guid clientId);
        Task<Dictionary<Guid, string>> GetUsers(Guid clientId);
        Task AddChannel(string channelName, Guid clientId);
    }

    public class UserService : IUserService
    {
        private readonly IHubConnections<SampleHub> Connections;
        private readonly ILogger<UserService> Logger;
        private readonly SortedDictionary<Guid, string> ConnectionList = new SortedDictionary<Guid, string>();
        private readonly SortedDictionary<Guid, string> Channels = new SortedDictionary<Guid, string>();

        public UserService(IHubConnections<SampleHub> connections, ILogger<UserService> logger)
        {
            Connections = connections;
            Logger = logger;
        }

        public async Task AddUser(string userName, Guid clientId)
        {
            if (!ConnectionList.ContainsKey(clientId)) ConnectionList.Add(clientId, userName);

            var users = await GetUsers(clientId);

            Logger.LogInformation("Event:CONNECTED,clientId:{clientId} payload:{@payload}", clientId, new {clientId});

            await Connections.PushAll("USER_CONNECTED", clientId, new {sessionId = Guid.NewGuid()});
        }

        public Task Remove(Guid clientId)
        {
            ConnectionList.Remove(clientId);
            
            Channels.Remove(clientId);
            
            Logger.LogInformation("Event:DISCONNECTED,clientId:{clientId} payload:{@payload}", clientId, new {clientId});
            
            Connections.PushAll("USER_DISCONNECTED", clientId, new {sessionId = Guid.NewGuid()});

            return Task.CompletedTask;
        }

        public Task<Dictionary<Guid, string>> GetUsers(Guid clientId)
        {
            var result = ConnectionList.Where(p => p.Key != clientId)
                .Select(s => new KeyValuePair<Guid, string>(s.Key, s.Value))
                .ToDictionary(x => x.Key, x => x.Value);

            return Task.FromResult(result);
        }

        public Task AddChannel(string channelName, Guid clientId)
        {
            if (!Channels.ContainsKey(clientId)) ConnectionList.Add(clientId, channelName);

            return Task.CompletedTask;
        }
    }
}