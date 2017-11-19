using System;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Constructor parameter configuration</summary>
  /// <typeparam name="TParam"></typeparam>
  public class FluentDecoratorWithCtorConfiguration<TParam> : ProxyFluentDecoratorStrategyConfiguration, IFluentDecoratorWithCtorConfiguration<TParam>
  {
    private readonly ConstructorParameterInfo _constructorParameterInfo;

    /// <summary>Default constructor</summary>
    /// <param name="configuration"></param>
    /// <param name="constructorParameterInfo"></param>
    public FluentDecoratorWithCtorConfiguration(IFluentDecoratorStrategyConfiguration configuration, ConstructorParameterInfo constructorParameterInfo)
      : base(configuration) => _constructorParameterInfo = constructorParameterInfo;

    /// <summary>
    /// Applies a filter to be used when resolving a parameter constructor It will be called each
    /// time the parameter is resolved
    /// </summary>
    /// <param name="filter">filter delegate to be used when resolving parameter</param>
    /// <returns>configuration object</returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> Consider(ActivationStrategyFilter filter)
    {
      _constructorParameterInfo.ExportStrategyFilter = filter ?? throw new ArgumentNullException(nameof(filter));

      return this;
    }

    /// <summary>Assign a default value if no better option is found</summary>
    /// <param name="defaultValue">default value</param>
    /// <returns>configuration object</returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> DefaultValue(TParam defaultValue)
    {
      _constructorParameterInfo.DefaultValue = defaultValue;

      return this;
    }

    /// <summary>Default value func</summary>
    /// <param name="defaultValueFunc"></param>
    /// <returns></returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> DefaultValue(Func<TParam> defaultValueFunc)
    {
      _constructorParameterInfo.DefaultValue = defaultValueFunc ?? throw new ArgumentNullException(nameof(defaultValueFunc));

      return this;
    }

    /// <summary>Default value func</summary>
    /// <param name="defaultValueFunc"></param>
    /// <returns></returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> DefaultValue(Func<IExportLocatorScope, StaticInjectionContext, IInjectionContext, TParam> defaultValueFunc)
    {
      _constructorParameterInfo.DefaultValue = defaultValueFunc ?? throw new ArgumentNullException(nameof(defaultValueFunc));

      return this;
    }

    /// <summary>Mark the parameter as dynamic</summary>
    /// <param name="isDynamic"></param>
    /// <returns>configuration object</returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> IsDynamic(bool isDynamic = true)
    {
      _constructorParameterInfo.IsDynamic = isDynamic;

      return this;
    }

    /// <summary>Is the parameter required when resolving the type</summary>
    /// <param name="isRequired">is the parameter required</param>
    /// <returns>configuration object</returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> IsRequired(bool isRequired = true)
    {
      _constructorParameterInfo.IsRequired = isRequired;

      return this;
    }

    /// <summary>Locate with a particular key</summary>
    /// <param name="locateKey">ocate key</param>
    /// <returns>configuration object</returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> LocateWithKey(object locateKey)
    {
      _constructorParameterInfo.LocateWithKey = locateKey ?? throw new ArgumentNullException(nameof(locateKey));

      return this;
    }

    /// <summary>Name of the parameter being configured</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> Named(string name)
    {
      _constructorParameterInfo.ParameterName = name ?? throw new ArgumentNullException(nameof(name));

      return this;
    }

    /// <summary>Use a specific type for parameter</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IFluentDecoratorWithCtorConfiguration<TParam> Use(Type type)
    {
      _constructorParameterInfo.UseType = type ?? throw new ArgumentNullException(nameof(type));

      return this;
    }
  }
}