﻿using System;
using Grace.DependencyInjection;
using Grace.Dynamic;
using Grace.Tests.Classes.Simple;
using Xunit;

namespace Grace.Tests.Dynamic
{
    public class DynamicScopedTests
    {
        public class DisposableDependent : IDisposable
        {
            public DisposableDependent(IBasicService basicService)
            {
                BasicService = basicService;
            }

            public IBasicService BasicService { get; }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
                
            }
        }

        [Fact]
        public void Scoped_DisposalTest()
        {
            var container = new DependencyInjectionContainer(GraceDynamicMethod.Configuration(c =>
            {
                c.Trace = s => Assert.DoesNotContain("falling back", s);
            }));

            container.Configure(c => c.Export<BasicService>().As<IBasicService>().Lifestyle.SingletonPerScope());

            var value = container.Locate<DisposableDependent>();

            using (var scope = container.BeginLifetimeScope())
            {
                value = scope.Locate<DisposableDependent>();
            }
        }
        
        [Fact]
        public void DynamicMethod_Per_Scope()
        {
            var container = new DependencyInjectionContainer(GraceDynamicMethod.Configuration(c =>
            {
                c.Trace = s => Assert.DoesNotContain("falling back", s);
            }));

            container.Configure(c => c.Export<BasicService>().As<IBasicService>().Lifestyle.SingletonPerScope());

            var value = container.Locate<DisposableDependent>();

            using (var scope = container.BeginLifetimeScope())
            {
                value = scope.Locate<DisposableDependent>();
            }
        }
    }
}
