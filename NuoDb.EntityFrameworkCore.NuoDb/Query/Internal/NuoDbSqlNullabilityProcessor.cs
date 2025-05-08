using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    /// <inheritdoc />
    public class NuoDbSqlNullabilityProcessor: SqlNullabilityProcessor
    {
        public NuoDbSqlNullabilityProcessor([NotNull] RelationalParameterBasedSqlProcessorDependencies dependencies,
            bool useRelationalNulls)
                : base(dependencies, useRelationalNulls)
        {
        }
        
        /// <inheritdoc />
        protected override SqlExpression VisitCustomSqlExpression(
            SqlExpression sqlExpression, bool allowOptimizedExpansion, out bool nullable)
        => sqlExpression switch
        {
                NuoDbComplexFunctionArgumentExpression complexFunctionArgumentExpression => VisitComplexFunctionArgument(complexFunctionArgumentExpression, allowOptimizedExpansion, out nullable),
                _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
            };

        protected virtual SqlExpression VisitComplexFunctionArgument(
            [NotNull] NuoDbComplexFunctionArgumentExpression complexFunctionArgumentExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(complexFunctionArgumentExpression, nameof(complexFunctionArgumentExpression));

            nullable = false;

            var argumentParts = new SqlExpression[complexFunctionArgumentExpression.ArgumentParts.Count];

            for (var i = 0; i < argumentParts.Length; i++)
            {
                argumentParts[i] = Visit(complexFunctionArgumentExpression.ArgumentParts[i], allowOptimizedExpansion, out var argumentPartNullable);
                nullable |= argumentPartNullable;
            }

            return complexFunctionArgumentExpression.Update(argumentParts, complexFunctionArgumentExpression.Delimiter);
        }
    }
}
