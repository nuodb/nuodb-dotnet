﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public class TestNuoDbRetryingExecutionStrategy: NuoDbRetryingExecutionStrategy

    {
        private const bool ErrorNumberDebugMode = false;

        private static readonly int[] _additionalErrorNumbers =
        {
           
        };

        public TestNuoDbRetryingExecutionStrategy()
            : base(
                new DbContext(
                    new DbContextOptionsBuilder()
                        .EnableServiceProviderCaching(false)
                        .UseNuoDb(TestEnvironment.DefaultConnection).Options),
                DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        public TestNuoDbRetryingExecutionStrategy(DbContext context)
            : base(context, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        public TestNuoDbRetryingExecutionStrategy(DbContext context, TimeSpan maxDelay)
            : base(context, DefaultMaxRetryCount, maxDelay, _additionalErrorNumbers)
        {
        }

        public TestNuoDbRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
            : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            if (base.ShouldRetryOn(exception))
            {
                return true;
            }

            // if (ErrorNumberDebugMode
            //     && exception is NuoDbSqlException sqlException)
            // {
            //     var message = "Didn't retry on";
            //     foreach (SqlError err in sqlException.Errors)
            //     {
            //         message += " " + err.Number;
            //     }
            //
            //     message += Environment.NewLine;
            //     throw new InvalidOperationException(message + exception, exception);
            // }

            return exception is InvalidOperationException invalidOperationException
                && invalidOperationException.Message == "Internal .Net Framework Data Provider error 6.";
        }

        public new virtual TimeSpan? GetNextDelay(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }
    }
}
