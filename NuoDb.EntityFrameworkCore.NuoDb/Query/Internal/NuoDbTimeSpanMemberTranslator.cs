using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NuoDb.EntityFrameworkCore.NuoDb.Storage.Internal;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    public class NuoDbTimeSpanMemberTranslator: IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMappings = new()
        {
            { nameof(TimeSpan.Hours), "hour" },
            { nameof(TimeSpan.Minutes), "minute" },
            { nameof(TimeSpan.Seconds), "second" },
            { nameof(TimeSpan.Milliseconds), "millisecond" }
        };

        private readonly NuoDbSqlExpressionFactory _sqlExpressionFactory;
        private readonly NuoDbTypeMappingSource _typeMappingSource;
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NuoDbTimeSpanMemberTranslator(ISqlExpressionFactory sqlExpressionFactory, NuoDbTypeMappingSource nuodbTypeMappingSource)
        {
            _sqlExpressionFactory = (NuoDbSqlExpressionFactory)sqlExpressionFactory;
            _typeMappingSource = nuodbTypeMappingSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

            if (member.DeclaringType == typeof(TimeSpan) && _datePartMappings.TryGetValue(member.Name, out var value))
            {
                if (instance is SqlConstantExpression instanceConstant)
                {
                    instance = instanceConstant.ApplyTypeMapping(_typeMappingSource.FindMapping(typeof(DateTime), "TIMESTAMP"));
                }
                return _sqlExpressionFactory.NullableFunction(
                    "EXTRACT",
                    new[]
                    {
                        _sqlExpressionFactory.ComplexFunctionArgument(
                            new SqlExpression[]
                            {
                                _sqlExpressionFactory.Fragment(value),
                                _sqlExpressionFactory.Fragment("IN"),
                                instance!,
                                
                            },
                            " ",
                            typeof(string))
                    },
                    instance.Type,
                    instance.TypeMapping,
                    true,
                    new[] {true, false});
            }

            return null;
        }
    }
}
