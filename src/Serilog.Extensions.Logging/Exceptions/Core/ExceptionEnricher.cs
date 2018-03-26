﻿namespace Serilog.Exceptions.Core
{
    using System;
    using System.Collections.Generic;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;

    /// <summary>
    /// Enrich a <see cref="LogEvent"/> with details about an <see cref="LogEvent.Exception"/> if present.
    /// </summary>
    public sealed class ExceptionEnricher : ILogEventEnricher
    {
        /// <summary>
        /// Collection of destructurers provided by Serilog.Exceptions itself for standard library exceptions.
        /// </summary>
        [Obsolete("Use new fluent configuration API based on the DestructuringOptionsBuilder")]
        public static readonly IEnumerable<IExceptionDestructurer> DefaultDestructurers = DestructuringOptionsBuilder.DefaultDestructurers;

        private readonly IExceptionDestructurer reflectionBasedDestructurer;
        private readonly Dictionary<Type, IExceptionDestructurer> destructurers;
        private readonly IDestructuringOptions destructuringOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEnricher"/> class.
        /// </summary>
        [Obsolete("Specify DestructuringOptions explicitly")]
        public ExceptionEnricher()
            : this(new DestructuringOptionsBuilder().WithDefaultDestructurers())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEnricher"/> class.
        /// </summary>
        /// <param name="destructurers">Collection of destructurers</param>
        [Obsolete("Use new, fluent API based on the DestructuringOptionsBuilder. To specify destructurers, call WithDestructurers method.")]
        public ExceptionEnricher(
            params IExceptionDestructurer[] destructurers)
            : this(new DestructuringOptionsBuilder().WithDestructurers(destructurers))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEnricher"/> class.
        /// </summary>
        /// <param name="destructuringOptions">The destructuring options, cannot be null</param>
        public ExceptionEnricher(IDestructuringOptions destructuringOptions)
        {
            if (destructuringOptions == null)
            {
                throw new ArgumentNullException(
                    nameof(destructuringOptions),
                    "Destructuring options cannot be null");
            }

            this.destructuringOptions = destructuringOptions;
            this.reflectionBasedDestructurer = new ReflectionBasedDestructurer(destructuringOptions.DestructuringDepth);

            this.destructurers = new Dictionary<Type, IExceptionDestructurer>();
            foreach (var destructurer in this.destructuringOptions.Destructurers)
            {
                foreach (var targetType in destructurer.TargetTypes)
                {
                    this.destructurers.Add(targetType, destructurer);
                }
            }
        }

        /// <summary>
        /// Enriches <paramref name="logEvent"/> with a destructured exception's properties.
        /// If the exception is not present, no action is taken.
        /// </summary>
        /// <param name="logEvent">Log event that will be enriched</param>
        /// <param name="propertyFactory">The property factory</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    this.destructuringOptions.RootName,
                    this.DestructureException(logEvent.Exception),
                    true));
            }
        }

        private IReadOnlyDictionary<string, object> DestructureException(Exception exception)
        {
            var data = new ExceptionPropertiesBag(exception, this.destructuringOptions.Filter);

            var exceptionType = exception.GetType();

            if (this.destructurers.ContainsKey(exceptionType))
            {
                var destructurer = this.destructurers[exceptionType];
                destructurer.Destructure(exception, data, this.DestructureException);
            }
            else
            {
                this.reflectionBasedDestructurer.Destructure(exception, data, this.DestructureException);
            }

            return data.GetResultDictionary();
        }
    }
}