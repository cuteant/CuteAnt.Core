using System;
using System.Collections.Generic;
#if NET_4_0_GREATER
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt
{
	public sealed class ValidationHelper
	{
		#region -- method ArgumentNullOrEmpty --

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentNullOrEmpty(String value, String parameterName)
		{
			ArgumentNullOrEmpty(value, parameterName, "'{0}' cannot be empty.");
		}

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentNullOrEmpty(String value, String parameterName, String message)
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			else
			{
				for (Int32 i = 0; i < value.Length; i++)
				{
					if (!Char.IsWhiteSpace(value[i])) { return; }
				}
				throw new ArgumentException(message.FormatWith(parameterName), parameterName);
			}
		}

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentNullOrEmpty<T>(ICollection<T> collection, String parameterName)
		{
			ArgumentNullOrEmpty<T>(collection, parameterName, "Collection '{0}' cannot be empty.".FormatWith(parameterName));
		}

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentNullOrEmpty<T>(ICollection<T> collection, String parameterName, String message)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			if (collection.Count == 0)
			{
				throw new ArgumentException(message, parameterName);
			}
		}

		#endregion

		#region -- method ArgumentNull --

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentNull<T>(T value, String parameterName) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
		}

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentNull<T>(T value, String parameterName, String message)
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName, message.FormatWith(parameterName));
			}
		}

		#endregion

		#region -- method ArgumentCondition --

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentCondition(Boolean condition, String message)
		{
			if (condition)
			{
				throw new ArgumentException(message);
			}
		}

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentCondition(Boolean condition, String parameterName, String message)
		{
			if (condition)
			{
				throw new ArgumentException(message.FormatWith(parameterName), parameterName);
			}
		}

		#endregion

		#region -- method ArgumentOutOfRangeCondition --

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentOutOfRangeCondition(Boolean condition, String parameterName)
		{
			if (condition)
			{
				throw new ArgumentOutOfRangeException(parameterName);
			}
		}

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentOutOfRangeCondition(Boolean condition, String parameterName, String message)
		{
			if (condition)
			{
				throw new ArgumentOutOfRangeException(parameterName, message.FormatWith(parameterName));
			}
		}

		#endregion

		#region -- method ArgumentTypeIsEnum --

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ArgumentTypeIsEnum(Type enumType, String parameterName)
		{
			ArgumentNull(enumType, "enumType");
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Type {0} is not an Enum.".FormatWith(enumType), parameterName);
			}
		}

		#endregion

		#region -- method InvalidOperationCondition --

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void InvalidOperationCondition(Boolean condition, String message)
		{
			if (condition)
			{
				throw new InvalidOperationException(message);
			}
		}

		#endregion

		#region -- method ObjectDisposedCondition --

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ObjectDisposedCondition(Boolean condition, String objectName)
		{
			if (condition)
			{
				throw new ObjectDisposedException(objectName);
			}
		}

#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void ObjectDisposedCondition(Boolean condition, String objectName, String message)
		{
			if (condition)
			{
				throw new ObjectDisposedException(objectName, message.FormatWith(objectName));
			}
		}

		#endregion

		//public static void NotNull<T>(T value) where T : class
		//{
		//	Debug.Assert(value != null);
		//}

		//[Conditional("DEBUG")]
		//public static void NotNull<T>(T? value) where T : struct
		//{
		//	Debug.Assert(value != null);
		//}
		//public static void NotEmpty(string value)
		//{
		//	Debug.Assert(!string.IsNullOrWhiteSpace(value));
		//}
	}
}