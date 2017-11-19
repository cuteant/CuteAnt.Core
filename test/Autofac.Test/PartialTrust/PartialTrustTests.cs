﻿#if PARTIAL_TRUST
using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using Autofac.Test.Scenarios.Graph1;
using Xunit;

namespace Autofac.Test.PartialTrust
{
    /// <summary>
    /// Fixture containing the set of tests that will execute in partial trust.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These tests are not marked with any NUnit attributes because they actually get executed
    /// through XUnit via the <see cref="PartialTrustTestExecutor"/>.
    /// Any public void method with no parameters found here will execute as a unit test.
    /// </para>
    /// </remarks>
    public class PartialTrustTests : MarshalByRefObject
    {
        public void AppDomainSetupCorrect()
        {
            // The sandbox should have the expected name and we shouldn't be able to demand unrestricted permissions.
            Assert.Equal("Sandbox", AppDomain.CurrentDomain.FriendlyName);
            Assert.Throws<SecurityException>(() => new SecurityPermission(PermissionState.Unrestricted).Demand());
        }

        public void Integration_CanCorrectlyBuildGraph1()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A1>().SingleInstance();
            builder.RegisterType<CD1>().As<IC1, ID1>().SingleInstance();
            builder.RegisterType<E1>().SingleInstance();
            builder.Register(ctr => new B1(ctr.Resolve<A1>()));

            var target = builder.Build();

            var e = target.Resolve<E1>();
            var a = target.Resolve<A1>();
            var b = target.Resolve<B1>();
            var c = target.Resolve<IC1>();
            var d = target.Resolve<ID1>();

            Assert.IsType<CD1>(c);
            var cd = (CD1)c;

            Assert.Same(a, b.A);
            Assert.Same(a, cd.A);
            Assert.NotSame(b, cd.B);
            Assert.Same(c, e.C);
            Assert.NotSame(b, e.B);
            Assert.NotSame(e.B, cd.B);
        }

        public interface I1<T>
        {
        }

        public interface I2<T>
        {
        }

        public class C<T> : I1<T>, I2<T>
        {
        }

        public void Integration_MultipleServicesOnAnOpenGenericType_ShareTheSameRegistration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(C<>)).As(typeof(I1<>), typeof(I2<>));
            var container = builder.Build();
            container.Resolve<I1<int>>();
            var count = container.ComponentRegistry.Registrations.Count();
            container.Resolve<I2<int>>();
            Assert.Equal(count, container.ComponentRegistry.Registrations.Count());
        }
    }
}
#endif
