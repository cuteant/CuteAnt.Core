using Grace.DependencyInjection;
using Grace.Tests.Classes.Simple;
using Xunit;

namespace Grace.Tests.DependencyInjection.ExportInstance
{
    public class ExportInstanceFuncWithStaticContextTests
    {
        [Fact]
        public void ExportInstance_With_StaticContext()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
#pragma warning disable 0618
                c.ExportInstance((scope, staticContext) => new BasicService { Count = 5 }).As<IBasicService>();
#pragma warning restore 0618
                c.Export<ConstructorImportService>().As<IConstructorImportService>();
            });

            var service = container.Locate<IConstructorImportService>();

            Assert.NotNull(service);
            Assert.Equal(5, service.BasicService.Count);
        }
    }
}
