using System;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.ApplicationParts
{
  /// <summary>Manages the parts and features of an application.</summary>
  public class ApplicationPartManager
  {
    private readonly List<IApplicationPart> _applicationParts = new List<IApplicationPart>();
    private readonly List<IApplicationFeatureProvider> _featureProviders = new List<IApplicationFeatureProvider>();

    public static ApplicationPartManager Singleton { get; } = new ApplicationPartManager();

    /// <summary>Gets the list of <see cref="IApplicationFeatureProvider"/>s.</summary>
    public
#if NET40
      IList<IApplicationFeatureProvider> 
#else
      IReadOnlyList<IApplicationFeatureProvider>
#endif
      FeatureProviders => _featureProviders;

    /// <summary>Gets the list of <see cref="IApplicationPart"/>s.</summary>
    public
#if NET40
      IList<IApplicationPart> 
#else
      IReadOnlyList<IApplicationPart>
#endif
      ApplicationParts => _applicationParts;

    /// <summary>Adds an application part.</summary>
    /// <param name="part">The application part.</param>
    public void AddApplicationPart(IApplicationPart part)
    {
      if (!_applicationParts.Contains(part)) { _applicationParts.Add(part); }
    }

    /// <summary>Adds a feature provider.</summary>
    /// <param name="featureProvider">The feature provider.</param>
    public void AddFeatureProvider(IApplicationFeatureProvider featureProvider)
    {
      if (!_featureProviders.Contains(featureProvider)) { _featureProviders.Add(featureProvider); }
    }

    /// <summary>Populates the given <paramref name="feature"/> using the list of
    /// <see cref="IApplicationFeatureProvider{TFeature}"/>s configured on the
    /// <see cref="ApplicationPartManager"/>.</summary>
    /// <typeparam name="TFeature">The type of the feature.</typeparam>
    /// <param name="feature">The feature instance to populate.</param>
    public void PopulateFeature<TFeature>(TFeature feature)
    {
      if (feature == null) { throw new ArgumentNullException(nameof(feature)); }

      foreach (var provider in FeatureProviders.OfType<IApplicationFeatureProvider<TFeature>>())
      {
        provider.PopulateFeature(ApplicationParts, feature);
      }
    }
  }
}
