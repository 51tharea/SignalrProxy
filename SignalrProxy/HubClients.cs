using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SignalrProxy.Interfaces;

namespace SignalrProxy
{
    public class HubClients<THub> : IHubConnections<THub> where THub : Hub
    {
        private readonly IHubContext<THub> Context;
        private readonly IOptions<HubClientOptions> Options;
        private readonly ConcurrentDictionary<string, Guid> Connections = new ConcurrentDictionary<string, Guid>();
        private readonly ConcurrentDictionary<string, string> Channels = new ConcurrentDictionary<string, string>();
        private readonly SemaphoreSlim SendLock;

        public HubClients(IHubContext<THub> context, IOptions<HubClientOptions> options)
        {
            Context = context;
            Options = options;
            SendLock = new SemaphoreSlim(options.Value.InitialWorkerCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public Task Remove(string connectionId)
        {
            Connections.Remove(connectionId, out var clientId);

            Channels.Remove(connectionId, out var channel);

            Context.Clients.Clients(connectionId) //Client(connectionId)
                .SendAsync(channel!,
                    new
                    {
                        type = Options.Value.DisconnectType,
                        payload = new
                        {
                            time = DateTime.Now.Ticks,
                            clientId
                        }
                    });

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="clientId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public Task Push(string eventName, Guid clientId, object payload)
        {
            try
            {
                SendLock.Wait();

                var connectionId = Connections.Where(p => p.Value == clientId).Select(s => s.Key).SingleOrDefault();
                var channel = Channels.Where(p => p.Key == connectionId).Select(s => s.Value).SingleOrDefault();

                if (connectionId != null)
                {
                    Context.Clients.Client(connectionId).SendAsync(channel!, new
                    {
                        type = eventName,
                        payload
                    });
                }
            }
            finally
            {
                SendLock.Release();
            }


            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="clientId"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public Task Add(string connectionId, Guid clientId, string channel)
        {
            if (Connections.Any(p => p.Value == clientId))
            {
                var oldConnectionId = Connections.Where(p => p.Value == clientId).Select(p => p.Key).Single();

                Connections.Remove(connectionId, out _);

                Connections.TryAdd(connectionId, clientId);


                Context.Clients.Client(oldConnectionId).SendAsync(channel,
                    new
                    {
                        type = Options.Value.UserDetectedType,
                        payload = new
                        {
                            time = DateTime.Now.Ticks,
                            message = "Kullanıcı başka bir tarayıcıdan oturum açtı."
                        }
                    });

                Context.Clients.Client(connectionId)
                    .SendAsync(channel, new
                    {
                        type = Options.Value.ConnectType,
                        payload = new
                        {
                            time = DateTime.Now.Ticks,
                            clientId
                        }
                    });
            }
            else
            {
                Connections.TryAdd(connectionId, clientId);

                Channels.TryAdd(connectionId, channel);

                Context.Clients.Client(connectionId).SendAsync(channel, new {type = Options.Value.ConnectType, payload = new {time = DateTime.Now.Ticks, clientId}});
            }

            return Task.CompletedTask;
        }
    }
}