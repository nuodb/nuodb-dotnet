using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class NuoDbDateTimeOffsetMemberTranslator: IMemberTranslator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public NuoDbDateTimeOffsetMemberTranslator()
        {

        }
        private readonly NuoDbSqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        private static readonly Dictionary<string, string> _datePartMapping
            = new()
            {
                { nameof(DateTimeOffset.Year), "yyyy" },
                { nameof(DateTimeOffset.Month), "MM" },
                { nameof(DateTimeOffset.DayOfYear), "D" },
                { nameof(DateTimeOffset.Day), "dd" },
                { nameof(DateTimeOffset.Hour), "HH" },
                { nameof(DateTimeOffset.Minute), "mm" },
                { nameof(DateTimeOffset.Second), "ss" },
                { nameof(DateTimeOffset.DayOfWeek), "e" }
            };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbDateTimeOffsetMemberTranslator(ISqlExpressionFactory sqlExpressionFactory,
            IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = (NuoDbSqlExpressionFactory)sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

              if (member.DeclaringType == typeof(DateTimeOffset))
            {
                var memberName = member.Name;

                if (memberName == nameof(DateTimeOffset.DayOfWeek))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.ComplexFunctionArgument(
                            new SqlExpression[]
                            {
                                _sqlExpressionFactory.NullableFunction(
                                "DAYOFWEEK",
                                new SqlExpression[]
                                {
                                    instance
                                },
                                instance.Type,
                                instance.TypeMapping,
                                true,
                                new[] {true, false}),
                                _sqlExpressionFactory.Fragment(" - 1") //apply an offset to account for nuodb's day of week being 1 based index
                            }," ", typeof(string));
                }

                
                if (memberName == nameof(DateTimeOffset.Date))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.Function(
                        "CAST",
                        new SqlExpression[]
                        {
                            new NuoDbComplexFunctionArgumentExpression(
                                new SqlExpression[]
                                {
                                    instance,
                                    _sqlExpressionFactory.Fragment("AS DATE")
                                },
                                " ",
                                typeof(string),
                                instance.TypeMapping
                            )
                        },
                        true,
                        new []{true,false},
                        returnType
                    );
                }

                if (memberName == nameof(DateTimeOffset.Now))
                {
                    return _sqlExpressionFactory.Function("now", Array.Empty<SqlExpression>(), true, new []{true,false},typeof(DateTimeOffset), _typeMappingSource.FindMapping(typeof(DateTimeOffset)));
                }

                if (memberName == nameof(DateTimeOffset.UtcNow))
                {
                    return _sqlExpressionFactory.Function("now", Array.Empty<SqlExpression>(), true, new []{true,false},typeof(DateTimeOffset),_typeMappingSource.FindMapping(typeof(DateTimeOffset)));
                }

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.Convert(
                        NuoDbExpression.DateToStr(
                            _sqlExpressionFactory,
                            typeof(string),
                            datePart,
                            instance),
                        returnType);
                }

                if (memberName == nameof(DateTimeOffset.TimeOfDay))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.Function(
                        "CAST",
                        new SqlExpression[]
                        {
                            new NuoDbComplexFunctionArgumentExpression(
                                new SqlExpression[]
                                {
                                    instance,
                                    _sqlExpressionFactory.Fragment("AS TIME")
                                },
                                " ",
                                typeof(TimeSpan),
                                instance.TypeMapping
                            )
                        },
                        true,
                        new []{true,false},
                        returnType
                    );
                }

                if (memberName == nameof(DateTimeOffset.Millisecond))
                {
                    Check.NotNull(instance, nameof(instance));
                    return _sqlExpressionFactory.Modulo(
                        _sqlExpressionFactory.Multiply(
                            _sqlExpressionFactory.Convert(
                                NuoDbExpression.DateToStr(
                                    _sqlExpressionFactory,
                                    typeof(string),
                                    "SSS",
                                    instance!),
                                typeof(double)),
                            _sqlExpressionFactory.Constant(1000)),
                        _sqlExpressionFactory.Constant(1000));
                }
            }

            return null;
        }
    }
}
