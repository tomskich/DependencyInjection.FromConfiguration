using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.FromConfiguration.Extensions
{
    public class ServiceItem
    {
        public string Service { get; set; }
        public string Implementation { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public ServiceItemOptions Options { get; set; }
    }
}
