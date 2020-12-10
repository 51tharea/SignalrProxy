using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApplication.Services
{
    public interface IUserService
    {
        Task AddUser(string userName, Guid clientId);
        Task Remove(Guid clientId);
        Task<Dictionary<Guid, string>> GetUsers(Guid clientId);
    }

    public class UserService : IUserService
    {
        private readonly SortedDictionary<Guid, string> ConnectionList = new SortedDictionary<Guid, string>();

        public UserService() { }

        public Task AddUser(string userName, Guid clientId)
        {
            if (!ConnectionList.ContainsKey(clientId)) ConnectionList.Add(clientId, userName);

            return Task.CompletedTask;
        }

        public Task Remove(Guid clientId)
        {
            ConnectionList.Remove(clientId);

            return Task.CompletedTask;
        }

        public Task<Dictionary<Guid, string>> GetUsers(Guid clientId)
        {
            var result = ConnectionList.Where(p => p.Key != clientId)
                                       .Select(s => new KeyValuePair<Guid, string>(s.Key, s.Value))
                                       .ToDictionary(x => x.Key, x => x.Value);
           
            return Task.FromResult(result);
        }
    }
}