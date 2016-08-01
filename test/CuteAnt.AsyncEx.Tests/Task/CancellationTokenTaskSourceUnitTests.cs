using System;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using System.Linq;
using System.Threading;
using Xunit;

namespace CuteAnt.AsyncEx.Tests
{
    public class CancellationTokenTaskSourceUnitTests
    {
        [Fact]
        public void Constructor_AlreadyCanceledToken_TaskReturnsSynchronouslyCanceledTask()
        {
            var token = new CancellationToken(true);
            using (var source = new CancellationTokenTaskSource<object>(token))
                Assert.True(source.Task.IsCanceled);
        }
    }
}
