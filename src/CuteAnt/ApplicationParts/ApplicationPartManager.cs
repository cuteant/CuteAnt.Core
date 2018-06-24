using System;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.ApplicationParts
{
  /// <summary>Manages the parts and features of an application.</summary>
  public class ApplicationPartManager : IApplicationPartManager
  {
    private readonly List<IApplicationPart> _applicationParts = new List<IApplicationPart>();
    private readonly List<IApplicationFeatureProvider> _featureProviders = new List<IApplicationFeatureProvider>();

    /// <summary>Singleton</summary>
    public static ApplicationPartManager Singleton { get; } = new ApplicationPartManager();

    /// <summary>Gets the list of <see cref="IApplicationFeatureProvider"/>s.</summary>
    public
#if NET40
      IList
#else
      IReadOnlyList
#endif
      <IApplicationFeatureProvider> FeatureProviders => _featureProviders;

    /// <summary>Gets the list of <see cref="IApplicationPart"/>s.</summary>
    public
#if NET40
      IList
#else
      IReadOnlyList
#endif
      <IApplicationPart> ApplicationParts => _applicationParts;

    /// <summary>Adds an application part.</summary>
    /// <param name="part">The application part.</param>
    public IApplicationPartManager AddApplicationPart(IApplicationPart part)
    {
      if (!_applicationParts.Contains(part)) { _applicationParts.Add(part); }
      return this;
    }

    /// <summary>Adds a feature provider.</summary>
    /// <param name="featureProvider">The feature provider.</param>
    public IApplicationPartManager AddFeatureProvider(IApplicationFeatureProvider featureProvider)
    {
      if (!_featureProviders.Contains(featureProvider)) { _featureProviders.Add(featureProvider); }
      return this;
    }

    /// <summary>Populates the given <paramref name="feature"/> using the list of
    /// <see cref="IApplicationFeatureProvider{TFeature}"/>s configured on the
    /// <see cref="ApplicationPartManager"/>.</summary>
    /// <typeparam name="TFeature">The type of the feature.</typeparam>
    /// <param name="feature">The feature instance to populate.</param>
    public void PopulateFeature<TFeature>(TFeature feature)
    {
      if (null == feature) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.feature); }

      foreach (var provider in FeatureProviders.OfType<IApplicationFeatureProvider<TFeature>>())
      {
        provider.PopulateFeature(ApplicationParts, feature);
      }
    }
  }
}
