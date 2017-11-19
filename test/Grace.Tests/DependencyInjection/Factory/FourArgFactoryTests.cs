﻿using System.Collections.Generic;
using System.Linq;
using Grace.DependencyInjection;
using Grace.Tests.Classes.Simple;
using Xunit;

namespace Grace.Tests.DependencyInjection.Factory
{
    public class FourArgFactoryTests
    {
        [Fact]
        public void FactoryThreeArg_Create_Instance()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(block =>
            {
                block.ExportFactory<SimpleObjectA, SimpleObjectB, SimpleObjectC,SimpleObjectD, IEnumerable<ISimpleObject>>(
                    (a, b, c, d) => new List<ISimpleObject> { a, b, c, d });
            });

            var list = container.Locate<IEnumerable<ISimpleObject>>();

            Assert.NotNull(list);

            var array = list.ToArray();

            Assert.Equal(4, array.Length);
            Assert.IsType<SimpleObjectA>(array[0]);
            Assert.IsType<SimpleObjectB>(array[1]);
            Assert.IsType<SimpleObjectC>(array[2]);
            Assert.IsType<SimpleObjectD>(array[3]);

        }
    }
}
