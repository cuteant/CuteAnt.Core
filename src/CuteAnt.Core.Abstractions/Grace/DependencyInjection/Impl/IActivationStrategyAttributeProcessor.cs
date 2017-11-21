
namespace Grace.DependencyInjection.Impl
{
  /// <summary>Process attributes on activation strategy</summary>
  public interface IActivationStrategyAttributeProcessor
  {
    /// <summary>Process attribute on strategy</summary>
    /// <param name="strategy">activation strategy</param>
    void ProcessAttributeForConfigurableActivationStrategy(IConfigurableActivationStrategy strategy);
  }
}