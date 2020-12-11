using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SampleApplication.Services;
using SignalrProxy.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SampleApplication.Hubs
{
    public class SampleHub : Hub
    {
        private readonly IHubConnections<SampleHub> Connections;
        private readonly IUserService UserService;

        public SampleHub(IHubConnections<SampleHub> connections, IUserService userService)
        {
            Connections = connections;
            UserService = userService;
        }

        public Task Typing(Guid clientId, Guid fromId)
        {
            return Connections.Push("TYPING", fromId, new {clientId, message = "typing..."});
        }

        public Task SendMessage(IDictionary<string, string> payload)
        {
            var fromId = Guid.Parse(payload["fromId"]);

            var clientId = Guid.Parse(payload["clientId"]);

            var message = payload["message"];

            return Connections.Push("NEW_MESSAGE", fromId, new {clientId, message});
        }

        public Task GetUsers(IDictionary<string, string> payload)
        {
            var channel = payload["channel"];

            var clientId = Guid.Parse(payload["clientId"]);

            var users = UserService.GetUsers(clientId);

            return Connections.Push("USER_LOAD_SUCCESS", clientId, users);
        }

        public Task Join(IDictionary<string, string> payload)
        {
            var username = payload["username"];

            var clientId = Guid.Parse(payload["clientId"]);

            return UserService.AddUser(username, clientId);
        }

        public Task JoinChannel(IDictionary<string, string> payload)
        {
            var channel = payload["channel"];

            var clientId = Guid.Parse(payload["clientId"]);

            return UserService.addChannel(channel, clientId);
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