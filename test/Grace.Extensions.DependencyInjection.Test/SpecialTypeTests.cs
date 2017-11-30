using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using CuteAnt;
using Grace.DependencyInjection;
//using Autofac;

namespace Grace.Extensions.DependencyInjection.Test
{
  public class AA { }

  public class SpecialTypeTests
  {
    [Fact]
    public void MsDITest()
    {
      var services = new ServiceCollection().BuildServiceProvider();
      var aa = services.GetService<AA>();
      Assert.Null(aa);
      var ts = services.GetService(typeof(TimeSpan));
      Assert.Null(ts);
      var dtoffset = services.GetService(typeof(DateTimeOffset));
      Assert.Null(dtoffset);
      var guid = services.GetService(typeof(Guid));
      Assert.Null(guid);
      var combguid = services.GetService(typeof(CombGuid));
      Assert.Null(combguid);
    }

    [Fact]
    public void GraceTest()
    {
      var services = new DependencyInjectionContainer();
      var aa = services.Locate<AA>();
      Assert.True(services.CanLocate(typeof(AA)));
      
      Assert.NotNull(aa);
      var ts = services.LocateOrDefault(typeof(TimeSpan));
      Assert.Null(ts);
      var dtoffset = services.LocateOrDefault(typeof(DateTimeOffset));
      Assert.Null(dtoffset);
      var guid = services.LocateOrDefault(typeof(Guid));
      Assert.Null(guid);
      var combguid = services.LocateOrDefault(typeof(CombGuid));
      Assert.Null(combguid);
    }

    //[Fact]
    //public void AutofacTest()
    //{
    //  var services = new ContainerBuilder().Build();
    //  var aa = services.ResolveOptional<AA>(); ;
    //  Assert.Null(aa);
    //  services.TryResolve(typeof(TimeSpan), out var ts);
    //  Assert.Null(ts);
    //  services.TryResolve(typeof(DateTimeOffset), out var dtoffset);
    //  Assert.Null(dtoffset);
    //  services.TryResolve(typeof(Guid), out var guid);
    //  Assert.Null(guid);
    //  services.TryResolve(typeof(CombGuid), out var combguid);
    //  Assert.Null(combguid);
    //}
  }
}
