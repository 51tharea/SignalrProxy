using System;
using System.Threading.Tasks;
using SampleApplication.Hubs;
using SampleApplication.Requests;
using SampleApplication.Responses;
using SignalrProxy.Interfaces;

namespace SampleApplication.Services
{
    public interface ISampleService
    {
        Task<(bool, ServiceResponse)> GetId(GetUserDetailRequest request);
    }

    public class SampleService : ISampleService
    {
        private readonly IHubConnections<SampleHub> Connections;

        public SampleService(IHubConnections<SampleHub> connections)
        {
            Connections = connections;
        }
        
        public Task<(bool, ServiceResponse)> GetId(GetUserDetailRequest request)
        {
            ServiceResponse response = new ServiceResponse();
             
            Connections.Push("GET_USER_DETAIL", request.Id, new {Id = request.Id, Name = "Test"});

            response.Status = true;

            response.Data = new
            {
                Id = request.Id,
                Name = "Test",
            };

            (bool, ServiceResponse) result = (true, response);

            return Task.FromResult(result);
        }
    }
}