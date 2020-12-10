using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleApplication.Hubs;
using SampleApplication.Requests;
using SampleApplication.Responses;
using SampleApplication.Services;
using SignalrProxy.Interfaces;

namespace SampleApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IUserService UserService;
        private readonly ILogger<ChatController> logger;

        public ChatController(ILogger<TestController> logger, IUserService userService)
        {
            UserService = userService;
            logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> Join([FromBody] IDictionary<string, string> request)
        {
            var hasClient = request.TryGetValue("clientId", out var clientId);

            if (!hasClient) return BadRequest(request);

            var userName = request["username"].ToString();

            await UserService.AddUser(userName, Guid.Parse(clientId));

            return Ok(request);
        }

        [HttpGet("{GUID:clientId}")]
        public async Task<ActionResult> Users([FromRoute] Guid clientId)
        {
             var users= await UserService.GetUsers(clientId);

            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult> Logout([FromBody] IDictionary<string, string> request)
        {
            var hasClient = request.TryGetValue("clientId", out var clientId);

            if (!hasClient) return BadRequest(request);
      
            await UserService.Remove(Guid.Parse(clientId));

            return Ok(request);
        }
    }
}