using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleApplication.Services;

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

        [HttpPost("/join")]
        public async Task<ActionResult> Join([FromBody] IDictionary<string, string> request)
        {
            var hasClient = request.TryGetValue("clientId", out var clientId);

            if (!hasClient) return BadRequest(request);

            var userName = request["username"].ToString();

            await UserService.AddUser(userName, Guid.Parse(clientId));

            return Ok(request);
        }

        [HttpGet("{clientId:GUID}")]
        public async Task<ActionResult> Users([FromRoute] Guid clientId)
        {
            var users = await UserService.GetUsers(clientId);

            return Ok(users);
        }

        [HttpPost("/logout")]
        public async Task<ActionResult> Logout([FromBody] IDictionary<string, string> request)
        {
            var hasClient = request.TryGetValue("clientId", out var clientId);

            if (!hasClient) return BadRequest(request);

            await UserService.Remove(Guid.Parse(clientId));

            return Ok(request);
        }
    }
}