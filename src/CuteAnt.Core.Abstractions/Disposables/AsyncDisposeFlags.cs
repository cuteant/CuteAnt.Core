#if !(NETCOREAPP2_1 || NETSTANDARD2_0)
using System;

namespace CuteAnt.Disposables
{
    /// <summary>
    /// Flags to control asynchronous disposal.
    /// </summary>
    [Flags]
    public enum AsyncDisposeFlags
    {
        /// <summary>
        /// Execute multiple asynchronous disposal methods serially. Each asynchronous disposal method will not start until the previous one has completed.
        /// </summary>
        ExecuteSerially = 0,

        /// <summary>
        /// Execute multiple asynchronous disposal methods concurrently. All asynchronous disposal methods are started, and then asynchronously wait for all of them to complete.
        /// </summary>
        ExecuteConcurrently = 1,
    }
}
#endif