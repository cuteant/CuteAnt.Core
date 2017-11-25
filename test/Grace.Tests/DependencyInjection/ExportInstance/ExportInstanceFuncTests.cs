using Grace.DependencyInjection;
using Grace.Tests.Classes.Simple;
using Xunit;

namespace Grace.Tests.DependencyInjection.ExportInstance
{
  public class ExportInstanceFuncTests
  {
    [Fact]
    public void ExportInstance_Func_Create_Instance()
    {
      var container = new DependencyInjectionContainer();

      container.Configure(c =>
      {
#pragma warning disable 0618
        c.ExportInstance(() => new BasicService { Count = 5 }).As<IBasicService>();
#pragma warning restore 0618
        c.Export<ConstructorImportService>().As<IConstructorImportService>();
      });

      var service = container.Locate<IConstructorImportService>();

      Assert.NotNull(service);
      Assert.Equal(5, service.BasicService.Count);
    }

    [Fact]
    public void ExportInstance_Func_With_Singleton_Lifestyle()
    {
      var container = new DependencyInjectionContainer();

#pragma warning disable 0618
      container.Configure(c => c.ExportInstance<IBasicService>(() => new BasicService { Count = 5 }).Lifestyle.Singleton());
#pragma warning restore 0618

      var instance1 = container.Locate<IBasicService>();
      var instance2 = container.Locate<IBasicService>();

      Assert.Same(instance1, instance2);
    }
  }
}
