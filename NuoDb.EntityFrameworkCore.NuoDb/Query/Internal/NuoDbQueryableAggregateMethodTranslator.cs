﻿using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    public class NuoDbQueryableAggregateMethodTranslator: IAggregateMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbQueryableAggregateMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType == typeof(Queryable))
        {
            var methodInfo = method.IsGenericMethod
                ? method.GetGenericMethodDefinition()
                : method;
            switch (methodInfo.Name)
            {
                case nameof(Queryable.Average)
                    when (QueryableMethods.IsAverageWithoutSelector(methodInfo)
                        || QueryableMethods.IsAverageWithSelector(methodInfo))
                    && source.Selector is SqlExpression averageSqlExpression:
                    var averageArgumentType = GetProviderType(averageSqlExpression);
                    // if (averageArgumentType == typeof(decimal))
                    // {
                    //     throw new NotSupportedException(
                    //         NuoDbStrings.AggregateOperationNotSupported(
                    //             nameof(Queryable.Average), averageArgumentType.ShortDisplayName()));
                    // }

                    break;

                case nameof(Queryable.Max)
                    when (methodInfo == QueryableMethods.MaxWithoutSelector
                        || methodInfo == QueryableMethods.MaxWithSelector)
                    && source.Selector is SqlExpression maxSqlExpression:
                    var maxArgumentType = GetProviderType(maxSqlExpression);
                    if (maxArgumentType == typeof(DateTimeOffset)
                        || maxArgumentType == typeof(TimeSpan)
                        || maxArgumentType == typeof(ulong))
                    {
                        throw new NotSupportedException(
                            NuoDbStrings.AggregateOperationNotSupported(nameof(Queryable.Max), maxArgumentType.ShortDisplayName()));
                    }

                    break;

                case nameof(Queryable.Min)
                    when (methodInfo == QueryableMethods.MinWithoutSelector
                        || methodInfo == QueryableMethods.MinWithSelector)
                    && source.Selector is SqlExpression minSqlExpression:
                    var minArgumentType = GetProviderType(minSqlExpression);
                    if (minArgumentType == typeof(DateTimeOffset)
                        || minArgumentType == typeof(TimeSpan)
                        || minArgumentType == typeof(ulong))
                    {
                        throw new NotSupportedException(
                            NuoDbStrings.AggregateOperationNotSupported(nameof(Queryable.Min), minArgumentType.ShortDisplayName()));
                    }

                    break;

                case nameof(Queryable.Sum)
                    when (QueryableMethods.IsSumWithoutSelector(methodInfo)
                        || QueryableMethods.IsSumWithSelector(methodInfo))
                    && source.Selector is SqlExpression sumSqlExpression:
                    var sumArgumentType = GetProviderType(sumSqlExpression);
                    // if (sumArgumentType == typeof(decimal))
                    // {
                    //     throw new NotSupportedException(
                    //         NuoDbStrings.AggregateOperationNotSupported(nameof(Queryable.Sum), sumArgumentType.ShortDisplayName()));
                    // }

                    break;
            }
        }

        return null;
    }

    private static Type? GetProviderType(SqlExpression expression)
        => expression.TypeMapping?.Converter?.ProviderClrType
            ?? expression.TypeMapping?.ClrType
            ?? expression.Type;

    }
}
