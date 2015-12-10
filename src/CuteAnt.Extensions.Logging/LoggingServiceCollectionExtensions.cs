// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CuteAnt.Extensions.Logging;
using CuteAnt.Extensions.DependencyInjection.Extensions;

namespace CuteAnt.Extensions.DependencyInjection
{
    public static class LoggingServiceCollectionExtensions
    {
        public static IServiceCollection AddLogging(this IServiceCollection services)
        {
            services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
            return services;
        }
    }
}