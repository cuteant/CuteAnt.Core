
namespace Grace.DependencyInjection.Impl
{
  /// <summary>Provides activation strategy</summary>
  public interface IActivationStrategyProvider
  {
    /// <summary>Get stragey from configuration</summary>
    /// <returns></returns>
    IActivationStrategy GetStrategy();
  }
}