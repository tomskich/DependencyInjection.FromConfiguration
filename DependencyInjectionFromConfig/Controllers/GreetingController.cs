using System;
using DependencyInjectionFromConfig.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace NetCoreExternalDIConfig.Controllers
{
    [ApiController]
    public class GreetingController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public GreetingController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Route("Default")]
        public IActionResult Default()
        {
            var service = _serviceProvider.GetRequiredService<DefaultGreetingService>();
            return Ok(service.SayHello());
        }

        [HttpGet]
        [Route("FromConfig")]
        public IActionResult FromConfig()
        {
            var service = _serviceProvider.GetRequiredService<IGreetingService>();
            return Ok(service.SayHello());
        }
    }
}
