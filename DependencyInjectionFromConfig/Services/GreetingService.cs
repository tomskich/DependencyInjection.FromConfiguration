using Microsoft.Extensions.Options;

namespace DependencyInjectionFromConfig.Services
{
    public class GreetingService : IGreetingService
    {
        public GreetingService(IOptions<GreetingServiceOptions> options)
        {
            Options = options.Value;
        }

        public GreetingServiceOptions Options { get; }

        public string SayHello()
        {
            return $"Hello, {Options.Name}!";
        }
    }
}
