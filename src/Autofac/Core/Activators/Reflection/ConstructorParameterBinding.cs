﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
#if DNX451 || DNXCORE50
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Binds a constructor to the parameters that will be used when it is invoked.
    /// </summary>
    public class ConstructorParameterBinding
    {
        readonly ConstructorInfo _ci;
        readonly Func<object>[] _valueRetrievers;
#if DNX451 || DNXCORE50
        readonly static ConcurrentDictionary<ConstructorInfo, Func<object[], object>> _constructorInvokers = new ConcurrentDictionary<ConstructorInfo, Func<object[], object>>();
#else
        readonly static SafeDictionary<ConstructorInfo, Func<object[], object>> _constructorInvokers = new SafeDictionary<ConstructorInfo, Func<object[], object>>();
#endif

        // We really need to report all non-bindable parameters, howevers some refactoring
        // will be necessary before this is possible. Adding this now to ease the
        // pain of working with the preview builds.
        readonly ParameterInfo _firstNonBindableParameter;

        /// <summary>
        /// The constructor on the target type. The actual constructor used
        /// might differ, e.g. if using a dynamic proxy.
        /// </summary>
        public ConstructorInfo TargetConstructor => _ci;

        /// <summary>
        /// True if the binding is valid.
        /// </summary>
        public bool CanInstantiate { get; }

        /// <summary>
        /// Construct a new ConstructorParameterBinding.
        /// </summary>
        /// <param name="ci">ConstructorInfo to bind.</param>
        /// <param name="availableParameters">Available parameters.</param>
        /// <param name="context">Context in which to construct instance.</param>
        public ConstructorParameterBinding(
            ConstructorInfo ci,
            IEnumerable<Parameter> availableParameters,
            IComponentContext context)
        {
            if (ci == null) throw new ArgumentNullException(nameof(ci));
            if (availableParameters == null) throw new ArgumentNullException(nameof(availableParameters));
            if (context == null) throw new ArgumentNullException(nameof(context));

            CanInstantiate = true;
            _ci = ci;
            var parameters = ci.GetParameters();
            _valueRetrievers = new Func<object>[parameters.Length];

            for (int i = 0; i < parameters.Length; ++i)
            {
                var pi = parameters[i];
                bool foundValue = false;
                foreach (var param in availableParameters)
                {
                    Func<object> valueRetriever;
                    if (param.CanSupplyValue(pi, context, out valueRetriever))
                    {
                        _valueRetrievers[i] = valueRetriever;
                        foundValue = true;
                        break;
                    }
                }
                if (!foundValue)
                {
                    CanInstantiate = false;
                    _firstNonBindableParameter = pi;
                    break;
                }
            }
        }

        /// <summary>
        /// Invoke the constructor with the parameter bindings.
        /// </summary>
        /// <returns>The constructed instance.</returns>
        public object Instantiate()
        {
            if (!CanInstantiate)
                throw new InvalidOperationException(ConstructorParameterBindingResources.CannotInstantitate);

            var values = new object[_valueRetrievers.Length];
            for (var i = 0; i < _valueRetrievers.Length; ++i)
                values[i] = _valueRetrievers[i]();

            Func<object[], object> constructorInvoker;
            if (!_constructorInvokers.TryGetValue(TargetConstructor, out constructorInvoker))
            {
                constructorInvoker = GetConstructorInvoker(TargetConstructor);
                _constructorInvokers[TargetConstructor] = constructorInvoker;
            }

            try
            {
                return constructorInvoker(values);
            }
            catch (TargetInvocationException ex)
            {
                throw new DependencyResolutionException(
                    string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType.Name), ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new DependencyResolutionException(
                    string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType.Name), ex);
            }
        }

        /// <summary>
        /// Describes the constructor parameter binding.
        /// </summary>
        public string Description => CanInstantiate
            ? string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.BoundConstructor, _ci)
            : string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.NonBindableConstructor, _ci, _firstNonBindableParameter);

        ///<summary>Returns a System.String that represents the current System.Object.</summary>
        ///<returns>A System.String that represents the current System.Object.</returns>
        public override string ToString()
        {
            return Description;
        }

        static Func<object[], object> GetConstructorInvoker(ConstructorInfo constructorInfo)
        {
            var paramsInfo = constructorInfo.GetParameters();

            var parametersExpression = Expression.Parameter(typeof(object[]), "args");
            var argumentsExpression = new Expression[paramsInfo.Length];

            for (int paramIndex = 0; paramIndex < paramsInfo.Length; paramIndex++)
            {
                var indexExpression = Expression.Constant(paramIndex);
                var parameterType = paramsInfo[paramIndex].ParameterType;

                var parameterIndexExpression = Expression.ArrayIndex(parametersExpression, indexExpression);
                var convertExpression = Expression.Convert(parameterIndexExpression, parameterType);
                argumentsExpression[paramIndex] = convertExpression;

                if (!parameterType.GetTypeInfo().IsValueType) continue;

                var nullConditionExpression = Expression.Equal(
                    parameterIndexExpression, Expression.Constant(null));
                argumentsExpression[paramIndex] = Expression.Condition(
                    nullConditionExpression, Expression.Default(parameterType), convertExpression);
            }

            var newExpression = Expression.New(constructorInfo, argumentsExpression);
            var lambdaExpression = Expression.Lambda<Func<object[], object>>(newExpression, parametersExpression);

            return lambdaExpression.Compile();
        }
    }
}
