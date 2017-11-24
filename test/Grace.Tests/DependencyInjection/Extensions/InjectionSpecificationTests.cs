//using System;
//using Grace.DependencyInjection;
//using Grace.DependencyInjection.Extensions;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Specification;

//namespace Grace.Tests.DependencyInjection.Extensions
//{
//  public class GraceContainerTests : DependencyInjectionSpecificationTests
//  {
//    protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
//    {
//      DependencyInjectionContainer container = new DependencyInjectionContainer();

//      return container.Populate(serviceCollection);
//    }
//  }
//}