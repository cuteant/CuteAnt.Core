﻿using System;
using Grace.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Grace.Tests.DependencyInjection.Extensions
{
    /// <summary>
    /// These tests are from microsoft to make sure it conforms to it's container specifications
    /// </summary>
    public class InjectionSpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var container = new DependencyInjectionContainer();

            return container.Populate(serviceCollection);
        }
    }
}
