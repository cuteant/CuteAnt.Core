using System;
using System.Linq;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Impl.InstanceStrategies;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace Grace.Tests.DependencyInjection.ExportInstance
{
  public class BaseInstanceTests
  {
    [Theory]
    [AutoData]
    [SubFixtureInitialize]
    public void BaseInstanceExportStrategy_AddSecondaryStrategy(ConstantInstanceExportStrategy<int> strategy, ICompiledExportStrategy addStrategy)
    {
      strategy.AddSecondaryStrategy(addStrategy);

      var array = strategy.SecondaryStrategies().ToArray();

      Assert.Single(array);
      Assert.Same(addStrategy, array[0]);
    }

    [Theory]
    [AutoData]
    [SubFixtureInitialize]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    public void BaseInstanceExportStrategy_AddSecondaryStrategy_Null(ConstantInstanceExportStrategy<int> strategy, ICompiledExportStrategy addStrategy)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    {
      Assert.Throws<ArgumentNullException>(() => strategy.AddSecondaryStrategy(null));
    }
  }
}
