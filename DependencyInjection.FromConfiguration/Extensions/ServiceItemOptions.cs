using Microsoft.Extensions.Configuration;

namespace DependencyInjection.FromConfiguration.Extensions
{
    public class ServiceItemOptions
    {
        public string Implementation { get; set; }
        public IConfigurationSection Value { get; set; }
    }
}
