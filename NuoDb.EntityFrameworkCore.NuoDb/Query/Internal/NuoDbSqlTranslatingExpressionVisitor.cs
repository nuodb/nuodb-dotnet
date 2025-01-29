// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using NuoDb.EntityFrameworkCore.NuoDb.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
    {
        private static readonly List<string> DateTimeDataTypes
            =new List<string>{
            
                "time",
                "date",
                "datetime",
                "datetimeoffset"
            };
        private static readonly List<Type> DateTimeClrTypes
            = new List<Type>{
            
                typeof(TimeOnly),
                typeof(DateOnly),
                typeof(TimeSpan),
                typeof(DateTime),
                typeof(DateTimeOffset)
            };
        private static readonly IReadOnlyDictionary<ExpressionType, IReadOnlyCollection<Type>> _restrictedBinaryExpressions
            = new Dictionary<ExpressionType, IReadOnlyCollection<Type>>
            {
                [ExpressionType.Add] = new HashSet<Type>
                {
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan)
                },
                [ExpressionType.Divide] = new HashSet<Type> { typeof(TimeSpan), typeof(ulong) },
                [ExpressionType.GreaterThan] = new HashSet<Type>
                {
                    //typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.GreaterThanOrEqual] = new HashSet<Type>
                {
                    //typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.LessThan] = new HashSet<Type>
                {
                    //typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.LessThanOrEqual] = new HashSet<Type>
                {
                    //typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.Modulo] = new HashSet<Type> { typeof(ulong) },
                [ExpressionType.Multiply] = new HashSet<Type> { typeof(TimeSpan), typeof(ulong) },
                [ExpressionType.Subtract] = new HashSet<Type>
                {
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan)
                }
            };

        private static readonly IReadOnlyCollection<Type> _functionModuloTypes = new HashSet<Type>
        {
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long)
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbSqlTranslatingExpressionVisitor(
            RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            QueryCompilationContext queryCompilationContext,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
        {
            _sqlExpressionFactory = dependencies.SqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            //return base.VisitUnary(unaryExpression);
            Check.NotNull(unaryExpression, nameof(unaryExpression));

           

            var visitedExpression = base.VisitUnary(unaryExpression);
            if (visitedExpression == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            return visitedExpression;
        }
        

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (!(base.VisitBinary(binaryExpression) is SqlExpression visitedExpression))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            if (visitedExpression is SqlBinaryExpression sqlBinary )
            {
                
                if (sqlBinary.OperatorType == ExpressionType.Modulo
                    && (_functionModuloTypes.Contains(GetProviderType(sqlBinary.Left))
                        || _functionModuloTypes.Contains(GetProviderType(sqlBinary.Right))))
                {
                    return //Dependencies.SqlExpressionFactory.Equal(
                     Dependencies.SqlExpressionFactory.Modulo(
                        sqlBinary.Left,
                        sqlBinary.Right,
                        //new[] { sqlBinary.Left, sqlBinary.Right },
                        visitedExpression.TypeMapping);//, Dependencies.SqlExpressionFactory.Constant(0));
                }

                // var inferredProviderType = GetProviderTypeName(sqlBinary.Left) ?? GetProviderTypeName(sqlBinary.Right);
                // if (inferredProviderType != null)
                // {
                //     if (DateTimeDataTypes.Contains(inferredProviderType))
                //     {
                //         return QueryCompilationContext.NotTranslatedExpression;
                //     }
                // }
                // else
                // {
                //     var leftType = sqlBinary.Left.Type;
                //     var rightType = sqlBinary.Right.Type;
                //
                //     if (DateTimeClrTypes.Contains(leftType)
                //         || DateTimeClrTypes.Contains(rightType))
                //     {
                //         return QueryCompilationContext.NotTranslatedExpression;
                //     }
                // }

                //don't try to handle bitwise and of boolean predicates
                if (sqlBinary.OperatorType == ExpressionType.And && sqlBinary.Left.Type == typeof(bool) && sqlBinary.Right.Type == typeof(bool))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                //don't try to handle bitwise OR of boolean predicates
                if (sqlBinary.OperatorType == ExpressionType.Or && sqlBinary.Left.Type == typeof(bool) && sqlBinary.Right.Type == typeof(bool))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                
                if (_restrictedBinaryExpressions.TryGetValue(sqlBinary.OperatorType, out var restrictedTypes)
                     && (restrictedTypes.Contains(GetProviderType(sqlBinary.Left))
                         || restrictedTypes.Contains(GetProviderType(sqlBinary.Right))))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }

            return visitedExpression;
        }


        /// <summary>
        ///     Translates Average over an expression to an equivalent SQL representation.
        /// </summary>
        /// <param name="sqlExpression">An expression to translate Average over.</param>
        /// <returns>A SQL translation of Average over the given expression.</returns>
        public override SqlExpression? TranslateAverage(SqlExpression sqlExpression)
        {
            Check.NotNull(sqlExpression, nameof(sqlExpression));

            var inputType = sqlExpression.Type;
            if (inputType == typeof(int)
                || inputType == typeof(long))
            {
                sqlExpression = sqlExpression is DistinctExpression distinctExpression
                    ? new DistinctExpression(
                        _sqlExpressionFactory.ApplyDefaultTypeMapping(
                            _sqlExpressionFactory.Convert(distinctExpression.Operand, typeof(double))))
                    : _sqlExpressionFactory.ApplyDefaultTypeMapping(
                        _sqlExpressionFactory.Convert(sqlExpression, typeof(double)));
            }

            return inputType == typeof(float) || inputType == typeof(double) || inputType == typeof(decimal)
                ? _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "AVG",
                        new[]
                        {
                            _sqlExpressionFactory.Convert(sqlExpression, typeof(float))
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false },
                        typeof(double)),
                    sqlExpression.Type,
                    sqlExpression.TypeMapping)
                : _sqlExpressionFactory.Function(
                    "AVG",
                    new[]
                    {
                        sqlExpression
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false },
                    sqlExpression.Type,
                    sqlExpression.TypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override SqlExpression? TranslateMax(SqlExpression sqlExpression)
        {
            Check.NotNull(sqlExpression, nameof(sqlExpression));

            var visitedExpression = base.TranslateMax(sqlExpression);
            var argumentType = GetProviderType(visitedExpression);
            if (argumentType == typeof(DateTimeOffset)
                || argumentType == typeof(TimeSpan)
                || argumentType == typeof(ulong))
            {
                throw new NotSupportedException(
                    NuoDbStrings.AggregateOperationNotSupported(nameof(Queryable.Max), argumentType.ShortDisplayName()));
            }

            return visitedExpression;
        }
        

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override SqlExpression? TranslateMin(SqlExpression sqlExpression)
        {
            Check.NotNull(sqlExpression, nameof(sqlExpression));

            var visitedExpression = base.TranslateMin(sqlExpression);
            var argumentType = GetProviderType(visitedExpression);
            if (argumentType == typeof(DateTimeOffset)
                || argumentType == typeof(TimeSpan)
                || argumentType == typeof(ulong))
            {
                throw new NotSupportedException(
                    NuoDbStrings.AggregateOperationNotSupported(nameof(Queryable.Min), argumentType.ShortDisplayName()));
            }
           
            return visitedExpression;
        }

        // protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        // {
        //     return base.VisitMethodCall(methodCallExpression);
        // }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        // public override SqlExpression? TranslateSum(SqlExpression sqlExpression)
        // {
        //     Check.NotNull(sqlExpression, nameof(sqlExpression));
        //
        //     var visitedExpression = base.TranslateSum(sqlExpression);
        //     var argumentType = GetProviderType(visitedExpression);
        //
        //     return visitedExpression;
        // }

        

        private static Type? GetProviderType(SqlExpression? expression)
            => expression == null
                ? null
                : (expression.TypeMapping?.Converter?.ProviderClrType
                    ?? expression.TypeMapping?.ClrType
                    ?? expression.Type);

        // private static string? GetProviderTypeName(SqlExpression expression)
        //     => expression.TypeMapping?.StoreType;

        // private static bool AreOperandsDecimals(SqlBinaryExpression sqlExpression)
        //     => GetProviderType(sqlExpression.Left) == typeof(decimal)
        //         && GetProviderType(sqlExpression.Right) == typeof(decimal);
    }
}
