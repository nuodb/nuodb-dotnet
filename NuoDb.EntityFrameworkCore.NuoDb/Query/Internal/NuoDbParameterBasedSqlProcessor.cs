using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace NuoDb.EntityFrameworkCore.NuoDb.Query.Internal
{
    public class NuoDbParameterBasedSqlProcessor: RelationalParameterBasedSqlProcessor
    {
        public NuoDbParameterBasedSqlProcessor(
            RelationalParameterBasedSqlProcessorDependencies dependencies,
            bool useRelationalNulls)
            : base(dependencies, useRelationalNulls)
        {
        }
        // public override SelectExpression Optimize(SelectExpression selectExpression, IReadOnlyDictionary<string, object?> parametersValues, out bool canCache)
        // {
        //     Check.NotNull(selectExpression, nameof(selectExpression));
        //     Check.NotNull(parametersValues, nameof(parametersValues));
        //
        //     selectExpression = base.Optimize(selectExpression, parametersValues, out canCache);
        //
        //     selectExpression = (SelectExpression)new NuoDbHavingExpressionVisitor(_sqlExpressionFactory).Visit(selectExpression);
        //
        //     // Run the compatibility checks as late in the query pipeline (before the actual SQL translation happens) as reasonable.
        //     //selectExpression = (SelectExpression)new MySqlCompatibilityExpressionVisitor(_options).Visit(selectExpression);
        //
        //     return selectExpression;
        // }

        /// <inheritdoc />
        protected override SelectExpression ProcessSqlNullability(
            SelectExpression selectExpression, IReadOnlyDictionary<string, object?> parametersValues, out bool canCache)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            selectExpression = new NuoDbSqlNullabilityProcessor(Dependencies, UseRelationalNulls).Process(selectExpression, parametersValues, out canCache);

            return selectExpression;
        }
    }
}
