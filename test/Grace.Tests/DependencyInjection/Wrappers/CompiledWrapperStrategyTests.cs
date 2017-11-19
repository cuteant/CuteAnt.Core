﻿using System.Collections.Generic;
using System.Linq;
using Grace.DependencyInjection;
using Grace.Tests.Classes.Simple;
using Xunit;

namespace Grace.Tests.DependencyInjection.Wrappers
{
    public class CompiledWrapperStrategyTests
    {
        [Fact]
        public void ExportWrapper_Non_Generic()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.Export<MultipleService1>().As<IMultipleService>();
                c.ExportWrapper(typeof(ImportOneInstanceOfMultipleService))
                    .As(typeof(IImportOneInstanceOfMultipleService))
                    .WrappedType(typeof(IMultipleService));

            });

            var instance = container.Locate<IImportOneInstanceOfMultipleService>();

            Assert.NotNull(instance);
            Assert.IsType<MultipleService1>(instance.MultipleService);
        }

        [Fact]
        public void ExportWrapper_Non_Generic_Enumerable()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.Export<MultipleService1>().As<IMultipleService>();
                c.Export<MultipleService2>().As<IMultipleService>();
                c.Export<MultipleService3>().As<IMultipleService>();
                c.Export<MultipleService4>().As<IMultipleService>();
                c.Export<MultipleService5>().As<IMultipleService>();
                c.ExportWrapper(typeof(ImportOneInstanceOfMultipleService))
                    .As(typeof(IImportOneInstanceOfMultipleService))
                    .WrappedType(typeof(IMultipleService));
            });

            var enumerable = container.Locate<IEnumerable<IImportOneInstanceOfMultipleService>>();

            Assert.NotNull(enumerable);

            var array = enumerable.ToArray();

            Assert.Equal(5, array.Length);
            Assert.IsType<MultipleService1>(array[0].MultipleService);
            Assert.IsType<MultipleService2>(array[1].MultipleService);
            Assert.IsType<MultipleService3>(array[2].MultipleService);
            Assert.IsType<MultipleService4>(array[3].MultipleService);
            Assert.IsType<MultipleService5>(array[4].MultipleService);
        }

        [Fact]
        public void ExportWrapper_Generic()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.Export<MultipleService1>().As<IMultipleService>();
                c.ExportWrapper(typeof(DependentService<>)).As(typeof(IDependentService<>)).WrappedGenericArg(0);
            });

            var instance = container.Locate<IDependentService<IMultipleService>>();

            Assert.NotNull(instance);
            Assert.IsType<MultipleService1>(instance.Value);
        }
        
        [Fact]
        public void ExportWrapper_Generic_Enumerable()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.Export<MultipleService1>().As<IMultipleService>();
                c.Export<MultipleService2>().As<IMultipleService>();
                c.Export<MultipleService3>().As<IMultipleService>();
                c.Export<MultipleService4>().As<IMultipleService>();
                c.Export<MultipleService5>().As<IMultipleService>();
                c.ExportWrapper(typeof(DependentService<>)).As(typeof(IDependentService<>)).WrappedGenericArg(0);
            });

            var instance = container.Locate<IEnumerable<IDependentService<IMultipleService>>>();

            Assert.NotNull(instance);

            var array = instance.ToArray();
            
            Assert.Equal(5, array.Length);
            Assert.IsType<MultipleService1>(array[0].Value);
            Assert.IsType<MultipleService2>(array[1].Value);
            Assert.IsType<MultipleService3>(array[2].Value);
            Assert.IsType<MultipleService4>(array[3].Value);
            Assert.IsType<MultipleService5>(array[4].Value);

        }

        public class DependentClassA
        {
            public DependentClassA(IDependentService<IBasicService> service)
            {
                Service = service;
            }

            public IDependentService<IBasicService> Service { get; }
        }

        public class DependentClassB
        {
            public DependentClassB(IDependentService<IBasicService> service)
            {
                Service = service;
            }

            public IDependentService<IBasicService> Service { get; }
        }

        [Fact]
        public void ExportWrapper_With_Conditions()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.ExportWrapper(typeof(DependentService<>))
                    .As(typeof(IDependentService<>))
                    .When.InjectedInto(TypesThat.EndWith("A"));

                c.ExportWrapper(typeof(OtherDependentService<>))
                    .As(typeof(IDependentService<>))
                    .When.InjectedInto(TypesThat.EndWith("B"));

                c.Export<BasicService>().As<IBasicService>();
            });

            var instanceA = container.Locate<DependentClassA>();

            Assert.NotNull(instanceA);
            Assert.NotNull(instanceA.Service);
            Assert.NotNull(instanceA.Service.Value);
            Assert.IsType<DependentService<IBasicService>>(instanceA.Service);

            var instanceB = container.Locate<DependentClassB>();

            Assert.NotNull(instanceB);
            Assert.NotNull(instanceB.Service);
            Assert.NotNull(instanceB.Service.Value);
            Assert.IsType<OtherDependentService<IBasicService>>(instanceB.Service);
        }
    }
}
