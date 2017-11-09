using System;
#if NET40
using CuteAnt.AsyncEx.Internal.PlatformEnlightenment;
#else
using System.Runtime.ExceptionServices;
#endif

namespace CuteAnt.AsyncEx
{
  /// <summary>Provides helper (non-extension) methods dealing with exceptions.</summary>
  public static class ExceptionHelpers
  {
    /// <summary>Attempts to prepare the exception for re-throwing by preserving the stack trace. 
    /// The returned exception should be immediately thrown.</summary>
    /// <param name="exception">The exception. May not be <c>null</c>.</param>
    /// <returns>The <see cref="Exception"/> that was passed into this method.</returns>
    public static Exception PrepareForRethrow(Exception exception)
    {
#if NET40
      return ExceptionEnlightenment.PrepareForRethrow(exception);
#else
      ExceptionDispatchInfo.Capture(exception).Throw();

      // The code cannot ever get here. We just return a value to work around a badly-designed API (ExceptionDispatchInfo.Throw):
      //  https://connect.microsoft.com/VisualStudio/feedback/details/689516/exceptiondispatchinfo-api-modifications (http://www.webcitation.org/6XQ7RoJmO)
      return exception;
#endif
    }
  }
}