namespace Serilog.Exceptions.Destructurers
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;

  public class ExceptionDestructurer : IExceptionDestructurer
  {
    public virtual Type[] TargetTypes
    {
      get
      {
        var targetTypes = new List<Type>
        {
          typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException),
          typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderInternalCompilerException),
#if DESKTOPCLR
          typeof(Microsoft.SqlServer.Server.InvalidUdtException),
#endif
          typeof(System.AccessViolationException),
          typeof(System.AppDomainUnloadedException),
          typeof(System.ApplicationException),

          typeof(System.ArithmeticException),
          typeof(System.ArrayTypeMismatchException),
          typeof(System.CannotUnloadAppDomainException),
          typeof(System.Collections.Generic.KeyNotFoundException),
          typeof(System.ComponentModel.Design.CheckoutException),
          typeof(System.ComponentModel.InvalidAsynchronousStateException),
          typeof(System.ComponentModel.InvalidEnumArgumentException),
#if DESKTOPCLR
          typeof(System.Configuration.SettingsPropertyIsReadOnlyException),
          typeof(System.Configuration.SettingsPropertyNotFoundException),
          typeof(System.Configuration.SettingsPropertyWrongTypeException),
#endif
          typeof(System.ContextMarshalException),
          typeof(System.Data.ConstraintException),
          typeof(System.Data.DataException),
          typeof(System.Data.DeletedRowInaccessibleException),
          typeof(System.Data.DuplicateNameException),
          typeof(System.Data.EvaluateException),
          typeof(System.Data.InRowChangingEventException),
          typeof(System.Data.InvalidConstraintException),
          typeof(System.Data.InvalidExpressionException),
          typeof(System.Data.MissingPrimaryKeyException),
          typeof(System.Data.NoNullAllowedException),
#if DESKTOPCLR
          typeof(System.Data.OperationAbortedException),
#endif
          typeof(System.Data.ReadOnlyException),
          typeof(System.Data.RowNotInTableException),
          typeof(System.Data.SqlTypes.SqlAlreadyFilledException),
          typeof(System.Data.SqlTypes.SqlNotFilledException),

          typeof(System.Data.StrongTypingException),
          typeof(System.Data.SyntaxErrorException),
          typeof(System.Data.VersionNotFoundException),

          typeof(System.DataMisalignedException),
          typeof(System.DivideByZeroException),
          typeof(System.DllNotFoundException),

          typeof(System.DuplicateWaitObjectException),
          typeof(System.EntryPointNotFoundException),

          typeof(System.Exception),
          typeof(System.FieldAccessException),
          typeof(System.FormatException),
          typeof(System.IndexOutOfRangeException),
          typeof(System.InsufficientExecutionStackException),

          typeof(System.InsufficientMemoryException),

          typeof(System.InvalidCastException),
          typeof(System.InvalidOperationException),
          typeof(System.InvalidProgramException),
          typeof(System.InvalidTimeZoneException),
          typeof(System.IO.DirectoryNotFoundException),

          typeof(System.IO.DriveNotFoundException),

          typeof(System.IO.EndOfStreamException),

          typeof(System.IO.InternalBufferOverflowException),

          typeof(System.IO.InvalidDataException),
          typeof(System.IO.IOException),

          typeof(System.IO.IsolatedStorage.IsolatedStorageException),

          typeof(System.IO.PathTooLongException),
          typeof(System.MemberAccessException),
          typeof(System.MethodAccessException),

          typeof(System.MulticastNotSupportedException),

          typeof(System.Net.CookieException),

          typeof(System.Net.NetworkInformation.PingException),
          typeof(System.Net.ProtocolViolationException),

          typeof(System.NotImplementedException),
          typeof(System.NotSupportedException),
          typeof(System.NullReferenceException),
          typeof(System.OutOfMemoryException),
          typeof(System.OverflowException),
          typeof(System.PlatformNotSupportedException),
          typeof(System.RankException),
          typeof(System.Reflection.AmbiguousMatchException),

          typeof(System.Reflection.CustomAttributeFormatException),

          typeof(System.Reflection.InvalidFilterCriteriaException),
          typeof(System.Reflection.TargetException),

          typeof(System.Reflection.TargetInvocationException),
          typeof(System.Reflection.TargetParameterCountException),
          typeof(System.Resources.MissingManifestResourceException),
          typeof(System.Runtime.InteropServices.COMException),
          typeof(System.Runtime.InteropServices.InvalidComObjectException),
          typeof(System.Runtime.InteropServices.InvalidOleVariantTypeException),
          typeof(System.Runtime.InteropServices.MarshalDirectiveException),
          typeof(System.Runtime.InteropServices.SafeArrayRankMismatchException),
          typeof(System.Runtime.InteropServices.SafeArrayTypeMismatchException),
          typeof(System.Runtime.InteropServices.SEHException),
#if DESKTOPCLR
          typeof(System.Runtime.Remoting.RemotingException),
          typeof(System.Runtime.Remoting.RemotingTimeoutException),
          typeof(System.Runtime.Remoting.ServerException),
#endif
          typeof(System.Runtime.Serialization.SerializationException),
          typeof(System.Security.Authentication.AuthenticationException),
          typeof(System.Security.Authentication.InvalidCredentialException),
          typeof(System.Security.Cryptography.CryptographicException),
          typeof(System.Security.Cryptography.CryptographicUnexpectedOperationException),
#if DESKTOPCLR
          typeof(System.Security.Policy.PolicyException),
#endif
          typeof(System.Security.VerificationException),
#if DESKTOPCLR
          typeof(System.Security.XmlSyntaxException),
#endif
          typeof(System.StackOverflowException),
          typeof(System.SystemException),
          typeof(System.Threading.BarrierPostPhaseException),
          typeof(System.Threading.LockRecursionException),
          typeof(System.Threading.SemaphoreFullException),
          typeof(System.Threading.SynchronizationLockException),
          typeof(System.Threading.Tasks.TaskSchedulerException),

          typeof(System.Threading.ThreadInterruptedException),
          typeof(System.Threading.ThreadStartException),
          typeof(System.Threading.ThreadStateException),

          typeof(System.Threading.WaitHandleCannotBeOpenedException),
          typeof(System.TimeoutException),

          typeof(System.TimeZoneNotFoundException),

          typeof(System.TypeAccessException),

          typeof(System.TypeUnloadedException),

          typeof(System.UnauthorizedAccessException),
          typeof(System.UriFormatException)
        };

#if DESKTOPCLR
        foreach (var dangerousType in GetNotHandledByMonoTypes())
        {
          var type = Type.GetType(dangerousType);
          if (type != null)
          {
            targetTypes.Add(type);
          }
        }
#endif
        return targetTypes.ToArray();
      }
    }

    public virtual void Destructure(Exception exception,
      IDictionary<string, object> data, Func<Exception, IDictionary<string, object>> innerDestructure)
    {
      data.Add("Type", exception.GetType().FullName);

      if (exception.Data.Count != 0)
      {
        data.Add(nameof(Exception.Data), exception.Data.ToStringObjectDictionary());
      }

      if (!string.IsNullOrEmpty(exception.HelpLink))
      {
        data.Add(nameof(Exception.HelpLink), exception.HelpLink);
      }

      if (exception.HResult != 0)
      {
        data.Add(nameof(Exception.HResult), exception.HResult);
      }

      data.Add(nameof(Exception.Message), exception.Message);
      data.Add(nameof(Exception.Source), exception.Source);
      data.Add(nameof(Exception.StackTrace), exception.StackTrace);


      if (exception.TargetSite != null)
      {
        data.Add(nameof(Exception.TargetSite), exception.TargetSite.ToString());
      }


      if (exception.InnerException != null)
      {
        data.Add(nameof(Exception.InnerException), innerDestructure(exception.InnerException));
      }
    }

    /// <summary>Get types that are currently not handled by mono and could raise a LoadTypeException.</summary>
    /// <returns>List of type names.</returns>
    private static string[] GetNotHandledByMonoTypes()
    {
      return new string[]
      {
        "System.Diagnostics.Eventing.Reader.EventLogInvalidDataException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        "System.Diagnostics.Eventing.Reader.EventLogNotFoundException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        "System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        "System.Diagnostics.Eventing.Reader.EventLogReadingException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        "System.Diagnostics.Tracing.EventSourceException, mscorlib, Version=4.0.0.0, PublicKeyToken=b77a5c561934e089",
        "System.Management.Instrumentation.InstanceNotFoundException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        "System.Management.Instrumentation.InstrumentationBaseException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        "System.Management.Instrumentation.InstrumentationException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
      };
    }
  }
}
