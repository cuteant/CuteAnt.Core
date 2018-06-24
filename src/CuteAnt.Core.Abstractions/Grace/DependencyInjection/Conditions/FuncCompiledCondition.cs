using System;

namespace Grace.DependencyInjection.Conditions
{
  /// <summary>Condition that calls a function to test if conditions are meet</summary>
  public class FuncCompiledCondition : ICompiledCondition
  {
    private readonly Func<IActivationStrategy, StaticInjectionContext, bool> _condition;

    /// <summary>Default constructor takes condition function</summary>
    /// <param name="condition">condition function</param>
    public FuncCompiledCondition(Func<IActivationStrategy, StaticInjectionContext, bool> condition)
    {
      if (null == condition) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.condition);
      _condition = condition;
    }

    /// <summary>Test if condition is meet</summary>
    /// <param name="strategy">strategy to test</param>
    /// <param name="staticInjectionContext">static injection context</param>
    /// <returns>true if condition is meet</returns>
    public bool MeetsCondition(IActivationStrategy strategy, StaticInjectionContext staticInjectionContext)
        => _condition(strategy, staticInjectionContext);
  }
}