﻿using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public class FakeDiagnosticsLogger<T> : IDiagnosticsLogger<T>, ILogger
        where T : LoggerCategory<T>, new()
    {
        public ILoggingOptions Options { get; } = new LoggingOptions();

        public bool ShouldLogSensitiveData()
            => false;

        public ILogger Logger
            => this;

        public DiagnosticSource DiagnosticSource { get; } = new DiagnosticListener("Fake");

        public IDbContextLogger DbContextLogger { get; } = new NullDbContextLogger();

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public bool IsEnabled(EventId eventId, LogLevel logLevel)
            => true;

        public IDisposable BeginScope<TState>(TState state)
            => null;

        public virtual LoggingDefinitions Definitions { get; } = new TestRelationalLoggingDefinitions();

        public IInterceptors Interceptors { get; }
    }
}
