using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using CuteAnt.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace CuteAnt.Reflection.Tests
{
    /// <summary>
    /// Tests for <see cref="RuntimeTypeNameFormatter"/>.
    /// </summary>
    public class RuntimeTypeNameFormatterTests
    {
        private readonly ITestOutputHelper output;
        private readonly Type[] types = new[]
            {
                typeof(NameValueCollection),
                typeof(int),
                typeof(int[]),
                typeof(int*[]),
                typeof(int[]),
                typeof(List<>),
                typeof(List<int>),
                typeof(List<int*[]>),
                typeof(Inner<int[,,]>.InnerInner<string, List<int>>.Bottom[,]),
                typeof(Inner<>.InnerInner<,>.Bottom),
                typeof(RuntimeTypeNameFormatterTests),
                typeof(int).MakeByRefType(),
                typeof(Inner<int[]>.InnerInner<string, List<int>>.Bottom[,])
                    .MakePointerType()
                    .MakePointerType()
                    .MakeArrayType(10)
                    .MakeByRefType(),
                typeof(NameValueCollection)
            };

        public RuntimeTypeNameFormatterTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Tests that various strings formatted with <see cref="RuntimeTypeNameFormatter"/> can be loaded using <see cref="Type.GetType(string)"/>.
        /// </summary>
        [Fact]
        public void FormattedTypeNamesAreRecoverable()
        {
            foreach (var type in types)
            {
                var formatted = RuntimeTypeNameFormatter.Format(type);
                this.output.WriteLine($"Full Name: {type.FullName}");
                this.output.WriteLine($"Formatted: {formatted}");
                var isRecoverable = TypeUtils.TryResolveType(formatted, out var resolved) && resolved == type;
                Assert.True(isRecoverable, $"Type.GetType(\"{formatted}\") must be equal to the original type.");
            }
        }

        public class Inner<T>
        {
            public class InnerInner<U, V>
            {
                public class Bottom { }
            }
        }
    }
}
