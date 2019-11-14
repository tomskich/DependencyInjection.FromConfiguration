using Microsoft.Extensions.Configuration;

namespace DependencyInjectionFromConfig.Utils
{
    public class ServiceItemOptions
    {
        public string Implementation { get; set; }
        public IConfigurationSection Value { get; set; }
    }
}
