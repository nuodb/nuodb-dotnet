using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public static class NuoDbDbContextOptionsBuilderExtensions
    {
        public static NuoDbDbContextOptionsBuilder ApplyConfiguration(this NuoDbDbContextOptionsBuilder optionsBuilder)
        {
            var maxBatch = TestEnvironment.GetInt(nameof(NuoDbDbContextOptionsBuilder.MaxBatchSize));
            if (maxBatch.HasValue)
            {
                optionsBuilder.MaxBatchSize(maxBatch.Value);
            }

            optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);

            optionsBuilder.ExecutionStrategy(d => new TestNuoDbRetryingExecutionStrategy(d));

            optionsBuilder.CommandTimeout(NuoDbTestStore.CommandTimeout);

            return optionsBuilder;
        }
    }
}
