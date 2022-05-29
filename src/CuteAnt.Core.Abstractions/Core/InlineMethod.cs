using System.Runtime.CompilerServices;

namespace CuteAnt
{
    /// <summary>Helper class for constants for inlining methods</summary>
    public static class InlineMethod
    {
        /// <summary>Value for lining method</summary>
        public const MethodImplOptions Value = MethodImplOptions.AggressiveInlining;
    }
}