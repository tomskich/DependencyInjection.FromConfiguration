﻿namespace DependencyInjectionFromConfig.Utils
{
    public class ServiceItem
    {
        public string Service { get; set; }
        public string Implementation { get; set; }
        public ServiceItemOptions Options { get; set; }
    }
}
