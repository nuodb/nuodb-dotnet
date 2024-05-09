// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         An <see cref="IExecutionStrategy" /> implementation for retrying failed executions on SQL Server.
    ///     </para>
    ///     <para>
    ///         This strategy is specifically tailored to SQL Server (including SQL Azure). It is pre-configured with
    ///         error numbers for transient errors that can be retried. Additional error numbers to retry on can also be supplied.
    ///     </para>
    /// </summary>
    public class NuoDbRetryingExecutionStrategy : ExecutionStrategy
    {
        private readonly ICollection<int>? _additionalErrorNumbers;

        /// <summary>
        ///     <para>
        ///         Creates a new instance of <see cref="NuoDbRetryingExecutionStrategy" />.
        ///     </para>
        ///     <para>
        ///         Default values of 6 for the maximum retry count and 30 seconds for the maximum default delay are used.
        ///     </para>
        /// </summary>
        /// <param name="context">The context on which the operations will be invoked.</param>
        public NuoDbRetryingExecutionStrategy(
            DbContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     <para>
        ///         Creates a new instance of <see cref="NuoDbRetryingExecutionStrategy" />.
        ///     </para>
        ///     <para>
        ///         Default values of 6 for the maximum retry count and 30 seconds for the maximum default delay are used.
        ///     </para>
        /// </summary>
        /// <param name="dependencies">Parameter object containing service dependencies.</param>
        public NuoDbRetryingExecutionStrategy(
            ExecutionStrategyDependencies dependencies)
            : this(dependencies, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     <para>
        ///         Creates a new instance of <see cref="NuoDbRetryingExecutionStrategy" />.
        ///     </para>
        ///     <para>
        ///         A default value 30 seconds for the maximum default delay is used.
        ///     </para>
        /// </summary>
        /// <param name="context">The context on which the operations will be invoked.</param>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        public NuoDbRetryingExecutionStrategy(
            DbContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
        {
        }

        /// <summary>
        ///     <para>
        ///         Creates a new instance of <see cref="NuoDbRetryingExecutionStrategy" />.
        ///     </para>
        ///     <para>
        ///         A default value 30 seconds for the maximum default delay is used.
        ///     </para>
        /// </summary>
        /// <param name="dependencies">Parameter object containing service dependencies.</param>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        public NuoDbRetryingExecutionStrategy(
            ExecutionStrategyDependencies dependencies,
            int maxRetryCount)
            : this(dependencies, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NuoDbRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="context">The context on which the operations will be invoked.</param>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        /// <param name="maxRetryDelay">The maximum delay between retries.</param>
        /// <param name="errorNumbersToAdd">Additional SQL error numbers that should be considered transient.</param>
        public NuoDbRetryingExecutionStrategy(
            DbContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            ICollection<int>? errorNumbersToAdd)
            : base(
                context,
                maxRetryCount,
                maxRetryDelay)
            => _additionalErrorNumbers = errorNumbersToAdd;

        /// <summary>
        ///     Creates a new instance of <see cref="NuoDbRetryingExecutionStrategy" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing service dependencies.</param>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        /// <param name="maxRetryDelay">The maximum delay between retries.</param>
        /// <param name="errorNumbersToAdd">Additional SQL error numbers that should be considered transient.</param>
        public NuoDbRetryingExecutionStrategy(
            ExecutionStrategyDependencies dependencies,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            ICollection<int>? errorNumbersToAdd)
            : base(dependencies, maxRetryCount, maxRetryDelay)
            => _additionalErrorNumbers = errorNumbersToAdd;

        /// <summary>
        ///     Determines whether the specified exception represents a transient failure that can be
        ///     compensated by a retry. Additional exceptions to retry on can be passed to the constructor.
        /// </summary>
        /// <param name="exception">The exception object to be verified.</param>
        /// <returns>
        ///     <see langword="true" /> if the specified exception is considered as transient, otherwise <see langword="false" />.
        /// </returns>
        protected override bool ShouldRetryOn(Exception exception)
        {
            return NuoDbTransientExceptionDetector.ShouldRetryOn(exception);
        }

        /// <summary>
        ///     Determines whether the operation should be retried and the delay before the next attempt.
        /// </summary>
        /// <param name="lastException">The exception thrown during the last execution attempt.</param>
        /// <returns>
        ///     Returns the delay indicating how long to wait for before the next execution attempt if the operation should be retried;
        ///     <see langword="null" /> otherwise
        /// </returns>
        protected override TimeSpan? GetNextDelay(Exception lastException)
        {
            var baseDelay = base.GetNextDelay(lastException);
            if (baseDelay == null)
            {
                return null;
            }

            return CallOnWrappedException(lastException, IsMemoryOptimizedError)
                ? TimeSpan.FromMilliseconds(baseDelay.Value.TotalSeconds)
                : baseDelay;
        }

        private static bool IsMemoryOptimizedError(Exception exception)
        {
            return false;
        }
    }
}
