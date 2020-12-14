using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SampleApplication.Services;
using SignalrProxy.Interfaces;

namespace SampleApplication.Hubs
{
    public class SampleHub : Hub
    {
        private readonly ILogger<SampleHub> Logger;
        private readonly IHubConnections<SampleHub> Connections;
        private readonly IUserService UserService;

        public SampleHub(ILogger<SampleHub> logger, IHubConnections<SampleHub> connections, IUserService userService)
        {
            Logger = logger;
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

            Logger.LogInformation("Event:NEW_MESSAGE,clientId:{clientId} payload:{@payload}", fromId, new {clientId, message});

            return Connections.Push("NEW_MESSAGE", fromId, new {clientId, message});
        }

        [HubMethodName("getUserList")]
        public Task GetUserList(IDictionary<string, string> payload)
        {
            var clientId = Guid.Parse(payload["clientId"]);

            var users = UserService.GetUsers(clientId);
            
            Logger.LogInformation("Event:USER_LOAD_SUCCESS,clientId:{clientId} payload:{@payload}", clientId, payload);

            return Connections.Push("USER_LOAD_SUCCESS", clientId, users.Result.Select(p => new {clientId = p.Key, username = p.Value}).ToList());
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

            return UserService.AddChannel(channel, clientId);
        }

        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var hasToken = httpContext.Items.TryGetValue("access_token", out var access_Token);

            var hasUserId = httpContext.Items.TryGetValue("userId", out var userId);

            var hasChannel = httpContext.Items.TryGetValue("channel", out var channel);

            if (!hasToken || !hasUserId || !hasChannel) return Task.CompletedTask;

            //passed access token

            var clientId = Guid.Parse(userId?.ToString()!);

            Connections.Add(Context.ConnectionId, clientId, channel?.ToString());

            Logger.LogInformation("Event:CONNECTED,clientId:{clientId} payload:{@payload}", clientId, new {clientId});


            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception ex)
        {
            var httpContext = Context.GetHttpContext();

            var connectionId = Context.ConnectionId;

            var connection = Connections.GetUserId(connectionId);

            Connections.Remove(connectionId);

            UserService.Remove(connection.Result.Value);

            var user = UserService.GetUsers(connection.Result.Value);

            Logger.LogInformation("Event:DISCONNECTED,clientId:{clientId} payload:{@payload}", connection.Result.Value, user.Result);

            return Connections.Push("USER_DISCONNECTED", connection.Result.Value, new {clientId = connection.Result.Value, time = DateTime.Now.Hour.ToString("HH:mm:ss")});

            return Task.CompletedTask;
        }
    }
}