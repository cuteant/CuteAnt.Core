// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging.Internal;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Creates delegates which can be later cached to log messages in a performant way.
    /// </summary>
    public static partial class LoggerMessageFactory
    {
        private static readonly EventId EmptyEventId = new EventId(0);

        #region -- Define --

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, Exception> Define(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 0);

            return (logger, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues(formatter), exception, LogValues.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="eventId">The event id</param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, Exception> Define(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 0);

            return (logger, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues(formatter), exception, LogValues.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, Exception> Define<T1>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 1);

            return (logger, arg1, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1>(formatter, arg1), exception, LogValues<T1>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="eventId">The event id</param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, Exception> Define<T1>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 1);

            return (logger, arg1, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1>(formatter, arg1), exception, LogValues<T1>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, Exception> Define<T1, T2>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 2);

            return (logger, arg1, arg2, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2>(formatter, arg1, arg2), exception, LogValues<T1, T2>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="eventId">The event id</param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, Exception> Define<T1, T2>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 2);

            return (logger, arg1, arg2, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2>(formatter, arg1, arg2), exception, LogValues<T1, T2>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <typeparam name="T3">The type of the third parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, T3, Exception> Define<T1, T2, T3>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 3);

            return (logger, arg1, arg2, arg3, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3>(formatter, arg1, arg2, arg3), exception, LogValues<T1, T2, T3>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <typeparam name="T3">The type of the third parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="eventId">The event id</param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, T3, Exception> Define<T1, T2, T3>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 3);

            return (logger, arg1, arg2, arg3, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3>(formatter, arg1, arg2, arg3), exception, LogValues<T1, T2, T3>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <typeparam name="T3">The type of the third parameter passed to the named format string.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, T3, T4, Exception> Define<T1, T2, T3, T4>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 4);

            return (logger, arg1, arg2, arg3, arg4, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3, T4>(formatter, arg1, arg2, arg3, arg4), exception, LogValues<T1, T2, T3, T4>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <typeparam name="T3">The type of the third parameter passed to the named format string.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="eventId">The event id</param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, T3, T4, Exception> Define<T1, T2, T3, T4>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 4);

            return (logger, arg1, arg2, arg3, arg4, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3, T4>(formatter, arg1, arg2, arg3, arg4), exception, LogValues<T1, T2, T3, T4>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <typeparam name="T3">The type of the third parameter passed to the named format string.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter passed to the named format string.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, T3, T4, T5, Exception> Define<T1, T2, T3, T4, T5>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 5);

            return (logger, arg1, arg2, arg3, arg4, arg5, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3, T4, T5>(formatter, arg1, arg2, arg3, arg4, arg5), exception, LogValues<T1, T2, T3, T4, T5>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <typeparam name="T3">The type of the third parameter passed to the named format string.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter passed to the named format string.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="eventId">The event id</param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, T3, T4, T5, Exception> Define<T1, T2, T3, T4, T5>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 5);

            return (logger, arg1, arg2, arg3, arg4, arg5, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3, T4, T5>(formatter, arg1, arg2, arg3, arg4, arg5), exception, LogValues<T1, T2, T3, T4, T5>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <typeparam name="T3">The type of the third parameter passed to the named format string.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter passed to the named format string.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter passed to the named format string.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, Exception> Define<T1, T2, T3, T4, T5, T6>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 6);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3, T4, T5, T6>(formatter, arg1, arg2, arg3, arg4, arg5, arg6), exception, LogValues<T1, T2, T3, T4, T5, T6>.Callback);
            };
        }

        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter passed to the named format string.</typeparam>
        /// <typeparam name="T2">The type of the second parameter passed to the named format string.</typeparam>
        /// <typeparam name="T3">The type of the third parameter passed to the named format string.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter passed to the named format string.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter passed to the named format string.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter passed to the named format string.</typeparam>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="eventId">The event id</param>
        /// <param name="formatString">The named format string</param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, Exception> Define<T1, T2, T3, T4, T5, T6>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 6);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3, T4, T5, T6>(formatter, arg1, arg2, arg3, arg4, arg5, arg6), exception, LogValues<T1, T2, T3, T4, T5, T6>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, Exception> Define<T1, T2, T3, T4, T5, T6, T7>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 7);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3, T4, T5, T6, T7>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7), exception, LogValues<T1, T2, T3, T4, T5, T6, T7>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, Exception> Define<T1, T2, T3, T4, T5, T6, T7>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 7);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3, T4, T5, T6, T7>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7), exception, LogValues<T1, T2, T3, T4, T5, T6, T7>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, T8, Exception> Define<T1, T2, T3, T4, T5, T6, T7, T8>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 8);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3, T4, T5, T6, T7, T8>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), exception, LogValues<T1, T2, T3, T4, T5, T6, T7, T8>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, T8, Exception> Define<T1, T2, T3, T4, T5, T6, T7, T8>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 8);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3, T4, T5, T6, T7, T8>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), exception, LogValues<T1, T2, T3, T4, T5, T6, T7, T8>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, T8, T9, Exception> Define<T1, T2, T3, T4, T5, T6, T7, T8, T9>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 9);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9), exception, LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, T8, T9, Exception> Define<T1, T2, T3, T4, T5, T6, T7, T8, T9>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 9);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9), exception, LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Exception> Define<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 10);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10), exception, LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Exception> Define<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 10);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10), exception, LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Exception> Define<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(LogLevel logLevel, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 11);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, exception) =>
            {
                logger.Log(logLevel, EmptyEventId, new LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11), exception, LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Callback);
            };
        }

        /// <summary>Creates a delegate which can be invoked for logging a message.</summary>
        public static Action<ILogger, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Exception> Define<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(LogLevel logLevel, EventId eventId, string formatString)
        {
            var formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 11);

            return (logger, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, exception) =>
            {
                logger.Log(logLevel, eventId, new LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(formatter, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11), exception, LogValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>.Callback);
            };
        }

        #endregion

        #region ** CreateLogValuesFormatter **

        private static LogValuesFormatter CreateLogValuesFormatter(string formatString, int expectedNamedParameterCount)
        {
            var logValuesFormatter = new LogValuesFormatter(formatString);

            var actualCount = logValuesFormatter.ValueNames.Count;
            if (actualCount != expectedNamedParameterCount)
            {
                throw new ArgumentException(
                    $"The format string '{formatString}' does not have the expected number of named parameters. Expected {expectedNamedParameterCount} parameter(s) but found {actualCount} parameter(s).");
            }

            return logValuesFormatter;
        }

        #endregion
    }
}

