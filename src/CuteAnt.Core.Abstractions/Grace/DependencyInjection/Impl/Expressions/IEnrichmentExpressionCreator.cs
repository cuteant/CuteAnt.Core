﻿namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>interface for creating enrichment expressions for activation strategy</summary>
  public interface IEnrichmentExpressionCreator
  {
    /// <summary>Create enrichment expressions</summary>
    /// <param name="scope">scope for strategy</param>
    /// <param name="request">request</param>
    /// <param name="activationConfiguration">activation configuration</param>
    /// <param name="result">expression result</param>
    /// <returns></returns>
    IActivationExpressionResult CreateExpression(IInjectionScope scope, IActivationExpressionRequest request, TypeActivationConfiguration activationConfiguration, IActivationExpressionResult result);
  }
}