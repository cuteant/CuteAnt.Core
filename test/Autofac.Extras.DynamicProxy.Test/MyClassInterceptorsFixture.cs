using Castle.DynamicProxy;
using Xunit;

namespace Autofac.Extras.DynamicProxy.Test
{
  public class MyClassInterceptorsFixture
  {
    private const int c_c = 10;
    private const int c_d = 20;

    [Fact]
    public void ClassInterceptor_AsImplementedInterfaces_AsSelf()
    {
      var builder = new ContainerBuilder();
      builder.RegisterType<D>()
             .AsImplementedInterfaces().AsSelf()
             .InstancePerDependency()
             .EnableClassInterceptors(ProxyGenerationOptions.Default)
             .InterceptedBy(typeof(AddNumInterceptor))
             ;
      builder.RegisterType<AddNumInterceptor>();
      var container = builder.Build();
      var dc = container.Resolve<D>();
      var rci = dc.GetI();
      Assert.Equal(c_d + 1, rci);
      var rcy = dc.GetY();
      Assert.Equal(c_d * 2 + 2, rcy);
      var rcz = dc.GetZ();
      Assert.Equal(c_d * 3 + 3, rcz);

      var di = container.Resolve<IHasI>();
      var ri = di.GetI();
      Assert.Equal(c_d + 1, ri);
      var dy = container.Resolve<IHasY>();
      var ry = dy.GetY();
      Assert.Equal(c_d * 2 + 2, ry);
      var dz = container.Resolve<IHasZ>();
      var rz = dz.GetZ();
      Assert.Equal(c_d * 3 + 3, rz);
    }

    [Fact]
    public void ClassInterceptor_AdditionalInterfaces()
    {
      var builder = new ContainerBuilder();
      builder.RegisterType<D>()
             .InstancePerDependency()
             .EnableClassInterceptors(ProxyGenerationOptions.Default, typeof(IHasI), typeof(IHasY), typeof(IHasZ))
             .InterceptedBy(typeof(AddNumInterceptor))
             ;
      builder.RegisterType<AddNumInterceptor>();
      var container = builder.Build();
      var dc = container.Resolve<D>();
      var rci = dc.GetI();
      Assert.Equal(c_d + 1, rci);
      var rcy = dc.GetY();
      Assert.Equal(c_d * 2 + 2, rcy);
      var rcz = dc.GetZ();
      Assert.Equal(c_d * 3 + 3, rcz);

      var di = dc as IHasI;
      var ri = di.GetI();
      Assert.Equal(c_d + 1, ri);
      var dy = dc as IHasY;
      var ry = dy.GetY();
      Assert.Equal(c_d * 2 + 2, ry);
      var dz = dc as IHasZ;
      var rz = dz.GetZ();
      Assert.Equal(c_d * 3 + 3, rz);
    }

    [Fact]
    public void ClassInterceptor_OnlyClass_InterfacesCanDo()
    {
      var builder = new ContainerBuilder();
      builder.RegisterType<D>()
             .InstancePerDependency()
             .EnableClassInterceptors()
             .InterceptedBy(typeof(AddNumInterceptor))
             ;
      builder.RegisterType<AddNumInterceptor>();
      var container = builder.Build();
      var dc = container.Resolve<D>();
      var rci = dc.GetI();
      Assert.Equal(c_d + 1, rci);
      var rcy = dc.GetY();
      Assert.Equal(c_d * 2 + 2, rcy);
      var rcz = dc.GetZ();
      Assert.Equal(c_d * 3 + 3, rcz);

      var di = dc as IHasI;
      var ri = di.GetI();
      Assert.Equal(c_d + 1, ri);
      var dy = dc as IHasY;
      var ry = dy.GetY();
      Assert.Equal(c_d * 2 + 2, ry);
      var dz = dc as IHasZ;
      var rz = dz.GetZ();
      Assert.Equal(c_d * 3 + 3, rz);
    }

    [Fact]
    public void ClassInterceptor_Class_InterfacesCanDo()
    {
      var builder = new ContainerBuilder();
      builder.RegisterType<D>()
             .InstancePerDependency()
             .EnableClassInterceptors(ProxyGenerationOptions.Default)
             .InterceptedBy(typeof(AddNumInterceptor))
             ;
      builder.RegisterType<D>().As<IHasI, IHasY, IHasZ>()
             .InstancePerDependency()
             .EnableInterfaceInterceptors()
             .InterceptedBy(typeof(AddNumInterceptor))
             ;
      builder.RegisterType<AddNumInterceptor>();
      var container = builder.Build();
      var dc = container.Resolve<D>();
      var rci = dc.GetI();
      Assert.Equal(c_d + 1, rci);
      var rcy = dc.GetY();
      Assert.Equal(c_d * 2 + 2, rcy);
      var rcz = dc.GetZ();
      Assert.Equal(c_d * 3 + 3, rcz);

      var di = container.Resolve<IHasI>();
      var ri = di.GetI();
      Assert.Equal(c_d + 1, ri);
      var dy = container.Resolve<IHasY>();
      var ry = dy.GetY();
      Assert.Equal(c_d * 2 + 2, ry);
      var dz = container.Resolve<IHasZ>();
      var rz = dz.GetZ();
      Assert.Equal(c_d * 3 + 3, rz);
    }

    [Fact]
    public void InterfaceInterceptor_AsImplementedInterfaces()
    {
      var builder = new ContainerBuilder();
      builder.RegisterType<C>()
             .AsImplementedInterfaces()
             .InstancePerDependency()
             .EnableInterfaceInterceptors()
             .InterceptedBy(typeof(AddNumInterceptor))
             ;
      builder.RegisterType<AddNumInterceptor>();
      var container = builder.Build();

      var ci = container.Resolve<IHasI>();
      var ri = ci.GetI();
      Assert.Equal(c_c + 1, ri);
      var cy = container.Resolve<IHasY>();
      var ry = cy.GetY();
      Assert.Equal(c_c * 2 + 2, ry);
    }

    public interface IHasI
    {
      int GetI();
    }

    public interface IHasY
    {
      int GetY();
    }

    public interface IHasZ
    {
      int GetZ();
    }

    public class C : IHasI, IHasY, IHasZ
    {
      public C()
      {
        I = c_c;
      }

      public int I { get; set; }

      public virtual int GetI()
      {
        return I;
      }

      public virtual int GetY()
      {
        return I * 2;
      }

      public virtual int GetZ()
      {
        return I * 3;
      }
    }

    public class D : IHasI, IHasY, IHasZ
    {
      public D()
      {
        I = c_d;
      }

      public int I { get; set; }

      public virtual int GetI()
      {
        return I;
      }

      public virtual int GetY()
      {
        return I * 2;
      }

      public virtual int GetZ()
      {
        return I * 3;
      }
    }

    private class AddNumInterceptor : IInterceptor
    {
      public void Intercept(IInvocation invocation)
      {
        invocation.Proceed();
        switch (invocation.Method.Name)
        {
          case "GetI":
            invocation.ReturnValue = 1 + (int)invocation.ReturnValue;
            break;

          case "GetY":
            invocation.ReturnValue = 2 + (int)invocation.ReturnValue;
            break;

          case "GetZ":
          default:
            invocation.ReturnValue = 3 + (int)invocation.ReturnValue;
            break;
        }
      }
    }
  }
}
