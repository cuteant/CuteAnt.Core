﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;

namespace DotNet.DependencyInjectionBenchmarks.Containers
{
	public class CastleWindsorContainer : IContainer
	{
		private WindsorContainer _container;
		private IKernel _kernel;

		public string DisplayName => "Castle Windsor";

		public string Version => typeof(WindsorContainer).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";

		public string WebSite => "https://github.com/castleproject/Windsor";

		public CastleWindsorContainer()
		{
			_container = new WindsorContainer();
			_kernel = _container.Kernel;
            
			_kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel, true));
			_container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());
		}

		public void BuildContainer()
		{
		}

		public class CastleWindsorScope : IResolveScope
		{
			private readonly WindsorContainer _container;
			private IDisposable _disposable;

			public CastleWindsorScope(WindsorContainer container)
			{
				_container = container;
				_disposable = container.BeginScope();
			}

			public void Dispose()
			{
				_disposable.Dispose();
			}

			public IResolveScope CreateScope(string scopeName = "")
			{
				return new CastleWindsorScope(_container);
			}

			public object Resolve(Type type)
			{
				return _container.Kernel.Resolve(type);
			}

			public object Resolve(Type type, object data)
			{
				return _container.Kernel.Resolve(type, data);
			}

			public bool TryResolve(Type type, object data, out object value)
			{
				throw new NotImplementedException();
			}
		}

		public IResolveScope CreateScope(string scopeName = "")
		{
			return new CastleWindsorScope(_container);
		}

		public void Dispose()
		{
			_container.Dispose();
		}

		public void RegisterFactory<TResult>(Func<TResult> factory, RegistrationMode mode, RegistrationLifestyle lifestyle) where TResult : class
		{
			switch (lifestyle)
			{
				case RegistrationLifestyle.Singleton:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(factory).LifeStyle.Singleton);
					break;
				case RegistrationLifestyle.SingletonPerScope:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(factory)
						.LifeStyle.Scoped());
					break;
				case RegistrationLifestyle.Transient:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(factory).LifeStyle.Transient);
					break;
			}
		}

		public void RegisterFactory<T1, TResult>(Func<T1, TResult> factory, RegistrationMode mode, RegistrationLifestyle lifestyle) where TResult : class
		{
			switch (lifestyle)
			{
				case RegistrationLifestyle.Singleton:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(kernel => factory(kernel.Resolve<T1>())).LifeStyle.Singleton);
					break;
				case RegistrationLifestyle.SingletonPerScope:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(kernel => factory(kernel.Resolve<T1>()))
						.LifeStyle.Scoped());
					break;
				case RegistrationLifestyle.Transient:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(kernel => factory(kernel.Resolve<T1>())).LifeStyle.Transient);
					break;
			}
		}

		public void RegisterFactory<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> factory, RegistrationMode mode, RegistrationLifestyle lifestyle) where TResult : class
		{
			switch (lifestyle)
			{
				case RegistrationLifestyle.Singleton:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(kernel => factory(kernel.Resolve<T1>(), kernel.Resolve<T2>(), kernel.Resolve<T3>())).LifeStyle.Singleton);
					break;
				case RegistrationLifestyle.SingletonPerScope:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(kernel => factory(kernel.Resolve<T1>(), kernel.Resolve<T2>(), kernel.Resolve<T3>())).LifeStyle.Scoped());
					break;
				case RegistrationLifestyle.Transient:
					_container.Register(Component.For<TResult>().UsingFactoryMethod(kernel => factory(kernel.Resolve<T1>(), kernel.Resolve<T2>(), kernel.Resolve<T3>())).LifeStyle.Transient);
					break;
			}
		}

		public void Registration(IEnumerable<RegistrationDefinition> definitions)
		{
			foreach (var definition in definitions)
			{
				switch (definition.RegistrationLifestyle)
				{
					case RegistrationLifestyle.Singleton:
						_container.Register(Component.For(definition.ExportType).ImplementedBy(definition.ActivationType)
							.LifeStyle.Singleton);
						break;
					case RegistrationLifestyle.SingletonPerScope:
						_container.Register(Component.For(definition.ExportType).ImplementedBy(definition.ActivationType)
							.LifeStyle.Scoped());
						break;
					case RegistrationLifestyle.Transient:
						_container.Register(Component.For(definition.ExportType).ImplementedBy(definition.ActivationType)
							.LifeStyle.Transient);
						break;
					case RegistrationLifestyle.SingletonPerAncestor:
					case RegistrationLifestyle.SingletonPerObjectGraph:
						var rootType = definition.LifestyleInformation as Type;
						_container.Register(Component.For(definition.ExportType).ImplementedBy(definition.ActivationType)
							.LifestyleBoundTo(resolutionStack => resolutionStack.FirstOrDefault(h => rootType.GetTypeInfo().IsAssignableFrom(h.ComponentModel.Implementation))));
						break;
				}
			}
		}

		public object Resolve(Type type)
		{
			return _kernel.Resolve(type);
		}

		public object Resolve(Type type, object data)
		{
			return _kernel.Resolve(type, data);
		}

		public bool TryResolve(Type type, object data, out object value)
		{
			throw new NotImplementedException();
		}
	}
}
