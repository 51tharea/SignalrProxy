using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SampleApplication
{
    public class ChatMiddleware
    {
        private readonly RequestDelegate Next;

        public ChatMiddleware(RequestDelegate next)
        {
            Next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;

            // web sockets cannot pass headers so we must take the access token from query param and
            // add it to the header before authentication middleware runs
            if (request.Path.StartsWithSegments("/chat", StringComparison.OrdinalIgnoreCase)
                && request.Query.TryGetValue("access_token", out var accessToken)
                && request.Query.TryGetValue("userId", out var userId)
                && request.Query.TryGetValue("channel", out var channel))
            {
                request.Headers.Add("Access-Token", $"{accessToken}");
                request.Headers.Add("User-Id", $"{userId}");

                httpContext.Items.Add("access_token", accessToken);
                httpContext.Items.Add("userId", userId);
                httpContext.Items.Add("channel", channel);
            }

            await Next(httpContext);
        }
    }
}