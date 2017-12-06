using System.Collections.Generic;

namespace CuteAnt.ApplicationParts
{
  /// <summary>Builder for configuring application parts.</summary>
  public interface IApplicationPartManager
  {
    /// <summary>Gets the list of <see cref="IApplicationFeatureProvider"/> s.</summary>
#if NET40
    IList
#else
    IReadOnlyList
#endif
      <IApplicationFeatureProvider> FeatureProviders { get; }

    /// <summary>Gets the list of <see cref="IApplicationPart"/> s.</summary>
#if NET40
    IList
#else
    IReadOnlyList
#endif
      <IApplicationPart> ApplicationParts { get; }

    /// <summary>Adds an application part.</summary>
    /// <param name="part">The application part.</param>
    IApplicationPartManager AddApplicationPart(IApplicationPart part);

    /// <summary>Adds a feature provider.</summary>
    /// <param name="featureProvider">The feature provider.</param>
    IApplicationPartManager AddFeatureProvider(IApplicationFeatureProvider featureProvider);

    /// <summary>Populates the given <paramref name="feature"/> using the list of <see
    /// cref="IApplicationFeatureProvider{TFeature}"/> s configured on the <see cref="IApplicationPartManager"/>.</summary>
    /// <typeparam name="TFeature">The type of the feature.</typeparam>
    /// <param name="feature">The feature instance to populate.</param>
    void PopulateFeature<TFeature>(TFeature feature);
  }
}