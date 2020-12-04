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
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly ISampleService SampleService;
        public TestController(ILogger<TestController> logger, ISampleService sampleService)
        {
            _logger = logger;
            SampleService = sampleService;
        }
         
        [HttpGet]
        public async Task<ServiceResponse> Get([FromQuery] GetUserDetailRequest request)
        {
            (var isOk, ServiceResponse response) = await SampleService.GetId(request);

            return response;
        }
    }
}