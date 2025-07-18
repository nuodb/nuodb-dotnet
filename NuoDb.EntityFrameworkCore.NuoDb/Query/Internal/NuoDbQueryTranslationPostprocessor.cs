﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
    {
        private readonly ApplyValidatingVisitor _applyValidator = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbQueryTranslationPostprocessor(
            QueryTranslationPostprocessorDependencies dependencies,
            RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression Process(Expression query)
        {
            var result = base.Process(query);
            _applyValidator.Visit(result);

            return result;
        }

        private sealed class ApplyValidatingVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
                {
                    Visit(shapedQueryExpression.QueryExpression);
                    Visit(shapedQueryExpression.ShaperExpression);

                    return extensionExpression;
                }

                if (extensionExpression is OrderingExpression orderingExpression)
                {
                    if (orderingExpression.Expression is ScalarSubqueryExpression)
                    {
                        throw new InvalidOperationException(NuoDbStrings.SubqueriesInOrderByNotSupported);
                    }

                   
                    if (orderingExpression.Expression is SqlBinaryExpression binaryExpression)
                    {
                        if (binaryExpression.Left is ScalarSubqueryExpression ||
                            binaryExpression.Right is ScalarSubqueryExpression)
                            throw new InvalidOperationException(NuoDbStrings.SubqueriesInOrderByNotSupported);
                    }
                }

                if (extensionExpression is ExceptExpression)
                {
                    throw new InvalidOperationException(NuoDbStrings.ExceptNotSupported);
                }

                if (extensionExpression is IntersectExpression)
                {
                    throw new InvalidOperationException(NuoDbStrings.InterceptNotSupported);
                }
                if (extensionExpression is SelectExpression selectExpression)
                {
                    if(selectExpression.Tables.Any(t => t is CrossApplyExpression || t is OuterApplyExpression))
                    {
                        throw new InvalidOperationException(NuoDbStrings.ApplyNotSupported);
                    }

                    if(selectExpression.Tables.Any(x=>x is CrossJoinExpression))
                    {
                        throw new InvalidOperationException(NuoDbStrings.CrossJoinNotSupported);
                    }

                    if (selectExpression.Predicate is SqlParameterExpression &&
                        selectExpression.Predicate.Type == typeof(bool))
                    {
                        throw new InvalidOperationException(
                            "NuoDb does not support boolean predicates as parameter values");
                    }

                }
                return base.VisitExtension(extensionExpression);
            }
        }
    }
}
