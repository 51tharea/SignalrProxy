using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalrProxy.Interfaces;

namespace SampleApplication.Hubs
{
    public class SampleHub : Hub
    {
        private readonly IHubConnections<SampleHub> Connections;

        public SampleHub(IHubConnections<SampleHub> connections)
        {
            Connections = connections;
        }

        public Task Typing(Guid clientId, Guid fromId)
        {
            return Connections.Push("TYPING", fromId, new {clientId, message = "typing..."});
        }

        public Task SendMessage(Guid clientId, Guid fromId, string message)
        {
            return Connections.Push("NEW_MESSAGE", fromId, new {clientId, message});
        }

        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var hasToken = httpContext.Items.TryGetValue("access_token", out var access_Token);

            var hasUserId = httpContext.Items.TryGetValue("userId", out var userId);

            var hasChannel = httpContext.Items.TryGetValue("channel", out var channel);

            if (!hasToken || !hasUserId || !hasChannel) return Task.CompletedTask;

            //passed access token

            var clientId = Guid.Parse(userId.ToString()!);

            Connections.Add(Context.ConnectionId, clientId, channel.ToString());

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception ex)
        {
            var httpContext = Context.GetHttpContext();

            var connectionId = Context.ConnectionId;

            Connections.Remove(connectionId);

            return Task.CompletedTask;
        }
    }
}