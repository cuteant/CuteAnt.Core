﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class CustomBeginScopeTest : NLogTestBase
    {
        [Fact]
        public void TestNonSerializableSayHello()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>();
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${mdlc:World}. Welcome ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHello().Wait();
            Assert.Single(target.Logs);
            Assert.Equal("Hello Earth. Welcome Earth People", target.Logs[0]);
        }

        [Fact]
        public void TestNonSerializableSayHelloWithScope()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>(new NLogProviderOptions() { IncludeScopes = false });
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${mdlc:World}. Welcome ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHello().Wait();
            Assert.Single(target.Logs);
            Assert.Equal("Hello . Welcome ", target.Logs[0]);
        }

        [Fact]
        public void TestNonSerializableSayHi()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>();
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${mdlc:World}. Welcome ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            var scopeString = runner.SayHi().Result;
            Assert.Single(target.Logs);
            Assert.Equal("Hi Earth. Welcome Earth People", target.Logs[0]);
            // Assert.Equal("Earth People", scopeString); <-- Bug https://github.com/aspnet/Logging/issues/893
        }

        [Fact]
        public void TestNonSerializableSayNothing()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>();
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayNothing().Wait();
            Assert.Single(target.Logs);
            Assert.Equal("Nothing", target.Logs[0]);
        }

        public class CustomBeginScopeTestRunner
        {
            private readonly ILogger<CustomBeginScopeTestRunner> _logger;

            public CustomBeginScopeTestRunner(ILogger<CustomBeginScopeTestRunner> logger)
            {
                _logger = logger;
            }

            public async Task SayHello()
            {
                using (_logger.BeginScope(new ActionLogScope("Earth")))
                {
                    await Task.Yield();
                    _logger.LogInformation("Hello");
                }
            }

            public async Task<string> SayHi()
            {
                using (var scopeState = _logger.BeginScope("{World} People", "Earth"))
                {
                    await Task.Yield();
                    _logger.LogInformation("Hi");
                    return scopeState.ToString();
                }
            }

            public async Task SayNothing()
            {
                using (var scopeState = _logger.BeginScope(new Dictionary<string,string>()))
                {
                    await Task.Yield();
                    _logger.LogInformation("Nothing");
                }
            }
        }

#if NET40
        private class ActionLogScope : IList<KeyValuePair<string, object>>
#else
        private class ActionLogScope : IReadOnlyList<KeyValuePair<string, object>>
#endif
        {
            private readonly string _world;

            public ActionLogScope(string world)
            {
                if (world == null)
                {
                    throw new ArgumentNullException(nameof(world));
                }

                _world = world;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object>("World", _world);
                    }
                    throw new IndexOutOfRangeException(nameof(index));
                }
#if NET40
                set => throw new NotImplementedException();
#endif
            }

            public int Count => 1;

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            public override string ToString()
            {
                // We don't include the _action.Id here because it's just an opaque guid, and if
                // you have text logging, you can already use the requestId for correlation.
                return string.Concat(_world, " People");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
#if NET40
            public bool IsReadOnly => throw new NotImplementedException();

            public int IndexOf(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }
#endif
        }
    }
}
