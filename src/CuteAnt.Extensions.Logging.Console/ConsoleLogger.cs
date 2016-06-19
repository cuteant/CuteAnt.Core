﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using CuteAnt.Extensions.Logging.Console.Internal;

namespace CuteAnt.Extensions.Logging.Console
{
    public class ConsoleLogger : ILogger
    {
        // Writing to console is not an atomic operation in the current implementation and since multiple logger
        // instances are created with a different name. Also since Console is global, using a static lock is fine.
        private static readonly object _lock = new object();
        private static readonly string _loglevelPadding = ": ";
        private static readonly string _messagePadding;
        private static readonly string _newLineWithMessagePadding;

        // ConsoleColor does not have a value to specify the 'Default' color
        private readonly ConsoleColor? DefaultConsoleColor = null;

        private IConsole _console;
        private Func<string, LogLevel, bool> _filter;

        static ConsoleLogger()
        {
            var logLevelString = GetLogLevelString(LogLevel.Information);
            _messagePadding = new string(' ', logLevelString.Length + _loglevelPadding.Length);
            _newLineWithMessagePadding = Environment.NewLine + _messagePadding;
        }

        public ConsoleLogger(string name, Func<string, LogLevel, bool> filter, bool includeScopes)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Filter = filter ?? ((category, logLevel) => true);
            IncludeScopes = includeScopes;

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
                Console = new WindowsLogConsole();
            //}
            //else
            //{
            //    Console = new AnsiLogConsole(new AnsiSystemConsole());
            //}
        }

        public IConsole Console
        {
            get { return _console; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _console = value;
            }
        }

        public Func<string, LogLevel, bool> Filter
        {
            get { return _filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _filter = value;
            }
        }

        public bool IncludeScopes { get; set; }

        public string Name { get; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, Name, eventId.Id, message, exception);
            }
        }

        public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            var logLevelColors = default(ConsoleColors);
            var logLevelString = string.Empty;
            var logIdentifier = string.Empty;
            var scopeInformation = string.Empty;
            var exceptionText = string.Empty;
            var printLog = false;

            // Example:
            // INFO: ConsoleApp.Program[10]
            //       Request received
            if (!string.IsNullOrEmpty(message))
            {
                logLevelColors = GetLogLevelConsoleColors(logLevel);
                logLevelString = GetLogLevelString(logLevel);
                // category and event id
                logIdentifier = _loglevelPadding + logName + "[" + eventId + "]";
                // scope information
                if (IncludeScopes)
                {
                    scopeInformation = GetScopeInformation();
                }
                // message
                message = _messagePadding + ReplaceMessageNewLinesWithPadding(message);
                printLog = true;
            }

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception != null)
            {
                // exception message
                exceptionText = exception.ToString();
                printLog = true;
            }

            if (printLog)
            {
                lock (_lock)
                {
                    if (!string.IsNullOrEmpty(logLevelString))
                    {
                        // log level string
                        Console.Write(
                            logLevelString,
                            logLevelColors.Background,
                            logLevelColors.Foreground);
                    }

                    // use default colors from here on
                    if (!string.IsNullOrEmpty(logIdentifier))
                    {
                        Console.WriteLine(
                            logIdentifier,
                            DefaultConsoleColor,
                            DefaultConsoleColor);
                    }
                    if (!string.IsNullOrEmpty(scopeInformation))
                    {
                        Console.WriteLine(
                            scopeInformation,
                            DefaultConsoleColor,
                            DefaultConsoleColor);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine(
                            message,
                            DefaultConsoleColor,
                            DefaultConsoleColor);
                    }
                    if (!string.IsNullOrEmpty(exceptionText))
                    {
                        Console.WriteLine(
                            exceptionText,
                            DefaultConsoleColor,
                            DefaultConsoleColor);
                    }

                    // In case of AnsiLogConsole, the messages are not yet written to the console,
                    // this would flush them instead.
                    Console.Flush();
                }
            }
        }

        private string ReplaceMessageNewLinesWithPadding(string message)
        {
            return message.Replace(Environment.NewLine, _newLineWithMessagePadding);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Filter(Name, logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return ConsoleLogScope.Push(Name, state);
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
        {
            // We must explicitly set the background color if we are setting the foreground color,
            // since just setting one can look bad on the users console.
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return new ConsoleColors(ConsoleColor.White, ConsoleColor.Red);
                case LogLevel.Error:
                    return new ConsoleColors(ConsoleColor.Black, ConsoleColor.Red);
                case LogLevel.Warning:
                    return new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black);
                case LogLevel.Information:
                    return new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black);
                case LogLevel.Debug:
                    return new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black);
                case LogLevel.Trace:
                    return new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black);
                default:
                    return new ConsoleColors(DefaultConsoleColor, DefaultConsoleColor);
            }
        }

        private string GetScopeInformation()
        {
            var current = ConsoleLogScope.Current;
            var output = new StringBuilder();
            string scopeLog = string.Empty;
            while (current != null)
            {
                if (output.Length == 0)
                {
                    scopeLog = $"=> {current}";
                }
                else
                {
                    scopeLog = $"=> {current} ";
                }

                output.Insert(0, scopeLog);
                current = current.Parent;
            }
            if (output.Length > 0)
            {
                output.Insert(0, _messagePadding);
            }

            return output.ToString();
        }

        private struct ConsoleColors
        {
            public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
            {
                Foreground = foreground;
                Background = background;
            }

            public ConsoleColor? Foreground { get; }

            public ConsoleColor? Background { get; }
        }

        private class AnsiSystemConsole : IAnsiSystemConsole
        {
            public void Write(string message)
            {
                System.Console.Write(message);
            }

            public void WriteLine(string message)
            {
                System.Console.WriteLine(message);
            }
        }
    }
}