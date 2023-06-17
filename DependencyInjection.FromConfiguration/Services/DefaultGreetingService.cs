namespace DependencyInjectionFromConfig.Services
{
    public class DefaultGreetingService : IGreetingService
    {
        public string SayHello()
        {
            return "Hello, NoName!";
        }
    }
}
