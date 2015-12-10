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
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Provides parameters that have a default value, set with an optional parameter
    /// declaration in C# or VB.
    /// </summary>
    public class DefaultValueParameter : Parameter
    {
        /// <summary>
        /// Returns true if the parameter is able to provide a value to a particular site.
        /// </summary>
        /// <param name="pi">Constructor, method, or property-mutator parameter.</param>
        /// <param name="context">The component context in which the value is being provided.</param>
        /// <param name="valueProvider">If the result is true, the valueProvider parameter will
        /// be set to a function that will lazily retrieve the parameter value. If the result is false,
        /// will be set to null.</param>
        /// <returns>True if a value can be supplied; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="pi" /> is <see langword="null" />.
        /// </exception>
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
        {
            if (pi == null) throw new ArgumentNullException(nameof(pi));

            // System.DBNull is not included in PCL even though it seems to be available in the selected targets.
            // Verified through experimentation 12/14/2012 - PCL initial release in VS 2012 does not support System.DBNull
            // even though the documentation claims it's available. It doesn't appear to matter what the target
            // framework combination is - .NET for Windows Store apps, Windows Phone, Silverlight... it's never
            // available.
            // http://msdn.microsoft.com/en-us/library/windows/apps/system.dbnull(v=vs.110).aspx

            var hasDefaultValue = pi.DefaultValue == null || pi.DefaultValue.GetType().FullName != "System.DBNull";

            if (hasDefaultValue)
            {
                valueProvider = () => pi.DefaultValue;
                return true;
            }
            valueProvider = null;
            return false;
        }
    }
}
